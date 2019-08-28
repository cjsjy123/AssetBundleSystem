#if UNITY_EDITOR
using System;
using System.Collections;
using UnityEngine;
using System.Collections.Generic;
using UnityEditor;
using System.IO;
using CommonUtils;
using CommonUtils.Editor;
using UnityEngine.Profiling;
using Object = UnityEngine.Object;

namespace AssetBundleSystem.Editor
{
    internal class AssetLogic : ILogicInterface
    {
        public AssetWindow.WindowMode TypeMode { get { return AssetWindow.WindowMode.Asset; } }
        public bool Inited { get; private set; }

        private readonly List<Shader> _alwayShaders = new List<Shader>();

        public void Clear()
        {
            Inited = false;
            AssetTreeManager.mIns.GetModel<AssetTreeData>().Clear();
        }

        void GenerateAlwaysShaders()
        {
            SerializedObject graphicsSettings = new SerializedObject(AssetDatabase.LoadAllAssetsAtPath("ProjectSettings/GraphicsSettings.asset")[0]);
            SerializedProperty it = graphicsSettings.GetIterator();

            while (it.NextVisible(true))
            {
                if (it.name == "m_AlwaysIncludedShaders")
                {
                    _alwayShaders.Clear();
                    int size = it.arraySize;
                    for (int i = 0; i < size; i++)
                    {
                        var property = it.GetArrayElementAtIndex(i);
                        Shader shader = property.objectReferenceValue as Shader;
                        if (shader != null)
                            _alwayShaders.Add(shader);
                    }
                    graphicsSettings.ApplyModifiedProperties();
                }
            }
        }

        public IEnumerator ReLoad()
        {

            Dictionary<string, string[]> assetbundleDictionary = new Dictionary<string, string[]>();
            var dataModel = AssetTreeManager.mIns.GetModel<AssetTreeData>();
            int total = 0;

            GenerateAlwaysShaders();

            if (AssetBundleConfig.IsFileterEnable())
            {
                for (int i = 0; i < AssetBundleConfig.FilterPaths.Length; i++)
                {
                    string filter = null;
                    if (i < AssetBundleConfig.FilterFiles.Length)
                    {
                        filter = AssetBundleConfig.FilterFiles[i];
                    }
                    else
                    {
                        Debug.Log("use Default Filter >> " + AssetBundleConfig.FilterPaths[i]);
                        filter = AssetBundleConfig.FilterFiles[0];
                    }

                    var result = EditorTools.FindAllAsset(filter, AssetBundleConfig.FilterPaths[i]);

                    int subIndex = 0;
                    foreach (var assetpath in result)
                    {
                        AssetImporter importer = AssetImporter.GetAtPath(assetpath);
                        if (importer != null && !string.IsNullOrEmpty(importer.assetBundleName) && !assetbundleDictionary.ContainsKey(importer.assetBundleName))
                        {
                            var assetbundlePaths = AssetDatabase.GetAssetPathsFromAssetBundle(importer.assetBundleName);
                            assetbundleDictionary[importer.assetBundleName] = assetbundlePaths;

                            yield return DisplayProgressBar("Asset-Parse AssetBundle", "Add AssetBundleName", (float)subIndex / result.Count);
                            total += assetbundlePaths.Length;
                            yield return null;
                        }
                        subIndex++;
                    }
                }
            }
            else if (AssetBundleConfig.ParseConfig == AssetParseConfig.All)
            {
                var allassetnames = AssetDatabase.GetAllAssetBundleNames();
                for (int i = 0; i < allassetnames.Length; i++)
                {
                    var assetbundlename = allassetnames[i];
                    var assetpaths = AssetDatabase.GetAssetPathsFromAssetBundle(assetbundlename);
                    assetbundleDictionary[assetbundlename] = assetpaths;
                    total += assetpaths.Length;

                    yield return DisplayProgressBar("Asset-Parse AssetBundle", "Add AssetBundleName", (float)i / allassetnames.Length);
                }
            }

            if (dataModel != null)
            {
                yield return ParseAssetBundle(dataModel, assetbundleDictionary, total);
            }

            Inited = true;

            //gc
            System.GC.Collect();
        }

        IEnumerator DisplayProgressBar(string title, string info, float progress)
        {
#if !UNITY_EDITOR_OSX
            if (EditorUtility.DisplayCancelableProgressBar(title, info, progress))
            {
                EditorContexts.mIns.CancelProgress();
                yield break;
            }
#else
            yield return null;
#endif
        }

