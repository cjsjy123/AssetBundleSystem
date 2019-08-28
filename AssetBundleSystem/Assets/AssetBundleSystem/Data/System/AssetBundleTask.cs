using System;
using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using CommonUtils;
using System.Text;
using UnityEngine.SceneManagement;

namespace AssetBundleSystem
{

    internal struct AssetBundleTask:IEquatable<AssetBundleTask>
    {
        /// <summary>
        /// 任务ID
        /// </summary>
        public long TaskId;
        /// <summary>
        /// 资源信息
        /// </summary>
        public AssetBundleInfo AssetInfo;
        /// <summary>
        /// 任务类型
        /// </summary>
        public AssetBundleTaskType TaskLoadType;
        /// <summary>
        /// 任务资源类型
        /// </summary>
        public TaskResType TaskResType;
        /// <summary>
        /// 优先级
        /// </summary>
        public int Priority;
        /// <summary>
        /// 结果
        /// </summary>
        public ResultArgs Result;
        /// <summary>
        /// 子任务
        /// </summary>
        public List<AssetBundleTask> SubTasks;
        /// <summary>
        /// 大小不受限制
        /// </summary>
        public bool FreeSize;
        /// <summary>
        /// 创建时间
        /// </summary>
        public float CreateTime;

        private float _finishtime;
        /// <summary>
        /// The finish time.
        /// </summary>
        public float FinishTime
        {
            get
            {
                return _finishtime;
            }
            set
            {
                _finishtime = value;
                if(AssetBundleConfig.IsDetail())
                {
                    Debug.LogFormat("Task:{0} Finish time is :{1} cost:{2}", this.TaskId, value,value-CreateTime);
                }
            }
        }
        /// <summary>
        /// 如果pin的时间小于0 则会使用userreference的pin住，否则使用时间pin住
        /// </summary>
        public float PinTime;

        public LoadSceneMode LoadSceneMode;

        public string AssetPath
        {
            get { return AssetInfo.UnityPath; }
        }

        public string AssetBundleName
        {
            get
            {
                return AssetInfo.AssetBundleName;
            }
        }

        public override string ToString()
        {
            return string.Format("AssetBundleTask Id:{0},Path:{1},Type:{2} LoadType:{3} SubCount:{4}", TaskId,AssetPath, TaskResType,TaskLoadType,SubTasks == null?0:SubTasks.Count);
        }

        public void AddSubTask(ref AssetBundleTask task)
        {
            if (SubTasks == null)
            {
                SubTasks = ListPool<AssetBundleTask>.Get();
            }
            SubTasks.Add(task);
        }

        public void Dispose()
        {
            if (SubTasks != null)
            {
                ListPool<AssetBundleTask>.Release(SubTasks);
                SubTasks = null;
            }

            this.Result.LoadObject = null;
        }

        public bool IsAsync()
        {
            return TaskLoadType.HasEnum(AssetBundleTaskType.Async);
        }

        public bool IsLoad()
        {
            return TaskLoadType.HasEnum(AssetBundleTaskType.LoadAsset);
        }

        public bool IsUnLoad()
        {
            return TaskLoadType.HasEnum(AssetBundleTaskType.UnLoadAsset);
        }

        public bool IsPreLoad()
        {
            return TaskLoadType.HasEnum(AssetBundleTaskType.PreLoadAsset);
        }


        public bool IsStatus(ref AssetBundleContext context,AssetBundleStatus status)
        {
            GameAssetBundle assetBundle;
            return IsStatus(ref context, out assetBundle, status);
        }

        public bool IsStatus(ref AssetBundleContext context, out GameAssetBundle assetBundle,AssetBundleStatus status)
        {
            if (context.Cache.GetAssetBundle(AssetBundleName, out assetBundle))
            {
                if (assetBundle.AssetStatus.ContainsEnum(status))
                {
                    return true;
                }
                else if (SubTasks != null && SubTasks.Count > 0)
                {
                    for (int i = 0; i < SubTasks.Count; i++)
                    {
                        if (SubTasks[i].IsStatus(ref context,status))
                        {
                            return true;
                        }
                    }
                }
            }
            return false;
        }

      

        public bool IsSubAllDone( ref AssetBundleContext context,out int finishcount,out int total)
        {
            finishcount = 0;
            total = 0;
            if (SubTasks != null)
            {
                for (int i = 0; i < SubTasks.Count; i++)
                {
                    var subtask = SubTasks[i];
                    if (!SubAllDone(ref context, ref subtask,ref finishcount,ref total))
                    {
                        return false;
                    }
                }
            }
            return true;
        }

        private static bool SubAllDone(ref AssetBundleContext context,ref AssetBundleTask task,ref int finishcount,ref int total)
        {
            total++;
            if (task.SubTasks != null)
            {
                for (int i = 0; i < task.SubTasks.Count; i++)
                {
                    var subtask = task.SubTasks[i];
                    if (!SubAllDone(ref context,ref subtask,ref finishcount,ref total))
                    {
                        return false;
                    }
                }
            }

            GameAssetBundle gameAssetBundle;
            if (context.Cache.GetAssetBundle(task.AssetBundleName, out gameAssetBundle) )
            {
                if(AssetBundleConfig.SafeMode && gameAssetBundle.IsException())
                {
                    return false;
                }
                else if(gameAssetBundle.AssetStatus.HasEnum(AssetBundleStatus.InMemory))
                {
                    finishcount++;
                    return true;
                }
            }
#pragma warning disable
            else if(!AssetBundleConfig.SafeMode)
            {
                return true;
            }
#pragma warning restore
            return false;
        }

