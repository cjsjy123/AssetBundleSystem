using UnityEngine;
using System.Collections;

namespace AssetBundleSystem
{
    public struct GameObjectLoadArgs : ILoadArgs
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

        public readonly Vector3 Position;

        public readonly Quaternion Rotation;

        public readonly Vector3 Scale;

        public readonly Transform Parent;
        

        public GameObjectLoadArgs(string path)
        {
            _assetPath = path;
            Parent = null;
            Position = Vector3.zero;
            Scale = Vector3.one;
            Rotation = Quaternion.identity;
            _priority = -1;
        }

        public GameObjectLoadArgs(string path, Transform parent)
        {
            _assetPath = path;
            Parent = parent;
            Position = Vector3.zero;
            Scale = Vector3.one;
            Rotation = Quaternion.identity;
            _priority = -1;
        }


        public GameObjectLoadArgs(string path, Transform parent, Vector3 pos,Quaternion rot,Vector3 scale)
        {
            _assetPath = path;
            Parent = parent;
            Position = pos;
            Scale = scale;
            Rotation = rot;
            _priority = -1;
        }

        public GameObjectLoadArgs(string path, Transform parent, Vector3 pos, Quaternion rot, Vector3 scale,int priority)
        {
            _assetPath = path;
            Parent = parent;
            Position = pos;
            Scale = scale;
            Rotation = rot;
            _priority = priority;
        }

        public static implicit operator GameObjectLoadArgs(string path)
        {
            return  new GameObjectLoadArgs(path);
        }

    }
}

