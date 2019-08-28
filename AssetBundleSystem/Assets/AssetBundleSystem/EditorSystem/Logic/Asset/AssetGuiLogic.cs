#if UNITY_EDITOR
using System;
using UnityEngine;
using System.Collections.Generic;
using CommonUtils;
using UnityEditor;
using UnityEditor.IMGUI.Controls;

namespace AssetBundleSystem.Editor
{
    internal class AssetGuiLogic : IGuiTree
    {
        public AssetWindow.WindowMode TypeMode { get { return AssetWindow.WindowMode.Asset; } }

        private enum TopMenu
        {
            AssetBundle,
            InAssetBundle,
            ResType,
            Size,
            FileSize,
            SelfSize,
            SelfFileSize,
            Dependency,
            BeDepend,
            Info,
        }

        public struct GuiShowMsg : IEquatable<GuiShowMsg>
        {
            public string Message;
            public MessageType MessageType;
            public float Time;

            public bool Equals(GuiShowMsg other)
            {
                return string.Equals(Message, other.Message) && MessageType == other.MessageType && Time.Equals(other.Time);
            }

            public override bool Equals(object obj)
            {
                if (ReferenceEquals(null, obj)) return false;
                return obj is GuiShowMsg && Equals((GuiShowMsg)obj);
            }

            public override int GetHashCode()
            {
                unchecked
                {
                    var hashCode = (Message != null ? Message.GetHashCode() : 0);
                    hashCode = (hashCode * 397) ^ (int)MessageType;
                    hashCode = (hashCode * 397) ^ Time.GetHashCode();
                    return hashCode;
                }
            }
        }

        private int _sortval;
        private SearchField _searchField;
        private TreeView _treeView;
        private TreeViewState _treeViewState;
        private TreeColumnHeader _columnHeader;
        private MultiColumnHeaderState _mMultiColumnHeaderState;
        private List<string> _lastExpanded;

        private string _selectfile;
        private float _showtime;

        private GuiShowMsg _currentmsg;
        private readonly Queue<GuiShowMsg> _tips = new Queue<GuiShowMsg>();
        private AssetTreeModel<AssetTreeData> _model;

        public void Init()
        {
            
            if (_searchField == null)
                _searchField = new SearchField();
            if (_treeViewState == null)
                _treeViewState = new TreeViewState();

            //create Head
            RefreshHead();

            //tree view
            if (_treeView == null)
                _treeView = new AssetTreeView<AssetTreeData>(_treeViewState, _columnHeader, AssetWindow.WindowMode.Asset);
            else
            {
                _columnHeader.Repaint();
                _treeView.Repaint();
            }

            //search field
            _searchField.downOrUpArrowKeyPressed -= PressSearch;
            _searchField.downOrUpArrowKeyPressed += PressSearch;

            _model = AssetTreeManager.mIns.GetModel<AssetTreeData>();
        }

        void PressSearch()
        {
            _treeView.SetFocusAndEnsureSelectedItem();
        }

        public void RefreshHead()
        {
            var headerState = CreateDefaultHeaderState();
            if (MultiColumnHeaderState.CanOverwriteSerializedFields(headerState, _mMultiColumnHeaderState))
                MultiColumnHeaderState.OverwriteSerializedFields(headerState, _mMultiColumnHeaderState);
            _mMultiColumnHeaderState = headerState;

            if (_columnHeader == null)
                _columnHeader = new TreeColumnHeader(_mMultiColumnHeaderState);
            else
            {
                _columnHeader.state = _mMultiColumnHeaderState;
            }
        }

        public void Destory()
        {
            EditorUtility.UnloadUnusedAssetsImmediate();
            Resources.UnloadUnusedAssets();
        }

        public List<string> Refresh()
        {
            List<string> list = ListPool<string>.Get();
            if (_treeView != null)
            {
                var expanded = _treeView.GetExpanded();
                var rows = _treeView.GetRows();
                for (int i = 0; i < expanded.Count; i++)
                {
                    var id = expanded[i];
                    for (int j = 0; j < rows.Count; j++)
                    {
                        var item = rows[j] as AssetTreeItem<AssetTreeData>;
                        if (item != null && item.id == id)
                        {
                            list.Add(item.displayName);
                            break;
                        }
                    }
                }

                _lastExpanded = list;
                _treeView.Reload();
            }
            return list;
        }


        public void Rebuild()
        {
            if (_lastExpanded != null)
            {
                List<int> idlist = new List<int>();
                var rows = _treeView.GetRows();
                for (int i = 0; i < _lastExpanded.Count; i++)
                {
                    string idkey = _lastExpanded[i];
                    for (int j = 0; j < rows.Count; j++)
                    {
                        var item = rows[j];
                        if (item != null && item.displayName.Equals(idkey))
                        {
                            idlist.Add(item.id);
                            break;
                        }
                    }
                }
                ListPool<string>.Release(_lastExpanded);
                _lastExpanded = null;
                _treeView.SetExpanded(idlist);
            }
        }

        public void RefreshGuiInfo(EditorContexts contexts)
        {
            contexts.GuiContext.SearchFieldRect = new Rect(5, EditorGuiContext.GuiHeight, contexts.GuiContext.WindowRect.width - 10, EditorGuiContext.GuiHeight);
            contexts.GuiContext.TreeViewRect = new Rect(5, 2 * EditorGuiContext.GuiHeight, contexts.GuiContext.WindowRect.width - 10, contexts.GuiContext.WindowRect.height - 3 * EditorGuiContext.GuiHeight);
  
        }

