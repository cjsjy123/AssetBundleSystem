using System;


namespace AssetBundleSystem
{

    internal struct AssetBundleInfo:IEquatable<AssetBundleInfo>
    {
        /// <summary>
        /// hash
        /// </summary>
        public string HashCode;
        /// <summary>
        /// crc
        /// </summary>
        public uint Crc;
        /// <summary>
        /// 在unity中的路径
        /// </summary>
        public string UnityPath;
        /// <summary>
        /// assetbundle Name
        /// </summary>
        public string AssetBundleName;

        /// <summary>
        /// Asset Name
        /// </summary>
        public string AssetName;
        /// <summary>
        /// 资源大小
        /// </summary>
        public long AssetSize;
        /// <summary>
        /// 文件大小
        /// </summary>
        public long FileSize;
        /// <summary>
        /// 资源类型
        /// </summary>
        public AssetBundleResType AssetResType;
        /// <summary>
        /// 依赖路径集合
        /// </summary>
        public string[] DependNames;
        /// <summary>
        /// assetbundlename 集合
        /// </summary>
        public string[] DepAssetBundleNames;

        public int DependenciesCnt
        {
            get
            {
                if (DependNames == null)
                {
                    return 0;
                }
                return DependNames.Length;
            }
        }

        public override string ToString()
        {
            return string.Format("AssetBundleName :{0} Path:{1} Type:{2}",AssetBundleName,UnityPath,AssetResType);
        }


        public bool Equals(AssetBundleInfo other)
        {
            return string.Equals(HashCode, other.HashCode) && Crc == other.Crc && string.Equals(UnityPath, other.UnityPath) && string.Equals(AssetBundleName, other.AssetBundleName) && string.Equals(AssetName, other.AssetName) && AssetSize == other.AssetSize && FileSize == other.FileSize && AssetResType == other.AssetResType && Equals(DependNames, other.DependNames) && Equals(DepAssetBundleNames, other.DepAssetBundleNames);
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            return obj is AssetBundleInfo && Equals((AssetBundleInfo) obj);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                var hashCode = (HashCode != null ? HashCode.GetHashCode() : 0);
                hashCode = (hashCode * 397) ^ (int) Crc;
                hashCode = (hashCode * 397) ^ (UnityPath != null ? UnityPath.GetHashCode() : 0);
                hashCode = (hashCode * 397) ^ (AssetBundleName != null ? AssetBundleName.GetHashCode() : 0);
                hashCode = (hashCode * 397) ^ (AssetName != null ? AssetName.GetHashCode() : 0);
                hashCode = (hashCode * 397) ^ AssetSize.GetHashCode();
                hashCode = (hashCode * 397) ^ FileSize.GetHashCode();
                hashCode = (hashCode * 397) ^ (int) AssetResType;
                hashCode = (hashCode * 397) ^ (DependNames != null ? DependNames.GetHashCode() : 0);
                hashCode = (hashCode * 397) ^ (DepAssetBundleNames != null ? DepAssetBundleNames.GetHashCode() : 0);
                return hashCode;
            }
        }
    }

}

