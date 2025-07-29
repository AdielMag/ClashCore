using Grpc.Core;
using Grpc.Core.Interceptors;

using MessagePack;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

using Shared.Helpersl;

namespace Server.Helpers
{
    public static class MagicOnionHelper
    {
        public static IServiceCollection ConfigureMagicOnion(this IServiceCollection services)
        {
            // Add gRPC with Cloud Run optimized settings
            services.AddGrpc(options =>
            {
                options.EnableDetailedErrors = true;
                options.MaxReceiveMessageSize = 4 * 1024 * 1024; // 4MB
                options.MaxSendMessageSize = 4 * 1024 * 1024; // 4MB
                
                // Critical for Cloud Run - handle timeouts gracefully
                options.Interceptors.Add<ConnectionLoggingInterceptor>();
            });

            var serializerOptions =
                MessagePackSerializerOptions.Standard.WithResolver(MessagePackResolverConfig.Resolver);

            services.AddMagicOnion(options =>
            {
                options.MessageSerializer = new CustomMagicOnionSerializerProvider(serializerOptions);
                options.IsReturnExceptionStackTraceInErrorDetail = true;
                options.EnableCurrentContext = true;
            });

            // Add the logging interceptor
            services.AddSingleton<ConnectionLoggingInterceptor>();

            return services;
        }
    }
    
    // Add this interceptor to debug connection issues
    public class ConnectionLoggingInterceptor : Interceptor
    {
        private readonly ILogger<ConnectionLoggingInterceptor> _logger;

        public ConnectionLoggingInterceptor(ILogger<ConnectionLoggingInterceptor> logger)
        {
            _logger = logger;
        }

        public override async Task<TResponse> UnaryServerHandler<TRequest, TResponse>(
            TRequest request, 
            ServerCallContext context, 
            UnaryServerMethod<TRequest, TResponse> continuation)
        {
            var method = context.Method;
            var peer = context.Peer;
            
            _logger.LogInformation($"[gRPC] Incoming request: {method} from {peer}");
            
            try
            {
                var response = await continuation(request, context);
                _logger.LogInformation($"[gRPC] Request completed: {method}");
                return response;
            }
            catch (RpcException rpcEx)
            {
                _logger.LogError($"[gRPC] RPC Error in {method}: {rpcEx.StatusCode}");
                throw;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"[gRPC] Unexpected error in {method}");
                throw;
            }
        }
    }
}