using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using NFluent;
using UUIDNext.Tools;

namespace UUIDNext.Test
{
    internal static class UuidTestHelper
    {
        public const string LoremIpsum = "Lorem ipsum dolor sit amet, consectetur adipiscing elit, sed do eiusmod tempor incididunt";

        public static void CheckVersionAndVariant(Guid uuid, byte version)
        {
            var strUuid = uuid.ToString();
            Check.That(strUuid[14]).IsEqualTo(version.ToString().Single());
            Check.That(strUuid[19]).IsOneOf('8', '9', 'a', 'b', 'A', 'B');
        }

        public static Guid New(object generator, DateTimeOffset date)
        {
            var tryGenerateNewMethod = generator.GetType()
                                                .GetMethods(BindingFlags.Instance | BindingFlags.NonPublic)
                                                .Single(m => m.Name == "New" && m.GetParameters().Length == 1);
            object[] parameters = { date };
            return (Guid)tryGenerateNewMethod.Invoke(generator, parameters);
        }

        public static short DecodeSequence(Guid uuid)
        {
            Check.That(UuidDecoder.TryDecodeSequence(uuid, out var sequence)).IsTrue();
            return sequence;
        }

        public static DateTime DecodeDate(Guid uuid)
        {
            Check.That(UuidDecoder.TryDecodeTimestamp(uuid, out var date)).IsTrue();
            return date;
        }

        public static IEnumerable<(int expectedPosition, Guid uuid)> GetDatabaseTestSet(object generator, int stepSize = 10)
        {
            DateTimeOffset date = new(new(2023, 1, 1));

            return GenerateTestSet(generator, date, 0, stepSize)
                    .Concat(GenerateTestSet(generator, date.AddMilliseconds(1), stepSize, stepSize))
                    .Concat(GenerateTestSet(generator, date.AddSeconds(2), 2 * stepSize, stepSize))
                    .Concat(GenerateTestSet(generator, date.AddMinutes(3), 3 * stepSize, stepSize))
                    .Concat(GenerateTestSet(generator, date.AddHours(4), 4 * stepSize, stepSize))
                    .Concat(GenerateTestSet(generator, date.AddDays(5), 5 * stepSize, stepSize))
                    .Concat(GenerateTestSet(generator, date.AddYears(6), 6 * stepSize, stepSize))
                    .Concat(GenerateTestSet(generator, date.AddYears(50), 7 * stepSize, stepSize));
        }

        private static IEnumerable<(int expectedPosition, Guid uuid)> GenerateTestSet(object generator, DateTimeOffset date, int offset, int setSize)
        {
            for (int i = 0; i < setSize; i++)
            {
                yield return (i + offset, New(generator, date));
            }
        }
    }
}