        public void GuiUpdate(bool isrunning, float delta)
        {
            if (!isrunning)
            {
                if (_showtime > 0)
                    _showtime -= delta;
            }
        }

        public void RenderGui()
        {
            //Render
            RenderHelpBox();
            RenderSearchBar();
            RenderTree();
            RenderBottom();
        }

        void RenderBottom()
        {
            GUI.BeginGroup(EditorContexts.mIns.GuiContext.ToolbarRect);
            var style = "miniButton";

            var md = AssetBundleEditorHelper.GetMainMode(EditorContexts.mIns.Mode);
            var lw = EditorGUIUtility.labelWidth;
            var lt = EditorGUI.indentLevel;

            EditorGUIUtility.labelWidth = 80;
            EditorGUI.indentLevel = 0;
            var newmd = (AssetWindow.WindowMode)EditorGUI.EnumPopup(new Rect(0, 0, 180, EditorGuiContext.GuiHeight), new GUIContent("Select Mode"),md);

            EditorGUIUtility.labelWidth = lw;
            EditorGUI.indentLevel = lt;

            newmd = AssetBundleEditorHelper.GetMainMode(newmd);
            if (newmd != md)
            {
                EditorContexts.mIns.Mode = newmd;
            }

            if (GUI.Button(new Rect(190, 0, 80, EditorGuiContext.GuiHeight), "Expand All", style))
            {
                _treeView.ExpandAll();
            }

            if (GUI.Button(new Rect(280, 0, 80, EditorGuiContext.GuiHeight), "Collapse All", style))
            {
                _treeView.CollapseAll();
            }

            if (GUI.Button(new Rect(370, 0, 80, EditorGuiContext.GuiHeight), "Reload", style))
            {
                EditorContexts.mIns.ForceModeChange(EditorContexts.mIns.Mode);
                if(_treeView == null)
                {
                    Init();
                    _treeView.Reload();
                }
            }

            //if (GUI.Button(new Rect(460, 0, 80, EditorGuiContext.GuiHeight), "ReInit", style))
            //{
            //    EditorContexts.mIns.ForceModeChange(EditorContexts.mIns.Mode,true);
            //}

            GUI.EndGroup();
        }

        void RenderHelpBox()
        {
            if (_showtime > 0)
            {
                float height = 2 * EditorGuiContext.GuiHeight;
                EditorContexts.mIns.GuiContext.SearchFieldRect.y += height;
                EditorContexts.mIns.GuiContext.TreeViewRect.y += height;
                EditorContexts.mIns.GuiContext.DependencyRect.y += height;


                GUIStyle style = EditorStyles.helpBox;
                float oh = style.fixedHeight;
                var oa = style.alignment;
                var os = style.fontSize;
                style.fontSize = 16;
                style.fixedHeight = height;
                style.alignment = TextAnchor.MiddleCenter;

                EditorGUI.HelpBox(new Rect(0, EditorGuiContext.GuiHeight, EditorContexts.mIns.GuiContext.WindowRect.width, height), _currentmsg.Message, _currentmsg.MessageType);
      
                style.fixedHeight = oh;
                style.alignment = oa;
                style.fontSize = os;
            }
            else if (_tips.Count > 0)
            {
                _currentmsg = _tips.Dequeue();
                _showtime = _currentmsg.Time;
            }
        }

        void RenderSearchBar()
        {
            if(_treeView != null)
            {
                var newstring= _searchField.OnGUI(EditorContexts.mIns.GuiContext.SearchFieldRect, _treeView.searchString);
                if(newstring != _treeView.searchString)
                {
                    _treeView.searchString = newstring;
                    var selects = _treeView.GetSelection();
                    
                    if(selects != null && selects.Count >0)
                    {
                        _treeView.SetSelection(selects, TreeViewSelectionOptions.RevealAndFrame);
                       // _treeView.FrameItem(selects[0]);
                    }
                }
            }

        }

        void RenderTree()
        {
            if (_treeView != null)
                _treeView.OnGUI(EditorContexts.mIns.GuiContext.TreeViewRect);
        }

        //List<AssetTreeItem<AssetTreeData>g> GetSelectItems()
        //{

        //    var selects = _treeView.GetSelection();
        //    List<AssetTreeItem<AssetTreeData>> list = ListPool<AssetTreeItem<AssetTreeData>>.Get();
        //    foreach (var id in selects)
        //    {
        //        for (int i = 0; i < _treeView.GetRows().Count; i++)
        //        {
        //            var item = _treeView.GetRows()[i];
        //            if (item.id == id)
        //            {
        //                list.Add(item as AssetTreeItem<AssetTreeData>);
        //                break;
        //            }
        //        }
        //    }
        //    return list;
        //}

