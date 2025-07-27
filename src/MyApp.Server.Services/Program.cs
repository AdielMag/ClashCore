using Server.Helpers;
using Server.Services;

namespace Server
{
    public class Program
    {
        private static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);
            var port = Environment.GetEnvironmentVariable("PORT") ?? "5002";

            builder.ConfigureSecureKestrel<Program>(new KestrelSecureOptions
            {
                HttpsPort = int.Parse(port)
            });
            
            builder.Services.AddMongoDb();

            builder.Services.ConfigureMagicOnion();
            builder.Services.AddScoped<PlayersService>();
            builder.Services.AddScoped<MatchMakerService>();
            
            var app = builder.Build();
            app.MapMagicOnionService();
            app.Run();
        }
    }
}