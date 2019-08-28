using UnityEngine;
using System.Collections;

namespace AssetBundleSystem
{
    public static class AssetSystemExtension
    {

        #region floag enums
        public static bool HasEnum(this DebugMode md, DebugMode targetmd)
        {
            return (md & targetmd) == targetmd;
        }

        public static bool ContainsEnum(this DebugMode md, DebugMode targetmd)
        {
            return (md & targetmd) != 0;
        }

        public static bool HasEnum(this AssetBundleStatus md, AssetBundleStatus targetmd)
        {
            return (md & targetmd) == targetmd;
        }

        public static bool ContainsEnum(this AssetBundleStatus md, AssetBundleStatus targetmd)
        {
            return (md & targetmd) != 0;
        }

        public static bool HasEnum(this AssetBundleTaskType md, AssetBundleTaskType targetmd)
        {
            return (md & targetmd) == targetmd;
        }

        public static bool ContainsEnum(this AssetBundleTaskType md, AssetBundleTaskType targetmd)
        {
            return (md & targetmd) != 0;
        }
        #endregion
    }
}
