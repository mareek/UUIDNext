using System;
using NFluent;
using UUIDNext.Tools;
using Xunit;

namespace UUIDNext.Test.Tools;

public class UuidDecoderTest
{
    [Fact]
    public void CheckVersionOfGeneratedUuids()
    {
        Check.That(UuidDecoder.GetVersion(Guid.NewGuid())).Is(4);
        Check.That(UuidDecoder.GetVersion(Uuid.NewRandom())).Is(4);
        Check.That(UuidDecoder.GetVersion(Uuid.NewSequential())).Is(7);
        Check.That(UuidDecoder.GetVersion(Uuid.NewNameBased(Guid.NewGuid(), "uuid"))).Is(5);
        Check.That(UuidDecoder.GetVersion(Uuid.NewDatabaseFriendly(Database.PostgreSql))).Is(7);
        Check.That(UuidDecoder.GetVersion(Uuid.NewDatabaseFriendly(Database.SqlServer))).Is(8);
    }

    [Theory]
    [InlineData("c232ab00-9414-11ec-b3c8-9f6bdeced846", 1)]
    [InlineData("5df41881-3aed-3515-88a7-2f4a814cf09e", 3)]
    [InlineData("919108f7-52d1-4320-9bac-f847db4148a8", 4)]
    [InlineData("2ed6657d-e927-568b-95e1-2665a8aea6a2", 5)]
    [InlineData("1ec9414c-232a-6b00-b3c8-9f6bdeced846", 6)]
    [InlineData("017f22e2-79b0-7cc3-98c4-dc0c0c07398f", 7)]
    [InlineData("5c146b14-3c52-8afd-938a-375d0df1fbf6", 8)]
    public void TestVersion(string strGuid, int version)
    {
        var guid = Guid.Parse(strGuid);
        Check.That(UuidDecoder.GetVersion(guid)).Is(version);
    }

    [Fact]
    public void TestTimestampOfGeneratedUuids()
    {
        var now = DateTime.UtcNow;
        Guid uuidv7 = Uuid.NewSequential();
        Check.That(UuidDecoder.TryDecodeTimestamp(uuidv7, out var v7Date)).IsTrue();
        Check.That(v7Date).IsCloseTo(now, TimeSpan.FromMilliseconds(20));

        now = DateTime.UtcNow;
        Guid uuidv8 = Uuid.NewDatabaseFriendly(Database.SqlServer);
        Check.That(UuidDecoder.TryDecodeTimestamp(uuidv8, out var v8Date)).IsTrue();
        Check.That(v8Date).IsCloseTo(now, TimeSpan.FromMilliseconds(20));

        var nowTimestamp = new DateTimeOffset(now).ToUnixTimeMilliseconds();
        uuidv7 = UuidToolkit.CreateUuidV7(nowTimestamp, []);
        Check.That(UuidDecoder.TryDecodeTimestamp(uuidv7, out v7Date)).IsTrue();
        Check.That(v7Date).IsCloseTo(now, TimeSpan.FromMilliseconds(1));

        uuidv7 = UuidToolkit.CreateUuidV7FromSpecificDate(now);
        Check.That(UuidDecoder.TryDecodeTimestamp(uuidv7, out v7Date)).IsTrue();
        Check.That(v7Date).IsCloseTo(now, TimeSpan.FromMilliseconds(1));
    }

    [Theory]
    [InlineData("c232ab00-9414-11ec-b3c8-9f6bdeced846", true, 1645557742000)]
    [InlineData("5df41881-3aed-3515-88a7-2f4a814cf09e", false)]
    [InlineData("919108f7-52d1-4320-9bac-f847db4148a8", false)]
    [InlineData("2ed6657d-e927-568b-95e1-2665a8aea6a2", false)]
    [InlineData("1ec9414c-232a-6b00-b3c8-9f6bdeced846", true, 1645557742000)]
    [InlineData("017f22e2-79b0-7cc3-98c4-dc0c0c07398f", true, 1645557742000)]
    [InlineData("c2ff3d1f-e3b1-8dad-8361-017f22e279b0", true, 1645557742000)]
    [InlineData("5c146b14-3c52-8afd-938a-375d0df1fbf6", false)]
    public void TestTimestamp(string strGuid, bool hasTimestamp, long? expectedTimestamp = null)
    {
        var guid = Guid.Parse(strGuid);
        Check.That(UuidDecoder.TryDecodeTimestamp(guid, out DateTime date)).Is(hasTimestamp);
        if (hasTimestamp)
        {
            var actualTimestamp = new DateTimeOffset(date).ToUnixTimeMilliseconds();
            Check.That(actualTimestamp).Is(expectedTimestamp.Value);
        }
    }
}
