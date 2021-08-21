using System;
using Xunit;

namespace UUIDNext.Test
{
    public class UuidTest
    {
        [Fact]
        public void TestDefaultUuidVersions()
        {
            UuidTestHelper.CheckVersionAndVariant(Uuid.NewRandom(), 4);
            UuidTestHelper.CheckVersionAndVariant(Uuid.NewNameBased(Guid.NewGuid(), "toto"), 5);
            UuidTestHelper.CheckVersionAndVariant(Uuid.NewSequential(), 7);
        }
    }
}
