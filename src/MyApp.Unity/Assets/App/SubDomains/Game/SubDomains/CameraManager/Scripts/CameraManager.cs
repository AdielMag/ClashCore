using App.SubDomains.Game.SubDomains.PlayersManager;

using Cinemachine;


namespace App.SubDomains.Game.SubDomains.CameraManager.Scripts
{
    public class CameraManager : ICameraManager
    {
        private readonly CinemachineVirtualCamera _virtualCamera;
        private readonly CinemachineBrain _cameraBrain;
        private readonly ILocalPlayerProvider _localPlayerProvider;

        public CameraManager(CinemachineVirtualCamera virtualCamera,
                             CinemachineBrain cameraBrain,
                             ILocalPlayerProvider localPlayerProvider)
        {
            _virtualCamera = virtualCamera;
            _cameraBrain = cameraBrain;
            _localPlayerProvider = localPlayerProvider;
        }

        public void Initialize()
        {
            var target = _localPlayerProvider.LocalPlayerController.ViewGameObject.transform;
            _virtualCamera.Follow = target;
            _virtualCamera.LookAt = target;
        }
    }
}