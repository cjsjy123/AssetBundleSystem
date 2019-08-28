using UnityEngine;
using System.Collections.Generic;
using CommonUtils;

namespace AssetBundleSystem
{
    internal struct AssetBundleCache
    {
        /// <summary>
        /// assetbundle信息字典
        /// </summary>
        private Dictionary<IgnoreCaseString, AssetBundleInfo> _assetBundleInfoDict;
        /// <summary>
        /// 异常资源字典
        /// </summary>
        private HashSet<IgnoreCaseString> _exceptionHashSet;
        /// <summary>
        /// 运行时的assetbundle 字典
        /// </summary>
        private Dictionary<IgnoreCaseString, GameAssetBundle> _assetbundleDictionary;
        /// <summary>
        /// assetbundle 包含了哪些asset
        /// </summary>
        private Dictionary<string, List<string>> _assetbundleincludes;
        /// <summary>
        /// 远程资源信息
        /// </summary>
        private IRemoteAssets _remoteinfo;

        public AssetBundleCache(int capacity)
        {
            _assetBundleInfoDict = DictionaryPool<IgnoreCaseString, AssetBundleInfo>.Get();
            _assetbundleDictionary = DictionaryPool<IgnoreCaseString, GameAssetBundle>.Get();
            _exceptionHashSet =HashSetPool<IgnoreCaseString>.Get();
            _assetbundleincludes = DictionaryPool<string, List<string>>.Get();
            _remoteinfo = null;
        }

        public void Dispose()
        {
            if (_assetBundleInfoDict != null)
            {
                DictionaryPool<IgnoreCaseString, AssetBundleInfo>.Release(_assetBundleInfoDict);
                _assetBundleInfoDict = null;
            }

            if (_assetbundleDictionary != null)
            {
                DictionaryPool<IgnoreCaseString, GameAssetBundle>.Release(_assetbundleDictionary);
                _assetbundleDictionary = null;
            }

            if (_assetbundleincludes != null)
            {
                foreach(var item in _assetbundleincludes)
                {
                    ListPool<string>.Release(item.Value);
                }

                DictionaryPool<string, List<string>>.Release(_assetbundleincludes);
                _assetbundleincludes = null;
            }


            if (_exceptionHashSet != null)
            {
                HashSetPool<IgnoreCaseString>.Release(_exceptionHashSet);
                _exceptionHashSet = null;
            }
            _remoteinfo = null;
        }


        public bool CanLoad(string key, out AssetBundleInfo info)
        {
            return CanLoad(new IgnoreCaseString(key), out info);
        }

        public bool CanLoad(IgnoreCaseString key,out AssetBundleInfo info)
        {
            if (!string.IsNullOrEmpty(key.Info) && !IsExceptionRes(key) && TryGetInfo(key,out info))
            {
                return true;
            }
            info = default(AssetBundleInfo);
            return false;
        }

        public List<string> GetPathsInSceneAsset(string key)
        {
            List<string> list;
            if(_assetbundleincludes != null && _assetbundleincludes.TryGetValue(key,out list))
            {
                return list;
            }
            return null;
        }

        public void InitRemote(IRemoteAssets remoteAssets)
        {
            _remoteinfo = remoteAssets;
        }

        public bool TryGetDownloadUrl(IgnoreCaseString key,out string result)
        {
            if(_remoteinfo != null)
            {
                return _remoteinfo.TryGetRemoteUrl(key, out result);
            }
            result = null;
            return false;
        }

        #region Basic Info
        public bool TryGetInfo(string key, out AssetBundleInfo info)
        {
            return TryGetInfo(new IgnoreCaseString(key), out info);
        }

        public bool TryGetInfo(IgnoreCaseString key, out AssetBundleInfo info)
        {
            if (_assetBundleInfoDict != null && _assetBundleInfoDict.TryGetValue(key, out info))
            {
                return true;
            }
            info = default(AssetBundleInfo);
            return false;
        }

        #endregion
        #region Exception
        public bool AddExceptionRes(string key)
        {
            return AddExceptionRes(new IgnoreCaseString(key));
        }

        public bool AddExceptionRes(IgnoreCaseString key)
        {
            return _exceptionHashSet.Add(key);
        }

        public bool IsExceptionRes(string key)
        {
            return IsExceptionRes(new IgnoreCaseString(key));
        }

        public bool IsExceptionRes(IgnoreCaseString key)
        {
            if (!string.IsNullOrEmpty(key.Info) && _exceptionHashSet != null )
            {
                return _exceptionHashSet.Contains(key);
            }
            return false;
        }
        #endregion

