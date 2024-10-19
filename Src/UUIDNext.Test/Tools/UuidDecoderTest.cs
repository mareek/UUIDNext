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
        var nowTimestamp = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
        Guid uuidv7 = Uuid.NewSequential();
        Check.That(UuidDecoder.TryGetTimestamp(uuidv7, out long v7Timestamp)).IsTrue();
        CheckThatIsCloseTo(nowTimestamp, v7Timestamp, 10);

        nowTimestamp = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
        Guid uuidv8 = Uuid.NewDatabaseFriendly(Database.SqlServer);
        Check.That(UuidDecoder.TryGetTimestamp(uuidv8, out long v8Timestamp)).IsTrue();
        CheckThatIsCloseTo(nowTimestamp, v8Timestamp, 10);

        var now = DateTimeOffset.UtcNow;
        nowTimestamp = now.ToUnixTimeMilliseconds();
        uuidv7 = UuidToolkit.CreateUuidV7(nowTimestamp, []);
        Check.That(UuidDecoder.TryGetTimestamp(uuidv7, out v7Timestamp)).IsTrue();
        CheckThatIsCloseTo(nowTimestamp, v7Timestamp);

        uuidv7 = UuidToolkit.CreateUuidV7FromSpecificDate(now);
        Check.That(UuidDecoder.TryGetTimestamp(uuidv7, out v7Timestamp)).IsTrue();
        CheckThatIsCloseTo(nowTimestamp, v7Timestamp);
    }

    private static void CheckThatIsCloseTo(long nowTimestamp, long v7Timestamp, long margin = 0)
        => Check.That(v7Timestamp).IsGreaterOrEqualThan(nowTimestamp - margin)
                              .And.IsLessOrEqualThan(nowTimestamp + margin);

    [Theory]
    //[InlineData("c232ab00-9414-11ec-b3c8-9f6bdeced846", true)] // I need to sleep before I implement v1 decoding
    [InlineData("5df41881-3aed-3515-88a7-2f4a814cf09e", false)]
    [InlineData("919108f7-52d1-4320-9bac-f847db4148a8", false)]
    [InlineData("2ed6657d-e927-568b-95e1-2665a8aea6a2", false)]
    [InlineData("1c80415d-efe2-6100-811e-2ac60686f88e", true, 616415336208)]
    [InlineData("011ceeaa-6cd8-71ec-ae27-d9621b04ada3", true, 1223774858456)]
    [InlineData("45f98020-66e7-871d-9641-0170e4be4d77", true, 1584385641847)]
    [InlineData("5c146b14-3c52-8afd-938a-375d0df1fbf6", false)]
    public void TestTimestamp(string strGuid, bool hasTimestamp, long? expectedTimestamp = null)
    {
        var guid = Guid.Parse(strGuid);
        Check.That(UuidDecoder.TryGetTimestamp(guid, out long actualTimestamp)).Is(hasTimestamp);
        if (hasTimestamp)
            Check.That(actualTimestamp).Is(expectedTimestamp.Value);
    }
}
