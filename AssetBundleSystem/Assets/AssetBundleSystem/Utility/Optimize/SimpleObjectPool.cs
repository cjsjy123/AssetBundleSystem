using System;
using System.Collections.Generic;
using System.Text;

namespace CommonUtils
{
    public class SimpleObjectPool<T> where T:new ()
    {
        private static ObjectPool<T> _pool = new ObjectPool<T>(null,null);

        public static T Get()
        {
            return _pool.Get();
        }

        public static void Release(T data)
        {
            _pool.Release(data);
        }
    }
}
