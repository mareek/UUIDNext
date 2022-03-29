using System;
using System.Buffers.Binary;

namespace UUIDNext.Generator
{
    /// <summary>
    /// Generate a UUID version 7 based on RFC draft at https://github.com/uuid6/uuid6-ietf-draft/
    /// </summary>
    public class UuidV7Generator : UuidTimestampGeneratorBase
    {
        protected override byte Version => 7;

        protected override int SequenceBitSize => 12;

        protected override bool TryGenerateNew(DateTime date, out Guid newUuid)
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

            Span<byte> bytes = stackalloc byte[16];

            TimeSpan unixTimeStamp = date - DateTime.UnixEpoch;
            long timestampInMs = Convert.ToInt64(Math.Floor(unixTimeStamp.TotalMilliseconds));

            if (!TrySetSequence(bytes[6..8], ref timestampInMs))
            {
                newUuid = Guid.Empty;
                return false;
            }

            SetTimestamp(bytes[0..6], timestampInMs);
            _rng.GetBytes(bytes[8..16]);

            newUuid = CreateGuidFromBigEndianBytes(bytes);
            return true;
        }

        private bool TrySetSequence(Span<byte> bytes, ref long timestampInMs)
        {
            if (!TryGetSequenceNumber(ref timestampInMs, out ushort sequence))
            {
                return false;
            }

            BinaryPrimitives.TryWriteUInt16BigEndian(bytes, sequence);
            return true;
        }

        protected override ushort GetSequenceSeed()
        {
            // following section 6.2 on "Fixed-Length Dedicated Counter Seeding", the initial value of the sequence is randomized
            Span<byte> buffer = stackalloc byte[2];
            _rng.GetBytes(buffer);
            //Setting the highest bit to 0 mitigate the risk of a sequence overflow (see section 6.2)
            buffer[0] &= 0b0000_0111;
            return BinaryPrimitives.ReadUInt16BigEndian(buffer);
        }

        private void SetTimestamp(Span<byte> bytes, long timestampInMs)
        {
            Span<byte> timestampInMillisecondsBytes = stackalloc byte[8];
            BinaryPrimitives.TryWriteInt64BigEndian(timestampInMillisecondsBytes, timestampInMs);
            timestampInMillisecondsBytes[2..8].CopyTo(bytes);
        }

        public static (long timestampMs, short sequence) Decode(Guid guid)
        {
            Span<byte> bytes = stackalloc byte[16];
            GuidHelper.TryWriteBigEndianBytes(guid, bytes);

            Span<byte> timestampBytes = stackalloc byte[8];
            bytes[0..6].CopyTo(timestampBytes[2..8]);
            long timestampMs = BinaryPrimitives.ReadInt64BigEndian(timestampBytes);

            var sequenceBytes = bytes[6..8];
            //remove version information
            sequenceBytes[0] &= 0b0000_1111;
            short sequence = BinaryPrimitives.ReadInt16BigEndian(sequenceBytes);

            return (timestampMs, sequence);
        }
    }
}
