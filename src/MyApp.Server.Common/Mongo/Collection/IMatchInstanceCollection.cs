using System.Threading.Tasks;
using Server.Mongo.Entity;

namespace Server.Mongo.Collection
{
    public interface IMatchInstanceCollection
    {
        Task<MatchInstance?> TryAllocateInstanceAsync(int capacity, int requiredSlots);
        Task<MatchInstance> CreateInstanceAsync(MatchInstance instance);
    }
}
