#if UNITY_EDITOR
using UnityEngine;
using System.Collections;
using UnityEditor.IMGUI.Controls;

namespace  AssetBundleSystem.Editor
{
    internal class TreeColumnHeader :MultiColumnHeader
    {
        public TreeColumnHeader(MultiColumnHeaderState state)
            : base(state)
        {
            
        }

    }
}
#endif

