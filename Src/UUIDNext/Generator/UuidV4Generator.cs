using UUIDNext.Tools;

namespace UUIDNext.Generator;

/// <summary>
/// Generate a UUID version 4 based on RFC 9562
/// </summary>
internal class UuidV4Generator
{
    public Guid New()
    {
        Span<byte> bytes = stackalloc byte[16];
        RandomNumberGeneratorPolyfill.Fill(bytes);
        return UuidToolkit.CreateGuidFromBigEndianBytes(bytes, 4);
    }
}
