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

        protected override (long timestamp, int sequence) DecodeUuid(Guid uuid) => UuidV7Generator.Decode(uuid);

        [Fact]
        public void TestSequence()
        {
            UuidV7Generator generator = new();
            var date = DateTime.UnixEpoch.AddMilliseconds(1789);

            var guido = generator.New(date);
            var (timestampMsO, sequenceO) = UuidV7Generator.Decode(guido);

            Check.That(timestampMsO).IsEqualTo(1789);

            //check that sequence initial value is randomized
            Check.That(sequenceO).IsStrictlyGreaterThan(0);
            Check.That(sequenceO).IsStrictlyLessThan(2048);

            var guida = generator.New(date);
            var (timestampMsA, sequenceA) = UuidV7Generator.Decode(guida);

            Check.That(timestampMsA).IsEqualTo(timestampMsO);
            Check.That(sequenceA).IsEqualTo(sequenceO + 1);

            var guidu = generator.New(date.AddTicks(1));
            var (timestampMsU, sequenceU) = UuidV7Generator.Decode(guidu);

            Check.That(timestampMsU).IsEqualTo(timestampMsO);
            Check.That(sequenceU).IsEqualTo(sequenceA + 1);

            var guidi = generator.New(date.AddMilliseconds(1));
            var (timestampMsI, sequenceI) = UuidV7Generator.Decode(guidi);

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
            Parallel.For(0, 100, _ => generatedUuids.Add(generator.New(date)));

            var uuidsParts = generatedUuids.Select(u => UuidV7Generator.Decode(u)).ToArray();
            Check.That(uuidsParts.Select(x => x.sequence)).ContainsNoDuplicateItem();
            Check.That(uuidsParts.Select(x => x.timestampMs).Distinct()).HasSize(1);
        }
    }
}
