using App.SubDomains.Game.Scripts.Interface;
using App.SubDomains.Game.Scripts.Utils;
using App.SubDomains.Game.SubDomains.GameNetworkHub;
using App.SubDomains.Game.SubDomains.InputManager.Scripts;
using App.SubDomains.Game.SubDomains.PlayerController.Scripts.ScriptableObjects;

using Cysharp.Threading.Tasks;

using Shared.Controller.PhysicsController.Interface;
using Shared.Data;

using UnityEngine;

using VContainer;

namespace App.SubDomains.Game.SubDomains.PlayerController.Scripts.Controller
{
    public class LocalPlayerController : PlayerControllerBase
    {
        private readonly IInputManager _inputManager;
        private readonly IGameHubNetworkManager _gameHub;
        private readonly PlayerPhysicsSettings _playerPhysicsSettings;
        private readonly IProximityService _proximityService;

        private IPhysicsController _physicsController;
        private bool _isCheckingTarget;

        public LocalPlayerController(IGameHubNetworkManager gameHub,
                                     PlayerPhysicsSettings playerPhysicsSettings,
                                     IInputManager inputManager,
                                     IProximityService proximityService)
        {
            _gameHub = gameHub;
            _playerPhysicsSettings = playerPhysicsSettings;
            _inputManager = inputManager;
            _proximityService = proximityService;
        }

        public override void Create(TransformData transformData)
        {
            base.Create(transformData);
            view.name = $"Local-> {view.name}";
            
            _physicsController = resolver.Resolve<IPhysicsController>();

            var accelerationCurve = _playerPhysicsSettings.AccelerationCurve.ToPhysicsCurve();
            var decelerationCurve = _playerPhysicsSettings.DecelerationCurve.ToPhysicsCurve();

            _physicsController.Setup(view,
                                     _playerPhysicsSettings.Speed,
                                     _playerPhysicsSettings.RotationSpeed,
                                     accelerationCurve,
                                     decelerationCurve);
        }

        public override void LateTick()
        {
            var positionChangedData =
                _physicsController.Move(_inputManager.NormalizedMovement, Time.deltaTime);
            if (positionChangedData.HasMoved)
            {
                _gameHub.MoveAsync(view.Position, view.Rotation).Forget();
            }

            var rotationChangedData =
                _physicsController.Rotate(_inputManager.NormalizedMovement, Time.deltaTime);
            if (rotationChangedData.HasRotated)
            {
                _gameHub.MoveAsync(view.Position, view.Rotation).Forget();
            }

            CheckForTarget();
        }
        
        private void CheckForTarget()
        {
            if (_isCheckingTarget)
            {
                return;
            }

            _isCheckingTarget = true;

            try
            {
                var target = _proximityService.GetNearbyTarget(view.transform, view.transform.position, 100);
                UpdateTarget(target ? target : null);

                var targetMoveSpeed =
                    target ? _playerPhysicsSettings.LookAtSpeed : _playerPhysicsSettings.Speed;
                _physicsController.SetMoveSpeed(targetMoveSpeed);
            }
            finally
            {
                _isCheckingTarget = false;
            }
        }
        
        public override void Dispose()
        {
            base.Dispose();
            
            _physicsController.Dispose();
        }
    }
}