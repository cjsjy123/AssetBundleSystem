#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using CommonUtils;
using UnityEditor;
using UnityEditor.IMGUI.Controls;
using UnityEngine;

namespace AssetBundleSystem.Editor
{
    class SpriteGuiLogic: IGuiTree
    {
        public AssetWindow.WindowMode TypeMode
        {
            get { return AssetWindow.WindowMode.ToolsSprite; }
        }

        public enum SpriteTopMenu
        {
            Name,
            Type,
            AtlasName,
            BundleName,
            UsedCount,
            MemSize,
            Size,
            Info,
        }

        private SearchField _searchField;
        private TreeView _treeView;
        private TreeViewState _treeViewState;
        private TreeColumnHeader _columnHeader;
        private MultiColumnHeaderState _mMultiColumnHeaderState;
        private AssetTreeModel<SpriteTrackData> _model;

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
                _treeView = new AssetTreeView<SpriteTrackData>(_treeViewState, _columnHeader, AssetWindow.WindowMode.ToolsSprite);
            else
            {
                _columnHeader.Repaint();
                _treeView.Repaint();
            }

            //search field
            _searchField.downOrUpArrowKeyPressed -= PressSearch;
            _searchField.downOrUpArrowKeyPressed += PressSearch;

            _model = AssetTreeManager.mIns.GetModel<SpriteTrackData>();
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

        public void RefreshGuiInfo(EditorContexts contexts)
        {
            float wid = contexts.GuiContext.WindowRect.width;
            float height = contexts.GuiContext.WindowRect.height;

            float usedheight = EditorGuiContext.GuiHeight;

            contexts.GuiContext.SpriteSearchRect = new Rect(0, usedheight, wid, EditorGuiContext.GuiHeight);

            usedheight += EditorGuiContext.GuiHeight;

            contexts.GuiContext.SpriteTreeRect = new Rect(0, usedheight, wid, Mathf.Max(100, height - usedheight - EditorGuiContext.GuiHeight));

            contexts.GuiContext.SpriteBotBarRect = new Rect(0, height - EditorGuiContext.GuiHeight, wid, EditorGuiContext.GuiHeight);
        }

        public void Destory()
        {
            EditorUtility.UnloadUnusedAssetsImmediate();
            Resources.UnloadUnusedAssets();
            EditorContexts.mIns.GuiContext.MaskValue = 0;
        }

        public void SelectionChanged(IList<TreeViewItem> selectedIds)
        {
            if (selectedIds.Count > 0)
            {
                List<int> list = new List<int>();
                foreach (var item in selectedIds)
                {
                    var assetitem = item as AssetTreeItem<SpriteTrackData>;
                    if (assetitem != null)
                    {
                        SpriteTrackData data = assetitem.GetData();

                        if (data.ShowMode == SpriteShowMode.Scene)
                        {
                            if (data.SceneData.CsReferences != null)
                            {
                                list.Add(data.SceneData.InstanceId);
                            }
                        }
                        else if (data.ShowMode == SpriteShowMode.Prefabs)
                        {
                            if (data.GameObjectData.GoData.Go != null)
                            {
                                list.Add(data.GameObjectData.GoData.InstanceId);
                            }
                        }
                        else if (data.ShowMode == SpriteShowMode.Atlas)
                        {
                            if (data.AtlasData.RefData.Target != null)
                            {
                                list.Add(data.AtlasData.RefData.InstanceId);
                            }
                        }
                    }
                }

                if (list.Count >0)
                {
                    EditorGUIUtility.PingObject(list[0]);
                    Selection.instanceIDs = list.ToArray();
                }
                else
                {
                    Selection.instanceIDs = new int[0];
                }
            }
        }

        public List<string> Refresh()
        {
            if (_treeView != null)
            {
                _treeView.Reload();
            }
            return null;
        }

        public void Rebuild()
        {

        }

        public void GuiUpdate(bool isrunning, float delta)
        {

        }


        public void RenderGui()
        {
            RenderSearchField();
            RenderTree();
            RenderBotToolbar();
        }

        void RenderTree()
        {
            if (_treeView != null)
                _treeView.OnGUI(EditorContexts.mIns.GuiContext.SpriteTreeRect);
        }

