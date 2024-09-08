namespace UUIDNext.Tools;

/// <summary>
/// A quick and dirty cache
/// </summary>
internal class QDCache<TKey, TValue>(int capacity)
{
#if NET9_OR_GREATER
    private readonly System.Threading.Lock _lock = new();
#else
    private readonly object _lock = new();
#endif  

    // a sorted array of the cache's key/value where the first item is the most recently asked one
    private readonly KeyValue[] _store = new KeyValue[capacity];
    private int _firstAvailableSlot = 0;

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

    public void Set(TKey key, TValue value)
    {
        lock (_lock)
        {
            if (TryFindKey(key, out var index))
            {
                _store[index] = new(key, value);
                UpdateStoreOrder(index);
            }
            else
                AddValue(key, value);
        }
    }

    private void AddValue(TKey key, TValue value)
    {
        // if the store is full, the new item will replace the least recently asked item of the store
        // if not, the new item will be added at the first available slot
        int index = _firstAvailableSlot < capacity ? _firstAvailableSlot++ : capacity - 1;

        _store[index] = new(key, value);
        UpdateStoreOrder(index);
    }

    private bool TryFindKey(TKey key, out int index)
    {
        for (int i = 0; i < _firstAvailableSlot; i++)
        {
            var itemKey = _store[i].Key;
            if ((key == null && itemKey == null) || (key.Equals(itemKey)))
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
        public TKey Key => key;
        public TValue Value => value;
    }
}
