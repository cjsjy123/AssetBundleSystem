#if UNITY_EDITOR
using UnityEngine;
using System.Collections.Generic;
using CommonUtils;
using System.Reflection;
using UnityEditor;
using UnityEditor.IMGUI.Controls;
using System.Text;
using AssetBundleSystem;
using System;

namespace AssetBundleSystem.Editor
{

    internal class ProfilerGuiLogic : IGuiTree
    {
        public AssetWindow.WindowMode TypeMode
        {
            get
            {
                return AssetWindow.WindowMode.Profiler;
            }
        }
        [System.Flags]
        public enum ViewStatus
        {
            Basic =1,
            Helper =2,
            Detail =4,
        }

        public enum TopMenu
        {
            Name,
            /// <summary>
            /// Type root or ab or task
            /// </summary>
            Type,
            Size,
            RefCount,
            GameRefCount,
            Pin,
            Detail,
            Info,
        }

        private ViewStatus _viewstatus = ViewStatus.Basic;
        private int _detailhashcode;
        private string _detailstring ="";
        private const int defaultmax = 6;

        private AssetBundleFeature _feature;
        private MethodInfo _readmd;
        private MethodInfo _writemd;
        private string _selectassetbundle;

        private int _maxref;
        private int _maxtaskcount;
        private StringBuilder _stringbuilder;
        private Vector2 _scroll;
        private int _sortval;
        private SearchField _searchField;
        private TreeView _treeView;
        private TreeViewState _treeViewState;
        private TreeColumnHeader _columnHeader;
        private MultiColumnHeaderState _mMultiColumnHeaderState;
        private AssetTreeModel<RuntimeProfilerData> _model;

        private bool HasStatus(ViewStatus data)
        {
            return AssetBundleEditorHelper.HasEnum(_viewstatus, data);
        }

        public void Destory()
        {
            EditorUtility.UnloadUnusedAssetsImmediate();
            Resources.UnloadUnusedAssets();
        }

        public void GuiUpdate(bool isrunning, float delta)
        {

        }

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
                _treeView = new AssetTreeView<RuntimeProfilerData>(_treeViewState, _columnHeader, AssetWindow.WindowMode.Profiler);
            else
            {
                _columnHeader.Repaint();
                _treeView.Repaint();
            }

            //search field
            _searchField.downOrUpArrowKeyPressed -= PressSearch;
            _searchField.downOrUpArrowKeyPressed += PressSearch;

            _model = AssetTreeManager.mIns.GetModel<RuntimeProfilerData>();
        }

        void PressSearch()
        {
            _treeView.SetFocusAndEnsureSelectedItem();
        }


        public void RefreshGuiInfo(EditorContexts contexts)
        {
            float wid = contexts.GuiContext.WindowRect.width;
            float height = contexts.GuiContext.WindowRect.height;

            float usedwid = 0;
            float usedheight =  EditorGuiContext.GuiHeight;

            contexts.GuiContext.ProfilerCurveDxy = new Vector2(10, 30);
            //
            if (HasStatus(ViewStatus.Helper))
            {
                contexts.GuiContext.ProfilerHelperRect = new Rect(0, EditorGuiContext.GuiHeight, wid, 2 * EditorGuiContext.GuiHeight);

                usedheight += 2 * EditorGuiContext.GuiHeight;
            }

            contexts.GuiContext.ProfilerToolbarRect = new Rect(0, usedheight, wid, EditorGuiContext.GuiHeight);

            usedheight += EditorGuiContext.GuiHeight;

            contexts.GuiContext.ProfilerCurveRect = new Rect(10, usedheight, wid-20, 120);

            usedheight += contexts.GuiContext.ProfilerCurveRect.height +5;//space =5

            contexts.GuiContext.ProfilerSearchRect = new Rect(0, usedheight, wid, EditorGuiContext.GuiHeight);

            usedheight += EditorGuiContext.GuiHeight;

            contexts.GuiContext.ProfilerTreeRect = new Rect(0, usedheight, wid, Mathf.Max(100,height- usedheight - EditorGuiContext.GuiHeight));

            if(HasStatus( ViewStatus.Detail))
            {
                usedwid = wid * 2/3f;

                contexts.GuiContext.ProfilerDetailRect = new Rect(usedwid,usedheight,wid - usedwid,contexts.GuiContext.ProfilerTreeRect.height);
            }

            contexts.GuiContext.ProfilerBotBarRect = new Rect(0, height - EditorGuiContext.GuiHeight, wid, EditorGuiContext.GuiHeight);
        }

        public void RenderGui()
        {
            RenderHelper();

            if(Application.isPlaying && AssetBundleConfig.IsProfiler() && !AssetBundleConfig.IsSimulator())
            {
                RenderToolbar();
                RenderCurve();
                RenderSearchField();
                RenderTree();

                RenderDetail();

            }
            RenderBotToolbar();
        }

