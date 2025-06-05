using System.Collections.Concurrent;

using MagicOnion.Server.Hubs;

using Shared.Data;

namespace Server.Hubs.GamingHub.Data
{
    public class Room 
    {
        public string Name { get; }
        public IGroup Group { get; }
        public IInMemoryStorage<TransformData> Storage { get; }
        public ConcurrentDictionary<string, PlayerConnection> Players { get; }

        public Room(string name, IGroup group, IInMemoryStorage<TransformData> storage)
        {
            Name = name;
            Group = group;
            Storage = storage;
            Players = new ConcurrentDictionary<string, PlayerConnection>();
        }
    }
}