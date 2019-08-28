using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using CommonUtils;
using UnityEngine;

namespace AssetBundleSystem
{
    /// <summary>
    /// 解析文件 组建information
    /// </summary>
    internal class ParseAssetInfoSystem : IAssetBundleSystem, IAssetBundleInitContext<AssetBundleContext>, 
        IAssetBundleExecute<AssetBundleContext,AssetEvent>,
        IAssetBundleReadData
    {
        public bool Block { get; private set; }

        public IFeature Parent { get; set; }

        public int Execute(ref AssetBundleContext context, AssetEvent assetEvent)
        {
            if (assetEvent == AssetEvent.Destroy)
            {
                context.IsDestroying = true;
            }

            return 0;
        }

        public int Init(ref AssetBundleContext context)
        {
            try
            {
                string perpath = AssetBundleHelper.GetBundlePersistentPath(AssetBundleConfig.DepFileName);
                if (!AddPersistentAtPath(ref context, perpath))
                {
                    string streampath = AssetBundleHelper.GetBundleStreamPath(AssetBundleConfig.DepFileName);
                    if (!AddStreamAtPath(ref context, streampath))
                    {
                        Debug.Log("BundleStreamPath dont have depfile");
                        return -1;
                    }
                }
                else if (AssetBundleConfig.DebugMode.HasEnum(DebugMode.Detail))
                {
                    Debug.Log("PersistentPath have depfile");
                }
                return 0;
            }
            catch (Exception e)
            {
                Debug.LogException(e);
                return -1;
            }
        }

        bool AddPersistentAtPath(ref AssetBundleContext context,string assetpath)
        {
            if (File.Exists(assetpath))
            {
                IParseDependencyReader reader = AssetBundleTypeGetter.GetDepReader();
                if (reader != null)
                {
                    List<AssetBundleInfo> list;
                    if (reader.Read(assetpath, out list))
                    {
                        context.Cache.Load(list);
                        ListPool<AssetBundleInfo>.Release(list);
                    }
                    return true;
                }
            }
            return false;
        }

        bool AddStreamAtPath(ref AssetBundleContext context, string assetpath)
        {
            if (File.Exists(assetpath))
            {
                IParseDependencyReader reader = AssetBundleTypeGetter.GetDepReader();
                if (reader != null)
                {
                    if (reader.IsAsync)
                    {
                        Block = true;
                        GUpdater.mIns.StartCoroutine(TryRead(reader, assetpath));
                    }
                    else
                    {
                        List<AssetBundleInfo> list;
                        if (reader.Read(assetpath, out list))
                        {
                            context.Cache.Load(list);
                            ListPool<AssetBundleInfo>.Release(list);
                        }
                        return true;
                    }
                }
            }
            return false;
        }

        IEnumerator TryRead(IParseDependencyReader reader,string filepath)
        {
            yield return reader.ReadAsync(filepath);

            AssetBundleFeature assetfeature = Parent as AssetBundleFeature;
            if (assetfeature != null)
            {
                DefaultParseDepReader depReader =reader as DefaultParseDepReader;
                if (depReader != null)
                {
                    AssetBundleContext context = assetfeature.StartRead();
                    context.Cache.Load(depReader.AsyncResultList);

                    ListPool<AssetBundleInfo>.Release(depReader.AsyncResultList);
                    depReader.AsyncResultList = null;
                    assetfeature.EndRead(ref context);
                }
            }
            Block = false;
        }

    }
}


