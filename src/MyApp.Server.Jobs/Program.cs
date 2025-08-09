using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using MongoDB.Driver;
using Server.Mongo.Entity;
using Common.Mongo.Collection;
using Server.Mongo.Collection;

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
                var database = client.GetDatabase("MagicOnion-Exmaple");

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
                logger.LogInformation("Starting instance and match invalidation job...");

                var matchInstanceCollection = new MatchInstanceCollection(database, LoggerFactory.Create(builder => builder.AddConsole()).CreateLogger<MatchInstanceCollection>());
                var matchCollection = new MatchCollection(database, LoggerFactory.Create(builder => builder.AddConsole()).CreateLogger<MatchCollection>());

                // First, get all valid instances to know which matches to invalidate
                var allInstances = await database.GetCollection<MatchInstance>("matchInstances")
                    .Find(Builders<MatchInstance>.Filter.Eq(i => i.IsValid, true))
                    .ToListAsync();

                logger.LogInformation("Found {InstanceCount} valid instances", allInstances.Count);

                if (allInstances.Count == 0)
                {
                    logger.LogInformation("No valid instances found to invalidate");
                    return 0;
                }

                long totalInvalidatedMatches = 0;

                // For each instance, invalidate all matches running on it
                foreach (var instance in allInstances)
                {
                    logger.LogInformation("Invalidating matches for instance {InstanceId} at {Url}:{Port}", 
                        instance.Id, instance.Url, instance.Port);

                    var invalidatedMatches = await matchCollection.InvalidateMatchesByInstanceAsync(instance.Url, instance.Port);
                    totalInvalidatedMatches += invalidatedMatches;
                }

                // Then invalidate all instances
                logger.LogInformation("Invalidating all instances...");
                var invalidatedInstances = await matchInstanceCollection.InvalidateAllInstancesAsync();

                logger.LogInformation("Invalidation completed. Invalidated {InstanceCount} instances and {MatchCount} matches", 
                    invalidatedInstances, totalInvalidatedMatches);
                
                return 0;
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Failed to invalidate instances and matches");
                return 1;
            }
        }
    }
}