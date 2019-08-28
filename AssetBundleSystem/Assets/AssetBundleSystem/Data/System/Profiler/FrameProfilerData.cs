using System;
using UnityEngine;
using System.Collections.Generic;
using CommonUtils;

namespace AssetBundleSystem
{
    internal struct FrameProfilerData:IEquatable<FrameProfilerData>
    {
        public long Frame;

        public long TotalAssetBundleSize;

        public long TotalLoadedSize;

        public int RefCount;

        public int UserRefCount;

        public string Scene;

        public List<AssetBundleTask> Tasks;

        public List<AssetBundleHistory> Histories;

        public int TaskCount
        {
            get
            {
                return Tasks == null ? 0 : Tasks.Count;
            }
        }

        public int HistoryCount
        {
            get
            {
                return Histories == null ? 0 : Histories.Count;
            }
        }

        public void AddCopyTask(ref AssetBundleTask task)
        {
            if(Tasks == null)
            {
                Tasks = ListPool<AssetBundleTask>.Get();
            }

            Tasks.Add(task);
        }

        public void AddHistory(ref AssetBundleHistory history)
        {
            if(Histories == null)
            {
                Histories = ListPool<AssetBundleHistory>.Get();
            }

            Histories.Add(history);

            UserRefCount += history.UserRefCount;
            RefCount += history.ObjectReferences == null ? 0 : history.ObjectReferences.Count;
        }

        public void Dispose()
        {
            if(Tasks != null)
            {
                //dont dispose
                ListPool<AssetBundleTask>.Release(Tasks);
                Tasks = null;
            }

            if(Histories != null)
            {
                foreach(var item in Histories)
                {
                    item.Dispose();
                }
                ListPool<AssetBundleHistory>.Release(Histories);
                Histories = null;
            }
        }

        public bool Equals(FrameProfilerData other)
        {
            return Frame == other.Frame && TotalAssetBundleSize == other.TotalAssetBundleSize && TotalLoadedSize == other.TotalLoadedSize && RefCount == other.RefCount && UserRefCount == other.UserRefCount && string.Equals(Scene, other.Scene) && Equals(Tasks, other.Tasks) && Equals(Histories, other.Histories);
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            return obj is FrameProfilerData && Equals((FrameProfilerData) obj);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                var hashCode = Frame.GetHashCode();
                hashCode = (hashCode * 397) ^ TotalAssetBundleSize.GetHashCode();
                hashCode = (hashCode * 397) ^ TotalLoadedSize.GetHashCode();
                hashCode = (hashCode * 397) ^ RefCount;
                hashCode = (hashCode * 397) ^ UserRefCount;
                hashCode = (hashCode * 397) ^ (Scene != null ? Scene.GetHashCode() : 0);
                hashCode = (hashCode * 397) ^ (Tasks != null ? Tasks.GetHashCode() : 0);
                hashCode = (hashCode * 397) ^ (Histories != null ? Histories.GetHashCode() : 0);
                return hashCode;
            }
        }
    }
}