        void RenderHelper()
        {
            if(AssetBundleConfig.IsSimulator())
            {
                EditorGUI.HelpBox(EditorContexts.mIns.GuiContext.ProfilerHelperRect, "cant profiler in simulator", MessageType.Error);
                _viewstatus |= ViewStatus.Helper;
            }
            else if (!Application.isPlaying)
            {
                EditorGUI.HelpBox(EditorContexts.mIns.GuiContext.ProfilerHelperRect, "should be in playemode", MessageType.Error);

                if(GUI.Button(new Rect(EditorContexts.mIns.GuiContext.WindowRect.width/2 -100,4*EditorGuiContext.GuiHeight,200,EditorGuiContext.GuiHeight),"Play"))
                {
                    EditorApplication.isPlaying = true;
                }
                _viewstatus |= ViewStatus.Helper;
            }
            else if(!AssetBundleConfig.IsProfiler())
            {
                EditorGUI.HelpBox(EditorContexts.mIns.GuiContext.ProfilerHelperRect, "should open profiler in config", MessageType.Error);

                if (GUI.Button(new Rect(EditorContexts.mIns.GuiContext.WindowRect.width / 2 - 100, 4 * EditorGuiContext.GuiHeight, 200, EditorGuiContext.GuiHeight), "Profiler"))
                {
                    AssetBundleConfig.DebugMode |= DebugMode.ProfilerLog;
                }
                _viewstatus |= ViewStatus.Helper;
            }
            else
            {
                _viewstatus &= ~ViewStatus.Helper;
            }
        }

        AssetBundleContext ReadRuntimeContext()
        {
            if(_feature == null)
            {
                var type = typeof(AssetBundleLoadManager);
                var fs = type.GetField("_assetBundleFeature", BindingFlags.NonPublic| BindingFlags.Instance);
                _feature = fs.GetValue(AssetBundleLoadManager.mIns) as AssetBundleFeature;

                var featuretype = typeof(AssetBundleFeature);
                _readmd = featuretype.GetMethod("StartRead", BindingFlags.NonPublic | BindingFlags.Instance);
                _writemd = featuretype.GetMethod("EndRead", BindingFlags.NonPublic | BindingFlags.Instance);
            }

            var value = _readmd == null? null: _readmd.Invoke(_feature, new object[] { });
            if (value == null)
                throw new System.NullReferenceException("Cant read context");

            return (AssetBundleContext)value;

        }

        void WriteContext(ref AssetBundleContext context)
        {
            if(_writemd != null)
            {
                _writemd.Invoke(AssetBundleLoadManager.mIns, new object[] { context });
            }
        }

        void RenderCurve()
        {
            GUI.BeginGroup(EditorContexts.mIns.GuiContext.ProfilerCurveRect, GUI.skin.FindStyle("box"));

            //curve
            var context = ReadRuntimeContext();

            var s = GUI.skin.label.font.fontSize;
            float w = EditorContexts.mIns.GuiContext.ProfilerCurveRect.width;
            float h = EditorContexts.mIns.GuiContext.ProfilerCurveRect.height;
            var dxy = EditorContexts.mIns.GuiContext.ProfilerCurveDxy;

            long frame = EditorContexts.mIns.GuiContext.UserSelectFrame >=0? EditorContexts.mIns.GuiContext.UserSelectFrame :context.ProfilerData.GetLastestData().Frame;

            string info = null;
            if(_selectassetbundle != null)
            {
                info = "show select assetbundle frame info >>Current:" + frame;
            }
            else
            {
                info = "it will show the gameassetbundle && task frame info totally >>Current:" + frame;
            }

            info = AssetBundleEditorHelper.GetColorText("{0}", Color.blue, info);

            EditorStyles.helpBox.richText = true;
            EditorGUI.HelpBox(new Rect(dxy.y, 0, w - 2 * dxy.y, EditorGuiContext.GuiHeight), info , MessageType.Info);
            EditorStyles.helpBox.richText = false;

            Handles.DrawLine(new Vector3(dxy.y, EditorGuiContext.GuiHeight, 0),new Vector3(dxy.y, h-dxy.x+5,0));
            Handles.DrawLine(new Vector3(w- dxy.y, EditorGuiContext.GuiHeight, 0), new Vector3(w - dxy.y, h - dxy.x + 5, 0));
            Handles.DrawLine(new Vector3(dxy.y, h - dxy.x + 5, 0), new Vector3(w - dxy.y, h - dxy.x + 5, 0));

            if(context.ProfilerData.Datas != null && context.ProfilerData.DataCount >0)
            {
                EditorContexts.mIns.Repaint();
            }

            RenderAssetBundleCurve(ref context);

            RenderTaskCurve(ref context);


            //Y axis
            int topval = Mathf.Max(defaultmax, _maxtaskcount);
            float step = (h - dxy.x - dxy.y) / 3;

            Color tempcol = GUI.skin.label.normal.textColor;
            GUI.skin.label.normal.textColor = Color.blue;

            for (int i = 0; i <= 3; i++)
            {
                GUI.Label(new Rect(30, h - dxy.x - step * i - s, 100, EditorGuiContext.GuiHeight), (topval * i / 3).ToString());
            }

            //Y
            topval = Mathf.Max(defaultmax, _maxref);
            step = (h - dxy.x - dxy.y) / 3;
            GUI.skin.label.normal.textColor = Color.red;

            for (int i = 0; i <= 3; i++)
            {
                GUI.Label(new Rect(0, h - dxy.x - step * i - s, 100, EditorGuiContext.GuiHeight), (topval * i / 3).ToString());
            }

            GUI.skin.label.normal.textColor = tempcol;
            EnsureCurveEvent(ref context);
            //WriteContext(ref context);

            GUI.EndGroup();
        }

