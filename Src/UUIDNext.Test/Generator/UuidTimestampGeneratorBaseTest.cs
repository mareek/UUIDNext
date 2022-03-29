using System;
using System.Collections.Concurrent;
using System.Threading.Tasks;
using NFluent;
using UUIDNext.Generator;
using Xunit;

namespace UUIDNext.Test.Generator
{
    public abstract class UuidTimestampGeneratorBaseTest
    {
        protected abstract byte Version { get; }
        protected abstract UuidTimestampGeneratorBase GetNewGenerator();

        protected abstract int GetSequence(Guid uuid);

        [Fact]
        public void DumbTest()
        {
            var generator = GetNewGenerator();
            ConcurrentBag<Guid> generatedUuids = new();
            Parallel.For(0, 100, _ => generatedUuids.Add(generator.New()));

            Check.That(generatedUuids).ContainsNoDuplicateItem();

            foreach (var uuid in generatedUuids)
            {
                UuidTestHelper.CheckVersionAndVariant(uuid, Version);
            }
        }

        [Fact]
        public void OrderTest()
        {
            var generator = GetNewGenerator();
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
        public void TestSequenceOverflow()
        {
            var generator = GetNewGenerator();
            var date = DateTime.UtcNow.Date;
            int sequenceMaxValue = generator.GetSequenceMaxValue();

            Check.That(generator.TryGenerateNew(date, out var firstUuid)).IsTrue();
            var firsSequenceNumber = GetSequence(firstUuid);

            Check.That(firsSequenceNumber).IsStrictlyLessThan((sequenceMaxValue + 1) / 2);

            ConcurrentBag<bool> succeses = new();
            Parallel.For(0, sequenceMaxValue - firsSequenceNumber, _ => succeses.Add(generator.TryGenerateNew(date, out var _)));

            Check.That(succeses).ContainsOnlyElementsThatMatch(e => e);
            Check.That(generator.TryGenerateNew(date, out var _)).IsFalse();
        }

        [Fact]
        public void TestBackwardClock()
        {
            var date = DateTime.UtcNow.Date;
            var pastDate = date.AddSeconds(-5);

            var generator = GetNewGenerator();
            Check.That(generator.TryGenerateNew(date, out var guid1)).IsTrue();

            Check.That(generator.TryGenerateNew(pastDate, out var guid2)).IsTrue();
            Check.That(guid1.ToString()).IsBefore(guid2.ToString());

            Check.That(generator.TryGenerateNew(date, out var guid3)).IsTrue();
            Check.That(guid2.ToString()).IsBefore(guid3.ToString());

            Check.That(generator.TryGenerateNew(pastDate, out var guid4)).IsTrue();
            Check.That(guid3.ToString()).IsBefore(guid4.ToString());
        }
    }
}
