#if UNITY_EDITOR
using UnityEngine;
using System.Collections;

namespace AssetBundleSystem.Editor
{
    internal class BuildLogic : ILogicInterface
    {
        public AssetWindow.WindowMode TypeMode
        {
            get { return AssetWindow.WindowMode.Build; }
        }

        public bool Inited { get; private set; }

        public void Clear()
        {
            Inited = false;
        }

        public IEnumerator ReLoad()
        {
            AssetLogic assetLogic = AssetTreeManager.mIns.Pick<AssetLogic>();
            if (assetLogic != null && !assetLogic.Inited)
            {
                yield return assetLogic.ReLoad();
            }

            Inited = true;
        }
    }
}
#endif

