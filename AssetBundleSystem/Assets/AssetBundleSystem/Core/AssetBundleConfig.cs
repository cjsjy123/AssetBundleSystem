using System;
using System.Collections.Generic;

namespace AssetBundleSystem
{
    [Flags]
    public enum DebugMode
    {
        None,
        Log,
        Detail,
        ProfilerLog,
        Simulator,
    }

    public enum AssetParseConfig
    {
        Filter,
        All,
    }

    public static class AssetBundleConfig
    {
        public const int Version = 0;
#if UNITY_EDITOR
        public static string DownloadUrl = "http://127.0.0.1:8080/ab";
#elif UNITY_IPHONE || UNITY_IOS
        public static string DownloadUrl = "http://127.0.0.1:8080/ios";
#elif UNITY_ANDROID
        public static string DownloadUrl = "http://127.0.0.1:8080/android";
#else
        public static string DownloadUrl = "http://127.0.0.1:8080/ab";
#endif

        public static AssetParseConfig ParseConfig = AssetParseConfig.Filter;


        public static string AssetBundleDownloadPath = "";

        public const string DepFileName = "abdep.tql";

        public static int TaskStep = 1;

        public static long TaskLoadAssetLimit = 65536;

        public static int Capacity = 64;
        /// <summary>
        /// 遇到异常资源两种做法，一种不管直接加载，丢失就丢失， 另一种就是把使用到资源也标记为不可用 Safe为采用第二种
        /// </summary>
        public const bool SafeMode = true;
#if UNITY_EDITOR
        public static DebugMode DebugMode = DebugMode.ProfilerLog | DebugMode.Log | DebugMode.Detail;
#else
        public static DebugMode DebugMode = DebugMode.None;
#endif
        /// <summary>
        /// Unload Strategy
        /// </summary>
        public static UnLoadStrategy UnLoadStrategy = UnLoadStrategy.Default;

        public static List<Type> LoadSystems = new List<Type>()
        {
            typeof(ParseAssetInfoSystem),
            typeof(ParseRemoteSystem),
            typeof(AssetBundleDownloadSystem),
            typeof(RemoveTaskSystem),
            typeof(LoadAssetSystem),
            typeof(LoadSceneObjectSystem),
            typeof(LoadProfilerSystem),
            typeof(LoadDispatcherSystem),
            typeof(SceneTrackingSystem),
          
            typeof(AssetBundleMemorySystem)

        };

        public static List<Type> SimulatorLoadSystems = new List<Type>()
        {
            typeof(ParseAssetInfoSystem),
            typeof(RemoveTaskSystem),
            typeof(SimualtorLoadSystem),
            typeof(LoadDispatcherSystem),

        };
        /// <summary>
        /// 和FilterFiles 一一对应
        /// </summary>
        public static string[] FilterPaths = new string[]
        {
            //例子用这个路径
                    "Assets",
                    //"Assets/Res_2"
                    //项目用这个
                    //"Assets/Res/Assets/uiprefabs"
        };
        /// <summary>
        /// 支持unity的t 也可使用*.xxx   xxx.asset
        /// </summary>
        public static string[] FilterFiles = new string[]
        {
            "t:texture2d t:GameObject t:Scene t:Animation t:Material",
            //"t:texture2d t:GameObject t:Scene t:Animation t:Material"
        };


        public static bool IsFileterEnable()
        {
            return ParseConfig == AssetParseConfig.Filter && FilterPaths.Length > 0 &&
                   FilterFiles.Length >0;
        }

        /// <summary>
        /// 将assetbundle name 转化为真实的文件名
        /// </summary>
        /// <returns></returns>
        public static string Convert(string assetbundlename)
        {
            return assetbundlename;
        }

        public static bool IsDetail()
        {
            return DebugMode.HasEnum(DebugMode.Detail);
        }

        public static bool IsProfiler()
        {
            return DebugMode.HasEnum(DebugMode.ProfilerLog);
        }

        public static bool IsSimulator()
        {
            return DebugMode.HasEnum(DebugMode.Simulator);
        }
    }
}


