using System;
using NFluent;
using UUIDNext.Generator;
using Xunit;

namespace UUIDNext.Test.Generator
{
    public class UuidV7GeneratorTest : UuidTimestampGeneratorBaseTest
    {
        private readonly UuidV7Generator _generator = new();

        protected override byte Version => 7;

        protected override Guid NewUuid() => _generator.New();

        [Fact]
        public void TestSequence()
        {
            UuidV7Generator generator = new();
            var date = DateTime.UtcNow.Date;

            var guido = generator.NewInternal(date);
            var (timestampO, timestampMsO, sequenceO) = generator.Decode(guido);

            Check.That(sequenceO).IsEqualTo(0);

            var guida = generator.NewInternal(date);
            var (timestampA, timestampMsA, sequenceA) = generator.Decode(guida);

            Check.That(timestampA).IsEqualTo(timestampO);
            Check.That(timestampMsA).IsEqualTo(timestampMsO);
            Check.That(sequenceA).IsEqualTo(sequenceO + 1);

            var guidu = generator.NewInternal(date.AddTicks(1));
            var (timestampU, timestampMsU, sequenceU) = generator.Decode(guidu);

            Check.That(timestampU).IsEqualTo(timestampO);
            Check.That(timestampMsU).IsEqualTo(timestampMsO);
            Check.That(sequenceU).IsEqualTo(sequenceA + 1);

            var guidi = generator.NewInternal(date.AddMilliseconds(1));
            var (timestampI, timestampMsI, sequenceI) = generator.Decode(guidi);

            Check.That(timestampI).IsEqualTo(timestampO);
            Check.That(timestampMsI).IsNotEqualTo(timestampMsO);
            Check.That(sequenceI).IsEqualTo(0);
        }
    }
}
