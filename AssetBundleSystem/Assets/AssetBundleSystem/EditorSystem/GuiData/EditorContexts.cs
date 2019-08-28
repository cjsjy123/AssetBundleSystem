#if UNITY_EDITOR
using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using CommonUtils.Editor;
using UnityEditor;

namespace AssetBundleSystem.Editor
{
    internal class EditorContexts
    {
        private static EditorContexts _mIns;

        public static EditorContexts mIns
        {
            get
            {
                if (_mIns == null)
                {
                    _mIns = new EditorContexts();
                }
                return _mIns;
            }
        }

        private AssetWindow.WindowMode _mode = AssetWindow.WindowMode.Asset;

        public AssetWindow.WindowMode Mode
        {
            get { return _mode; }
            set
            {
                if (_mode != value)
                {
                    ModeChange(_mode, value);
                    _mode = value;
                }
            }
        }



        public AssetTreeData SelectForDependencyData;

        public EditorGuiContext GuiContext;

        public EditorSystemContext SystemContext;

        public EditorFeature Feature ;

        internal AssetWindow GuiWindow;

        EditorContexts()
        {
            this.GuiContext.UserSelectFrame = -1;
            this.GuiContext.SelectPaths = new Dictionary<string, bool>();
            this.GuiContext.UsablePaths = new List<string>();
        }

        public void Repaint()
        {
            if(GuiWindow != null)
            {
                GuiWindow.Repaint();
            }
        }

        public void CancelProgress()
        {
            EditorCoroutineGroup.StopAllCoroutines(EditorContexts.mIns.GuiWindow);
            GuiContext.IsLoading = false;
            if (GuiWindow)
            {
                GuiWindow.Inited = true;
            }
            EditorUtility.ClearProgressBar();
        }

        public void RefreshGuiInfo(AssetWindow window)
        {
            var guirender = AssetTreeManager.mIns.GetGuiRender(Mode);

            if (GuiContext.WindowRect.width > 0 && GuiContext.WindowRect.height > 0 && ( Mathf.Abs(GuiContext.WindowRect.width-window.position.width) > 0.1f || Mathf.Abs(GuiContext.WindowRect.height -window.position.height) >0.1f))
            {
                if (guirender != null)
                {
                    foreach (var grender in guirender)
                    {
                        IGuiTree treeRender = grender as IGuiTree;
                        if (treeRender != null)
                        {
                            treeRender.RefreshHead();
                        }
                    }
                }
            }

            GuiContext.WindowRect = new Rect((int)window.position.x, (int)window.position.y, (int)window.position.width, (int)window.position.height);
            GuiContext.TitleRect = new Rect(5, 0, GuiContext.WindowRect.width - 10, EditorGuiContext.GuiHeight);
            GuiContext.ToolbarRect = new Rect(5, GuiContext.WindowRect.height - EditorGuiContext.GuiHeight, GuiContext.WindowRect.width - 10, EditorGuiContext.GuiHeight);

            if (guirender != null)
            {
                foreach (var grender in guirender)
                {
                    grender.RefreshGuiInfo(this);
                }
            }
        }

        public void ForceModeChange(AssetWindow.WindowMode md)
        {
            EditorCoroutineGroup.StartCoroutine(DoForceModeChange(md), GuiWindow);
        }

        IEnumerator DoForceModeChange(AssetWindow.WindowMode md)
        {
            GuiContext.IsLoading = true;
            var logic = AssetTreeManager.mIns.Get(md);
            if (logic != null)
            {
                foreach (var sublogic in logic)
                {
                    if (sublogic.TypeMode == md)
                    {
                        sublogic.Clear();
                        yield return sublogic.ReLoad();
                    }
                }
            }

            var render = AssetTreeManager.mIns.GetGuiRender(md);
            if (render != null)
            {
                foreach (var subrender in render)
                {
                    if (subrender.TypeMode == md)
                    {
                        IGuiTree treeRender = subrender as IGuiTree;
                        if (treeRender != null)
                        {
                            treeRender.Refresh();
                        }
                    }
                }
            }

            EditorUtility.ClearProgressBar();
            GuiContext.IsLoading = false;
        }

        void ModeChange(AssetWindow.WindowMode old, AssetWindow.WindowMode newmd)
        {
            EditorCoroutineGroup.StartCoroutine(DoModeChange(old,newmd), GuiWindow);
        }

        IEnumerator DoModeChange(AssetWindow.WindowMode old, AssetWindow.WindowMode newmd)
        {
            GuiContext.IsLoading = true;
            bool needupdate = false;
            int oldval = (int)old;
            int newval = (int)newmd;
            int oldmain = oldval / 8;
            int newmain = newval / 8;
            if (oldmain != newmain)
            {
                var oldrender = AssetTreeManager.mIns.GetGuiRender(old);
                if (oldrender != null)
                {
                    foreach (var subrender in oldrender)
                    {
                        subrender.Destory();
                    }
                }

                needupdate = true;
            }
            else
            {
                if (newmain != newval)
                {
                    needupdate = true;
                }
            }

            if (needupdate)
            {
                //logic
                var logic = AssetTreeManager.mIns.Get(newmd);
                if (logic != null)
                {
                    foreach (var sublogic in logic)
                    {
                        if (sublogic.TypeMode == newmd)
                        {
                            sublogic.Clear();
                            yield return sublogic.ReLoad();
                        }
                    }
                }

                var render = AssetTreeManager.mIns.GetGuiRender(newmd);
                if (render != null)
                {
                    foreach (var subrender in render)
                    {
                        if (subrender.TypeMode == newmd)
                        {
                            subrender.Init();
                            IGuiTree treeRender = subrender as IGuiTree;
                            if (treeRender != null)
                            {
                                treeRender.Refresh();
                            }
                        }

                    }
                }
            }

            EditorUtility.ClearProgressBar();
            GuiContext.IsLoading = false;
        }
    }

}

#endif
