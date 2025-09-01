using System;
using System.Collections.Generic;
using UUIDNext.Tools;

namespace UUIDNext.Test;

internal class UuidWithTimestampComparer : IComparer<Guid>
{
    public int Compare(Guid x, Guid y)
    {
        if (!UuidDecoder.TryDecodeTimestamp(x, out var dateX) || !UuidDecoder.TryDecodeSequence(x, out var sequenceX))
            throw new ArgumentException($"Argument {nameof(x)} is not a timestamped GUID", nameof(x));

        if (!UuidDecoder.TryDecodeTimestamp(y, out var dateY) || !UuidDecoder.TryDecodeSequence(y, out var sequenceY))
            throw new ArgumentException($"Argument {nameof(y)} is not a timestamped GUID", nameof(y));

        if (dateX < dateY) return -1;
        if (dateX > dateY) return 1;
        if (sequenceX < sequenceY) return -1;
        if (sequenceX > sequenceY) return 1;

        return 0;
    }
}
