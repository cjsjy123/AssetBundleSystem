#if UNITY_EDITOR
using UnityEngine;
using System.Collections;

namespace AssetBundleSystem.Editor
{
    /// <summary>
    /// 冗余结构
    /// </summary>
    internal struct RedundancyData : ITreeData
    {
        public int Id { get; set; }

        public string DisplayName { get; set; }

        public string IconName { get; set; }

        public string FilePath { get; set; }

        public EditorAssetBundleInfo EditorInfo { get; set; }
    }
}
#endif

