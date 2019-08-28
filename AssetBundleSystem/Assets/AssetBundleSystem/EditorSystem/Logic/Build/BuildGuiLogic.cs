#if UNITY_EDITOR
using System;
using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Text;
using UnityEditor;
using UnityEditor.IMGUI.Controls;

namespace AssetBundleSystem.Editor
{
    internal class BuildGuiLogic :IAssetGuiInterface
    {
        public const string OutPutKey = "OutputAsstBundle";
        private const string BuildOptKey = "BuildOption";
        private const string AutoGenDepKey = "Auto_GenDepKey";
        private StringBuilder _stringBuilder;
        private Vector2 _scrollpos;

        public AssetWindow.WindowMode TypeMode
        {
            get { return AssetWindow.WindowMode.Build; }
        }

        public void Init()
        {
            
        }

        public void Destory()
        {
            EditorUtility.UnloadUnusedAssetsImmediate();
            Resources.UnloadUnusedAssets();
        }


        public void GuiUpdate(bool isrunning, float delta)
        {
            
        }

        public void RefreshGuiInfo(EditorContexts contexts)
        {
            contexts.GuiContext.PathRect = new Rect(5, EditorGuiContext.GuiHeight+10, contexts.GuiContext.WindowRect.width-10,4 * EditorGuiContext.GuiHeight);
            contexts.GuiContext.OptionRect = new Rect(5,15+5 *EditorGuiContext.GuiHeight, contexts.GuiContext.WindowRect.width - 10,6 * EditorGuiContext.GuiHeight);
            contexts.GuiContext.BuildInfoRect = new Rect(5, contexts.GuiContext.OptionRect.yMax , contexts.GuiContext.WindowRect.width - 10,contexts.GuiContext.WindowRect.height - 14* EditorGuiContext.GuiHeight);
        }

        public void RenderGui()
        {
            var oldcol = GUI.skin.label.normal.textColor;
            GUI.skin.label.normal.textColor = Color.black;

            RenderPathUi();
            RenderOption();
            RenderInfo();
            RenderBottom();
            GUI.skin.label.normal.textColor = oldcol;
        }

        void RenderPathUi()
        {
            var style = GUI.skin.FindStyle("flow node 2");

            GUI.BeginGroup(EditorContexts.mIns.GuiContext.PathRect,style);

            GUI.Label(new Rect(0,0,50,EditorGuiContext.GuiHeight),"Path:");

            var pathstyle = GUI.skin.FindStyle("dockarea");
            pathstyle.alignment = TextAnchor.MiddleLeft;
            GUI.Label(new Rect(40,EditorGuiContext.GuiHeight ,600,EditorGuiContext.GuiHeight),"OuputPath:"+ GetOutputPath(), pathstyle);

            if (GUI.Button(new Rect(650, EditorGuiContext.GuiHeight ,80, EditorGuiContext.GuiHeight), "Select"))
            {
                string result = EditorUtility.SaveFolderPanel("select output", AssetWindowConfig.DefaultOutputPath, "");
                if (!string.IsNullOrEmpty(result))
                {
                    EditorPrefs.SetString(OutPutKey, result);
                }
            }

            if (GUI.Button(new Rect(40,2 * EditorGuiContext.GuiHeight+10,120,EditorGuiContext.GuiHeight), "Clear Cache"))
            {
                EditorPrefs.DeleteKey(OutPutKey);
                EditorPrefs.DeleteKey(BuildOptKey);
            }

            GUI.EndGroup();
        }

        string GetOutputPath()
        {
            if (EditorPrefs.HasKey(OutPutKey))
            {
                return EditorPrefs.GetString(OutPutKey);
            }
            return AssetBundleHelper.DataPath;
        }

        bool GetAutoDep()
        {
            if (EditorPrefs.HasKey(AutoGenDepKey))
            {
                return EditorPrefs.GetBool(AutoGenDepKey);
            }
            return false;
        }

