
#if UNITY_EDITOR
using UnityEngine;
using System.Collections;
using UnityEditor;

namespace AssetBundleSystem.Editor
{
    internal class EditorWriteDepFileSystem : IAssetBundleSystem,IAssetBundleInitContext<EditorSystemContext>,IAssetBundleExecute<EditorSystemContext, EditorEvent>
    {
        public bool Block { get; private set; }

        public int Init(ref EditorSystemContext context)
        {
            return 0;
        }

        public int Execute(ref EditorSystemContext context, EditorEvent assetEvent)
        {
            if (assetEvent == EditorEvent.Build)
            {
                IParseDependencyWriter binarywriter = AssetWindowConfig.GetDepWriter();
                string exportPath = GetOutputPath() + "/" + AssetBundleConfig.DepFileName;
                string streamexportPath = AssetBundleHelper.GetBundleStreamPath(AssetBundleConfig.DepFileName);
                if (binarywriter == null || !binarywriter.Write(exportPath) || !binarywriter.Write(streamexportPath))
                {
                    return -1;
                }
                AssetDatabase.Refresh();
            } 
            return 0;
        }

        string GetOutputPath()
        {
            if (EditorPrefs.HasKey(BuildGuiLogic.OutPutKey))
            {
                return EditorPrefs.GetString(BuildGuiLogic.OutPutKey);
            }
            return AssetBundleHelper.DataPath;
        }
    }
}

#endif

