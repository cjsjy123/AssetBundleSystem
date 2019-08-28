using System;
using System.Collections.Generic;
using System.Text;
using CommonUtils;
using UnityEngine;
using UnityEngine.Profiling;
using Object = UnityEngine.Object;

namespace AssetBundleSystem
{
    internal struct AssetBundleHistory:IEquatable<AssetBundleHistory>
    {
        public struct AssetInfo:IEquatable<AssetInfo>
        {
            public string Path;

            public long Size;

            public void ToDetail(StringBuilder stringBuilder,string header ="")
            {
                if (stringBuilder != null)
                {
                    stringBuilder.AppendFormat("{0}AssetInfo Infomation:{{\n",header);
                    stringBuilder.AppendFormat("\t{0}AssetInfo Path:{1}\n", header, this.Path);
                    stringBuilder.AppendFormat("\t{0}AssetInfo Size:{1}\n", header,this.Size);

                    stringBuilder.AppendFormat("{0}}}\n",header);
                }

            }

            public override string ToString()
            {
                return string.Format("AssetInfo<Path:{0}>", Path);
            }

            public bool Equals(AssetInfo other)
            {
                return string.Equals(Path, other.Path) && Size == other.Size;
            }

            public override bool Equals(object obj)
            {
                if (ReferenceEquals(null, obj)) return false;
                return obj is AssetInfo && Equals((AssetInfo) obj);
            }

            public override int GetHashCode()
            {
                unchecked
                {
                    return ((Path != null ? Path.GetHashCode() : 0) * 397) ^ Size.GetHashCode();
                }
            }
        }

        public string AssetBundlename;

        public long LoadedSize;

        public long AssetBundleSize;

        public int UserRefCount;

        public AssetBundleStatus Status;

        public List<AssetInfo> AssetsList;

        public List<string> ObjectReferences;

        public List<UserRefHistory> RefHistories;

        public int AssetCount
        {
            get
            {
                return AssetsList == null ? 0 : AssetsList.Count;
            }
        }

        public int ObjRefCount
        {
            get
            {
                return ObjectReferences == null ? 0 : ObjectReferences.Count;
            }
        }

        public int HistroyCount
        {
            get
            {
                return RefHistories == null ? 0 : RefHistories.Count;
            }
        }

        public override string ToString()
        {
            return string.Format("AB History<Name:{0}>",AssetBundlename);
        }

        public void AddObjectRef(ref GameAssetBundle gameAssetBundle)
        {
            if(ObjectReferences == null)
            {
                ObjectReferences = ListPool<string>.Get();
            }

            if(RefHistories == null)
            {
                RefHistories = ListPool<UserRefHistory>.Get();
            }

            if(AssetsList == null)
            {
                AssetsList = ListPool<AssetInfo>.Get();
            }

            AssetBundlename = gameAssetBundle.AssetBundleInfo.AssetBundleName;
            AssetBundleSize = gameAssetBundle.AssetBundleInfo.AssetSize;
            Status = gameAssetBundle.AssetStatus;
            UserRefCount = gameAssetBundle.UserReference;

            if (gameAssetBundle.References != null)
            {
                foreach(var weakobj in gameAssetBundle.References)
                {
                    if(weakobj != null && weakobj.IsAlive)
                    {
                        ObjectReferences.Add(GetPath(weakobj.Target as Object));
                    }
                }
            }

            if (gameAssetBundle.loadedAssets != null)
            {
                foreach (var item in gameAssetBundle.loadedAssets)
                {
                    var asset = item.Value;
                    if (asset != null)
                    {
                        long size = Profiler.GetRuntimeMemorySizeLong(asset);
                        LoadedSize += size;

                        AssetInfo info = new AssetInfo();
                        info.Size = size;
                        info.Path = GetPath(asset);

                        AssetsList.Add(info);
                    }
                }
            }

#if UNITY_EDITOR

            if (gameAssetBundle.history != null)
            {
                foreach(var historydata  in gameAssetBundle.history)
                {
                    RefHistories.Add(historydata);
                }
            }
#endif
        }

