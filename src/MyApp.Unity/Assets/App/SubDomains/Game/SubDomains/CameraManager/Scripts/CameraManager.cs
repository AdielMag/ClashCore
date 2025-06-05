using App.SubDomains.Game.SubDomains.PlayersManager;

using Cinemachine;

using VContainer;

namespace App.SubDomains.Game.SubDomains.CameraManager.Scripts
{
    public class CameraManager : ICameraManager
    {
        [Inject] private readonly CinemachineVirtualCamera _virtualCamera;
        [Inject] private readonly CinemachineBrain _cameraBrain;
        [Inject] private readonly ILocalPlayerProvider _localPlayerProvider;
        
        public void Initialize()
        {
            var target = _localPlayerProvider.LocalPlayerController.ViewGameObject.transform;
            _virtualCamera.Follow = target;
            _virtualCamera.LookAt = target;
        }
    }
}