        public bool CellGui(TreeViewItem item, ref AssetRowGuiArgs args)
        {
            var assetitem = item as AssetTreeItem<AssetTreeData>;
            if (assetitem != null)
            {
                var data = assetitem.GetData();
                var runtimeInfo = data.EditorInfo.RuntimeInfo;
                bool isroot = item.depth == 0;
                var topmenutype = (TopMenu) args.Column;
                if (topmenutype == TopMenu.AssetBundle)
                {
                    //GUI.Label(args.CellRect, new GUIContent(item.displayName, item.icon));
                    return false;
                }
                else if (topmenutype == TopMenu.InAssetBundle)
                {
                    if (!isroot)
                    {
                        bool isIn = !string.IsNullOrEmpty(runtimeInfo.AssetBundleName) && item.depth <=1;
                        bool value = GUI.Toggle(args.CellRect, isIn, "");
                        if (isIn != value)
                        {
                            if (value)
                            {
                                if (item.parent != null && item.parent.parent != null)
                                {
                                    AssetImporter importer = AssetImporter.GetAtPath(runtimeInfo.UnityPath);
                                    if (importer != null)
                                    {
                                        importer.assetBundleName = item.parent.parent.displayName;
                                        importer.SaveAndReimport();
                                    }

                                    EditorContexts.mIns.ForceModeChange(EditorContexts.mIns.Mode);
                                }
                                else
                                {
                                    Debug.LogErrorFormat("{0} parent is Null", runtimeInfo.UnityPath);
                                }

                            }
                            else
                            {
                                AssetImporter importer = AssetImporter.GetAtPath(runtimeInfo.UnityPath);
                                if (importer != null)
                                {
                                    importer.assetBundleName = "";
                                    importer.SaveAndReimport();
                                }

                                EditorContexts.mIns.ForceModeChange(EditorContexts.mIns.Mode);
                            }
                        }
                    }
                }
                else if (topmenutype == TopMenu.ResType) //
                {
                    if (!isroot)
                    {
                        var restype = data.EditorInfo.RuntimeInfo.AssetResType;
                        EditorGUI.EnumPopup(args.CellRect, restype);
                    }
                    else
                    {
                        //EditorGUIUtility.FindTexture(AssetTreeManager.mIns.GetIconName("Folder"))
                        GUI.Label(args.CellRect, new GUIContent("ab", "Asset Bundle"));
                    }
                }
                else if (topmenutype == TopMenu.Size)//size
                {
                    long size = AssetBundleEditorHelper.GetAssetSize(ref data, isroot, _model);

                    GUI.Label(args.CellRect, AssetBundleEditorHelper.ConvertSize(size));
                }
                else if (topmenutype == TopMenu.FileSize)//file size
                {
                    long size = AssetBundleEditorHelper.GetFileSize(ref data, isroot, _model);

                    GUI.Label(args.CellRect, AssetBundleEditorHelper.ConvertSize(size));
                }
                else if (topmenutype == TopMenu.SelfSize)//size
                {
                    long size = AssetBundleEditorHelper.GetSelfAssetSize(ref data, isroot, _model);

                    GUI.Label(args.CellRect, AssetBundleEditorHelper.ConvertSize(size));
                }
                else if (topmenutype == TopMenu.SelfFileSize)//file size
                {
                    long size = AssetBundleEditorHelper.GetSelfFileSize(ref data, isroot, _model);

                    GUI.Label(args.CellRect, AssetBundleEditorHelper.ConvertSize(size));
                }
                else if (topmenutype == TopMenu.Dependency) //dependencies
                {
                    if (!isroot  )
                    {
                        if (runtimeInfo.DependenciesCnt == 0)
                        {
                            GUI.enabled = false;
                        }
                        if (GUI.Button(args.CellRect, new GUIContent(runtimeInfo.DependenciesCnt.ToString())))
                        {
                            
                            if (EditorContexts.mIns.Mode != AssetWindow.WindowMode.AssetDependency ||
                                !_selectfile.Equals(runtimeInfo.UnityPath) || EditorContexts.mIns.GuiContext.SelectDepth !=0)
                            {
                                _selectfile = runtimeInfo.UnityPath;
                                AssetTreeData selectData;
                                if (_model.GetItem(runtimeInfo.UnityPath, out selectData))
                                {
                                    EditorContexts.mIns.SelectForDependencyData = selectData;
                                }

                                EditorContexts.mIns.GuiContext.SelectDepth = 0;
                                if (EditorContexts.mIns.Mode != AssetWindow.WindowMode.AssetDependency)
                                {
                                    EditorContexts.mIns.Mode = AssetWindow.WindowMode.AssetDependency;
                                }
                                else
                                {
                                    EditorContexts.mIns.ForceModeChange(AssetWindow.WindowMode.AssetDependency);
                                }

                            }
                            else
                            {
                                EditorContexts.mIns.Mode = AssetWindow.WindowMode.Asset;
                            }
                            //_treeView.FrameItem(item.id);

                            var selectedIDs = new List<int>();
                            selectedIDs.Add(item.id);

                            _treeView.SetSelection(selectedIDs, TreeViewSelectionOptions.RevealAndFrame);
                        }

                        if (runtimeInfo.DependenciesCnt == 0)
                        {
                            GUI.enabled = true;
                        }
                    }
                }
                else if(topmenutype == TopMenu.BeDepend)
                {
                    if(!isroot)
                    {
                        var list = _model.GetDependParents(runtimeInfo.UnityPath);
                        if(list.Count ==0)
                        {
                            GUI.enabled = false;
                        }
                        if(GUI.Button(args.CellRect,list.Count.ToString()))
                        {

                            if (EditorContexts.mIns.Mode != AssetWindow.WindowMode.AssetDependency ||
                                !_selectfile.Equals(runtimeInfo.UnityPath) || EditorContexts.mIns.GuiContext.SelectDepth != 1)
                            {
                                _selectfile = runtimeInfo.UnityPath;
                                AssetTreeData selectData;
                                if (_model.GetItem(runtimeInfo.UnityPath, out selectData))
                                {
                                    EditorContexts.mIns.SelectForDependencyData = selectData;
                                }

                                EditorContexts.mIns.GuiContext.SelectDepth = 1;
                                if (EditorContexts.mIns.Mode != AssetWindow.WindowMode.AssetDependency)
                                {
                                    EditorContexts.mIns.Mode = AssetWindow.WindowMode.AssetDependency;
                                }
                                else
                                {
                                    EditorContexts.mIns.ForceModeChange(AssetWindow.WindowMode.AssetDependency);
                                }

                            }
                            else
                            {
                                EditorContexts.mIns.Mode = AssetWindow.WindowMode.Asset;
                            }
                            //_treeView.FrameItem(item.id);
                            var selectedIDs = new List<int>();
                            selectedIDs.Add(item.id);

                            _treeView.SetSelection(selectedIDs, TreeViewSelectionOptions.RevealAndFrame);
                        }
                        GUI.enabled = true;
                        ListPool<AssetTreeData>.Release(list);
                    }
                }
                else if (topmenutype == TopMenu.Info) //info
                {
                    GUI.skin.label.richText = true;
                    if (!isroot)
                    {
                        string info = AssetBundleEditorHelper.GetColorText(" Path :{0}", Color.yellow, runtimeInfo.UnityPath);

                        GUI.Label(args.CellRect, info);
                    }
                    else
                    {
                        string info = string.Format("Path:{0}", runtimeInfo.UnityPath);
                        info += AssetBundleEditorHelper.GetColorText(" Hash :{0}", Color.grey, runtimeInfo.HashCode);
                        info += AssetBundleEditorHelper.GetColorText(" Crc :{0}", Color.white, runtimeInfo.Crc);

                        if (HasNoAssetBundleName(ref data))
                        {
                            GUI.Label(args.CellRect, new GUIContent(info, AssetTreeManager.mIns.GetEditorTexture("warning")));
                        }
                        else
                        {
                            GUI.Label(args.CellRect, info);
                        }
                        
                    }
                    GUI.skin.label.richText = false;
                }
                return true;
            }
            return false;
        }