        public bool IsDone(ref AssetBundleContext context)
        {
            GameAssetBundle assetBundle;
            return IsDone(ref context,out assetBundle);
        }

        public bool IsDone(ref AssetBundleContext context,out GameAssetBundle assetBundle)
        {
            if (context.Cache.GetAssetBundle(AssetBundleName, out assetBundle))
            {
                //异步请求还未完成
                if(assetBundle.LoadRequest != null || assetBundle.LoadAssetRequest != null 
                || assetBundle.SceneRequest !=null || assetBundle.UnloadSceneRequest != null)
                {
                    return false;
                }

                if(AssetBundleConfig.IsSimulator())
                {
                    return true;
                }

                ///被预加载pin住的
                if (IsPreLoad() && PinTime >0)
                {
                    if(FinishTime > CreateTime)
                    {
                        return Time.realtimeSinceStartup - FinishTime > PinTime;
                    }
                    return false;
                }
                //正在下载的
                if(AssetBundleFunction.InDowloading(ref context,ref this))
                {
                    return false;
                }
                //加载异常
                if (AssetBundleConfig.SafeMode && assetBundle.IsException())
                {
                    return true;
                }
                else if (assetBundle.AssetStatus.HasEnum( AssetBundleStatus.InMemory) )//加载正常
                {
                    int finish;
                    int total;
                    return IsSubAllDone(ref context,out finish,out total);
                }
                //else if(assetBundle.AssetStatus == AssetBundleStatus.None)//啥事也没干的task
                //{
                //    return true;
                //}
            }
            else
            {
                return true;
            }
            return false;
        }

        public void ToDetail(StringBuilder stringBuilder)
        {
            if(stringBuilder != null)
            {
                stringBuilder.AppendLine("Task Infomation:{");
                stringBuilder.AppendFormat("\tTask Id:{0}\n", this.TaskId);
                stringBuilder.AppendFormat("\tTask Priority:{0}\n", this.Priority);
                stringBuilder.AppendFormat("\tTask LoadType :{0}\n", this.TaskLoadType);
                stringBuilder.AppendFormat("\tTask ResType :{0}\n", this.TaskResType);
                stringBuilder.AppendFormat("\tTask Create Time :{0}\n", this.CreateTime);
                stringBuilder.AppendFormat("\tTask Finish Time :{0}\n", this.FinishTime);
                stringBuilder.AppendFormat("\tTask Pin Time :{0}\n", this.PinTime);
                stringBuilder.AppendFormat("\tTask Free Size:{0}\n", this.FreeSize);

                //
                stringBuilder.AppendLine("\t Task Result Args:");

                stringBuilder.AppendFormat("\t\t Position:{0}\n", Result.Position);
                stringBuilder.AppendFormat("\t\t Rotation:{0}\n", Result.Rotation);
                stringBuilder.AppendFormat("\t\t Scale :{0}\n", Result.Scale);
                stringBuilder.AppendFormat("\t\t Success :{0}\n", Result.Result);
                stringBuilder.AppendFormat("\t\t Has Event:{0}\n", Result.ResultCallback != null);

                stringBuilder.AppendLine("}");
            }

        }

        public bool Equals(AssetBundleTask other)
        {
            return TaskId == other.TaskId && AssetInfo.Equals(other.AssetInfo) && TaskLoadType == other.TaskLoadType && TaskResType == other.TaskResType && Priority == other.Priority && Result.Equals(other.Result) && Equals(SubTasks, other.SubTasks) && FreeSize == other.FreeSize && CreateTime.Equals(other.CreateTime) && _finishtime.Equals(other._finishtime) && PinTime.Equals(other.PinTime) && LoadSceneMode == other.LoadSceneMode;
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            return obj is AssetBundleTask && Equals((AssetBundleTask) obj);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                var hashCode = TaskId.GetHashCode();
                hashCode = (hashCode * 397) ^ AssetInfo.GetHashCode();
                hashCode = (hashCode * 397) ^ (int) TaskLoadType;
                hashCode = (hashCode * 397) ^ (int) TaskResType;
                hashCode = (hashCode * 397) ^ Priority;
                hashCode = (hashCode * 397) ^ Result.GetHashCode();
                hashCode = (hashCode * 397) ^ (SubTasks != null ? SubTasks.GetHashCode() : 0);
                hashCode = (hashCode * 397) ^ FreeSize.GetHashCode();
                hashCode = (hashCode * 397) ^ CreateTime.GetHashCode();
                hashCode = (hashCode * 397) ^ _finishtime.GetHashCode();
                hashCode = (hashCode * 397) ^ PinTime.GetHashCode();
                hashCode = (hashCode * 397) ^ (int) LoadSceneMode;
                return hashCode;
            }
        }
    }
}


