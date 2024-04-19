using System;
using System.Buffers.Binary;
using System.Runtime.CompilerServices;
using System.Security.Cryptography;

namespace UUIDNext.Generator
{
    [SkipLocalsInit]
    internal class Uuidv7SkipInitGenerator
    {
        private const int _sequenceMaxValue = (1 << 12) - 1;

        private long _lastUsedTimestamp = 0;
        private long _timestampOffset = 0;
        private ushort _monotonicSequence = 0;

        internal Guid New() => New(DateTime.UtcNow);

        [SkipLocalsInit]
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

            Span<byte> bytes = stackalloc byte[16];

            TimeSpan unixTimeStamp = date - DateTime.UnixEpoch;
            long timestampInMs = Convert.ToInt64(Math.Floor(unixTimeStamp.TotalMilliseconds));

            SetSequence(bytes[6..8], ref timestampInMs);
            SetTimestamp(bytes[0..6], timestampInMs);
            RandomNumberGenerator.Fill(bytes[8..16]);

            return CreateGuidFromBigEndianBytes(bytes);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        [SkipLocalsInit]
        private void SetTimestamp(Span<byte> bytes, long timestampInMs)
        {
            Span<byte> timestampInMillisecondsBytes = stackalloc byte[8];
            BinaryPrimitives.TryWriteInt64BigEndian(timestampInMillisecondsBytes, timestampInMs);
            timestampInMillisecondsBytes[2..8].CopyTo(bytes);
        }

        [SkipLocalsInit]
        private void SetSequence(Span<byte> bytes, ref long timestamp)
        {
            ushort sequence;
            long originalTimestamp = timestamp;

            lock (this)
            {
                sequence = GetSequenceNumber(ref timestamp);
                if (sequence > _sequenceMaxValue)
                {
                    // if the sequence is greater than the max value, we take advantage
                    // of the anti-rewind mechanism to simulate a slight change in clock time
                    timestamp = originalTimestamp + 1;
                    sequence = GetSequenceNumber(ref timestamp);
                }
            }

            BinaryPrimitives.TryWriteUInt16BigEndian(bytes, sequence);
        }

        private ushort GetSequenceNumber(ref long timestamp)
        {
            EnsureTimestampNeverMoveBackward(ref timestamp);

            if (timestamp == _lastUsedTimestamp)
            {
                _monotonicSequence += 1;
            }
            else
            {
                _lastUsedTimestamp = timestamp;
                _monotonicSequence = GetSequenceSeed();
            }

            return _monotonicSequence;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        [SkipLocalsInit]
        private void EnsureTimestampNeverMoveBackward(ref long timestamp)
        {
            long offsetTimestamp = timestamp + _timestampOffset;

            if (offsetTimestamp < _lastUsedTimestamp)
            {
                // if the computer clock has moved backward since the last generated UUID,
                // we add an offset to ensure the timestamp always move forward (See RFC Section 6.2)
                _timestampOffset = _lastUsedTimestamp - timestamp;
                timestamp = _lastUsedTimestamp;
            }
            else if (_timestampOffset > 0 && timestamp > _lastUsedTimestamp)
            {
                // reset the offset to reduce the drift with the actual time when possible
                _timestampOffset = 0;
            }
            else
            {
                timestamp = offsetTimestamp;
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        [SkipLocalsInit]
        private ushort GetSequenceSeed()
        {
            // following section 6.2 on "Fixed-Length Dedicated Counter Seeding", the initial value of the sequence is randomized
            Span<byte> buffer = stackalloc byte[2];
            RandomNumberGenerator.Fill(buffer);
            // Setting the highest bit to 0 mitigate the risk of a sequence overflow (see section 6.2)
            buffer[0] &= 0b0000_0111;
            return BinaryPrimitives.ReadUInt16BigEndian(buffer);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private Guid CreateGuidFromBigEndianBytes(Span<byte> bigEndianBytes)
        {
            SetVersion(bigEndianBytes);
            SetVariant(bigEndianBytes);
            return GuidHelper.FromBytes(bigEndianBytes, bigEndian: true);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void SetVersion(Span<byte> bigEndianBytes)
        {
            const int versionByte = 6;
            //Erase upper 4 bits
            bigEndianBytes[versionByte] &= 0b0000_1111;
            //Set 4 upper bits to version
            bigEndianBytes[versionByte] |= (byte)(7 << 4);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void SetVariant(Span<byte> bigEndianBytes)
        {
            const int variantByte = 8;
            //Erase upper 2 bits
            bigEndianBytes[variantByte] &= 0b0011_1111;
            //Set 2 upper bits to variant
            bigEndianBytes[variantByte] |= 0b1000_0000;
        }
    }
}
