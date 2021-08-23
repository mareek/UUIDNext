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
        private readonly UuidV6Generator _generator = new();

        protected override byte Version => 6;

        protected override Guid NewUuid() => _generator.New();

        [Fact]
        public void TestSequence()
        {
            UuidV6Generator generator = new();
            var date = DateTime.UtcNow.Date;

            var guido = generator.NewInternal(date);
            var (timestampO, sequenceO) = generator.Decode(guido);

            Check.That(sequenceO).IsEqualTo(0);

            var guida = generator.NewInternal(date);
            var (timestampA, sequenceA) = generator.Decode(guida);

            Check.That(timestampA).IsEqualTo(timestampO);
            Check.That(sequenceA).IsEqualTo(sequenceO + 1);

            var guidu = generator.NewInternal(date.AddTicks(1));
            var (timestampU, sequenceU) = generator.Decode(guidu);

            Check.That(sequenceU).IsEqualTo(0);
            Check.That(timestampU).IsNotEqualTo(timestampO);
        }

        [Fact]
        public void TestSequenceMultiThread()
        {
            UuidV6Generator generator = new();
            var date = DateTime.UtcNow.Date;

            ConcurrentBag<Guid> generatedUuids = new();
            Parallel.For(0, 100, _ => generatedUuids.Add(generator.NewInternal(date)));

            var uuidsParts = generatedUuids.Select(u => generator.Decode(u)).ToArray();
            Check.That(uuidsParts.Select(x => x.sequence)).ContainsNoDuplicateItem();
            Check.That(uuidsParts.Select(x => x.timestamp).Distinct()).HasSize(1);
        }
    }
}
