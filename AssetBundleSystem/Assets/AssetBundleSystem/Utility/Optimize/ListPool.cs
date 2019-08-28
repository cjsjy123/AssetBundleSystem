using System;
using System.Collections.Generic;
using UnityEngine;

namespace CommonUtils
{
    /// <summary>
    /// UGUI POOL
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public static class ListPool<T>
    {
        // Object pool to avoid allocations.
        private static readonly ObjectPool<List<T>> s_ListPool = new ObjectPool<List<T>>(null, Init);

        static void Init(List<T> list)
        {
            if(list != null)
                list.Clear();
        }

        public static List<T> Get()
        {
            return s_ListPool.Get();
        }

        public static void Release(List<T> toRelease)
        {
            s_ListPool.Release(toRelease);
        }
    }
}
