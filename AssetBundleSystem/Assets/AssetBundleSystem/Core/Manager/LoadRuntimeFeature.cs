using System;
using UnityEngine;
using System.Collections;

namespace AssetBundleSystem
{
    public partial class AssetBundleLoadManager : MonoBehaviour
    {
        public static AssetBundleLoadManager mIns;

        private AssetBundleFeature _assetBundleFeature;

        void Awake()
        {
            if (mIns != null)
            {
                throw  new ArgumentException("Multi Load Manger");
            }
            mIns = this;
            DontDestroyOnLoad(gameObject);

            Debug.logger.logEnabled = AssetBundleConfig.DebugMode.HasEnum( DebugMode.Log);
            if (_assetBundleFeature == null)
            {
                _assetBundleFeature = new AssetBundleFeature();
            }
        }

        void Update()
        {
            if (_assetBundleFeature != null)
            {
                _assetBundleFeature.Execute( AssetEvent.Update);
            }
        }

        void LateUpdate()
        {
            if (_assetBundleFeature != null)
            {
                _assetBundleFeature.Execute(AssetEvent.LateUpdate);
            }
        }

        void OnDrawGizmos()
        {
            if (_assetBundleFeature != null)
            {
                _assetBundleFeature.Execute(AssetEvent.DrawGizmos);
            }
        }

        void OnDestroy()
        {
            if (_assetBundleFeature != null)
            {
                AssetBundleHelper.IsQuit = true;
                _assetBundleFeature.Execute(AssetEvent.Destroy);
            }
        }
    }
}


