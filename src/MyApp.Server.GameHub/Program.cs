using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

using Server.Helpers;
using Server.Hubs.GamingHub;

namespace Server
{
    public class Program
    {
        private static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            // Configure logging to reduce MagicOnion noise while keeping application logs
            builder.Logging.AddFilter("MagicOnion", LogLevel.Warning);
            builder.Logging.AddFilter("Grpc", LogLevel.Warning);
            builder.Logging.AddFilter("Microsoft.AspNetCore", LogLevel.Warning);
            builder.Services.AddLogging(logging => logging.SetMinimumLevel(LogLevel.Information));

            builder.Services.ConfigureMagicOnion();
            
            builder.ConfigureSecureKestrel<Program>(new KestrelSecureOptions
            {
                HttpsPort = 12346
            });
            builder.Services.AddMongoDb();

            builder.Services.AddScoped<GameHub>();

            var app = builder.Build();
            app.MapMagicOnionService();
            app.Run();
        }
    }
}