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

            if (!TrySetSequence(bytes[6..8], unixTimeStamp))
            {
                newUuid = Guid.Empty;
                return false;
            }

            SetTimestamp(bytes[0..6], unixTimeStamp);
            _rng.GetBytes(bytes[8..16]);

            newUuid = CreateGuidFromBigEndianBytes(bytes);
            return true;
        }

        private bool TrySetSequence(Span<byte> bytes, TimeSpan unixTimeStamp)
        {
            long timestampInMs = Convert.ToInt64(Math.Floor(unixTimeStamp.TotalMilliseconds));
            if (!TryGetSequenceNumber(timestampInMs, out int sequence))
            {
                return false;
            }

            BinaryPrimitives.TryWriteUInt16BigEndian(bytes, (ushort)sequence);
            return true;
        }

        protected override int GetSequenceSeed()
        {
            // following section 6.2 on "Fixed-Length Dedicated Counter Seeding", the initial value of the sequence is randomized
            Span<byte> buffer = stackalloc byte[4];
            _rng.GetBytes(buffer[2..4]);
            int randomSeed = BinaryPrimitives.ReadInt32BigEndian(buffer);

            //Setting the highest bit to 0 mitigate the risk of a sequence overflow (see section 6.2)
            return randomSeed & 0b111_1111_1111;
        }

        private void SetTimestamp(Span<byte> bytes, TimeSpan unixTimeStamp)
        {
            long timestampInMilliseconds = Convert.ToInt64(Math.Floor(unixTimeStamp.TotalMilliseconds));
            Span<byte> timestampInMillisecondsBytes = stackalloc byte[8];
            BinaryPrimitives.TryWriteInt64BigEndian(timestampInMillisecondsBytes, timestampInMilliseconds);
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
