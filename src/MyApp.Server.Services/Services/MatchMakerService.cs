using Grpc.Core;

using MagicOnion;
using MagicOnion.Server;

using Microsoft.Extensions.Logging;

using MongoDB.Driver;

using Server.Mongo.Collection;
using Server.Mongo.Entity;

using Shared.Data;
using Shared.Services;

using MatchType = Shared.Data.MatchType;

namespace Server.Services
{
    public class MatchMakerService : ServiceBase<IMatchMakerService>, IMatchMakerService
    {
        private readonly IMongoClient _mongoClient;
        private readonly IMatchCollection _matches;
        private readonly ILogger<MatchMakerService> _logger;

        public MatchMakerService(IMongoClient mongoClient,
                                 ILogger<MatchMakerService> logger,
                                 IMatchCollection matches)
        {
            _mongoClient = mongoClient;
            _logger = logger;
            _matches = matches;
        }

        public UnaryResult<string> Test()
        {
            return new UnaryResult<string>("Hello from MatchMakerService!");
        }

        public async UnaryResult<MatchConnectionData> JoinMatchAsync(
            string playerId,
            MatchType matchType,
            string matchId)
        {
            _logger.LogInformation(
                $"Player {playerId} is trying to join match of type {matchType} with matchId {matchId ?? "none"}");
            
            var maxPlayers = GetMaxPlayers(matchType);

            using var session = await _mongoClient.StartSessionAsync();
            session.StartTransaction();

            try
            {
                Match ? match = null;
                var hasMatchId = ! string.IsNullOrWhiteSpace(matchId);
                if (hasMatchId)
                {
                    match = await _matches.GetMatchByIdAsync(matchId);

                    if (match == null)
                    {
                        throw new RpcException(new Status(StatusCode.NotFound, "Match not found"));
                    }

                    if (match.PlayerCount >= maxPlayers)
                    {
                        throw new RpcException(new Status(StatusCode.ResourceExhausted, "Match is full"));
                    }

                    if (! match.Players.Contains(playerId))
                    {
                        match.Players.Add(playerId);
                        match.PlayerCount++;
                    }
                }
                else
                {
                    // ② try to reuse any open match
                    match = await _matches.TryJoinOpenMatchAsync(matchType, maxPlayers, playerId, session);
                }

                // ③ if still null, create a brand-new match
                if (match == null)
                {
                    match = await CreateMatchAsync(matchType, playerId, session);
                }

                await session.CommitTransactionAsync();

                return new MatchConnectionData
                {
                    MatchId = match.Id, Url = match.Url, Port = match.Port
                };
            }
            catch (RpcException)
            {
                throw;
            } // preserve gRPC status codes
            catch (Exception ex)
            {
                await session.AbortTransactionAsync();
                _logger.LogError(ex, "Error joining match");
                throw new RpcException(new Status(StatusCode.Internal, "Failed to join match"));
            }
        }
        
        private int GetMaxPlayers(MatchType type)
        {
            return type switch
            {
                MatchType.Default => 10, _ => 10
            };
        }

        private async Task<Match> CreateMatchAsync(MatchType type,
                                                   string playerId,
                                                   IClientSessionHandle session)
        {
            var (url, port) = AllocateServerEndpoint(type);

            var match = await _matches.CreateMatchAsync(new()
            {
                playerId
            }, type, url, port);

            return match;
        }

        private static (string url, int port) AllocateServerEndpoint(MatchType type)
        {
            // quick placeholder – eg. choose random port on localhost
            // integrate with real pool later.
            return ("localhost", 12346);
        }
    }
}