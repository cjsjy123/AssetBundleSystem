using UnityEngine;
using System.Collections.Generic;

namespace CommonUtils
{
    public static class DictionaryPool<T,V>
    {
        // Object pool to avoid allocations.
        private static readonly ObjectPool<Dictionary<T,V>> s_ListPool = new ObjectPool<Dictionary<T,V>>(null, Init);

        static void Init(Dictionary<T,V> dict)
        {
            if(dict != null)
                dict.Clear();
        }

        public static Dictionary<T, V> Get()
        {
            return s_ListPool.Get();
        }

        public static void Release(Dictionary<T, V> toRelease)
        {
            s_ListPool.Release(toRelease);
        }
    }
}

