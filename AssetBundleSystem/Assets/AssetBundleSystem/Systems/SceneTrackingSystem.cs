using UnityEngine;
using System.Collections;
using UnityEngine.SceneManagement;

namespace AssetBundleSystem
{
    internal class SceneTrackingSystem : IAssetBundleSystem, IAssetBundleInitContext<AssetBundleContext>, IAssetBundleExecute<AssetBundleContext, AssetEvent>,IAssetBundleReadData
    {
        public IFeature Parent { get; set; }

        public bool Block { get; private set; }

        public int Init(ref AssetBundleContext context)
        {
            SceneManager.sceneLoaded -= Enterscene;
            SceneManager.sceneLoaded += Enterscene;

            SceneManager.sceneUnloaded -= ExitScene;
            SceneManager.sceneUnloaded += ExitScene;

            SceneManager.activeSceneChanged -= ChangeScene;
            SceneManager.activeSceneChanged += ChangeScene;
            return 0;
        }


        public int Execute(ref AssetBundleContext context, AssetEvent assetEvent)
        {
            return 0;
        }

        void ChangeScene(Scene oldscene,Scene newscene)
        {
            if(AssetBundleConfig.DebugMode.HasEnum( DebugMode.Detail))
            {
                Debug.LogFormat("Change scene From {0} to {1}, at {2}", oldscene.name, newscene.name, Time.frameCount);
            }
        }

        void Enterscene(Scene scene,LoadSceneMode md)
        {
            if (AssetBundleConfig.DebugMode.HasEnum(DebugMode.Detail))
            {
                Debug.LogFormat("Enterscene {0} by {1} at {2}", scene.name, md, Time.frameCount);
            }



        }

        void ExitScene(Scene scene)
        {
            if (AssetBundleConfig.DebugMode.HasEnum(DebugMode.Detail))
            {
                Debug.LogFormat("ExitScene {0} at {1}", scene.name, Time.frameCount);
            }

            AssetBundleFeature assetfeature = Parent as AssetBundleFeature;
            if (assetfeature != null)
            {
                var context = assetfeature.StartRead();
                AssetBundleInfo info;
                if (context.Cache.TryGetInfo(scene.path, out info))
                {
                    GameAssetBundle gameAssetBundle;
                    if (context.Cache.GetAssetBundle(info.AssetBundleName, out gameAssetBundle))
                    {
                        AssetBundleFunction.RemoveRef(ref context, ref gameAssetBundle);
                        assetfeature.EndRead(ref context);
                    }
                }

            }
            else
            {
                Debug.LogError("assetfeature is Null");
            }
        }

    }
}


