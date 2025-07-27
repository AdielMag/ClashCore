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
                    Http2KeepAliveInterval = TimeSpan.FromSeconds(60),
                    Http2KeepAliveTimeout = TimeSpan.FromSeconds(30),
                    SkipCertificateVerification = true
                },
                DisposeHttpClient = true
            }));
        }
        
        private void CreateServicesChannel()
        {
            _servicesChannel = GrpcChannelx.ForTarget(new GrpcChannelTarget("clashcore-services.onrender.com", 5002, false));
        }
        
        public void CreateMatchChannel(string url, int port)
        {
            _matchChannel = GrpcChannelx.ForTarget(new GrpcChannelTarget(url, port, false));
        }

        public async UniTask<TService> GetService<TService>() where TService : IService<TService>
        {
            await UniTask.WaitUntil(() => _networkScope);
            return _networkScope.Container.Resolve<TService>();
        }

        public async UniTask<THub> ConnectHub<THub, THubReceiver>(THubReceiver receiver) where THub : IStreamingHub<THub, THubReceiver>
        {
            var hubClient = await StreamingHubClient.ConnectAsync<THub, THubReceiver>(_matchChannel, receiver);
            return hubClient;
        }
        
        public void Dispose()
        {
            _appLifetimeScope?.Dispose();
            _servicesChannel?.Dispose();
            _networkScope?.Dispose();
        }
    }
}
