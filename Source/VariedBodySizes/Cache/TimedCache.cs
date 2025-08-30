namespace VariedBodySizes;

public class TimedCache<T>(int expiryTime)
{
    private readonly Dictionary<int, CacheEntry<T>> internalCache = new();

    // ReSharper disable once ChangeFieldTypeToSystemThreadingLock
    private readonly object sync = new();

    public void Set(Pawn pawn, T value)
    {
        var key = pawn.thingIDNumber;
        lock (sync)
        {
            internalCache[key] = new CacheEntry<T>(value);
        }
    }

    public void Clear()
    {
        lock (sync)
        {
            internalCache.Clear();
        }
    }

    public void Remove(Pawn pawn)
    {
        var key = pawn.thingIDNumber;
        lock (sync)
        {
            internalCache.Remove(key);
        }
    }

    public bool Contains(Pawn pawn)
    {
        var key = pawn.thingIDNumber;
        lock (sync)
        {
            return internalCache.ContainsKey(key);
        }
    }

    public bool TryGet(Pawn pawn, out T value)
    {
        var key = pawn.thingIDNumber;
        lock (sync)
        {
            if (internalCache.TryGetValue(key, out var entry))
            {
                if (entry.Expired(expiryTime))
                {
                    internalCache.Remove(key);
                    value = default;
                    return false;
                }

                value = entry.CachedValue;
                return true;
            }

            value = default;
            return false;
        }
    }

    public T Get(Pawn pawn)
    {
        var key = pawn.thingIDNumber;
        lock (sync)
        {
            return internalCache[key].CachedValue;
        }
    }
}