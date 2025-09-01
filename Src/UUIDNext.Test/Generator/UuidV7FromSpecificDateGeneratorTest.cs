using System;
using System.Collections.Generic;
using System.Linq;
using NFluent;
using UUIDNext.Generator;
using Xunit;

namespace UUIDNext.Test.Generator;

public class UuidV7FromSpecificDateGeneratorTest : UuidFromSpecificDateGeneratorTestBase
{
    protected override int SequenceBitSize => 12;

    protected override byte Version => 7;

    protected override object NewGenerator() => new UuidV7FromSpecificDateGenerator();

    protected override Guid NewUuid(object generator)
        => ((UuidV7FromSpecificDateGenerator)generator).New(DateTimeOffset.Now);

    protected override Guid NewUuid(object generator, DateTimeOffset date)
        => ((UuidV7FromSpecificDateGenerator)generator).New(date);

    [Fact]
    public void TestSequenceSeed()
    {
        const int runCount = 1000;
        var date = DateTimeOffset.UtcNow;

        HashSet<short> generatedSequences = [];
        for (int i = 0; i < runCount; i++)
        {
            UuidV7FromSpecificDateGenerator generator = new();
            var guid = generator.New(date);

            //check that sequence seed leaves room for the next genertions
            var sequence = UuidTestHelper.DecodeSequence(guid);
            Check.That(sequence).IsLessOrEqualThan(0b_0111_1111_1111);

            generatedSequences.Add(sequence);
        }

        //check that sequence seed is randomized
        Check.That(generatedSequences.Count).IsStrictlyGreaterThan(runCount / 2);

        //check that only the highest bit of the sequence seed is always 0
        Check.That(generatedSequences.Any(s => s > 0b_0100_0000_0000)).IsTrue();
    }
}
