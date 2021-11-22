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

            ConcurrentBag<bool> succeses = new();
            Parallel.For(0, generator.GetSequenceMaxValue() + 1, _ => succeses.Add(generator.TryGenerateNew(date, out var _)));

            Check.That(succeses).ContainsOnlyElementsThatMatch(e => e);
            Check.That(generator.TryGenerateNew(date, out var _)).IsFalse();
        }
    }
}
