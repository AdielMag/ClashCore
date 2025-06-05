using System;

using Shared.Controller.PhysicsController.Interface;
using UnityEngine;
using Vector3 = System.Numerics.Vector3;
using Quaternion = System.Numerics.Quaternion;

namespace App.SubDomains.Game.SubDomains.Transformable
{
    public abstract class Transformable : MonoBehaviour, ITransformable
    {
        private const float _kEpsilon = 0.000001f;

        private Vector3 _position;
        private Quaternion _rotation;
        private Vector3 _scale;

        private UnityEngine.Vector3 _unityPosition;
        private UnityEngine.Quaternion _unityRotation;
        private UnityEngine.Vector3 _unityScale;
        
        public Vector3 Position
        {
            get => _position;
            set
            {
                _position = value;
                _unityPosition.Set(value.X, value.Y, value.Z);
                transform.position = _unityPosition;
            }
        }

        public Quaternion Rotation
        {
            get => _rotation;
            set
            {
                _rotation = value;
                
                // Convert System.Numerics quaternion to Unity quaternion
                // Ensure the quaternion is normalized
                var norm = MathF.Sqrt(value.X * value.X + value.Y * value.Y + 
                                    value.Z * value.Z + value.W * value.W);
                
                if (norm > float.Epsilon)
                {
                    _unityRotation.x = value.X / norm;
                    _unityRotation.y = value.Y / norm;
                    _unityRotation.z = value.Z / norm;
                    _unityRotation.w = value.W / norm;
                    transform.rotation = _unityRotation;
                }
            }
        }

        public Vector3 Scale
        {
            get => _scale;
            set
            {
                _scale = value;
                _unityScale.Set(value.X, value.Y, value.Z);
                transform.localScale = _unityScale;
            }
        }

        protected virtual void Awake()
        {
            InitializeVectors();
        }

        protected virtual void Update()
        {
            SyncUnityTransform();
        }

        private void InitializeVectors()
        {
            _unityPosition = transform.position;
            _unityRotation = transform.rotation;
            _unityScale = transform.localScale;

            _position = new Vector3(_unityPosition.x, _unityPosition.y, _unityPosition.z);
            _rotation = new Quaternion(
                _unityRotation.x,
                _unityRotation.y,
                _unityRotation.z,
                _unityRotation.w
            );
            _scale = new Vector3(_unityScale.x, _unityScale.y, _unityScale.z);
        }

        private void SyncUnityTransform()
        {
            SyncPosition();
            SyncRotation();
            SyncScale();
        }

        private void SyncPosition()
        {
            _unityPosition = transform.position;

            var currentX = _position.X;
            var currentY = _position.Y;
            var currentZ = _position.Z;

            if (ApproximatelyEqual(currentX, _unityPosition.x) &&
                ApproximatelyEqual(currentY, _unityPosition.y) &&
                ApproximatelyEqual(currentZ, _unityPosition.z))
            {
                return;
            }

            _position.X = _unityPosition.x;
            _position.Y = _unityPosition.y;
            _position.Z = _unityPosition.z;
        }

        private void SyncRotation()
        {
            _unityRotation = transform.rotation;

            var currentX = _rotation.X;
            var currentY = _rotation.Y;
            var currentZ = _rotation.Z;
            var currentW = _rotation.W;

            if (ApproximatelyEqual(currentX, _unityRotation.x) &&
                ApproximatelyEqual(currentY, _unityRotation.y) &&
                ApproximatelyEqual(currentZ, _unityRotation.z) &&
                ApproximatelyEqual(currentW, _unityRotation.w))
            {
                return;
            }

            _rotation = new Quaternion(
                _unityRotation.x,
                _unityRotation.y,
                _unityRotation.z,
                _unityRotation.w
            );
        }

        private void SyncScale()
        {
            _unityScale = transform.localScale;

            var currentX = _scale.X;
            var currentY = _scale.Y;
            var currentZ = _scale.Z;

            if (ApproximatelyEqual(currentX, _unityScale.x) &&
                ApproximatelyEqual(currentY, _unityScale.y) &&
                ApproximatelyEqual(currentZ, _unityScale.z))
            {
                return;
            }

            _scale.X = _unityScale.x;
            _scale.Y = _unityScale.y;
            _scale.Z = _unityScale.z;
        }

        private bool ApproximatelyEqual(float a, float b)
        {
            return Mathf.Abs(a - b) < _kEpsilon;
        }
    }
}