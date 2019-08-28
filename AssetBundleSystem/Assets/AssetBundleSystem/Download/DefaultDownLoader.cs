using UnityEngine;
using System.Collections;
using UnityEngine.Networking;
using System.IO;
using CommonUtils;

namespace AssetBundleSystem
{
    internal class DefaultDownLoader : IDownloader
    {
        public bool IsDone { get; private set; }

        private UnityWebRequest _cacherequest;

        public void DownLoad(ref AssetDownloadInfo downloadInfo)
        {
            if(!downloadInfo.IsDone )
            {
                if(!downloadInfo.IsDownloading)
                {
                    if(AssetBundleConfig.IsDetail())
                    {
                        Debug.LogFormat("Start DownLoad :{0}", downloadInfo.Url);
                    }

                    GUpdater.mIns.StartCoroutine(DownLoadAssetBundle(downloadInfo.Url,downloadInfo.DstPath));
                    downloadInfo.IsDownloading = true;
                }
            }

            if(_cacherequest != null)
            {
                downloadInfo.IsDone = _cacherequest.isDone;
                downloadInfo.Progress = _cacherequest.downloadProgress;

                if(_cacherequest.isDone)
                {
                    downloadInfo.HasError = !string.IsNullOrEmpty(_cacherequest.error) || _cacherequest.responseCode != 0;
                    downloadInfo.IsDownloading = false;
                    _cacherequest.Dispose();
                    _cacherequest = null;
                }
            }
        }

        IEnumerator DownLoadAssetBundle(string url,string dstpath)
        {
            _cacherequest = UnityWebRequest.Get(url);

            yield return _cacherequest.Send();

            if (!string.IsNullOrEmpty(_cacherequest.error))
            {
                Debug.LogError(_cacherequest.error);
            }
            else if (_cacherequest.responseCode != 200)
            {
                Debug.LogErrorFormat("Error :{0} Info: {1}", _cacherequest.url, _cacherequest.downloadHandler.text);
            }
            else
            {
                var dirname = Path.GetDirectoryName(dstpath);
                if(Directory.Exists(dirname) == false)
                {
                    Directory.CreateDirectory(dirname);
                }

                File.WriteAllBytes(dstpath, _cacherequest.downloadHandler.data);

                if (AssetBundleConfig.IsDetail())
                {
                    Debug.LogFormat( "DownLoad Finish:{0}", _cacherequest.url);
                }
            }

            IsDone = true;
        }

       
    }

}

