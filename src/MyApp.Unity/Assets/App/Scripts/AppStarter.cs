using App.InternalDomains.DebugService;
using App.InternalDomains.LoadingScreen.Scripts.Services;
using App.InternalDomains.NetworkService;
using App.InternalDomains.PlayersService;
using App.InternalDomains.SceneService;
using App.Scripts.View;

using Cysharp.Threading.Tasks;

using VContainer.Unity;

namespace App.Scripts
{
    public class AppStarter : IInitializable
    {
        private readonly ISceneService _sceneService;
        private readonly IPlayersService _playersService;
        private readonly ILoadingScreenService _loadingScreenService;
        
        public AppStarter(ISceneService sceneService,
                          IPlayersService playersService,
                          ILoadingScreenService loadingScreenService)
        {
            _sceneService = sceneService;
            _playersService = playersService;
            _loadingScreenService = loadingScreenService;
        }

        public void Initialize()
        {
            LoadLobbySceneAsync().Forget();
        }
        
        private async UniTask LoadLobbySceneAsync()
        {
            await _playersService.Login();

            var loadingBar = await _loadingScreenService.ShowLoadingScreenAsync();
            
            var lobbyLoadingTask = _sceneService.LoadSceneAsync(SceneConstants.LobbyScene);
            
            await loadingBar.UpdateProgressAsync(.5f);
            
            await lobbyLoadingTask;
            
            await loadingBar.UpdateProgressAsync(1);
            
            await _loadingScreenService.HideLoadingScreenAsync();
            
            _sceneService.UnloadSceneAsync(SceneConstants.AppStarter).Forget();
        }
    }
}