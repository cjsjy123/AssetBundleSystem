#if UNITY_EDITOR
using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace AssetBundleSystem.Editor
{
    internal struct DependencyTreeData : ITreeData
    {
        public int Id { get; set; }

        public string DisplayName { get; set; }

        public string IconName { get; set; }

        public string FilePath { get; set; }

        public EditorAssetBundleInfo EditorInfo { get; set; }

        public bool IsAssetBundleViewData;

        //public List<AssetTreeData> DependOnList;
    }
}
#endif

