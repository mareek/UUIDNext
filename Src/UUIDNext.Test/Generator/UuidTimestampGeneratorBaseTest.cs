using System;
using System.Collections.Concurrent;
using System.Threading.Tasks;
using NFluent;
using Xunit;

namespace UUIDNext.Test.Generator;

public abstract class UuidTimestampGeneratorBaseTest
{
    protected abstract byte Version { get; }

    protected abstract int SequenceBitSize { get; }

    protected abstract Guid NewUuid(object generator);

    protected abstract Guid NewUuid(object generator, DateTimeOffset date);

    protected abstract object NewGenerator();

    protected int GetSequenceMaxValue() => (1 << SequenceBitSize) - 1;

    protected short GetSeedMaxValue() => (short)((GetSequenceMaxValue() + 1) / 2 - 1);

    [Fact]
    public void DumbTest()
    {
        var generator = NewGenerator();
        ConcurrentBag<Guid> generatedUuids = new();
        Parallel.For(0, 100, _ => generatedUuids.Add(NewUuid(generator)));

        Check.That(generatedUuids).ContainsNoDuplicateItem();

        foreach (var uuid in generatedUuids)
        {
            UuidTestHelper.CheckVersionAndVariant(uuid, Version);
        }
    }

    [Fact]

    public void CheckSequenceSeeding()
    {
        const int iterationCOunt = 1_000_000;

        short maxSequence = (short)GetSequenceMaxValue();

        var generator = NewGenerator();

        short maxGeneratedSequence = 0;
        DateTimeOffset baseDate = new(new(2020, 1, 2));

        for (int i = 0; i < iterationCOunt; i++)
        {
            var guid = NewUuid(generator, baseDate.AddMilliseconds(i));
            bool decodedSequence = UUIDNext.Tools.UuidDecoder.TryDecodeSequence(guid, out short sequence);
            Check.That(decodedSequence)
                 .IsTrue();

            Check.That(sequence).IsGreaterOrEqualThan(0);
            Check.That(sequence).IsLessOrEqualThan(maxSequence);

            maxGeneratedSequence = Math.Max(maxGeneratedSequence, sequence);
        }

        Check.That(maxGeneratedSequence).IsStrictlyGreaterThan((short)(1 << (SequenceBitSize - 2)));
    }
}