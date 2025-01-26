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
    private static readonly ThreadLocal<PrefetchedRandomNumberGenerator> _prefetchRng = new(() => new(512));

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void Fill(Span<byte> span) => _prefetchRng.Value.Fill(span);
#endif
}