using System;
using UnityEngine;
using System.Collections;
using Object = UnityEngine.Object;
using CommonUtils;

namespace AssetBundleSystem
{

    internal class LoadDispatcherSystem : IAssetBundleSystem, IAssetBundleInitContext<AssetBundleContext>, IAssetBundleExecute<AssetBundleContext, AssetEvent>,IAssetBundleReadData
    {

        public bool Block { get; private set; }

        public IFeature Parent { get;  set; }

        public int Init(ref AssetBundleContext context)
        {
            return 0;
        }

        public int Execute(ref AssetBundleContext context, AssetEvent assetEvent)
        {
            if (assetEvent == AssetEvent.Now || assetEvent == AssetEvent.LateUpdate || assetEvent == AssetEvent.Destroy)
            {
                if(!context.IsDestroying)
                {
                    for (int i = 0; i < context.Tasks.Count; i++)
                    {
                        var task = context.Tasks[i];
                        bool changed = false;
                        try
                        {
                            changed = DispatcherTask(ref context, ref task);
                        }
                        catch (Exception e)
                        {
                            Debug.LogError(e);
                            changed = true;

                            context.Cache.UpdateAssetBundleStatus(task.AssetPath, AssetBundleStatus.LoadException, true);
                        }

                        if (changed)
                        {
                            context.Tasks[i] = task;
                        }
                    }
                    RemoveSomeTasks(ref context);
                }
            }

            return 0;
        }

        void RemoveSomeTasks(ref AssetBundleContext context)
        {
            for (int i = context.Tasks.Count-1; i >=0; i--)
            {
                var task = context.Tasks[i];
                if (task.IsDone(ref context))
                {
                    context.Tasks.RemoveAt(i);
                }
            }
        }

        bool DispatcherTask(ref AssetBundleContext context, ref AssetBundleTask task)
        {
            GameAssetBundle gameAssetBundle;
            if (task.IsDone(ref context,out gameAssetBundle))
            {
                //set or not ?
                //task.FinishTime = Time.realtimeSinceStartup;
                task.Result.Result = !gameAssetBundle.IsException();
                task.Result.LoadObject = gameAssetBundle.LoadAsset(task.AssetPath);
                task.Result.RuntimeInfo = task.AssetInfo;
                //因为这里只改边list 中的task存储内容，不涉及context 非reference的修改，所以没有关系
                task.Result.UpdateReferenceAction = AddRefrence;

                //notify
                if (task.Result.ResultCallback != null)
                {
                    task.Result.ResultCallback(ref task.Result);
                }
                else if(task.IsPreLoad())
                {
                    if(task.PinTime < 0)
                    {
                        AssetBundleFunction.AddRef(ref context, ref gameAssetBundle,task.AssetPath);
#if UNITY_EDITOR
                        context.Cache.UpdateAssetBundle(task.AssetBundleName, ref gameAssetBundle);
#endif
                    }
                }
                else if (task.TaskResType == TaskResType.GameObject)
                {
                    task.Result.Instantiate();
                }
                else if (task.TaskResType == TaskResType.Scene)
                {
                    if (!task.Result.Hide)
                    {
                        task.Result.LoadScene();
                    }
                }


                Debug.LogFormat("DisposeTask : {0} dispose", task);
                DisposeTask(ref task);
                return true;

            }

            return false;
        }


        void DisposeTask(ref AssetBundleTask task)
        {
            if (task.SubTasks != null)
            {
                for (int i = 0; i < task.SubTasks.Count; i++)
                {
                    var subtask = task.SubTasks[i];
                    DisposeTask(ref subtask);
                }
            }

            task.Dispose();
        }



        void AddRefrence(ref AssetBundleInfo assetbundleinfo,Object target)
        {
            AssetBundleFeature assetfeature = Parent as AssetBundleFeature;
            if(assetfeature != null)
            {
                var context = assetfeature.StartRead();
                AssetBundleFunction.AddAssetBundleRef(ref context,ref assetbundleinfo, assetbundleinfo.AssetBundleName, target);

                assetfeature.EndRead(ref context);
            }
            else
            {
                Debug.LogError("assetfeature is Null");
            }
        }

      
    }

}

