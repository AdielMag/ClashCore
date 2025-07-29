using Common.Mongo.Collection;

using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

using MongoDB.Driver;

using Server.Mongo.Collection;

namespace Server.Helpers
{
    public static class MongoDBHelper
    {
        public static IServiceCollection AddMongoDb(this IServiceCollection services)
        {
            var connectionUri = Environment.GetEnvironmentVariable("MONGO_DB_CONNECTION_STRING");
            if (string.IsNullOrEmpty(connectionUri))
            {
                throw new InvalidOperationException("MongoDB connection string is not set in environment variables.");
            }
            
            services.AddSingleton<IMongoClient>(sp =>
            {
                var settings = MongoClientSettings.FromConnectionString(connectionUri);
                settings.ServerApi = new ServerApi(ServerApiVersion.V1);
                return new MongoClient(settings);
            });

            services.AddSingleton<IMongoDatabase>(sp =>
            {
                var client = sp.GetRequiredService<IMongoClient>();
                return client.GetDatabase("MagicOnion-Exmaple");
            });

            services.AddSingleton<IPlayersCollection>(sp =>
            {
                var database = sp.GetRequiredService<IMongoDatabase>();
                var logger = sp.GetRequiredService<ILogger<PlayersCollection>>();
                return new PlayersCollection(database, logger);
            });

            services.AddSingleton<IMatchCollection>(sp =>
            {
                var database = sp.GetRequiredService<IMongoDatabase>();
                var logger = sp.GetRequiredService<ILogger<MatchCollection>>();
                return new MatchCollection(database, logger);
            });
            
            return services;
        }
    }
}