        internal string GetPath(Object target)
        {
            if(target != null)
            {
                string result = target.name;
                Transform transform = null;
                if(target is Component)
                {
                    transform = (target as Component).transform;
                }
                else if(target is GameObject)
                {
                    transform = (target as GameObject).transform;
                }

                if(transform != null)
                {
                    while(transform.parent != null)
                    {
                        transform = transform.parent;
                        result = transform.name + "/" + result;
                    }
                }
                return result;
            }
            return "<Empty>";
        }

        public void Dispose()
        {
            if(ObjectReferences != null)
            {
                ListPool<string>.Release(ObjectReferences);
                ObjectReferences = null;
            }

            if(RefHistories !=null)
            {
                ListPool<UserRefHistory>.Release(RefHistories);
                RefHistories = null;
            }

            if(AssetsList != null)
            {
                ListPool<AssetInfo>.Release(AssetsList);
                AssetsList = null;
            }
        }

        public void ToDetail(StringBuilder stringBuilder)
        {
            if (stringBuilder != null)
            {

                stringBuilder.AppendLine("History Infomation:{");
                stringBuilder.AppendFormat("\tAssetBundlename :{0}\n", this.AssetBundlename);
                stringBuilder.AppendFormat("\tLoadedSize :{0}\n", this.LoadedSize);
                stringBuilder.AppendFormat("\tAssetBundleSize :{0}\n", this.AssetBundleSize);
                stringBuilder.AppendFormat("\tUserRefCount :{0}\n", this.UserRefCount);
                stringBuilder.AppendFormat("\tStatus :{0}\n", this.Status);

                stringBuilder.AppendFormat("\tAssetsList Count :{0}\n", AssetCount);

                for (int i = 0; i < AssetCount; i++)
                {
                    var assetinfo = AssetsList[i];
                    assetinfo.ToDetail(stringBuilder, string.Format("\t[{0}]",i));
                }

                stringBuilder.AppendFormat("\tObjectReferences Count :{0}\n", ObjRefCount);

                for (int i = 0; i < ObjRefCount; i++)
                {
                    var refstring = ObjectReferences[i];
                    stringBuilder.AppendFormat("\t\t[{0}]ObjRef Path:{1}\n", i, refstring);
                }

                for (int i = 0; i < HistroyCount; i++)
                {
                    var history = RefHistories[i];
                    history.ToDetail(stringBuilder, string.Format("\t[{0}]", i));
                }

                stringBuilder.AppendLine("}");

            }
        }

        public bool Equals(AssetBundleHistory other)
        {
            return string.Equals(AssetBundlename, other.AssetBundlename) && LoadedSize == other.LoadedSize && AssetBundleSize == other.AssetBundleSize && UserRefCount == other.UserRefCount && Status == other.Status && Equals(AssetsList, other.AssetsList) && Equals(ObjectReferences, other.ObjectReferences) && Equals(RefHistories, other.RefHistories);
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            return obj is AssetBundleHistory && Equals((AssetBundleHistory) obj);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                var hashCode = (AssetBundlename != null ? AssetBundlename.GetHashCode() : 0);
                hashCode = (hashCode * 397) ^ LoadedSize.GetHashCode();
                hashCode = (hashCode * 397) ^ AssetBundleSize.GetHashCode();
                hashCode = (hashCode * 397) ^ UserRefCount;
                hashCode = (hashCode * 397) ^ (int) Status;
                hashCode = (hashCode * 397) ^ (AssetsList != null ? AssetsList.GetHashCode() : 0);
                hashCode = (hashCode * 397) ^ (ObjectReferences != null ? ObjectReferences.GetHashCode() : 0);
                hashCode = (hashCode * 397) ^ (RefHistories != null ? RefHistories.GetHashCode() : 0);
                return hashCode;
            }
        }
    }
}