        bool HasNoAssetBundleName(ref AssetTreeData data)
        {
            bool ret = false;
            if (_model.HasChildren(ref data))
            {
                var children = _model.GetChildren(ref data);

                for (int i = 0; i < children.Count; i++)
                {
                    var child = children[i];
                    if (_model.HasChildren(ref child))
                    {
                        ret = true;
                        break;
                    }
                }

                ListPool<AssetTreeData>.Release(children);
            }
            return ret;
        }

        public void SelectionChanged(IList<TreeViewItem> selectedIds)
        {
            if (selectedIds.Count > 0)
            {
                List<int> list = new List<int>();
                foreach (var item in selectedIds)
                {
                    var assetitem = item as AssetTreeItem<AssetTreeData>;
                    if (assetitem != null)
                    {
                        var data = assetitem.GetData();

                        list.Add(data.EditorInfo.EditorInstanceId);
                    }
                }

                Selection.instanceIDs = list.ToArray();
            }
        }

        #region Drag


        public void CanDrag(ref bool retval)
        {
            retval = true;
        }

        public void Drag(ref AssetDragAndDropArgs dragargs, ref DragAndDropVisualMode dragmd)
        {
            var draggedRows = DragAndDrop.GetGenericData("AssetTreeViewDragging") as List<TreeViewItem>;
            if (draggedRows != null)
            {
                switch (dragargs.dragAndDropPosition)
                {
                    case AssetDragAndDropArgs.DragAndDropPosition.UponItem:
                    case AssetDragAndDropArgs.DragAndDropPosition.BetweenItems:
                        {
                            bool validDrag = dragargs.parentItem.depth != 0;
                            if (dragargs.performDrop && validDrag)
                            {
                                if ( dragargs.parentItem.parent != null)
                                {
                                    AssetTreeItem<AssetTreeData> itemdata = dragargs.parentItem.parent as AssetTreeItem<AssetTreeData>;
                                    if (itemdata != null)
                                        OnDropDraggedElements(draggedRows, itemdata);
                                }
                            }

                            dragmd = validDrag ? DragAndDropVisualMode.Move : DragAndDropVisualMode.None;
                            break;
                        }
                    case AssetDragAndDropArgs.DragAndDropPosition.OutsideItems:
                        {
                            //if (dragargs.performDrop)
                            //{
                            //    int index = dragargs.insertAtIndex == -1 ? 0 : dragargs.insertAtIndex;
                            //    var rows = dragargs.parentItem.children;

                            //    if (index < rows.Count)
                            //    {
                            //        AssetTreeItem<AssetTreeData> itemdata = rows[index] as AssetTreeItem<AssetTreeData>;
                            //        if (itemdata != null)
                            //            OnDropDraggedElements(draggedRows, itemdata);
                            //    }
                            //    else
                            //    {
                            //        Debug.LogError("drag index error");
                            //    }
                            //}

                            dragmd = DragAndDropVisualMode.Move;
                            break;
                        }
                    default:
                        Debug.LogError("Unhandled enum " + dragargs.dragAndDropPosition);
                        dragmd = DragAndDropVisualMode.None;
                        break;
                }
            }
            else
            {
                dragmd = DragAndDropVisualMode.None;
            }


        }

