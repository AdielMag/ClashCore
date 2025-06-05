using System;

using App.InternalDomains.DebugService;
using App.InternalDomains.NetworkService;
using App.InternalDomains.PlayersService;
using App.InternalDomains.SceneService;

using Cysharp.Threading.Tasks;

using VContainer;
using VContainer.Unity;

namespace App.Scripts
{
    public class AppStarter : IInitializable
    {
        [Inject] private readonly IDebugService _debugService;
        [Inject] private readonly INetworkService _networkService;
        [Inject] private readonly ISceneService _sceneService;
        [Inject] private readonly IPlayersService _playersService;
        
        public void Initialize()
        {
            LoadGameSceneAsync().Forget();
        }
        
        private async UniTask LoadGameSceneAsync()
        {
            await _playersService.Login();
            
            await _sceneService.LoadSceneAsync(SceneConstants.GameScene);
            _sceneService.UnloadSceneAsync(SceneConstants.AppStarter).Forget();
        }
    }
}