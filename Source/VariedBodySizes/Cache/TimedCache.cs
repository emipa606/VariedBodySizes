namespace VariedBodySizes;

public class TimedCache<T>(int expiryTime)
{
    private readonly Dictionary<int, CacheEntry<T>> internalCache = new();

    public void Set(Pawn pawn, T value)
    {
        internalCache[pawn.thingIDNumber] = new CacheEntry<T>(value);
    }

    public void Clear()
    {
        internalCache.Clear();
    }

    public void Remove(Pawn pawn)
    {
        internalCache.Remove(pawn.thingIDNumber);
    }

    public bool Contains(Pawn pawn)
    {
        return internalCache.ContainsKey(pawn.thingIDNumber);
    }

    public bool TryGet(Pawn pawn, out T value)
    {
        if (internalCache.TryGetValue(pawn.thingIDNumber, out var entry))
        {
            if (entry.Expired(expiryTime))
            {
                internalCache.Remove(pawn.thingIDNumber);
                value = default;
                return false;
            }

            value = entry.CachedValue;
            return true;
        }

        value = default;
        return false;
    }

    public T Get(Pawn pawn)
    {
        return internalCache[pawn.thingIDNumber].CachedValue;
    }
}