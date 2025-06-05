using System.Numerics;

namespace Shared.Controller.PhysicsController.Interface
{
    public interface ITransformable
    {
        Vector3 Position { get; set; }
        Quaternion Rotation { get; set; }
        Vector3 Scale { get; set; }
    }
}