        IEnumerator ParseAssetBundle(AssetTreeModel<AssetTreeData> dataModel, Dictionary<string, string[]> assetbundleDictionary, int total)
        {
            //create root
            AssetTreeData rootData = new AssetTreeData();
            rootData.Id = AssetTreeManager.mIns.GetUniqueId();
            dataModel.Root = rootData;
            dataModel.Add(ref rootData);

            //Editor Ui
            int progress = 0;

            float totalprogress = total + 2;
            yield return DisplayProgressBar("Asset-Parse AssetBundle", "Start Parseing", 0f);

            foreach (var assetbundleinfo in assetbundleDictionary)
            {
                var assetbundlename = assetbundleinfo.Key;
                var assetpaths = assetbundleinfo.Value;

                if (assetpaths.Length > 0)
                {
                    AssetTreeData folderData = CreateBaseAssetBundle(assetpaths, assetbundlename, dataModel);
                    folderData.IsAssetBundleViewData = true;

                    foreach (var assetpath in assetpaths)
                    {
                        CreateSubAssetBundle(dataModel, folderData.Id, assetpath, assetbundlename);
                        //Editor Ui
                        progress++;
                        if (progress % AssetWindowConfig.ParseStep == 0)
                        {
                            yield return DisplayProgressBar("Asset-Parse AssetBundle", "Parseing " + assetpath, progress / totalprogress);
                        }
                    }
                }
            }

            yield return DisplayProgressBar("Asset-Parse AssetBundle", "Set Dependency", (progress + 1) / totalprogress);

            List<AssetTreeData> nonamelist = new List<AssetTreeData>();
            Stack<AssetTreeData> itemStack = StackPool<AssetTreeData>.Get();
            var allTreeDatas = dataModel.GetAllItems();

            //set dependency references
            for (int i = 0; i < allTreeDatas.Count; i++)
            {
                var info = allTreeDatas[i];
                itemStack.Push(info);
                //SetAssetDependRef(ref info,dataModel,nonamelist);
            }

            ListPool<AssetTreeData>.Release(allTreeDatas);

            yield return SetAssetDependRef(itemStack, dataModel, nonamelist);
            StackPool<AssetTreeData>.Release(itemStack);

            yield return DisplayProgressBar("Asset-Parse AssetBundle", "Assigning NoAssetName", 1f);

            for (int i = 0; i < nonamelist.Count; i++)
            {
                var nonameitem = nonamelist[i];

                var deplist = dataModel.GetDependParents(nonameitem.FilePath);
                for (int j = 0; j < deplist.Count; j++)
                {
                    var dep = deplist[j];
                    if (j == 0)
                    {
                        dataModel.AddChild(dep.Id, nonameitem.Id);
                    }
                    else
                    {
                        nonameitem.Id = AssetTreeManager.mIns.GetUniqueId();
                        dataModel.Add(ref nonameitem);
                        //dataModel.AddViewData(ref nonameitem);
                        dataModel.AddChild(dep.Id, nonameitem.Id);
                    }

                }
            }

            yield return null;
            EditorUtility.ClearProgressBar();
        }