        void RenderSearchField()
        {
            if (_treeView != null)
            {
                var newstring = _searchField.OnGUI(EditorContexts.mIns.GuiContext.SpriteSearchRect, _treeView.searchString);
                if (newstring != _treeView.searchString)
                {
                    _treeView.searchString = newstring;
                    var selects = _treeView.GetSelection();
                    if (selects != null && selects.Count > 0)
                    {
                        _treeView.SetSelection(selects, TreeViewSelectionOptions.RevealAndFrame);
                        //_treeView.FrameItem(selects[0]);
                    }
                }
            }
        }

        void RenderBotToolbar()
        {
            GUI.BeginGroup(EditorContexts.mIns.GuiContext.SpriteBotBarRect);

            var style = "miniButton";

            var md = AssetBundleEditorHelper.GetMainMode(EditorContexts.mIns.Mode);
            var lw = EditorGUIUtility.labelWidth;
            var lt = EditorGUI.indentLevel;

            EditorGUIUtility.labelWidth = 80;
            EditorGUI.indentLevel = 0;
            var newmd = (AssetWindow.WindowMode)EditorGUI.EnumPopup(new Rect(0, 0, 180, EditorGuiContext.GuiHeight), new GUIContent("Select Mode"), md);

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

            if (GUI.Button(new Rect(380, 0, 80, EditorGuiContext.GuiHeight), "Refresh", style))
            {
                EditorContexts.mIns.ForceModeChange(AssetWindow.WindowMode.ToolsSprite);
            }

            if (EditorContexts.mIns.GuiContext.UsablePaths != null && EditorContexts.mIns.GuiContext.UsablePaths.Count > 0)
            {

                if (GUI.Button(new Rect(500, 0, 100, EditorGuiContext.GuiHeight), "Select Path"))
                {
                    GenericMenu menu = EditorContexts.mIns.GuiContext.Menu;
                    if (menu != null)
                    {
                        EditorContexts.mIns.GuiContext.Menu.ShowAsContext();
                    }
                }

            }


            GUI.EndGroup();
        }


        public void Sort(MultiColumnHeader header, TreeViewItem root, IList<TreeViewItem> items)
        {
            
        }

        bool CheckTag(string newval, string current)
        {
            return newval != current && current != SpriteLogic.BuildInTag && newval != SpriteLogic.BuildInTag && current != SpriteLogic.ResourcesTag && newval != SpriteLogic.ResourcesTag;
        }

