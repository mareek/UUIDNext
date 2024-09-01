using System.Security.Cryptography;
using System.Text;

namespace UUIDNext.Tools;

internal static class UuidToolkit
{
    public static Guid CreateGuidFromBigEndianBytes(Span<byte> bigEndianBytes, byte version)
    {
        SetVersion(bigEndianBytes, version);
        SetVariant(bigEndianBytes);
        return GuidHelper.FromBytes(bigEndianBytes, bigEndian: true);
    }

    private static void SetVersion(Span<byte> bigEndianBytes, byte version)
    {
        const int versionByte = 6;
        //Erase upper 4 bits
        bigEndianBytes[versionByte] &= 0b0000_1111;
        //Set 4 upper bits to version
        bigEndianBytes[versionByte] |= (byte)(version << 4);
    }

    private static void SetVariant(Span<byte> bigEndianBytes)
    {
        const int variantByte = 8;
        //Erase upper 2 bits
        bigEndianBytes[variantByte] &= 0b0011_1111;
        //Set 2 upper bits to variant
        bigEndianBytes[variantByte] |= 0b1000_0000;
    }

    public static Guid CreateUuidFromName(Guid namespaceId, string name, HashAlgorithm hashAlgorithm, byte version)
    {
        //Convert the name to a canonical sequence of octets (as defined by the standards or conventions of its name space);
        var utf8NameByteCount = Encoding.UTF8.GetByteCount(name.Normalize(NormalizationForm.FormC));
#if NET472_OR_GREATER
        byte[] utf8NameBytes = new byte[utf8NameByteCount];
        Encoding.UTF8.GetBytes(name, 0, name.Length, utf8NameBytes, 0);

        //put the name space ID in network byte order.
        Span<byte> namespaceBytes = stackalloc byte[16];
        namespaceId.TryWriteBytes(namespaceBytes, bigEndian: true, out var _);

        //Compute the hash of the name space ID concatenated with the name.
        int bytesToHashCount = namespaceBytes.Length + utf8NameBytes.Length;
        byte[] bytesToHash = new byte[bytesToHashCount];
        namespaceBytes.CopyTo(bytesToHash);
        utf8NameBytes.CopyTo(bytesToHash, namespaceBytes.Length);

        var hash = hashAlgorithm.ComputeHash(bytesToHash);

        return CreateGuidFromBigEndianBytes(hash.AsSpan(0, 16), version);
#else
        Span<byte> utf8NameBytes = (utf8NameByteCount > 256) ? new byte[utf8NameByteCount] : stackalloc byte[utf8NameByteCount];
        Encoding.UTF8.GetBytes(name, utf8NameBytes);

        //put the name space ID in network byte order.
        Span<byte> namespaceBytes = stackalloc byte[16];
        namespaceId.TryWriteBytes(namespaceBytes, bigEndian: true, out var _);

        //Compute the hash of the name space ID concatenated with the name.
        int bytesToHashCount = namespaceBytes.Length + utf8NameBytes.Length;
        Span<byte> bytesToHash = (utf8NameByteCount > 256) ? new byte[bytesToHashCount] : stackalloc byte[bytesToHashCount];
        namespaceBytes.CopyTo(bytesToHash);
        utf8NameBytes.CopyTo(bytesToHash[namespaceBytes.Length..]);

        Span<byte> hash = stackalloc byte[hashAlgorithm.HashSize / 8];
        hashAlgorithm.TryComputeHash(bytesToHash, hash, out var _);

        return CreateGuidFromBigEndianBytes(hash[0..16], version);
#endif
    }
}
