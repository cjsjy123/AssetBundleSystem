using System;
using UnityEngine;
using System.Collections;

namespace AssetBundleSystem
{
    internal class LoadAssetSystem :LoadCommonObjectSystem
    {
        private long _size;
        protected override bool IsEnableTask(ref AssetBundleContext context, ref AssetBundleTask task)
        {
            return task.TaskResType <= TaskResType.GameObject && base.IsEnableTask(ref context, ref task);
        }

        protected override void TryDoTask(ref AssetBundleContext context)
        {
            _size = 0;
            base.TryDoTask(ref context);
        }

        protected override bool IsEnableLoadAssetBunlde(ref AssetBundleContext context, ref AssetBundleTask task, ref GameAssetBundle gameAssetBundle)
        {
            return task.FreeSize || _size < Mathf.Max(1, AssetBundleConfig.TaskLoadAssetLimit);
        }

        protected override void AfterLoadAssetBundle(ref AssetBundleContext context, ref AssetBundleTask task, ref GameAssetBundle gameAssetBundle,ref bool retcode, ref bool mainloadrequest)
        {
            _size += gameAssetBundle.AssetBundleInfo.AssetSize;
            base.AfterLoadAssetBundle(ref context,ref task, ref gameAssetBundle,ref retcode,ref mainloadrequest);
        }
    }

}


