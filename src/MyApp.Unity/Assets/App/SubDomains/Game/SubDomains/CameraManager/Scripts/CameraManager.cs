using App.SubDomains.Game.SubDomains.PlayersManager;

using Cinemachine;

using UnityEngine;

namespace App.SubDomains.Game.SubDomains.CameraManager.Scripts
{
    public class CameraManager : ICameraManager
    {
        private readonly CinemachineVirtualCamera _virtualCamera;
        private readonly CinemachineBrain _cameraBrain;
        private readonly ILocalPlayerProvider _localPlayerProvider;
        private readonly CinemachineTransposer _transposer;
        
        public CameraManager(CinemachineVirtualCamera virtualCamera,
                             CinemachineBrain cameraBrain,
                             ILocalPlayerProvider localPlayerProvider)
        {
            _virtualCamera = virtualCamera;
            _transposer = _virtualCamera.GetCinemachineComponent<CinemachineTransposer>();
            _cameraBrain = cameraBrain;
            _localPlayerProvider = localPlayerProvider;
        }

        public void Initialize()
        {
            var target = _localPlayerProvider.LocalPlayerController.ViewGameObject.transform;
            _virtualCamera.Follow = target;
            _virtualCamera.LookAt = target;
        }

        public float GetCameraAngleOffsetDeg()
        {
            var offset = _transposer.m_FollowOffset;
            var angle = -Mathf.Atan2(-offset.x, -offset.z) * Mathf.Rad2Deg;
            return angle;
        }
    }
}