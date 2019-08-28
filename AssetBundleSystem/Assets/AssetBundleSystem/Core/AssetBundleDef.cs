using System;
using System.Collections;
using System.Collections.Generic;
using CommonUtils;

namespace AssetBundleSystem
{
    public enum AssetEvent
    {
        Now,
        Update,
        LateUpdate,
        Destroy,
        DrawGizmos,
    }

    public enum AssetBundleResType
    {
        None,
        Font,
        Image,
        Sound,
        Scene,
        AnimationControl,
        Animation,
        Bytes,
        Material,
        Prefab,
        RenderTexture,
        Text,
        Shader,
        Fbx,
        CustomAsset,
        Cs,
        GameObject,
        Sprite,
        Folder,
        Unknown,
    }

    [System.Flags]
    public enum AssetBundleTaskType
    {
        /// <summary>
        /// 加载
        /// </summary>
        LoadAsset = 1,

        /// <summary>
        /// 卸载
        /// </summary>
        UnLoadAsset = 2,

        /// <summary>
        /// 异步
        /// </summary>
        Async = 4,
        /// <summary>
        /// 同步
        /// </summary>
        Sync = 8,
        /// <summary>
        /// 预加载ab
        /// </summary>
        PreLoadAsset = 16,

    }

    public enum TaskResType
    {
        Common,
        GameObject,
        Scene,
    }

    [Flags]
    public enum AssetBundleStatus
    {
        None,
        /// <summary>
        /// 正在加载中
        /// </summary>
        Loading = 1,
        /// <summary>
        /// 加载时候异常的资源
        /// </summary>
        LoadException = 2,
        /// <summary>
        /// 文件不存在(需要下载)
        /// </summary>
        FileNotExist = 4,
        /// <summary>
        /// 需要下载
        /// </summary>
        DownLoading = 8,
        /// <summary>
        /// 在内存中
        /// </summary>
        InMemory = 16,
        /// <summary>
        /// 不在内存中
        /// </summary>
        NotInMemory = 32,
    }

    public enum UnLoadStrategy
    {
        Default,
        Memory,
        FramesPerSecond,
    }
}