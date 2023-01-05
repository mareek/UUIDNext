using System;
using NFluent;
using UUIDNext.Generator;
using Xunit;

namespace UUIDNext.Test.Generator
{
    public class UuidV3GeneratorTest
    {
        [Fact]
        public void UuidV3KnowValue()
        {
            Guid namespaceId = new("48161fc7-b695-4d7c-8eea-3e0205f43427");
            Guid expectedResult = new("f1f8d712-f502-3bfc-b920-bd200754cd5e");
#pragma warning disable CS0618 // Type or member is obsolete
            var guidV3 = new UuidV3Generator().New(namespaceId, UuidTestHelper.LoremIpsum);
#pragma warning restore CS0618 // Type or member is obsolete
            UuidTestHelper.CheckVersionAndVariant(guidV3, 3);
            Check.That(guidV3).IsEqualTo(expectedResult);
        }
    }
}
