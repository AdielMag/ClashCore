using System;
using System.Security.Cryptography.X509Certificates;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Server.Kestrel.Core;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace Server.Helpers
{
    public static class KestrelHelpers
    {
        public static WebApplicationBuilder ConfigureSecureKestrel<T>(this WebApplicationBuilder builder,
                                                                      KestrelSecureOptions options = null)
        {
            var logger = builder.Services.BuildServiceProvider().GetRequiredService<ILogger<T>>();

            builder.WebHost.ConfigureKestrel(serverOptions =>
            {
                serverOptions.ListenAnyIP(options.HttpsPort, listenOptions =>
                {
                    listenOptions.Protocols = HttpProtocols.Http2;

                    if (builder.Environment.IsProduction() || builder.Environment.IsStaging())
                    {
                        return ;
                    }
                    
                    try
                    {
                        var cert = new X509Certificate2(
                            options.CertificatePath,
                            options.CertificatePassword,
                            X509KeyStorageFlags.MachineKeySet |
                            X509KeyStorageFlags.PersistKeySet |
                            X509KeyStorageFlags.Exportable);

                        listenOptions.UseHttps(cert);
                        logger.LogInformation("Listening securely on HTTPS port {Port}", options.HttpsPort);
                    }
                    catch (Exception ex)
                    {
                        logger.LogError(ex, "Failed to load certificate.");
                        throw;
                    }
                });
            });

            return builder;
        }
    }

    public class KestrelSecureOptions
    {
        public string CertificatePath { get; set; } = "/https/aspnetapp.pfx";
        public string CertificatePassword { get; set; } = "a1234567";
        public int HttpsPort { get; set; } = 443;
    }
}
