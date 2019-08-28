using UnityEngine;
using System.Collections;

namespace AssetBundleSystem
{
    public struct CommonLoadArgs :ILoadArgs
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

        public CommonLoadArgs(string path)
        {
            _assetPath = path;
            _priority = -1;
        }

        public CommonLoadArgs(string path, int priority)
        {
            _assetPath = path;
            _priority = priority;
        }

        public static implicit operator CommonLoadArgs(string path)
        {
            return  new CommonLoadArgs(path);
        }

    }

}

