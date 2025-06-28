using System.Numerics;
using Shared.Data;

namespace Shared.Controller.PhysicsController.Interface
{
    public interface IPhysicsController
    {
        void Setup(ITransformable transform,
                   float         moveSpeed,
                   float         rotationSpeedDeg,
                   Curve         accelerationCurve = null,
                   Curve         decelerationCurve = null);

        PositionChangedEventData Move(Vector2 normalizedInput, float deltaTime);
        RotationChangedData Rotate(Vector2 normalizedInput, float deltaTime);
        void SetMoveSpeed(float moveSpeed);
        void SetRotationSpeedDeg(float rotationSpeedDeg);
        void Dispose();
    }
}