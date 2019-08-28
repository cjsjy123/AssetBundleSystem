using System;
using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using CommonUtils;

namespace AssetBundleSystem
{

    public abstract class BaseFeature<T,TI,E> : IFeature where T:struct where TI :IAssetBundleSystem where E:struct 
    {
        protected T Context;
        protected readonly List<TI> Systems;
        protected bool _InitSuccess;
        protected int _initstep;
        public bool InitSuccess { get { return _InitSuccess; } }

        protected BaseFeature()
        {
            Systems = new List<TI>();

            Create();

        }

        internal T GetContext()
        {
            return Context;
        }

        void Create()
        {
            AddAssetSystems();
            _InitSuccess = InitSystems();

            if (InitSuccess)
            {
                Debug.LogFormat("{0} Init Success", this);
            }

#if UNITY_EDITOR
            if (!Application.isPlaying)
            {
                GUpdater.mIns.AddEditorUpdate(OnUpdate);
            }
            else
            {
                GUpdater.mIns.AddUpdate(OnUpdate);
            }
#else
            GUpdater.mIns.AddUpdate(OnUpdate);
#endif
        }

        public void Destroy()
        {
            GUpdater.mIns.RemoveUpdate(OnUpdate);
#if UNITY_EDITOR
            GUpdater.mIns.RemoveEditorUpdate(OnUpdate);
#endif
        }

        protected virtual void OnUpdate()
        {
            if(!_InitSuccess)
            {
                //retry
                _InitSuccess = true;

                if(_initstep <= Systems.Count)
                {
                    var blocksystem = Systems[_initstep - 1];
                    if(blocksystem.Block)
                    {
                        _InitSuccess = false;
                    }

                    if(_InitSuccess)
                    {
                        for (int i = _initstep; i < Systems.Count; i++)
                        {
                            var sys = Systems[i];
                            if (!sys.Block)
                            {
                                TryCallInit(sys, ref Context);
                                _initstep++;
                            }
                            else
                            {
                                _initstep++;
                                _InitSuccess = false;
                                break;
                            }
                        }
                    }
                }

                if(InitSuccess)
                {
                    Debug.LogFormat("{0} Init Success", this);
                }
            }
        }

        protected int TryCallInit(TI task,ref T data)
        {
            try
            {
                if (task is IAssetBundleReadData)
                {
                    IAssetBundleReadData sys = task as IAssetBundleReadData;
                    sys.Parent = this;
                }

                if (task is IAssetBundleInitContext<T>)
                {
                    IAssetBundleInitContext<T> initsystem = task as IAssetBundleInitContext<T>;
                    int code = initsystem.Init(ref data);
                    if (task.Block)
                    {
                        return 0;
                    }
                    else if (code != 0)
                    {
                        Debug.LogErrorFormat("{0} Init Error :{1}", initsystem, code);
                    }
                    else if (AssetBundleConfig.DebugMode.HasEnum(DebugMode.Detail))
                    {
                        Debug.LogFormat("{0} Init Success", initsystem);
                    }
                    return code;
                }
                return 0;
            }
            catch (Exception e)
            {
                Debug.LogError(e);
                return -1;
            }

        }

        protected int TryCallExcute(TI task, ref T data, ref E eventdata)
        {
            try
            {
                if (task is IAssetBundleExecute<T, E>)
                {
                    IAssetBundleExecute<T, E> initsystem = task as IAssetBundleExecute<T, E>;
                    int code = initsystem.Execute(ref data, eventdata);
                    if (task.Block)
                    {
                        return 0;
                    }
                    else if (code != 0)
                    {
                        Debug.LogErrorFormat("{0} Excute Error :{1}", initsystem, code);
                    }
                    //else if (AssetBundleConfig.DebugMode.HasEnum(DebugMode.Detail))
                    //{
                    //    Debug.LogFormat("{0} Excute Success", initsystem);
                    //}
                    return code;
                }
                return 0;
            }
            catch (Exception e)
            {
                Debug.LogError(e);
                return -1;
            }

        }

        protected abstract void AddAssetSystems();

        protected abstract bool InitSystems();

        public abstract void Execute(E assetEvent);

        public abstract void ReLoad();
        /// <summary>
        /// must be carefully
        /// </summary>
        /// <returns>The read.</returns>
        internal T StartRead()
        {
            return Context;
        }

        internal void EndRead(ref T data)
        {
            Context = data;
        }
    }
}