        static Vector3[] _sharedv2 = new Vector3[2];

        void RenderAssetBundleCurve(ref AssetBundleContext context)
        {
            var profilerdata = context.ProfilerData;
            var alldatas = profilerdata.Datas;
            if(alldatas != null && profilerdata.Datas != null && profilerdata.DataCount >0)
            {
                var color = Handles.color;
                Handles.color = Color.red;

                int capacity = profilerdata.Datas.Length;
                var dxy = EditorContexts.mIns.GuiContext.ProfilerCurveDxy;
                float eachx = (EditorContexts.mIns.GuiContext.ProfilerCurveRect.width - 2*dxy.y) / capacity;

                int viewindex = 0;
                if(profilerdata.DataCount >= profilerdata.Datas.Length)
                {
                    int offset = profilerdata.Offset;
     
                    if (_selectassetbundle == null)
                    {
                        for (int i = capacity - 1; i >= 0; --i, viewindex++)
                        {
                            int fixoffset = AssetBundleFunction.GetRingOffset(offset + i, 0, profilerdata.Datas.Length - 1);
                            int lastoffset = AssetBundleFunction.GetRingOffset(offset + i-1, 0, profilerdata.Datas.Length - 1);

                            CurveNoSelectAb(ref profilerdata, fixoffset, lastoffset, viewindex, eachx);
                        }
                    }
                    else
                    {
                        for (int i = capacity - 1; i >= 0; --i, viewindex++)
                        {
                            int fixoffset = AssetBundleFunction.GetRingOffset(offset + i , 0, profilerdata.Datas.Length - 1);
                            int lastoffset = AssetBundleFunction.GetRingOffset(offset + i-1, 0, profilerdata.Datas.Length - 1);

                            CurveSelectAb(ref profilerdata, fixoffset, lastoffset, viewindex, eachx);
                        }
                    }
                }
                else
                {
                    int len = profilerdata.Offset;
                    if (_selectassetbundle == null)
                    {
                       
                        for (int i = len-1; i >0; --i, viewindex++)
                        {
                            CurveNoSelectAb(ref profilerdata, i, i - 1, viewindex, eachx);
                        }
                    }
                    else
                    {

                        for (int i = len - 1; i > 0; --i, viewindex++)
                        {
                            CurveSelectAb(ref profilerdata, i,i-1, viewindex, eachx);
                        }
                    }
                }

                Handles.color = color;
            }
        }

        void CurveNoSelectAb(ref AssetProfilerData profilerdata, int i,int last,int viewindex,float eachx)
        {
            if(i != profilerdata.Offset)
            {
                FrameProfilerData lastframedata = profilerdata.Datas[last];
                FrameProfilerData framedata = profilerdata.Datas[i];
                _maxref = Mathf.Max(defaultmax, _maxref, framedata.RefCount, lastframedata.RefCount);


                int total = profilerdata.Datas.Length;

                Vector3 p0 = GetPoint(viewindex + 1, eachx, lastframedata.RefCount, _maxref);
                Vector3 p1 = GetPoint(viewindex , eachx, framedata.RefCount, _maxref);


                if (EditorContexts.mIns.GuiContext.UserSelectFrame >= 0 && framedata.Frame == EditorContexts.mIns.GuiContext.UserSelectFrame)
                {
                    float h = EditorContexts.mIns.GuiContext.ProfilerCurveRect.height;
                    var dxy = EditorContexts.mIns.GuiContext.ProfilerCurveDxy;
                    var col = Handles.color;
                    Handles.color = Color.green;
                    Handles.DrawLine(new Vector3(p1.x, EditorGuiContext.GuiHeight, 0), new Vector3(p1.x, h - dxy.x + 5, 0));

                    Handles.color = col;
                }

                _sharedv2[0] = p0;
                _sharedv2[1] = p1;

                Handles.DrawAAPolyLine(_sharedv2);
            }

        }

