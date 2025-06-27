using System.Threading;

using App.InternalDomains.LoadingScreen.Scripts.Services;
using App.InternalDomains.SceneService;
using App.SubDomains.Game.SubDomains.GameNetworkHub;

using Cysharp.Threading.Tasks;

namespace App.SubDomains.Game.Scripts.Command
{
    public class QuitGameCommand : App.Scripts.Command.Command
    {
        private readonly IGameHubNetworkManager _gameHubNetworkManager;
        private readonly ILoadingScreenService _loadingScreenService;
        private readonly ISceneService _sceneService;
        
        public QuitGameCommand(IGameHubNetworkManager gameHubNetworkManager,
                               ILoadingScreenService loadingScreenService,
                               ISceneService sceneService)
        {
            _gameHubNetworkManager = gameHubNetworkManager;
            _loadingScreenService = loadingScreenService;
            _sceneService = sceneService;
        }

        public override async UniTask ExecuteAsync(CancellationToken cancellationToken = default)
        {
            var loadingBar = await _loadingScreenService.ShowLoadingScreenAsync();
            
            loadingBar.UpdateProgressAsync(0.4f).Forget();
            
            await _gameHubNetworkManager.LeaveAsync();

            loadingBar.UpdateProgressAsync(0.7f).Forget();
            
            await _sceneService.LoadSceneAsync(SceneConstants.LobbyScene, cancellationToken: cancellationToken);

            loadingBar.UpdateProgressAsync(0.9f).Forget();
            
            await _sceneService.UnloadSceneAsync(SceneConstants.GameScene, cancellationToken: cancellationToken);
            
            await loadingBar.UpdateProgressAsync(1f);
            
            await _loadingScreenService.HideLoadingScreenAsync();
        }
    }
}