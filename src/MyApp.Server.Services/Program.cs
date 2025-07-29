using Server.Helpers;
using Server.Services;

namespace Server
{
    public class Program
    {
        private static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            builder.ConfigureSecureKestrel<Program>(new KestrelSecureOptions
            {
                HttpsPort = 5002
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