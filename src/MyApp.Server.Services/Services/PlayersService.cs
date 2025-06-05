using Grpc.Core;
using MagicOnion;
using MagicOnion.Server;
using Microsoft.Extensions.Logging;
using MongoDB.Driver;

using Server.Common.Exceptions;
using Server.Mongo.Collection;
using Server.Mongo.Entity;

using Shared.Services;

namespace Server.Services
{
    public class PlayersService : ServiceBase<IPlayersService>, IPlayersService
    {
        private readonly IMongoClient _mongoClient;
        private readonly ILogger<PlayersService> _logger;
        private readonly IPlayersCollection _players;

        public PlayersService(
            IMongoClient mongoClient,
            ILogger<PlayersService> logger,
            IPlayersCollection players)
        {
            _mongoClient = mongoClient;
            _logger = logger;
            _players = players;
        }

        public async UnaryResult<string> RegisterAsync()
        {
            try
            {
                var userId = MongoDB.Bson.ObjectId.GenerateNewId().ToString();
                var username = $"Player_{DateTime.UtcNow.Ticks}_{Random.Shared.Next(1000, 9999)}";

                using var session = await _mongoClient.StartSessionAsync();
                
                try
                {
                    session.StartTransaction();
                    
                    Player existingPlayer;
                    try
                    {
                        existingPlayer = await _players.GetPlayerByUserIdAsync(userId);
                    }
                    catch (Exception e) when(e is  PlayerNotFoundException)
                    {
                        existingPlayer = null;
                    }
                    catch (Exception ex)
                    {
                        throw new RpcException(new Status(StatusCode.Internal, $"Failed to check existing player: {ex.Message}"));
                    }
                    
                    if (existingPlayer != null)
                    {
                        throw new RpcException(new Status(StatusCode.AlreadyExists, "Player already exists"));
                    }

                    var player = await _players.CreatePlayerAsync(userId, username);
                    await session.CommitTransactionAsync();

                    _logger.LogInformation(
                        "New player registered - UserId: {UserId}, Username: {Username}",
                        player.UserId,
                        player.Username
                    );

                    return player.UserId;
                }
                catch (Exception)
                {
                    await session.AbortTransactionAsync();
                    throw;
                }
            }
            catch (MongoException ex)
            {
                _logger.LogError(ex, "MongoDB error during player registration");
                throw new RpcException(new Status(StatusCode.Internal, $"Failed to register player: {ex.Message}"));
            }
            catch (RpcException)
            {
                throw;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unexpected error during player registration");
                throw new RpcException(new Status(StatusCode.Internal, $"Unexpected error during registration: {ex.Message}"));
            }
        }

        public async UnaryResult LoginAsync(string userId)
        {
            try
            {
                using var session = await _mongoClient.StartSessionAsync();
                
                try
                {
                    session.StartTransaction();

                    var player = await _players.GetPlayerByUserIdAsync(userId);
                    
                    if (player == null)
                    {
                        _logger.LogWarning("Login attempt with invalid userId: {UserId}", userId);
                        throw new RpcException(new Status(StatusCode.NotFound, "Player not found"));
                    }

                    await _players.UpdateLastLoginAsync(userId);
                    await session.CommitTransactionAsync();

                    _logger.LogInformation(
                        "Player logged in successfully - UserId: {UserId}, Username: {Username}",
                        player.UserId,
                        player.Username
                    );
                    
                }
                catch (Exception)
                {
                    await session.AbortTransactionAsync();
                    throw;
                }
            }
            catch (MongoException ex)
            {
                _logger.LogError(ex, "MongoDB error during player login - UserId: {UserId}", userId);
                throw new RpcException(new Status(StatusCode.Internal, $"Failed to process login: {ex.Message}"));
            }
            catch (RpcException)
            {
                throw;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unexpected error during player login - UserId: {UserId}", userId);
                throw new RpcException(new Status(StatusCode.Internal, $"Unexpected error during login: {ex.Message}"));
            }
        }
    }
}