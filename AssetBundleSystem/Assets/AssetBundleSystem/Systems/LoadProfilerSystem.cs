using UnityEngine;
using System.Collections.Generic;
using CommonUtils;
using UnityEngine.SceneManagement;
using Object = UnityEngine.Object;

namespace AssetBundleSystem
{
    internal class LoadProfilerSystem : IAssetBundleSystem, IAssetBundleInitContext<AssetBundleContext>, IAssetBundleExecute<AssetBundleContext, AssetEvent>
    {
        public bool Block { get; private set; }

        public int Init(ref AssetBundleContext context)
        {
            return 0;
        }

        public int Execute(ref AssetBundleContext context, AssetEvent assetEvent)
        {
            if(assetEvent == AssetEvent.Update && AssetBundleConfig.IsProfiler())
            {
                if(context.ProfilerData.IsProfilering)
                {
                    CaptureFrame(ref context);
                }
            }
            return 0;
        }

        void CaptureFrame(ref AssetBundleContext context)
        {
            context.ProfilerData.DataCount++;

            FrameProfilerData currentframedata = new FrameProfilerData();
            currentframedata.Frame = Time.frameCount;
            var scene = SceneManager.GetActiveScene();
            currentframedata.Scene = scene.name;

            //tasks
            for (int i = 0; i < context.Tasks.Count; i++)
            {
                var task = context.Tasks[i];
                currentframedata.AddCopyTask(ref task);
            }

            //Historyx
            var allassetbundles = context.Cache.GetAllAssetBundle();

            foreach (var kv in allassetbundles)
            {
                var gameassetbundle = kv.Value;

                AssetBundleHistory history = new AssetBundleHistory();

                history.AddObjectRef(ref gameassetbundle);

                currentframedata.AddHistory(ref history);
            }

            context.ProfilerData.Add(ref currentframedata);
        }

    }
}


