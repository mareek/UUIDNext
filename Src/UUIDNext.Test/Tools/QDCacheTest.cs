using NFluent;
using UUIDNext.Tools;
using Xunit;

namespace UUIDNext.Test.Tools;

public class QDCacheTest
{
    [Fact]
    public void EnsureBasicCacheWorks()
    {
        int factoryCount = 0;
        QDCache<string, int> cache = new(5);

        for (int i = 0; i < 10; i++)
        {
            var first = cache.GetOrAdd("first", _ => ++factoryCount);
            Check.That(first).Is(1);
            var second = cache.GetOrAdd("second", _ => ++factoryCount);
            Check.That(second).Is(2);
        }
    }

    [Fact]
    public void EnsureCacheWorksWithOverflow()
    {
        int factoryCount = 0;
        QDCache<string, int> cache = new(5);

        for (int i = 0; i < 10; i++)
        {
            var other = cache.GetOrAdd($"{i}", _ => ++factoryCount);
            Check.That(other).Is(factoryCount);
            var second = cache.GetOrAdd("second", _ => ++factoryCount);
            Check.That(second).Is(2);
        }

        Check.That(factoryCount).Is(11);
    }

    [Fact]
    public void EnsureSetMethodWorks()
    {
        int factoryCount = 0;
        QDCache<string, int> cache = new(5);

        cache.Set("first", -1);
        var first = cache.GetOrAdd("first", _ => ++factoryCount);
        Check.That(first).Is(-1);

        cache.Set("second", -2);
        var second = cache.GetOrAdd("second", _ => ++factoryCount);
        Check.That(second).Is(-2);


        for (int i = 0; i < 10; i++)
        {
            var other = cache.GetOrAdd($"{i}", _ => ++factoryCount);
            Check.That(other).Is(factoryCount);
            cache.Set("first", 0); // this line keeps "first" in the cache
        }

        // "first" is still in the cache
        first = cache.GetOrAdd("first", _ => ++factoryCount);
        Check.That(first).Is(0);

        // "second" has been driven out of the cache by the for loop
        second = cache.GetOrAdd("second", _ => ++factoryCount);
        Check.That(second).Is(factoryCount);

        Check.That(factoryCount).Is(11);
    }
}
