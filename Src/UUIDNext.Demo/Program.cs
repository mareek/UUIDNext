using UUIDNext;
using UUIDNext.Demo;

const int SampleSize = 10_000;
const int PercentStep = SampleSize / 100;
const int WorkerCount = 1;
const int ChunkSize = SampleSize / WorkerCount;

static Guid GenerateId() => Uuid.NewDatabaseFriendly(Database.SqlServer);
//static Guid GenerateId() => Uuid.NewRandom();

try
{
    await SqlServerHelper.CreateDatabase();
    await SqlServerHelper.CreateTable();
    TimeSpan[] chronos = new TimeSpan[SampleSize];

    int rowInsertedCount = 0;
    int offset = 0;
    var workers = new Task[WorkerCount];
    for (int i = 0; i < WorkerCount; i++)
    {
        Console.WriteLine($"Worker {i} started");
         ça merde ici, je sais pas pourquoi
        workers[i] = Task.Factory.StartNew(async () => await InsertChunk(offset), TaskCreationOptions.LongRunning);
        offset += ChunkSize;
    }

    async Task InsertChunk(int offset)
    {
        using var connection = await SqlServerHelper.OpenDBConnection();
        for (int i = 0; i < ChunkSize; i++)
        {
            var id = GenerateId();
            var data = SqlServerHelper.GenerateData();

            chronos[i + offset] = await SqlServerHelper.InsertLine(connection, id, data);

            var step = Interlocked.Increment(ref rowInsertedCount);
            if (step % PercentStep == 0)
            {
                Console.Clear();
                Console.WriteLine($"{step / PercentStep} %");
            }
        }
    }

    await Task.WhenAll(workers);

    var buckets = StatsHelper.ComputeMedian(chronos, 100).Select(t => t.TotalMicroseconds).ToArray();
    var max = buckets.Max();

    Console.WriteLine($"Average duration = {buckets.Average()}µs");
    Console.WriteLine($"Max duration = {max}µs");

    foreach (var bucket in buckets)
    {
        var length = (int)(bucket / max * 100);
        if (length == 0)
            Console.WriteLine($"||");
        else
            Console.WriteLine($"|{new string('=', length)}|");
    }
}
finally
{
    await SqlServerHelper.DropDatabase();
}