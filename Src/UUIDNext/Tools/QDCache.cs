namespace UUIDNext.Tools;

/// <summary>
/// A quick and dirty cache
/// </summary>
internal class QDCache<TKey, TValue>(int capacity)
{
    private readonly Lock _lock = LockFactory.Create();

    // a sorted array of the cache's key/value where the first item is the most recently asked one
    private readonly KeyValue[] _store = new KeyValue[capacity];
    private int _firstAvailableSlot = 0;

    public TValue AddOrUpdate(TKey key, Func<TKey, TValue> addValueFactory, Func<TKey, TValue, TValue> updateValueFactory)
    {
        lock (_lock)
        {
            TValue value;
            if (TryFindKey(key, out var index))
            {
                value = updateValueFactory(key, _store[index].Value);
                UpdateStore(index, key, value);
            }
            else
            {
                value = addValueFactory(key);
                AddValue(key, value);
            }

            return value;
        }
    }

    public TValue GetOrAdd(TKey key, Func<TKey, TValue> factory)
    {
        lock (_lock)
        {
            if (TryFindKey(key, out var index))
                UpdateStoreOrder(index);
            else
                AddValue(key, factory(key));

            return _store[0].Value;
        }
    }

    private void AddValue(TKey key, TValue value)
    {
        // if the store is full, the new item will replace the least recently asked item of the store
        // if not, the new item will be added at the first available slot
        int index = _firstAvailableSlot < capacity ? _firstAvailableSlot++ : capacity - 1;

        UpdateStore(index, key, value);
    }

    private void UpdateStore(int index, TKey key, TValue value)
    {
        _store[index] = new(key, value);
        UpdateStoreOrder(index);
    }

    private bool TryFindKey(TKey key, out int index)
    {
        for (int i = 0; i < _firstAvailableSlot; i++)
        {
            if (EqualityComparer<TKey>.Default.Equals(key, _store[i].Key))
            {
                index = i;
                return true;
            }
        }

        index = 0;
        return false;
    }

    private void UpdateStoreOrder(int latestAskedIndex)
    {
        // bubble up the asked item to the most recently asked item position
        for (int i = latestAskedIndex; i > 0; i--)
        {
            var temp = _store[i - 1];
            _store[i - 1] = _store[i];
            _store[i] = temp;
        }
    }

    private struct KeyValue(TKey key, TValue value)
    {
        public TKey Key { get; } = key;
        public TValue Value { get; } = value;
    }
}
