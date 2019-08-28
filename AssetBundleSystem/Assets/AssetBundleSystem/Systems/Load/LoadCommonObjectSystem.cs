using UnityEngine;
using System;
using System.IO;
using CommonUtils;
using Object = UnityEngine.Object;

namespace AssetBundleSystem
{
    internal class LoadCommonObjectSystem : IAssetBundleSystem, IAssetBundleInitContext<AssetBundleContext>, IAssetBundleExecute<AssetBundleContext, AssetEvent>
    {
        public bool Block { get; protected set; }

        public virtual int Init(ref AssetBundleContext context)
        {
            return 0;
        }

        public int Execute(ref AssetBundleContext context, AssetEvent assetEvent)
        {
            if (assetEvent == AssetEvent.Now || assetEvent == AssetEvent.Update)
            {
                if (!context.IsDestroying)
                {
                    TryDoTask(ref context);
                }
            }

            return 0;
        }

        protected virtual void TryDoTask(ref AssetBundleContext context)
        {
            int loadcnt = Mathf.Max(1, AssetBundleConfig.TaskStep);
            int loaded = 0;
            for (int i = 0; i < context.Tasks.Count; i++)
            {
                var task = context.Tasks[i];
                if ( IsEnableTask(ref context, ref task))
                {

                    bool changed = true;
                    bool indownloading = AssetBundleFunction.InDowloading(ref context, ref task);
                    if (indownloading)
                    {
                        changed = false;
                    }
                    else
                    {
                        try
                        {
                            changed = LoadAssetBundle(ref context, ref task);
                        }
                        catch (Exception e)
                        {
                            Debug.LogError(e);

                            changed = true;
                            AddException(ref context, task.AssetPath);
                        }
                    }


                    if (changed)
                        context.Tasks[i] = task;

                    if(task.IsPreLoad() || indownloading)//ignore
                    {
                        continue;
                    }
                    else//真实加载的才被记录
                    {
                        loaded++;
                        if(loaded >= loadcnt)
                        {
                            break;
                        }
                    }
                }
            }
        }

        protected virtual bool IsEnableTask(ref AssetBundleContext context,ref AssetBundleTask task)
        {
            return !task.IsStatus(ref context, AssetBundleStatus.FileNotExist| AssetBundleStatus.LoadException | AssetBundleStatus.NotInMemory);
        }

        protected virtual void AddException(ref AssetBundleContext context,string assetpath)
        {
            context.Cache.UpdateAssetBundleStatus(assetpath, AssetBundleStatus.LoadException, true);
            context.Cache.AddExceptionRes(assetpath);
        }

