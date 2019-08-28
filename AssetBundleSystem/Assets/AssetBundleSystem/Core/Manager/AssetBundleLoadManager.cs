using UnityEngine;
using System.Collections;
using Object = UnityEngine.Object;

namespace AssetBundleSystem
{

    public partial class AssetBundleLoadManager
    {
        private long _taskid;
        /// <summary>
        /// 1000-1999
        /// </summary>
        private const long CancelTask = 1000;
        /// <summary>
        /// 2000-2999
        /// </summary>
        private const long UnLoadTask = 2000;

        public void ReLoad()
        {
            if(_assetBundleFeature != null)
            {
                _assetBundleFeature.ReLoad();
            }
        }

        public void UnloadAllUnusedAssets()
        {
            Resources.UnloadUnusedAssets();
        }

        public bool IsProfilering()
        {
            if(_assetBundleFeature != null)
            {
                return _assetBundleFeature.StartRead().ProfilerData.IsProfilering;
            }
            return false;
        }

        public void SetProfiler(bool value)
        {
            if(_assetBundleFeature != null)
            {
                var context = _assetBundleFeature.StartRead();
                if(context.ProfilerData.IsProfilering != value)
                {
                    context.ProfilerData.IsProfilering = value;
                    _assetBundleFeature.EndRead(ref context);
                }
            }
        }

        public void ReomveTask(long id)
        {
            if (_assetBundleFeature != null)
            {
                var context = _assetBundleFeature.StartRead();
                context.RemoveTask(id);
                _assetBundleFeature.EndRead(ref context);
            }
        }

        #region load
        public LoadReturnArgs LoadAsset(string assetpath)
        {
            return LoadCommonAsset(new CommonLoadArgs(assetpath));
        }

        public LoadReturnArgs LoadAsset(string assetpath,int priority)
        {
            return LoadCommonAsset(new CommonLoadArgs(assetpath, priority));
        }

        public LoadReturnArgs LoadAssetAsync(string assetpath)
        {
            return LoadCommonAsset(new CommonLoadArgs(assetpath), AssetBundleTaskType.Async | AssetBundleTaskType.LoadAsset);
        }

        public LoadReturnArgs LoadAssetAsync(string assetpath, int priority)
        {
            return LoadCommonAsset(new CommonLoadArgs(assetpath, priority));
        }

        LoadReturnArgs LoadCommonAsset( CommonLoadArgs args, AssetBundleTaskType type = AssetBundleTaskType.Sync | AssetBundleTaskType.LoadAsset)
        {
            return LoadCommonAsset(ref args, type);
        }

        LoadReturnArgs LoadCommonAsset(ref CommonLoadArgs args,AssetBundleTaskType type = AssetBundleTaskType.Sync | AssetBundleTaskType.LoadAsset)
        {
            if (_assetBundleFeature != null)
            {
                AssetBundleContext context = _assetBundleFeature.StartRead();

                AssetBundleInfo info;
                GameAssetBundle gameAssetBundle;
                if (CanLoad(ref context, args.AssetPath, out info, out gameAssetBundle))
                {
                    AssetBundleTask task = new AssetBundleTask();
                    //
                    task.AssetInfo = info;
                    task.TaskLoadType = type;
                    task.TaskId = ++_taskid;
                    task.TaskResType = TaskResType.Common;
                    task.Priority = args.Priority;
                    task.CreateTime = Time.realtimeSinceStartup;

                    //
                    Debug.LogFormat("Task ---- {0} Created", task);

                    context.AddTask(ref task);
                    _assetBundleFeature.EndRead(ref context);

                    LoadReturnArgs returnArgs = new LoadReturnArgs(this._assetBundleFeature, task.TaskId);
                    return returnArgs;
                }
            }

            return default(LoadReturnArgs);
        }
        #endregion
        #region preload
        public LoadReturnArgs PreLoadAsset(string assetpath)
        {
            return PreLoadAsset(new PreLoadArgs(assetpath));
        }

        public LoadReturnArgs PreLoadAsset(string assetpath,float time)
        {
            return PreLoadAsset(new PreLoadArgs(assetpath,time));
        }

        public LoadReturnArgs PreLoadAssetAsync(string assetpath)
        {
            return PreLoadAsset(new PreLoadArgs(assetpath), AssetBundleTaskType.Async | AssetBundleTaskType.PreLoadAsset);
        }

        public LoadReturnArgs PreLoadAssetAsync(string assetpath,float time)
        {
            return PreLoadAsset(new PreLoadArgs(assetpath,time), AssetBundleTaskType.Async | AssetBundleTaskType.PreLoadAsset);
        }

