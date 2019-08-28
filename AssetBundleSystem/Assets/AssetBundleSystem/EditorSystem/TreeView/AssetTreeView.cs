#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using CommonUtils;
using UnityEngine;
using UnityEditor;
using UnityEditor.IMGUI.Controls;
using UnityEditor.TreeViewExamples;

namespace AssetBundleSystem.Editor
{
    internal class AssetTreeView<T> : TreeView where T:ITreeData
    {
        private readonly  List<TreeViewItem> _rows = new List<TreeViewItem>(64);
        private readonly AssetWindow.WindowMode _viewMode;
        private readonly AssetTreeModel<T> _model;

        public AssetTreeView(TreeViewState state, MultiColumnHeader header, AssetWindow.WindowMode md) : base(state,header)
        {
            header.sortingChanged += OnSortingChanged;
            _viewMode = md;
            _model = AssetTreeManager.mIns.GetModel<T>();
            //Reload();
        }

        protected override TreeViewItem BuildRoot()
        {
            var rootdata = _model.Root;
           
            return new AssetTreeItem<T>(ref rootdata, -1);
        }

        protected override IList<TreeViewItem> BuildRows(TreeViewItem rootitem)
        {
            var selections = GetSelection();

            _rows.Clear();

            AssetTreeItem<T> root = rootitem as AssetTreeItem<T>;
            if (root != null)
            {
                if (!string.IsNullOrEmpty(searchString))
                {
                    SearchItem(_rows, _model, root.DataId);
                }
                else
                {
                    TryAddChildren(root, _rows, _model, -1);
                }

                SetupParentsAndChildrenFromDepths(rootitem, _rows);

                var render = AssetTreeManager.mIns.GetGuiRender(EditorContexts.mIns.Mode);
                if (render != null)
                {
                    foreach (var subrender in render)
                    {
                        if (subrender.TypeMode == _viewMode)
                        {
                            IGuiTree treeRender = subrender as IGuiTree;
                            if (treeRender != null)
                            {
                                treeRender.Rebuild();
                            }
                        }
                    }
                }
            }

            if (selections != null && selections.Count > 0)
            {
                FrameItem(selections[0]);
            }

            return _rows;
        }

        void SearchItem(List<TreeViewItem> list,AssetTreeModel<T> model,int itemid)
        {
            if (list != null && model != null &&  model.HasChildren(itemid))
            {
                List<T> children = model.GetChildren(itemid);
                Stack<T> stack = StackPool<T>.Get();
      
                for (int i = children.Count - 1; i >= 0; i--)
                {
                    stack.Push(children[i]);
                }

                ListPool<T>.Release(children);
                while (stack.Count > 0)
                {
                    T current = stack.Pop();

                    if (!string.IsNullOrEmpty(current.DisplayName) && current.DisplayName.IndexOf(searchString, StringComparison.OrdinalIgnoreCase) >= 0)
                    {
                        //Add result
                        var viewitem =  CreateItem( ref current, 0);
                        list.Add(viewitem);
                    }

                    if (model.HasChildren(current.Id))
                    {
                        children = model.GetChildren(current.Id);
                        for (int i = children.Count - 1; i >= 0; i--)
                        {
                            stack.Push(children[i]);
                        }

                        ListPool<T>.Release(children);
                    }
                }

                StackPool<T>.Release(stack);
            }
        }

