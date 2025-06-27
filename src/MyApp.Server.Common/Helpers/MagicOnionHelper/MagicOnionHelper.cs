using MessagePack;
using Microsoft.Extensions.DependencyInjection;

using Shared.Helpersl;

namespace Server.Helpers
{
    public static class MagicOnionHelper
    {
        public static IServiceCollection ConfigureMagicOnion(this IServiceCollection services)
        {
            services.AddGrpc();

            var serializerOptions =
                MessagePackSerializerOptions.Standard.WithResolver(MessagePackResolverConfig.Resolver);

            services.AddMagicOnion(options =>
            {
                options.MessageSerializer = new CustomMagicOnionSerializerProvider(serializerOptions);
                options.IsReturnExceptionStackTraceInErrorDetail = true;
                options.EnableCurrentContext = true;
            });

            return services;
        }
    }
}