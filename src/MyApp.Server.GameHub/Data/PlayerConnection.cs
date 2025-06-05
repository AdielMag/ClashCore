using System.Numerics;

using Shared.Data;

namespace Server.Hubs.GamingHub.Data
{
    public class PlayerConnection
    {
        public string Id { get; }
        public string ConnectionId { get; }
        public TransformData TransformData { get; }
        public DateTime LastUpdateTime { get; private set; }

        public PlayerConnection(string id, string connectionId, TransformData transformData)
        {
            Id = id;
            ConnectionId = connectionId;
            TransformData = transformData;
            LastUpdateTime = DateTime.UtcNow;
        }

        public void UpdateTransform(Vector3 position, Quaternion rotation)
        {
            TransformData.Position = position;
            TransformData.Rotation = rotation;
            LastUpdateTime = DateTime.UtcNow;
        }
    }
}