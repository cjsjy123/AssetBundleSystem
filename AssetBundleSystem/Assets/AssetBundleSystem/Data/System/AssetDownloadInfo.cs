using UnityEngine;
using System.Collections;
using System;

namespace AssetBundleSystem
{
    internal struct AssetDownloadInfo:IEquatable<AssetDownloadInfo>
    {
        public string AssetPath;

        public string AssetBundleName;

        public string DstPath;

        public long TaskId;

        public string Url;

        public bool IsDownloading;

        public bool IsDone;

        public float Progress;

        public bool HasError;

        public IDownloader Loader;

        public bool Equals(AssetDownloadInfo other)
        {
            if(!AssetBundleName.Equals(other.AssetBundleName))
            {
                return false;
            }

            return true;
        }

        public override int GetHashCode()
        {
            return AssetBundleName.GetHashCode();
        }
    }
}

