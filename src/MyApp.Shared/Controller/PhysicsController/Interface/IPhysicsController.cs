using System;
using System.Numerics;
using Shared.Data;

namespace Shared.Controller.PhysicsController.Interface
{
    public interface IPhysicsController
    {
        event Action<PositionChangedEventArgs> OnPositionChanged;
        event Action<RotationChangedEventArgs> OnRotationChanged;

        void Setup(ITransformable transform,
                   float         moveSpeed,
                   float         rotationSpeedDeg,
                   Curve         accelerationCurve = null,
                   Curve         decelerationCurve = null);

        void Rotate(Vector2 normalizedInput, float deltaTime);
        void Move(Vector2 normalizedInput, float deltaTime);
        void Dispose();
    }
}