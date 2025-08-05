using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using MongoDB.Driver;
using Server.Mongo.Entity;

namespace MyApp.Server.Jobs
{
    public class Program
    {
        public static async Task<int> Main(string[] args)
        {
            var config = new ConfigurationBuilder()
                .AddEnvironmentVariables()
                .Build();

            using var loggerFactory = LoggerFactory.Create(builder =>
                builder.AddConsole().SetMinimumLevel(LogLevel.Information));
            var logger = loggerFactory.CreateLogger<Program>();

            try
            {
                var connectionString = config["MONGO_DB_CONNECTION_STRING"];
                if (string.IsNullOrEmpty(connectionString))
                {
                    logger.LogError("MONGO_DB_CONNECTION_STRING environment variable is required");
                    return 1;
                }
                
                var client = new MongoClient(connectionString);
                var database = client.GetDatabase("solaria");
                var matchesCollection = database.GetCollection<Match>("matches");

                logger.LogInformation("Starting match invalidation job...");

                var filter = Builders<Match>.Filter.Eq(m => m.IsValid, true);
                var update = Builders<Match>.Update.Set(m => m.IsValid, false);

                var result = await matchesCollection.UpdateManyAsync(filter, update);

                logger.LogInformation("Match invalidation completed. Modified {Count} matches", result.ModifiedCount);
                
                return 0;
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Failed to invalidate matches");
                return 1;
            }
        }
    }
}