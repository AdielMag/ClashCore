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
            builder.WebHost.ConfigureKestrel(serverOptions =>
            {
                // Cloud Run configuration
                if (builder.Environment.IsProduction() || builder.Environment.IsStaging())
                {
                    var port = GetPort(options);

                    serverOptions.ListenAnyIP(port, listenOptions =>
                    {
                        // Cloud Run terminates TLS, so use HTTP/2 without TLS
                        listenOptions.Protocols = HttpProtocols.Http2;
                        
                        // Important: Don't use HTTPS - Cloud Run handles TLS termination
                        // The internal connection between Cloud Run and your container is HTTP
                    });
                }
                else
                {
                    // Local development with HTTPS
                    serverOptions.ListenAnyIP(options?.HttpsPort ?? 5001, listenOptions =>
                    {
                        listenOptions.Protocols = HttpProtocols.Http2;

                        var cert = new X509Certificate2(
                            options?.CertificatePath ?? "/https/aspnetapp.pfx",
                            options?.CertificatePassword ?? "a1234567",
                            X509KeyStorageFlags.MachineKeySet |
                            X509KeyStorageFlags.PersistKeySet |
                            X509KeyStorageFlags.Exportable);

                        listenOptions.UseHttps(cert);
                    });
                }
            });

            return builder;
        }

        private static int GetPort(KestrelSecureOptions options)
        {
            var envPort = Environment.GetEnvironmentVariable("PORT");

            if (!string.IsNullOrEmpty(envPort) && int.TryParse(envPort, out var port))
            {
                return port;
            }
            
            // Cloud Run typically uses port 8080
            return options?.HttpsPort ?? 8080;
        }
    }

    public class KestrelSecureOptions
    {
        public string CertificatePath { get; set; } = "/https/aspnetapp.pfx";
        public string CertificatePassword { get; set; } = "a1234567";
        public int HttpsPort { get; set; } = 5001; // Changed from 443 to 5001 for local dev
    }
}