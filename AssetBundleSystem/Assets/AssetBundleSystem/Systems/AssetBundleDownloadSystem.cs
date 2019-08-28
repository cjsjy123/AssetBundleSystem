using UnityEngine;
using System.Collections;
using System;

namespace AssetBundleSystem
{
    internal class AssetBundleDownloadSystem : IAssetBundleSystem, IAssetBundleInitContext<AssetBundleContext>, IAssetBundleExecute<AssetBundleContext, AssetEvent>
    {
        public bool Block { get; private set; }

        public int Init(ref AssetBundleContext context)
        {
            return 0;
        }

        public int Execute(ref AssetBundleContext context, AssetEvent assetEvent)
        {
            if(assetEvent == AssetEvent.Update)
            {
                for (int i = context.DownLoadQueue.Count-1; i >=0;i--)
                {
                    var info = context.DownLoadQueue[i];
                    DownLoadAssetBundle(ref context, ref info);
                    context.DownLoadQueue[i] = info;

                    if (info.IsDone)
                    {
                        context.DownLoadQueue.RemoveAt(i);
                    }
                }
  
            }

            return 0;
        }


        void DownLoadAssetBundle(ref AssetBundleContext context,ref AssetDownloadInfo downloadtask)
        {
            if(!downloadtask.IsDone && !downloadtask.IsDownloading)
            {
                downloadtask.Loader = AssetBundleTypeGetter.GetDownloader();
            }

            //update
            downloadtask.Loader.DownLoad(ref downloadtask);


            if(downloadtask.TaskId >=0)
            {
                for (int i = 0; i < context.Tasks.Count; i++)
                {
                    var assettask = context.Tasks[i];
                    if(assettask.TaskId == downloadtask.TaskId)
                    {
                        if (assettask.Result.ProgresCallback != null)
                        {

                            int finishcnt;
                            int totalcnt;
                            assettask.IsSubAllDone(ref context, out finishcnt, out totalcnt);

                            ProgressArgs progressArgs = new ProgressArgs((float)finishcnt /totalcnt, 0, assettask.AssetPath, true);
                            assettask.Result.ProgresCallback(ref progressArgs);
                        }
                        break;
                    }
                }
            }
        }

    }


}

