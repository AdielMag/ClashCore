using System.Numerics;

using MessagePack;

namespace Shared.Data
{
    [MessagePackObject]
    public class TransformData
    {
        [Key(0)]
        public string Id { get; set; }
        [Key(1)]
        public Vector3 Position { get; set; }
        [Key(2)]
        public Quaternion Rotation { get; set; }
    }

}