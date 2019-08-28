using UnityEngine;
using System.Collections;
using System.IO;
using UnityEngine.SceneManagement;

namespace AssetBundleSystem
{
    public struct SceneLoadArgs : ILoadArgs
    {
        public enum UnLoadSceneMode
        {
            None,
            Sync,
            Async,
        }

        private string _assetPath;
        public string AssetPath
        {
            get { return _assetPath; }
            set { _assetPath = value; }
        }
        private int _priority;
        public int Priority
        {
            get { return _priority; }
            set { _priority = value; }
        }

        public LoadSceneMode SceneMode;

        public UnLoadSceneMode UnloadMode;

        public SceneLoadArgs(string path)
        {
            _assetPath = path;
            SceneMode = LoadSceneMode.Single;
            _priority = -1;
            UnloadMode = UnLoadSceneMode.None;

        }

        public SceneLoadArgs(string path,LoadSceneMode md)
        {
            _assetPath = path;
            SceneMode = md;
            _priority = -1;
            UnloadMode = UnLoadSceneMode.None;
        }


        public SceneLoadArgs(string path, LoadSceneMode md,int priority)
        {

            _assetPath = path;
            SceneMode = md;
            _priority = priority;
            UnloadMode = UnLoadSceneMode.None;
        }

        public SceneLoadArgs(string path, LoadSceneMode md, UnLoadSceneMode unloadmd)
        {
            _assetPath = path;
            SceneMode = md;
            _priority = -1;
            UnloadMode = unloadmd;
        }

        public SceneLoadArgs(string path, LoadSceneMode md, UnLoadSceneMode unloadmd, int priority)
        {
            _assetPath = path;
            SceneMode = md;
            _priority = priority;
            UnloadMode = unloadmd;
        }


        public static implicit operator SceneLoadArgs(string path)
        {
            return  new SceneLoadArgs(path);
        }
    }

}


