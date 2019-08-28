using UnityEngine;
using System.Collections;

namespace AssetBundleSystem
{
    public struct ProgressArgs
    {
        public readonly float Progress;

        public readonly float DownloadProgres;

        public readonly string AssetPath;

        public readonly bool IsDone;

        public ProgressArgs(float progress,float download,string path,bool done)
        {
            Progress = progress;
            AssetPath = path;
            IsDone = done;
            DownloadProgres = download;
        }

    }
}

