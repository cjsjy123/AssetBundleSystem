#if UNITY_EDITOR
using System;
using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Security.Cryptography;
using System.Text;
using CommonUtils;
using UnityEditor.IMGUI.Controls;

namespace AssetBundleSystem.Editor
{

    internal class AssetBundleEditorHelper 
    {

        public  static string GetNormalText(string format, params object[] info)
        {
            return string.Format(format, info);
        }

        public static string GetColorText(string format, Color col, params object[] info)
        {
            return string.Format("<color=#{0}>{1}</color>", ColorUtility.ToHtmlStringRGBA(col),
                GetNormalText(format, info));
        }

        public static string ConvertSize(long value)
        {
            if (value < 1024)
            {
                return string.Format("{0}b", value);
            }
            else if (value < 1048576)
            {
                return string.Format("{0:N3}kb", value/1024f);
            }
            return string.Format("{0:N3}mb", value / 1048576f);
        }

        public static string GetDirectoryName(string path)
        {
            string dirname = path;
            int index = dirname.LastIndexOf('/');
            if (index != -1)
            {
                return dirname.Substring(index+1);
            }
            index = dirname.LastIndexOf('\\');
            if (index != -1)
            {
                return dirname.Substring(index+1);
            }
            return dirname;
        }

        public static string Md5Sum(string strToEncrypt)
        {
            byte[] bs = Encoding.UTF8.GetBytes(strToEncrypt);

            MD5 md5;
            md5 = MD5CryptoServiceProvider.Create();

            byte[] hashBytes = md5.ComputeHash(bs);

            string hashString = "";
            for (int i = 0; i < hashBytes.Length; i++)
            {
                hashString = Convert.ToString(hashBytes[i], 16).PadLeft(2, '0');
            }

            return hashString.PadLeft(32, '0');
        }

        public static void DirectoryCopy(string sourceDirName, string destDirName)
        {
            if (!Directory.Exists(destDirName))
            {
                Directory.CreateDirectory(destDirName);
            }

            foreach (string folderPath in Directory.GetDirectories(sourceDirName, "*", SearchOption.AllDirectories))
            {
                if (!Directory.Exists(folderPath.Replace(sourceDirName, destDirName)))
                    Directory.CreateDirectory(folderPath.Replace(sourceDirName, destDirName));
            }

            foreach (string filePath in Directory.GetFiles(sourceDirName, "*.*", SearchOption.AllDirectories))
            {
                var fileDirName = Path.GetDirectoryName(filePath).Replace("\\", "/");
                var fileName = Path.GetFileName(filePath);
                string newFilePath = Path.Combine(fileDirName.Replace(sourceDirName, destDirName), fileName);

                File.Copy(filePath, newFilePath, true);
            }
        }


        public static bool HasEnum<T>(T current, T target) where T : struct, IConvertible
        {
            int c = Convert.ToInt32(current);
            int t = Convert.ToInt32(target);
            if ((c & t) == t)
            {
                return true;
            }
            return false;
        }

        public static bool HasEnumMask<T>(T current, T target) where T : struct, IConvertible
        {
            int c = Convert.ToInt32(current) >> 1;
            int t = Convert.ToInt32(target);
            if ((c & t) == t)
            {
                return true;
            }
            return false;
        }

        public static string GetUnityAssetPath(string path)
        {
            int index = path.IndexOf("Assets/",StringComparison.OrdinalIgnoreCase);
            if (index != -1)
                path = path.Substring(index);

            index = path.IndexOf("Assets\\", StringComparison.OrdinalIgnoreCase);
            if (index == -1)
                return path;
            return path.Substring(index);

        }

        public static AssetWindow.WindowMode GetMainMode(AssetWindow.WindowMode md)
        {
            if(md == AssetWindow.WindowMode.End)
            {
                return AssetWindow.WindowMode.Asset;
            }

            var mdidx = (int)md / 8;
            return (AssetWindow.WindowMode)(mdidx*8);
        }

        public static void TreeListChildren(TreeViewItem rootItem, IList<TreeViewItem> row)
        {
            row.Clear();
            Stack<TreeViewItem> stack = StackPool<TreeViewItem>.Get();

            for (int i = rootItem.children.Count - 1; i >= 0; i--)
            {
                stack.Push(rootItem.children[i]);
            }

            while (stack.Count > 0)
            {
                var item = stack.Pop();
                row.Add(item);

                if (item.hasChildren && item.children[0] != null)
                {
                    for (int i = item.children.Count - 1; i >= 0; i--)
                    {
                        stack.Push(item.children[i]);
                    }
                }
            }

            StackPool<TreeViewItem>.Release(stack);
        }

        public static long GetFileSize<T>(ref T data, bool isroot, AssetTreeModel<T> datamodel) where T : ITreeData
        {
            long size = 0;
            if (isroot)
            {
                if (datamodel.HasChildren(ref data))
                {
                    var children = datamodel.GetChildren(ref data);
                    for (int i = 0; i < children.Count; i++)
                    {
                        size += children[i].EditorInfo.GetAllFileSize();
                    }

                    ListPool<T>.Release(children);
                }
            }
            else
            {
                size = data.EditorInfo.GetAllFileSize();
            }
            return size;
        }

        public static long GetAssetSize<T>(ref T data, bool isroot, AssetTreeModel<T> datamodel) where T : ITreeData
        {
            long size = 0;
            if (isroot)
            {
                if (datamodel.HasChildren(ref data))
                {
                    var children = datamodel.GetChildren(ref data);
                    for (int i = 0; i < children.Count; i++)
                    {
                        size += children[i].EditorInfo.GetAllAssetSize();
                    }

                    ListPool<T>.Release(children);
                }
            }
            else
            {
                size = data.EditorInfo.GetAllAssetSize();
            }
            return size;
        }

        public static long GetSelfFileSize<T>(ref T data, bool isroot, AssetTreeModel<T> datamodel) where T : ITreeData
        {
            long size = 0;
            if (isroot)
            {
                if (datamodel.HasChildren(ref data))
                {
                    var children = datamodel.GetChildren(ref data);
                    for (int i = 0; i < children.Count; i++)
                    {
                        size += children[i].EditorInfo.RuntimeInfo.FileSize;
                    }

                    ListPool<T>.Release(children);
                }
            }
            else
            {
                size = data.EditorInfo.RuntimeInfo.FileSize;
            }
            return size;
        }

        public static long GetSelfAssetSize<T>(ref T data, bool isroot, AssetTreeModel<T> datamodel) where T : ITreeData
        {
            long size = 0;
            if (isroot)
            {
                if (datamodel.HasChildren(ref data))
                {
                    var children = datamodel.GetChildren(ref data);
                    for (int i = 0; i < children.Count; i++)
                    {
                        size += children[i].EditorInfo.RuntimeInfo.AssetSize;
                    }

                    ListPool<T>.Release(children);
                }
            }
            else
            {
                size = data.EditorInfo.RuntimeInfo.AssetSize;
            }
            return size;
        }
    }
}


#endif