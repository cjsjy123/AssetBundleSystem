using System;
using UnityEngine;
using System.Collections;
#if UNITY_EDITOR
using UnityEditor;
#endif
namespace CommonUtils
{

    public class GUpdater
    {
        private static GUpdater _mins;

        public static GUpdater mIns
        {
            get
            {
                if (_mins == null)
                {
                    _mins = new GUpdater();
                }
                return _mins;
            }
        }

        private class RunningUpdater:MonoBehaviour
        {
            public Action UpdateAction;
            public Action LateUpdateAction;
            public Action QuitAction;
            void Update()
            {
                if (UpdateAction != null)
                {
                    UpdateAction();
                }
            }

            void LateUpdate()
            {
                if (LateUpdateAction != null)
                {
                    LateUpdateAction();
                }
            }

            void OnDestroy()
            {
                if (QuitAction != null)
                {
                    QuitAction();
                }
            }
        }

        private RunningUpdater _runningUpdater;

        void CreateUnityObject()
        {
            if (_runningUpdater == null)
            {
                GameObject Updater = new GameObject("Updater");
                _runningUpdater = Updater.AddComponent<RunningUpdater>();
                GameObject.DontDestroyOnLoad(Updater);
            }
        }

        void AddUpdateToRunning(Action cbk)
        {
            CreateUnityObject();
            _runningUpdater.UpdateAction -= cbk;
            _runningUpdater.UpdateAction += cbk;
        }


        void AddLateUpdateToRunning(Action cbk)
        {
            CreateUnityObject();
            _runningUpdater.LateUpdateAction -= cbk;
            _runningUpdater.LateUpdateAction += cbk;
        }

        void AddQuitToRunning(Action cbk)
        {
            CreateUnityObject();
            _runningUpdater.QuitAction -= cbk;
            _runningUpdater.QuitAction += cbk;
        }

        void RemoveUpdateToRunning(Action cbk)
        {
            if (_runningUpdater != null)
            {
                _runningUpdater.UpdateAction -= cbk;
            }
        }

        void RemoveLateUpdateToRunning(Action cbk)
        {
            if (_runningUpdater != null)
            {
                _runningUpdater.LateUpdateAction -= cbk;
            }
        }

        void RemoveQuitToRunning(Action cbk)
        {
            if (_runningUpdater != null)
            {
                _runningUpdater.QuitAction -= cbk;
            }
        }

        public void AddUpdate(Action cbk)
        {
            if (Application.isPlaying)
            {
                AddUpdateToRunning(cbk);
            }
        }

        public void AddLateUpdate(Action cbk)
        {
            if (Application.isPlaying)
            {
                AddLateUpdateToRunning(cbk);
            }
        }

        public void AddQuitCallback(Action cbk)
        {
            if (Application.isPlaying)
            {
                AddQuitToRunning(cbk);
            }
        }

#if UNITY_EDITOR
        public void AddEditorUpdate(EditorApplication.CallbackFunction editorFunction)
        {
            EditorApplication.update -= editorFunction;
            EditorApplication.update += editorFunction;
        }

        public void RemoveEditorUpdate(EditorApplication.CallbackFunction editorFunction)
        {
            EditorApplication.update -= editorFunction;
        }
#endif

        public void RemoveUpdate(Action cbk)
        {
            if (Application.isPlaying)
            {
                RemoveUpdateToRunning(cbk);
            }
            else
            {
#if UNITY_EDITOR
                EditorApplication.CallbackFunction cbkfunc = (EditorApplication.CallbackFunction)((object)cbk);
                EditorApplication.update -= cbkfunc;
#endif
            }
        }

        public void RemoveLateUpdate(Action cbk)
        {
            if (Application.isPlaying)
            {
                RemoveLateUpdateToRunning(cbk);
            }
        }

        public Coroutine StartCoroutine(IEnumerator func)
        {
            CreateUnityObject();
            return _runningUpdater.StartCoroutine(func);
        }

        public void StopCoroutine(IEnumerator func)
        {
            if (_runningUpdater != null)
            {
                _runningUpdater.StopCoroutine(func);
            }

        }

        public void StopCoroutine(Coroutine func)
        {
            if (_runningUpdater != null)
            {
                _runningUpdater.StopCoroutine(func);
            }

        }
    }
}


