#if UNITY_EDITOR
using System.Collections.Generic;

namespace AssetBundleSystem.Editor
{
    internal struct EditorSystemContext
    {
        public Dictionary<string, AssetBundleBuildInfo> Manifests;

    }

}
#endif