        public LoadReturnArgs PreLoadAssetAsync(string assetpath, float time,int priority)
        {
            return PreLoadAsset(new PreLoadArgs(assetpath, time, priority), AssetBundleTaskType.Async | AssetBundleTaskType.PreLoadAsset);
        }

        LoadReturnArgs PreLoadAsset(PreLoadArgs args, AssetBundleTaskType type = AssetBundleTaskType.Sync | AssetBundleTaskType.PreLoadAsset)
        {
            return PreLoadAsset(ref args, type);
        }

        LoadReturnArgs PreLoadAsset(ref PreLoadArgs args, AssetBundleTaskType type = AssetBundleTaskType.Sync | AssetBundleTaskType.PreLoadAsset)
        {
            if (_assetBundleFeature != null)
            {
                AssetBundleContext context = _assetBundleFeature.StartRead();

                AssetBundleInfo info;
                GameAssetBundle gameAssetBundle;
                if (CanLoad(ref context, args.AssetPath, out info, out gameAssetBundle))
                {
                    AssetBundleTask task = new AssetBundleTask();
                    //
                    task.AssetInfo = info;
                    task.TaskLoadType = type;
                    task.TaskId = ++_taskid;
                    task.TaskResType = TaskResType.GameObject;
                    task.Priority = args.Priority;
                    task.CreateTime = Time.realtimeSinceStartup;
                    task.PinTime = args.PinTime;

                    //
                    Debug.LogFormat("Task ---- {0} Created", task);

                    context.AddTask(ref task);
                    _assetBundleFeature.EndRead(ref context);

                    LoadReturnArgs returnArgs = new LoadReturnArgs(this._assetBundleFeature, task.TaskId);
                    return returnArgs;
                }
            }

            return default(LoadReturnArgs);
        }
        #endregion

        //public void UnLoadAsset(string assetpath)
        //{
        //    if(_assetBundleFeature != null)
        //    {
        //        var context = _assetBundleFeature.StartRead();

        //        AssetBundleInfo info;
        //        GameAssetBundle gameAssetBundle;
        //        if(context.Cache.TryGetInfo(assetpath,out info) && context.Cache.GetAssetBundle(info.AssetName,out gameAssetBundle))
        //        {

        //            //gameAssetBundle.RemoveUserRef();
        //            context.Cache.UpdateAssetBundle(info.AssetBundleName, ref gameAssetBundle);
        //        }

        //        _assetBundleFeature.EndRead(ref context);
        //    }
        //}

        //public void UnLoadAsset(Object obj)
        //{
        //    if (_assetBundleFeature != null)
        //    {
        //        var context = _assetBundleFeature.StartRead();

        //        bool contains = false;
        //        GameAssetBundle tempbundle = default(GameAssetBundle);
        //        foreach(var gameassetbundle in context.Cache.GetAllAssetBundle())
        //        {
        //            if(gameassetbundle.Value.Contains(obj))
        //            {
        //                tempbundle = gameassetbundle.Value;
        //                contains = true;
        //                break;
        //            }
        //        }

        //        if(contains)
        //        {
        //            //tempbundle.RemoveUserRef();
        //            context.Cache.UpdateAssetBundle(tempbundle.AssetBundleInfo.AssetBundleName,ref tempbundle);
        //        }

        //        _assetBundleFeature.EndRead(ref context);
        //    }
        //}

        /// <summary>
        /// 这里会检查是否包含assetbundle info ，是否已经加载,是否是异常资源
        /// </summary>
        internal bool CanLoad(ref AssetBundleContext context,string assetpath,out AssetBundleInfo info, out GameAssetBundle assetbundle)
        {
            if (context.Cache.CanLoad(assetpath, out info))
            {
                if(!context.Cache.GetAssetBundle(info.AssetBundleName,out assetbundle))
                {
                    assetbundle.AssetBundleInfo = info;
                    context.Cache.UpdateAssetBundle(info.AssetBundleName,ref assetbundle);
                }
                return true;
            }
            else
            {
                Debug.LogErrorFormat("{0} can't Load ", assetpath);
            }

            assetbundle = default(GameAssetBundle);
            return false;
        }

        public AssetBundleManifest GetManifest(string targetPath)
        {
            if (_assetBundleFeature != null)
            {
                var context = _assetBundleFeature.GetContext();
                AssetBundleInfo info;
                if (context.Cache.CanLoad(targetPath, out info))
                {
                    GameAssetBundle assetbundle;
                    if (!context.Cache.GetAssetBundle(info.AssetBundleName, out assetbundle))
                    {
                        assetbundle.AssetBundleInfo = info;
                        if (assetbundle.AssetBundle != null)
                        {
                            return assetbundle.AssetBundle.LoadAsset<AssetBundleManifest>("AssetBundleManifest");
                        }
                    }
                }
            }
            return null;
        }
    }
}


