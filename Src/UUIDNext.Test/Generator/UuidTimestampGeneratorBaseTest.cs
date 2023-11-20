using System;
using System.Collections.Concurrent;
using System.Threading.Tasks;
using NFluent;
using UUIDNext.Generator;
using Xunit;

namespace UUIDNext.Test.Generator
{
    public abstract class UuidTimestampGeneratorBaseTest<TGenerator>
        where TGenerator : UuidTimestampGeneratorBase, new()
    {
        protected abstract byte Version { get; }

        protected abstract TimeSpan TimestampGranularity { get; }

        protected abstract (long timestamp, int sequence) DecodeUuid(Guid uuid);

        [Fact]
        public void DumbTest()
        {
            var generator = new TGenerator();
            ConcurrentBag<Guid> generatedUuids = new();
            Parallel.For(0, 100, _ => generatedUuids.Add(generator.New()));

            Check.That(generatedUuids).ContainsNoDuplicateItem();

            foreach (var uuid in generatedUuids)
            {
                UuidTestHelper.CheckVersionAndVariant(uuid, Version);
            }
        }

        [Fact]
        public void TestSequenceOverflow()
        {
            var generator = new TGenerator();
            var date = DateTime.UtcNow.Date;
            int sequenceMaxValue = generator.GetSequenceMaxValue();

            var firstUuid = generator.New(date);
            var (firstTimestamp, firstSequenceNumber) = DecodeUuid(firstUuid);

            Check.That(firstSequenceNumber).IsStrictlyLessThan((sequenceMaxValue + 1) / 2);

            Parallel.For(0, sequenceMaxValue - firstSequenceNumber, _ => generator.New(date));

            var lastUuid = generator.New(date);
            var (lastTimestamp, lastSequenceNumber) = DecodeUuid(lastUuid);
            Check.That(lastSequenceNumber).IsStrictlyLessThan((sequenceMaxValue + 1) / 2);
            Check.That(lastTimestamp).IsEqualTo(firstTimestamp + 1);
        }

        [Fact]
        public void TestSequenceOverflowWithOffset()
        {
            var generator = new TGenerator();
            var date = DateTime.UtcNow.Date;
            int sequenceMaxValue = generator.GetSequenceMaxValue();

            //setup an offset by generating an uuid into the future
            generator.New(date.AddSeconds(1));

            var firstUuid = generator.New(date);
            var (firstTimestamp, firstSequenceNumber) = DecodeUuid(firstUuid);

            Check.That(firstSequenceNumber).IsStrictlyLessThan((sequenceMaxValue + 1) / 2);

            Parallel.For(0, sequenceMaxValue - firstSequenceNumber, _ => generator.New(date));

            var lastUuid = generator.New(date);
            var (lastTimestamp, lastSequenceNumber) = DecodeUuid(lastUuid);
            Check.That(lastSequenceNumber).IsStrictlyLessThan((sequenceMaxValue + 1) / 2);
            Check.That(lastTimestamp).IsEqualTo(firstTimestamp + 1);
        }
    }
}
