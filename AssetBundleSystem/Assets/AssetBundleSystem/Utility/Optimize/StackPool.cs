using System;
using System.Collections.Generic;
using UnityEngine;

namespace CommonUtils
{
    /// <summary>
    /// UGUI POOL
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public static class StackPool<T>
    {
        // Object pool to avoid allocations.
        private static readonly ObjectPool<Stack<T>> s_stackPool = new ObjectPool<Stack<T>>(null, Init);

        static void Init(Stack<T> stack)
        {
            if(stack != null)
                stack.Clear();
        }

        public static Stack<T> Get()
        {
            return s_stackPool.Get();
        }

        public static void Release(Stack<T> toRelease)
        {
            s_stackPool.Release(toRelease);
        }
    }
}
