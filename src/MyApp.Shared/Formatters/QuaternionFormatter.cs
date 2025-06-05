using System;
using System.Numerics;

using MessagePack;
using MessagePack.Formatters;

namespace Shared.Formatters
{
    public class QuaternionFormatter : IMessagePackFormatter<Quaternion>
    {
        public void Serialize(ref MessagePackWriter writer, Quaternion value, MessagePackSerializerOptions options)
        {
            writer.WriteArrayHeader(4);
            writer.Write(value.X);
            writer.Write(value.Y);
            writer.Write(value.Z);
            writer.Write(value.W);
        }

        public Quaternion Deserialize(ref MessagePackReader reader, MessagePackSerializerOptions options)
        {
            if (reader.IsNil)
            {
                throw new InvalidOperationException("Invalid Quaternion data: Unexpected nil value");
            }

            var count = reader.ReadArrayHeader();
            if (count != 4)
            {
                throw new InvalidOperationException($"Invalid Quaternion data: Expected 4 elements, got {count}");
            }

            var x = reader.ReadSingle();
            var y = reader.ReadSingle();
            var z = reader.ReadSingle();
            var w = reader.ReadSingle();

            return new Quaternion(x, y, z, w);
        }
    }
}