        void OnDropDraggedElements(List<TreeViewItem> draggedRows, AssetTreeItem<AssetTreeData> parent)
        {
            if (parent.depth != 0)
            {
                Debug.LogErrorFormat("{0} depth error ", parent);
                return;
            }

            var draggedElements = ListPool<AssetTreeData>.Get();
            foreach (var value in draggedRows)
            {
                AssetTreeItem<AssetTreeData> assetval = value as AssetTreeItem<AssetTreeData>;
                if (assetval != null)
                {
                    draggedElements.Add(assetval.GetData());
                }
            }

            var selectedIDs = new List<int>();
            foreach (var data in draggedElements)
            {
                selectedIDs.Add(data.Id);
            }
            //m_TreeModel.MoveElements(parent, insertIndex, draggedElements);
            _treeView.SetSelection(selectedIDs, TreeViewSelectionOptions.RevealAndFrame);

            //reset assetbundle
            foreach (var data in draggedElements)
            {
                var import = AssetImporter.GetAtPath(data.FilePath);
                if (import != null )
                {
                    var oldname = import.assetBundleName;
                    var newname = parent.displayName;

                    GuiShowMsg msg = new GuiShowMsg();
                    msg.Time = 3f;
                    msg.MessageType = MessageType.Info;
                    msg.Message = string.Format("{0} assetbundlename rename from {1} to {2}", data.FilePath, oldname, newname);

                    import.assetBundleName = newname;
                    import.SaveAndReimport();
                    _tips.Enqueue(msg);
                }
            }

            bool update = draggedElements.Count > 0;

            ListPool<AssetTreeData>.Release(draggedElements);

            if (update)
            {
                //refresh
                EditorContexts.mIns.ForceModeChange(EditorContexts.mIns.Mode);
            }
        }

        #endregion

        #region  Sort

        public void Sort(MultiColumnHeader header, TreeViewItem rootItem, IList<TreeViewItem> items)
        {
            int sortIndex = header.sortedColumnIndex;
            bool ascending = header.IsSortedAscending(sortIndex);
            var topmenutype = (TopMenu)sortIndex;
            if (topmenutype == TopMenu.AssetBundle)//name
            {
                SortDefault(rootItem, ascending);
            }
            else if (topmenutype == TopMenu.ResType)//restype
            {
                SortResType(rootItem, ascending);
            }
            else if (topmenutype == TopMenu.Size)
            {
                SortSize(rootItem, ascending);
            }
            else if (topmenutype == TopMenu.FileSize)
            {
                SortFileSize(rootItem, ascending);
            }
            else if (topmenutype == TopMenu.SelfSize)
            {
                SortSelfSize(rootItem, ascending);
            }
            else if (topmenutype == TopMenu.SelfFileSize)
            {
                SortSelfFileSize(rootItem, ascending);
            }
            else if (topmenutype == TopMenu.Dependency)
            {
                SortDependency(rootItem, ascending);
            }
            else if (topmenutype == TopMenu.Info)
            {
                SortDefault(rootItem, ascending);
            }
        }

        void Sort<T>(T rootItem, Comparison<TreeViewItem> func, Comparison<int> intfunc) where  T:TreeViewItem
        {
            rootItem.children.Sort(func);
            Stack<TreeViewItem> itemstack = StackPool<TreeViewItem>.Get();

            foreach (var child in rootItem.children)
            {
                itemstack.Push(child);
            }

            while (itemstack.Count > 0)
            {
                var item = itemstack.Pop();
                if (_treeView.IsExpanded(item.id) && item.children.Count >0 && item.children[0] != null)
                {
                    foreach (var child in item.children)
                    {
                        itemstack.Push(child);
                    }
                    item.children.Sort(func);
                }
            }

            StackPool<TreeViewItem>.Release(itemstack);

            _model.Sort(intfunc);
        }

        void SortDefault(TreeViewItem rootItem, bool ascending)
        {
            _sortval = ascending ? 1 : -1;
            Sort(rootItem, SortDefualtFunc,SortDefualtFunc);
        }

        void SortSize(TreeViewItem rootItem, bool ascending)
        {
            _sortval = ascending ? 1 : -1;
            Sort(rootItem, SortSizeFunc, SortSizeFunc);
        }

        void SortFileSize(TreeViewItem rootItem, bool ascending)
        {
            _sortval = ascending ? 1 : -1;
            Sort(rootItem, SortFileSizeFunc, SortFileSizeFunc);
        }

        void SortSelfSize(TreeViewItem rootItem, bool ascending)
        {
            _sortval = ascending ? 1 : -1;
            Sort(rootItem, SortSelfSizeFunc, SortSelfSizeFunc);
        }

        void SortSelfFileSize(TreeViewItem rootItem, bool ascending)
        {
            _sortval = ascending ? 1 : -1;
            Sort(rootItem, SortSelfFileSizeFunc, SortSelfFileSizeFunc);
        }

