using System;
using System.Numerics;
using App.InternalDomains.DebugService;
using UnityEngine.InputSystem;
using VContainer;
using VContainer.Unity;

namespace App.SubDomains.Game.SubDomains.InputManager.Scripts
{
    public class InputManager : IInputManager, IInitializable, IDisposable
    {
        private Vector2 _normalizedMovement;
        public Vector2 NormalizedMovement => _normalizedMovement;

        [Inject] private readonly InputActionAsset _inputActionAsset;
        [Inject] private readonly IDebugService _debugService;
        
        private InputAction _moveAction;
        
        public void Initialize()
        {
            var gameplay = _inputActionAsset.FindActionMap("gameplay");
            _moveAction = gameplay.FindAction("movement");
            _moveAction.performed += OnMovementPerformed;
            _moveAction.canceled += OnMovementCanceled;
            gameplay.Enable();
        }
        
        public void Dispose()
        {
            if (_moveAction != null)
            {
                _moveAction.performed -= OnMovementPerformed;
                _moveAction.canceled -= OnMovementCanceled;
                _moveAction.Dispose();
                _moveAction = null;
            }
        }

        private void OnMovementPerformed(InputAction.CallbackContext context)
        {
            UpdateMovement(context.ReadValue<UnityEngine.Vector2>());
        }

        private void OnMovementCanceled(InputAction.CallbackContext context)
        {
            UpdateMovement(UnityEngine.Vector2.zero);
        }

        private void UpdateMovement(UnityEngine.Vector2 unityMovement)
        {
            unityMovement = unityMovement.normalized;
            _normalizedMovement.X = unityMovement.x;
            _normalizedMovement.Y = unityMovement.y;
        }
    }
}