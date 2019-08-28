using System;
using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using CommonUtils;
using UnityEngine.Networking;

namespace AssetBundleSystem
{
    internal class DefaultParseDepReader : IParseDependencyReader
    {
        public bool IsAsync
        {
            get
            {
#if UNITY_ANDROID
                return true;
#else
                return false;
#endif
            }
        }

        public List<AssetBundleInfo> AsyncResultList;

        public bool Read(string filepath,out List<AssetBundleInfo> assetbundlelist)
        {
            if (File.Exists(filepath))
            {
                assetbundlelist = ListPool<AssetBundleInfo>.Get();

                byte[] bytes = File.ReadAllBytes(filepath);
                ReadBytes(assetbundlelist, bytes);
                return true;
            }
            assetbundlelist = null;
            return false;
        }


        public IEnumerator ReadAsync(string filepath)
        {
#if UNITY_EDITOR
            filepath = string.Format("file:///{0}", filepath);
#endif
            WWW www = new WWW(filepath);
            yield return www;

            if (string.IsNullOrEmpty(www.error))
            {
                if (AsyncResultList != null)
                {
                    ListPool<AssetBundleInfo>.Release(AsyncResultList);
                    AsyncResultList = null;
                }

                AsyncResultList = ListPool<AssetBundleInfo>.Get();

                ReadBytes(AsyncResultList, www.bytes);

                www.Dispose();
            }
            else
            {
                Debug.LogErrorFormat("read dep error: {0} http:{1}", www.error, www.text);
            }

        }

        public void ReadBytes(List<AssetBundleInfo> assetbundlelist,byte[] bytes)
        {
            using (var ms = new MemoryStream(bytes))
            {
                using (BinaryReader reader = new BinaryReader(ms))
                {
                    int version = reader.ReadInt32();
                    if (version == AssetBundleConfig.Version)
                    {
                        int allcnt = reader.ReadInt32();
                        for (int i = 0; i < allcnt; i++)
                        {
                            AssetBundleInfo info = new AssetBundleInfo();
                            info.HashCode = reader.ReadString();
                            info.Crc = reader.ReadUInt32();
                            info.AssetName = reader.ReadString();
                            info.UnityPath = reader.ReadString();
                            info.AssetBundleName = reader.ReadString();
                            info.AssetSize = reader.ReadInt64();
                            info.FileSize = reader.ReadInt64();
                            info.AssetResType = (AssetBundleResType)reader.ReadInt32();

                            info.DependNames = ReadArray(reader);
                            info.DepAssetBundleNames = ReadArray(reader);

                            assetbundlelist.Add(info);
                        }
                    }
                    else
                    {
                        Debug.LogErrorFormat("version not match :{0} - {1}",version,AssetBundleConfig.Version);
                    }
                    reader.Close();
                }
                ms.Close();
            }
        }

        string[] ReadArray(BinaryReader reader)
        {
            int cnt = reader.ReadInt32();
            if(cnt == 0)
            {
                return null;
            }

            string[] data = new string[cnt];
            for (int i = 0; i < cnt; i++)
            {
                data[i] = reader.ReadString();
            }
            return data;
        }

    }


}

