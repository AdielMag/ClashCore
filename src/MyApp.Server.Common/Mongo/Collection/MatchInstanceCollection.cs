using System;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using MongoDB.Driver;
using Server.Mongo.Entity;
using Server.Common.Exceptions;

namespace Server.Mongo.Collection
{
    public class MatchInstanceCollection : IMatchInstanceCollection
    {
        private readonly IMongoCollection<MatchInstance> _collection;
        private readonly ILogger<MatchInstanceCollection> _logger;

        public MatchInstanceCollection(IMongoDatabase database, ILogger<MatchInstanceCollection> logger)
        {
            _collection = database.GetCollection<MatchInstance>("matchInstances") ?? throw new ArgumentNullException(nameof(database));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            CreateIndexes();
        }

        private void CreateIndexes()
        {
            try
            {
                var indexOptions = new CreateIndexOptions { Background = true };
                var createdAtIndex = new CreateIndexModel<MatchInstance>(Builders<MatchInstance>.IndexKeys.Ascending(m => m.CreatedAt), indexOptions);
                _collection.Indexes.CreateOne(createdAtIndex);
                _logger.LogInformation("Created indexes for MatchInstance collection");
            }
            catch (MongoCommandException ex) when (ex.CodeName == "IndexOptionsConflict")
            {
                _logger.LogInformation("Indexes already exist for MatchInstance collection");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to create indexes for MatchInstance collection");
                throw new DatabaseInitializationException("Failed to create indexes", ex);
            }
        }

        public async Task<MatchInstance?> TryAllocateInstanceAsync(int capacity, int requiredSlots)
        {
            var filter = Builders<MatchInstance>.Filter.And(
                Builders<MatchInstance>.Filter.Lte(i => i.PlayerCount, capacity - requiredSlots),
                Builders<MatchInstance>.Filter.Eq(i => i.IsValid, true)
            );
            var update = Builders<MatchInstance>.Update.Inc(i => i.PlayerCount, requiredSlots);
            var options = new FindOneAndUpdateOptions<MatchInstance> { ReturnDocument = ReturnDocument.After };

            try
            {
                return await _collection.FindOneAndUpdateAsync(filter, update, options);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to allocate instance");
                throw new DatabaseOperationException("Failed to allocate instance", ex);
            }
        }

        public async Task<MatchInstance> CreateInstanceAsync(string url, int port)
        {
            try
            {
                var instanceData = new MatchInstance
                {
                    Url = url,
                    Port = port,
                    PlayerCount = 0,
                    CreatedAt = DateTime.UtcNow,
                    IsValid = true
                };
                await _collection.InsertOneAsync(instanceData);
                return instanceData;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to create instance document");
                throw new DatabaseOperationException("Failed to create instance document", ex);
            }
        }

        public async Task<long> InvalidateAllInstancesAsync()
        {
            try
            {
                var filter = Builders<MatchInstance>.Filter.Eq(i => i.IsValid, true);
                var update = Builders<MatchInstance>.Update.Set(i => i.IsValid, false);

                var result = await _collection.UpdateManyAsync(filter, update);
                
                _logger.LogInformation("Invalidated {Count} instances", result.ModifiedCount);
                
                return result.ModifiedCount;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error invalidating all instances");
                throw new DatabaseOperationException("Failed to invalidate all instances", ex);
            }
        }

        public async Task<List<MatchInstance>> GetInstancesByUrlAndPortAsync(string url, int port)
        {
            try
            {
                var filter = Builders<MatchInstance>.Filter.And(
                    Builders<MatchInstance>.Filter.Eq(i => i.Url, url),
                    Builders<MatchInstance>.Filter.Eq(i => i.Port, port)
                );
                
                return await _collection.Find(filter).ToListAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting instances by URL and port: {Url}:{Port}", url, port);
                throw new DatabaseOperationException($"Failed to get instances for {url}:{port}", ex);
            }
        }
    }
}
