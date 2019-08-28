using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace AssetBundleSystem
{
    internal class AssetBundleFeature: BaseFeature<AssetBundleContext, IAssetBundleSystem,AssetEvent>
    {
        protected override void AddAssetSystems()
        {
            if(AssetBundleConfig.IsSimulator())
            {
                foreach (var systype in AssetBundleConfig.SimulatorLoadSystems)
                {
                    IAssetBundleSystem sys = System.Activator.CreateInstance(systype) as IAssetBundleSystem;
                    if (sys != null)
                        Systems.Add(sys);
                }
            }
            else
            {
                foreach (var systype in AssetBundleConfig.LoadSystems)
                {
                    IAssetBundleSystem sys = System.Activator.CreateInstance(systype) as IAssetBundleSystem;
                    if (sys != null)
                        Systems.Add(sys);
                }
            }

        }

        public override void ReLoad()
        {
            if(InitSuccess)
            {
                this.Execute(AssetEvent.Destroy);
                Context.Dispose();
                InitSystems();
                _InitSuccess = false;
            }
        }

        protected override bool InitSystems()
        {
            Context = new AssetBundleContext(AssetBundleConfig.Capacity);

            bool success = true;
            for (int i = 0; i < Systems.Count; i++)
            {
                var sys = Systems[i];
                TryCallInit(sys, ref Context);
                _initstep++;
                if (sys.Block)
                {
                    success = false;
                    break;
                }
            }
            return success;
        }

        public override void Execute(AssetEvent assetEvent)
        {
            if(InitSuccess)
            {
                for (int i = 0; i < Systems.Count; i++)
                {
                    var sys = Systems[i];
                    TryCallExcute(sys, ref Context, ref assetEvent);

                    if (sys.Block)
                    {
                        break;
                    }
                }
            }

            
        }
    }

}


