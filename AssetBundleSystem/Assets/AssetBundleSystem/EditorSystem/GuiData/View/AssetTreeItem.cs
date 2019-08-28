#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.IMGUI.Controls;

namespace AssetBundleSystem.Editor
{

    internal class AssetTreeItem<T> : TreeViewItem where T:ITreeData
    {
        private T _data;

        public int DataId
        {
            get { return _data.Id; }
        }

        public AssetTreeItem(ref T data, int depth) : base(data.Id, depth, data.DisplayName)
        {
            _data = data;
            
            icon = EditorGUIUtility.FindTexture(data.IconName);
        }

        public T GetData()
        {
            return _data;
        }

        internal void SetData(T data)
        {
            _data = data;
        }
    }
}


#endif