using System;
using System.Collections.Generic;
using UnityEngine;

namespace CommonUtils
{
    /// <summary>
    /// UGUI POOL
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public static class QueuePool<T>
    {
        // Object pool to avoid allocations.
        private static readonly ObjectPool<Queue<T>> s_stackPool = new ObjectPool<Queue<T>>(null, Init);

        static void Init(Queue<T> stack)
        {
            if(stack != null)
                stack.Clear();
        }

        public static Queue<T> Get()
        {
            return s_stackPool.Get();
        }

        public static void Release(Queue<T> toRelease)
        {
            s_stackPool.Release(toRelease);
        }
    }
}
