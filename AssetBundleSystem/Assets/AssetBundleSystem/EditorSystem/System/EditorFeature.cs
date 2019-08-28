#if UNITY_EDITOR
using UnityEngine;
using System.Collections;

namespace AssetBundleSystem.Editor
{
    internal class EditorFeature : BaseFeature<EditorSystemContext, IAssetBundleSystem,EditorEvent>
    {

        protected override void AddAssetSystems()
        {
            foreach (var systype in AssetWindowConfig.LoadSystems)
            {
                IAssetBundleSystem sys = System.Activator.CreateInstance(systype) as IAssetBundleSystem;
                if (sys != null)
                    Systems.Add(sys);
            }
        }

        public override void ReLoad()
        {

        }

        protected override bool InitSystems()
        {
            Context = new EditorSystemContext();

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
            //copy
            EditorContexts.mIns.SystemContext = Context;
            return success;

        }

        public override void Execute(EditorEvent assetEvent)
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

                //copy
                EditorContexts.mIns.SystemContext = Context;
            }
        }
    }
}
#endif
