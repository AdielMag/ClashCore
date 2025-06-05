using System.Numerics;

namespace Server.Hubs.GamingHub.Validators.MovementValidator
{
    public class MovementValidator : IMovementValidator
    {
        private const float MAX_MOVEMENT_SPEED = 10f;
        private const float MAX_TELEPORT_DISTANCE = 5f;

        public MovementValidationResult ValidateMovement(Vector3 currentPosition, Vector3 newPosition, float deltaTime)
        {
            if (deltaTime <= 0)
            {
                return new MovementValidationResult
                {
                    IsValid = false,
                    ErrorMessage = "Invalid delta time",
                    CorrectedPosition = currentPosition
                };
            }

            var distance = Vector3.Distance(currentPosition, newPosition);
            var speed = distance / deltaTime;

            /*if (speed > MAX_MOVEMENT_SPEED)
            {
                var direction = Vector3.Normalize(newPosition - currentPosition);
                var maxPosition = currentPosition + direction * MAX_MOVEMENT_SPEED * deltaTime;

                return new MovementValidationResult
                {
                    IsValid = false,
                    ErrorMessage = $"Movement speed exceeds maximum allowed speed: {speed:F2} > {MAX_MOVEMENT_SPEED}",
                    CorrectedPosition = maxPosition
                };
            }

            if (distance > MAX_TELEPORT_DISTANCE)
            {
                return new MovementValidationResult
                {
                    IsValid = false,
                    ErrorMessage = $"Teleport distance too large: {distance:F2} > {MAX_TELEPORT_DISTANCE}",
                    CorrectedPosition = currentPosition
                };
            }*/

            return new MovementValidationResult
            {
                IsValid = true,
                CorrectedPosition = newPosition
            };
        }
    }

}