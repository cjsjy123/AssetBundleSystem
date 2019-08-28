using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using CommonUtils;

namespace AssetBundleSystem
{
    internal struct AssetBundleContext
    {
        /// <summary>
        /// assetbundle 缓存
        /// </summary>
        public AssetBundleCache Cache;

        /// <summary>
        /// 任务
        /// </summary>
        public List<AssetBundleTask> Tasks;

        public List<AssetDownloadInfo> DownLoadQueue;

        public AssetProfilerData ProfilerData;

        public bool IsDestroying;

        public List<long> RemovedTasks;

        public AssetBundleContext(int capacity)
        {
            Cache = new AssetBundleCache(capacity);
            Tasks = ListPool<AssetBundleTask>.Get();
            Tasks.Capacity = capacity;
            DownLoadQueue = ListPool<AssetDownloadInfo>.Get();
            DownLoadQueue.Capacity = capacity;
            ProfilerData = new AssetProfilerData();
            IsDestroying = false;
            RemovedTasks = null;
        }

        public void Dispose()
        {
            if(Tasks != null)
            {
                ListPool<AssetBundleTask>.Release(Tasks);
                Tasks = null;
            }

            if (DownLoadQueue != null)
            {
                ListPool<AssetDownloadInfo>.Release(DownLoadQueue);
                DownLoadQueue = null;
            }

            if(RemovedTasks != null)
            {
                ListPool<long>.Release(RemovedTasks);
                RemovedTasks = null;
            }

            Cache.Dispose();
            ProfilerData.Dispose();
            IsDestroying = false;
        }

        public void AddTask(ref AssetBundleTask task)
        {
            if (Tasks != null)
            {
                if (task.Priority < 0)
                {
                    Tasks.Add(task);
                }
                else
                {
                    for (int i = 0; i < Tasks.Count; i++)
                    {
                        var intask = Tasks[i];
                        if (intask.Priority < task.Priority)
                        {
                            Tasks.Insert(i,task);
                            break;
                        }
                    }
                }
            }
        }

        public void RemoveTask(ref AssetBundleTask task)
        {
            RemoveTask(task.TaskId);
        }

        public void RemoveTask(long taskid)
        {
            if(RemovedTasks == null)
            {
                RemovedTasks = ListPool<long>.Get();
            }
            RemovedTasks.Add(taskid);
        }
    }
}


