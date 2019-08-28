#if UNITY_EDITOR
using System;
using UnityEngine;
using System.Collections.Generic;
using CommonUtils;
using UnityEditor;
using UnityEditor.IMGUI.Controls;

namespace AssetBundleSystem.Editor
{

    internal class AssetDependencyGuiLogic : IGuiTree
    {
        private enum TopMenu
        {
            Name,
            ResType,
            Size,
            FileSize,
            SelfSize,
            SelfFileSize,
            BeDependOn,
        }

        public AssetWindow.WindowMode TypeMode { get { return AssetWindow.WindowMode.AssetDependency; } }
        private TreeView _treeView;
        private TreeViewState _treeViewState;
        private TreeColumnHeader _columnHeader;
        private MultiColumnHeaderState _mMultiColumnHeaderState;
        private int _sortval;
        //private List<string> _lastExpanded;
        private AssetTreeModel<DependencyTreeData> _model;

        public void Init()
        {
            if(_treeViewState == null)
                _treeViewState = new TreeViewState();

            //create Head
            RefreshHead();

            //tree view
            if (_treeView == null)
                _treeView = new AssetTreeView<DependencyTreeData>(_treeViewState, _columnHeader, AssetWindow.WindowMode.AssetDependency);

            _model = AssetTreeManager.mIns.GetModel<DependencyTreeData>();
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

        }

        public List<string> Refresh()
        {
            //List<string> list = ListPool<string>.Get();
            //if (_treeView != null)
            //{
            //    var expanded = _treeView.GetExpanded();
            //    var rows = _treeView.GetRows();
            //    for (int i = 0; i < expanded.Count; i++)
            //    {
            //        var id = expanded[i];
            //        for (int j = 0; j < rows.Count; j++)
            //        {
            //            var item = rows[j] as AssetTreeItem<DependencyTreeData>;
            //            if (item != null && item.id == id)
            //            {
            //                list.Add(item.displayName);
            //                break;
            //            }
            //        }
            //    }

            //    _lastExpanded = list;
            //    _treeView.Reload();
            //}
            //return list;

            _treeView.Reload();
            return null;
        }


        public void Rebuild()
        {
            //if (_lastExpanded != null)
            //{
            //    List<int> idlist = new List<int>();
            //    var rows = _treeView.GetRows();
            //    for (int i = 0; i < _lastExpanded.Count; i++)
            //    {
            //        string idkey = _lastExpanded[i];
            //        for (int j = 0; j < rows.Count; j++)
            //        {
            //            var item = rows[j];
            //            if (item != null && item.displayName.Equals(idkey))
            //            {
            //                idlist.Add(item.id);
            //                break;
            //            }
            //        }
            //    }
            //    ListPool<string>.Release(_lastExpanded);
            //    _lastExpanded = null;
            //   // _treeView.SetExpanded(idlist);
            //}
        }

        GUIContent GetResTypeContent(AssetBundleResType resType)
        {
            if (resType == AssetBundleResType.None)
            {
                return  new GUIContent("Asset Bundle", AssetTreeManager.mIns.GetEditorTexture("Folder"));
            }
            return new GUIContent(resType.ToString(), AssetTreeManager.mIns.GetEditorTexture(resType));
        }



