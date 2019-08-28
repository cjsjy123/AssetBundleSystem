
#if UNITY_EDITOR

using System;
using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Reflection.Emit;
using UnityEditor;

namespace AssetBundleSystem.Editor
{
    internal class EditorReadManiFestSystem : IAssetBundleSystem, IAssetBundleInitContext<EditorSystemContext>, IAssetBundleExecute<EditorSystemContext, EditorEvent>
    {
        public bool Block { get; private set; }

        public int Execute(ref EditorSystemContext context, EditorEvent assetEvent)
        {
            if (assetEvent == EditorEvent.Enable)
            {
#if UNITY_EDITOR
                string path = GetOutputPath();
                if (!string.IsNullOrEmpty(path))
                {
                    string dirname = AssetBundleEditorHelper.GetDirectoryName(path);
                    string allabpath = path + "/" + dirname;
                    if (File.Exists(allabpath) && AssetBundleLoadManager.mIns != null)
                    {
                        int retcode = 0;
                        try
                        {
                            var allmanifest = AssetBundleLoadManager.mIns.GetManifest(allabpath);
                            if (allmanifest != null)
                            {
                                if (context.Manifests == null)
                                    context.Manifests = new Dictionary<string, AssetBundleBuildInfo>();
                                else
                                {
                                    context.Manifests.Clear();
                                }

                                var ablist = allmanifest.GetAllAssetBundles();
                                foreach (var assetbundlename in ablist)
                                {
                                    AssignMainfest(ref context, allmanifest, path, assetbundlename);
                                }
                            }
                        }
                        catch (Exception e)
                        {
                            Debug.LogError(e);
                            retcode = -1;
                        }

                        return retcode;
                    }
                }
#endif
            }
            return 0;
        }

        public int Init(ref EditorSystemContext context)
        {
            return 0;
        }

        void AssignMainfest(ref EditorSystemContext context, AssetBundleManifest allmanifest, string basepath, string assetbundlename)
        {
            string assetbundlepath = basepath + "/" + assetbundlename;
            AssetBundle ab = AssetBundle.LoadFromFile(assetbundlepath);
            if (ab != null)
            {
                try
                {
                    AssetBundleBuildInfo info;
                    if (context.Manifests.TryGetValue(assetbundlename, out info))
                    {
                        Debug.LogError("has same name manifest " + assetbundlepath + ".manifest  name =" +
                                       assetbundlename);
                    }
                    else
                    {
                        var hash = allmanifest.GetAssetBundleHash(assetbundlename);
                        if (hash.isValid)
                            info.AssetBundleHash = hash.ToString();

                        uint crc;
                        if (BuildPipeline.GetCRCForAssetBundle(assetbundlepath, out crc))
                        {
                            info.Crc = crc;
                        }

                        context.Manifests.Add(assetbundlename, info);
                    }
                }
                catch (Exception e)
                {
                    Debug.LogError(e);
                }
                finally
                {
                    ab.Unload(true);
                }
            }
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

