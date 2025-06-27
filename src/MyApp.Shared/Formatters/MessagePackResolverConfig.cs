using MessagePack;
using MessagePack.Resolvers;
using MessagePack.Unity;

namespace Shared.Helpersl
{
    public static class MessagePackResolverConfig
    {
        public static readonly IFormatterResolver Resolver;

        static MessagePackResolverConfig()
        {
            var resolvers = new []
            {
                StandardResolverAllowPrivate.Instance,
                UnityResolver.InstanceWithStandardResolver
            };

            Resolver = CompositeResolver.Create(resolvers);
        }
    }
}