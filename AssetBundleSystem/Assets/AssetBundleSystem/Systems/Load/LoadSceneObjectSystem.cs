using UnityEngine;
using System.Collections;
using System;
using CommonUtils;
using UnityEngine.SceneManagement;

namespace AssetBundleSystem
{
    internal class LoadSceneObjectSystem : LoadCommonObjectSystem
    {
        protected override bool IsEnableTask(ref AssetBundleContext context, ref AssetBundleTask task)
        {
            return task.TaskResType == TaskResType.Scene && base.IsEnableTask(ref context, ref task);
        }

        protected override void UnLoad(ref AssetBundleTask task, ref GameAssetBundle gameAssetBundle, ref bool retcode)
        {
            Scene scene = SceneManager.GetSceneByPath(task.AssetPath);
            if(scene.IsValid() && scene.isLoaded)
            {
                if (task.IsAsync())
                {
                    if (gameAssetBundle.UnloadSceneRequest == null)
                    {
                        gameAssetBundle.UnloadSceneRequest = SceneManager.UnloadSceneAsync(task.AssetPath);
                    }
                    else if (gameAssetBundle.UnloadSceneRequest != null)
                    {
                        if (gameAssetBundle.UnloadSceneRequest.progress.SimpleEqual(0.9f))
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
                }
                else
                {
#pragma warning disable
                    if ( SceneManager.UnloadScene(task.AssetPath))
                    {
                        Debug.LogFormat("UnLoadScene  :{0} Success", task.AssetPath);
                    }
#pragma warning restore
                    if (task.Result.ProgresCallback != null)
                    {
                        ProgressArgs progressArgs = new ProgressArgs(1, 0, task.AssetPath, true);
                        task.Result.ProgresCallback(ref progressArgs);
                    }
                    task.FinishTime = Time.realtimeSinceStartup;
                    retcode = true;
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


        protected override void LoadMainAsset(ref AssetBundleContext context, ref AssetBundleTask task, ref GameAssetBundle gameAssetBundle,ref bool retcode)
        {
            if(task.IsAsync())
            {
                if (gameAssetBundle.SceneRequest == null)//准备加载场景
                {
                    gameAssetBundle.SceneRequest = SceneManager.LoadSceneAsync(task.AssetPath,task.LoadSceneMode);
                    if(gameAssetBundle.SceneRequest != null)
                    {
                        gameAssetBundle.SceneRequest.allowSceneActivation = false;
                    }

                    if (task.Result.ProgresCallback != null)
                    {
                        ProgressArgs progressArgs = new ProgressArgs( 0, 0, task.AssetPath, true);
                        task.Result.ProgresCallback(ref progressArgs);
                    }

                }
                else if(gameAssetBundle.SceneRequest != null)//检查加载场景的状态
                {
                    if(Mathf.Abs(gameAssetBundle.SceneRequest.progress- 0.9f) < 0.001f)
                    {
                        gameAssetBundle.AssetStatus |= AssetBundleStatus.InMemory;
                        //暂时不释放
                        AssetBundleFunction.AddRef(ref context, ref gameAssetBundle,task.AssetPath);

                        task.Result.scene = gameAssetBundle.SceneRequest;

                        gameAssetBundle.SceneRequest = null;

                        if (task.Result.ProgresCallback != null)
                        {
                            ProgressArgs progressArgs = new ProgressArgs(1, 0, task.AssetPath, true);
                            task.Result.ProgresCallback(ref progressArgs);
                        }
                        task.FinishTime = Time.realtimeSinceStartup;
                        retcode = true;
                    }
                }
            }
            else
            {
                SceneManager.LoadScene(task.AssetPath,task.LoadSceneMode);
                gameAssetBundle.AssetStatus |= AssetBundleStatus.InMemory;
                AssetBundleFunction.AddRef(ref context, ref gameAssetBundle, task.AssetPath); 

                if (task.Result.ProgresCallback != null)
                {
                    ProgressArgs progressArgs = new ProgressArgs(1, 0, task.AssetPath, true);
                    task.Result.ProgresCallback(ref progressArgs);
                }
                task.FinishTime = Time.realtimeSinceStartup;
                retcode = true;
            }

        }


    }
}





