#if UNITY_EDITOR
using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEditor.IMGUI.Controls;

namespace AssetBundleSystem.Editor
{
    internal interface ILogicInterface
    {
        AssetWindow.WindowMode TypeMode { get; }

        bool Inited { get; }

        void Clear();

        IEnumerator ReLoad();
    }


    internal interface IAssetGuiInterface
    {
        AssetWindow.WindowMode TypeMode { get; }

        void Init();

        void RefreshGuiInfo(EditorContexts contexts);

        void GuiUpdate(bool isrunning,float delta);

        void RenderGui();

        void Destory();
    }

    internal interface IGuiTree: IAssetGuiInterface
    {
        void SelectionChanged(IList<TreeViewItem> selectedIds);

        List<string> Refresh();

        void RefreshHead();

        void Rebuild();

        #region treeview

        void Sort(MultiColumnHeader header, TreeViewItem root, IList<TreeViewItem> items);

        bool CellGui(TreeViewItem item, ref AssetRowGuiArgs args);

        #endregion

        #region Drag

        void CanDrag(ref bool retval);

        void Drag(ref AssetDragAndDropArgs dragargs, ref DragAndDropVisualMode dragmd);

        #endregion
    }
}

#endif