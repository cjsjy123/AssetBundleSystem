using UnityEngine;
using System.Collections;
using UnityEngine.SceneManagement;

namespace AssetBundleSystem
{
    public partial class AssetBundleLoadManager
    {
        #region  Scene

        public LoadReturnArgs LoadScene(SceneLoadArgs args)
        {
            return LoadAssetScene(ref args);
        }

        public LoadReturnArgs LoadScene(string assetpath, LoadSceneMode md)
        {
            return LoadAssetScene(new SceneLoadArgs(assetpath, md, SceneLoadArgs.UnLoadSceneMode.None));
        }

        public LoadReturnArgs LoadSceneAsync(SceneLoadArgs args)
        {
            return LoadAssetScene(ref args, AssetBundleTaskType.LoadAsset | AssetBundleTaskType.Async);
        }


        public LoadReturnArgs LoadSceneAsync(string assetpath, LoadSceneMode md)
        {
            return LoadAssetScene(new SceneLoadArgs(assetpath, md, SceneLoadArgs.UnLoadSceneMode.None), AssetBundleTaskType.LoadAsset | AssetBundleTaskType.Async);
        }

        public LoadReturnArgs LoadSceneAsync(string assetpath, LoadSceneMode md,int prioroty)
        {
            return LoadAssetScene(new SceneLoadArgs(assetpath, md, SceneLoadArgs.UnLoadSceneMode.None, prioroty), AssetBundleTaskType.LoadAsset | AssetBundleTaskType.Async);
        }

        //-------------------UnLoad----------
        [System.Obsolete("This function is not safe to use during triggers and under other circumstances.")]
        public LoadReturnArgs UnLoadScene(string assetpath)
        {
            SceneLoadArgs args = new SceneLoadArgs(assetpath, LoadSceneMode.Single,SceneLoadArgs.UnLoadSceneMode.Sync);
            return LoadAssetScene(ref args, AssetBundleTaskType.UnLoadAsset);
        }
        [System.Obsolete("This function is not safe to use during triggers and under other circumstances.")]
        public LoadReturnArgs UnLoadScene(string assetpath,int priority)
        {
            SceneLoadArgs args = new SceneLoadArgs(assetpath, LoadSceneMode.Single, SceneLoadArgs.UnLoadSceneMode.Sync, priority);
            return LoadAssetScene(ref args, AssetBundleTaskType.UnLoadAsset);
        }

        public LoadReturnArgs UnLoadSceneAsync(string assetpath)
        {
            SceneLoadArgs args = new SceneLoadArgs(assetpath, LoadSceneMode.Single, SceneLoadArgs.UnLoadSceneMode.Async);
            return LoadAssetScene(ref args, AssetBundleTaskType.UnLoadAsset | AssetBundleTaskType.Async);
        }

        public LoadReturnArgs UnLoadSceneAsync(string assetpath, int priority)
        {
            SceneLoadArgs args = new SceneLoadArgs(assetpath, LoadSceneMode.Single, SceneLoadArgs.UnLoadSceneMode.Async, priority);
            return LoadAssetScene(ref args, AssetBundleTaskType.UnLoadAsset | AssetBundleTaskType.Async);
        }

        LoadReturnArgs LoadAssetScene(SceneLoadArgs args, AssetBundleTaskType type = AssetBundleTaskType.LoadAsset | AssetBundleTaskType.Sync)
        {
            return LoadAssetScene(ref args, type);
        }

        LoadReturnArgs LoadAssetScene(ref SceneLoadArgs args, AssetBundleTaskType type = AssetBundleTaskType.LoadAsset | AssetBundleTaskType.Sync)
        {
            if (_assetBundleFeature != null)
            {
                AssetBundleContext context = _assetBundleFeature.StartRead();

                AssetBundleInfo info;
                GameAssetBundle gameAssetBundle;
                if (CanLoad(ref context, args.AssetPath, out info, out gameAssetBundle))
                {
                    AssetBundleTask task = new AssetBundleTask();
                    task.AssetInfo = info;
                    task.TaskLoadType = type;
                    task.TaskId = ++_taskid;
                    task.TaskResType = TaskResType.Scene;
                    task.Priority = args.Priority;
                    task.CreateTime = Time.realtimeSinceStartup;
                    task.LoadSceneMode = args.SceneMode;

                    context.AddTask(ref task);
                    _assetBundleFeature.EndRead(ref context);

                    LoadReturnArgs returnArgs = new LoadReturnArgs(this._assetBundleFeature, task.TaskId);
                    return returnArgs;
                }
            }
            return default(LoadReturnArgs);
        }


        #endregion

    }

}
