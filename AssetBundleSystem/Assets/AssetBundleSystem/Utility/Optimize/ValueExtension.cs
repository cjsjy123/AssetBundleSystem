using System;
using UnityEngine;
using System.Collections;

namespace CommonUtils
{
    public static class ValueExtension 
    {
        public static bool GenericValueEqual<T>(this T left, T right)
        {
            if (typeof(T).IsClass)
            {
                if (left == null && right != null)
                {
                    return false;
                }
                else if (left != null && right == null)
                {
                    return false;
                }
                else if (left == null && right == null)
                {
                    return true;
                }
            }

            return left.GetHashCode() == right.GetHashCode();
        }

        public static bool SimpleEqual(this float left, float right)
        {
            return Mathf.Abs(left - right) < float.Epsilon;
        }
        public static bool SimpleEqual(this double left, double right)
        {
            return Math.Abs(left - right) < double.Epsilon;
        }

        public static bool SimpleEqual(this int left, int right)
        {
            return left == right;
        }

        public static bool SimpleEqual(this uint left, uint right)
        {
            return left == right;
        }

        public static bool SimpleEqual(this short left, short right)
        {
            return left == right;
        }

        public static bool SimpleEqual(this ushort left, ushort right)
        {
            return left == right;
        }

        public static bool SimpleEqual(this long left, long right)
        {
            return left == right;
        }

        public static bool SimpleEqual(this ulong left, ulong right)
        {
            return left == right;
        }

        public static bool SimpleEqual(this byte left, byte right)
        {
            return left == right;
        }


        public static bool SimpleEqual(this sbyte left, sbyte right)
        {
            return left == right;
        }

        public static bool SimpleEqual(this char left, char right)
        {
            return left == right;
        }

        public static bool SimpleEqual(this bool left, bool right)
        {
            return left == right;
        }

        public static bool SimpleEqual(this string left, string right)
        {
            bool ln = string.IsNullOrEmpty(left);
            bool rn = string.IsNullOrEmpty(right);
            if (ln == rn && ln)
            {
                return true;
            }
            else if (ln == rn )
            {
                return left.Equals(right);
            }
            return false;
        }
    }
}


