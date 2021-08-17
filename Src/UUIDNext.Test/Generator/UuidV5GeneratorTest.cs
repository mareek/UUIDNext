using System;
using System.Collections.Generic;
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
            Guid namespaceId = new("f8e58ba0-803a-402f-9229-a40e7d2d35e4");
            Guid expectedResult = new("5f9e84e4-1a9f-5ae7-a8dd-522559d3537f");
            var guidV5 = new UuidV5Generator().New(namespaceId, "toto");
            Check.That(guidV5).IsEqualTo(expectedResult);
        }

        [Fact]
        public void UuidV5MustAndShould()
        {
            const string loremIpsum = "Lorem ipsum dolor sit amet, consectetur adipiscing elit, sed do eiusmod tempor incididunt";
            const string poetry = "Mais tu cries dans l'eau même en hiver";
            Guid namespaceId1 = Guid.NewGuid();
            Guid namespaceId2 = Guid.NewGuid();

            Check.That(Uuid.NewV5(namespaceId1, loremIpsum)).IsEqualTo(Uuid.NewV5(namespaceId1, loremIpsum));
            Check.That(Uuid.NewV5(namespaceId2, poetry)).IsEqualTo(Uuid.NewV5(namespaceId2, poetry));
            Check.That(Uuid.NewV5(namespaceId1, loremIpsum)).IsNotEqualTo(Uuid.NewV5(namespaceId2, loremIpsum));
            Check.That(Uuid.NewV5(namespaceId1, loremIpsum)).IsNotEqualTo(Uuid.NewV5(namespaceId1, poetry));
            Check.That(Uuid.NewV5(namespaceId1, loremIpsum)).IsNotEqualTo(Uuid.NewV5(namespaceId2, poetry));
        }
    }
}