        void CurveSelectAb(ref AssetProfilerData profilerdata, int i,int last, int viewindex, float eachx)
        {
            FrameProfilerData lastframedata = profilerdata.Datas[last];
            FrameProfilerData framedata = profilerdata.Datas[i];
            int total = profilerdata.Datas.Length;

            Vector3 p0 = GetPoint(viewindex + 1, eachx, 0, 0);
            Vector3 p1 = GetPoint(viewindex , eachx, 0, 0);

            if (lastframedata.Histories != null)
            {
                foreach (var item in lastframedata.Histories)
                {
                    if (item.AssetBundlename.Equals(_selectassetbundle))
                    {
                        int objref = item.ObjRefCount;
                        _maxref = Mathf.Max(defaultmax, _maxref, objref);
                        p0 = GetPoint(viewindex + 1, eachx, objref, _maxref);
                        break;
                    }
                }
            }

            if (framedata.Histories != null)
            {
                foreach (var item in framedata.Histories)
                {
                    if (item.AssetBundlename.Equals(_selectassetbundle))
                    {
                        int objref = item.ObjRefCount;
                        _maxref = Mathf.Max(defaultmax, _maxref, objref);
                        p1 = GetPoint(viewindex, eachx, objref, _maxref);
                        break;
                    }
                }
            }

            if (EditorContexts.mIns.GuiContext.UserSelectFrame >= 0 && framedata.Frame == EditorContexts.mIns.GuiContext.UserSelectFrame)
            {
                float h = EditorContexts.mIns.GuiContext.ProfilerCurveRect.height;
                var dxy = EditorContexts.mIns.GuiContext.ProfilerCurveDxy;
                var col = Handles.color;
                Handles.color = Color.green;
                Handles.DrawLine(new Vector3(p1.x, EditorGuiContext.GuiHeight, 0), new Vector3(p1.x, h - dxy.x + 5, 0));

                Handles.color = col;
            }

            _sharedv2[0] = p0;
            _sharedv2[1] = p1;

            Handles.DrawAAPolyLine(_sharedv2);
        }


        void CurveNoSelectTask(ref AssetProfilerData profilerdata, int i, int last, int viewindex, float eachx)
        {
            if (i != profilerdata.Offset)
            {
                FrameProfilerData lastframedata = profilerdata.Datas[last];
                FrameProfilerData framedata = profilerdata.Datas[i];
                _maxtaskcount = Mathf.Max(defaultmax, _maxtaskcount, framedata.TaskCount, lastframedata.TaskCount);


                int total = profilerdata.Datas.Length;

                Vector3 p0 = GetPoint(viewindex + 1, eachx, lastframedata.TaskCount, _maxtaskcount);
                Vector3 p1 = GetPoint(viewindex, eachx, framedata.TaskCount, _maxtaskcount);


                _sharedv2[0] = p0;
                _sharedv2[1] = p1;

                Handles.DrawAAPolyLine(_sharedv2);
            }

        }

        Vector3 GetPoint(int index,float eachx,int value,int maxvalue)
        {
            var rect = EditorContexts.mIns.GuiContext.ProfilerCurveRect;
            var dxy = EditorContexts.mIns.GuiContext.ProfilerCurveDxy;
            float height = rect.height- dxy.x;

            float x = dxy.y + index *eachx;

            float y = height - (maxvalue ==0?0: value * (height- dxy.y) / maxvalue);
            return new Vector3(x, y, 0);
        }

        void RenderTaskCurve(ref AssetBundleContext context)
        {
            var profilerdata = context.ProfilerData;
            var alldatas = profilerdata.Datas;
            if (alldatas != null && profilerdata.DataCount > 0)
            {
                var color = Handles.color;
                Handles.color = Color.blue;

                int capacity = profilerdata.Datas.Length;
                var dxy = EditorContexts.mIns.GuiContext.ProfilerCurveDxy;
                float eachx = (EditorContexts.mIns.GuiContext.ProfilerCurveRect.width - 2 * dxy.y) / capacity;

                int viewindex = 0;
                if (profilerdata.DataCount >= profilerdata.Datas.Length)
                {
                    int offset = profilerdata.Offset;

                    for (int i = capacity - 1; i >= 0; --i, viewindex++)
                    {
                        int fixoffset = AssetBundleFunction.GetRingOffset(offset + i, 0, profilerdata.Datas.Length - 1);
                        int lastoffset = AssetBundleFunction.GetRingOffset(offset + i - 1, 0, profilerdata.Datas.Length - 1);

                        CurveNoSelectTask(ref profilerdata, fixoffset, lastoffset, viewindex, eachx);
                    }
                }
                else
                {
                    int len = profilerdata.Offset;
                    for (int i = len - 1; i > 0; --i, viewindex++)
                    {
                        CurveNoSelectTask(ref profilerdata, i, i - 1, viewindex, eachx);
                    }
                }

                Handles.color = color;
            }
        }

        void EnsureCurveEvent(ref AssetBundleContext context)
        {
            if(Event.current.type == EventType.MouseDown)
            {
                if(Event.current.button == 0)
                {
                    var x = Event.current.mousePosition.x;
                    var dxy = EditorContexts.mIns.GuiContext.ProfilerCurveDxy;
                    if (x > dxy.y && x < EditorContexts.mIns.GuiContext.WindowRect.width - dxy.y)
                    {

                        var data = context.ProfilerData;
                        int maxcnt = Mathf.Min(data.DataCount, data.Datas == null?0: data.Datas.Length);
                        if(maxcnt >0 && data.Datas != null)
                        {

                            float percent = (x - dxy.y) / (EditorContexts.mIns.GuiContext.ProfilerCurveRect.width - 2* dxy.y);
                            float maxpercent = maxcnt / (float)(data.Datas.Length);
            
                            if (percent < maxpercent || Mathf.Abs(percent-maxpercent) < 0.001f)
                            {
                               
                                if(data.DataCount >= data.Datas.Length)// is full
                                {
                                    int index = Mathf.RoundToInt(percent * data.Datas.Length );
                                    int realindex = AssetBundleFunction.GetRingOffset(data.Offset - index, 0, data.Datas.Length - 1);
                                    var framedata = data.Datas[realindex];
                        
                                    SelectFrame(framedata.Frame);
                                }
                                else
                                {
                                    int index = Mathf.RoundToInt(percent * data.Datas.Length);
                                    var framedata = data.Datas[data.Offset- index];

                                    SelectFrame(framedata.Frame);
                                }

                            }
                        }
                    }

                    //EditorContexts.mIns.UserSelectFrame =
                }
            }
            else if( Event.current.type == EventType.KeyDown)
            {
                long frame = EditorContexts.mIns.GuiContext.UserSelectFrame;
                if (frame >= 0 )
                {
                    if(Event.current.keyCode == KeyCode.LeftArrow)
                    {
                        SelectFrame(frame + 1);
                    }
                    else if(Event.current.keyCode == KeyCode.RightArrow)
                    {
                        SelectFrame(frame - 1);
                    }
                }
            }
        }

