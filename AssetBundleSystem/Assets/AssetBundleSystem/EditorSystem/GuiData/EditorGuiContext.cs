#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEditor;
using UnityEngine;

namespace AssetBundleSystem.Editor
{
    internal struct EditorGuiContext
    {
        public const float GuiHeight = 20;
        public const float GuiSpace = 20;

        public Rect WindowRect;
        public Rect SearchFieldRect;
        public Rect TreeViewRect;
        public Rect ToolbarRect;
        public Rect TitleRect;

        public int SelectDepth;

        public bool IsLoading;

        public Rect DependencyRect;

        #region profiler
        public Rect ProfilerHelperRect;

        public Rect ProfilerToolbarRect;

        public Rect ProfilerCurveRect;

        public Rect ProfilerTreeRect;

        public Rect ProfilerSearchRect;

        public Rect ProfilerDetailRect;

        public Rect ProfilerBotBarRect;

        public Vector2 ProfilerCurveDxy;

        public long UserSelectFrame ;

        #endregion

        #region Build

        public Rect PathRect;
        public Rect OptionRect;
        public Rect BuildInfoRect;

        #endregion

        #region Sprite

        public Dictionary<string,bool> SelectPaths;

        public GenericMenu Menu;

        public List<string> UsablePaths;

        public int MaskValue;

        public Rect SpriteSearchRect;

        public Rect SpriteTreeRect;

        public Rect SpriteBotBarRect;

        #endregion

    }
}
#endif