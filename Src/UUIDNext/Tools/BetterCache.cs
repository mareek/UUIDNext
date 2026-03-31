namespace UUIDNext.Tools;

/// <summary>
/// A better cache
/// </summary>
internal class BetterCache<TKey, TValue>(int capacity)
    where TKey : notnull
{
#if NET9_OR_GREATER
    private readonly System.Threading.Lock _lock = new();
#else
    private readonly object _lock = new();
#endif  

    private readonly Dictionary<TKey, int> _keysIndex = new(capacity);
    private readonly ListItem[] _items = new ListItem[capacity];
    private int _firstIndex = -1;
    private int _lastIindex = capacity - 1;
    private int _firstAvailbleIndex = capacity - 1;

    public TValue AddOrUpdate(TKey key, Func<TKey, TValue> addValueFactory, Func<TKey, TValue, TValue> updateValueFactory)
    {
        lock (_lock)
        {
            TValue newValue;
            if (_keysIndex.TryGetValue(key, out int index))
            {
                var oldValue = GetValue(index);
                newValue = updateValueFactory(key, oldValue);
                SetValue(index, newValue);
                MoveToTop(index);
            }
            else
            {
                newValue = addValueFactory(key);
                AddOnTop(key, newValue);
            }

            return newValue;
        }
    }

    public TValue GetOrAdd(TKey key, Func<TKey, TValue> factory)
    {
        lock (_lock)
        {
            TValue value;
            if (_keysIndex.TryGetValue(key, out int index))
            {
                value = GetValue(index);
                MoveToTop(index);
            }
            else
            {
                value = factory(key);
                AddOnTop(key, value);
            }

            return value;
        }
    }

    private void AddOnTop(TKey key, TValue value)
    {
        ListItem newItem = new(-1, key, value, _firstIndex);

        // if the cache is not full, we put the new item at the first available index
        if (_firstAvailbleIndex != -1)
        {
            // put the new item at the first avilable index
            var newFirstIndex = _firstAvailbleIndex;
            _items[newFirstIndex] = newItem;

            // if the cache is not empty, move the previous first item to the second place
            if (_firstIndex != -1)
                _items[_firstIndex] = _items[_firstIndex].WithPreviousIndex(newFirstIndex);

            // update the "pointers"
            _firstIndex = newFirstIndex;
            _firstAvailbleIndex--;
        }
        else
        {
            // remove last item from cache
            var itemToRemove = _items[_lastIindex];
            _keysIndex.Remove(itemToRemove.Key);

            // set the penultimate item as the last item
            var newLastIndex = itemToRemove.PreviousIndex;
            _items[newLastIndex] = _items[newLastIndex].WithNextIndex(-1);

            // move the previous first item to the second place
            var newFirstIndex = _lastIindex;
            _items[_firstIndex] = _items[_firstIndex].WithPreviousIndex(newFirstIndex);

            // set the new item as the first item
            _items[newFirstIndex] = newItem;

            // update the "pointers"
            _lastIindex = newLastIndex;
            _firstIndex = newFirstIndex;
        }

        _keysIndex[key] = _firstIndex;
    }

    private TValue GetValue(int index) => _items[index].Value;

    private void SetValue(int index, TValue newValue) => _items[index] = _items[index].WithValue(newValue);

    private void MoveToTop(int index)
    {
        // if the item is already first we've got nothing to do
        if (index == _firstIndex)
            return;

        var item = _items[index];

        // re remove the item from its position in the "chain" of items 
        _items[item.PreviousIndex] = _items[item.PreviousIndex].WithNextIndex(item.NextIndex);
        if (item.NextIndex != -1) // same thing but backward
            _items[item.NextIndex] = _items[item.NextIndex].WithPreviousIndex(item.PreviousIndex);

        // move the previous first item to the second place
        _items[_firstIndex] = _items[_firstIndex].WithPreviousIndex(index);

        // we put the item at the first place
        _items[index] = new(-1, item.Key, item.Value, _firstIndex);

        // update the "pointers"
        _firstIndex = index;
        if (index == _lastIindex)
            _lastIindex = item.PreviousIndex;
    }

    private readonly struct ListItem(int previousIndex, TKey key, TValue value, int nextIndex)
    {
        public int PreviousIndex { get; } = previousIndex;
        public TKey Key { get; } = key;
        public TValue Value { get; } = value;
        public int NextIndex { get; } = nextIndex;

        public ListItem WithPreviousIndex(int index) => new(index, Key, Value, NextIndex);
        public ListItem WithNextIndex(int index) => new(PreviousIndex, Key, Value, index);
        public ListItem WithValue(TValue value) => new(PreviousIndex, Key, value, NextIndex);
    }
}