        void RenderOption()
        {
            var style = GUI.skin.FindStyle("flow node 2");
            GUI.BeginGroup(EditorContexts.mIns.GuiContext.OptionRect, style);

            GUI.Label(new Rect(0, 0, 50, EditorGuiContext.GuiHeight), "Option:");

            var lw = EditorGUIUtility.labelWidth;
            var lt = EditorGUI.indentLevel;

            EditorGUIUtility.labelWidth = 140;
            EditorGUI.indentLevel = 0;

            BuildAssetBundleOptions optval = (BuildAssetBundleOptions)((int)GetBuildAbOptions() << 1);
            BuildAssetBundleOptions optnewval = (BuildAssetBundleOptions)EditorGUI.EnumMaskField(new Rect(40, EditorGuiContext.GuiHeight, 350, EditorGuiContext.GuiHeight),new GUIContent("AssetBundle Options"), optval);
            if (optnewval != optval)
            {
                EditorPrefs.SetInt(BuildOptKey,(int)optnewval);
            }

            BuildTarget target = EditorUserBuildSettings.activeBuildTarget;
            var newtarget = (BuildTarget)EditorGUI.EnumPopup(new Rect(40, 2 * EditorGuiContext.GuiHeight, 350, EditorGuiContext.GuiHeight),
                new GUIContent("BuildTarget"), target);
            if(newtarget != target)
            {
                if (EditorUtility.DisplayDialog("info", "Change to " + newtarget, "yes", "no"))
                {
                    EditorUserBuildSettings.SwitchActiveBuildTarget(ConvertBuildTarget(newtarget), newtarget);
                }
            }

            EditorGUIUtility.labelWidth = lw;
            EditorGUI.indentLevel = lt;

            bool oldgen = GetAutoDep();
            bool newval = GUI.Toggle(new Rect(40, 3 * EditorGuiContext.GuiHeight,160,EditorGuiContext.GuiHeight),oldgen, "Auto Generate Dep");
            if (newval != oldgen)
            {
                EditorPrefs.SetBool(AutoGenDepKey, newval);
            }

            if (GUI.Button(new Rect(40, 4 * EditorGuiContext.GuiHeight+5, 80, EditorGuiContext.GuiHeight),"Build"))
            {
                StartBuild();
            }

            if (GUI.Button(new Rect(125, 4 * EditorGuiContext.GuiHeight + 5, 120, EditorGuiContext.GuiHeight), "Build DepFile"))
            {
                GenerateDependencyInfo();
            }

            if (GUI.Button(new Rect(250, 4 * EditorGuiContext.GuiHeight + 5, 190, EditorGuiContext.GuiHeight), "Copy to StreamAssets"))
            {
                string streampath = AssetBundleHelper.StreamingAssetsPath;
                if (!Directory.Exists(streampath))
                    Directory.CreateDirectory(streampath);

                AssetBundleEditorHelper.DirectoryCopy(GetOutputPath(), streampath);
                AssetDatabase.Refresh();
            }

            if (GUI.Button(new Rect(445, 4 * EditorGuiContext.GuiHeight + 5, 140, EditorGuiContext.GuiHeight), "Clear StreamAssets"))
            {
                string streampath = AssetBundleHelper.StreamingAssetsPath;
                if (Directory.Exists(streampath))
                {
                    FileUtil.DeleteFileOrDirectory(AssetBundleEditorHelper.GetUnityAssetPath(streampath));
                    AssetDatabase.Refresh();
                }
            }

            if (GUI.Button(new Rect(590, 4 * EditorGuiContext.GuiHeight + 5,120, EditorGuiContext.GuiHeight), "Clear BuildPath"))
            {
                string buildpath = GetOutputPath();
                if (Directory.Exists(buildpath))
                {
                    FileUtil.DeleteFileOrDirectory(AssetBundleEditorHelper.GetUnityAssetPath(buildpath));
                    AssetDatabase.Refresh();
                }
            }

            GUI.EndGroup();
        }

