#if UNITY_EDITOR
using UnityEngine;
using System.Collections;

namespace AssetBundleSystem.Editor
{

    internal struct BuildData : ITreeData
    {
        public int Id { get;  set; }

        public int ViewId { get; set; }

        public string DisplayName { get; set; }

        public string IconName { get; set; }

        public string FilePath { get; set; }

        public EditorAssetBundleInfo EditorInfo { get; set; }
    }

}
#endif

