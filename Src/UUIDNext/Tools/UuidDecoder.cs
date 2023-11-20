using System;
using System.Buffers.Binary;

namespace UUIDNext.Tools
{
    public static class UuidDecoder
    {
        public static (long timestampMs, short sequence) DecodeUuidV7(Guid guid)
        {
            Span<byte> bytes = stackalloc byte[16];
            guid.TryWriteBytes(bytes, bigEndian: true, out var _);

            Span<byte> timestampBytes = stackalloc byte[8];
            bytes[0..6].CopyTo(timestampBytes[2..8]);
            long timestampMs = BinaryPrimitives.ReadInt64BigEndian(timestampBytes);

            var sequenceBytes = bytes[6..8];
            //remove version information
            sequenceBytes[0] &= 0b0000_1111;
            short sequence = BinaryPrimitives.ReadInt16BigEndian(sequenceBytes);

            return (timestampMs, sequence);
        }

        public static (long timestampMs, short sequence) DecodeUuidV8ForSqlServer(Guid guid)
        {
            Span<byte> bytes = stackalloc byte[16];
            guid.TryWriteBytes(bytes, bigEndian: true, out var _);

            Span<byte> timestampBytes = stackalloc byte[8];
            bytes[10..16].CopyTo(timestampBytes[2..8]);
            long timestampMs = BinaryPrimitives.ReadInt64BigEndian(timestampBytes);

            var sequenceBytes = bytes[8..10];
            //remove variant information
            sequenceBytes[0] &= 0b0011_1111;
            short sequence = BinaryPrimitives.ReadInt16BigEndian(sequenceBytes);

            return (timestampMs, sequence);
        }
    }
}
