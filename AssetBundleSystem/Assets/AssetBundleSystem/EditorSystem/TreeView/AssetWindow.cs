#if UNITY_EDITOR
using System;
using System.Collections;
using System.Collections.Generic;
using CommonUtils.Editor;
using UnityEngine;
using UnityEditor;
using UnityEditor.IMGUI.Controls;

namespace AssetBundleSystem.Editor
{

    public class AssetWindow : EditorWindow
    {
        public enum WindowMode
        {
            Asset =0,
            AssetDependency=1,
            Build =8,
            Profiler =16,
            ToolsSprite=32,
            End,
        }

        internal bool Inited;
        private float _lasttime;
        private float _waittime;

        [MenuItem("AssetBundles/AssetBundle Window")]
        public static void CreateWindow()
        {
            AssetWindow window = EditorWindow.GetWindow<AssetWindow>();

            window.titleContent = new GUIContent("AssetBundle Window");
            window.Focus();
            window.Show();

            if (EditorContexts.mIns.GuiWindow == null || !EditorContexts.mIns.GuiWindow.Inited)
            {
                EditorContexts.mIns.Mode = WindowMode.Asset;
            }
            else
            {
                EditorContexts.mIns.ForceModeChange(WindowMode.Asset);
            }
        }

        [MenuItem("AssetBundles/AssetBundle Profiler Window")]
        public static void CreateProfilerWindow()
        {
            AssetWindow window = EditorWindow.GetWindow<AssetWindow>();

            window.titleContent = new GUIContent("AssetBundle Window");
            window.Focus();
            window.Show();

            if (EditorContexts.mIns.GuiWindow == null || !EditorContexts.mIns.GuiWindow.Inited)
            {
                EditorContexts.mIns.Mode = WindowMode.Profiler;
            }
            else
            {
                EditorContexts.mIns.ForceModeChange(WindowMode.Profiler);
            }
        }

        [MenuItem("AssetBundles/AssetBundle Build Window")]
        public static void CreateBuildWindow()
        {
            AssetWindow window = EditorWindow.GetWindow<AssetWindow>();

            window.titleContent = new GUIContent("AssetBundle Window");
            window.Focus();
            window.Show();

            if (EditorContexts.mIns.GuiWindow == null || !EditorContexts.mIns.GuiWindow.Inited)
            {
                EditorContexts.mIns.Mode = WindowMode.Build;
            }
            else
            {
                EditorContexts.mIns.ForceModeChange(WindowMode.Build);
            }
        }


        [MenuItem("AssetBundles/AssetBundle Sprite Window")]
        public static void CreateSpriteWindow()
        {
            AssetWindow window = EditorWindow.GetWindow<AssetWindow>();

            window.titleContent = new GUIContent("AssetBundle Window");
            window.Focus();
            window.Show();

            if (EditorContexts.mIns.GuiWindow == null || !EditorContexts.mIns.GuiWindow.Inited)
            {
                EditorContexts.mIns.Mode = WindowMode.ToolsSprite;
            }
            else
            {
                EditorContexts.mIns.ForceModeChange(WindowMode.ToolsSprite);
            }
        }

        void OnEnable()
        {
            _waittime = 0;

            EditorContexts.mIns.GuiWindow = this;
            if(EditorContexts.mIns.Feature == null)
                EditorContexts.mIns.Feature = new EditorFeature();
            EditorContexts.mIns.Feature.Execute(EditorEvent.Enable);
        }

        private void OnDisable()
        {
            EditorContexts.mIns.GuiWindow = null;
        }

        IEnumerator InitForWindow()
        {
            EditorContexts.mIns.GuiContext.IsLoading = true;
            if (!Inited)
            {
                var logic = AssetTreeManager.mIns.Get(EditorContexts.mIns.Mode);
                if (logic != null)
                {
                    for (int i = 0; i < logic.Count; i++)
                    {
                        logic[i].Clear();
                        yield return logic[i].ReLoad();
                    }
                }

                EditorContexts.mIns.RefreshGuiInfo(this);

                var render = AssetTreeManager.mIns.GetGuiRender(EditorContexts.mIns.Mode);
                if (render != null)
                {
                    foreach (var subrender in render)
                    {
                        subrender.Init();
                        IGuiTree treeRender = subrender as IGuiTree;
                        if (treeRender != null)
                        {
                            treeRender.Refresh();
                        }
                    }
                }

                Inited = true;
                EditorUtility.ClearProgressBar();

                Repaint();
            }
            EditorContexts.mIns.GuiContext.IsLoading = false;
        }

        /// <summary>
        /// Render GUI
        /// </summary>
        void OnGUI()
        {
            //Render
            RenderTitle();

            if (Inited && !EditorContexts.mIns.GuiContext.IsLoading)
            {
                EditorContexts.mIns.RefreshGuiInfo(this);
                var render = AssetTreeManager.mIns.GetGuiRender(EditorContexts.mIns.Mode);
                if (render != null)
                {
                    foreach (var subrender in render)
                    {
                        subrender.RenderGui();
                    }
                }
            }
        }
         
        void Update()
        {
            if (_waittime < 0 || _waittime > AssetWindowConfig.WaitSeconds)
            {
                if (!Inited && !EditorContexts.mIns.GuiContext.IsLoading)
                    this.StartCoroutine(InitForWindow());
            }

            var render = AssetTreeManager.mIns.GetGuiRender(EditorContexts.mIns.Mode);
            if (render != null)
            {
                float curtime = Time.realtimeSinceStartup;
                _lasttime = Mathf.Min(curtime, _lasttime);
                float dt = curtime - _lasttime;
                foreach (var subrender in render)
                {
                    subrender.GuiUpdate(Application.isPlaying, dt);
                }

                if (Mathf.Abs(_waittime - 0f) < 0.001f || _waittime > 0)
                {
                    if (Mathf.Abs(_lasttime - 0.001f) > 0.001f)
                    {
                        _waittime += dt;
                    }

                    if (_waittime > AssetWindowConfig.WaitSeconds)
                    {
                        _waittime = -100f;
                        Repaint();
                    }
                }

                _lasttime = curtime;
            }
            if (EditorContexts.mIns.Feature != null)
                EditorContexts.mIns.Feature.Execute(EditorEvent.Update);

        }

        void RenderTitle()
        {
            var style = GUI.skin.FindStyle("MeTransOnRight");
            style.alignment = TextAnchor.MiddleCenter;
            style.fixedHeight = EditorContexts.mIns.GuiContext.TitleRect.height;
            GUI.Label(EditorContexts.mIns.GuiContext.TitleRect, new GUIContent("Asset Bundle System"), style);
        }
    }
}

#endif