using System.Runtime.CompilerServices;
using System.Security.Cryptography;

namespace UUIDNext.Tools;

internal static class RandomNumberGeneratorPolyfill
{
#if NETSTANDARD2_0
    private static readonly ThreadLocal<RandomNumberGenerator> _rng = new(RandomNumberGenerator.Create);

    public static void Fill(Span<byte> span)
    {
        var tempBytes = new byte[span.Length];
        _rng.Value.GetBytes(tempBytes);
        tempBytes.CopyTo(span);
    }
#else
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void Fill(Span<byte> span) => RandomNumberGenerator.Fill(span);
#endif
}