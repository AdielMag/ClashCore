using Microsoft.Extensions.Logging;

using MongoDB.Bson;
using MongoDB.Driver;

using Server.Common.Exceptions;
using Server.Mongo.Collection;
using Server.Mongo.Entity;

using MatchType = Shared.Data.MatchType;

namespace Common.Mongo.Collection
{
    public class MatchCollection : IMatchCollection
    {
        private readonly IMongoCollection<Match> _collection;
        private readonly ILogger<MatchCollection> _logger;

        public MatchCollection(IMongoDatabase database, ILogger<MatchCollection> logger)
        {
            _collection = database.GetCollection<Match>("matches") 
                ?? throw new ArgumentNullException(nameof(database));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            CreateIndexes();
        }

        private void CreateIndexes()
        {
            try
            {
                var indexOptions = new CreateIndexOptions { Background = true };

                var createdAtIndex = new CreateIndexModel<Match>(
                    Builders<Match>.IndexKeys.Ascending(m => m.CreatedAt),
                    indexOptions
                );

                var typeAndPlayerCountIndex = new CreateIndexModel<Match>(
                    Builders<Match>.IndexKeys
                                   .Ascending(m => m.Type)
                                   .Ascending(m => m.PlayerCount),
                    indexOptions
                );

                _collection.Indexes.CreateMany(new[] { createdAtIndex, typeAndPlayerCountIndex });

                _logger.LogInformation("Created indexes for Matches collection");
            }
            catch (MongoCommandException ex) when (ex.CodeName == "IndexOptionsConflict")
            {
                _logger.LogInformation("Indexes already exist for Matches collection");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to create indexes for Matches collection");
                throw new DatabaseInitializationException("Failed to create indexes", ex);
            }
        }

        public async Task<Match> CreateMatchAsync(List<string> players, MatchType matchType, string url, int port)
        {
            if (players == null || !players.Any())
            {
                throw new ArgumentException("Players list cannot be null or empty", nameof(players));
            }

            var match = new Match
            {
                Id = ObjectId.GenerateNewId().ToString(),
                Players = players,
                PlayerCount = players.Count,
                CreatedAt = DateTime.UtcNow,
                Type = matchType,
                Url = url,
                Port = port
            };

            try
            {
                await _collection.InsertOneAsync(match);

                _logger.LogInformation("Created new match - Id: {Id}, Players: {Players}", 
                    match.Id, string.Join(", ", players));

                return match;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to create match - Players: {Players}",
                    string.Join(", ", players));
                throw new DatabaseOperationException("Failed to create match", ex);
            }
        }

        public async Task<Match> GetMatchByIdAsync(string matchId)
        {
            if (string.IsNullOrEmpty(matchId))
            {
                throw new ArgumentException("Match ID cannot be null or empty", nameof(matchId));
            }

            try
            {
                var match = await _collection.Find(m => m.Id == matchId).FirstOrDefaultAsync();

                if (match == null)
                {
                    _logger.LogWarning("Match not found with Id: {MatchId}", matchId);
                    throw new MatchNotFoundException(matchId);
                }

                return match;
            }
            catch (Exception ex) when (!(ex is MatchNotFoundException))
            {
                _logger.LogError(ex, "Error retrieving match with Id: {MatchId}", matchId);
                throw new DatabaseOperationException($"Failed to retrieve match {matchId}", ex);
            }
        }

        public async Task<List<Match>> GetMatchesByPlayerAsync(string playerId)
        {
            if (string.IsNullOrEmpty(playerId))
            {
                throw new ArgumentException("Player ID cannot be null or empty", nameof(playerId));
            }

            try
            {
                var matches = await _collection.Find(m => m.Players.Contains(playerId))
                                            .ToListAsync();

                _logger.LogInformation("Retrieved {Count} matches for player: {PlayerId}", 
                    matches.Count, playerId);

                return matches;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving matches for player: {PlayerId}", playerId);
                throw new DatabaseOperationException($"Failed to retrieve matches for player {playerId}", ex);
            }
        }

        public async Task<Match?> TryJoinOpenMatchAsync(
            MatchType matchType, int maxPlayers, string playerId, IClientSessionHandle session)
        {
            var filter = Builders<Match>.Filter.And(
                Builders<Match>.Filter.Eq(m => m.Type, matchType),
                Builders<Match>.Filter.Lt(m => m.PlayerCount, maxPlayers));

            var update = Builders<Match>.Update
                                        .AddToSet(m => m.Players, playerId); // won't add duplicates

            var result = await _collection.FindOneAndUpdateAsync(
                session,
                filter,
                update,
                new FindOneAndUpdateOptions<Match>
                {
                    ReturnDocument = ReturnDocument.After
                });

            // If player was added, do second update to increment count
            if (result != null && !result.Players.Contains(playerId))
            {
                var inc = Builders<Match>.Update.Inc(m => m.PlayerCount, 1);
                await _collection.UpdateOneAsync(session,
                                                 Builders<Match>.Filter.Eq(m => m.Id, result.Id),
                                                 inc);
        
                result.PlayerCount += 1;      // reflect update locally
                result.Players.Add(playerId); // reflect update locally
            }

            return result;
        }

        
        public async Task DeleteMatchAsync(string matchId)
        {
            if (string.IsNullOrEmpty(matchId))
            {
                throw new ArgumentException("Match ID cannot be null or empty", nameof(matchId));
            }

            try
            {
                var result = await _collection.DeleteOneAsync(m => m.Id == matchId);

                if (result.DeletedCount == 0)
                {
                    _logger.LogWarning("No match found to delete with Id: {MatchId}", matchId);
                    throw new MatchNotFoundException(matchId);
                }

                _logger.LogInformation("Deleted match with Id: {MatchId}", matchId);
            }
            catch (Exception ex) when (ex is not MatchNotFoundException)
            {
                _logger.LogError(ex, "Error deleting match with Id: {MatchId}", matchId);
                throw new DatabaseOperationException($"Failed to delete match {matchId}", ex);
            }
        }
    }
} 