#if UNITY_EDITOR
using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using CommonUtils;
using UnityEditor;

namespace  AssetBundleSystem.Editor
{
    //public struct EditorRef
    //{
    //    public int instanceId;

    //    public string instancePath;
    //}

    internal struct EditorAssetBundleInfo
    {
        /// <summary>
        /// 运行时assetbundle info
        /// </summary>
        public AssetBundleInfo RuntimeInfo;

        //private Object _unityobject;

        public int EditorInstanceId;

        ///// <summary>
        ///// Editor Object
        ///// </summary>
        //public Object UnityObject
        //{
        //    get
        //    {
        //        if (_unityobject == null)
        //            _unityobject = AssetDatabase.LoadAssetAtPath<Object>(RuntimeInfo.UnityPath);
        //        return UnityObject;
        //    }
        //}
        /// <summary>
        /// 编辑器依赖 -用来排查meta残留
        /// </summary>
        public string[] EditorDependencies;


        //public EditorRef[] EditorRefInfo;

        #region Editor 
        /// <summary>
        /// 依赖
        /// </summary>
        public AssetBundleInfo[] Dependencies;


        public int DependenciesRefCount
        {
            get
            {
                if(Dependencies == null)
                {
                    return 0;
                }
                return Dependencies.Length;
            }
        }

        public long GetAllAssetSize()
        {
            long assetsize = RuntimeInfo.AssetSize;

            for (int i = 0; i < DependenciesRefCount; i++)
            {
                var dep = Dependencies[i];
                assetsize += dep.AssetSize;
            }
            return assetsize;
        }

        public long GetAllFileSize()
        {
            long filesize = RuntimeInfo.FileSize;

            for (int i = 0; i < DependenciesRefCount; i++)
            {
                filesize += Dependencies[i].FileSize;
            }
            return filesize;
        }

        #endregion

        //public bool TryBuildRefInfo()
        //{
        //    if (!string.IsNullOrEmpty(RuntimeInfo.UnityPath) && UnityObject != null)
        //    {
        //        SerializedObject serializedObject = new SerializedObject(UnityObject);

        //        List<EditorRef> list = ListPool<EditorRef>.Get();

        //        var it = serializedObject.GetIterator();
        //        while (it.NextVisible(true))
        //        {
        //            if (it.propertyType == SerializedPropertyType.ObjectReference &&
        //                it.objectReferenceValue == UnityObject)
        //            {
        //                EditorRef data = new EditorRef();
        //                data.instanceId = it.objectReferenceInstanceIDValue;
        //                data.instancePath = it.propertyPath;
        //                list.Add(data);
        //            }
        //        }

        //        this.EditorRefInfo = list.ToArray();
        //        ListPool<EditorRef>.Release(list);
        //        return true;
        //    }
        //    return false;
        //}
    }

}
#endif
