using System;
using System.Linq;
using NFluent;
using Xunit;

namespace UUIDNext.Test.Generator;

public class GuidHelperTest
{
    [Theory]
    [InlineData("332E882B-9BEC-4CC1-8079-450A0AD93485", false, 43, 136, 46, 51, 236, 155, 193, 76, 128, 121, 69, 10, 10, 217, 52, 133)]
    [InlineData("00010203-0405-0607-0809-0A0B0C0D0E0F", true, 0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15)]
    [InlineData("00010203-0405-0607-0809-0A0B0C0D0E0F", false, 3, 2, 1, 0, 5, 4, 7, 6, 8, 9, 10, 11, 12, 13, 14, 15)]
    public void TestToByteArrayExtension(string strGuid, bool bigEndian, params int[] expectedResult)
    {
        var guid = Guid.Parse(strGuid);
        
        var toByteArrayResult = guid.ToByteArray(bigEndian);
        Check.That(toByteArrayResult).IsEquivalentTo(expectedResult);

        var tryWriteByteResult = new byte[16];
        bool success = guid.TryWriteBytes(tryWriteByteResult, bigEndian, out var bytesWritten);
        Check.That(success).IsTrue();
        Check.That(tryWriteByteResult).IsEquivalentTo(expectedResult);
        Check.That(bytesWritten).IsEqualTo(16);
    }

    [Fact]
    public void TestTryWriteBytesFailures()
    {
        var guid = Guid.NewGuid();
        Span<byte> tooSmallBuffer = stackalloc byte[12];
        var success = guid.TryWriteBytes(tooSmallBuffer, true, out var bytesWritten);
        
        Check.That(success).IsFalse();
        Check.That(bytesWritten).IsEqualTo(0);
    }

    [Theory]
    [InlineData("332E882B-9BEC-4CC1-8079-450A0AD93485", false, 43, 136, 46, 51, 236, 155, 193, 76, 128, 121, 69, 10, 10, 217, 52, 133)]
    [InlineData("00010203-0405-0607-0809-0A0B0C0D0E0F", true, 0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15)]
    [InlineData("00010203-0405-0607-0809-0A0B0C0D0E0F", false, 3, 2, 1, 0, 5, 4, 7, 6, 8, 9, 10, 11, 12, 13, 14, 15)]
    public void FromBytesTest(string expectedStrGuid, bool bigEndian, params int[] input)
    {
        var expectedGuid = Guid.Parse(expectedStrGuid);
        var bytes = input.Select(i => (byte)i).ToArray();
        
        var guid = GuidHelper.FromBytes(bytes, bigEndian);
        Check.That(guid).IsEqualTo(expectedGuid);

        var secondRunGuid = GuidHelper.FromBytes(bytes, bigEndian);
        Check.That(secondRunGuid).IsEqualTo(expectedGuid);

        var invertEndiannessGuid = GuidHelper.FromBytes(bytes, !bigEndian);
        Check.That(invertEndiannessGuid).IsNotEqualTo(expectedGuid);
    }
}
