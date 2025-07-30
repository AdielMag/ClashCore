using System;

using App.InternalDomains.DebugService;
using App.InternalDomains.NetworkService;
using App.InternalDomains.PlayersService;
using App.SubDomains.Game.SubDomains.CameraManager.Scripts;
using App.SubDomains.Game.SubDomains.Environment.Scripts.Interface;
using App.SubDomains.Game.SubDomains.GameNetworkHub;

using Cysharp.Threading.Tasks;

using Shared.Data;
using Shared.Services;

using VContainer;
using VContainer.Unity;

namespace App.SubDomains.Game.Scripts
{
    public class GameStarter : IInitializable
    {
        private readonly INetworkService _networkService;
        private readonly IGameHubNetworkManager _gameNetworkHubManager;
        private readonly ICameraManager _cameraManager;
        private readonly IEnvironmentManager _environmentManager;
        private readonly IPlayerIdProvider _playerIdProvider;
        private readonly IDebugService _debugService;
        
        private IMatchMakerService _matchMakerService;

        public GameStarter(INetworkService networkService,
                           IGameHubNetworkManager gameNetworkHubManager,
                           ICameraManager cameraManager,
                           IEnvironmentManager environmentManager,
                           IPlayerIdProvider playerIdProvider,
                           IDebugService debugService)
        {
            _networkService = networkService;
            _gameNetworkHubManager = gameNetworkHubManager;
            _cameraManager = cameraManager;
            _environmentManager = environmentManager;
            _playerIdProvider = playerIdProvider;
            _debugService = debugService;
        }

        public void Initialize()
        {
            InitializeAsync().Forget();
        }
        
        private async UniTask InitializeAsync()
        {
            _debugService.Log("GameStarter: Initializing game...");
            
            _matchMakerService ??= await _networkService.GetService<IMatchMakerService>();

            _debugService.Log("GameStarter: MatchMakerService initialized.");
            var playerId = _playerIdProvider.PlayerId;
            var test = await _matchMakerService.Test();
            _debugService.Log($"GameStarter: MatchMakerService test result: {test}");

            var matchData =
                await _matchMakerService.JoinMatchAsync(playerId, MatchType.Casual, string.Empty);
            
            _debugService.Log($"GameStarter: Player {playerId} joined match {matchData.MatchId} at {matchData.Url}:{matchData.Port}.");
            _networkService.CreateMatchChannel(matchData.Url, matchData.Port);

            await _gameNetworkHubManager.ConnectAsync(matchData.MatchId, playerId);
            
            await _environmentManager.LoadEnvironment("Environment_01");
            
            _cameraManager.Initialize();
        }
    }
}