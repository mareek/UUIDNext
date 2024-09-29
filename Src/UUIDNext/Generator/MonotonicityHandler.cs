﻿using System.Buffers.Binary;
using UUIDNext.Tools;

namespace UUIDNext.Generator;

/// <summary>
/// This class implement the "Fixed Bit-Length Dedicated Counter" described in the section 6.2 of the RFC
/// </summary>
internal class MonotonicityHandler(int sequenceBitSize)
{
    private readonly int _sequenceMaxValue = (1 << sequenceBitSize) - 1;

    private long _lastUsedTimestamp = 0;
    private long _timestampOffset = 0;
    private ushort _monotonicSequence = 0;

    public (long timestamp, ushort sequence) GetTimestampAndSequence(DateTimeOffset date)
    {
        ushort sequence;

        long timestamp = date.ToUnixTimeMilliseconds();
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

        return (timestamp, sequence);
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

    private ushort GetSequenceSeed()
    {
        // following section 6.2 on "Fixed-Length Dedicated Counter Seeding", the initial value of the sequence is randomized
        Span<byte> buffer = stackalloc byte[2];
        RandomNumberGeneratorPolyfill.Fill(buffer);
        // Setting the highest bit to 0 mitigate the risk of a sequence overflow (see section 6.2)
        var seedHighByteMask = (byte)((1 << (sequenceBitSize - 8 - 1)) - 1);
        buffer[0] &= seedHighByteMask;
        return BinaryPrimitives.ReadUInt16BigEndian(buffer);
    }
}
