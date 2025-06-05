using System;
using System.Numerics;

using MessagePack;
using MessagePack.Formatters;

namespace Shared.Formatters
{
    public class Vector3Formatter : IMessagePackFormatter<Vector3>
    {
        public void Serialize(ref MessagePackWriter writer, Vector3 value, MessagePackSerializerOptions options)
        {
            writer.WriteArrayHeader(3);
            writer.Write(value.X);
            writer.Write(value.Y);
            writer.Write(value.Z);
        }

        public Vector3 Deserialize(ref MessagePackReader reader, MessagePackSerializerOptions options)
        {
            if (reader.IsNil)
            {
                throw new InvalidOperationException("Invalid Vector3 data: Unexpected nil value");
            }

            var count = reader.ReadArrayHeader();
            if (count != 3)
            {
                throw new InvalidOperationException($"Invalid Vector3 data: Expected 3 elements, got {count}");
            }

            var x = reader.ReadSingle();
            var y = reader.ReadSingle();
            var z = reader.ReadSingle();

            return new Vector3(x, y, z);
        }
    }
}