        void SortResType(TreeViewItem rootItem, bool ascending)
        {
            _sortval = ascending ? 1 : -1;
            Sort(rootItem, SortResTypeFunc, SortResTypeFunc);
        }

        void SortDependency(TreeViewItem rootItem, bool ascending)
        {
            _sortval = ascending ? 1 : -1;
            Sort(rootItem, SortDependcyFunc, SortDependcyFunc);
        }

        int SortDefualtFunc(TreeViewItem left, TreeViewItem right)
        {
            return _sortval * (left.id - right.id);
        }

        int SortDefualtFunc(int left, int right)
        {
            return _sortval * (left -right);
        }

        int SortDependcyFunc(TreeViewItem left, TreeViewItem right)
        {
            AssetTreeItem<AssetTreeData> leftItem = left as AssetTreeItem<AssetTreeData>;
            AssetTreeItem<AssetTreeData> rightItem = right as AssetTreeItem<AssetTreeData>;
            if (leftItem != null && rightItem != null)
            {
                int leftsize = leftItem.GetData().EditorInfo.RuntimeInfo.DependenciesCnt;
                int rightsize = rightItem.GetData().EditorInfo.RuntimeInfo.DependenciesCnt;
                return _sortval * (leftsize - rightsize);
            }
            return 0;
        }

        int SortDependcyFunc(int left, int right)
        {
            AssetTreeData ld;
            AssetTreeData rd;
            if (_model.GetItem(left, out ld) && _model.GetItem(right, out rd))
            {
                long leftsize = ld.EditorInfo.RuntimeInfo.DependenciesCnt;
                long rightsize = rd.EditorInfo.RuntimeInfo.DependenciesCnt;
                return _sortval * (int)(leftsize - rightsize);
            }

            return 0;
        }

        int SortSizeFunc(TreeViewItem left, TreeViewItem right)
        {
            AssetTreeItem<AssetTreeData> leftItem = left as AssetTreeItem<AssetTreeData>;
            AssetTreeItem<AssetTreeData> rightItem = right as AssetTreeItem<AssetTreeData>;
            if (leftItem != null && rightItem != null)
            {
                var ld = leftItem.GetData();
                var rd = rightItem.GetData();
                long leftsize = AssetBundleEditorHelper.GetAssetSize(ref ld, left.depth == 0,_model);
                long rightsize = AssetBundleEditorHelper.GetAssetSize(ref rd, left.depth == 0,_model);
                return _sortval * (int)(leftsize - rightsize);
            }
            return 0;
        }

        int SortSizeFunc(int left, int right)
        {
            AssetTreeData ld;
            AssetTreeData rd;
            if(_model.GetItem(left,out ld) && _model.GetItem(right,out rd))
            {
                long leftsize = AssetBundleEditorHelper.GetAssetSize(ref ld, ld.IsAssetBundleViewData, _model);
                long rightsize = AssetBundleEditorHelper.GetAssetSize(ref rd, rd.IsAssetBundleViewData, _model);
                return _sortval * (int)(leftsize - rightsize);
            }

            return 0;
        }

        int SortFileSizeFunc(TreeViewItem left, TreeViewItem right)
        {
            AssetTreeItem<AssetTreeData> leftItem = left as AssetTreeItem<AssetTreeData>;
            AssetTreeItem<AssetTreeData> rightItem = right as AssetTreeItem<AssetTreeData>;
            if (leftItem != null && rightItem != null)
            {
                var ld = leftItem.GetData();
                var rd = rightItem.GetData();
                long leftsize = AssetBundleEditorHelper.GetFileSize(ref ld,left.depth == 0,_model);
                long rightsize = AssetBundleEditorHelper.GetFileSize(ref rd, left.depth == 0,_model);
                return _sortval * (int)(leftsize - rightsize);
            }
            return 0;
        }

        int SortFileSizeFunc(int left, int right)
        {
            AssetTreeData ld;
            AssetTreeData rd;
            if (_model.GetItem(left, out ld) && _model.GetItem(right, out rd))
            {
                long leftsize = AssetBundleEditorHelper.GetFileSize(ref ld, ld.IsAssetBundleViewData, _model);
                long rightsize = AssetBundleEditorHelper.GetFileSize(ref rd, rd.IsAssetBundleViewData, _model);
                return _sortval * (int)(leftsize - rightsize);
            }

            return 0;
        }

        int SortSelfSizeFunc(TreeViewItem left, TreeViewItem right)
        {
            AssetTreeItem<AssetTreeData> leftItem = left as AssetTreeItem<AssetTreeData>;
            AssetTreeItem<AssetTreeData> rightItem = right as AssetTreeItem<AssetTreeData>;
            if (leftItem != null && rightItem != null)
            {
                var ld = leftItem.GetData();
                var rd = rightItem.GetData();
                long leftsize = AssetBundleEditorHelper.GetSelfAssetSize(ref ld, left.depth == 0, _model);
                long rightsize = AssetBundleEditorHelper.GetSelfAssetSize(ref rd, left.depth == 0, _model);
                return _sortval * (int)(leftsize - rightsize);
            }
            return 0;
        }

