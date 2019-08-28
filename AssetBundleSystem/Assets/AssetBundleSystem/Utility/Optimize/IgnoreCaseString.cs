using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

namespace CommonUtils
{
    /// <summary>
    /// 忽略大小写的封装string struct *****暂时不想开放隐式转换，开了可能某些需要大小写敏感的地方会有使用错误，应该让开发者自主控制
    /// </summary>
    public struct IgnoreCaseString :IEquatable<IgnoreCaseString>
    {
        public readonly string Info;

        public IgnoreCaseString(string val)
        {
            Info = val;
        }

        public override string ToString()
        {
            return Info;
        }

        public static bool CompareString(string left, string right)
        {
            return string.Equals(left, right, StringComparison.OrdinalIgnoreCase);
        }

        public bool Equals(IgnoreCaseString other)
        {
            if (string.Equals(Info, other.Info, StringComparison.OrdinalIgnoreCase))
            {
                return true;
            }
            return false;
        }

        public override bool Equals(object obj)
        {
            return this.Info.Equals(obj);
        }

        public override int GetHashCode()
        {
            return StringComparer.OrdinalIgnoreCase.GetHashCode(Info);
        }

        public static implicit operator string(IgnoreCaseString left)
        {
            return left.Info;
        }

        public static implicit operator IgnoreCaseString(string left)
        {
            return new IgnoreCaseString(left);
        }

        //public static bool operator ==(IgnoreCaseString left, IgnoreCaseString right)
        //{
        //    return left.Equals(right);
        //}

        //public static bool operator !=(IgnoreCaseString left, IgnoreCaseString right)
        //{
        //    return !left.Equals(right);
        //}

        //public static bool operator ==(IgnoreCaseString left, string right)
        //{
        //    return left.Equals(right);
        //}

        //public static bool operator !=(IgnoreCaseString left, string right)
        //{
        //    return !left.Equals(right);
        //}

        //public static bool operator ==(string left, IgnoreCaseString right)
        //{
        //    return left.Equals(right.Info);
        //}

        //public static bool operator !=(string left, IgnoreCaseString right)
        //{
        //    return !left.Equals(right.Info);
        //}
    }
}

