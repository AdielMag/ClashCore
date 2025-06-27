using System;
using System.Numerics;

using Shared.Data;

using Time = UnityEngine.Time;

namespace App.SubDomains.Game.SubDomains.PlayerController.Scripts.Controller
{
    public class RemotePlayerController : PlayerControllerBase
    {
        private const float POSITION_LERP_SPEED = 15f;
        private const float ROTATION_LERP_SPEED = 15f;
        private const float SNAP_THRESHOLD = 3f;
        private const float EPSILON = 0.001f;
        
        private Vector3 _targetPosition;
        private Quaternion _targetRotation;
        
        public override void Create(TransformData transformData)
        {
            base.Create(transformData);
            view.name = $"Remote-> {view.name}";
            
            _targetPosition = transformData.Position;
            _targetRotation = transformData.Rotation;
        }

        public override void UpdatePositionAndRotation(Vector3 position, Quaternion rotation)
        {
            _targetPosition = position;
            _targetRotation = rotation;
            
            if (Vector3.Distance(view.Position, _targetPosition) > SNAP_THRESHOLD)
            {
                view.Position = _targetPosition;
                view.Rotation = _targetRotation;
            }
        }

        public override void LateTick()
        {
            var lerpFactor = 1f - MathF.Exp(-POSITION_LERP_SPEED * Time.deltaTime);
            
            var currentPosition = view.Position;
            var currentRotation = view.Rotation;
            
            // Only update if needed
            if (Vector3.Distance(currentPosition, _targetPosition) > EPSILON)
            {
                view.Position = Vector3.Lerp(currentPosition, _targetPosition, lerpFactor);
            }
            
            if (! IsAlmostEqual(currentRotation, _targetRotation))
            {
                view.Rotation = Quaternion.Slerp(currentRotation, _targetRotation, lerpFactor);
            }
        }

        private static bool IsAlmostEqual(Quaternion a, Quaternion b)
        {
            return Quaternion.Dot(a, b) > (1f - EPSILON);
        }
    }
}
