using UnityEngine;
using System.Collections;

namespace AssetBundleSystem
{
    public struct PreLoadArgs :ILoadArgs
    {
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

        public float PinTime;

        public PreLoadArgs(string path)
        {
            _assetPath = path;
            PinTime = -1;
            _priority = -1;
        }

        public PreLoadArgs(string path,float time)
        {
            _assetPath = path;
            PinTime = time;
            _priority = -1;
        }

        public PreLoadArgs(string path, int priority)
        {
            _assetPath = path;
            PinTime = -1;
            _priority = priority;
        }

        public PreLoadArgs(string path, float time, int priority)
        {
            _assetPath = path;
            PinTime = time;
            _priority = priority;
        }

        public static implicit operator PreLoadArgs(string path)
        {
            return  new PreLoadArgs(path);
        }

    }

}

