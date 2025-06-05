using MagicOnion.Server.Hubs;

using Server.Hubs.GamingHub.Data;

using Shared.Data;

namespace Server.Hubs.GamingHub.Managers.RoomManager
{
    public interface IRoomManager
    {
        Task<Room> GetOrCreateRoom(string roomName, IGroup group, IInMemoryStorage<TransformData> storage);
        bool TryGetRoom(string roomName, out Room room);
    }
}