        void TryAddChildren(AssetTreeItem<T> rootitem,List<TreeViewItem> list,AssetTreeModel<T> model, int depth)
        {
            if (model.HasChildren(rootitem.DataId))
            {
                List<T> children = model.GetChildren(rootitem.DataId);
                Stack<AssetTreeItem<T>> stack = StackPool<AssetTreeItem<T>>.Get();

                for (int i = children.Count - 1; i >= 0; i--)
                {
                    var child = children[i];
                    
                    //create item
                    var childItem = CreateItem( ref child, depth + 1);
                    stack.Push(childItem);
                }

                ListPool<T>.Release(children);

                while (stack.Count >0)
                {
                    var stackChild = stack.Pop();
                    list.Add(stackChild);
                    if (model.HasChildren(stackChild.DataId))
                    {
                        if (IsExpanded(stackChild.id))
                        {
                            children = model.GetChildren(stackChild.DataId);
                            //
                            //stackChild.children = new List<TreeViewItem>();
                            for (int i = children.Count - 1; i >= 0; i--)
                            {
                                var child = children[i];

                                //create item
                                var childItem = CreateItem(ref child, stackChild.depth+1);
                                stack.Push(childItem);
                                //stackChild.children.Add(childItem);
                            }

                            ListPool<T>.Release(children);
                        }
                        else
                        {
                            stackChild.children = CreateChildListForCollapsedParent();
                        }
                    }
                }

                StackPool<AssetTreeItem<T>>.Release(stack);
            }
        }

        AssetTreeItem<T> CreateItem( ref T data, int depth)
        {
            AssetTreeItem<T> viewItem = new AssetTreeItem<T>(ref data, depth );
            return viewItem;
        }

        protected override IList<int> GetAncestors(int id)
        {
            return _model.GetAncestors(id);
        }

        protected override IList<int> GetDescendantsThatHaveChildren(int id)
        {
            return _model.GetDescendantsThatHaveChildren(id);
        }

        protected override void ExpandedStateChanged()
        {

            base.ExpandedStateChanged();
        }

        protected override void SelectionChanged(IList<int> selectedIds)
        {

            if (selectedIds.Count > 0 )
            {
                FrameItem(selectedIds[0]);

                var render = AssetTreeManager.mIns.GetGuiRender(EditorContexts.mIns.Mode);
                if (render != null)
                {
                    List<TreeViewItem> itemList = ListPool<TreeViewItem>.Get();
                    var rows = GetRows();
                    for (int i = 0; i < rows.Count; i++)
                    {
                        var rowdata = rows[i] as AssetTreeItem<T>;
                        if (rowdata != null && selectedIds.Contains(rowdata.id))
                        {
                            itemList.Add(rowdata);
                        }
                    }

                    foreach (var subrender in render)
                    {
                        if (subrender.TypeMode == _viewMode)
                        {
                            IGuiTree treeRender = subrender as IGuiTree;
                            if (treeRender != null)
                            {
                                treeRender.SelectionChanged(itemList);
                            }
                        }
                            
                    }

                    //is searching
                    if (!string.IsNullOrEmpty(searchString))
                    {
                        IList<int> expandlist = GetExpanded();
                        if (expandlist != null)
                        {
                            List<int> list = new List<int>();
                            list.AddRange(expandlist);

                            Stack<int> stack = StackPool<int>.Get();
                            foreach (var item in itemList)
                            {
                                stack.Push(item.id);
      
                            }

                            while (stack.Count >0)
                            {
                                var itemid = stack.Pop();
                                if (!list.Contains(itemid))
                                {
                                    list.Add(itemid);
                                    if (_model.HasParent(itemid) )
                                    {
                                        var parents = _model.GetParent(itemid);
                                        foreach (var parent in parents)
                                        {
                                            if (!list.Contains(parent.Id) && parent.Id != _model.Root.Id)
                                                stack.Push(parent.Id);
                                        }

                                        ListPool<T>.Release(parents);
                                    }
                                }
                            }
                            StackPool<int>.Release(stack);
                            SetExpanded(list);
                        } 
                    }

                    ListPool<TreeViewItem>.Release(itemList);
                   
                }
                else
                {
                    base.SelectionChanged(selectedIds);
                }

                
            }
        }

        protected override bool CanStartDrag(CanStartDragArgs args)
        {
            var render = AssetTreeManager.mIns.GetGuiRender(EditorContexts.mIns.Mode);
            if (render != null)
            {
                bool ret = false;
                foreach (var subrender in render)
                {
                    if (subrender.TypeMode == _viewMode)
                    {
                        IGuiTree treeRender = subrender as IGuiTree;
                        if(treeRender != null)
                            treeRender.CanDrag(ref ret);
                    }
                }
                return ret;
            }
            return false;
        }