        public bool CellGui(TreeViewItem item, ref AssetRowGuiArgs args)
        {
            var assetitem = item as AssetTreeItem<DependencyTreeData>;
            if (assetitem != null)
            {

                var data = assetitem.GetData();
                var runtimeInfo = data.EditorInfo.RuntimeInfo;
                bool isroot = item.depth == 0;
                var topmenutype = (TopMenu) args.Column;

                if (topmenutype == TopMenu.Name)
                {
                    return false;
                }
                else if (topmenutype == TopMenu.ResType)
                {
                    GUI.Label(args.CellRect, GetResTypeContent(runtimeInfo.AssetResType));
                }
                else if (topmenutype == TopMenu.Size)//size
                {
                    long size = AssetBundleEditorHelper.GetAssetSize(ref data, isroot,_model);

                    GUI.Label(args.CellRect, AssetBundleEditorHelper.ConvertSize(size));
                }
                else if (topmenutype == TopMenu.FileSize)//file size
                {
                    long size = AssetBundleEditorHelper.GetFileSize(ref data, isroot,_model);

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
                else if (topmenutype == TopMenu.BeDependOn)
                {
                    if (isroot)
                    {
                        var list = _model.GetDependParents(runtimeInfo.UnityPath);

                        GUI.skin.label.richText = true;
                        var info = AssetBundleEditorHelper.GetColorText("be depond on count :{0}", Color.grey, list.Count.ToString());
                        if (!string.IsNullOrEmpty(data.EditorInfo.RuntimeInfo.AssetBundleName))
                        {
                            info += AssetBundleEditorHelper.GetColorText(" AssetBundle Name :{0},It should be Load {0} first",Color.yellow, data.EditorInfo.RuntimeInfo.AssetBundleName);
                        }
                        else
                        {
                            info += AssetBundleEditorHelper.GetColorText(" it's an noassetbundlename resource,it will pack into this assetbundle alonely",Color.white);
                        }

                        GUI.Label(args.CellRect,new GUIContent(info));
                        GUI.skin.label.richText = false;
                    }
                }
                return true;
            }
            else
            {
                return false;
            }
        }

        public void GuiUpdate(bool isrunning, float delta)
        {
            
        }

        #region Drag


        public void CanDrag(ref bool retval)
        {
            retval = true;
        }


        public void Drag(ref AssetDragAndDropArgs dragargs, ref DragAndDropVisualMode dragmd)
        {
            
        }
        #endregion

        public void RefreshGuiInfo(EditorContexts contexts)
        {
            contexts.GuiContext.SearchFieldRect = new Rect(5, EditorGuiContext.GuiHeight, contexts.GuiContext.WindowRect.width - 10, EditorGuiContext.GuiHeight);
            contexts.GuiContext.TreeViewRect = new Rect(5, 2 * EditorGuiContext.GuiHeight, contexts.GuiContext.WindowRect.width - 10, contexts.GuiContext.WindowRect.height - 3 * EditorGuiContext.GuiHeight);

            if (contexts.Mode == AssetWindow.WindowMode.AssetDependency)
            {
                Rect rect = EditorContexts.mIns.GuiContext.WindowRect;
                float totalheight = rect.height - 3 * EditorGuiContext.GuiHeight;
                float halfheight = Mathf.RoundToInt(totalheight / 2);

                EditorContexts.mIns.GuiContext.TreeViewRect = new Rect(5, 2 * EditorGuiContext.GuiHeight, rect.width - 10, halfheight);
                EditorContexts.mIns.GuiContext.DependencyRect = new Rect(5, 2 * EditorGuiContext.GuiHeight + halfheight, rect.width - 10, halfheight);
            }
        }

        public void RenderGui()
        {
            if (EditorContexts.mIns.Mode == AssetWindow.WindowMode.AssetDependency)
            {
                GUI.BeginGroup(EditorContexts.mIns.GuiContext.DependencyRect, GUI.skin.FindStyle("CurveEditorBackground"));
                float wid = EditorContexts.mIns.GuiContext.DependencyRect.width;
                float height = EditorContexts.mIns.GuiContext.DependencyRect.height;

                var style = GUI.skin.FindStyle("MeTransOnRight");
                style.alignment = TextAnchor.MiddleCenter;
                style.fixedHeight = Mathf.RoundToInt(EditorGuiContext.GuiHeight + 2);

                

                GUI.Label(new Rect(0, 0, wid, EditorGuiContext.GuiHeight), new GUIContent("Dependency Panel :"+ EditorContexts.mIns.SelectForDependencyData.EditorInfo.RuntimeInfo.AssetBundleName), style);

                _treeView.OnGUI(new Rect(0, EditorGuiContext.GuiHeight,wid,height- EditorGuiContext.GuiHeight));

                GUI.EndGroup();
            }
        }


        public void SelectionChanged(IList<TreeViewItem> selectedIds)
        {
            if (selectedIds.Count > 0)
            {
                List<int> list = new List<int>();
                foreach (var item in selectedIds)
                {
                    var assetitem = item as AssetTreeItem<DependencyTreeData>;
                    if (assetitem != null)
                    {
                        var data = assetitem.GetData();
                        list.Add(data.EditorInfo.EditorInstanceId);
                    }
                }

                Selection.instanceIDs = list.ToArray();
            }
        }

        public void Sort(MultiColumnHeader header, TreeViewItem root, IList<TreeViewItem> items)
        {
            int sortIndex = header.sortedColumnIndex;
            bool ascending = header.IsSortedAscending(sortIndex);
            var topmenutype = (TopMenu)sortIndex;
            if (topmenutype == TopMenu.Name)//name
            {
                SortDefault(root, ascending);
            }
            else if (topmenutype == TopMenu.ResType)//restype
            {
                SortResType(root, ascending);
            }
            else if (topmenutype == TopMenu.Size)
            {
                SortSize(root,ascending);
            }
            else if (topmenutype == TopMenu.FileSize)
            {
                SortFileSize(root, ascending);
            }
            else if (topmenutype == TopMenu.SelfSize)
            {
                SortSelfSize(root, ascending);
            }
            else if (topmenutype == TopMenu.SelfFileSize)
            {
                SortSelfFileSize(root, ascending);
            }
            else if (topmenutype == TopMenu.BeDependOn)
            {
                SortDependency(root, ascending);
            }
        }

        void Sort<T>(T rootItem, Comparison<TreeViewItem> func,Comparison<int> intfunc) where T : TreeViewItem
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
                if (_treeView.IsExpanded(item.id) && item.children.Count > 0 && item.children[0] != null)
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
            Sort(rootItem, SortDefualtFunc, SortDefualtFunc);
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

        void SortDependency(TreeViewItem rootItem, bool ascending)
        {
            _sortval = ascending ? 1 : -1;
            Sort(rootItem, SortDependcyFunc, SortDependcyFunc);
        }

        void SortResType(TreeViewItem rootItem, bool ascending)
        {
            _sortval = ascending ? 1 : -1;
            Sort(rootItem, SortResTypeFunc, SortResTypeFunc);
        }

        int SortDefualtFunc(TreeViewItem left, TreeViewItem right)
        {
            return _sortval * string.Compare(left.displayName, right.displayName, StringComparison.OrdinalIgnoreCase);
        }

        int SortDefualtFunc(int left,int right)
        {
            return _sortval * (left -right);
        }

        int SortSizeFunc(TreeViewItem left, TreeViewItem right)
        {
            AssetTreeItem<DependencyTreeData> leftItem = left as AssetTreeItem<DependencyTreeData>;
            AssetTreeItem<DependencyTreeData> rightItem = right as AssetTreeItem<DependencyTreeData>;
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
            DependencyTreeData ld;
            DependencyTreeData rd;
            if (_model.GetItem(left, out ld) && _model.GetItem(right, out rd))
            {
                long leftsize = AssetBundleEditorHelper.GetAssetSize(ref ld, ld.IsAssetBundleViewData, _model);
                long rightsize = AssetBundleEditorHelper.GetAssetSize(ref rd, rd.IsAssetBundleViewData, _model);
                return _sortval * (int)(leftsize - rightsize);
            }

            return 0;
        }

        int SortFileSizeFunc(TreeViewItem left, TreeViewItem right)
        {
            AssetTreeItem<DependencyTreeData> leftItem = left as AssetTreeItem<DependencyTreeData>;
            AssetTreeItem<DependencyTreeData> rightItem = right as AssetTreeItem<DependencyTreeData>;
            if (leftItem != null && rightItem != null)
            {
                var ld = leftItem.GetData();
                var rd = rightItem.GetData();
                long leftsize = AssetBundleEditorHelper.GetFileSize(ref ld, left.depth == 0,_model);
                long rightsize = AssetBundleEditorHelper.GetFileSize(ref rd, left.depth == 0,_model);
                return _sortval * (int)(leftsize - rightsize);
            }
            return 0;
        }

        int SortFileSizeFunc(int left, int right)
        {
            DependencyTreeData ld;
            DependencyTreeData rd;
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
            AssetTreeItem<DependencyTreeData> leftItem = left as AssetTreeItem<DependencyTreeData>;
            AssetTreeItem<DependencyTreeData> rightItem = right as AssetTreeItem<DependencyTreeData>;
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
            DependencyTreeData ld;
            DependencyTreeData rd;
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
            AssetTreeItem<DependencyTreeData> leftItem = left as AssetTreeItem<DependencyTreeData>;
            AssetTreeItem<DependencyTreeData> rightItem = right as AssetTreeItem<DependencyTreeData>;
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
            DependencyTreeData ld;
            DependencyTreeData rd;
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
            AssetTreeItem<DependencyTreeData> leftItem = left as AssetTreeItem<DependencyTreeData>;
            AssetTreeItem<DependencyTreeData> rightItem = right as AssetTreeItem<DependencyTreeData>;
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
            DependencyTreeData ld;
            DependencyTreeData rd;
            if (_model.GetItem(left, out ld) && _model.GetItem(right, out rd))
            {
                long leftsize = (int)ld.EditorInfo.RuntimeInfo.AssetResType;
                long rightsize = (int)rd.EditorInfo.RuntimeInfo.AssetResType;
                return _sortval * (int)(leftsize - rightsize);
            }

            return 0;
        }


        int SortDependcyFunc(TreeViewItem left, TreeViewItem right)
        {
            if (left.depth == 0 && right.depth == 0)
            {
                AssetTreeItem<DependencyTreeData> leftItem = left as AssetTreeItem<DependencyTreeData>;
                AssetTreeItem<DependencyTreeData> rightItem = right as AssetTreeItem<DependencyTreeData>;
                if (leftItem != null && rightItem != null)
                {

                    var leftList = _model.GetDependParents(leftItem.GetData().FilePath);
                    var rightList = _model.GetDependParents(rightItem.GetData().FilePath);
                    int leftcnt = leftList.Count;
                    int rightcnt = rightList.Count;
                    ListPool<DependencyTreeData>.Release(leftList);
                    ListPool<DependencyTreeData>.Release(rightList);
                    return _sortval * (leftcnt - rightcnt);
                }
            }
            else if (left.depth == 0)
            {
                return -1;
            }
            else if (right.depth == 0)
            {
                return 1;
            }


            return 0;
        }

        int SortDependcyFunc(int left, int right)
        {
            DependencyTreeData ld;
            DependencyTreeData rd;
            if (_model.GetItem(left, out ld) && _model.GetItem(right, out rd))
            {
                if(ld.IsAssetBundleViewData && rd.IsAssetBundleViewData)
                {
                    var leftList = _model.GetDependParents(ld.FilePath);
                    var rightList = _model.GetDependParents(rd.FilePath);
                    int leftcnt = leftList.Count;
                    int rightcnt = rightList.Count;
                    ListPool<DependencyTreeData>.Release(leftList);
                    ListPool<DependencyTreeData>.Release(rightList);
                    return _sortval * (leftcnt - rightcnt);
                }
                else if(ld.IsAssetBundleViewData)
                {
                    return -1;
                }
                else if(rd.IsAssetBundleViewData)
                {
                    return 1;
                }
            }

            return 0;
        }

        public MultiColumnHeaderState CreateDefaultHeaderState()
        {
            var windowrect = EditorContexts.mIns.GuiContext.WindowRect;
            List<MultiColumnHeaderState.Column> colnums = new List<MultiColumnHeaderState.Column>();

            float colwidth = 0;
            float colminwidth = 0;
            float colmaxwidth = 0;

            colnums.Add(new MultiColumnHeaderState.Column
            {
                headerContent = new GUIContent(EditorGUIUtility.FindTexture("FilterByLabel")),
                contextMenuText = "Name",
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
                headerContent = new GUIContent(EditorGUIUtility.FindTexture("FilterByType")),
                contextMenuText = "ResType",
                headerTextAlignment = TextAlignment.Left,
                sortedAscending = true,
                sortingArrowAlignment = TextAlignment.Left,
                width = 120,
                minWidth = 80,
                maxWidth = 150,
                autoResize = false,
                allowToggleVisibility = true
            });

            colwidth += colnums[colnums.Count - 1].width;
            colminwidth += colnums[colnums.Count - 1].minWidth;
            colmaxwidth += colnums[colnums.Count - 1].maxWidth;

            colnums.Add(new MultiColumnHeaderState.Column
            {
                headerContent = new GUIContent("Size", "Memory Size"),
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
                headerContent = new GUIContent("Self Size"),
                contextMenuText = "Self Size",
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
                headerContent = new GUIContent("Self FileSize"),
                contextMenuText = "Self FileSize",
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
                headerContent = new GUIContent("Be dependent on"),
                headerTextAlignment = TextAlignment.Center,
                sortedAscending = true,
                sortingArrowAlignment = TextAlignment.Center,
                width = windowrect.width - 10 - colwidth,
                minWidth = windowrect.width - 10 - colmaxwidth,
                maxWidth = windowrect.width - 10 - colminwidth,
                autoResize = true
            });


            var state = new MultiColumnHeaderState(colnums.ToArray());
            return state;
        }

    }
}
#endif