        #region GameAssetBundle

        internal Dictionary<IgnoreCaseString, GameAssetBundle> GetAllAssetBundle()
        {
            return _assetbundleDictionary;
        }

        public int AssetBundleCount()
        {
            return _assetbundleDictionary == null ? 0 : _assetbundleDictionary.Count;
        }

        public bool UpdateAssetBundle(string assetbundlename, ref GameAssetBundle ab)
        {
            return UpdateAssetBundle(new IgnoreCaseString(assetbundlename),ref ab);
        }

        public bool UpdateAssetBundle(IgnoreCaseString assetbundlename, ref GameAssetBundle ab)
        {
            if (_assetbundleDictionary != null)
            {
                _assetbundleDictionary[assetbundlename] = ab;
                return true;
            }
            return false;
        }

        public bool RemoveAssetBundle(string key)
        {
            return RemoveAssetBundle(new IgnoreCaseString(key));
        }

        public bool RemoveAssetBundle(IgnoreCaseString key)
        {
            if (_assetbundleDictionary != null)
            {
                return _assetbundleDictionary.Remove(key);
            }
            return false;
        }

        public bool GetAssetBundle(string key, out GameAssetBundle ab)
        {
            return GetAssetBundle(new IgnoreCaseString(key), out ab);
        }

        public bool GetAssetBundle(IgnoreCaseString key, out GameAssetBundle ab)
        {
            if (_assetbundleDictionary != null && _assetbundleDictionary.TryGetValue(key, out ab))
            {
                return true;
            }
            ab = default(GameAssetBundle);
            return false;
        }

        public AssetBundleStatus GetAssetBundleStatus(string key)
        {
            return GetAssetBundleStatus(new IgnoreCaseString(key));
        }

        public AssetBundleStatus GetAssetBundleStatus(IgnoreCaseString key)
        {
            GameAssetBundle ab;
            if (GetAssetBundle(key, out ab))
            {
                return ab.AssetStatus;
            }
            return AssetBundleStatus.None;
        }

        public void UpdateAssetBundleStatus(string key, AssetBundleStatus status, bool and = false)
        {
            UpdateAssetBundleStatus(new IgnoreCaseString(key), status, and);
        }

        public void UpdateAssetBundleStatus(IgnoreCaseString key,  AssetBundleStatus status,bool and =false)
        {
            GameAssetBundle ab;
            if(GetAssetBundle(key,out ab))
            {
                if (and)
                {
                    ab.AssetStatus |= status;
                }
                else
                {
                    ab.AssetStatus = status;
                }

                UpdateAssetBundle(key, ref ab);
            }
        }

        #endregion

        Dictionary<T,U> InitCollection<T,U>(Dictionary<T,U> container) 
        {
            if(container == null)
            {
                container = DictionaryPool<T, U>.Get();
            }
            else
            {
                container.Clear();
            }
            return container;
        }

        HashSet<T> InitCollection<T>(HashSet<T> container)
        {
            if (container == null)
            {
                container = HashSetPool<T>.Get();
            }
            else
            {
                container.Clear();
            }
            return container;
        }

        public void Load(List<AssetBundleInfo> assetbundlelist)
        {
            _assetbundleDictionary = InitCollection(_assetbundleDictionary);
            _exceptionHashSet = InitCollection(_exceptionHashSet);
            _assetBundleInfoDict = InitCollection(_assetBundleInfoDict);
            _assetbundleincludes = InitCollection(_assetbundleincludes);

            if (assetbundlelist != null)
            {
                foreach (var assetbundleinfo in assetbundlelist)
                {
                    AssetBundleInfo info;
                    IgnoreCaseString key = new IgnoreCaseString(assetbundleinfo.UnityPath);

                    if (_assetBundleInfoDict.TryGetValue(key, out info))
                    {
                        Debug.LogErrorFormat("duplicate assetbundle info : {0}",assetbundleinfo);
                    }
                    else
                    {
                        _assetBundleInfoDict.Add(key, assetbundleinfo);

                        List<string> list;
                        if (_assetbundleincludes.TryGetValue(assetbundleinfo.AssetBundleName, out list))
                        {
                            list.Add(assetbundleinfo.UnityPath);
                        }
                        else
                        {
                            list = ListPool<string>.Get();
                            list.Add(assetbundleinfo.UnityPath);
                            _assetbundleincludes.Add(assetbundleinfo.AssetBundleName, list);
                        }
                    }
                }
            }
        }
    }

}