        int SortSelfSizeFunc(int left, int right)
        {
            AssetTreeData ld;
            AssetTreeData rd;
            if (_model.GetItem(left, out ld) && _model.GetItem(right, out rd))
            {
                long leftsize = AssetBundleEditorHelper.GetSelfAssetSize(ref ld, ld.IsAssetBundleViewData, _model);
                long rightsize = AssetBundleEditorHelper.GetSelfAssetSize(ref rd, rd.IsAssetBundleViewData, _model);
                return _sortval * (int)(leftsize - rightsize);
            }

            return 0;
        }

        int SortSelfFileSizeFunc(TreeViewItem left, TreeViewItem right)
        {
            AssetTreeItem<AssetTreeData> leftItem = left as AssetTreeItem<AssetTreeData>;
            AssetTreeItem<AssetTreeData> rightItem = right as AssetTreeItem<AssetTreeData>;
            if (leftItem != null && rightItem != null)
            {
                var ld = leftItem.GetData();
                var rd = rightItem.GetData();
                long leftsize = AssetBundleEditorHelper.GetSelfFileSize(ref ld, left.depth == 0, _model);
                long rightsize = AssetBundleEditorHelper.GetSelfFileSize(ref rd, left.depth == 0, _model);
                return _sortval * (int)(leftsize - rightsize);
            }
            return 0;
        }

        int SortSelfFileSizeFunc(int left, int right)
        {
            AssetTreeData ld;
            AssetTreeData rd;
            if (_model.GetItem(left, out ld) && _model.GetItem(right, out rd))
            {
                long leftsize = AssetBundleEditorHelper.GetSelfFileSize(ref ld, ld.IsAssetBundleViewData, _model);
                long rightsize = AssetBundleEditorHelper.GetSelfFileSize(ref rd, rd.IsAssetBundleViewData, _model);
                return _sortval * (int)(leftsize - rightsize);
            }

            return 0;
        }

        int SortResTypeFunc(TreeViewItem left, TreeViewItem right)
        {
            AssetTreeItem<AssetTreeData> leftItem = left as AssetTreeItem<AssetTreeData>;
            AssetTreeItem<AssetTreeData> rightItem = right as AssetTreeItem<AssetTreeData>;
            if (leftItem != null && rightItem != null)
            {
                int leftrestype = (int)leftItem.GetData().EditorInfo.RuntimeInfo.AssetResType;
                int rightrestype = (int)rightItem.GetData().EditorInfo.RuntimeInfo.AssetResType;
                return _sortval * (leftrestype - rightrestype);
            }
            return 0;
        }

        int SortResTypeFunc(int left, int right)
        {
            AssetTreeData ld;
            AssetTreeData rd;
            if (_model.GetItem(left, out ld) && _model.GetItem(right, out rd))
            {
                long leftsize = (int)ld.EditorInfo.RuntimeInfo.AssetResType;
                long rightsize = (int)rd.EditorInfo.RuntimeInfo.AssetResType;
                return _sortval * (int)(leftsize - rightsize);
            }

            return 0;
        }
        #endregion

