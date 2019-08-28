using System;
using System.Collections;
using CommonUtils;
using System.Collections.Generic;
using Object = UnityEngine.Object;

namespace AssetBundleSystem
{
    public static class AssetBundleFunction
    {
        public static AssetBundleResType TypeToResType(string filename)
        {
            if (filename.EndsWith(".prefab", StringComparison.OrdinalIgnoreCase))
            {
                return AssetBundleResType.Prefab;
            }
            else if (filename.EndsWith(".ttf", StringComparison.OrdinalIgnoreCase) || filename.EndsWith(".fnt", StringComparison.OrdinalIgnoreCase) || filename.EndsWith(".fontsettings", StringComparison.OrdinalIgnoreCase))
            {
                return AssetBundleResType.Font;
            }
            else if (filename.EndsWith(".mat", StringComparison.OrdinalIgnoreCase))
            {
                return AssetBundleResType.Material;
            }
            else if (filename.EndsWith(".png", StringComparison.OrdinalIgnoreCase) || filename.EndsWith(".jpg", StringComparison.OrdinalIgnoreCase) || filename.EndsWith(".psd", StringComparison.OrdinalIgnoreCase))
            {
                return AssetBundleResType.Image;
            }
            else if (filename.EndsWith(".renderTexture", StringComparison.OrdinalIgnoreCase))
            {
                return AssetBundleResType.RenderTexture;
            }
            else if (filename.EndsWith(".unity", StringComparison.OrdinalIgnoreCase))
            {
                return AssetBundleResType.Scene;
            }
            else if (filename.EndsWith(".mp4", StringComparison.OrdinalIgnoreCase) || filename.EndsWith(".mp3", StringComparison.OrdinalIgnoreCase) || filename.EndsWith(".wav", StringComparison.OrdinalIgnoreCase))
            {
                return AssetBundleResType.Sound;
            }
            else if (filename.EndsWith(".controller", StringComparison.OrdinalIgnoreCase))
            {
                return AssetBundleResType.AnimationControl;
            }
            else if (filename.EndsWith(".anim", StringComparison.OrdinalIgnoreCase))
            {
                return AssetBundleResType.Animation;
            }
            else if (filename.EndsWith(".shader", StringComparison.OrdinalIgnoreCase) || filename.EndsWith(".cginc", StringComparison.OrdinalIgnoreCase))
            {
                return AssetBundleResType.Shader;
            }
            else if (filename.EndsWith(".bytes", StringComparison.OrdinalIgnoreCase))
            {
                return AssetBundleResType.Bytes;
            }
            else if (filename.EndsWith(".fbx", StringComparison.OrdinalIgnoreCase))
            {
                return AssetBundleResType.Fbx;
            }
            else if (filename.EndsWith(".asset", StringComparison.OrdinalIgnoreCase))
            {
                return AssetBundleResType.CustomAsset;
            }
            return AssetBundleResType.Unknown;
        }



        internal static void AddRef(ref AssetBundleContext context, ref GameAssetBundle gameAssetBundle,string rootpath)
        {
            gameAssetBundle.AddUserRef(rootpath);

            context.Cache.UpdateAssetBundle(gameAssetBundle.AssetBundleInfo.AssetBundleName, ref gameAssetBundle);

            if (gameAssetBundle.AssetBundleInfo.DepAssetBundleNames != null)
            {
                foreach (var depname in gameAssetBundle.AssetBundleInfo.DepAssetBundleNames)
                {
                    if (!string.IsNullOrEmpty(depname))
                    {
                        GameAssetBundle depassetbundle;
                        if (context.Cache.GetAssetBundle(depname, out depassetbundle))
                        {
                            AddRef(ref context, ref depassetbundle, rootpath);
                        }
                    }

                }
            }
        }

        internal static void AddAssetBundleRef(ref AssetBundleContext context, ref AssetBundleInfo assetbundleinfo, string name, Object target)
        {
            GameAssetBundle gameAssetBundle;
            if (context.Cache.GetAssetBundle(name, out gameAssetBundle))
            {
                gameAssetBundle.AddReference(target);
                context.Cache.UpdateAssetBundle(name, ref gameAssetBundle);
            }

            if (assetbundleinfo.DepAssetBundleNames != null)
            {
                for (int i = 0; i < assetbundleinfo.DepAssetBundleNames.Length; i++)
                {
                    var assetbundlename = assetbundleinfo.DepAssetBundleNames[i];
                    string resname = assetbundleinfo.DependNames[i];
                    AssetBundleInfo runtimedata;
                    if (!string.IsNullOrEmpty(assetbundlename) && context.Cache.TryGetInfo(resname, out runtimedata))
                    {
                        AddAssetBundleRef(ref context, ref runtimedata, assetbundlename, target);
                    }
                }
            }

        }


        internal static void RemoveRef(ref AssetBundleContext context, ref GameAssetBundle gameAssetBundle)
        {
            gameAssetBundle.RemoveUserRef();

            context.Cache.UpdateAssetBundle(gameAssetBundle.AssetBundleInfo.AssetBundleName, ref gameAssetBundle);


            if (gameAssetBundle.AssetBundleInfo.DepAssetBundleNames != null)
            {
                foreach (var depname in gameAssetBundle.AssetBundleInfo.DepAssetBundleNames)
                {
                    if (!string.IsNullOrEmpty(depname))
                    {
                        GameAssetBundle depassetbundle;
                        if (context.Cache.GetAssetBundle(depname, out depassetbundle))
                        {
                            RemoveRef(ref context, ref depassetbundle);
                        }
                    }

                }
            }
        }

        internal static bool InDowloading(ref AssetBundleContext context, ref AssetBundleTask task)
        {

            AssetDownloadInfo downloadInfo = new AssetDownloadInfo();
            downloadInfo.AssetBundleName = task.AssetBundleName;
            if (context.DownLoadQueue.Contains(downloadInfo))
            {
                return true;
            }

            if (task.SubTasks != null)
            {
                for (int i = 0; i < task.SubTasks.Count; i++)
                {
                    var subtask = task.SubTasks[i];
                    if (InDowloading(ref context, ref subtask))
                    {
                        return true;
                    }
                }
            }

            return false;
        }

        public static int GetRingOffset(int value, int begin, int end)
        {
            if (value > end)
            {
                int len = value - end - 1;
                return begin + len % (end - begin+1) ;
            }
            else if (value < begin)
            {
                int len = begin - value -1 ;
                return end - len % (end - begin+1);
            }
            return value;
        }
    }
}

