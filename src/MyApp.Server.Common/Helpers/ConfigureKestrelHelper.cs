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
                // Always listen on the PORT environment variable in production
                if (builder.Environment.IsProduction() || builder.Environment.IsStaging())
                {
                    var port = GetPort(options);

                    serverOptions.ListenAnyIP(port, listenOptions =>
                    {
                        listenOptions.Protocols = HttpProtocols.Http1AndHttp2;
                    });
                }
                else
                {
                    serverOptions.ListenAnyIP(options.HttpsPort, listenOptions =>
                    {
                        listenOptions.Protocols = HttpProtocols.Http2;

                        var cert = new X509Certificate2(
                            options.CertificatePath,
                            options.CertificatePassword,
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

            if (! string.IsNullOrEmpty(envPort) &&
                int.TryParse(envPort, out var port))
            {
                return port;
            }
            
            if (options is
            {
                HttpsPort: > 0
            })
            {
                return options.HttpsPort;
            }
            
            // Default port if not specified
            return 443; // Default HTTPS port
        }
    }

    public class KestrelSecureOptions
    {
        public string CertificatePath { get; set; } = "/https/aspnetapp.pfx";
        public string CertificatePassword { get; set; } = "a1234567";
        public int HttpsPort { get; set; } = 443;
    }
}
