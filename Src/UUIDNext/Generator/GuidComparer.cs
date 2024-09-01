namespace UUIDNext.Generator;

public class GuidComparer : IComparer<Guid>
{
    public int Compare(Guid x, Guid y)
    {
        Span<byte> xBytes = stackalloc byte[16];
        x.TryWriteBytes(xBytes, bigEndian: true, out var _);

        Span<byte> yBytes = stackalloc byte[16];
        y.TryWriteBytes(yBytes, bigEndian: true, out var _);

        for (int i = 0; i < 16; i++)
        {
            int compareResult = xBytes[i].CompareTo(yBytes[i]);
            if (compareResult != 0)
            {
                return compareResult;
            }
        }

        return 0;
    }
}
