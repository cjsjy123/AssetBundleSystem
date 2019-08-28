using UnityEngine;
using System.Collections;
using System;

namespace AssetBundleSystem
{
    internal class RemoveTaskSystem : IAssetBundleSystem, IAssetBundleInitContext<AssetBundleContext>, IAssetBundleExecute<AssetBundleContext, AssetEvent>
    {
        public bool Block { get; private set; }

        public int Init(ref AssetBundleContext context)
        {
            return 0;
        }

        public int Execute(ref AssetBundleContext context, AssetEvent assetEvent)
        {
            if (assetEvent == AssetEvent.Update)
            {
               
                if(context.RemovedTasks != null)
                {
                    for (int i = 0; i < context.RemovedTasks.Count; i++)
                    {
                        long id = context.RemovedTasks[i];
                        for(int j =0; j < context.Tasks.Count;++j)
                        {
                            var task = context.Tasks[j];
                            if(task.TaskId == id)
                            {
                                if(AssetBundleConfig.DebugMode.HasEnum( DebugMode.Detail))
                                {
                                    Debug.LogFormat("Remove Task ==>> {0}", task);
                                }

                                context.Tasks.RemoveAt(j);
                                break;
                            }
                        }
                    }

                    context.RemovedTasks.Clear();
                }
            }

            return 0;
        }

    }


}

