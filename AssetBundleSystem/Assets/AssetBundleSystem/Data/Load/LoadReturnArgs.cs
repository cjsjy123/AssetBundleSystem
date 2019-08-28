using System;
using UnityEngine;
using System.Collections;

namespace AssetBundleSystem
{

    public struct LoadReturnArgs
    {
        public delegate void LoadCallback(ref ResultArgs result);

        public delegate void ProgressCallback(ref ProgressArgs result);

        public long TaskId;

        private LoadCallback _loadCallback;

        public LoadCallback LoadResultEvent
        {
            get { return _loadCallback; }
            set
            {
                SetCallBack(value);
            }
        }

        private ProgressCallback _progressEvent;

        public ProgressCallback ProgressEvent
        {
            get
            {
                return _progressEvent;
            }
            set
            {
                SetCallBack(value);
            }
        }

        private readonly AssetBundleFeature _cacheFeature;

        internal LoadReturnArgs(AssetBundleFeature feature, long taskid)
        {
            _cacheFeature = feature;
            TaskId = taskid;
            _loadCallback = null;
            _progressEvent = null;
        }

        public bool IsInTask()
        {
            return TaskId >0;
        }

        public LoadReturnArgs SetActive(bool active)
        {
            if (_cacheFeature != null)
            {
                var contexts = _cacheFeature.StartRead();

                if (contexts.Tasks != null)
                {
                    for (int i = 0; i < contexts.Tasks.Count; i++)
                    {
                        var task = contexts.Tasks[i];
                        if (task.TaskId == this.TaskId)
                        {
                            if(task.Result.Hide == active)
                            {
                                task.Result.Hide = !active;
                                contexts.Tasks[i] = task;
                                _cacheFeature.EndRead(ref contexts);
                            }

                            break;
                        }
                    }
                }
            }
            else
            {
                Debug.LogError("cant set active");
            }
            return this;
        }

        public LoadReturnArgs SetCallBack(LoadCallback value)
        {
            if (_loadCallback != value)
            {
                _loadCallback = value;
                if (_cacheFeature != null)
                {
                    var contexts = _cacheFeature.StartRead();

                    if (contexts.Tasks != null)
                    {
                        for (int i = 0; i < contexts.Tasks.Count; i++)
                        {
                            var task = contexts.Tasks[i];
                            if (task.TaskId == this.TaskId)
                            {
                                task.Result.ResultCallback = value;
                                contexts.Tasks[i] = task;
                                _cacheFeature.EndRead(ref contexts);
                                break;
                            }
                        }
                    }
                }
                else
                {
                    Debug.LogError("cant bind callback");
                }
            }
            return this;
        }

        public LoadReturnArgs SetCallBack(ProgressCallback value)
        {
            if (_progressEvent != value)
            {
                _progressEvent = value;
                if (_cacheFeature != null)
                {
                    var contexts = _cacheFeature.StartRead();

                    if (contexts.Tasks != null)
                    {
                        for (int i = 0; i < contexts.Tasks.Count; i++)
                        {
                            var task = contexts.Tasks[i];
                            if (task.TaskId == this.TaskId)
                            {
                                task.Result.ProgresCallback = value;
                                contexts.Tasks[i] = task;
                                _cacheFeature.EndRead(ref contexts);
                                break;
                            }
                        }
                    }
                }
                else
                {
                    Debug.LogError("cant bind callback");
                }
            }
            return this;
        }

        public LoadReturnArgs SetFreeSize(bool free)
        {
            if (_cacheFeature != null)
            {
                var contexts = _cacheFeature.StartRead();

                if (contexts.Tasks != null)
                {
                    for (int i = 0; i < contexts.Tasks.Count; i++)
                    {
                        var task = contexts.Tasks[i];
                        if (task.TaskId == this.TaskId)
                        {
                            task.FreeSize = free;
                            contexts.Tasks[i] = task;
                            _cacheFeature.EndRead(ref contexts);
                            break;
                        }
                    }
                }
            }
            else
            {
                Debug.LogError("cant SetFreeSize");
            }
            return this;
        }

        public void Excute()
        {
            if (_cacheFeature != null)
            {
                _cacheFeature.Execute( AssetEvent.Now);
            }
            else
            {
                Debug.LogError("Faild Invoke Now");
            }
        }
    }

}

