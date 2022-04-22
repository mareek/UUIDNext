using System;
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

        public static Guid New(this UuidTimestampGeneratorBase generator, DateTime date)
        {
            var tryGenerateNewMethod = generator.GetType().GetMethod("New", BindingFlags.Instance | BindingFlags.NonPublic);
            object[] parameters = { date };
            return (Guid)tryGenerateNewMethod.Invoke(generator, parameters);
        }

        public static int GetSequenceMaxValue(this UuidTimestampGeneratorBase generator)
        {
            var sequenceBitSizeProperty = generator.GetType().GetProperty("SequenceBitSize", BindingFlags.Instance | BindingFlags.NonPublic);
            int sequenceBitSize = (int)sequenceBitSizeProperty.GetValue(generator, null);
            return (1 << sequenceBitSize) - 1;
        }
    }
}
