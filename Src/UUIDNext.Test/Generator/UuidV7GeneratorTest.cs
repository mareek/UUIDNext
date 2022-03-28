using System;
using System.Collections.Concurrent;
using System.Linq;
using System.Threading.Tasks;
using NFluent;
using UUIDNext.Generator;
using Xunit;

namespace UUIDNext.Test.Generator
{
    public class UuidV7GeneratorTest : UuidTimestampGeneratorBaseTest
    {
        protected override byte Version => 7;

        protected override UuidTimestampGeneratorBase GetNewGenerator() => new UuidV7Generator();

        protected override int GetSequence(Guid uuid) => UuidV7Generator.Decode(uuid).sequence;

        [Fact]
        public void TestSequence()
        {
            UuidV7Generator generator = new();
            var date = DateTime.UtcNow.Date;

            Check.That(generator.TryGenerateNew(date, out var guido)).IsTrue();
            var (timestampO, timestampMsO, sequenceO) = UuidV7Generator.Decode(guido);

            //check that sequence initial value is randomized
            Check.That(sequenceO).IsStrictlyGreaterThan(0);
            Check.That(sequenceO).IsStrictlyLessThan(2048);

            Check.That(generator.TryGenerateNew(date, out var guida)).IsTrue();
            var (timestampA, timestampMsA, sequenceA) = UuidV7Generator.Decode(guida);

            Check.That(timestampA).IsEqualTo(timestampO);
            Check.That(timestampMsA).IsEqualTo(timestampMsO);
            Check.That(sequenceA).IsEqualTo(sequenceO + 1);

            Check.That(generator.TryGenerateNew(date.AddTicks(1), out var guidu)).IsTrue();
            var (timestampU, timestampMsU, sequenceU) = UuidV7Generator.Decode(guidu);

            Check.That(timestampU).IsEqualTo(timestampO);
            Check.That(timestampMsU).IsEqualTo(timestampMsO);
            Check.That(sequenceU).IsEqualTo(sequenceA + 1);

            Check.That(generator.TryGenerateNew(date.AddMilliseconds(1), out var guidi)).IsTrue();
            var (timestampI, timestampMsI, sequenceI) = UuidV7Generator.Decode(guidi);

            Check.That(timestampI).IsEqualTo(timestampO);
            Check.That(timestampMsI).IsNotEqualTo(timestampMsO);
            Check.That(sequenceI).IsNotEqualTo(sequenceA + 1);
            Check.That(sequenceI).IsStrictlyGreaterThan(0);
            Check.That(sequenceI).IsStrictlyLessThan(2048);
        }

        [Fact]
        public void TestSequenceMultiThread()
        {
            UuidV7Generator generator = new();
            var date = DateTime.UtcNow.Date;

            ConcurrentBag<Guid> generatedUuids = new();
            Parallel.For(0, 100, _ => generatedUuids.Add(generator.TryGenerateNew(date, out var guido) ? guido : Guid.Empty));

            var uuidsParts = generatedUuids.Select(u => UuidV7Generator.Decode(u)).ToArray();
            Check.That(uuidsParts.Select(x => x.sequence)).ContainsNoDuplicateItem();
            Check.That(uuidsParts.Select(x => x.timestamp).Distinct()).HasSize(1);
            Check.That(uuidsParts.Select(x => x.timestampMs).Distinct()).HasSize(1);
        }

        [Theory]
        [InlineData(0, 0, 0)]
        [InlineData(500, 0b0000_1000, 0)]
        [InlineData(937.5, 0b0000_1111, 0)]
        [InlineData(31.25, 0, 0b1000_0000)]
        [InlineData(0.48828125, 0, 0b10)]
        public void TestSubSecondEncoding(double msValue, int expectedMsUpperByte, int expectedMsLowerByte)
        {
            UuidV7Generator generator = new();

            var msValueInTicks = msValue * 10_000;
            generator.TryGenerateNew(DateTime.Today.AddTicks((long)msValueInTicks), out var guid);

            Span<byte> guidBytes = stackalloc byte[16];
            GuidHelper.TryWriteBigEndianBytes(guid, guidBytes);
            var msUpperByte = 0b0000_1111 & guidBytes[4];
            var msLowerByte = guidBytes[5];

            Check.That(msUpperByte).IsEqualTo(expectedMsUpperByte);
            Check.That(msLowerByte).IsEqualTo(expectedMsLowerByte);

            Check.That(UuidV7Generator.Decode(guid).timestampMs).IsEqualTo(msValue);
        }
    }
}
