using UnityEngine;
using System.Collections.Generic;
using System.Text;

namespace CommonUtils
{
    public static class StringBuilderCache
    {
        // Object pool to avoid allocations.
        private static readonly ObjectPool<StringBuilder> s_sbpool = new ObjectPool<StringBuilder>(null, Init);

        static void Init(StringBuilder stringBuilder)
        {
            if (stringBuilder != null)
                stringBuilder.Length = 0;
        }

        public static StringBuilder Get()
        {
            return s_sbpool.Get();
        }

        public static void Release(StringBuilder toRelease)
        {
            s_sbpool.Release(toRelease);
        }
    }
}

