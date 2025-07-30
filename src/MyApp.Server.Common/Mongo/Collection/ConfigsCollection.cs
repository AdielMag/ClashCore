using System;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using MongoDB.Driver;
using Server.Common.Exceptions;
using Server.Mongo.Entity;

namespace Server.Mongo.Collection
{
    public class ConfigsCollection : IConfigsCollection
    {
        private readonly IMongoCollection<Config> _collection;
        private readonly ILogger<ConfigsCollection> _logger;

        public ConfigsCollection(IMongoDatabase database, ILogger<ConfigsCollection> logger)
        {
            _collection = database.GetCollection<Config>("configs") ?? throw new ArgumentNullException(nameof(database));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            CreateIndexes();
        }

        private void CreateIndexes()
        {
            try
            {
                var indexOptions = new CreateIndexOptions { Background = true, Unique = true };
                var keyIndex = new CreateIndexModel<Config>(Builders<Config>.IndexKeys.Ascending(c => c.Key), indexOptions);
                _collection.Indexes.CreateOne(keyIndex);
                _logger.LogInformation("Created indexes for Configs collection");
            }
            catch (MongoCommandException ex) when (ex.CodeName == "IndexOptionsConflict")
            {
                _logger.LogInformation("Indexes already exist for Configs collection");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to create indexes for Configs collection");
                throw new DatabaseInitializationException("Failed to create indexes", ex);
            }
        }

        public async Task<Config?> GetConfigAsync(string key)
        {
            if (string.IsNullOrEmpty(key))
                throw new ArgumentException("Key cannot be null or empty", nameof(key));

            try
            {
                return await _collection.Find(c => c.Key == key).FirstOrDefaultAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to retrieve config {Key}", key);
                throw new DatabaseOperationException("Failed to retrieve config", ex);
            }
        }

        public async Task SetConfigAsync(string key, string value)
        {
            if (string.IsNullOrEmpty(key))
                throw new ArgumentException("Key cannot be null or empty", nameof(key));

            var update = Builders<Config>.Update.Set(c => c.Value, value);
            var options = new UpdateOptions { IsUpsert = true };

            try
            {
                await _collection.UpdateOneAsync(c => c.Key == key, update, options);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to set config {Key}", key);
                throw new DatabaseOperationException("Failed to set config", ex);
            }
        }
    }
}
