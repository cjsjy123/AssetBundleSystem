#if UNITY_EDITOR
using System;
using UnityEngine;
using System.Collections;

namespace AssetBundleSystem.Editor
{

    internal struct AssetBundleBuildInfo:IEquatable<AssetBundleBuildInfo>
    {
        public string AssetBundleHash;

        public uint Crc;

        public bool Equals(AssetBundleBuildInfo other)
        {
            return string.Equals(AssetBundleHash, other.AssetBundleHash) && Crc == other.Crc;
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            return obj is AssetBundleBuildInfo && Equals((AssetBundleBuildInfo) obj);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                var hashCode = (AssetBundleHash != null ? AssetBundleHash.GetHashCode() : 0);
                hashCode = (hashCode * 397) ^ (int) Crc;
                return hashCode;
            }
        }
    }
}
#endif

