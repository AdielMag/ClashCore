using System.Numerics;

namespace Server.Hubs.GamingHub.Validators.MovementValidator
{
    public class MovementValidationResult
    {
        public bool IsValid { get; init; }
        public string ErrorMessage { get; init; }
        public Vector3 CorrectedPosition { get; init; }
    }
}