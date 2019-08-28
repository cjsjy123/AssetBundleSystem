using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using CommonUtils;
using System;
using UnityEngine.SceneManagement;
using Object = UnityEngine.Object;

namespace AssetBundleSystem
{
    internal class AssetBundleMemorySystem : IAssetBundleSystem, IAssetBundleInitContext<AssetBundleContext>, IAssetBundleExecute<AssetBundleContext, AssetEvent>
    {
        public bool Block { get; private set; }

        private Dictionary<int, IBaseStrategy<AssetBundleContext>> _unloadDictionary = new Dictionary<int, IBaseStrategy<AssetBundleContext>>();

        public int Init(ref AssetBundleContext context)
        {
            _unloadDictionary[(int) UnLoadStrategy.Default] = new DefaultStrategy();
            return 0;
        }

        public int Execute(ref AssetBundleContext context, AssetEvent assetEvent)
        {
            if (assetEvent == AssetEvent.Destroy || assetEvent == AssetEvent.LateUpdate)
            {
                IBaseStrategy<AssetBundleContext> runner;
                if (this._unloadDictionary.TryGetValue((int)AssetBundleConfig.UnLoadStrategy, out runner))
                {
                    runner.Run(ref context, assetEvent);
                }
            }

            return 0;
        }

    }
}


