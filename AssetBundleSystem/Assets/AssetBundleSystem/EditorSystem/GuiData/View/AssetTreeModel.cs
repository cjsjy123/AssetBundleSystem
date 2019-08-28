#if UNITY_EDITOR
using System;
using UnityEngine;
using System.Collections.Generic;
using System.IO;
using CommonUtils;
using UnityEditor.IMGUI.Controls;

namespace AssetBundleSystem.Editor
{

    internal interface IAssetTreeModel<T>: ITreeModel
    {
        T Root { get; set; }

        void Clear();

        void Add(ref T data);

        bool HasParent(int id);

        List<T> GetParent(int childId);

        List<T> GetChildren(int parentId);

        bool HasChildren(int parentid);

        bool AddChild(int parentid, int childid);
    }

    internal class AssetTreeModel<T>: IAssetTreeModel<T> where T:ITreeData
    {
        private readonly Dictionary<int,T> _dictionary = new Dictionary<int, T>();
        private readonly Dictionary<string,T> _assetbundleDictionary = new Dictionary<string, T>();
        private readonly Dictionary<string,T> _pathdictionary = new Dictionary<string, T>();
        private readonly Dictionary<int,List<int>> _children = new Dictionary<int, List<int>>();
        private readonly Dictionary<int,List<int>> _parents = new Dictionary<int, List<int>>();
        private readonly Dictionary<string,List<string>> _dependDictionary = new Dictionary<string, List<string>>();

        public T Root { get; set; }

        public void Clear()
        {
            _assetbundleDictionary.Clear();
            _pathdictionary.Clear();
            _dictionary.Clear();
            _children.Clear();
            _parents.Clear();
            _dependDictionary.Clear();
        }

        public List<T> GetAllItems()
        {
            List<T> list = ListPool<T>.Get();
            list.AddRange(_dictionary.Values);
            return list;
        }

        public void Add(ref T data)
        {
            //replace
            _dictionary[data.Id] = data;
            if (Directory.Exists(data.FilePath))
            {
                _assetbundleDictionary[data.DisplayName] = data;
            }
           
            AddDependRef(ref data);

            if (!string.IsNullOrEmpty(data.FilePath))
                _pathdictionary[data.FilePath] = data;
        }

        public List<T> GetDependParents(string filepath)
        {
            List<T> list = ListPool<T>.Get();

            List<string> deps;
            if (_dependDictionary.TryGetValue(filepath, out deps))
            {
                T item ;
                foreach (var dep in deps)
                {
                    if (GetItem(dep, out item) && item.EditorInfo.RuntimeInfo.AssetResType != AssetBundleResType.None)
                    {
                        list.Add(item);
                    }
                }
            }
            return list;
        }

        void AddDependRef(ref T data)
        {
            if (data.EditorInfo.RuntimeInfo.DependNames != null)
            {
                for (int i = 0; i < data.EditorInfo.RuntimeInfo.DependNames.Length; i++)
                {
                    var deppath = data.EditorInfo.RuntimeInfo.DependNames[i];
                    if (!string.IsNullOrEmpty(deppath))
                    {
                        List<string> list;
                        if (_dependDictionary.TryGetValue(deppath, out list))
                        {
                            if(list.Contains(data.FilePath) == false)
                            {
                                list.Add(data.FilePath);
                            }
                        }
                        else
                        {
                            _dependDictionary[deppath] = new List<string>(){ data.FilePath };
                        }
                    }
                }
            }
        }

        public bool GetItem(string key,out T data)
        {
            if (_pathdictionary.TryGetValue(key, out data))
            {
                return true;
            }
            return false;
        }

        public bool GetItem(int key, out T data)
        {
            if (_dictionary.TryGetValue(key, out data))
            {
                return true;
            }
            return false;
        }

        public bool GetItemByAssetBundleName(string key, out T data)
        {
            if (_assetbundleDictionary.TryGetValue(key, out data))
            {
                return true;
            }
            return false;
        }

        public void Sort(Comparison<int> comparefunction)//TreeView treeView,TreeViewItem rootItem
        {
            foreach(var kv in _children)
            {
                var list = kv.Value;
                if(list != null)
                {
                    list.Sort(comparefunction);
                }
            }
            //Stack<TreeViewItem> itemstack = StackPool<TreeViewItem>.Get();

            //itemstack.Push(rootItem);

            //while (itemstack.Count > 0)
            //{
            //    var item = itemstack.Pop();
            //    if (treeView.IsExpanded(item.id) && item.children.Count > 0 && item.children[0] != null)
            //    {
            //        List<int> idlist;
            //        if (!_children.TryGetValue(item.id, out idlist))
            //        {
            //            Debug.LogError("not found item");
            //        }
            //        else
            //        {
            //            idlist.Clear();
            //        }

            //        for (int i = 0; i < item.children.Count; i++)
            //        {
            //            var child = item.children[i];
            //            itemstack.Push(child);
            //            idlist.Add(child.id);
            //        }


            //    }
            //}

            //StackPool<TreeViewItem>.Release(itemstack);
        }

