using System;
using System.Collections.Concurrent;
using System.Threading.Tasks;
using NFluent;
using Xunit;

namespace UUIDNext.Test.Generator
{
    public abstract class UuidTimestampGeneratorBaseTest
    {
        protected abstract byte Version { get; }

        protected abstract TimeSpan TimestampGranularity { get; }

        protected abstract int SequenceBitSize { get; }

        protected abstract (long timestamp, int sequence) DecodeUuid(Guid uuid);

        protected abstract Guid NewUuid(object generator);

        protected abstract object NewGenerator();

        private int GetSequenceMaxValue() => (1 << SequenceBitSize) - 1;

        [Fact]
        public void DumbTest()
        {
            var generator = NewGenerator();
            ConcurrentBag<Guid> generatedUuids = new();
            Parallel.For(0, 100, _ => generatedUuids.Add(NewUuid(generator)));

            Check.That(generatedUuids).ContainsNoDuplicateItem();

            foreach (var uuid in generatedUuids)
            {
                UuidTestHelper.CheckVersionAndVariant(uuid, Version);
            }
        }

        [Fact]
        public void TestSequenceOverflow()
        {
            var generator = NewGenerator();
            var date = DateTime.UtcNow.Date;
            int sequenceMaxValue = GetSequenceMaxValue();

            var firstUuid = UuidTestHelper.New(generator, date);
            var (firstTimestamp, firstSequenceNumber) = DecodeUuid(firstUuid);

            Check.That(firstSequenceNumber).IsStrictlyLessThan((sequenceMaxValue + 1) / 2);

            Parallel.For(0, sequenceMaxValue - firstSequenceNumber, _ => UuidTestHelper.New(generator, date));

            var lastUuid = UuidTestHelper.New(generator, date);
            var (lastTimestamp, lastSequenceNumber) = DecodeUuid(lastUuid);
            Check.That(lastSequenceNumber).IsStrictlyLessThan((sequenceMaxValue + 1) / 2);
            Check.That(lastTimestamp).IsEqualTo(firstTimestamp + 1);
        }

        [Fact]
        public void TestSequenceOverflowWithOffset()
        {
            var generator = NewGenerator();
            var date = DateTime.UtcNow.Date;
            int sequenceMaxValue = GetSequenceMaxValue();

            //setup an offset by generating an uuid into the future
            UuidTestHelper.New(generator, date.AddSeconds(1));

            var firstUuid = UuidTestHelper.New(generator, date);
            var (firstTimestamp, firstSequenceNumber) = DecodeUuid(firstUuid);

            Check.That(firstSequenceNumber).IsStrictlyLessThan((sequenceMaxValue + 1) / 2);

            Parallel.For(0, sequenceMaxValue - firstSequenceNumber, _ => UuidTestHelper.New(generator, date));

            var lastUuid = UuidTestHelper.New(generator, date);
            var (lastTimestamp, lastSequenceNumber) = DecodeUuid(lastUuid);
            Check.That(lastSequenceNumber).IsStrictlyLessThan((sequenceMaxValue + 1) / 2);
            Check.That(lastTimestamp).IsEqualTo(firstTimestamp + 1);
        }
    }
}
