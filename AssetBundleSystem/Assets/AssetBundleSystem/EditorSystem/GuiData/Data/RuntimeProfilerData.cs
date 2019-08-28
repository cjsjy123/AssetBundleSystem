#if UNITY_EDITOR
using UnityEngine;
using System.Collections;


namespace AssetBundleSystem.Editor
{
    internal struct RuntimeProfilerData :ITreeData
    {
        public int Id { get; set; }

        public string DisplayName { get; set; }

        public string IconName { get; set; }

        public string FilePath { get; set; }

        public EditorAssetBundleInfo EditorInfo { get; set; }

        public FrameProfilerData? ProfilerData;

        public AssetBundleTask? Task;

        public AssetBundleHistory? History;

        public AssetBundleHistory.AssetInfo? AssetInfo;

        public bool IsTaskItem()
        {
            return Task != null;
        }

        public bool IsGameAssetBundleItem()
        {
            return History != null;
        }

        public bool IsAssetInfoItem()
        {
            return AssetInfo != null;
        }
    }
}

#endif
