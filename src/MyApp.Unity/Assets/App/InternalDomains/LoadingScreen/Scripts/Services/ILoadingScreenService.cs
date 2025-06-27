using App.Scripts.View;

using Cysharp.Threading.Tasks;

namespace App.InternalDomains.LoadingScreen.Scripts.Services
{
    public interface ILoadingScreenService
    {
        UniTask<LoadingScreenBar> ShowLoadingScreenAsync();
        UniTask HideLoadingScreenAsync();
    }
}