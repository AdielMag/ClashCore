using MessagePack;
using MessagePack.Formatters;
using System;
using System.Numerics;
using Shared.Data;

namespace Shared.Formatters
{
    public class PlayerFormatter : IMessagePackFormatter<TransformData>
    {
        public void Serialize(ref MessagePackWriter writer, TransformData value, MessagePackSerializerOptions options)
        {
            if (value == null)
            {
                writer.WriteNil();
                return;
            }

            writer.WriteArrayHeader(3);

            // Serialize Name
            writer.Write(value.Id);

            // Serialize Position (Vector3)
            writer.WriteArrayHeader(3);
            writer.Write(value.Position.X);
            writer.Write(value.Position.Y);
            writer.Write(value.Position.Z);

            // Serialize Rotation (Quaternion)
            writer.WriteArrayHeader(4);
            writer.Write(value.Rotation.X);
            writer.Write(value.Rotation.Y);
            writer.Write(value.Rotation.Z);
            writer.Write(value.Rotation.W);
        }

        public TransformData Deserialize(ref MessagePackReader reader, MessagePackSerializerOptions options)
        {
            if (reader.TryReadNil())
            {
                return null;
            }

            var count = reader.ReadArrayHeader();
            if (count != 3)
            {
                throw new InvalidOperationException($"Invalid Player data: Expected 3 elements, got {count}");
            }

            var player = new TransformData();

            // Deserialize Name
            player.Id = reader.ReadString();

            // Deserialize Position (Vector3)
            var positionCount = reader.ReadArrayHeader();
            if (positionCount != 3)
            {
                throw new InvalidOperationException($"Invalid Vector3 data: Expected 3 elements, got {positionCount}");
            }
            var x = reader.ReadSingle();
            var y = reader.ReadSingle();
            var z = reader.ReadSingle();
            player.Position = new Vector3(x, y, z);

            // Deserialize Rotation (Quaternion)
            var rotationCount = reader.ReadArrayHeader();
            if (rotationCount != 4)
            {
                throw new InvalidOperationException($"Invalid Quaternion data: Expected 4 elements, got {rotationCount}");
            }
            var qx = reader.ReadSingle();
            var qy = reader.ReadSingle();
            var qz = reader.ReadSingle();
            var qw = reader.ReadSingle();
            player.Rotation = new Quaternion(qx, qy, qz, qw);

            return player;
        }
    }
}