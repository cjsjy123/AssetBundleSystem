using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;
using CommonUtils;
using Object = UnityEngine.Object;

namespace AssetBundleSystem
{

    internal struct GameAssetBundle:IEquatable<GameAssetBundle>
    {
        /// <summary>
        /// AssetBundle
        /// </summary>
        public AssetBundle AssetBundle;

        /// <summary>
        /// assetbundle 信息 备份 (assetbundle 中其中一份 的资源，不要当做全部)
        /// </summary>
        public AssetBundleInfo AssetBundleInfo;

        /// <summary>
        /// 已经加载内存中的Assets  Key = AssetPath
        /// </summary>
        internal Dictionary<string,Object> loadedAssets;
        /// <summary>
        /// 异步请求
        /// </summary>
        public AssetBundleCreateRequest LoadRequest;
        /// <summary>
        /// Load asset
        /// </summary>
        public AssetBundleRequest LoadAssetRequest;
        /// <summary>
        /// The load scene request.
        /// </summary>
        public AsyncOperation SceneRequest;
        /// <summary>
        /// The unload scene request.
        /// </summary>
        public AsyncOperation UnloadSceneRequest;

        private AssetBundleStatus _status;
        /// <summary>
        /// 资源状态
        /// </summary>
        public AssetBundleStatus AssetStatus
        {
            get
            {
                return _status;
            }
            set
            {
                _status = value;
            }
        }

#if UNITY_EDITOR
        internal int offset;
        internal UserRefHistory[] history;
#endif

        private int _userReference;

        /// <summary>
        /// 用户自定义的附加计数
        /// </summary>
        public int UserReference
        {
            get { return _userReference ; }
        }
        /// <summary>
        /// 弱引用集合
        /// </summary>
        internal List<WeakReference> References;
        /// <summary>
        /// 实例对象引用
        /// </summary>
        public int ReferenceCnt
        {
            get
            {
                if (References == null)
                {
                    return 0;
                }
                UpdateReference();
                return References.Count;
            }
        }

        public bool CanUnLoad
        {
            get { return ReferenceCnt == 0 && UserReference == 0 ; }
        }

        public void Dispose()
        {
            if (References != null)
            {
                ListPool<WeakReference>.Release(References);
                References = null;
            }
            loadedAssets = null;
            AssetBundle = null;
        }


        internal Dictionary<string, Object> GetAllAssets()
        {
            return loadedAssets;
        }

        internal void AddAsset(string assetname,Object asset)
        {
            if (loadedAssets == null)
                loadedAssets = new Dictionary<string, Object>();

            Object cacheAsset;
            if (!loadedAssets.TryGetValue(assetname, out cacheAsset))
            {
                loadedAssets.Add(assetname,asset);
            }
            else
            {
                Debug.LogErrorFormat("contains this asset: {0}",asset);
            }
        }

        public bool ContainsAsset(string assetname)
        {
            if (loadedAssets != null)
            {
                return loadedAssets.ContainsKey(assetname);
            }
            return false;
        }

        public Object LoadAsset(string assetname)
        {
            if(loadedAssets == null)
                loadedAssets = new Dictionary<string, Object>();

            Object result = null;
            if (loadedAssets.TryGetValue(assetname, out result))
            {
                return result;
            }
            else
            {
                return null;
            }
        }
#pragma warning disable
        public void AddUserRef(string rootpath)
        {
            _userReference ++;
#if UNITY_EDITOR
            if(history == null)
            {
                history = new UserRefHistory[64];
            }

            UserRefHistory data = new UserRefHistory();
            data.AddTime = Time.realtimeSinceStartup;
            data.AddRefTarget = rootpath;

            history[offset] = data;

            offset = (offset + 1) % history.Length;
#endif
        }
#pragma warning restore
        public void RemoveUserRef()
        {
            _userReference --;
        }

        public bool Contains(object obj)
        {
            if(References != null && obj != null)
            {
                for (int i = 0; i < References.Count; i++)
                {
                    var weak = References[i];
                    if(weak != null && weak.IsAlive && weak.Target == obj)
                    {
                        return true;
                    }
                }
            }
            return false;
        }

        public void AddReference(Object gameobject)
        {
            if (References == null)
            {
                References = ListPool<WeakReference>.Get();
                References.Add(new WeakReference(gameobject));
            }
            else
            {
                bool needAdd = true;
                for (int i = References.Count - 1; i >= 0; i--)
                {
                    var weak = References[i];
                    if (weak != null && weak.IsAlive && weak.Target.Equals(gameobject))
                    {
                        needAdd = false;
                        break;
                    }
                }

                if(needAdd)
                    References.Add(new WeakReference(gameobject));
            }
        }

        void UpdateReference()
        {
            if (References != null)
            {
                for (int i = References.Count - 1; i >= 0; i--)
                {
                    var weak = References[i];
                    if (weak == null || !weak.IsAlive || weak.Target == null)
                    {
                        References.RemoveAt(i);
                    }
                }
            }
        }

        public bool IsException()
        {
            return AssetStatus.ContainsEnum(AssetBundleStatus.FileNotExist | AssetBundleStatus.LoadException | AssetBundleStatus.NotInMemory);
        }

        public override string ToString()
        {
            return string.Format("AssetBundle :{0} Status :{1}", AssetBundle, AssetStatus);
        }

        public bool Equals(GameAssetBundle other)
        {
            return Equals(AssetBundle, other.AssetBundle) && AssetBundleInfo.Equals(other.AssetBundleInfo) && Equals(loadedAssets, other.loadedAssets) && Equals(LoadRequest, other.LoadRequest) && Equals(LoadAssetRequest, other.LoadAssetRequest) && Equals(SceneRequest, other.SceneRequest) && Equals(UnloadSceneRequest, other.UnloadSceneRequest) && _status == other._status
#if UNITY_EDITOR
                && offset == other.offset && Equals(history, other.history)
#endif
                && _userReference == other._userReference && Equals(References, other.References);
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            return obj is GameAssetBundle && Equals((GameAssetBundle) obj);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                var hashCode = (AssetBundle != null ? AssetBundle.GetHashCode() : 0);
                hashCode = (hashCode * 397) ^ AssetBundleInfo.GetHashCode();
                hashCode = (hashCode * 397) ^ (loadedAssets != null ? loadedAssets.GetHashCode() : 0);
                hashCode = (hashCode * 397) ^ (LoadRequest != null ? LoadRequest.GetHashCode() : 0);
                hashCode = (hashCode * 397) ^ (LoadAssetRequest != null ? LoadAssetRequest.GetHashCode() : 0);
                hashCode = (hashCode * 397) ^ (SceneRequest != null ? SceneRequest.GetHashCode() : 0);
                hashCode = (hashCode * 397) ^ (UnloadSceneRequest != null ? UnloadSceneRequest.GetHashCode() : 0);
                hashCode = (hashCode * 397) ^ (int) _status;
#if UNITY_EDITOR
                hashCode = (hashCode * 397) ^ offset;
                hashCode = (hashCode * 397) ^ (history != null ? history.GetHashCode() : 0);
#endif
                hashCode = (hashCode * 397) ^ _userReference;
                hashCode = (hashCode * 397) ^ (References != null ? References.GetHashCode() : 0);
                return hashCode;
            }
        }
    }

}

