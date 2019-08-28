using System;
using System.IO;
using UnityEngine;

namespace AssetBundleSystem
{
    public static class AssetBundleHelper
    {
        #region Path
        public static bool IsQuit { get; internal set; }

        private static string _streamingAssetsPath;

        public static string StreamingAssetsPath
        {
            get
            {
                if (_streamingAssetsPath == null)
                    _streamingAssetsPath = Application.streamingAssetsPath;
                return _streamingAssetsPath;
            }
        }

        private static string _persistentDataPath;

        public static string PersistentDataPath
        {
            get
            {
                if (_persistentDataPath == null)
                    _persistentDataPath = Application.persistentDataPath;
                return _persistentDataPath;
            }
        }

        private static string _dataPath;

        public static string DataPath
        {
            get
            {
                if (_dataPath == null)
                    _dataPath = Application.dataPath;
                return _dataPath;
            }
        }

        public static string GetBundleStreamPath(string basename)
        {
            if (Application.platform == RuntimePlatform.IPhonePlayer)
            {
                return string.Format("{0}/{1}", StreamingAssetsPath,  basename);
            }
            else if (Application.platform == RuntimePlatform.Android)
            {
                return string.Format("jar:file://{0}!/assets/{1}", DataPath, basename);
            }

            return string.Format("{0}/{1}", StreamingAssetsPath,  basename);
        }

        public static string GetBundlePersistentPath(string basename)
        {

            if (Application.platform == RuntimePlatform.IPhonePlayer)
            {
                return string.Format("{0}/IOS/{1}", PersistentDataPath, basename);
            }
            else if (Application.platform == RuntimePlatform.Android)
            {
                return string.Format("{0}/ANDROID/{1}", PersistentDataPath, basename);
            }

            return string.Format("{0}/Editor/{1}", PersistentDataPath, basename);

        }

        public static string EditorName2AssetName(string name)
        {
            //string result = name;
            name = name.Replace('\\', '/');
            return name;
        }

        #endregion

    }

}


