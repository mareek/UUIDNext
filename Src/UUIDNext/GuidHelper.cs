namespace UUIDNext;

public static class GuidHelper
{
    public static Guid FromBytes(Span<byte> bytes, bool bigEndian)
    {
        if (!bigEndian)
#if NET472_OR_GREATER
            return new(bytes.ToArray());
#else
            return new(bytes);
#endif

        Span<byte> localBytes = stackalloc byte[bytes.Length];

        bytes.CopyTo(localBytes);
        SwitchByteOrder(localBytes);
#if NET472_OR_GREATER
        return new(localBytes.ToArray());
#else
        return new(localBytes);
#endif
    }

    public static byte[] ToByteArray(this Guid guid, bool bigEndian)
    {
        if (!bigEndian)
            return guid.ToByteArray();

        byte[] result = new byte[16];
        guid.TryWriteBytes(result, true, out var _);
        return result;
    }

    public static bool TryWriteBytes(this Guid guid, Span<byte> bytes, bool bigEndian, out int bytesWritten)
    {
#if NET472_OR_GREATER
        if (bytes.Length < 16)
        {
            bytesWritten = 0;
            return false;
        }

        var tempBytes = guid.ToByteArray();
        tempBytes.CopyTo(bytes);
#else
        if (bytes.Length < 16 || !guid.TryWriteBytes(bytes))
        {
            bytesWritten = 0;
            return false;
        }
#endif

        if (bigEndian)
            SwitchByteOrder(bytes);

        bytesWritten = 16;
        return true;
    }

    private static void SwitchByteOrder(Span<byte> bytes)
    {
        Permut(bytes, 0, 3);
        Permut(bytes, 1, 2);

        Permut(bytes, 5, 4);

        Permut(bytes, 6, 7);

        static void Permut(Span<byte> array, int indexSource, int indexDest)
        {
            (array[indexSource], array[indexDest]) = (array[indexDest], array[indexSource]);
        }
    }
}
