using Microsoft.Extensions.Logging;
using MongoDB.Driver;
using Server.Mongo.Entity;
using Server.Common.Exceptions;

namespace Server.Mongo.Collection
{
    public class PlayersCollection : IPlayersCollection
    {
        private readonly IMongoCollection<Player> _collection;
        private readonly ILogger<PlayersCollection> _logger;

        public PlayersCollection(IMongoDatabase database, ILogger<PlayersCollection> logger)
        {
            _collection = database.GetCollection<Player>("players") 
                ?? throw new ArgumentNullException(nameof(database));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            CreateIndexes();
        }

        private void CreateIndexes()
        {
            try
            {
                var indexOptions = new CreateIndexOptions { Background = true, Unique = true };
                
                var userIdIndex = new CreateIndexModel<Player>(
                    Builders<Player>.IndexKeys.Ascending(p => p.UserId),
                    indexOptions
                );
                _collection.Indexes.CreateOne(userIdIndex);

                var usernameIndex = new CreateIndexModel<Player>(
                    Builders<Player>.IndexKeys.Ascending(p => p.Username),
                    indexOptions
                );
                _collection.Indexes.CreateOne(usernameIndex);

                _logger.LogInformation("Created indexes for Players collection");
            }
            catch (MongoCommandException ex) when (ex.CodeName == "IndexOptionsConflict")
            {
                _logger.LogInformation("Indexes already exist for Players collection");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to create indexes for Players collection");
                throw new DatabaseInitializationException("Failed to create indexes", ex);
            }
        }

        public async Task<Player> CreatePlayerAsync(string userId, string username)
        {
            if (string.IsNullOrEmpty(userId))
                throw new ArgumentException("UserId cannot be null or empty", nameof(userId));
            if (string.IsNullOrEmpty(username))
                throw new ArgumentException("Username cannot be null or empty", nameof(username));

            var player = new Player
            {
                UserId = userId,
                Username = username,
                LastLoginAt = DateTime.UtcNow,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

            try
            {
                await _collection.InsertOneAsync(player);
                _logger.LogInformation("Created new player - UserId: {UserId}, Username: {Username}", 
                    userId, username);
                return player;
            }
            catch (MongoWriteException ex) when (ex.WriteError.Category == ServerErrorCategory.DuplicateKey)
            {
                _logger.LogWarning("Attempted to create duplicate player - UserId: {UserId}, Username: {Username}", 
                    userId, username);
                throw new DuplicatePlayerException("Player with this userId or username already exists", ex);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to create player - UserId: {UserId}, Username: {Username}",
                    userId, username);
                throw new DatabaseOperationException("Failed to create player", ex);
            }
        }

        public async Task<Player> GetPlayerByUserIdAsync(string userId)
        {
            if (string.IsNullOrEmpty(userId))
                throw new ArgumentException("UserId cannot be null or empty", nameof(userId));

            try
            {
                var player = await _collection.Find(p => p.UserId == userId).FirstOrDefaultAsync();

                if (player == null)
                {
                    _logger.LogWarning("Player not found with UserId: {UserId}", userId);
                    throw new PlayerNotFoundException(userId);
                }

                return player;
            }
            catch (Exception ex) when (!(ex is PlayerNotFoundException))
            {
                _logger.LogError(ex, "Error retrieving player with UserId: {UserId}", userId);
                throw new DatabaseOperationException($"Failed to retrieve player {userId}", ex);
            }
        }

        public async Task UpdateLastLoginAsync(string userId)
        {
            if (string.IsNullOrEmpty(userId))
                throw new ArgumentException("UserId cannot be null or empty", nameof(userId));

            try
            {
                var update = Builders<Player>.Update
                    .Set(p => p.LastLoginAt, DateTime.UtcNow)
                    .Set(p => p.UpdatedAt, DateTime.UtcNow);

                var result = await _collection.UpdateOneAsync(p => p.UserId == userId, update);

                if (result.ModifiedCount == 0)
                {
                    _logger.LogWarning("No player updated for UserId: {UserId}", userId);
                    throw new PlayerNotFoundException(userId);
                }

                _logger.LogInformation("Updated last login for player with UserId: {UserId}", userId);
            }
            catch (Exception ex) when (!(ex is PlayerNotFoundException))
            {
                _logger.LogError(ex, "Error updating last login for player with UserId: {UserId}", userId);
                throw new DatabaseOperationException($"Failed to update player {userId}", ex);
            }
        }
    }
}
