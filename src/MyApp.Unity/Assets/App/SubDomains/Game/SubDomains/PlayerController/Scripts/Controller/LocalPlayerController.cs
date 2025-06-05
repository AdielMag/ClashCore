using App.SubDomains.Game.Scripts.Utils;
using App.SubDomains.Game.SubDomains.GameNetworkHub;
using App.SubDomains.Game.SubDomains.InputManager.Scripts;
using App.SubDomains.Game.SubDomains.PlayerController.Scripts.ScriptableObjects;

using Shared.Controller.PhysicsController.Controller;
using Shared.Controller.PhysicsController.Interface;
using Shared.Data;

using UnityEngine;

using VContainer;

namespace App.SubDomains.Game.SubDomains.PlayerController.Scripts.Controller
{
    public class LocalPlayerController : PlayerControllerBase
    {
        [Inject] private readonly IInputManager _inputManager;
        [Inject] private readonly IGameHubNetworkManager _gameHub;
        [Inject] private readonly PlayerPhysicsSettings _playerPhysicsSettings;
        
        private IPhysicsController _physicsController;

        public override void Create(TransformData transformData)
        {
            base.Create(transformData);
            view.name = $"Local-> {view.name}";
            
            _physicsController = resolver.Resolve<IPhysicsController>();

            var accelerationCurve = _playerPhysicsSettings.AccelerationCurve.ToPhysicsCurve();
            var decelerationCurve = _playerPhysicsSettings.DecelerationCurve.ToPhysicsCurve();

            _physicsController.Setup(view, _playerPhysicsSettings.Speed, _playerPhysicsSettings.RotationSpeed,
                                     accelerationCurve, decelerationCurve);
            
            _physicsController.OnPositionChanged += HandlePositionChanged;
            _physicsController.OnRotationChanged += HandleRotationChanged;
        }

        public override void LateTick()
        {
            _physicsController.Move(_inputManager.NormalizedMovement, Time.deltaTime); 
            _physicsController.Rotate(_inputManager.NormalizedMovement, Time.deltaTime);
        }
        
        private void HandlePositionChanged(PositionChangedEventArgs eventArgs)
        {
            _gameHub.MoveAsync(view.Position, view.Rotation);
        }

        private void HandleRotationChanged(RotationChangedEventArgs eventArgs)
        {
            _gameHub.MoveAsync(view.Position, view.Rotation);
        }
        
        public override void Dispose()
        {
            base.Dispose();
            
            _physicsController.OnPositionChanged -= HandlePositionChanged;
            _physicsController.OnRotationChanged -= HandleRotationChanged;
            _physicsController.Dispose();
        }
    }
}