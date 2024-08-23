﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using NFluent;
using UUIDNext.Generator;

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

        public static Guid New(this UuidGeneratorBase generator, DateTime date)
        {
            var tryGenerateNewMethod = generator.GetType()
                                                .GetMethods(BindingFlags.Instance | BindingFlags.NonPublic)
                                                .Single(m => m.Name == "New" && m.GetParameters().Length == 1);
            object[] parameters = { date };
            return (Guid)tryGenerateNewMethod.Invoke(generator, parameters);
        }

        public static IEnumerable<(int expectedPosition, Guid uuid)> GetDatabaseTestSet(this UuidGeneratorBase generator, int stepSize = 10)
        {
            DateTime date = new(2023, 1, 1);

            return generator.GenerateTestSet(date, 0, stepSize)
                            .Concat(generator.GenerateTestSet(date.AddMilliseconds(1), stepSize, stepSize))
                            .Concat(generator.GenerateTestSet(date.AddSeconds(2), 2 * stepSize, stepSize))
                            .Concat(generator.GenerateTestSet(date.AddMinutes(3), 3 * stepSize, stepSize))
                            .Concat(generator.GenerateTestSet(date.AddHours(4), 4 * stepSize, stepSize))
                            .Concat(generator.GenerateTestSet(date.AddDays(5), 5 * stepSize, stepSize))
                            .Concat(generator.GenerateTestSet(date.AddYears(6), 6 * stepSize, stepSize))
                            .Concat(generator.GenerateTestSet(date.AddYears(50), 7 * stepSize, stepSize));
        }

        private static IEnumerable<(int expectedPosition, Guid uuid)> GenerateTestSet(this UuidGeneratorBase generator, DateTime date, int offset, int setSize)
        {
            for (int i = 0; i < setSize; i++)
            {
                yield return (i + offset, generator.New(date));
            }
        }
    }
}
