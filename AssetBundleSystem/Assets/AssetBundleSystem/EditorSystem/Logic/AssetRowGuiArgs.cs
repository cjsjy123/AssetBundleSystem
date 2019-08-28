#if UNITY_EDITOR
using UnityEngine;
using System.Collections;
using UnityEditor.IMGUI.Controls;

namespace AssetBundleSystem.Editor
{
    /// <summary>
    /// Copy From RowGUIArgs
    /// </summary>
    internal struct AssetRowGuiArgs 
    {
        /// <summary>
        ///   <para>Item for the current row being handled in TreeView.RowGUI.</para>
        /// </summary>
        public TreeViewItem item;
        /// <summary>
        ///   <para>Label used for text rendering of the item displayName. Note this is an empty string when isRenaming == true.</para>
        /// </summary>
        public string label;
        /// <summary>
        ///   <para>Row rect for the current row being handled.</para>
        /// </summary>
        public Rect rowRect;
        /// <summary>
        ///   <para>Row index into the list of current rows.</para>
        /// </summary>
        public int row;
        /// <summary>
        ///   <para>This value is true when the current row's item is part of the current selection.</para>
        /// </summary>
        public bool selected;
        /// <summary>
        ///   <para>This value is true only when the TreeView has keyboard focus and the TreeView's window has focus.</para>
        /// </summary>
        public bool focused;
        /// <summary>
        ///   <para>This value is true when the ::item is currently being renamed.</para>
        /// </summary>
        public bool isRenaming;

        public Rect CellRect;

        public int Column;

    }
}


#endif