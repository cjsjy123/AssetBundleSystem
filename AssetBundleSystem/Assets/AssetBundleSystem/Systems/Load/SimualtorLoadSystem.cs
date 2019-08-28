using UnityEngine;
using System.Collections.Generic;
using CommonUtils;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace AssetBundleSystem
{
    internal class SimualtorLoadSystem : IAssetBundleSystem, IAssetBundleExecute<AssetBundleContext, AssetEvent>
    {
        public bool Block { get; protected set; }

        public int Execute(ref AssetBundleContext context, AssetEvent assetEvent)
        {
#if UNITY_EDITOR
            if (assetEvent == AssetEvent.Now || assetEvent == AssetEvent.Update)
            {
                if (!context.IsDestroying)
                {
                    TryDoTask(ref context);
                }
            }
#endif
            return 0;
        }

        protected void TryDoTask(ref AssetBundleContext context)
        {
#if UNITY_EDITOR
            for (int i = 0; i < context.Tasks.Count; i++)
            {
                var task = context.Tasks[i];
                if(task.IsLoad())
                {
                    Load(ref context, ref task);
                }
                else if(task.IsUnLoad())
                {
                    UnLoad(ref context, ref task);
                }
                else
                {
                    AssetBundleLoadManager.mIns.ReomveTask(task.TaskId);
                }

                context.Tasks[i] = task;
            }
#endif
        }

        void AddException(ref AssetBundleContext context, string assetpath)
        {
            context.Cache.UpdateAssetBundleStatus(assetpath, AssetBundleStatus.LoadException, true);
            context.Cache.AddExceptionRes(assetpath);
        }

        void Load(ref AssetBundleContext context, ref AssetBundleTask task)
        {
#if UNITY_EDITOR
            GameAssetBundle gameAssetBundle;
            if (context.Cache.GetAssetBundle(task.AssetBundleName, out gameAssetBundle))
            {
                if (task.AssetInfo.AssetResType == AssetBundleResType.Scene)
                {
                    if (task.IsAsync())
                    {
                        if (gameAssetBundle.SceneRequest == null)//准备加载场景
                        {
                            gameAssetBundle.SceneRequest = SceneManager.LoadSceneAsync(task.AssetPath, task.LoadSceneMode);
                            if (gameAssetBundle.SceneRequest != null)
                            {
                                gameAssetBundle.SceneRequest.allowSceneActivation = false;
                            }

                            if (task.Result.ProgresCallback != null)
                            {
                                ProgressArgs progressArgs = new ProgressArgs(0, 0, task.AssetPath, true);
                                task.Result.ProgresCallback(ref progressArgs);
                            }
                            context.Cache.UpdateAssetBundle(task.AssetBundleName, ref gameAssetBundle);
                        }
                        else if (gameAssetBundle.SceneRequest != null)//检查加载场景的状态
                        {
                            if (gameAssetBundle.SceneRequest.progress.SimpleEqual(0.9f))
                            {

                                task.Result.scene = gameAssetBundle.SceneRequest;

                                gameAssetBundle.SceneRequest = null;

                                if (task.Result.ProgresCallback != null)
                                {
                                    ProgressArgs progressArgs = new ProgressArgs(1, 0, task.AssetPath, true);
                                    task.Result.ProgresCallback(ref progressArgs);
                                }
                                task.FinishTime = Time.realtimeSinceStartup;
                                context.Cache.UpdateAssetBundle(task.AssetBundleName, ref gameAssetBundle);
                            }
                        }
                    }
                    else
                    {
                        SceneManager.LoadScene(task.AssetPath, task.LoadSceneMode);

                        if (task.Result.ProgresCallback != null)
                        {
                            ProgressArgs progressArgs = new ProgressArgs(1, 0, task.AssetPath, true);
                            task.Result.ProgresCallback(ref progressArgs);
                        }
                        task.FinishTime = Time.realtimeSinceStartup;
                    }
                }
                else
                {
                    if (!gameAssetBundle.ContainsAsset(task.AssetPath))
                    {
                        var unityasset = AssetDatabase.LoadAssetAtPath<Object>(task.AssetPath);
                        if (unityasset != null)
                        {
                            UpdateMaterial(unityasset);
                            //update
                            gameAssetBundle.AddAsset(task.AssetPath, unityasset);
                            context.Cache.UpdateAssetBundle(task.AssetBundleName, ref gameAssetBundle);
                        }
                        else
                        {
                            Debug.LogErrorFormat("not found asset :{0}", task.AssetPath);
                            AddException(ref context, task.AssetPath);
                        }
                    }

                    task.FinishTime = Time.realtimeSinceStartup;
                }

            }
            else
            {
                Debug.LogErrorFormat("not found gameassetbundle :{0}", task.AssetBundleName);
                AddException(ref context, task.AssetPath);
            }
#endif
        }

        void UpdateMaterial(Object unityasset)
        {
            if(unityasset is Graphic)
            {
                Graphic graphic = unityasset as Graphic;
                if(graphic.material != null)
                {
                    //clone
                    graphic.material = Object.Instantiate<Material>(graphic.material);
                }
            }
            else if(unityasset is Renderer)
            {
                Renderer render = unityasset as Renderer;
                if (render.material != null)
                {
                    //clone
                    for (int i = 0; i < render.sharedMaterials.Length; i++)
                    {
                        render.sharedMaterials[i] = Object.Instantiate<Material>(render.sharedMaterials[i]);
                    }
                }
            }
            else if(unityasset is GameObject)
            {
                var renders = (unityasset as GameObject).GetComponentsInChildren<Renderer>();
                foreach(var render in renders)
                {
                    if (render.material != null)
                    {
                        //clone
                        for (int i = 0; i < render.sharedMaterials.Length; i++)
                        {
                            render.sharedMaterials[i] = Object.Instantiate<Material>(render.sharedMaterials[i]);
                        }
                    }
                }

                var graphiclist = (unityasset as GameObject).GetComponentsInChildren<Graphic>();
                foreach (var graphic in graphiclist)
                {
                    if (graphic.material != null)
                    {
                        //clone
                        graphic.material = Object.Instantiate<Material>(graphic.material);
                    }
                }
            }
        }

        void UnLoad(ref AssetBundleContext context, ref AssetBundleTask task)
        {
            if (task.AssetInfo.AssetResType == AssetBundleResType.Scene)
            {
                if (task.IsAsync())
                {
                    GameAssetBundle gameAssetBundle;
                    if (context.Cache.GetAssetBundle(task.AssetBundleName, out gameAssetBundle))
                    {
                        if (gameAssetBundle.UnloadSceneRequest == null)
                        {
                            gameAssetBundle.UnloadSceneRequest = SceneManager.UnloadSceneAsync(task.AssetPath);
                        }
                        else if (gameAssetBundle.UnloadSceneRequest.isDone)
                        {
                            if (Mathf.Abs(gameAssetBundle.UnloadSceneRequest.progress - 0.9f) < 0.001f)
                            {

                                if (task.Result.ProgresCallback != null)
                                {
                                    ProgressArgs progressArgs = new ProgressArgs(1, 0, task.AssetPath, true);
                                    task.Result.ProgresCallback(ref progressArgs);
                                }
                                task.FinishTime = Time.realtimeSinceStartup;

                            }
                        }
                    }
                }
                else
                {
#pragma warning disable
                    if (SceneManager.UnloadScene(task.AssetPath))
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
                }
            }
        }

    }

}

