using System;
using NFluent;
using UUIDNext.Generator;
using Xunit;

namespace UUIDNext.Test.Generator
{
    public class UuidV5GeneratorTest
    {
        [Fact]
        public void UuidV5KnowValue()
        {
            Guid expectedResult = new("2ed6657d-e927-568b-95e1-2665a8aea6a2");
            var guidV5 = new UuidV5Generator().New(Uuid.Namespace.DNS, "www.example.com");
            UuidTestHelper.CheckVersionAndVariant(guidV5, 5);
            Check.That(guidV5).IsEqualTo(expectedResult);
        }

        [Fact]
        public void UuidV5MustAndShould()
        {
            const string poetry = "Mais tu cries dans l'eau même en hiver";
            Guid namespaceId1 = Guid.NewGuid();
            Guid namespaceId2 = Guid.NewGuid();

            UuidV5Generator generator = new();

            Check.That(generator.New(namespaceId1, UuidTestHelper.LoremIpsum)).IsEqualTo(generator.New(namespaceId1, UuidTestHelper.LoremIpsum));
            Check.That(generator.New(namespaceId2, poetry)).IsEqualTo(generator.New(namespaceId2, poetry));
            Check.That(generator.New(namespaceId1, UuidTestHelper.LoremIpsum)).IsNotEqualTo(generator.New(namespaceId2, UuidTestHelper.LoremIpsum));
            Check.That(generator.New(namespaceId1, UuidTestHelper.LoremIpsum)).IsNotEqualTo(generator.New(namespaceId1, poetry));
            Check.That(generator.New(namespaceId1, UuidTestHelper.LoremIpsum)).IsNotEqualTo(generator.New(namespaceId2, poetry));

            UuidTestHelper.CheckVersionAndVariant(generator.New(namespaceId1, UuidTestHelper.LoremIpsum), 5);
            UuidTestHelper.CheckVersionAndVariant(generator.New(namespaceId2, poetry), 5);
        }

        [Fact]
        public void UuidV5MustHandleNotNormalizedString()
        {
            Guid namespaceId = Guid.NewGuid();

            const string poetry = "й";
            UuidV5Generator generator = new();
            var uuidv5 = generator.New(namespaceId, poetry);
            UuidTestHelper.CheckVersionAndVariant(uuidv5, 5);
        }
    }
}
