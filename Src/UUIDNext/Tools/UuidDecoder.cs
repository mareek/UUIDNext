using System.Buffers.Binary;

namespace UUIDNext.Tools;

/// <summary>
/// Provite a set of static methods for decoding UUIDs
/// </summary>
public static class UuidDecoder
{
    private static readonly DateTime GregorianCalendarStart = new(1582, 10, 15, 0, 0, 0, DateTimeKind.Utc);
    private static readonly DateTime Epoch = new(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc);

    /// <summary>
    /// Returns the version of the UUID
    /// </summary>
    public static int GetVersion(Guid guid)
    {
        Span<byte> bytes = stackalloc byte[16];
        guid.TryWriteBytes(bytes, bigEndian: true, out _);
        return GetVersion(bytes);
    }

    /// <summary>
    /// Try to retrieve the date part of a UUID v1, v6, v7 or V8 (if the UUIDv8 is a sequential UUID for SQL Server)
    /// </summary>
    public static bool TryDecodeTimestamp(Guid guid, out DateTime date)
    {
        Span<byte> bytes = stackalloc byte[16];
        guid.TryWriteBytes(bytes, bigEndian: true, out _);

        long timestamp;

        var version = GetVersion(bytes);
        switch (version)
        {
            case 1:
                // UUID v6 and v1 use a timestamp based on the start of the gregorian calendar (see RFC 9562 - Section 5.1)
                timestamp = GetUuidV1TimestampInTicksFromGegorianCalendar(bytes);
                date = GregorianCalendarStart.AddTicks(timestamp);
                return true;
            case 6:
                // UUID v6 and v1 use a timestamp based on the start of the gregorian calendar (see RFC 9562 - Section 5.1)
                timestamp = GetUuidV6TimestampInTicksFromGegorianCalendar(bytes);
                date = GregorianCalendarStart.AddTicks(timestamp);
                return true;
            case 7:
                timestamp = ReadUnixTimestamp(bytes, 0);
                date = Epoch.AddMilliseconds(timestamp);
                return true;
            case 8:
                timestamp = ReadUnixTimestamp(bytes, 10);
                date = Epoch.AddMilliseconds(timestamp);
                return IsReasonableV8Timestamp(timestamp);
            default:
                date = default;
                return false;
        }
    }

    /// <summary>
    /// Try to retrieve the sequence part of a UUID v1, v6, v7 or V8 (if the UUIDv8 is a sequential UUID for SQL Server)
    /// </summary>
    public static bool TryDecodeSequence(Guid guid, out short sequence)
    {
        Span<byte> bytes = stackalloc byte[16];
        guid.TryWriteBytes(bytes, bigEndian: true, out _);

        var version = GetVersion(bytes);

        if (version is not (1 or 6 or 7 or 8))
        {
            sequence = default;
            return false;
        }
        else if (version == 8 && !IsReasonableV8Timestamp(ReadUnixTimestamp(bytes, 10)))
        {
            sequence = default;
            return false;
        }

        int sequenceStart = version switch { 7 => 6, _ => 8 };
        int bitsToClear = version switch { 7 => 4, _ => 2 };

        var sequenceBytes = bytes.Slice(sequenceStart, 2);
        sequenceBytes[0] &= (byte)(255 >> bitsToClear);
        sequence = BinaryPrimitives.ReadInt16BigEndian(sequenceBytes);
        return true;
    }

    private static int GetVersion(Span<byte> uuidBytes) => uuidBytes[6] >> 4;

    private static long ReadUnixTimestamp(Span<byte> bytes, int timestampStart)
    {
        Span<byte> timestampBytes = stackalloc byte[8];
        bytes.Slice(timestampStart, 6).CopyTo(timestampBytes.Slice(2, 6));
        return BinaryPrimitives.ReadInt64BigEndian(timestampBytes);
    }

    private static long GetUuidV1TimestampInTicksFromGegorianCalendar(Span<byte> uuidBytes)
    {
        /* layout of a UUID version 1 (see RFC 9562, section 5.1)
          0                   1                   2                   3
          0 1 2 3 4 5 6 7 8 9 0 1 2 3 4 5 6 7 8 9 0 1 2 3 4 5 6 7 8 9 0 1
         +-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+
         |                           time_low                            |
         +-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+
         |           time_mid            |  ver  |       time_high       |
         +-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+
         |var|         clock_seq         |             node              |
         +-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+
         |                              node                             |
         +-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+
         */
        Span<byte> timeLow = uuidBytes.Slice(0, 4);
        Span<byte> timeMid = uuidBytes.Slice(4, 2);
        Span<byte> timeHigh = uuidBytes.Slice(6, 2);

        //remove version information
        timeHigh[0] &= 0b0000_1111;

        Span<byte> timestampBytes = stackalloc byte[8];
        timeHigh.CopyTo(timestampBytes.Slice(0, 2));
        timeMid.CopyTo(timestampBytes.Slice(2, 2));
        timeLow.CopyTo(timestampBytes.Slice(4, 4));
        return BinaryPrimitives.ReadInt64BigEndian(timestampBytes);
    }

    private static long GetUuidV6TimestampInTicksFromGegorianCalendar(Span<byte> uuidBytes)
    {
        /* layout of a UUID version 6 (see RFC 9562, section 5.6)
          0                   1                   2                   3
          0 1 2 3 4 5 6 7 8 9 0 1 2 3 4 5 6 7 8 9 0 1 2 3 4 5 6 7 8 9 0 1
         +-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+
         |                           time_high                           |
         +-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+
         |           time_mid            |  ver  |       time_low        |
         +-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+
         |var|         clock_seq         |             node              |
         +-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+
         |                              node                             |
         +-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+
         */
        Span<byte> timeHigh = uuidBytes.Slice(0, 4);
        Span<byte> timeMid = uuidBytes.Slice(4, 2);
        Span<byte> timeLow = uuidBytes.Slice(6, 2);

        //remove version information
        timeLow[0] &= 0b0000_1111;

        Span<byte> timestampBytes = stackalloc byte[8];
        timeHigh.CopyTo(timestampBytes.Slice(0, 4));
        var timeHighMs = BinaryPrimitives.ReadInt64BigEndian(timestampBytes) >> 4;

        timestampBytes.Clear();
        timeMid.CopyTo(timestampBytes.Slice(4, 2));
        var timeMidMs = BinaryPrimitives.ReadInt64BigEndian(timestampBytes) >> 4;

        timestampBytes.Clear();
        timeLow.CopyTo(timestampBytes.Slice(6, 2));
        var timeLowMs = BinaryPrimitives.ReadInt64BigEndian(timestampBytes);

        return timeHighMs + timeMidMs + timeLowMs;
    }

    // UUIDs generated by UuidV8SqlServerGenerator use the current date for the timestamp 
    // so we check if the decoded timestamp correspond to a reasonable date.
    // if not, that means that it's not a UUID generated by UuidV8SqlServerGenerator
    // If you're the person debugging that code in the year 2100, hello from the past
    // and sorry for that, I didn't think that my code would live so long
    // If the person debugging that code is ME, congratulation for living so long ! You're about to beat Jeanne Calment !
    private static bool IsReasonableV8Timestamp(long timestamp)
    {
        const long JanuaryFirst2020TimeStamp = 1577836800000;
        const long JanuaryFirst2100TimeStamp = 4102444800000;

        return JanuaryFirst2020TimeStamp < timestamp && timestamp < JanuaryFirst2100TimeStamp;
    }
}
