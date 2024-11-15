namespace UUIDNext;

/// <summary>
/// Provite a set of static and extensions methods that brings .NET8+ features to .NET Standard 2.1 and .NET framework
/// </summary>
public static class GuidHelper
{
    /// <summary>
    /// Creates a new guid from a span of bytes.
    /// </summary>
    public static Guid FromBytes(Span<byte> bytes, bool bigEndian)
    {
        if (!bigEndian)
#if NETSTANDARD2_0
            return new(bytes.ToArray());
#else
            return new(bytes);
#endif

        Span<byte> localBytes = stackalloc byte[bytes.Length];

        bytes.CopyTo(localBytes);
        SwitchByteOrder(localBytes);
#if NETSTANDARD2_0
        return new(localBytes.ToArray());
#else
        return new(localBytes);
#endif
    }

#if !NET8_0_OR_GREATER
    /// <summary>
    /// Returns an unsigned byte array containing the GUID.
    /// </summary>
    public static byte[] ToByteArray(this Guid guid, bool bigEndian)
    {
        if (!bigEndian)
            return guid.ToByteArray();

        byte[] result = new byte[16];
        guid.TryWriteBytes(result, true, out var _);
        return result;
    }

    /// <summary>
    /// Returns whether bytes are successfully written to given span.
    /// </summary>
    public static bool TryWriteBytes(this Guid guid, Span<byte> bytes, bool bigEndian, out int bytesWritten)
    {
#if NETSTANDARD2_0
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
#endif

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