        bool LoadAssetBundle(ref AssetBundleContext context, ref AssetBundleTask task)
        {
            bool result = false;

            if(task.FinishTime >0)
            {
                result = true;
            }
            else
            {

                if (task.IsLoad())
                {
                    LoadDependency(ref context, ref task, ref result);

                    LoadSubTask(ref context, ref task, ref result);

                    int finishcnt;
                    int totalcnt;
                    if (task.IsSubAllDone(ref context, out finishcnt, out totalcnt) )//
                    {

                        GameAssetBundle gameAssetBundle;
                        if (context.Cache.GetAssetBundle(task.AssetBundleName, out gameAssetBundle))
                        {
                            if (task.Result.ProgresCallback != null)
                            {
                                ProgressArgs progressArgs = new ProgressArgs((float)finishcnt / (totalcnt + 1), 0, task.AssetPath, false);
                                task.Result.ProgresCallback(ref progressArgs);
                            }

                            Load(ref context, ref task, ref gameAssetBundle, ref result, true);
                            context.Cache.UpdateAssetBundle(task.AssetBundleName, ref gameAssetBundle);
                        }
                        else
                        {
                            Debug.LogErrorFormat("not found gameassetbundle :{0}", task.AssetBundleName);
                            AddException(ref context, task.AssetPath);
                        }

                    }
                    else
                    {
                        if (task.Result.ProgresCallback != null)
                        {
                            ProgressArgs progressArgs = new ProgressArgs((float)finishcnt / (totalcnt + 1), 0, task.AssetPath, false);
                            task.Result.ProgresCallback(ref progressArgs);
                        }
                    }
                }
                else if (task.IsUnLoad())
                {
                    GameAssetBundle gameAssetBundle;
                    if (context.Cache.GetAssetBundle(task.AssetBundleName, out gameAssetBundle))
                    {
                        UnLoad(ref task, ref gameAssetBundle, ref result);
                    }
                    else
                    {
                        Debug.LogErrorFormat("not found gameassetbundle :{0}", task.AssetBundleName);
                        AddException(ref context, task.AssetPath);
                    }
                }
                else if (task.IsPreLoad())
                {
                    LoadDependency(ref context, ref task, ref result);
                    PreLoadSubTask(ref context, ref task, ref result);

                    int finishcnt;
                    int totalcnt;
                  
                    if (task.IsSubAllDone(ref context, out finishcnt, out totalcnt) )//
                    {
                        GameAssetBundle gameAssetBundle;
                        if (context.Cache.GetAssetBundle(task.AssetBundleName, out gameAssetBundle))
                        {
                            if (task.Result.ProgresCallback != null)
                            {
                                ProgressArgs progressArgs = new ProgressArgs((float)finishcnt / (totalcnt + 1), 0, task.AssetPath, false);
                                task.Result.ProgresCallback(ref progressArgs);
                            }

                            PreLoad(ref context, ref task, ref gameAssetBundle, ref result, true);
                            context.Cache.UpdateAssetBundle(task.AssetBundleName, ref gameAssetBundle);
                        }
                        else
                        {
                            Debug.LogErrorFormat("not found gameassetbundle :{0}", task.AssetBundleName);
                            AddException(ref context, task.AssetPath);
                        }

                    }
                    else
                    {
                        if (task.Result.ProgresCallback != null)
                        {
                            ProgressArgs progressArgs = new ProgressArgs((float)finishcnt / (totalcnt + 1), 0, task.AssetPath, false);
                            task.Result.ProgresCallback(ref progressArgs);
                        }
                    }
                }
            }

            return result;
        }


        protected virtual bool IsEnableLoadAssetBunlde(ref AssetBundleContext context, ref AssetBundleTask task, ref GameAssetBundle gameAssetBundle)
        {
            return true;
        }

