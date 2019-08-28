#if UNITY_EDITOR
using UnityEngine;
using System.Collections;

namespace AssetBundleSystem.Editor
{

    /// <summary>
    /// Asset View 结构
    /// </summary>
    internal struct AssetTreeData : ITreeData
    {
        public int Id { get; set; }

        public string DisplayName { get; set; }

        public string IconName { get; set; }

        public string FilePath { get; set; }

        public EditorAssetBundleInfo EditorInfo { get; set; }

        public bool IsAssetBundleViewData;

        public override string ToString()
        {
            return string.Format("ATD FilePath:{0} AssetBundleName:{1}",FilePath,EditorInfo.RuntimeInfo.AssetBundleName);
        }
    }
}

#endif