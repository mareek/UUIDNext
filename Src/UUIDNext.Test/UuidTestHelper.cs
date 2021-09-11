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

        public static Guid NewInternal(this UuidTimestampGeneratorBase generator, DateTime date)
        {
            var newInternalMethod = generator.GetType().GetMethod("NewInternal", BindingFlags.Instance | BindingFlags.NonPublic);
            return (Guid)newInternalMethod.Invoke(generator, new object[] { date });
        }
    }
}
