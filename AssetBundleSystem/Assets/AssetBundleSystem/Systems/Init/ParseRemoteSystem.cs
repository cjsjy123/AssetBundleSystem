using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using CommonUtils;
using UnityEngine;

namespace AssetBundleSystem
{
    internal class ParseRemoteSystem : IAssetBundleSystem, IAssetBundleInitContext<AssetBundleContext>, 
        IAssetBundleExecute<AssetBundleContext,AssetEvent>,
        IAssetBundleReadData
    {
        public bool Block { get; private set; }

        public IFeature Parent { get; set; }

        private IRemoteAssets _remote;

        public int Execute(ref AssetBundleContext context, AssetEvent assetEvent)
        {
            return 0;
        }

        public int Init(ref AssetBundleContext context)
        {
            if (_remote == null)
            {
                string url = AssetBundleConfig.DownloadUrl;
                _remote = AssetBundleTypeGetter.GetRemoteAssets();
                GUpdater.mIns.StartCoroutine(ParseRemote(url));
            }

            Block = !_remote.IsDone;

            if (_remote.IsDone)
            {
                context.Cache.InitRemote(_remote);
                _remote = null;
            }
            return 0;
        }

        IEnumerator ParseRemote(string url)
        {
            yield return _remote.Init(url);
            if (_remote.IsDone)
            {
                AssetBundleFeature assetfeature = Parent as AssetBundleFeature;
                if (assetfeature != null)
                {
                    var context = assetfeature.StartRead();
                    context.Cache.InitRemote(_remote);
                    _remote = null;
                    assetfeature.EndRead(ref context);
                }
            }

            Block = false;
        }

    }
}


