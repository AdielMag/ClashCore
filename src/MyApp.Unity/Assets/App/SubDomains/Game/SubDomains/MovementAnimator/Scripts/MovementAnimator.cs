using App.SubDomains.Game.SubDomains.PlayerController.Scripts.ScriptableObjects;

using UnityEngine;

using VContainer;

namespace App.SubDomains.Game.SubDomains.MovementAnimator.Scripts
{
    public class MovementAnimator : MonoBehaviour
    {
        private static readonly int _sForwardHash = Animator.StringToHash("Forward");
        private static readonly int _sSwayHash = Animator.StringToHash("Sway");
        
        [SerializeField] private Animator animator;
        [SerializeField] private float smoothTime = 0.1f;

        [Inject] private readonly PlayerPhysicsSettings _playerPhysicsSettings;

        private Vector3 _lastPosition;
        private Vector3 _currentPosition;
        private Vector3 _movementDelta;
        private Quaternion _currentRotation;
        
        private float _currentForward;
        private float _currentSway;
        private float _forwardVelocity;
        private float _swayVelocity;
        
        private bool _isInitialized;

        private void Awake()
        {
            if (_isInitialized)
            {
                return;
            }

            _lastPosition = transform.position;
            _currentRotation = transform.rotation;
            _isInitialized = true;
        }

        private void LateUpdate()
        {
            UpdateMovementParameters();
        }

        private void UpdateMovementParameters()
        {
            // Cache current transform state
            _currentPosition = transform.position;
            _currentRotation = transform.rotation;

            // Calculate movement in world space
            _movementDelta = _currentPosition - _lastPosition;

            // Convert to local space relative to character rotation
            var localDelta = Quaternion.Inverse(_currentRotation) * _movementDelta;

            // Calculate target values (normalize by time to handle varying frame rates)
            var rawForward = localDelta.z / Time.deltaTime;
            var rawSway = localDelta.x / Time.deltaTime;

            ;
            // Normalize and clamp the values
            var targetForward = Mathf.Abs(rawForward) / _playerPhysicsSettings.Speed;
            var targetSway = Mathf.Clamp(rawSway / _playerPhysicsSettings.Speed, -1f, 1f);

            // Smooth the values
            _currentForward = Mathf.SmoothDamp(_currentForward, targetForward, ref _forwardVelocity, smoothTime);
            _currentSway = Mathf.SmoothDamp(_currentSway, targetSway, ref _swayVelocity, smoothTime);

            // Update animator
            animator.SetFloat(_sForwardHash, _currentForward);
            animator.SetFloat(_sSwayHash, _currentSway);

            // Store position for next frame
            _lastPosition = _currentPosition;
        }

        public void ResetMovement()
        {
            _currentForward = 0f;
            _currentSway = 0f;
            _forwardVelocity = 0f;
            _swayVelocity = 0f;
            animator.SetFloat(_sForwardHash, 0f);
            animator.SetFloat(_sSwayHash, 0f);
        }
    }
}