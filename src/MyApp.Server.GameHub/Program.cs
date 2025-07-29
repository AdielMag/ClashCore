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

            builder.Services.ConfigureMagicOnion();

            builder.ConfigureSecureKestrel<Program>(new KestrelSecureOptions
            {
                HttpsPort = 12346
            });
            builder.Services.AddMongoDb();

            builder.Services.AddScoped<GameHub>();

            builder.Services.AddLogging(logging => logging.SetMinimumLevel(LogLevel.Debug));

            var app = builder.Build();
            app.MapMagicOnionService();
            app.Run();
        }
    }
}