        protected virtual void Load(ref AssetBundleContext context, ref AssetBundleTask task, 
        ref GameAssetBundle gameAssetBundle, ref bool retcode, bool mainloadrequest = false)
        {

            if(gameAssetBundle.IsException())
            {
                return;
            }

            if (AssetBundleConfig.DebugMode.HasEnum(DebugMode.Detail))
                Debug.LogFormat("Load --- {0}", task);

            gameAssetBundle.AssetStatus |= AssetBundleStatus.Loading;
            //not loaded
            if (gameAssetBundle.AssetBundle == null && gameAssetBundle.LoadRequest == null && gameAssetBundle.LoadAssetRequest == null)
            {
                if(IsEnableLoadAssetBunlde(ref context,ref task,ref gameAssetBundle))
                {
                    string assetfilename = AssetBundleConfig.Convert(task.AssetBundleName);
                    string path = AssetBundleHelper.GetBundlePersistentPath(assetfilename);
                    bool fileExist = File.Exists(path);

                    string url;
                    bool indownloadcache = context.Cache.TryGetDownloadUrl(new IgnoreCaseString(task.AssetBundleName), out url);
                    bool canload = false;
                    if(indownloadcache && !fileExist)
                    {
                        AssetDownloadInfo downloadInfo = new AssetDownloadInfo();
                        downloadInfo.AssetPath = task.AssetPath;
                        downloadInfo.AssetBundleName = task.AssetBundleName;
                        if (!context.DownLoadQueue.Contains(downloadInfo))
                        {
                            downloadInfo.DstPath = path;
                            downloadInfo.Url = url;
                            downloadInfo.TaskId = task.TaskId;
                            context.DownLoadQueue.Add(downloadInfo);
                        }
                    }
                    else if(fileExist)
                    {
                        canload = true;
                    }
                    else if(!fileExist)
                    {
                        string streampath = AssetBundleHelper.GetBundleStreamPath(assetfilename);
                        if(File.Exists(streampath))
                        {
                            canload = true;
                            path = streampath;
                        }
                    }

                    if (canload)
                    {
                        if (task.IsAsync())
                        {
                            gameAssetBundle.LoadRequest = AssetBundle.LoadFromFileAsync(path);
                            if (AssetBundleConfig.DebugMode.HasEnum(DebugMode.Detail))
                            {
                                Debug.LogFormat("LoadFromFileAsync AssetBundle :{0} ", task.AssetBundleName);
                            }

                        }
                        else
                        {
                            gameAssetBundle.AssetBundle = AssetBundle.LoadFromFile(path);
                            if (AssetBundleConfig.DebugMode.HasEnum(DebugMode.Detail))
                            {
                                Debug.LogFormat("LoadFromFile AssetBundle :{0} ", task.AssetBundleName);
                            }

                            AfterLoadAssetBundle(ref context, ref task, ref gameAssetBundle, ref retcode, ref mainloadrequest);
                        }
                    }
                    else if(!indownloadcache)
                    {
                        gameAssetBundle.AssetStatus |= AssetBundleStatus.FileNotExist;
                        AddException(ref context,task.AssetPath);
                        Debug.LogError(string.Format( "cant load :{0}", task));
                    
                    }
                }
            }
            else if (gameAssetBundle.LoadRequest != null)// in asyncing Load AssetBundle
            {
                if (gameAssetBundle.LoadRequest.isDone)
                {
                    if (AssetBundleConfig.DebugMode.HasEnum(DebugMode.Detail))
                    {
                        Debug.LogFormat("Async Task for Load AssetBundle is Done :{0} ", task);
                    }
                    gameAssetBundle.AssetBundle = gameAssetBundle.LoadRequest.assetBundle;

                    AfterLoadAssetBundle(ref context,ref task, ref gameAssetBundle,ref retcode,ref mainloadrequest);
                    gameAssetBundle.LoadRequest = null;
                }
            }
            else if (gameAssetBundle.LoadAssetRequest != null && mainloadrequest)// Load Asset
            {
                if (gameAssetBundle.LoadAssetRequest.isDone)
                {
                    var loadfinishasset = gameAssetBundle.LoadAssetRequest.asset;

                    if (loadfinishasset != null)
                    {
                        if (AssetBundleConfig.DebugMode.HasEnum(DebugMode.Detail))
                        {
                            Debug.LogFormat("Async Task Add Asset :{0} ", task);
                        }
                        gameAssetBundle.AddAsset(task.AssetPath, gameAssetBundle.LoadAssetRequest.asset);
                        gameAssetBundle.AssetStatus |= AssetBundleStatus.InMemory;
                        if (task.Result.ProgresCallback != null)
                        {
                            ProgressArgs progressArgs = new ProgressArgs(1, 0, task.AssetPath, true);
                            task.Result.ProgresCallback(ref progressArgs);
                        }
                        task.FinishTime = Time.realtimeSinceStartup;
                        retcode = true;
                    }
                    else
                    {
                        Debug.LogErrorFormat("{0} has 0 Assets", gameAssetBundle.AssetBundle);
                        gameAssetBundle.AssetStatus |= AssetBundleStatus.NotInMemory;
                    }

                    gameAssetBundle.LoadAssetRequest = null;
                }
            }
            else
            {
                if (mainloadrequest)
                {
                    LoadMainAsset(ref context, ref task, ref gameAssetBundle,ref retcode);
                }
            }

        }
        //Load Main Asset
        protected virtual  void LoadMainAsset(ref AssetBundleContext context, ref AssetBundleTask task, ref GameAssetBundle gameAssetBundle, ref bool retcode)
        {
            if (!gameAssetBundle.ContainsAsset(task.AssetPath))
            {
                if (task.IsAsync())
                {
                    //Load Async Asset
   
                    if (task.AssetInfo.AssetResType == AssetBundleResType.Image)
                    {
         
                        gameAssetBundle.LoadAssetRequest = gameAssetBundle.AssetBundle.LoadAssetAsync<Sprite>(task.AssetInfo.AssetName);
                        if (gameAssetBundle.LoadAssetRequest == null || gameAssetBundle.LoadAssetRequest.asset == null)
                        {
                            gameAssetBundle.LoadAssetRequest = gameAssetBundle.AssetBundle.LoadAssetAsync(task.AssetInfo.AssetName);
                        }
                    }
                    else
                    {
                        gameAssetBundle.LoadAssetRequest = gameAssetBundle.AssetBundle.LoadAssetAsync(task.AssetInfo.AssetName);
                    }

                    if (AssetBundleConfig.DebugMode.HasEnum(DebugMode.Detail))
                    {
                        Debug.LogFormat("Load Async Asset :{0}", task);
                    }
                }
                else
                {
                    //Load Asset
                    Object asset = null;
                    if(task.AssetInfo.AssetResType == AssetBundleResType.Image)
                    {
                        asset = gameAssetBundle.AssetBundle.LoadAsset<Sprite>(task.AssetInfo.AssetName);
                        if(asset == null)
                        {
                            asset = gameAssetBundle.AssetBundle.LoadAsset(task.AssetInfo.AssetName);

                        }
                    }
                    else
                    {
                        asset = gameAssetBundle.AssetBundle.LoadAsset(task.AssetInfo.AssetName);
                    }
                    // gameAssetBundle.AssetBundle.LoadAsset(task.AssetInfo.AssetName);

                    if (AssetBundleConfig.DebugMode.HasEnum(DebugMode.Detail))
                    {
                        Debug.LogFormat("Load Sync Asset :{0}", task);
                    }

                    if (asset != null)
                    {
                        gameAssetBundle.AddAsset(task.AssetPath, asset);
                        if (AssetBundleConfig.DebugMode.HasEnum(DebugMode.Detail))
                        {
                            Debug.LogFormat("Add Asset :{0} ", task);
                        }
                        gameAssetBundle.AssetStatus |= AssetBundleStatus.InMemory;

                        if (task.Result.ProgresCallback != null)
                        {
                            ProgressArgs progressArgs = new ProgressArgs(1, 0, task.AssetPath, true);
                            task.Result.ProgresCallback(ref progressArgs);
                        }
                        task.FinishTime = Time.realtimeSinceStartup;
                        retcode = true;
                    }
                    else
                    {
                        Debug.LogErrorFormat("{0} cant load Asset", gameAssetBundle.AssetBundle);
                        gameAssetBundle.AssetStatus |= AssetBundleStatus.NotInMemory;
                    }
                }
            }
            else
            {
                if (task.Result.ProgresCallback != null)
                {
                    ProgressArgs progressArgs = new ProgressArgs(1, 0, task.AssetPath, true);
                    task.Result.ProgresCallback(ref progressArgs);
                }

                task.FinishTime = Time.realtimeSinceStartup;
                retcode = true;
            }
        }

