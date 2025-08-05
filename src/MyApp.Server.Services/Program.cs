using Server.Helpers;
using Server.Services;

namespace Server
{
    public class Program
    {
        private static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            // Configure Kestrel - don't hardcode port for Cloud Run
            builder.ConfigureSecureKestrel<Program>(new KestrelSecureOptions
            {
                HttpsPort = 5002
            });

            // Add MongoDB
            builder.Services.AddMongoDb();

            // Configure logging to reduce MagicOnion noise
            builder.Logging.AddFilter("MagicOnion", LogLevel.Warning);
            builder.Logging.AddFilter("Grpc", LogLevel.Warning);

            // Configure MagicOnion with proper gRPC settings
            builder.Services.ConfigureMagicOnion();

            // Add your services
            builder.Services.AddScoped<PlayersService>();
            builder.Services.AddScoped<MatchMakerService>();
            builder.Services.AddSingleton<MatchInstanceService>();

            var app = builder.Build();

            // Add health check endpoints for Cloud Run
            app.MapGet("/", () => "gRPC Server is running");
            app.MapGet("/health", () => "OK");
            
            // Add HTTP endpoint for match invalidation (for CI/CD)
            app.MapPost("/invalidate-matches", async (MatchMakerService matchMakerService) =>
            {
                
                try
                {
                    var result = await matchMakerService.InvalidateAllMatchesAsync();
                    return Results.Ok(new { invalidatedCount = result, message = "Matches invalidated successfully" });
                }
                catch (Exception ex)
                {
                    return Results.Problem($"Failed to invalidate matches: {ex.Message}");
                }
            });

            // Important: Enable routing before mapping services
            app.UseRouting();

            // Map MagicOnion services
            app.MapMagicOnionService();

            app.Run();
        }
    }
}
