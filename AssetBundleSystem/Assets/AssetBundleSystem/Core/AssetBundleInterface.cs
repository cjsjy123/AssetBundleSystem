using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using CommonUtils;

namespace AssetBundleSystem
{
    #region Systems

    public interface IFeature
    {

    }

    public interface IAssetBundleSystem
    {
        /// <summary>
        /// 阻塞后续的system
        /// </summary>
        bool Block { get; }
    }

    public interface IAssetBundleInitContext<T> where T : struct
    {
        int Init(ref T context);
    }

    public interface IAssetBundleExecute<T, E> where T : struct
    {
        int Execute(ref T context, E assetEvent);
    }

    public interface IAssetBundleReadData
    {
        IFeature Parent { get; set; }
    }

    #endregion

    internal interface IDownloader
    {
        bool IsDone { get; }

        void DownLoad(ref AssetDownloadInfo downloadInfo);
    }

    internal interface ILoadArgs
    {
        string AssetPath { get; set; }
        int Priority { get; set; }
    }

    internal interface IRemoteAssets
    {
        bool HasError { get; }
        bool IsDone { get; }

        IEnumerator Init(string url);

        bool Contains(IgnoreCaseString key);

        bool TryGetRemoteUrl(IgnoreCaseString key, out string result);
    }

    internal interface IParseDependencyReader
    {
        bool IsAsync { get; }

        bool Read(string filepath, out List<AssetBundleInfo> assetbundlelist);

        IEnumerator ReadAsync(string filepath);

        void ReadBytes(List<AssetBundleInfo> assetbundlelist, byte[] bytes);
    }

    internal interface IParseDependencyWriter
    {
        bool Write(string filepath);
    }

    internal interface IBaseStrategy<T>
    {
        void Run(ref T data, AssetEvent eEvent);
    }

    internal interface IStrategy<T, TR, U>: IBaseStrategy<T>
    {
        HashSet<TR> Record(ref T data, ref U args, Dictionary<IgnoreCaseString, GameAssetBundle> targets);

        void UnLoad(HashSet<TR> result, ref T data);
    }
}
