using UnityEngine;
using System.Collections.Generic;
using CommonUtils;
using UnityEngine.Networking;
using System.Collections;
using System.IO;

namespace AssetBundleSystem
{
    public class DefaultRemoteDepFile : IRemoteAssets
    {
        public bool IsDone { get; private set; }

        public bool HasError { get; private set; }

        private Dictionary<IgnoreCaseString,string> _dict;

        public bool Contains(IgnoreCaseString key)
        {
            if(_dict == null)
            {
                return false;
            }

            return _dict.ContainsKey(key);
        }

        public bool TryGetRemoteUrl(IgnoreCaseString key, out string result)
        {
            if(_dict != null && _dict.TryGetValue(key,out result))
            {
                return true;
            }
            result = null;
            return false;
        }

        public IEnumerator Init(string url)
        {
            using (var request = UnityWebRequest.Get(url+"/"+AssetBundleConfig.DepFileName))
            {
                yield return request.Send();
  
                if(request.responseCode != 200)
                {
                    Debug.LogErrorFormat("Http Error :{0} Info: {1}",request.url,request.downloadHandler.text);
                }
                else if(!string.IsNullOrEmpty(request.error))
                {
                    Debug.LogErrorFormat("Error :{0} Info: {1}", request.error, request.downloadHandler.text);
                }
                else
                {
                    string perpath = AssetBundleHelper.GetBundlePersistentPath(AssetBundleConfig.DepFileName);
                    string dirname = Path.GetDirectoryName(perpath);
                    if(!Directory.Exists(dirname))
                    {
                        Directory.CreateDirectory(dirname);
                    }

                    File.WriteAllBytes(perpath, request.downloadHandler.data);

                    var getter = AssetBundleTypeGetter.GetDepReader();

                    List<AssetBundleInfo> list = new List<AssetBundleInfo>();
                    getter.ReadBytes(list,request.downloadHandler.data);

                    if (_dict == null)
                        _dict = new Dictionary<IgnoreCaseString, string>();

                    foreach(var element in list)
                    {
                        IgnoreCaseString key = new IgnoreCaseString(element.AssetBundleName);
                        _dict[key] = url + "/" + element.AssetBundleName;
                    }
                }

                IsDone = true;

            }
        }
    }
}
