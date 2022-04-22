using System;
using System.Collections.Concurrent;
using System.Linq;
using System.Threading.Tasks;
using NFluent;
using UUIDNext.Generator;
using Xunit;

namespace UUIDNext.Test.Generator
{
    public class UuidV6GeneratorTest : UuidTimestampGeneratorBaseTest
    {
        protected override byte Version => 6;

        protected override UuidTimestampGeneratorBase GetNewGenerator() => new UuidV6Generator();

        protected override (long timestamp, int sequence) DecodeUuid(Guid uuid) => UuidV6Generator.Decode(uuid);

        [Fact]
        public void TestSequence()
        {
            UuidV6Generator generator = new();
            var date = DateTime.UtcNow.Date;

            var guido = generator.New(date);
            var (timestampO, sequenceO) = UuidV6Generator.Decode(guido);

            var guida = generator.New(date);
            var (timestampA, sequenceA) = UuidV6Generator.Decode(guida);

            Check.That(timestampA).IsEqualTo(timestampO);
            Check.That(sequenceA).IsEqualTo(sequenceO + 1);

            var guidu = generator.New(date.AddTicks(1));
            var (timestampU, _) = UuidV6Generator.Decode(guidu);

            Check.That(timestampU).IsStrictlyGreaterThan(timestampO);
        }

        [Fact]
        public void TestSequenceMultiThread()
        {
            UuidV6Generator generator = new();
            var date = DateTime.UtcNow.Date;

            ConcurrentBag<Guid> generatedUuids = new();
            Parallel.For(0, 100, _ => generatedUuids.Add(generator.New(date)));

            var uuidsParts = generatedUuids.Select(u => UuidV6Generator.Decode(u)).ToArray();
            Check.That(uuidsParts.Select(x => x.sequence)).ContainsNoDuplicateItem();
            Check.That(uuidsParts.Select(x => x.timestamp).Distinct()).HasSize(1);
        }
    }
}