        static BuildTargetGroup ConvertBuildTarget(BuildTarget buildTarget)
        {
            switch (buildTarget)
            {
                case BuildTarget.StandaloneOSXIntel64:
                case BuildTarget.StandaloneOSXIntel:
                case BuildTarget.iOS:
                    return BuildTargetGroup.iOS;
                case BuildTarget.StandaloneWindows:
                case BuildTarget.StandaloneLinux:
                case BuildTarget.StandaloneWindows64:
                case BuildTarget.StandaloneLinux64:
                case BuildTarget.StandaloneLinuxUniversal:
                    return BuildTargetGroup.Standalone;
                case BuildTarget.Android:
                    return BuildTargetGroup.Android;
                case BuildTarget.WebGL:
                    return BuildTargetGroup.WebGL;
                case BuildTarget.WSAPlayer:
                    return BuildTargetGroup.WSA;
                case BuildTarget.Tizen:
                    return BuildTargetGroup.Tizen;
                case BuildTarget.PSP2:
                    return BuildTargetGroup.PSP2;
                case BuildTarget.PS4:
                    return BuildTargetGroup.PS4;
                case BuildTarget.PSM:
                    return BuildTargetGroup.PSM;
                case BuildTarget.XboxOne:
                    return BuildTargetGroup.XboxOne;
                case BuildTarget.N3DS:
                    return BuildTargetGroup.N3DS;
                case BuildTarget.WiiU:
                    return BuildTargetGroup.WiiU;
                case BuildTarget.tvOS:
                    return BuildTargetGroup.tvOS;
                case BuildTarget.Switch:
                    return BuildTargetGroup.Switch;
                case BuildTarget.NoTarget:
                default:
                    return BuildTargetGroup.Standalone;
            }
        }

        BuildAssetBundleOptions GetBuildAbOptions()
        {
            if (EditorPrefs.HasKey(BuildOptKey))
            {
                return (BuildAssetBundleOptions)(EditorPrefs.GetInt(BuildOptKey) >>1);
            }
#pragma warning disable
            return BuildAssetBundleOptions.CollectDependencies | BuildAssetBundleOptions.CompleteAssets;
#pragma warning restore
        }


        void RenderInfo()
        {
            var style = GUI.skin.FindStyle("flow node 2");
            style.alignment = TextAnchor.UpperLeft;
            //GUI.BeginGroup(EditorContexts.mIns.BuildInfoRect);

            using (var scope = new EditorGUILayout.VerticalScope())
            {
                GUILayout.BeginArea(EditorContexts.mIns.GuiContext.BuildInfoRect);
                _scrollpos =GUILayout.BeginScrollView(_scrollpos);

                var col = GUI.skin.label.normal.textColor;
            
                GUI.skin.label.normal.textColor = Color.white;
                GUI.skin.label.richText = true;

                var allassetnames = AssetDatabase.GetAllAssetBundleNames();
                GUILayout.Label(string.Format("Will Build {0} AsseetBundles\n", AssetBundleEditorHelper.GetColorText("{0}", Color.red, allassetnames.Length)));

                var currenttarget = EditorUserBuildSettings.activeBuildTarget;
                GUILayout.Label(string.Format("Build Target:{0}\n", AssetBundleEditorHelper.GetColorText("{0}",Color.yellow, currenttarget)));

                GUILayout.Label(string.Format("BuildOption:{0}\n", AssetBundleEditorHelper.GetColorText("{0}", Color.yellow, GetOptionInfo())));

                GUILayout.Label(AssetBundleEditorHelper.GetColorText("{0}",Color.cyan, "AssetBundleOption Helper:\n"));
                
                GUILayout.Label("UncompressedAssetBundle:"+AssetBundleEditorHelper.GetColorText("{0}\n", Color.yellow,
                "This bundle option builds the bundles in such a way that the data is completely uncompressed. The downside to being uncompressed is the larger file download size. However, the load times once downloaded will be much faster."));

                GUILayout.Label("DisableWriteTypeTree:" + AssetBundleEditorHelper.GetColorText("{0}\n", Color.yellow,
                    "Do not include type information within the AssetBundle."));

                GUILayout.Label("DeterministicAssetBundle:" + AssetBundleEditorHelper.GetColorText("{0}\n", Color.yellow,
                    "Builds an asset bundle using a hash for the id of the object stored in the asset bundle.When building the asset bundle with this method, the objects in it are assigned a 32 bit hash code that is calculated using the name of the asset bundle file, \n\tthe GUID of the asset and the local id of the object in the asset.For that reason make sure to use the same file name when rebuilding. \n\tAlso note that having a lot of objects might cause hash collisions preventing Unity from building the asset bundle."));

                GUILayout.Label("ForceRebuildAssetBundle:" + AssetBundleEditorHelper.GetColorText("{0}\n", Color.yellow,
                    "Force rebuild the assetBundles"));

                GUILayout.Label("IgnoreTypeTreeChanges:" + AssetBundleEditorHelper.GetColorText("{0}\n", Color.yellow,
                    "Ignore the type tree changes when doing the incremental build check."));


                GUILayout.Label("AppendHashToAssetBundleName:" + AssetBundleEditorHelper.GetColorText("{0}\n", Color.yellow,
                    "Append the hash to the assetBundle name."));

                GUILayout.Label("ChunkBasedCompression:" + AssetBundleEditorHelper.GetColorText("{0}\n", Color.yellow,
                    " This bundle option uses a compression method known as LZ4, which results in larger compressed file sizes than LZMA but does not require that entire bundle is decompressed,\n\tunlike LZMA, before it can be used. LZ4 uses a chunk based algorithm which allows the AssetBundle be loaded in pieces or “chunks.”\n\tDecompressing a single chunk allows the contained assets to be used even if the other chunks of the AssetBundle are not decompressed."));

                GUILayout.Label("StrictMode:" + AssetBundleEditorHelper.GetColorText("{0}\n", Color.yellow,
                    "Do not allow the build to succeed if any errors are reporting during it."));

                GUILayout.Label("DryRunBuild:" + AssetBundleEditorHelper.GetColorText("{0}\n", Color.yellow,
                    "Do a dry run build.."));

     
                GUI.skin.label.richText = false;
                GUI.skin.label.normal.textColor = col;

                GUILayout.EndScrollView();
                GUILayout.EndArea();
            }
        }

