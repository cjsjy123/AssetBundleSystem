#if UNITY_EDITOR
using System;
using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Text;
using CommonUtils;

namespace AssetBundleSystem.Editor
{
    internal class DefaultParseDepWriter :IParseDependencyWriter
    {
        public bool Write(string filepath)
        {
            try
            {
                var dirname = Path.GetDirectoryName(filepath);
                if (Directory.Exists(dirname) == false)
                {
                    Directory.CreateDirectory(dirname);
                }

                using (var fs = new FileStream(filepath, FileMode.OpenOrCreate, FileAccess.Write))
                {
                    //clear
                    fs.SetLength(0);
                    var datamodel = AssetTreeManager.mIns.GetModel<AssetTreeData>();
                    var allitems = datamodel.GetAllItems();
                    using (var ms = new MemoryStream())
                    {
                        using (var bs = new BinaryWriter(ms))
                        {
                            bs.Write(AssetBundleConfig.Version);
            
                            List<AssetTreeData> tempList = new List<AssetTreeData>();
                            for (int i = 0; i < allitems.Count; i++)
                            {
                                AssetTreeData item = allitems[i];
                                if (File.Exists(item.FilePath))
                                {
                                    var existindex = tempList.FindIndex(it => it.FilePath.Equals(item.FilePath));
                                    if(existindex == -1)
                                        tempList.Add(item);
                                }
                            }

                            bs.Write(tempList.Count);
                            for (int i = 0; i < tempList.Count; i++)
                            {
                                AssetTreeData item = tempList[i];
                                var data = item.EditorInfo.RuntimeInfo;
                                string hash =null;
                                uint crc = 0;

                                var parent = datamodel.GetParent(ref item);
                                if (parent.Count > 0)
                                {
                                    hash = parent[0].EditorInfo.RuntimeInfo.HashCode;
                                    crc = parent[0].EditorInfo.RuntimeInfo.Crc;
                                }
                                ListPool<AssetTreeData>.Release(parent);

                                //
                                bs.Write(hash ?? "");
                                bs.Write(crc);
                                bs.Write(data.AssetName);
                                bs.Write(data.UnityPath ??"");
                                bs.Write(data.AssetBundleName ??"");
                                bs.Write(data.AssetSize);
                                bs.Write(data.FileSize);
                                bs.Write((int)data.AssetResType);

                                WriteArray(bs, data.DependNames);
                                WriteArray(bs, data.DepAssetBundleNames);
                 
                            }

                            ms.WriteTo(fs);
                            bs.Close();
                        }
    
                        ms.Close();
                        fs.Flush();
                        fs.Close();
                    }

                    ListPool<AssetTreeData>.Release(allitems);
                }

                return true;
            }
            catch (Exception e)
            {
                Debug.LogError(e);
                return false;
            }
        }

        void WriteArray(BinaryWriter writer, string[] array)
        {
            int cnt = array == null ? 0 : array.Length;
            writer.Write(cnt);

            for (int i = 0; i < cnt; i++)
            {
                writer.Write(array[i] == null ?"":array[i]);
            }
        }
    }
}
#endif