        protected override void SetupDragAndDrop(SetupDragAndDropArgs args)
        {
            if (hasSearch)
                return;

            DragAndDrop.PrepareStartDrag();
            var rows = GetRows();
            List<TreeViewItem> draggedRows = new List<TreeViewItem>();
            for (int i = 0; i < rows.Count; i++)
            {
                if (args.draggedItemIDs.Contains(rows[i].id))
                {
                    draggedRows.Add(rows[i]);
                }
            }

            DragAndDrop.SetGenericData("AssetTreeViewDragging", draggedRows);
            DragAndDrop.objectReferences = new UnityEngine.Object[] { }; // this IS required for dragging to work
            string title = draggedRows.Count == 1 ? draggedRows[0].displayName : "< Multiple >";
            DragAndDrop.StartDrag(title);
        }

        protected override DragAndDropVisualMode HandleDragAndDrop(DragAndDropArgs args)
        {
            var render = AssetTreeManager.mIns.GetGuiRender(EditorContexts.mIns.Mode);
            if (render != null)
            {
                AssetDragAndDropArgs copyArgs = new AssetDragAndDropArgs();
                copyArgs.dragAndDropPosition =(AssetDragAndDropArgs.DragAndDropPosition) ((int)args.dragAndDropPosition);
                copyArgs.insertAtIndex = args.insertAtIndex;
                copyArgs.parentItem = args.parentItem;
                copyArgs.performDrop = args.performDrop;
                DragAndDropVisualMode result = DragAndDropVisualMode.Move;
                foreach (var subrender in render)
                {
                    if (subrender.TypeMode == _viewMode)
                    {
                        IGuiTree treeRender = subrender as IGuiTree;
                        if (treeRender != null)
                        {
                            treeRender.Drag(ref copyArgs, ref result);
                        }
                    }
                        
                }
                return result;
            }

            return base.HandleDragAndDrop(args);
        }

        #region  Sorting

        void OnSortingChanged(MultiColumnHeader header)
        {
            if(GetRows().Count <= 1 || header.sortedColumnIndex <0 || !header.canSort)
                return;

            var md = EditorContexts.mIns.Mode;
            var render = AssetTreeManager.mIns.GetGuiRender(md);
            if (render != null)
            {
                foreach (var subrender in render)
                {
                    if (subrender.TypeMode == _viewMode)
                    {
                        IGuiTree treeRender = subrender as IGuiTree;
                        if (treeRender != null)
                        {
                            treeRender.Sort(header, rootItem, GetRows());
                        }
                    }   
                }
            }

            AssetBundleEditorHelper.TreeListChildren(rootItem,GetRows());
            Repaint();
        }

        #endregion

        #region Rendering

        protected override void RowGUI(RowGUIArgs args)
        {

            try
            {
                var render = AssetTreeManager.mIns.GetGuiRender(EditorContexts.mIns.Mode);
                if (render != null)
                {
                    var item = (AssetTreeItem<T>)args.item;
                    for (int i = 0; i < args.GetNumVisibleColumns(); ++i)
                    {
                        AssetRowGuiArgs copyArgs = new AssetRowGuiArgs();
                        copyArgs.item = args.item;
                        copyArgs.focused = args.focused;
                        copyArgs.isRenaming = args.isRenaming;
                        copyArgs.label = args.label;
                        copyArgs.row = args.row;
                        copyArgs.rowRect = args.rowRect;
                        copyArgs.selected = args.selected;

                        //
                        copyArgs.CellRect = args.GetCellRect(i);
                        copyArgs.Column = args.GetColumn(i);

                        foreach (var subrender in render)
                        {
                            if (subrender.TypeMode == _viewMode)
                            {
                                IGuiTree treeRender = subrender as IGuiTree;
                                if (treeRender != null)
                                {
                                    if (!treeRender.CellGui(item, ref copyArgs))
                                    {
                                        base.RowGUI(args);
                                    }
                                }
                            }
                        }
                    }
                }
            }
            catch(Exception ex)
            {
                Debug.LogError(ex);
            }

        }

        #endregion
    }

}
#endif