        public bool CellGui(TreeViewItem item, ref AssetRowGuiArgs args)
        {
            if (item is AssetTreeItem<SpriteTrackData>)
            {
                AssetTreeItem<SpriteTrackData> trackItem = item as AssetTreeItem<SpriteTrackData>;

                GUI.skin.label.richText = true;
                SpriteTopMenu topMenu = (SpriteTopMenu)args.Column;
                SpriteTrackData data = trackItem.GetData();
                if (topMenu == SpriteTopMenu.Name)
                {
                    return false;
                }
                else if (topMenu == SpriteTopMenu.Type)
                {
                    if (data.ShowMode == SpriteShowMode.Scene)
                    {
                        GUI.Label(args.CellRect, "Scene");
                    }
                    else if (data.ShowMode == SpriteShowMode.Prefabs)
                    {
                        GUI.Label(args.CellRect, "GameObject");
                    }
                    else if (data.ShowMode == SpriteShowMode.Atlas)
                    {
                        GUI.Label(args.CellRect, "Atlas");
                    }
                }
                else if (topMenu == SpriteTopMenu.BundleName)
                {
                    if (data.ShowMode == SpriteShowMode.Scene)
                    {
                        GUI.Label(args.CellRect, data.SceneData.SprData.BundleName);
                    }
                    else if (data.ShowMode == SpriteShowMode.Prefabs)
                    {
                        GUI.Label(args.CellRect, data.GameObjectData.SprData.BundleName);
                    }
                    else if (data.ShowMode == SpriteShowMode.Atlas)
                    {
                        GUI.Label(args.CellRect, data.AtlasData.SprData.BundleName);
                    }
                }
                else if (topMenu == SpriteTopMenu.AtlasName)
                {
                    if (data.ShowMode == SpriteShowMode.Scene)
                    {
                        if (item.depth ==2)
                        {
                            if (string.IsNullOrEmpty(data.SceneData.SprData.PackingTag) == false)
                            {
                                string newval =  GUI.TextField(args.CellRect, data.SceneData.SprData.PackingTag);
                                if (CheckTag(newval, data.SceneData.SprData.PackingTag)) 
                                {
                                    data.SceneData.SprData.Importer.spritePackingTag = newval;
                                    data.SceneData.SprData.PackingTag = newval;
                                    data.SceneData.SprData.Importer.SaveAndReimport();

                                    trackItem.SetData(data);
                                }
                            }
                                
                        }
                    }
                    else if (data.ShowMode == SpriteShowMode.Prefabs)
                    {
                        if (item.depth == 3)
                        {
                            if (string.IsNullOrEmpty(data.GameObjectData.SprData.PackingTag) == false)
                            {
                                string newval = GUI.TextField(args.CellRect, data.GameObjectData.SprData.PackingTag);
                                if (CheckTag(newval, data.GameObjectData.SprData.PackingTag))
                                {
                                    data.GameObjectData.SprData.Importer.spritePackingTag = newval;
                                    data.GameObjectData.SprData.PackingTag = newval;
                                    data.GameObjectData.SprData.Importer.SaveAndReimport();

                                    trackItem.SetData(data);
                                }
                            }
  
                        }
                    }
                    else if(data.ShowMode == SpriteShowMode.Atlas)
                    {
                        if (item.depth == 2)
                        {
                            if (string.IsNullOrEmpty(data.AtlasData.SprData.PackingTag) == false)
                            {
                                string newval = GUI.TextField(args.CellRect, data.AtlasData.SprData.PackingTag);
                                if (CheckTag(newval, data.AtlasData.SprData.PackingTag))
                                {
                                    data.AtlasData.SprData.Importer.spritePackingTag = newval;
                                    data.AtlasData.SprData.PackingTag = newval;
                                    data.AtlasData.SprData.Importer.SaveAndReimport();

                                    trackItem.SetData(data);
                                }
                            }
    
                        }
                    }
                }
                else if (topMenu == SpriteTopMenu.UsedCount)
                {
                    if (item.depth >0 && ( data.ShowMode == SpriteShowMode.Scene || data.ShowMode == SpriteShowMode.Prefabs))
                    {
                        GUI.Label(args.CellRect, data.UsedRefCount.ToString());
                    }
                }
                else if (topMenu == SpriteTopMenu.MemSize)
                {
                    if (data.ShowMode == SpriteShowMode.Scene)
                    {
                        if (item.depth == 2)
                        {
                            GUI.Label(args.CellRect, "Spr:" + data.SceneData.SprData.MemSize + " Tex:" + data.SceneData.SprData.TexMemSize);
                        }
                        else if(item.depth >0)
                        {
                            GUI.Label(args.CellRect, "Script:" + data.SceneData.MemSize);
                        }
                    }
                    else if (data.ShowMode == SpriteShowMode.Prefabs)
                    {
                        if (item.depth == 3)
                        {
                            GUI.Label(args.CellRect, "Spr:" + data.GameObjectData.SprData.MemSize + " Tex:" + data.GameObjectData.SprData.TexMemSize);

                        }
                        else if (item.depth > 0)
                        {
                            GUI.Label(args.CellRect, "Go:" + data.GameObjectData.GoData.MemSize);
                        }
                    }
                    else if (data.ShowMode == SpriteShowMode.Atlas)
                    {
                        if (item.depth == 2)
                        {
                            GUI.Label(args.CellRect, "Spr:" + data.AtlasData.SprData.MemSize + " Tex:" + data.AtlasData.SprData.TexMemSize);
                        }
                        else if (item.depth > 1)
                        {
                            GUI.Label(args.CellRect, "Ref:" + data.AtlasData.RefData.MemSize);
                        }
                    }
                }
                else if (topMenu == SpriteTopMenu.Size)
                {
                    if (data.ShowMode == SpriteShowMode.Atlas)
                    {
                        if(data.AtlasData.SprData.Sprite != null)
                            GUI.Label(args.CellRect,data.AtlasData.SprData.TexSize.ToString());

                    }
                }
                else if (topMenu == SpriteTopMenu.Info)
                {
                    if (data.ShowMode == SpriteShowMode.Scene)
                    {
                        if (item.depth == 1)
                        {
                            if (data.SceneData.CsReferences)
                            {
                                GUI.Label(args.CellRect, string.Format("Type :{0} Path:{1} ",
                                    AssetBundleEditorHelper.GetColorText("{0}", Color.black, data.SceneData.CsReferences),
                                    AssetBundleEditorHelper.GetColorText("{0}", Color.white, data.SceneData.Path)
                                    ));
                            }
   
                        }
                        else if (item.depth == 2)
                        {
                            if (data.SceneData.SprData.Sprite)
                                GUI.Label(args.CellRect, GetSprInfo(ref data.SceneData.SprData));
                        }
                    }
                    else if (data.ShowMode == SpriteShowMode.Prefabs)
                    {
                        if (item.depth == 1)
                        {
                            GUI.Label(args.CellRect, string.Format("AssetPath :{0} Path:{1} PrefabType:{2}",
                                AssetBundleEditorHelper.GetColorText("{0}",Color.black,data.GameObjectData.GoData.AssetPath),
                                AssetBundleEditorHelper.GetColorText("{0}", Color.white, data.GameObjectData.GoData.Path),
                               AssetBundleEditorHelper.GetColorText("{0}", Color.yellow, data.GameObjectData.GoData.PrefabType)));
                        }
                        else if (item.depth == 2)
                        {
                            GUI.Label(args.CellRect, GetSprInfo(ref data.GameObjectData.SprData));
                        }
                        else if (item.depth == 3)
                        {
                            if (data.GameObjectData.SprData.Sprite)
                            {
                                GUI.Label(args.CellRect, GetSprInfo(ref data.GameObjectData.SprData));
                            }
                        }
                    }
                    else if (data.ShowMode == SpriteShowMode.Atlas)
                    {
                        if (item.depth == 2)
                        {
                            if (data.AtlasData.SprData.Sprite)
                            {
                                GUI.Label(args.CellRect, GetSprInfo(ref data.AtlasData.SprData));
                            }
                        }
                        else if (item.depth == 3)
                        {
                            if (data.AtlasData.RefData.Target)
                            {
                                GUI.Label(args.CellRect, string.Format("{0} Path:{1}",
                                    AssetBundleEditorHelper.GetColorText("{0}", Color.black, data.AtlasData.RefData.Target),
                                    AssetBundleEditorHelper.GetColorText("{0}", Color.white, data.AtlasData.RefData.Path)));
                            }
                        }

                    }
                }
                GUI.skin.label.richText = false;
            }
            else
            {
                return false;
            }

            return true;
        }

