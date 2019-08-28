using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

namespace CommonUtils
{
    [Serializable]
    public abstract  class BaseFrDictionary<TK, TV> 
    {
        [SerializeField]
        protected List<TK> _keys;
        [SerializeField]
        protected List<TV> _values;

        public TV this[TK key]
        {
            get
            {
                TV v;
                TryGetValue(key, out v);
                return v;
            }
            set
            {
                try
                {
                    int index = _keys.IndexOf(key);
                    if (index == -1)
                    {
                        _values.Add(value);
                        _keys.Add(key);
                    }
                    else
                    {
                        _values[index] = value;
                    }
                }
                catch (Exception ex)
                {
                    Debug.LogException(ex);
                }

            }
        }

        public TK this[TV key]
        {
            get
            {
                TK v;
                TryGetKey(key, out v);
                return v;
            }
            set
            {
                try
                {
                    int index = _values.IndexOf(key);
                    if (index == -1)
                    {
                        _values.Add(key);
                        _keys.Add(value);
                    }
                    else
                    {
                        _keys[index] = value;
                    }
                }
                catch (Exception ex)
                {
                    Debug.LogException(ex);
                }

            }
        }

        public int Count
        {
            get
            {
                return _keys.Count;
            }
        }
        /// <summary>
        /// low gc 获取的是原始对象
        /// </summary>
        public List<TK> ReadOnlyKeys
        {
            get
            {
                return _keys;
            }
        }
        /// <summary>
        /// low gc 获取的是原始对象
        /// </summary>
        public List<TV> ReadOnlyValues
        {
            get
            {
                return _values;
            }
        }

        public List<TK> Keys
        {
            get
            {
                List<TK> list = ListPool<TK>.Get();
                list.AddRange(_keys);
                return list;
            }
        }

        public List<TV> Values
        {
            get
            {
                List<TV> list = ListPool<TV>.Get();
                list.AddRange(_values);
                return list;
            }
        }

        public void Clear()
        {
            _values.Clear();
            _keys.Clear();
        }

        public List<TV> FindAll(Predicate<TK> p)
        {
            List<TV> values = ListPool<TV>.Get();
            List<TK> keys = _keys.FindAll(p);
            for (int i = 0; i < keys.Count; ++i)
            {
                values.Add(this[keys[i]]);
            }

            return values;
        }

        public bool RemoveAll(Predicate<TK> p)
        {
            List<TK> keys = _keys.FindAll(p);
            if (keys.Count == 0)
                return false;

            for (int i = 0; i < keys.Count; ++i)
            {
                TK k = keys[i];
                _values.Remove(this[k]);
                keys.Remove(k);
            }

            return true;
        }

        public bool TryGetValue(TK key, out TV v)
        {
            try
            {
                int index = _keys.IndexOf(key);
                if (index == -1)
                {
                    v = default(TV);
                    return false;
                }

                v = _values[index];
                return true;
            }
            catch (Exception ex)
            {
                Debug.LogException(ex);
                v = default(TV);
                return false;
            }
        }

        public bool TryGetKey(TV key, out TK v)
        {
            try
            {
                int index = _values.IndexOf(key);
                if (index == -1)
                {
                    v = default(TK);
                    return false;
                }

                v = _keys[index];
                return true;
            }
            catch (Exception ex)
            {
                Debug.LogException(ex);
                v = default(TK);
                return false;
            }
        }

        public void Add(TK key, TV value)
        {
            this[key] = value;
        }

        public bool Remove(TK key)
        {
            int index = _keys.IndexOf(key);
            if (index != -1)
            {
                _keys.RemoveAt(index);
                _values.RemoveAt(index);

                return true;
            }
            return false;
        }

        public bool RemoveValue(TV value)
        {
            int index = _values.IndexOf(value);
            if (index != -1)
            {
                _keys.RemoveAt(index);
                _values.RemoveAt(index);
                return true;
            }
            return false;
        }
    }
}
