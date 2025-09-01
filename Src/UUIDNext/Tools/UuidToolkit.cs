using System.Buffers.Binary;
using System.Security.Cryptography;
using System.Text;
using UUIDNext.Generator;

namespace UUIDNext.Tools;

/// <summary>
/// Provite a set of static methods for generating custom UUIDs
/// </summary>
public static class UuidToolkit
{
    // UuidV7FromSpecificDateGenerator has a footprint of ~50KB so we decalre it as Lazy so that it
    // only impacts the consumers of the feature
    private static readonly Lazy<UuidV7FromSpecificDateGenerator> _lazyV7Generator = new(() => new());
    private static UuidV7FromSpecificDateGenerator V7Generator => _lazyV7Generator.Value;

    /// <summary>
    /// Create new UUID version 8 with the provided bytes with the variant and version bits set
    /// </summary>
    /// <param name="bigEndianBytes">the bytes that will populate the UUID in big endian order</param>
    /// <returns>a UUID version 8</returns>
    /// <remarks>
    /// Here is the bit layout of the UUID Version 8 created :
    ///  0                   1                   2                   3
    ///  0 1 2 3 4 5 6 7 8 9 0 1 2 3 4 5 6 7 8 9 0 1 2 3 4 5 6 7 8 9 0 1
    /// +-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+
    /// |                        bigEndianBytes                         |
    /// +-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+
    /// |       bigEndianBytes          |  ver  |    bigEndianBytes     |
    /// +-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+
    /// |var|                    bigEndianBytes                         |
    /// +-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+
    /// |                        bigEndianBytes                         |
    /// +-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+     
    /// </remarks>
    public static Guid CreateGuidFromBigEndianBytes(Span<byte> bigEndianBytes)
        => CreateGuidFromBigEndianBytes(bigEndianBytes, 8);

