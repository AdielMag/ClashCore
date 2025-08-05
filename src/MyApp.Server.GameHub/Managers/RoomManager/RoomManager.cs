using System.Collections.Concurrent;

using MagicOnion.Server.Hubs;

using Microsoft.Extensions.Logging;

using Server.Hubs.GamingHub.Data;

using Shared.Data;

namespace Server.Hubs.GamingHub.Managers.RoomManager
{
    public class RoomManager : IRoomManager
    {
        private readonly ConcurrentDictionary<string, Room> _rooms = new();
        private readonly ILogger _logger;

        public RoomManager(ILogger logger)
        {
            _logger = logger;
        }

        public async Task<Room> GetOrCreateRoom(string roomName, IGroup group, IInMemoryStorage<TransformData> storage)
        {
            
            return _rooms.GetOrAdd(roomName, _ => new Room(roomName, group, storage));
        }

        public bool TryGetRoom(string roomName, out Room room)
        {
            return _rooms.TryGetValue(roomName, out room);
        }
    }

}