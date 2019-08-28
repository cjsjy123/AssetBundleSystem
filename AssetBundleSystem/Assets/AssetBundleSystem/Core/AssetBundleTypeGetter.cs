using UnityEngine;
using System.Collections;

namespace AssetBundleSystem
{
    internal static class AssetBundleTypeGetter
    {

        public static IParseDependencyReader GetDepReader()
        {
            return new DefaultParseDepReader();
        }

        public static IRemoteAssets GetRemoteAssets()
        {
            return new DefaultRemoteDepFile();
        }

        public static IDownloader GetDownloader()
        {
            return new DefaultDownLoader();
        }
    }
}
