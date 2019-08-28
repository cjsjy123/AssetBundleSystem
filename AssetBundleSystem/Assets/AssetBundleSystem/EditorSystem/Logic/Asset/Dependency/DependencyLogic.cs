#if UNITY_EDITOR
using UnityEngine;
using System.Collections;
using UnityEditor;
using System.IO;
using CommonUtils.Editor;

namespace AssetBundleSystem.Editor
{
    internal class DependencyLogic : ILogicInterface
    {
        public AssetWindow.WindowMode TypeMode { get { return AssetWindow.WindowMode.AssetDependency; } }
        public bool Inited { get; private set; }

        private int _progress;
        public void Clear()
        {
            Inited = false;
            AssetTreeManager.mIns.GetModel<DependencyTreeData>().Clear();
        }

        public IEnumerator ReLoad()
        {
            var datamodel = AssetTreeManager.mIns.GetModel<DependencyTreeData>();
            var treemodel = AssetTreeManager.mIns.GetModel<AssetTreeData>();
            if (datamodel != null && treemodel != null)
            {
                AssetTreeData data ;
                if (treemodel.GetItem(EditorContexts.mIns.SelectForDependencyData.FilePath, out data))
                {
                    yield return ParseSelect(datamodel, treemodel, data);
                }
            }
        }

        IEnumerator ParseSelect(AssetTreeModel<DependencyTreeData> datamodel, AssetTreeModel<AssetTreeData> treemodel, AssetTreeData data)
        {
#if !UNITY_EDITOR_OSX
            yield return DisplayProgressBar("Dependency-Parse","Parse Dependency for :"+ data.FilePath,0f);
#endif

            DependencyTreeData root = new DependencyTreeData();
            root.Id = AssetTreeManager.mIns.GetUniqueId();
            root.DisplayName = "root";

            //empty;
            datamodel.Root = root;
            datamodel.Add(ref root);

            _progress = 0;
            if (EditorContexts.mIns.GuiContext.SelectDepth == 0)
            {
                foreach (var dependency in data.EditorInfo.Dependencies)
                {
                    DependencyTreeData depdata = new DependencyTreeData();
                    depdata.Id = AssetTreeManager.mIns.GetUniqueId();
                    depdata.IsAssetBundleViewData = true;
                    CreateDependencyInfo(ref depdata, dependency);


                    datamodel.Add(ref depdata);
                    yield return CreateDepParent(datamodel, treemodel, depdata.FilePath, depdata.Id);
                    datamodel.AddChild(ref root, ref depdata);
                }
            }
            else
            {
                yield return CreateDepParent(datamodel, treemodel, data.FilePath, root.Id);
            }

            yield return null;

            Inited = true;
            EditorUtility.ClearProgressBar();
        }

        IEnumerator CreateDepParent(AssetTreeModel<DependencyTreeData> datamodel, AssetTreeModel<AssetTreeData> treemodel,string filepath,int parentId)
        {
            var dependonlist = treemodel.GetDependParents(filepath);
            foreach (var element in dependonlist)
            {
                DependencyTreeData subdata = new DependencyTreeData();
                Convert(ref subdata, element);
                datamodel.Add(ref subdata);

                datamodel.AddChild(parentId, subdata.Id);
                _progress++;
                if (_progress % AssetWindowConfig.ParseStep == 0)
                {
#if !UNITY_EDITOR_OSX
                    yield return DisplayProgressBar("Dependency-Parse", "Parse Dependency for :" + element.FilePath, 0);
#endif
                }
            }
        }

        protected virtual void Convert(ref DependencyTreeData dpdata, AssetTreeData treeData)
        {
            dpdata.Id = AssetTreeManager.mIns.GetUniqueId();
            dpdata.FilePath = treeData.FilePath;
            dpdata.IconName = treeData.IconName;
            dpdata.EditorInfo = treeData.EditorInfo;
            dpdata.DisplayName = treeData.DisplayName;
        }

        //default
        protected virtual void CreateDependencyInfo(ref DependencyTreeData treeData, AssetBundleInfo info)
        {
            EditorAssetBundleInfo assetBundleInfo = treeData.EditorInfo;
            assetBundleInfo.RuntimeInfo = info;

            //Object unityObject = AssetDatabase.LoadAssetAtPath<Object>(info.UnityPath);
            //assetBundleInfo.UnityObject = unityObject;
            //assetBundleInfo.TryBuildRefInfo();

            treeData.EditorInfo = assetBundleInfo;

            //set icon
            treeData.IconName = AssetTreeManager.mIns.GetIconName(info.AssetResType);
            treeData.FilePath = info.UnityPath;
            treeData.DisplayName = Path.GetFileNameWithoutExtension(info.UnityPath);
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
    }
}

#endif