        string GetOptionInfo()
        {
            if (_stringBuilder == null)
                _stringBuilder = new StringBuilder();
            else
            {
                _stringBuilder.Length = 0;
            }

            var currentopt = GetBuildAbOptions();
            bool first = true;
            AddEnumInfo(currentopt, BuildAssetBundleOptions.UncompressedAssetBundle, ref first);
#pragma warning disable
            AddEnumInfo(currentopt, BuildAssetBundleOptions.CollectDependencies, ref first);
            AddEnumInfo(currentopt, BuildAssetBundleOptions.CompleteAssets, ref first);
#pragma warning restore
            AddEnumInfo(currentopt, BuildAssetBundleOptions.DisableWriteTypeTree, ref first);
            AddEnumInfo(currentopt, BuildAssetBundleOptions.DeterministicAssetBundle, ref first);
            AddEnumInfo(currentopt, BuildAssetBundleOptions.ForceRebuildAssetBundle, ref first);
            AddEnumInfo(currentopt, BuildAssetBundleOptions.IgnoreTypeTreeChanges, ref first);
            AddEnumInfo(currentopt, BuildAssetBundleOptions.AppendHashToAssetBundleName, ref first);
            AddEnumInfo(currentopt, BuildAssetBundleOptions.ChunkBasedCompression, ref first);
            AddEnumInfo(currentopt, BuildAssetBundleOptions.StrictMode, ref first);
            AddEnumInfo(currentopt, BuildAssetBundleOptions.DryRunBuild, ref first);
            return _stringBuilder.ToString();
        }

        void AddEnumInfo<T>(T current, T optval,ref bool first) where T:struct ,IConvertible
        {
            bool has = AssetBundleEditorHelper.HasEnum(current, optval);
            if (has)
            {
                if (!first)
                {
                    _stringBuilder.Append("|");
                }
                _stringBuilder.Append(optval.ToString());
                first = false;
            }
        }
        
        void RenderBottom()
        {
            GUI.BeginGroup(EditorContexts.mIns.GuiContext.ToolbarRect);
            //var style = "miniButton";

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

            GUI.EndGroup();
        }

        void StartBuild()
        {
            string outpath = GetOutputPath();
            if (!string.IsNullOrEmpty(outpath))
            {
                if(GetAutoDep())
                {
                    GenerateDependencyInfo();
                }
                BuildPipeline.BuildAssetBundles(outpath, GetBuildAbOptions(),EditorUserBuildSettings.activeBuildTarget);

                AssetDatabase.Refresh();
            }
        }

        void GenerateDependencyInfo()
        {
            string outpath = GetOutputPath();
            if (!string.IsNullOrEmpty(outpath) && EditorContexts.mIns.Feature != null)
            {
                EditorContexts.mIns.Feature.Execute(EditorEvent.Build);
            }
            else
            {
                Debug.LogError("cant generate dep file");
            }
        }

    }
}

#endif