        string GetSprInfo(ref SimpleSpriteInfo sprData)
        {
            return string.Format("PackingTag:{0} MipMap:{1} BundleName:{2} Path:{3}", 
                sprData.PackingTag,
                AssetBundleEditorHelper.GetColorText("{0}", Color.yellow, sprData.Mipmap),
                AssetBundleEditorHelper.GetColorText("{0}", Color.blue, sprData.BundleName),
                AssetBundleEditorHelper.GetColorText("{0}", Color.gray, sprData.AssetPath));
        }


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
                                if (dragargs.parentItem.parent != null)
                                {
                                    AssetTreeItem<SpriteTrackData> itemdata = dragargs.parentItem as AssetTreeItem<SpriteTrackData>;
                                    if (itemdata != null && itemdata.GetData().ShowMode == SpriteShowMode.Atlas && itemdata.depth == 1)
                                        OnDropDraggedElements(draggedRows, itemdata);
                                }
                            }

                            dragmd = validDrag ? DragAndDropVisualMode.Move : DragAndDropVisualMode.None;
                            break;
                        }
                    case AssetDragAndDropArgs.DragAndDropPosition.OutsideItems:
                        {

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

        void OnDropDraggedElements(List<TreeViewItem> draggedRows, AssetTreeItem<SpriteTrackData> parent)
        {
            var draggedElements = ListPool<SpriteTrackData>.Get();
            foreach (var value in draggedRows)
            {
                AssetTreeItem<SpriteTrackData> assetval = value as AssetTreeItem<SpriteTrackData>;
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

            string parentTag = parent.displayName;

            if (!string.IsNullOrEmpty(parentTag))
            {

                foreach (var data in draggedElements)
                {
                    var import = AssetImporter.GetAtPath(data.AtlasData.SprData.AssetPath) as TextureImporter;
                    if (import != null)
                    {
                        import.spritePackingTag = parentTag;
                        import.SaveAndReimport();
                    }
                }

                bool update = draggedElements.Count > 0;

                ListPool<SpriteTrackData>.Release(draggedElements);

                if (update)
                {
                    //refresh
                    EditorContexts.mIns.ForceModeChange(EditorContexts.mIns.Mode);
                }
            }
            else
            {
                Debug.LogError("Parent Tag is Empty");
            }

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
                headerContent = new GUIContent("Name", EditorGUIUtility.FindTexture("FilterByLabel")),
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
                headerContent = new GUIContent("Type", EditorGUIUtility.FindTexture("FilterByType")),
                contextMenuText = "Type",
                headerTextAlignment = TextAlignment.Left,
                sortedAscending = true,
                sortingArrowAlignment = TextAlignment.Left,
                width = 120,
                minWidth = 100,
                maxWidth = 160,
                autoResize = false,
                allowToggleVisibility = true
            });

            colwidth += colnums[colnums.Count - 1].width;
            colminwidth += colnums[colnums.Count - 1].minWidth;
            colmaxwidth += colnums[colnums.Count - 1].maxWidth;

            colnums.Add(new MultiColumnHeaderState.Column
            {
                headerContent = new GUIContent("AtlasName", "AtlasName"),
                contextMenuText = "AtlasName",
                headerTextAlignment = TextAlignment.Left,
                sortedAscending = false,
                sortingArrowAlignment = TextAlignment.Left,
                width = 120,
                minWidth = 120,
                maxWidth = 260,
                autoResize = false,
                allowToggleVisibility = true
            });

            colwidth += colnums[colnums.Count - 1].width;
            colminwidth += colnums[colnums.Count - 1].minWidth;
            colmaxwidth += colnums[colnums.Count - 1].maxWidth;

            colnums.Add(new MultiColumnHeaderState.Column
            {
                headerContent = new GUIContent("BundleName", "Asset Bundle Name"),
                contextMenuText = "BundleName",
                headerTextAlignment = TextAlignment.Left,
                sortedAscending = false,
                sortingArrowAlignment = TextAlignment.Left,
                width = 150,
                minWidth = 90,
                maxWidth = 200,
                autoResize = false,
                allowToggleVisibility = true
            });


            colnums.Add(new MultiColumnHeaderState.Column
            {
                headerContent = new GUIContent("UsedCount", "Used Atlas Count"),
                contextMenuText = "UsedCount",
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
                headerContent = new GUIContent("Size", "Memory Size"),
                contextMenuText = "Size",
                headerTextAlignment = TextAlignment.Left,
                sortedAscending = false,
                sortingArrowAlignment = TextAlignment.Left,
                width = 180,
                minWidth = 60,
                maxWidth = 230,
                autoResize = false,
                allowToggleVisibility = true
            });

            colwidth += colnums[colnums.Count - 1].width;
            colminwidth += colnums[colnums.Count - 1].minWidth;
            colmaxwidth += colnums[colnums.Count - 1].maxWidth;

            colnums.Add(new MultiColumnHeaderState.Column
            {
                headerContent = new GUIContent("TexSize", "Texture Size"),
                contextMenuText = "TexSize",
                headerTextAlignment = TextAlignment.Left,
                sortedAscending = false,
                sortingArrowAlignment = TextAlignment.Left,
                width = 160,
                minWidth = 160,
                maxWidth = 190,
                autoResize = false,
                allowToggleVisibility = true
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
                width = Mathf.Max(150, windowrect.width - 10 - colwidth),
                minWidth = Mathf.Max(100, windowrect.width - 10 - colmaxwidth),
                maxWidth = Mathf.Max(200, windowrect.width - 10 - colminwidth),
                autoResize = true
            });


            var state = new MultiColumnHeaderState(colnums.ToArray());
            return state;
        }
    }
}
#endif
