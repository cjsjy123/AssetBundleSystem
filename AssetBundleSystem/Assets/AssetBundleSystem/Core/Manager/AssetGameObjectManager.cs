using UnityEngine;
using System.Collections;

namespace AssetBundleSystem
{

    public partial class AssetBundleLoadManager
    {
        #region Gameobject
        #region Async
        public LoadReturnArgs LoadGameObjectAsync(GameObjectLoadArgs args)
        {
            return CreateGameObject(ref args, AssetBundleTaskType.LoadAsset | AssetBundleTaskType.Async);
        }

        public LoadReturnArgs LoadGameObjectAsync(string assetpath, Transform parent)
        {
            return CreateGameObject(new GameObjectLoadArgs(assetpath, parent), AssetBundleTaskType.LoadAsset | AssetBundleTaskType.Async);
        }

        public LoadReturnArgs LoadGameObjectAsync(string assetpath, Transform parent, Vector3 position)
        {
            return CreateGameObject(new GameObjectLoadArgs(assetpath, parent, position, Quaternion.identity, Vector3.one), AssetBundleTaskType.LoadAsset | AssetBundleTaskType.Async);
        }

        public LoadReturnArgs LoadGameObjectAsync(string assetpath, Transform parent, Vector3 position, Quaternion rot)
        {
            return CreateGameObject(new GameObjectLoadArgs(assetpath, parent, position, rot, Vector3.one), AssetBundleTaskType.LoadAsset | AssetBundleTaskType.Async);
        }

        public LoadReturnArgs LoadGameObjectAsync(string assetpath, Transform parent, Vector3 position, Quaternion rot, Vector3 scale)
        {
            return CreateGameObject(new GameObjectLoadArgs(assetpath, parent, position, rot, scale), AssetBundleTaskType.LoadAsset | AssetBundleTaskType.Async);
        }

        public LoadReturnArgs LoadGameObjectAsync(string assetpath, Transform parent, Vector3 position, Quaternion rot, Vector3 scale, int priority)
        {
            return CreateGameObject(new GameObjectLoadArgs(assetpath, parent, position, rot, scale, priority), AssetBundleTaskType.LoadAsset | AssetBundleTaskType.Async);
        }

        #endregion

        #region Sync
        public LoadReturnArgs LoadGameObject(GameObjectLoadArgs args)
        {
            return CreateGameObject(ref args);
        }

        public LoadReturnArgs LoadGameObject(string assetpath, Transform parent)
        {
            return LoadGameObject(new GameObjectLoadArgs(assetpath, parent));
        }


        public LoadReturnArgs LoadGameObject(string assetpath, Transform parent, Vector3 position)
        {
            return LoadGameObject(new GameObjectLoadArgs(assetpath, parent, position, Quaternion.identity, Vector3.one));
        }

        public LoadReturnArgs LoadGameObject(string assetpath, Transform parent, Vector3 position, Quaternion rot)
        {
            return LoadGameObject(new GameObjectLoadArgs(assetpath, parent, position, rot, Vector3.one));
        }

        public LoadReturnArgs LoadGameObject(string assetpath, Transform parent, Vector3 position, Quaternion rot, Vector3 scale)
        {
            return  LoadGameObject(new GameObjectLoadArgs(assetpath, parent, position, rot, scale));
        }

        public LoadReturnArgs LoadGameObject(string assetpath, Transform parent, Vector3 position, Quaternion rot, Vector3 scale, int priority)
        {
            return LoadGameObject(new GameObjectLoadArgs(assetpath, parent, position, rot, scale, priority));
        }

        #endregion

        LoadReturnArgs CreateGameObject(GameObjectLoadArgs args, AssetBundleTaskType type = AssetBundleTaskType.LoadAsset | AssetBundleTaskType.Sync)
        {
            return CreateGameObject(ref args, type);
        }

        LoadReturnArgs CreateGameObject(ref GameObjectLoadArgs args, AssetBundleTaskType type = AssetBundleTaskType.LoadAsset | AssetBundleTaskType.Sync)
        {
            if (_assetBundleFeature != null)
            {
                AssetBundleContext context = _assetBundleFeature.StartRead();

                AssetBundleInfo info;
                GameAssetBundle gameAssetBundle;
                if (CanLoad(ref context,args.AssetPath,out info,out gameAssetBundle))
                {
                    AssetBundleTask task = new AssetBundleTask();
                    //
                    task.AssetInfo = info;
                    task.TaskLoadType = type;
                    task.TaskId = ++_taskid;
                    task.TaskResType = TaskResType.GameObject;
                    task.Priority = args.Priority;
                    task.CreateTime = Time.realtimeSinceStartup;

                    //info
                    task.Result.Position = args.Position;
                    task.Result.Rotation = args.Rotation;
                    task.Result.Scale = args.Scale;
                    task.Result.Parent = args.Parent;
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

    }
}


