using System;
using System.Buffers.Binary;
using UUIDNext.Tools;

namespace UUIDNext.Generator
{
    /// <summary>
    /// Generate a UUID version 7 based on RFC 9562
    /// </summary>
    public class UuidV7Generator : UuidGeneratorBase
    {
        protected override byte Version => 7;

        private readonly MonotonicityHandler _monotonicityHandler = new(sequenceBitSize: 12);

        internal Guid New() => New(DateTime.UtcNow);

        private Guid New(DateTime date)
        {
            /* We implement the first example given in section 4.4.4.1 of the RFC
              0                   1                   2                   3
              0 1 2 3 4 5 6 7 8 9 0 1 2 3 4 5 6 7 8 9 0 1 2 3 4 5 6 7 8 9 0 1
             +-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+
             |                           unix_ts_ms                          |
             +-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+
             |          unix_ts_ms           |  ver  |       rand_a          |
             +-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+
             |var|                        rand_b                             |
             +-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+
             |                            rand_b                             |
             +-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+
             */

            // Extra 2 bytes in front to prepend timestamp data.
            Span<byte> buffer = stackalloc byte[18];
            Span<byte> timestampBytes = buffer.Slice(0, 8);
            Span<byte> uuidBytes = buffer.Slice(2);

            var (timestamp, sequence) = _monotonicityHandler.GetTimestampAndSequence(date);

            BinaryPrimitives.TryWriteInt64BigEndian(timestampBytes, timestamp);
            BinaryPrimitives.TryWriteUInt16BigEndian(uuidBytes.Slice(6, 2), sequence);

            RandomNumberGeneratorPolyfill.Fill(uuidBytes.Slice(8, 8));

            return CreateGuidFromBigEndianBytes(uuidBytes);
        }

        [Obsolete("Use UuidDecoder.DecodeUuidV7 instead. This function will be removed in the next version")]
        public static (long timestampMs, short sequence) Decode(Guid guid) => UuidDecoder.DecodeUuidV7(guid);
    }
}
