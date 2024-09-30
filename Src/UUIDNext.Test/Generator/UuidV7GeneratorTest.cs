using System;
using System.Collections.Concurrent;
using System.Linq;
using System.Threading.Tasks;
using NFluent;
using UUIDNext.Generator;
using UUIDNext.Tools;
using Xunit;

namespace UUIDNext.Test.Generator
{
    public class UuidV7GeneratorTest : UuidTimestampGeneratorBaseTest
    {
        protected override byte Version => 7;

        protected override TimeSpan TimestampGranularity => TimeSpan.FromMilliseconds(1);

        protected override int SequenceBitSize => 12;

        protected override (long timestamp, int sequence) DecodeUuid(Guid uuid) => UuidDecoder.DecodeUuidV7(uuid);

        protected override Guid NewUuid(object generator) => ((UuidV7Generator)generator).New();

        protected override object NewGenerator() => new UuidV7Generator();


        [Fact]
        public void OrderTest()
        {
            var generator = new UuidV7Generator();
            Span<Guid> guids = stackalloc Guid[100];
            for (int i = 0; i < 100; i++)
            {
                guids[i] = generator.New();
            }

            var comparer = new GuidComparer();
            for (int i = 1; i < guids.Length; i++)
            {
                Check.That(comparer.Compare(guids[i - 1], guids[i])).IsStrictlyLessThan(0);
            }
        }

        [Fact]
        public void TestBackwardClock()
        {
            var date = DateTime.UtcNow.Date;
            var pastDate = date.AddSeconds(-5);

            var generator = new UuidV7Generator();
            var guid1 = UuidTestHelper.New(generator, date);

            var guid2 = UuidTestHelper.New(generator, pastDate);
            Check.That(guid1.ToString()).IsBefore(guid2.ToString());

            var guid3 = UuidTestHelper.New(generator, date);
            Check.That(guid2.ToString()).IsBefore(guid3.ToString());

            var guid4 = UuidTestHelper.New(generator, pastDate);
            Check.That(guid3.ToString()).IsBefore(guid4.ToString());
        }

        [Fact]
        public void TestTimestampDriftreduction()
        {
            var date = DateTime.UtcNow.Date;
            var pastDate = date.AddSeconds(-5);
            var futureDate = date.Add(TimestampGranularity);

            var generator = new UuidV7Generator();
            var guid1 = UuidTestHelper.New(generator, date);
            var (timestamp1, sequence1) = DecodeUuid(guid1);

            var guid2 = UuidTestHelper.New(generator, pastDate);
            var (timestamp2, sequence2) = DecodeUuid(guid2);
            Check.That(guid1.ToString()).IsBefore(guid2.ToString());
            Check.That(timestamp2).IsEqualTo(timestamp1);
            Check.That(sequence2).IsStrictlyGreaterThan(sequence1);

            var guid3 = UuidTestHelper.New(generator, futureDate);
            var (timestamp3, _) = DecodeUuid(guid3);
            Check.That(guid2.ToString()).IsBefore(guid3.ToString());
            Check.That(timestamp3).IsEqualTo(timestamp1 + 1);
        }

        [Fact]
        public void TestSequence()
        {
            UuidV7Generator generator = new();
            var date = DateTimeOffset.FromUnixTimeMilliseconds(0).AddMilliseconds(1789).UtcDateTime;

            var guido = UuidTestHelper.New(generator, date);
            var (timestampMsO, sequenceO) = UuidDecoder.DecodeUuidV7(guido);

            Check.That(timestampMsO).IsEqualTo(1789);

            //check that sequence initial value is randomized
            Check.That(sequenceO).IsStrictlyGreaterThan(0);
            Check.That(sequenceO).IsStrictlyLessThan(2048);

            var guida = UuidTestHelper.New(generator, date);
            var (timestampMsA, sequenceA) = UuidDecoder.DecodeUuidV7(guida);

            Check.That(timestampMsA).IsEqualTo(timestampMsO);
            Check.That(sequenceA).IsEqualTo(sequenceO + 1);

            var guidu = UuidTestHelper.New(generator, date.AddTicks(1));
            var (timestampMsU, sequenceU) = UuidDecoder.DecodeUuidV7(guidu);

            Check.That(timestampMsU).IsEqualTo(timestampMsO);
            Check.That(sequenceU).IsEqualTo(sequenceA + 1);

            var guidi = UuidTestHelper.New(generator, date.AddMilliseconds(1));
            var (timestampMsI, sequenceI) = UuidDecoder.DecodeUuidV7(guidi);

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
            Parallel.For(0, 100, _ => generatedUuids.Add(UuidTestHelper.New(generator, date)));

            var uuidsParts = generatedUuids.Select(u => UuidDecoder.DecodeUuidV7(u)).ToArray();
            Check.That(uuidsParts.Select(x => x.sequence)).ContainsNoDuplicateItem();
            Check.That(uuidsParts.Select(x => x.timestampMs).Distinct()).HasSize(1);
        }
    }
}