        IEnumerator SetAssetDependRef(Stack<AssetTreeData> stack, AssetTreeModel<AssetTreeData> dataModel, List<AssetTreeData> nonamelist)
        {
            while (stack.Count > 0)
            {

                var info = stack.Pop();

                yield return DisplayProgressBar("Asset-Parse AssetBundle", "Set Dependency :" + info.DisplayName, 1f / (stack.Count + 1));

                if (info.EditorInfo.RuntimeInfo.DependNames != null)
                {
                    List<AssetBundleInfo> infoList = ListPool<AssetBundleInfo>.Get();
                    for (int j = 0; j < info.EditorInfo.RuntimeInfo.DependNames.Length; j++)
                    {
                        string path = info.EditorInfo.RuntimeInfo.DependNames[j];

                        AssetTreeData assetdata;
                        if (dataModel.GetItem(path, out assetdata))
                        {
                            infoList.Add(assetdata.EditorInfo.RuntimeInfo);
                        }
                        else
                        {
                            AssetImporter importer = AssetImporter.GetAtPath(path);
                            if (importer != null && !string.IsNullOrEmpty(importer.assetBundleName))
                            {
                                AssetTreeData folderData;
                                if (!dataModel.GetItemByAssetBundleName(importer.assetBundleName, out folderData))
                                {
                                    string[] paths = AssetDatabase.GetAssetPathsFromAssetBundle(importer.assetBundleName);
                                    folderData = CreateBaseAssetBundle(paths, importer.assetBundleName, dataModel);

                                    foreach (var assetpath in paths)
                                    {
                                        var itemdata = CreateSubAssetBundle(dataModel, folderData.Id, assetpath, importer.assetBundleName);
                                        stack.Push(itemdata);
                                    }

                                }
                                infoList.Add(folderData.EditorInfo.RuntimeInfo);
                            }
                            else
                            {

                                bool needcreate = !IsResourceRes(path);
                                if (needcreate && path.EndsWith(".shader", StringComparison.OrdinalIgnoreCase))
                                {
                                    Shader shader = AssetDatabase.LoadAssetAtPath<Shader>(path);
                                    if (shader != null)
                                    {
                                        for (int i = 0; i < _alwayShaders.Count; i++)
                                        {
                                            var includeshader = _alwayShaders[i];
                                            if (includeshader != null && includeshader.name.Equals(shader.name))
                                            {
                                                needcreate = false;
                                                break;
                                            }
                                        }

                                    }
                                }

                                if (needcreate)
                                {
                                    Debug.LogWarningFormat("not found assetbundleInfo :{0}", path);

                                    //will pack into assetbundle
                                    assetdata = CreateNoAssetBundleNameAsset(path, info.EditorInfo.RuntimeInfo.AssetBundleName);
                                    dataModel.Add(ref assetdata);
                                    nonamelist.Add(assetdata);
                                    infoList.Add(assetdata.EditorInfo.RuntimeInfo);
                                }
                            }
                        }
                    }

                    //assign
                    var editordata = info.EditorInfo;

                    editordata.Dependencies = infoList.ToArray();

                    info.EditorInfo = editordata;

                    dataModel.Add(ref info);
                    ListPool<AssetBundleInfo>.Release(infoList);
                }
            }
        }

        bool IsResourceRes(string assetpath)
        {

            string dirname = Path.GetDirectoryName(assetpath);
            string[] sppath = dirname.Split('/');
            foreach (var spvalue in sppath)
            {
                if (string.Compare(spvalue, "resources", StringComparison.OrdinalIgnoreCase) == 0)
                {
                    return true;
                }
            }
            return false;
        }

        AssetTreeData CreateBaseAssetBundle(string[] assetpaths, string assetbundlename, AssetTreeModel<AssetTreeData> dataModel)
        {
            AssetTreeData folderData = new AssetTreeData();
            folderData.Id = AssetTreeManager.mIns.GetUniqueId();
            folderData.DisplayName = assetbundlename;
            folderData.IconName = AssetTreeManager.mIns.GetIconName("Folder");
            folderData.FilePath = Path.GetDirectoryName(assetpaths[0]);

            //set Folder
            var info = folderData.EditorInfo;
            var runtimeinfo = info.RuntimeInfo;
            runtimeinfo.UnityPath = folderData.FilePath;
            runtimeinfo.DependNames = assetpaths;

            runtimeinfo.DepAssetBundleNames = new string[assetpaths.Length];
            for (int i = 0; i < assetpaths.Length; i++)
            {
                var import = AssetImporter.GetAtPath(assetpaths[i]);
                if (import != null)
                {
                    runtimeinfo.DepAssetBundleNames[i] = import.assetBundleName;
                }
            }

            //hash
            if (EditorContexts.mIns.SystemContext.Manifests != null)
            {
                AssetBundleBuildInfo buildinfo;
                if (EditorContexts.mIns.SystemContext.Manifests.TryGetValue(assetbundlename, out buildinfo))
                {
                    runtimeinfo.HashCode = buildinfo.AssetBundleHash;
                    runtimeinfo.Crc = buildinfo.Crc;
                }
            }

            info.RuntimeInfo = runtimeinfo;
            folderData.EditorInfo = info;

            dataModel.AddChild(dataModel.Root.Id, folderData.Id);
            dataModel.Add(ref folderData);
            return folderData;
        }

        AssetTreeData CreateNoAssetBundleNameAsset(string path, string assetbundlename)
        {
            AssetTreeData data = new AssetTreeData();
            data.Id = AssetTreeManager.mIns.GetUniqueId();
            data.DisplayName = Path.GetFileNameWithoutExtension(path);
            data.FilePath = path;

            CreateAssetBundleInfo(ref data, path, assetbundlename);
            return data;
        }

