using System;
using App.SubDomains.Game.SubDomains.PlayerController.Scripts.ScriptableObjects;
using UnityEngine;
using VContainer;

namespace App.SubDomains.Game.SubDomains.MovementAnimator.Scripts
{
    /// <summary>
    /// Controls the player visual GameObject.
    /// • When a look-at target exists: the visual always faces the target and the
    ///   animator receives Vertical / Horizontal parameters for proper strafing.
    /// • When no target exists: the visual returns to its original local rotation
    ///   and the animator switches back to Forward / Sway parameters.
    /// </summary>
    public class PlayerVisualController : MonoBehaviour
    {
        // Animator parameter hashes
        private static readonly int _sForwardHash    = Animator.StringToHash("Forward");
        private static readonly int _sSwayHash       = Animator.StringToHash("Sway");
        private static readonly int _sVerticalHash   = Animator.StringToHash("Vertical");
        private static readonly int _sHorizontalHash = Animator.StringToHash("Horizontal");
        private static readonly int _sLookAtHash     = Animator.StringToHash("LookAtTarget");

        [SerializeField] private Animator animator;
        [SerializeField] private float smoothTime = 0.1f;     // Damping time for SmoothDamp

        [Inject] private readonly PlayerPhysicsSettings _playerPhysicsSettings;

        // Movement state
        private Vector3 _lastPosition;
        private Vector3 _movementDelta;

        private float _currentForward;
        private float _currentSway;
        private float _forwardVelocity;
        private float _swayVelocity;

        // Look-at state
        private Transform _lookAtTarget;
        private Quaternion _initialLocalRotation;             // Default local rotation of the visual

        // LateUpdate delegate (switched at runtime)
        private Action _lateUpdateAction;

        private void Awake()
        {
            _initialLocalRotation = transform.localRotation;
            _lastPosition         = transform.position;

            // Start with no target
            UpdateLookAtTarget(null);
        }

        private void LateUpdate() => _lateUpdateAction?.Invoke();

        /*──────────────────────── Public API ───────────────────────*/
        public void UpdateLookAtTarget(Transform lookAtTarget)
        {
            _lookAtTarget = lookAtTarget;
            bool hasTarget = _lookAtTarget != null;

            animator.SetBool(_sLookAtHash, hasTarget);

            if (hasTarget)
            {
                _lateUpdateAction = UpdateLookAtAndStrafeParameters;
            }
            else
            {
                // Reset visual rotation and strafing parameters
                transform.localRotation = _initialLocalRotation;
                animator.SetFloat(_sVerticalHash,   0f);
                animator.SetFloat(_sHorizontalHash, 0f);

                _lateUpdateAction = UpdateMovementParameters;
            }
        }

        /*──────────────────── No target → Forward / Sway ───────────*/
        private void UpdateMovementParameters()
        {
            Vector3 currentPos  = transform.position;
            _movementDelta      = currentPos - _lastPosition;

            // Delta relative to the character’s forward
            Vector3 localDelta  = transform.InverseTransformDirection(_movementDelta);

            float rawForward    = localDelta.z / Time.deltaTime;
            float rawSway       = localDelta.x / Time.deltaTime;

            float targetForward = Mathf.Abs(rawForward) / _playerPhysicsSettings.Speed;
            float targetSway    = Mathf.Clamp(rawSway / _playerPhysicsSettings.Speed, -1f, 1f);

            _currentForward = Mathf.SmoothDamp(_currentForward, targetForward, ref _forwardVelocity, smoothTime);
            _currentSway    = Mathf.SmoothDamp(_currentSway,    targetSway,    ref _swayVelocity,    smoothTime);

            animator.SetFloat(_sForwardHash, _currentForward);
            animator.SetFloat(_sSwayHash,    _currentSway);

            _lastPosition = currentPos;
        }

        /*──────────────────── Has target → Strafe ──────────────────*/
        private void UpdateLookAtAndStrafeParameters()
        {
            if (_lookAtTarget == null) return;

            /* 1) Rotate the visual to face the target (Y-axis only) */
            Vector3 toTarget = _lookAtTarget.position - transform.position;
            toTarget.y = 0f;
            if (toTarget.sqrMagnitude > 0.0001f)
                transform.rotation = Quaternion.LookRotation(toTarget.normalized, Vector3.up);

            /* 2) Compute movement relative to the new facing direction */
            Vector3 currentPos  = transform.position;
            _movementDelta      = currentPos - _lastPosition;
            Vector3 localDelta  = transform.InverseTransformDirection(_movementDelta);

            float rawForward    = localDelta.z / Time.deltaTime;
            float rawSide       = localDelta.x / Time.deltaTime;

            float targetVert    = Mathf.Clamp(rawForward / _playerPhysicsSettings.LookAtSpeed, -1f, 1f);
            float targetHoriz   = Mathf.Clamp(rawSide    / _playerPhysicsSettings.LookAtSpeed, -1f, 1f);

            float vertVel  = 0f, horizVel = 0f;
            float currentVert  = Mathf.SmoothDamp(animator.GetFloat(_sVerticalHash),  targetVert,  ref vertVel,  smoothTime);
            float currentHoriz = Mathf.SmoothDamp(animator.GetFloat(_sHorizontalHash), targetHoriz, ref horizVel, smoothTime);

            animator.SetFloat(_sVerticalHash,   currentVert);   // Forward / Back
            animator.SetFloat(_sHorizontalHash, currentHoriz);  // Left / Right

            _lastPosition = currentPos;
        }

        /*──────────────────────── Utility ──────────────────────────*/
        public void ResetMovement()
        {
            _currentForward  = _currentSway = 0f;
            _forwardVelocity = _swayVelocity = 0f;

            animator.SetFloat(_sForwardHash,     0f);
            animator.SetFloat(_sSwayHash,        0f);
            animator.SetFloat(_sVerticalHash,    0f);
            animator.SetFloat(_sHorizontalHash,  0f);
        }
    }
}
