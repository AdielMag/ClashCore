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

                // Get job type from command line arguments, default to invalidate-matches
                var jobType = args.Length > 0 ? args[0] : "invalidate-matches";
                
                logger.LogInformation("Starting job: {JobType}", jobType);

                switch (jobType.ToLowerInvariant())
                {
                    case "invalidate-matches":
                        return await InvalidateMatchesJob(database, logger);
                    
                    default:
                        logger.LogError("Unknown job type: {JobType}. Available jobs: invalidate-matches", jobType);
                        return 1;
                }
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Job execution failed");
                return 1;
            }
        }

        private static async Task<int> InvalidateMatchesJob(IMongoDatabase database, ILogger logger)
        {
            try
            {
                var matchesCollection = database.GetCollection<Match>("matches");

                logger.LogInformation("Starting match invalidation job...");

                // First, count total matches and valid matches for debugging
                var totalMatches = await matchesCollection.CountDocumentsAsync(Builders<Match>.Filter.Empty);
                var validMatchesFilter = Builders<Match>.Filter.Eq(m => m.IsValid, true);
                var validMatchesCount = await matchesCollection.CountDocumentsAsync(validMatchesFilter);

                logger.LogInformation("Found {TotalMatches} total matches, {ValidMatches} are currently valid", 
                    totalMatches, validMatchesCount);

                if (validMatchesCount == 0)
                {
                    logger.LogInformation("No valid matches found to invalidate");
                    return 0;
                }

                // Invalidate all valid matches
                var update = Builders<Match>.Update.Set(m => m.IsValid, false);
                var result = await matchesCollection.UpdateManyAsync(validMatchesFilter, update);

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