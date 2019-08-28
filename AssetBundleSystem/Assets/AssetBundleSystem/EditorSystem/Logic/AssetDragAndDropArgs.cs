#if UNITY_EDITOR
using UnityEngine;
using System.Collections;
using UnityEditor.IMGUI.Controls;

namespace AssetBundleSystem.Editor
{

    internal struct AssetDragAndDropArgs 
    {
        public enum DragAndDropPosition
        {
            UponItem,
            BetweenItems,
            OutsideItems,
        }

        /// <summary>
        ///   <para>When dragging items the current drag can have the following 3 positions relative to the items: Upon an item, Between two items or Outside items.</para>
        /// </summary>
        public DragAndDropPosition dragAndDropPosition;
        /// <summary>
        ///   <para>The parent item is set if the drag is either upon this item or between two of its children.</para>
        /// </summary>
        public TreeViewItem parentItem;
        /// <summary>
        ///   <para>This index refers to the index in the children list of the parentItem where the current drag is positioned.</para>
        /// </summary>
        public int insertAtIndex;
        /// <summary>
        ///   <para>This value is false as long as the mouse button is down, when the mouse button is released it is true.</para>
        /// </summary>
        public bool performDrop;

    }
}
#endif