        #region header
        public MultiColumnHeaderState CreateDefaultHeaderState()
        {
            var windowrect = EditorContexts.mIns.GuiContext.WindowRect;
            List<MultiColumnHeaderState.Column> colnums = new List<MultiColumnHeaderState.Column>();

            float colwidth = 0;
            float colminwidth = 0;
            float colmaxwidth = 0;

            colnums.Add(new MultiColumnHeaderState.Column
            {
                headerContent = new GUIContent("AssetBundle",EditorGUIUtility.FindTexture("FilterByLabel"),"Show AssetBundle And Resource in this AssetBundle"),
                contextMenuText = "AssetBundleName",
                headerTextAlignment = TextAlignment.Left,
                sortedAscending = true,
                sortingArrowAlignment = TextAlignment.Left,
                width = 260,
                minWidth = 100,
                maxWidth = 300,
                autoResize = false,
                allowToggleVisibility = true
            });

            colwidth += colnums[colnums.Count - 1].width;
            colminwidth += colnums[colnums.Count - 1].minWidth;
            colmaxwidth += colnums[colnums.Count - 1].maxWidth;


            colnums.Add(new MultiColumnHeaderState.Column
            {
                headerContent = new GUIContent("In AssetBundle"),
                contextMenuText = "In AssetBundle",
                headerTextAlignment = TextAlignment.Left,
                canSort = false,
                sortingArrowAlignment = TextAlignment.Left,
                width = 100,
                minWidth = 60,
                maxWidth = 120,
                autoResize = false,
                allowToggleVisibility = false
            });

            colwidth += colnums[colnums.Count - 1].width;
            colminwidth += colnums[colnums.Count - 1].minWidth;
            colmaxwidth += colnums[colnums.Count - 1].maxWidth;

            colnums.Add(new MultiColumnHeaderState.Column
            {
                headerContent = new GUIContent("ResType",EditorGUIUtility.FindTexture("FilterByType")),
                contextMenuText = "ResType",
                headerTextAlignment = TextAlignment.Left,
                sortedAscending = true,
                sortingArrowAlignment = TextAlignment.Left,
                width = 80,
                minWidth = 60,
                maxWidth = 90,
                autoResize = false,
                allowToggleVisibility = true
            });

            colwidth += colnums[colnums.Count - 1].width;
            colminwidth += colnums[colnums.Count - 1].minWidth;
            colmaxwidth += colnums[colnums.Count - 1].maxWidth;

            colnums.Add(new MultiColumnHeaderState.Column
            {
                headerContent = new GUIContent("Size","Memory Size"),
                contextMenuText = "Size",
                headerTextAlignment = TextAlignment.Left,
                sortedAscending = false,
                sortingArrowAlignment = TextAlignment.Left,
                width = 80,
                minWidth = 60,
                maxWidth = 90,
                autoResize = false,
                allowToggleVisibility = true
            });

            colwidth += colnums[colnums.Count - 1].width;
            colminwidth += colnums[colnums.Count - 1].minWidth;
            colmaxwidth += colnums[colnums.Count - 1].maxWidth;

            colnums.Add(new MultiColumnHeaderState.Column
            {
                headerContent = new GUIContent("FileSize"),
                contextMenuText = "FileSize",
                headerTextAlignment = TextAlignment.Left,
                sortedAscending = false,
                sortingArrowAlignment = TextAlignment.Left,
                width = 80,
                minWidth = 60,
                maxWidth = 90,
                autoResize = false,
                allowToggleVisibility = true
            });

            colwidth += colnums[colnums.Count - 1].width;
            colminwidth += colnums[colnums.Count - 1].minWidth;
            colmaxwidth += colnums[colnums.Count - 1].maxWidth;

            colnums.Add(new MultiColumnHeaderState.Column
            {
                headerContent = new GUIContent("SelfSize"),
                contextMenuText = "SelfSize",
                headerTextAlignment = TextAlignment.Left,
                sortedAscending = false,
                sortingArrowAlignment = TextAlignment.Left,
                width = 80,
                minWidth = 60,
                maxWidth = 90,
                autoResize = false,
                allowToggleVisibility = true
            });

            colwidth += colnums[colnums.Count - 1].width;
            colminwidth += colnums[colnums.Count - 1].minWidth;
            colmaxwidth += colnums[colnums.Count - 1].maxWidth;

            colnums.Add(new MultiColumnHeaderState.Column
            {
                headerContent = new GUIContent("SelfFileSize"),
                contextMenuText = "SelfFileSize",
                headerTextAlignment = TextAlignment.Left,
                sortedAscending = false,
                sortingArrowAlignment = TextAlignment.Left,
                width = 80,
                minWidth = 60,
                maxWidth = 90,
                autoResize = false,
                allowToggleVisibility = true
            });

            colwidth += colnums[colnums.Count - 1].width;
            colminwidth += colnums[colnums.Count - 1].minWidth;
            colmaxwidth += colnums[colnums.Count - 1].maxWidth;

            //colnums.Add(new MultiColumnHeaderState.Column
            //{
            //    headerContent = new GUIContent("Redundancy","If it has self Reduandancy"),
            //    contextMenuText = "Redundancy",
            //    headerTextAlignment = TextAlignment.Left,
            //    canSort = false,
            //    sortingArrowAlignment = TextAlignment.Left,
            //    width = 90,
            //    minWidth = 80,
            //    maxWidth = 100,
            //    autoResize = false,
            //    allowToggleVisibility = true
            //});

            //colwidth += colnums[colnums.Count - 1].width;
            //colminwidth += colnums[colnums.Count - 1].minWidth;
            //colmaxwidth += colnums[colnums.Count - 1].maxWidth;

            colnums.Add(new MultiColumnHeaderState.Column
            {
                headerContent = new GUIContent("Dependencies"),
                headerTextAlignment = TextAlignment.Center,
                sortedAscending = false,
                sortingArrowAlignment = TextAlignment.Right,
                width = 90,
                minWidth = 80,
                maxWidth = 100,
                autoResize = false,
                allowToggleVisibility = false
            });

            colwidth += colnums[colnums.Count - 1].width;
            colminwidth += colnums[colnums.Count - 1].minWidth;
            colmaxwidth += colnums[colnums.Count - 1].maxWidth;

            colnums.Add(new MultiColumnHeaderState.Column
            {
                headerContent = new GUIContent("BeDepend"),
                headerTextAlignment = TextAlignment.Center,
                sortedAscending = false,
                sortingArrowAlignment = TextAlignment.Center,
                width = 80,
                minWidth = 60,
                maxWidth = 100,
                autoResize = false,
                allowToggleVisibility = false
            });

            colwidth += colnums[colnums.Count - 1].width;
            colminwidth += colnums[colnums.Count - 1].minWidth;
            colmaxwidth += colnums[colnums.Count - 1].maxWidth;

            colnums.Add(new MultiColumnHeaderState.Column
            {
                headerContent = new GUIContent("Information"),
                headerTextAlignment = TextAlignment.Center,
                canSort = false,
                sortingArrowAlignment = TextAlignment.Center,
                width = Mathf.Max(150,  windowrect.width - 10 - colwidth),
                minWidth = Mathf.Max(100, windowrect.width - 10 - colmaxwidth),
                maxWidth = Mathf.Max(200, windowrect.width - 10 - colminwidth),
                autoResize = true
            });

            //colwidth += colnums[colnums.Count - 1].width;
            //colminwidth += colnums[colnums.Count - 1].minWidth;
            //colmaxwidth += colnums[colnums.Count - 1].maxWidth;


            var state = new MultiColumnHeaderState(colnums.ToArray());
            return state;
        }

        #endregion
    }
}
#endif