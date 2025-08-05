using System;
using App.InternalDomains.DebugService;
using App.Scripts;
using Cysharp.Net.Http;
using Cysharp.Threading.Tasks;
using Grpc.Net.Client;
using MagicOnion;
using MagicOnion.Client;
using MagicOnion.Unity;
using Shared.Services;
using VContainer;
using VContainer.Unity;

namespace App.InternalDomains.NetworkService
{
    public class NetworkService : IInitializable, INetworkService, IDisposable
    {
        [Inject] private readonly IDebugService _debugService;
        [Inject] private readonly AppLifetimeScope _appLifetimeScope;
        
        private GrpcChannelx _servicesChannel;
        private GrpcChannelx _matchChannel;

        private LifetimeScope _networkScope;
        
        public void Initialize()
        {
            InitializeGrpc();
            CreateServicesChannel();
            
            var serviceClient = MagicOnionClient.Create<ISampleService>(_servicesChannel);
            var playersServiceClient = MagicOnionClient.Create<IPlayersService>(_servicesChannel);
            var matchMakerServiceClient = MagicOnionClient.Create<IMatchMakerService>(_servicesChannel);
            
            _networkScope = _appLifetimeScope.CreateChild(builder =>
            {
                builder.RegisterInstance(serviceClient).As<ISampleService>();
                builder.RegisterInstance(playersServiceClient).As<IPlayersService>();
                builder.RegisterInstance(matchMakerServiceClient).As<IMatchMakerService>();
            });
        }

        private static void InitializeGrpc()
        {
            GrpcChannelProviderHost.Initialize(new DefaultGrpcChannelProvider(() => new GrpcChannelOptions
            {
                HttpHandler = new YetAnotherHttpHandler
                {
                    Http2Only = true,
                    Http2KeepAliveInterval = TimeSpan.FromSeconds(30), // Reduced from 60
                    Http2KeepAliveTimeout = TimeSpan.FromSeconds(15),  // Reduced from 30
                },
                DisposeHttpClient = true,
                // Add retry policy
                ServiceConfig = new Grpc.Net.Client.Configuration.ServiceConfig
                {
                    MethodConfigs =
                    {
                        new Grpc.Net.Client.Configuration.MethodConfig
                        {
                            Names = { Grpc.Net.Client.Configuration.MethodName.Default },
                            RetryPolicy = new Grpc.Net.Client.Configuration.RetryPolicy
                            {
                                MaxAttempts = 3,
                                InitialBackoff = TimeSpan.FromSeconds(1),
                                MaxBackoff = TimeSpan.FromSeconds(5),
                                BackoffMultiplier = 1.5,
                                RetryableStatusCodes = { Grpc.Core.StatusCode.Unavailable }
                            }
                        }
                    }
                }
            }));
        }
        
        private void CreateServicesChannel()
        {
            try
            {
                _servicesChannel = GrpcChannelx.ForAddress("https://clashcore-services-280011189315.us-central1.run.app");
                _debugService?.Log("Services channel created successfully");
            }
            catch (Exception ex)
            {
                _debugService?.LogError($"Failed to create services channel: {ex.Message}");
                throw;
            }
        }
        
        public void CreateMatchChannel(string url)
        {
            try
            {
                _matchChannel = GrpcChannelx.ForAddress(url);
                _debugService?.Log("Match channel created successfully");
            }
            catch (Exception ex)
            {
                _debugService?.LogError($"Failed to create match channel: {ex.Message}");
                throw;
            }
        }

        public async UniTask<TService> GetService<TService>() where TService : IService<TService>
        {
            await UniTask.WaitUntil(() => _networkScope != null);
            return _networkScope.Container.Resolve<TService>();
        }

        public async UniTask<THub> ConnectHub<THub, THubReceiver>(THubReceiver receiver) where THub : IStreamingHub<THub, THubReceiver>
        {
            if (_matchChannel == null)
            {
                throw new InvalidOperationException("Match channel not created. Call CreateMatchChannel first.");
            }
            
            var hubClient = await StreamingHubClient.ConnectAsync<THub, THubReceiver>(_matchChannel, receiver);
            return hubClient;
        }
        
        public void Dispose()
        {
            
            try
            {
                _servicesChannel?.Dispose();
                _matchChannel?.Dispose();
                _networkScope?.Dispose();
                _debugService?.Log("NetworkService disposed successfully");
            }
            catch (Exception ex)
            {
                _debugService?.LogError($"Error disposing NetworkService: {ex.Message}");
            }
        }
    }
}