        public IList<int> GetAncestors(int id)
        {
            var parents = new List<int>();
            if (HasParent(id))
            {
                var parentsList = GetParent(id);
                while (parentsList != null && parentsList.Count > 0)
                {
                    if (parentsList.Count > 1)
                        throw new ArgumentException("muliti parents");

                    bool hasparent = false;
                    for (int i = 0; i < parentsList.Count; i++)
                    {
                        var p = parentsList[i];
                        parents.Add(p.Id);
                        if (HasParent(p.Id))
                        {
                            ListPool<T>.Release(parentsList);
                            parentsList = GetParent(p.Id);
                            hasparent = true;
                            break;
                        }
                    }

                    if (!hasparent)
                    {
                        parentsList = null;
                    }
                }
            }
            return parents;
        }

        public IList<int> GetDescendantsThatHaveChildren(int id)
        {
            return GetParentsBelowStackBased(id);
        }

        IList<int> GetParentsBelowStackBased(int id)
        {
            Stack<int> stack = StackPool<int>.Get();
            stack.Push(id);

            var parentsBelow = new List<int>();
            while (stack.Count > 0)
            {
                int current = stack.Pop();
                if (HasChildren(current))
                {
                    parentsBelow.Add(current);
                    var children = GetChildren(current);
                    foreach (var val in children)
                    {
                        stack.Push(val.Id);
                    }
                    ListPool<T>.Release(children);
                }
            }
            StackPool<int>.Release(stack);
            return parentsBelow;
        }

        public bool HasParent<U>(ref U data) where U : ITreeData
        {
            return HasParent(data.Id);
        }

        public bool HasParent(int id)
        {
            return _parents.ContainsKey(id);
        }

        public List<T> GetParent<U>(ref U data) where U : ITreeData
        {
            return GetParent(data.Id);
        }

        public List<T> GetParent(int childId)
        {
            List<T> resultList = ListPool<T>.Get();

            List<int> idlist;
            if (_parents.TryGetValue(childId, out idlist))
            {
                for (int i = 0; i < idlist.Count; i++)
                {
                    var id = idlist[i];
                    T data;
                    if (_dictionary.TryGetValue(id, out data) )
                    {
                        resultList.Add(data);
                    }
                    else
                    {
                        Debug.LogErrorFormat("not Found :{0}",id);
                    }
                }

            }
            return resultList;
        }

        public List<T> GetChildren<U>(ref U data) where U : ITreeData
        {
            return GetChildren(data.Id);
        }

        public List<T> GetChildren(int parentId)
        {
            List<T> childrens = ListPool<T>.Get();

            List<int> list;
            if (_children.TryGetValue(parentId, out list))
            {
                T data ;
                foreach (var child in list)
                {
                    if (_dictionary.TryGetValue(child, out data) )
                    {
                        childrens.Add(data);
                    }
                    else
                    {
                        Debug.LogErrorFormat("not exist :{0}",child);
                    }
                }
                return childrens;
            }

            return childrens;
        }

        public bool HasChildren<U>(ref U data) where U : ITreeData
        {
            return HasChildren(data.Id);
        }

        public bool HasChildren(int parentid)
        {
            List<int> list;
            if (_children.TryGetValue(parentid, out list))
            {
                return list.Count > 0;
            }
            return false;
        }

        public bool AddChild<U>(ref T parent, ref U child) where U : ITreeData
        {
            return AddChild(parent.Id, child.Id);
        }

        public bool AddChild(int parentid,int childid)
        {
            bool ret = true;
            List<int> list;
            if (_children.TryGetValue(parentid, out list))
            {
                if (!list.Contains(childid))
                {
                    list.Add(childid);
                }
                else
                {
                    ret = false;
                }
            }
            else
            {
                _children.Add(parentid,new List<int>(){childid});
            }

            if (ret)
            {
                SetParent(childid,parentid);
            }

            return ret;
        }

        void SetParent(int childId,int parentId)
        {
            List<int> idList;
            if (!_parents.TryGetValue(childId, out idList))
            {
                _parents.Add(childId, new List<int>(){parentId});
            }
            else
            {
                if (!idList.Contains(parentId))
                {
                    idList.Add(parentId);
                }
            }
        }
    }
}

#endif