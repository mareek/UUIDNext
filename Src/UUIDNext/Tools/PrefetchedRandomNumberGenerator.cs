#if !NETSTANDARD2_0
using System.Security.Cryptography;

namespace UUIDNext.Tools;

/// <summary>
/// Call to .NET's RandomNumberGenerator has a significant overhead when filling small spans/arrays of a few bytes
/// The goal of this class is to generate random numbers in larger batch to reduce the number of calls to RandomNumberGenerator
/// </summary>
/// <param name="batchSize"></param>
internal class PrefetchedRandomNumberGenerator(int batchSize)
{
    private readonly RandomNumberGenerator _rng = RandomNumberGenerator.Create();
    private readonly byte[] _randomBatch = new byte[batchSize];
    private int index = batchSize;

    public void Fill(Span<byte> buffer)
    {
        // The buffer is larger than our batch size so there's no point in trying to be clever: we just call the RNG directly
        if (_randomBatch.Length < buffer.Length)
        {
            _rng.GetBytes(buffer);
            return;
        }

        // There's not enough data left in our latest batch to fill the buffer so we request a new batch of random numbers
        if (_randomBatch.Length < (buffer.Length + index))
        {
            _rng.GetBytes(_randomBatch);
            index = 0;
        }

        _randomBatch.AsSpan(index, buffer.Length).CopyTo(buffer);
        index += buffer.Length;
    }
}
#endif
