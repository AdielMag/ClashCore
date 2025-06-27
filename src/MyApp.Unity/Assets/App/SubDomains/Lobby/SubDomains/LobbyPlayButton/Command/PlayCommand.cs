using System.Threading;

using App.InternalDomains.LoadingScreen.Scripts.Services;
using App.InternalDomains.SceneService;
using App.Scripts.Command;

using Cysharp.Threading.Tasks;

namespace App.SubDomains.Lobby.SubDomains.LobbyPlayButton
{
    public class PlayCommand : Command
    {
        private readonly ISceneService _sceneService;
        private readonly ILoadingScreenService _loadingScreenService;
        
        public PlayCommand(ISceneService sceneService, ILoadingScreenService loadingScreenService)
        {
            _sceneService = sceneService;
            _loadingScreenService = loadingScreenService;
        }

        public override async UniTask ExecuteAsync(CancellationToken cancellationToken = default)
        {
            var loadingBar = await _loadingScreenService.ShowLoadingScreenAsync();

            loadingBar.UpdateProgressAsync(.5f).Forget();
            
            await _sceneService.LoadSceneAsync(SceneConstants.GameScene, cancellationToken: cancellationToken);
            
            loadingBar.UpdateProgressAsync(.75f).Forget();
            
            await _sceneService.UnloadSceneAsync(SceneConstants.LobbyScene, cancellationToken: cancellationToken);

            await loadingBar.UpdateProgressAsync(1f);

            await _loadingScreenService.HideLoadingScreenAsync();
        }
    }
}