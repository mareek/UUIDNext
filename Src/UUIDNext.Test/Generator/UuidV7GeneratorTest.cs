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
            var date = DateTime.UnixEpoch.AddMilliseconds(1789);

            Check.That(generator.TryGenerateNew(date, out var guido)).IsTrue();
            var (timestampMsO, sequenceO) = UuidV7Generator.Decode(guido);

            Check.That(timestampMsO).IsEqualTo(1789);

            //check that sequence initial value is randomized
            Check.That(sequenceO).IsStrictlyGreaterThan(0);
            Check.That(sequenceO).IsStrictlyLessThan(2048);

            Check.That(generator.TryGenerateNew(date, out var guida)).IsTrue();
            var (timestampMsA, sequenceA) = UuidV7Generator.Decode(guida);

            Check.That(timestampMsA).IsEqualTo(timestampMsO);
            Check.That(sequenceA).IsEqualTo(sequenceO + 1);

            Check.That(generator.TryGenerateNew(date.AddTicks(1), out var guidu)).IsTrue();
            var (timestampMsU, sequenceU) = UuidV7Generator.Decode(guidu);

            Check.That(timestampMsU).IsEqualTo(timestampMsO);
            Check.That(sequenceU).IsEqualTo(sequenceA + 1);

            Check.That(generator.TryGenerateNew(date.AddMilliseconds(1), out var guidi)).IsTrue();
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
            Parallel.For(0, 100, _ => generatedUuids.Add(generator.TryGenerateNew(date, out var guido) ? guido : Guid.Empty));

            var uuidsParts = generatedUuids.Select(u => UuidV7Generator.Decode(u)).ToArray();
            Check.That(uuidsParts.Select(x => x.sequence)).ContainsNoDuplicateItem();
            Check.That(uuidsParts.Select(x => x.timestampMs).Distinct()).HasSize(1);
        }
    }
}
