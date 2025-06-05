using System.Buffers;
using System.Reflection;

using Grpc.Core;

using MagicOnion.Serialization;

using MessagePack;

namespace Server
{
    public class CustomMagicOnionSerializerProvider : IMagicOnionSerializerProvider
    {
        private readonly MessagePackSerializerOptions _options;

        public CustomMagicOnionSerializerProvider(MessagePackSerializerOptions options)
        {
            _options = options;
        }

        public IMagicOnionSerializer Create(MethodType methodType, MethodInfo? methodInfo)
        {
            return new CustomMagicOnionSerializer(_options);
        }

        private class CustomMagicOnionSerializer : IMagicOnionSerializer
        {
            private readonly MessagePackSerializerOptions _options;

            public CustomMagicOnionSerializer(MessagePackSerializerOptions options)
            {
                _options = options;
            }

            public void Serialize<T>(IBufferWriter<byte> writer, in T value)
            {
                MessagePackSerializer.Serialize(writer, value, _options);
            }

            public T Deserialize<T>(in ReadOnlySequence<byte> bytes)
            {
                return MessagePackSerializer.Deserialize<T>(bytes, _options);
            }
        }
    }
}