        void RenderTree()
        {
            if (_treeView != null)
                _treeView.OnGUI(EditorContexts.mIns.GuiContext.ProfilerTreeRect);
        }

        void RenderSearchField()
        {
            if (_treeView != null)
            {
                var newstring = _searchField.OnGUI(EditorContexts.mIns.GuiContext.ProfilerSearchRect, _treeView.searchString);
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

        void RenderDetail()
        {
            if(HasStatus( ViewStatus.Detail))
            {
                using (var scope = new EditorGUILayout.VerticalScope(GUI.skin.FindStyle("box")))
                {
                    GUILayout.BeginArea(EditorContexts.mIns.GuiContext.ProfilerDetailRect, GUI.skin.FindStyle("box"));
                    int depth = GUI.depth;


                    if (GUILayout.Button( "X", "minibutton",GUILayout.Width(50)))
                    {
                        _viewstatus &= ~ViewStatus.Detail;
                    }

                    _scroll = GUILayout.BeginScrollView(_scroll);

                    GUILayout.Label(_detailstring);
                    GUILayout.EndScrollView();
                    GUILayout.EndArea();
                }
                //GUI.BeginGroup(EditorContexts.mIns.ProfilerDetailRect, GUI.skin.FindStyle("box"));

                //int depth = GUI.depth;

                //GUI.depth += 2;

                //if (GUI.Button(new Rect(0, 0, 50, EditorGuiContext.GuiHeight), "X","minibutton"))
                //{
                //    _viewstatus |= ~ViewStatus.Detail;
                //}
                //GUI.depth = depth;

                //_scroll = GUI.BeginScrollView(EditorContexts.mIns.ProfilerDetailRect, _scroll, EditorContexts.mIns.ProfilerDetailRect);
     
                //GUI.EndScrollView();

                //GUI.EndGroup();
            }
        }

        void RenderToolbar()
        {
            GUI.BeginGroup(EditorContexts.mIns.GuiContext.ProfilerToolbarRect);
            var style = "miniButton";

            var isprofilering = AssetBundleLoadManager.mIns.IsProfilering();

            var newvalue = GUI.Toggle(new Rect(10, 0, 80, EditorGuiContext.GuiHeight), isprofilering, "Record", style);

            if(newvalue != isprofilering)
            {
                AssetBundleLoadManager.mIns.SetProfiler(newvalue);
            }

            if (GUI.Button(new Rect(100, 0, 80, EditorGuiContext.GuiHeight), "current", style))
            {
                var context = ReadRuntimeContext();
                SelectFrame(context.ProfilerData.GetLastestData().Frame);
            }

            if (GUI.Button(new Rect(190, 0, 80, EditorGuiContext.GuiHeight), "disselect", style))
            {
                SelectFrame(-1);
            }

            GUI.EndGroup();
        }

        void RenderBotToolbar()
        {
            GUI.BeginGroup(EditorContexts.mIns.GuiContext.ProfilerBotBarRect);

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

 
            GUI.EndGroup();
        }

        void SelectFrame(long frame)
        {
            if(frame != EditorContexts.mIns.GuiContext.UserSelectFrame)
            {
                EditorContexts.mIns.GuiContext.UserSelectFrame = frame;
                EditorContexts.mIns.ForceModeChange(AssetWindow.WindowMode.Profiler);
            }
        }

        #region Tree
        public void SelectionChanged(IList<TreeViewItem> selectedIds)
        {

            if(selectedIds != null && selectedIds.Count >0)
            {
                var item = selectedIds[0] as AssetTreeItem<RuntimeProfilerData>;
                if(item !=null)
                {
                    var data = item.GetData();
                    if(data.IsTaskItem())
                    {
                    }
                    else if(data.IsGameAssetBundleItem())
                    {
                        _selectassetbundle = data.History.Value.AssetBundlename;
                    }
                    else if(data.IsAssetInfoItem())
                    {
                        var parent = item.parent;
                        if(parent != null)
                        {
                            var parentitem = parent as AssetTreeItem<RuntimeProfilerData>;
                            if(parentitem != null)
                            {
                                _selectassetbundle = parentitem.GetData().History.Value.AssetBundlename;
                            }
                        }
                    }
                    else
                    {
                        _selectassetbundle = null;
                    }
                }
            }
            else
            {
                _selectassetbundle = null;
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

        public void Rebuild()
        {
 
        }
        #region sort
        public void Sort(MultiColumnHeader header, TreeViewItem rootItem, IList<TreeViewItem> items)
        {
            int sortIndex = header.sortedColumnIndex;
            bool ascending = header.IsSortedAscending(sortIndex);
            var topmenutype = (TopMenu)sortIndex;
            if (topmenutype == TopMenu.Name)//name
            {
                SortDefault(rootItem, ascending);
            }
            else if (topmenutype == TopMenu.Type)//restype
            {
                SortType(rootItem, ascending);
            }
            else if (topmenutype == TopMenu.Size)
            {
                SortSize(rootItem, ascending);
            }
            else if (topmenutype == TopMenu.RefCount)
            {
                SortRefCount(rootItem, ascending);
            }
            else if (topmenutype == TopMenu.GameRefCount)
            {
                SortObjectRefCount(rootItem, ascending);
            }
        }

        void Sort<T>(T rootItem, Comparison<TreeViewItem> func, Comparison<int> intfunc) where T : TreeViewItem
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

        void SortType(TreeViewItem rootItem, bool ascending)
        {
            _sortval = ascending ? 1 : -1;
            Sort(rootItem, SortTypeFunc, SortTypeFunc);
        }


        void SortRefCount(TreeViewItem rootItem, bool ascending)
        {
            _sortval = ascending ? 1 : -1;
            Sort(rootItem, SortRefCountFunc, SortRefCountFunc);
        }

        void SortObjectRefCount(TreeViewItem rootItem, bool ascending)
        {
            _sortval = ascending ? 1 : -1;
            Sort(rootItem, SortObjRefCountFunc, SortObjRefCountFunc);
        }

        int SortDefualtFunc(TreeViewItem left, TreeViewItem right)
        {
            return _sortval * (left.id- right.id);
        }

        int SortDefualtFunc(int left, int right)
        {
            return _sortval * (left - right);
        }

        int SortSizeFunc(TreeViewItem left, TreeViewItem right)
        {
            AssetTreeItem<RuntimeProfilerData> leftItem = left as AssetTreeItem<RuntimeProfilerData>;
            AssetTreeItem<RuntimeProfilerData> rightItem = right as AssetTreeItem<RuntimeProfilerData>;
            if (leftItem != null && rightItem != null)
            {
                var ld = leftItem.GetData();
                var rd = rightItem.GetData();
                long leftsize = ld.AssetInfo.HasValue?ld.AssetInfo.Value.Size:0;
                long rightsize = rd.AssetInfo.HasValue ? rd.AssetInfo.Value.Size : 0;
                return _sortval * (int)(leftsize - rightsize);
            }
            return 0;
        }

        int SortSizeFunc(int left, int right)
        {
            RuntimeProfilerData ld;
            RuntimeProfilerData rd;
            if (_model.GetItem(left, out ld) && _model.GetItem(right, out rd))
            {
                long leftsize = ld.AssetInfo.HasValue ? ld.AssetInfo.Value.Size : 0;
                long rightsize = rd.AssetInfo.HasValue ? rd.AssetInfo.Value.Size : 0;
                return _sortval * (int)(leftsize - rightsize);
            }

            return 0;
        }

        int GetTypeLevel(ref RuntimeProfilerData data)
        {
            if(data.IsGameAssetBundleItem())
            {
                return 0;
            }
            else if(data.IsAssetInfoItem())
            {
                return 1;
            }
            else if(data.IsTaskItem())
            {
                return 2;
            }
            return 3;
        }

        int SortTypeFunc(TreeViewItem left, TreeViewItem right)
        {
            AssetTreeItem<RuntimeProfilerData> leftItem = left as AssetTreeItem<RuntimeProfilerData>;
            AssetTreeItem<RuntimeProfilerData> rightItem = right as AssetTreeItem<RuntimeProfilerData>;
            if (leftItem != null && rightItem != null)
            {
                var ld = leftItem.GetData();
                var rd = rightItem.GetData();
                long leftsize = GetTypeLevel(ref ld);
                long rightsize = GetTypeLevel(ref rd);
                return _sortval * (int)(leftsize - rightsize);
            }
            return 0;
        }

        int SortTypeFunc(int left, int right)
        {
            RuntimeProfilerData ld;
            RuntimeProfilerData rd;
            if (_model.GetItem(left, out ld) && _model.GetItem(right, out rd))
            {
                long leftsize = GetTypeLevel(ref ld);
                long rightsize = GetTypeLevel(ref rd);
                return _sortval * (int)(leftsize - rightsize);
            }

            return 0;
        }

        int SortRefCountFunc(TreeViewItem left, TreeViewItem right)
        {
            AssetTreeItem<RuntimeProfilerData> leftItem = left as AssetTreeItem<RuntimeProfilerData>;
            AssetTreeItem<RuntimeProfilerData> rightItem = right as AssetTreeItem<RuntimeProfilerData>;
            if (leftItem != null && rightItem != null)
            {
                var ld = leftItem.GetData();
                var rd = rightItem.GetData();
                long leftsize = ld.History.HasValue ? ld.History.Value.UserRefCount : 0;
                long rightsize = rd.History.HasValue ? rd.History.Value.UserRefCount : 0;
                return _sortval * (int)(leftsize - rightsize);
            }
            return 0;
        }

        int SortRefCountFunc(int left, int right)
        {
            RuntimeProfilerData ld;
            RuntimeProfilerData rd;
            if (_model.GetItem(left, out ld) && _model.GetItem(right, out rd))
            {
                long leftsize = ld.History.HasValue ? ld.History.Value.UserRefCount : 0;
                long rightsize = rd.History.HasValue ? rd.History.Value.UserRefCount : 0;
                return _sortval * (int)(leftsize - rightsize);
            }

            return 0;
        }

        int SortObjRefCountFunc(TreeViewItem left, TreeViewItem right)
        {
            AssetTreeItem<RuntimeProfilerData> leftItem = left as AssetTreeItem<RuntimeProfilerData>;
            AssetTreeItem<RuntimeProfilerData> rightItem = right as AssetTreeItem<RuntimeProfilerData>;
            if (leftItem != null && rightItem != null)
            {
                var ld = leftItem.GetData();
                var rd = rightItem.GetData();
                long leftsize = ld.History.HasValue ? ld.History.Value.UserRefCount : 0;
                long rightsize = rd.History.HasValue ? rd.History.Value.UserRefCount : 0;
                return _sortval * (int)(leftsize - rightsize);
            }
            return 0;
        }

        int SortObjRefCountFunc(int left, int right)
        {
            RuntimeProfilerData ld;
            RuntimeProfilerData rd;
            if (_model.GetItem(left, out ld) && _model.GetItem(right, out rd))
            {
                long leftsize = ld.History.HasValue ? ld.History.Value.UserRefCount : 0;
                long rightsize = rd.History.HasValue ? rd.History.Value.UserRefCount : 0;
                return _sortval * (int)(leftsize - rightsize);
            }

            return 0;
        }

        #endregion

        public bool CellGui(TreeViewItem item, ref AssetRowGuiArgs args)
        {
            TopMenu topMenu = (TopMenu)args.Column;
            var data = (item as AssetTreeItem<RuntimeProfilerData>).GetData();
            if(topMenu == TopMenu.Name)
            {
                return false;
            }
            else if(topMenu == TopMenu.Type)
            {
                if(data.IsTaskItem())
                {
                    GUI.Label(args.CellRect, "Task");
                }
                else if(data.IsGameAssetBundleItem())
                {
                    GUI.Label(args.CellRect, "GameAssetBundle");
                }
                else if(data.IsAssetInfoItem())
                {
                    GUI.Label(args.CellRect, "Asset In AB");
                }
                else
                {
                    GUI.Label(args.CellRect, "Root");
                }
            }
            else if(topMenu == TopMenu.Size)
            {
                GuiSize(ref args, ref data);
            }
            else if(topMenu == TopMenu.Detail)
            {
                GuiDetail(ref args, ref data);
            }
            else if (topMenu == TopMenu.RefCount)
            {
                GuiRefCount(ref args, ref data);
            }
            else if (topMenu == TopMenu.GameRefCount)
            {
                GuiObjRefCount(ref args, ref data);
            }
            else if(topMenu== TopMenu.Pin)
            {
                GuiPin(ref args, ref data);
            }
            else if(topMenu == TopMenu.Info)
            {
                if(data.IsTaskItem())
                {
                    GUI.Label(args.CellRect, "This is a Task: " + data.Task.Value.ToString());
                }
                else if(data.IsGameAssetBundleItem())
                {
                    GUI.Label(args.CellRect, "This is an GameAssetBundle " + data.History.Value.ToString());
                }
                else if(data.IsAssetInfoItem())
                {
                    GUI.Label(args.CellRect, "This is an AssetInfo " + data.AssetInfo.Value.ToString());
                }
            }
            else
            {
                return false;
            }

            return true;
        }

        void GuiPin(ref AssetRowGuiArgs args, ref RuntimeProfilerData data)
        {
            if (data.IsGameAssetBundleItem())
            {
                var cnt = data.History.Value.ObjRefCount;
                if(cnt ==0)
                {
                    GUI.enabled = false;
                }

                if (GUI.Button(args.CellRect,"Pin"))
                {
                    var history = data.History.Value;

                    List<GameObject> list = ListPool<GameObject>.Get();
                    foreach(var path in history.ObjectReferences)
                    {
                        GameObject gameObject = GameObject.Find(path);
                        if(gameObject != null)
                        {
                            list.Add(gameObject);
                        }
                    }

                    Selection.objects = list.ToArray();
                    EditorGUIUtility.PingObject(list[0]);
                    ListPool<GameObject>.Release(list);
                }

                if (cnt == 0)
                {
                    GUI.enabled = true;
                }
            }
        }

        void GuiObjRefCount(ref AssetRowGuiArgs args, ref RuntimeProfilerData data)
        {
            if (data.IsGameAssetBundleItem())
            {
                var cnt = data.History.Value.ObjectReferences == null ? 0 : data.History.Value.ObjectReferences.Count;
                GUI.Label(args.CellRect, cnt.ToString());
            }
        }

        void GuiRefCount(ref AssetRowGuiArgs args, ref RuntimeProfilerData data)
        {
            if (data.IsGameAssetBundleItem())
            {
                GUI.Label(args.CellRect, data.History.Value.UserRefCount.ToString());
            }
        }

        void GuiSize(ref AssetRowGuiArgs args, ref RuntimeProfilerData data)
        {
            if(data.IsTaskItem())
            {
                GUI.Label(args.CellRect, data.Task.Value.AssetInfo.AssetSize.ToString());
            }
            else if(data.IsGameAssetBundleItem())
            {
                GUI.Label(args.CellRect, data.History.Value.AssetBundleSize.ToString());
            }
        }

        void GuiDetail(ref AssetRowGuiArgs args,ref RuntimeProfilerData data)
        {
            if (data.IsTaskItem())
            {
                if (GUI.Button(args.CellRect, "Detail", "minibutton"))
                {
                    bool show = false;
                    if (HasStatus(ViewStatus.Detail))
                    {
                        var code = data.Task.Value.GetHashCode();
                        if (_detailhashcode == code)
                        {
                            _viewstatus &= ~ViewStatus.Detail;
                            _detailstring = "";
                            _detailhashcode = 0;
                        }
                        else
                        {
                            show = true;
                            _detailhashcode = code;
                        }
                    }
                    else
                    {
                        _viewstatus |= ViewStatus.Detail;
                        show = true;
                        _detailhashcode = data.Task.Value.GetHashCode();
                    }
                    if (show)
                    {
                        if (_stringbuilder == null)
                        {
                            _stringbuilder = new StringBuilder();
                        }
                        else
                        {
                            _stringbuilder.Length = 0;
                        }

                        data.Task.Value.ToDetail(_stringbuilder);

                        _detailstring = _stringbuilder.ToString();
                        _viewstatus |= ViewStatus.Detail;
                    }
                }
            }
            else if (data.IsGameAssetBundleItem())
            {
                if (GUI.Button(args.CellRect, "Detail", "minibutton"))
                {
                    bool show = false;
                    if (HasStatus(ViewStatus.Detail))
                    {
                        var code = data.History.Value.GetHashCode();
                        if (_detailhashcode == code)
                        {
                            _viewstatus &= ~ViewStatus.Detail;
                            _detailstring = "";
                            _detailhashcode = 0;
                        }
                        else
                        {
                            show = true;
                            _detailhashcode = code;
                        }
                    }
                    else
                    {
                        _viewstatus |= ViewStatus.Detail;
                        show = true;
                        _detailhashcode = data.History.Value.GetHashCode();
                    }

                    if (show)
                    {
                        if (_stringbuilder == null)
                        {
                            _stringbuilder = new StringBuilder();
                        }
                        else
                        {
                            _stringbuilder.Length = 0;
                        }

                        data.History.Value.ToDetail(_stringbuilder);

                        _detailstring = _stringbuilder.ToString();
                        _viewstatus |= ViewStatus.Detail;
                    }
                }
            }
            else if (data.IsAssetInfoItem())
            {
                if (GUI.Button(args.CellRect, "Detail", "minibutton"))
                {
                    bool show = false;
                    if (HasStatus(ViewStatus.Detail))
                    {
                        var code = data.AssetInfo.Value.GetHashCode();
                        if (_detailhashcode == code)
                        {
                            _viewstatus &= ~ViewStatus.Detail;
                            _detailstring = "";
                            _detailhashcode = 0;
                        }
                        else
                        {
                            show = true;
                            _detailhashcode = code;
                        }
                    }
                    else
                    {
                        _viewstatus |= ViewStatus.Detail;
                        show = true;
                        _detailhashcode = data.AssetInfo.Value.GetHashCode();
                    }

                    if (show)
                    {
                        if (_stringbuilder == null)
                        {
                            _stringbuilder = new StringBuilder();
                        }
                        else
                        {
                            _stringbuilder.Length = 0;
                        }

                        data.AssetInfo.Value.ToDetail(_stringbuilder);

                        _detailstring = _stringbuilder.ToString();
                        _viewstatus |= ViewStatus.Detail;
                    }

                }
            }
        }

        public void CanDrag(ref bool retval)
        {
            retval = false;
        }

        public void Drag(ref AssetDragAndDropArgs dragargs, ref DragAndDropVisualMode dragmd)
        {

        }
        #endregion


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
                headerContent = new GUIContent("RefCount", "User Refercence Count"),
                contextMenuText = "RefCount",
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
                headerContent = new GUIContent("GameObjectRefCount", "GameObjec Refercence Count"),
                contextMenuText = "GameObjectRefCount",
                headerTextAlignment = TextAlignment.Left,
                sortedAscending = false,
                sortingArrowAlignment = TextAlignment.Left,
                width = 130,
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
                headerContent = new GUIContent("Pin", "Try Pin Objects"),
                contextMenuText = "Pin",
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
                headerContent = new GUIContent("Detail", "Show Detail Info"),
                contextMenuText = "Detail",
                headerTextAlignment = TextAlignment.Left,
                canSort = false,
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