    /// <summary>
    /// This Function is kept internal so that UUIDNext can only be used to produce RFC Compliant UUIDs
    /// </summary>
    internal static Guid CreateGuidFromBigEndianBytes(Span<byte> bigEndianBytes, byte version)
    {
        SetVersion(bigEndianBytes, version);
        SetVariant(bigEndianBytes);
#if NET8_0_OR_GREATER
        return new Guid(bigEndianBytes, bigEndian: true);
#else
        return GuidHelper.FromBytes(bigEndianBytes, bigEndian: true);
#endif
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

    /// <summary>
    /// Create a new name based UUID version 8 according to section 6.5 of the RFC
    /// </summary>
    /// <param name="namespaceId">the namespace where the name belongs</param>
    /// <param name="name">the name to be hashed</param>
    /// <param name="hashAlgorithm">the hash algorithm used to compute the UUID (MD5, SHA-256, etc.)</param>
    /// <returns>A UUID version 8</returns>
    public static Guid CreateUuidFromName(Guid namespaceId, string name, HashAlgorithm hashAlgorithm)
        => CreateUuidFromName(namespaceId, name, hashAlgorithm, 8);

    /// <summary>
    /// This Function is kept internal so that UUIDNext can only be used to produce RFC Compliant UUIDs
    /// </summary>
    internal static Guid CreateUuidFromName(Guid namespaceId, string name, HashAlgorithm hashAlgorithm, byte version)
    {
        //Convert the name to a canonical sequence of octets (as defined by the standards or conventions of its name space);
        var normalizedName = name.Normalize(NormalizationForm.FormC);
        var utf8NameByteCount = Encoding.UTF8.GetByteCount(normalizedName);
#if NETSTANDARD2_0
        byte[] utf8NameBytes = new byte[utf8NameByteCount];
        Encoding.UTF8.GetBytes(normalizedName, 0, normalizedName.Length, utf8NameBytes, 0);

        //put the name space ID in network byte order.
        Span<byte> namespaceBytes = stackalloc byte[16];
        namespaceId.TryWriteBytes(namespaceBytes, bigEndian: true, out _);

        //Compute the hash of the name space ID concatenated with the name.
        int bytesToHashCount = namespaceBytes.Length + utf8NameBytes.Length;
        byte[] bytesToHash = new byte[bytesToHashCount];
        namespaceBytes.CopyTo(bytesToHash);
        utf8NameBytes.CopyTo(bytesToHash, namespaceBytes.Length);

        var hash = hashAlgorithm.ComputeHash(bytesToHash);

        return CreateGuidFromBigEndianBytes(hash.AsSpan(0, 16), version);
#else
        Span<byte> utf8NameBytes = (utf8NameByteCount > 256) ? new byte[utf8NameByteCount] : stackalloc byte[utf8NameByteCount];
        Encoding.UTF8.GetBytes(normalizedName, utf8NameBytes);

        //put the name space ID in network byte order.
        Span<byte> namespaceBytes = stackalloc byte[16];
        namespaceId.TryWriteBytes(namespaceBytes, bigEndian: true, out _);

        //Compute the hash of the name space ID concatenated with the name.
        int bytesToHashCount = namespaceBytes.Length + utf8NameBytes.Length;
        Span<byte> bytesToHash = (utf8NameByteCount > 256) ? new byte[bytesToHashCount] : stackalloc byte[bytesToHashCount];
        namespaceBytes.CopyTo(bytesToHash);
        utf8NameBytes.CopyTo(bytesToHash[namespaceBytes.Length..]);

        Span<byte> hash = stackalloc byte[hashAlgorithm.HashSize / 8];
        hashAlgorithm.TryComputeHash(bytesToHash, hash, out _);

        return CreateGuidFromBigEndianBytes(hash[0..16], version);
#endif
    }

    /// <summary>
    /// Create a new UUID version 7 with the given timestamp and bytes of otherBytes, filling the rest with random data
    /// </summary>
    /// <remarks>
    /// Here is the bit layout of the UUID Version 7 created
    ///  0                   1                   2                   3
    ///  0 1 2 3 4 5 6 7 8 9 0 1 2 3 4 5 6 7 8 9 0 1 2 3 4 5 6 7 8 9 0 1
    /// +-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+
    /// |                           timestamp                           |
    /// +-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+
    /// |           timestamp           |  ver  |      otherBytes       |
    /// +-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+
    /// |var|                      otherBytes                           |
    /// +-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+
    /// |                          otherBytes                           |
    /// +-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+
    /// </remarks>
    /// <param name="timestamp">A unix epoch timestamp in ms</param>
    /// <param name="followingBytes">
    /// A series of 0 to 10 bytes used to fill the rest of the UUID. 
    /// Be careful: some bits will be overxritten by the version and variant of the UUID (see remarks)
    /// </param>
    /// <returns>A UUID Version 7</returns>
    /// <exception cref="ArgumentException"></exception>
    public static Guid CreateUuidV7(long timestamp, Span<byte> followingBytes)
    {
        if (followingBytes.Length > 10)
            throw new ArgumentException($"argument {nameof(followingBytes)} should have a size of 10 bytes or less", nameof(followingBytes));

        // Extra 2 bytes in front to prepend timestamp data.
        Span<byte> buffer = stackalloc byte[18];

        // write the timestamp
        Span<byte> timestampBytes = buffer.Slice(0, 8);
        BinaryPrimitives.TryWriteInt64BigEndian(timestampBytes, timestamp);

        // write the data provided by the caller
        Span<byte> followingUuidBytes = buffer.Slice(8, followingBytes.Length);
        followingBytes.CopyTo(followingUuidBytes);

        // fill the remaining bytes with random data
        Span<byte> randomBytes = buffer.Slice(8 + followingBytes.Length);
        RandomNumberGeneratorPolyfill.Fill(randomBytes);

        // cut the 2 highest bytes of the timestamp to keep only 48bits (see bit layout) and create the UUID
        Span<byte> uuidBytes = buffer.Slice(2);
        return CreateGuidFromBigEndianBytes(uuidBytes, 7);
    }

    /// <summary>
    /// Create a new UUID version 7 with the given date as timestamp 
    /// </summary>
    public static Guid CreateUuidV7FromSpecificDate(DateTimeOffset date)
    {
        // if the date argument is equal or close enough to DateTimeOffset.UtcNow we consider that 
        // the API consumer just wanted a UUID Version 7 without specifieng the date and called the
        // wrong method so we're forwarding the call to Uuid.NewSequential to keep consistency accross 
        // different calls

        if (IsCloseToNow(date))
            return Uuid.NewSequential();

        return V7Generator.New(date);
    }

    internal static Guid CreateSequentialUuidForSqlServer(long timestamp, Span<byte> followingBytes)
    {
        if (followingBytes.Length > 10)
            throw new ArgumentException($"argument {nameof(followingBytes)} should have a size of 10 bytes or less", nameof(followingBytes));

        Span<byte> buffer = stackalloc byte[16];

        // We only use 48 bits of the timestamp so we can write it on 64 bits and then
        // erase the 16 most significant bits with the sequence to save some buffer allocation and copy
        BinaryPrimitives.TryWriteInt64BigEndian(buffer.Slice(8, 8), timestamp);

        // write the data provided by the caller
        int randomOffset = 0;
        Span<byte> sequenceBytes = buffer.Slice(8, 2);
        if (followingBytes.Length == 0)
            RandomNumberGeneratorPolyfill.Fill(buffer.Slice(8, 2));
        if (followingBytes.Length == 1)
        {
            sequenceBytes[0] = followingBytes[0];
            RandomNumberGeneratorPolyfill.Fill(sequenceBytes.Slice(1));
        }
        else
        {
            randomOffset = followingBytes.Length - 2;

            //write the sequence
            followingBytes.Slice(0, 2).CopyTo(sequenceBytes);

            Span<byte> randBytes = buffer.Slice(0, randomOffset);
            followingBytes.Slice(2).CopyTo(randBytes);
        }

        RandomNumberGeneratorPolyfill.Fill(buffer.Slice(randomOffset, 8 - randomOffset));

        return CreateGuidFromBigEndianBytes(buffer, 8);
    }

    /// <summary>
    /// returns true if the date is close to DateTimeOffset.UtcNow, false otherwise
    /// </summary>
    private static bool IsCloseToNow(DateTimeOffset date)
    {
        const long tickThreshold = 10; // 1 µs
        var now = DateTimeOffset.UtcNow;

        if (date.ToUnixTimeMilliseconds() == now.ToUnixTimeMilliseconds())
            return true;

        if (Math.Abs(date.UtcTicks - now.UtcTicks) < tickThreshold)
            return true;

        return false;
    }
}
