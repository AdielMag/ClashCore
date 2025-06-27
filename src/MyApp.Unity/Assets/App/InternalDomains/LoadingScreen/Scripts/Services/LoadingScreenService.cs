using System;

using App.InternalDomains.LifeTimeScopesRegistry;
using App.InternalDomains.SceneService;
using App.Scripts.View;

using Cysharp.Threading.Tasks;

namespace App.InternalDomains.LoadingScreen.Scripts.Services
{
    public class LoadingScreenService : ILoadingScreenService,
                                        IDisposable
    {
        private readonly ISceneService _sceneService;
        private readonly ILifeTimeScopeRegistry _lifeTimeScopeRegistry;
        
        private LoadingScreenBar _loadingScreenBar;
        
        public LoadingScreenService(ISceneService sceneService,
                                    ILifeTimeScopeRegistry lifeTimeScopeRegistry)
        {
            _sceneService = sceneService;
            _lifeTimeScopeRegistry = lifeTimeScopeRegistry;
        }
        
        public async UniTask<LoadingScreenBar> ShowLoadingScreenAsync()
        {
            await _sceneService.LoadSceneAsync(SceneConstants.LoadingScreen);
            
            _loadingScreenBar = _lifeTimeScopeRegistry.Resolve<LoadingScreenBar>(LifeTimeScopeType.LoadingScreen);
            _loadingScreenBar.UpdateProgress(0);
            return _loadingScreenBar;
        }
        
        public async UniTask HideLoadingScreenAsync()
        {
            _loadingScreenBar = null;

            await _sceneService.UnloadSceneAsync(SceneConstants.LoadingScreen);
        }

        public void Dispose()
        {
            _loadingScreenBar = null;
        }
    }
}