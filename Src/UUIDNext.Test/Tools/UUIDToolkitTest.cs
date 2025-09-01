using System;
using System.Security.Cryptography;
using NFluent;
using UUIDNext.Tools;
using Xunit;

namespace UUIDNext.Test.Tools;

public class UUIDToolkitTest
{
    [Fact]
    public void CheckThatUuidV7FromCurrentDateTimeIsCoherentWithNewSequential()
    {
        const int testCount = 10_000;

        GuidComparer comparer = new();

        int errorCount = 0;
        for (int i = 0; i < testCount; i++)
        {
            var uuidFromSequential = Uuid.NewSequential();
            var uuidFromToolkit = UuidToolkit.CreateUuidV7FromSpecificDate(DateTimeOffset.Now);

            if (0 <= comparer.Compare(uuidFromSequential, uuidFromToolkit))
                errorCount += 1;
        }

        // We check that 99.5% of the call get forwarded to Uuid.NewSequential() as we can never reach 100% acuracy
        Check.That(errorCount * 100.0 / testCount).IsStrictlyLessThan(0.5);
    }

    [Fact]
    public void CheckThatUuidV7DateOrderIsRespected()
    {
        GuidComparer comparer = new();

        var uuidBeforeNow = UuidToolkit.CreateUuidV7FromSpecificDate(DateTimeOffset.UtcNow.AddMilliseconds(-1));
        var uuidNow = Uuid.NewSequential();
        var uuidAfterNow = UuidToolkit.CreateUuidV7FromSpecificDate(DateTimeOffset.UtcNow.AddMilliseconds(1));

        Check.That(comparer.Compare(uuidBeforeNow, uuidNow)).IsStrictlyNegative();
        Check.That(comparer.Compare(uuidNow, uuidAfterNow)).IsStrictlyNegative();
    }

    [Fact]
    public void CheckThatUuidV8FromCurrentDateTimeIsCoherentWithNewDatabaseFriendly()
    {
        const int testCount = 10_000;

        UuidWithTimestampComparer comparer = new();

        int errorCount = 0;
        for (int i = 0; i < testCount; i++)
        {
            var uuidFromSequential = Uuid.NewDatabaseFriendly(Database.SqlServer);
            var uuidFromToolkit = UuidToolkit.CreateSequentialUuidForSqlServerFromSpecificDate(DateTimeOffset.Now);

            if (0 <= comparer.Compare(uuidFromSequential, uuidFromToolkit))
                errorCount += 1;
        }

        // We check that 99.5% of the call get forwarded to Uuid.NewDatabaseFriendly(Database.SqlServer) as we can never reach 100% acuracy
        Check.That(errorCount * 100.0 / testCount).IsStrictlyLessThan(0.5);
    }

    [Fact]
    public void CheckThatUuidV8DateOrderIsRespected()
    {
        UuidWithTimestampComparer comparer = new();

        var uuidBeforeNow = UuidToolkit.CreateSequentialUuidForSqlServerFromSpecificDate(DateTimeOffset.UtcNow.AddMilliseconds(-1));
        var uuidNow = Uuid.NewSequential();
        var uuidAfterNow = UuidToolkit.CreateSequentialUuidForSqlServerFromSpecificDate(DateTimeOffset.UtcNow.AddMilliseconds(1));

        Check.That(comparer.Compare(uuidBeforeNow, uuidNow)).IsStrictlyNegative();
        Check.That(comparer.Compare(uuidNow, uuidAfterNow)).IsStrictlyNegative();
    }

    [Fact]
    public void TestCreateUuidFromBigEndianBytes()
    {
        Span<byte> bytes = stackalloc byte[16];

        for (int i = 0; i < 16; i++)
            bytes[i] = 0;
        var nilV8Uuid = UuidToolkit.CreateGuidFromBigEndianBytes(bytes);
        Check.That(UuidDecoder.GetVersion(nilV8Uuid)).Is(8);
        Check.That(nilV8Uuid.ToString()).Is("00000000-0000-8000-8000-000000000000");

        for (int i = 0; i < 16; i++)
            bytes[i] = 255;
        var maxV8Uuid = UuidToolkit.CreateGuidFromBigEndianBytes(bytes);
        Check.That(UuidDecoder.GetVersion(maxV8Uuid)).Is(8);
        Check.That(maxV8Uuid.ToString()).Is("ffffffff-ffff-8fff-bfff-ffffffffffff");
    }

    [Fact]
    public void TestCreateNameBasedUuid()
    {
        // this example is taken from the RFC 9562 Appendix B.2
        var dnsNamespaceId = Guid.Parse("6ba7b810-9dad-11d1-80b4-00c04fd430c8");
        var uuid = UuidToolkit.CreateUuidFromName(dnsNamespaceId, "www.example.com", SHA256.Create());
        Check.That(uuid.ToString()).Is("5c146b14-3c52-8afd-938a-375d0df1fbf6");
    }
}