        AssetTreeData CreateSubAssetBundle(AssetTreeModel<AssetTreeData> dataModel, int parentid, string assetpath, string assetbundlename)
        {
            AssetTreeData data = new AssetTreeData();
            data.Id = AssetTreeManager.mIns.GetUniqueId();

            CreateAssetBundleInfo(ref data, assetpath, assetbundlename);
            dataModel.Add(ref data);
            dataModel.AddChild(parentid, data.Id);
            return data;
        }

        protected void GenerateDependencies(List<string> collectList, List<string> dependenciesList, Object unityObject)
        {
            string assetpath = AssetDatabase.GetAssetPath(unityObject);
            var collectdepends = EditorUtility.CollectDependencies(new Object[] { unityObject });
            var editordepends = AssetDatabase.GetDependencies(assetpath, false);
            foreach (var collectdep in collectdepends)
            {
                string deppath = AssetDatabase.GetAssetPath(collectdep);
                if (!deppath.EndsWith(".cs", StringComparison.OrdinalIgnoreCase) && !deppath.EndsWith(".js", StringComparison.OrdinalIgnoreCase)
                    && !deppath.Contains("Library/unity default resources")
                    && !deppath.Contains("Resources/unity_builtin_extra")
                    && !deppath.EndsWith(".dll", StringComparison.OrdinalIgnoreCase)
                    && !deppath.Equals(assetpath)) //shader ?
                {
                    if (!collectList.Contains(deppath) && !string.IsNullOrEmpty(deppath))
                    {
                        collectList.Add(deppath);
                    }

                }
            }

            foreach (var deppath in editordepends)
            {
                if (!deppath.EndsWith(".cs", StringComparison.OrdinalIgnoreCase) && !deppath.EndsWith(".js", StringComparison.OrdinalIgnoreCase)
                    && !deppath.Contains("Library/unity default resources")
                    && !deppath.Contains("Resources/unity_builtin_extra")
                    && !deppath.EndsWith(".dll", StringComparison.OrdinalIgnoreCase)
                    && !deppath.Equals(assetpath)) //shader ?
                {
                    if (!dependenciesList.Contains(deppath))
                    {
                        dependenciesList.Add(deppath);
                    }

                }
            }
        }


        //default
        protected virtual void CreateAssetBundleInfo(ref AssetTreeData treeData, string assetpath, string assetbundlename)
        {

            AssetBundleInfo runtimeInfo = new AssetBundleInfo();
            EditorAssetBundleInfo assetBundleInfo = new EditorAssetBundleInfo();
            Object unityObject = AssetDatabase.LoadAssetAtPath<Object>(assetpath);
            assetBundleInfo.EditorInstanceId = unityObject.GetInstanceID();
            //assetbundle info

            runtimeInfo.AssetBundleName = assetbundlename;
            runtimeInfo.AssetName = Path.GetFileNameWithoutExtension(assetpath);
            runtimeInfo.UnityPath = assetpath;
            runtimeInfo.AssetResType = AssetBundleFunction.TypeToResType(assetpath);
            //size
            runtimeInfo.AssetSize = Profiler.GetRuntimeMemorySizeLong(unityObject);
            if (runtimeInfo.AssetResType == AssetBundleResType.Image)//editor 下双倍内存
            {
                runtimeInfo.AssetSize /= 2;
            }
            FileInfo fileInfo = new FileInfo(assetpath);
            runtimeInfo.FileSize = fileInfo.Length;

            //dependency
            List<string> collectList = ListPool<string>.Get();
            List<string> editordependList = ListPool<string>.Get();

            GenerateDependencies(collectList, editordependList, unityObject);

            //set dependencies
            runtimeInfo.DependNames = collectList.ToArray();
            runtimeInfo.DepAssetBundleNames = new string[runtimeInfo.DependNames.Length];
            for (int i = 0; i < runtimeInfo.DependNames.Length; i++)
            {
                var import = AssetImporter.GetAtPath(runtimeInfo.DependNames[i]);
                if (import != null)
                {
                    runtimeInfo.DepAssetBundleNames[i] = import.assetBundleName;
                }
            }

            assetBundleInfo.EditorDependencies = editordependList.ToArray();

            assetBundleInfo.RuntimeInfo = runtimeInfo;
            treeData.EditorInfo = assetBundleInfo;

            //set icon
            treeData.IconName = AssetTreeManager.mIns.GetIconName(runtimeInfo.AssetResType);
            treeData.FilePath = assetpath;
            treeData.DisplayName = Path.GetFileNameWithoutExtension(assetpath);


            ListPool<string>.Release(collectList);
            ListPool<string>.Release(editordependList);

        }

    }
}

#endif