        protected virtual void LoadDependency(ref AssetBundleContext context, ref AssetBundleTask task, ref bool retcode)
        {
            var depinfo = task.AssetInfo.DependNames;
            if (depinfo != null && depinfo.Length > 0 && task.SubTasks == null)
            {
                for (int i = 0; i < depinfo.Length; i++)
                {
                    var depname = depinfo[i];
                    AssetBundleInfo info;
                    GameAssetBundle gameAssetBundle;
                    if (AssetBundleLoadManager.mIns.CanLoad(ref context, depname, out info, out gameAssetBundle))
                    {
                        if (!gameAssetBundle.AssetStatus.HasEnum( AssetBundleStatus.InMemory))
                        {
                            AssetBundleTask subtask = new AssetBundleTask();
                            subtask.AssetInfo = info;
                            subtask.Priority = task.Priority;
                            subtask.TaskResType = task.TaskResType;
                            subtask.TaskLoadType = task.TaskLoadType;
                            subtask.TaskId = task.TaskId;
                            subtask.CreateTime = Time.realtimeSinceStartup;

                            task.AddSubTask(ref subtask);
                            if (AssetBundleConfig.DebugMode.HasEnum(DebugMode.Detail))
                                Debug.LogFormat("subTask --- {0}  Created ", subtask);
                            LoadDependency(ref context, ref subtask,ref retcode);
                        }

                    }
                    else
                    {
                        Debug.LogErrorFormat("cant load Dependency :{0}", depname);
#pragma warning disable
                        if (AssetBundleConfig.SafeMode)
                        {
                            AddException(ref context, depname);
                        }
#pragma warning restore
                    }
                }

                retcode = true;
            }
        }

