using System.Collections.Generic;
using CommonUtils;
using UnityEngine;
using UnityEngine.SceneManagement;


namespace AssetBundleSystem
{
    class DefaultStrategy:IStrategy<AssetBundleContext,KeyValuePair<IgnoreCaseString,GameAssetBundle>,bool>
    {

        public void Run(ref AssetBundleContext context, AssetEvent eEvent)
        {
            bool force = eEvent == AssetEvent.Destroy;
            //简单点，暂时不准备做更精细的task 依赖比较，可能会cost 更多的时间，但是很多时候并不需要这么精确
            bool inLoading = context.Tasks == null ? false : context.Tasks.Count > 0;

            if (!inLoading || force)
            {
                var result = Record(ref context, ref force, context.Cache.GetAllAssetBundle());

                if (result != null && result.Count > 0)
                {
                    UnLoad(result, ref context);
                }
            }
        }

        public HashSet<KeyValuePair<IgnoreCaseString, GameAssetBundle>> Record( ref AssetBundleContext data, ref bool force, Dictionary<IgnoreCaseString, GameAssetBundle> targets)
        {
            HashSet<KeyValuePair<IgnoreCaseString, GameAssetBundle>> result = null;
            foreach (var pair in targets)
            {
                var gameAssetBundle = pair.Value;
                bool inmemory = gameAssetBundle.AssetStatus.HasEnum(AssetBundleStatus.InMemory);
                bool canUnload = gameAssetBundle.CanUnLoad;
                bool nointask = !InTask(ref data, ref gameAssetBundle);
                bool noinscene = !InScene(ref data, ref gameAssetBundle);
                bool hasactivescene = HasShowScene();

                if (force ||
                    (inmemory && canUnload && nointask && noinscene && hasactivescene))
                {
                    if (result == null)
                        result = HashSetPool<KeyValuePair<IgnoreCaseString, GameAssetBundle>>.Get();

                    KeyValuePair<IgnoreCaseString, GameAssetBundle> tuple = new KeyValuePair<IgnoreCaseString, GameAssetBundle>(pair.Key, gameAssetBundle);

                    result.Add(tuple);
                }
            }
            return result;
        }

        public void UnLoad(HashSet<KeyValuePair<IgnoreCaseString, GameAssetBundle>> removeList, ref AssetBundleContext context)
        {
            foreach (var pair in removeList)
            {
                GameAssetBundle assetBundle = pair.Value;
                var key = pair.Key;
                if (assetBundle.AssetBundle != null)
                {
                    Debug.LogFormat("UnLoad {0}", assetBundle.AssetBundle);
                    //UnLoadAsset
                    var allassets = assetBundle.GetAllAssets();
                    if (allassets != null)
                    {
                        foreach (var kvAllasset in allassets)
                        {
                            var asset = kvAllasset.Value;
                            if (asset != null)
                            {
                                UnityEngine.Object.DestroyImmediate(asset, true);
                            }
                        }
                    }
                    //Unload Reference
                    if (assetBundle.References != null)
                    {
                        for (int i = 0; i < assetBundle.References.Count; i++)
                        {
                            var target = assetBundle.References[i];
                            if (target != null && target.IsAlive && target.Target != null && target.Target is UnityEngine.Object)
                            {
                                UnityEngine.Object.Destroy(target.Target as UnityEngine.Object);
                            }
                        }
                    }

                    assetBundle.AssetBundle.Unload(true);
                    assetBundle.Dispose();
                }

                context.Cache.RemoveAssetBundle(key);
            }


            if (removeList != null && removeList.Count >0)
            {
                HashSetPool<KeyValuePair<IgnoreCaseString, GameAssetBundle>>.Release(removeList);
#if UNITY_EDITOR
                Debug.LogFormat("<color=#ff0a22ff>left AssetBundle Count:{0} at :{1}</color>", context.Cache.AssetBundleCount(), Time.frameCount);
#else
                Debug.LogFormat("left AssetBundle Count:{0} at :{1}", context.Cache.AssetBundleCount(),Time.frameCount);
#endif
                context.IsDestroying = false;
            }
        }

        bool InTask(ref AssetBundleContext context, ref GameAssetBundle gameAssetBundle)
        {
            if (context.Tasks != null)
            {
                for (int i = 0; i < context.Tasks.Count; i++)
                {
                    var task = context.Tasks[i];
                    if (InTask(ref gameAssetBundle, ref task))
                    {
                        return true;
                    }
                }
            }
            return false;
        }

        bool InTask(ref GameAssetBundle gameAssetBundle, ref AssetBundleTask task)
        {
            if (task.AssetBundleName == gameAssetBundle.AssetBundleInfo.AssetBundleName)
            {
                return true;
            }

            if (task.AssetInfo.DepAssetBundleNames != null)
            {
                for (int i = 0; i < task.AssetInfo.DepAssetBundleNames.Length; i++)
                {
                    var depname = task.AssetInfo.DepAssetBundleNames[i];
                    if (gameAssetBundle.AssetBundleInfo.AssetBundleName == depname)
                    {
                        return true;
                    }
                }
            }

            return false;
        }

        bool InScene(ref AssetBundleContext context, ref GameAssetBundle gameAssetBundle)
        {
            if (gameAssetBundle.AssetBundleInfo.AssetResType == AssetBundleResType.Scene)
            {
                var allassets = context.Cache.GetPathsInSceneAsset(gameAssetBundle.AssetBundleInfo.AssetBundleName);

                if (allassets != null && allassets.Count > 0)
                {
#pragma warning disable
                    var allscenes = SceneManager.GetAllScenes();
#pragma warning restore
                    foreach (var scene in allscenes)
                    {
                        if (allassets.Contains(scene.path))
                        {
                            return true;
                        }
                    }
                }
            }

            return false;
        }

        bool HasShowScene()
        {
            var scene = SceneManager.GetActiveScene();
            return scene.IsValid() && scene.isLoaded;
        }

    }
}
