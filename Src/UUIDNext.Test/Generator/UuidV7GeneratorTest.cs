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
    public class UuidV7GeneratorTest : UuidTimestampWithOverflowGeneratorBaseTest
    {
        protected override byte Version => 7;

        protected override int SequenceBitSize => 12;

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
            var futureDate = date.AddMilliseconds(1);

            var generator = new UuidV7Generator();

            var guid1 = UuidTestHelper.New(generator, date);
            var date1 = UuidTestHelper.DecodeDate(guid1);
            var sequence1 = UuidTestHelper.DecodeSequence(guid1);

            var guid2 = UuidTestHelper.New(generator, pastDate);
            var date2 = UuidTestHelper.DecodeDate(guid2);
            var sequence2 = UuidTestHelper.DecodeSequence(guid2);

            Check.That(guid1.ToString()).IsBefore(guid2.ToString());
            Check.That(date2).IsEqualTo(date1);
            Check.That(sequence2).IsStrictlyGreaterThan(sequence1);

            var guid3 = UuidTestHelper.New(generator, futureDate);
            var date3 = UuidTestHelper.DecodeDate(guid3);

            Check.That(guid2.ToString()).IsBefore(guid3.ToString());
            Check.That(date3).IsEqualTo(date.AddMilliseconds(1));
        }

        [Fact]
        public void TestSequence()
        {
            UuidV7Generator generator = new();
            var date = DateTimeOffset.FromUnixTimeMilliseconds(0).AddMilliseconds(1789).UtcDateTime;

            var guido = UuidTestHelper.New(generator, date);
            var dateO = UuidTestHelper.DecodeDate(guido);
            var sequenceO = UuidTestHelper.DecodeSequence(guido);

            Check.That(dateO).IsEqualTo(date);

            //check that sequence initial value is randomized
            Check.That(sequenceO).IsStrictlyGreaterThan(0);
            Check.That(sequenceO).IsStrictlyLessThan(2048);

            var guida = UuidTestHelper.New(generator, date);
            var dateA = UuidTestHelper.DecodeDate(guida);
            var sequenceA = UuidTestHelper.DecodeSequence(guida);

            Check.That(dateA).IsEqualTo(dateO);
            Check.That(sequenceA).IsEqualTo(sequenceO + 1);

            var guidu = UuidTestHelper.New(generator, date.AddTicks(1));
            var dateU = UuidTestHelper.DecodeDate(guidu);
            var sequenceU = UuidTestHelper.DecodeSequence(guidu);

            Check.That(dateU).IsEqualTo(dateO);
            Check.That(sequenceU).IsEqualTo(sequenceA + 1);

            var guidi = UuidTestHelper.New(generator, date.AddMilliseconds(1));
            var dateI = UuidTestHelper.DecodeDate(guidi);
            var sequenceI = UuidTestHelper.DecodeSequence(guidi);

            Check.That(dateI).IsNotEqualTo(dateO);
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

            Check.That(generatedUuids.Select(UuidTestHelper.DecodeSequence)).ContainsNoDuplicateItem();
            Check.That(generatedUuids.Select(UuidTestHelper.DecodeDate).Distinct()).HasSize(1);
        }
    }
}
