public static class ObjectPoolExtensions
{
    public static void Preload<T>(this UnityEngine.Pool.IObjectPool<T> pool, int amount) where T : class
    {
        var preloadObjects = System.Buffers.ArrayPool<T>.Shared.Rent(amount);
        for (int i = 0; i < amount; i++)
        {
            preloadObjects[i] = pool.Get();
        }
        for (int i = 0; i < amount; i++)
        {
            pool.Release(preloadObjects[i]);
        }
        System.Buffers.ArrayPool<T>.Shared.Return(preloadObjects, true);
    }
}