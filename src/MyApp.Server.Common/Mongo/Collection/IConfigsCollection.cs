using Server.Mongo.Entity;

namespace Server.Mongo.Collection
{
    public interface IConfigsCollection
    {
        Task<Config?> GetConfigAsync(string key);
        Task SetConfigAsync(string key, string value);
    }
}
