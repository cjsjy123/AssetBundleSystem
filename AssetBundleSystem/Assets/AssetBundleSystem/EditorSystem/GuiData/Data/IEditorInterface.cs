#if UNITY_EDITOR
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

namespace AssetBundleSystem.Editor
{
    internal interface ITreeData
    {
        int Id { get; }

        string DisplayName { get; }

        string IconName { get; }

        string FilePath { get; }

        EditorAssetBundleInfo EditorInfo { get; set; }
    }


    internal interface ITreeModel
    {

    }
}
#endif
