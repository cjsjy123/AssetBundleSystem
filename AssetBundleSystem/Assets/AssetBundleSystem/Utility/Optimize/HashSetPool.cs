using UnityEngine;
using System.Collections.Generic;

namespace CommonUtils
{
    public static class HashSetPool<T>
    {
        // Object pool to avoid allocations.
        private static readonly ObjectPool<HashSet<T>> s_ListPool = new ObjectPool<HashSet<T>>(null, Init);

        static void Init(HashSet<T> hashSet)
        {
            if(hashSet != null)
                hashSet.Clear();
        }

        public static HashSet<T> Get()
        {
            return s_ListPool.Get();
        }

        public static void Release(HashSet<T> toRelease)
        {
            s_ListPool.Release(toRelease);
        }
    }
}

