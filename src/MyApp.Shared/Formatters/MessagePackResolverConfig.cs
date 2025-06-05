using MessagePack;
using MessagePack.Formatters;
using MessagePack.Resolvers;
using Shared.Formatters;

using QuaternionFormatter = Shared.Formatters.QuaternionFormatter;
using Vector3Formatter = Shared.Formatters.Vector3Formatter;

namespace Shared.Helpers
{
    public static class MessagePackResolverConfig
    {
        public static readonly IFormatterResolver Resolver;

        static MessagePackResolverConfig()
        {
            var customFormatters = new IMessagePackFormatter[]
            {
                new Vector3Formatter(),
                new QuaternionFormatter(),
                new PlayerFormatter()
            };

            var resolvers = new IFormatterResolver[]
            {
                StandardResolverAllowPrivate.Instance
            };

            Resolver = CompositeResolver.Create(customFormatters, resolvers);
        }
    }
}