        protected virtual void LoadSubTask(ref AssetBundleContext context, ref AssetBundleTask task, ref bool retcode)
        {
            if (task.SubTasks != null && task.SubTasks.Count > 0)
            {
                for (int i = 0; i < task.SubTasks.Count; i++)
                {
                    var subtask = task.SubTasks[i];
                    GameAssetBundle gameAssetBundle;
                    if (context.Cache.GetAssetBundle(subtask.AssetBundleName, out gameAssetBundle))
                    {
                        Load(ref context, ref subtask, ref gameAssetBundle,ref retcode);

                        //Update
                        context.Cache.UpdateAssetBundle(subtask.AssetBundleName, ref gameAssetBundle);

                        LoadSubTask(ref context, ref subtask,ref retcode);
                    }
                    else
                    {
                        Debug.LogErrorFormat("not found assetbundle :{0}", subtask.AssetPath);

                        AddException(ref context, subtask.AssetPath);
                    }
                }
            }
        }

        protected virtual void AfterLoadAssetBundle(ref AssetBundleContext context, ref AssetBundleTask task, ref GameAssetBundle gameAssetBundle, ref bool retcode, ref bool mainloadrequest)
        {
            if (gameAssetBundle.AssetBundle == null)
            {
                gameAssetBundle.AssetStatus |= AssetBundleStatus.NotInMemory;
            }
            else if (!mainloadrequest)//作为依赖
            {
                if (AssetBundleConfig.DebugMode.HasEnum(DebugMode.Detail))
                {
                    Debug.LogFormat("dependency :{0} is Done", task);
                }
                gameAssetBundle.AssetStatus |= AssetBundleStatus.InMemory;
            }
            else
            {
                LoadMainAsset(ref context,ref task, ref gameAssetBundle,ref retcode);
            }
        }

        protected virtual  void UnLoad(ref AssetBundleTask task, ref GameAssetBundle gameAssetBundle, ref bool retcode)
        {

        }

        protected virtual void PreLoad(ref AssetBundleContext context, ref AssetBundleTask task, ref GameAssetBundle gameAssetBundle, ref bool retcode, bool mainloadrequest = false)
        {
            this.Load(ref context, ref task, ref gameAssetBundle, ref retcode, mainloadrequest);
        }

        protected virtual void PreLoadSubTask(ref AssetBundleContext context, ref AssetBundleTask task, ref bool retcode)
        {
            if (task.SubTasks != null && task.SubTasks.Count > 0)
            {
                for (int i = 0; i < task.SubTasks.Count; i++)
                {
                    var subtask = task.SubTasks[i];
                    GameAssetBundle gameAssetBundle;
                    if (context.Cache.GetAssetBundle(subtask.AssetBundleName, out gameAssetBundle))
                    {
                        PreLoad(ref context, ref subtask, ref gameAssetBundle, ref retcode);

                        //Update
                        context.Cache.UpdateAssetBundle(subtask.AssetBundleName, ref gameAssetBundle);

                        PreLoadSubTask(ref context, ref subtask, ref retcode);
                    }
                    else
                    {
                        Debug.LogErrorFormat("not found assetbundle :{0}", subtask.AssetPath);
                        AddException(ref context, subtask.AssetPath);
                    }
                }
            }
        }
    }

}