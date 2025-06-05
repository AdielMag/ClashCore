using System.Numerics;

namespace Server.Hubs.GamingHub.Validators.MovementValidator
{
    public interface IMovementValidator
    {
        MovementValidationResult ValidateMovement(Vector3 currentPosition, Vector3 newPosition, float deltaTime);
    }
}