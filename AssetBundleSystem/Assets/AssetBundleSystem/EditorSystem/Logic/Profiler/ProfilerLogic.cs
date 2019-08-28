#if UNITY_EDITOR
using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using CommonUtils;
using System.IO;
using System.Reflection;

namespace AssetBundleSystem.Editor
{
    internal class ProfilerLogic : ILogicInterface
    {
        public AssetWindow.WindowMode TypeMode
        {
            get
            {
                return AssetWindow.WindowMode.Profiler;
            }
        }

        public bool Inited { get; private set; }

        private FieldInfo _field;

        private MethodInfo _method;

        private AssetBundleFeature _feature;

        public void Clear()
        {
            Inited = false;
            AssetTreeManager.mIns.GetModel<RuntimeProfilerData>().Clear();
        }

        public IEnumerator ReLoad()
        {
            var datamodel = AssetTreeManager.mIns.GetModel<RuntimeProfilerData>();
            if (EditorContexts.mIns.GuiContext.UserSelectFrame >= 0 && AssetBundleLoadManager.mIns != null)
            {
                if (_feature == null)
                {
                    _field = typeof(AssetBundleLoadManager).GetField("_assetBundleFeature", BindingFlags.NonPublic | BindingFlags.Instance);
                    _method = typeof(AssetBundleFeature).GetMethod("StartRead", BindingFlags.NonPublic | BindingFlags.Instance);

                    _feature = _field.GetValue(AssetBundleLoadManager.mIns) as AssetBundleFeature;
                }

                if (_field != null && _method != null && _feature != null)
                {
                    AssetBundleFeature assetBundleFeature = _feature;
                    if (assetBundleFeature != null)
                    {
                        var context = (AssetBundleContext)_method.Invoke(assetBundleFeature, new object[] { }) ;
                        var profilerdata = context.ProfilerData;

                        if(profilerdata.Datas != null)
                        {

                            foreach (var item in profilerdata.Datas)
                            {
                                if(item.Frame == EditorContexts.mIns.GuiContext.UserSelectFrame)
                                {
                                    //Root
                                    RuntimeProfilerData rootdata = new RuntimeProfilerData();
                                    rootdata.Id = AssetTreeManager.mIns.GetUniqueId();
                                    rootdata.ProfilerData = item;

                                    datamodel.Root = rootdata;
                                    datamodel.Add(ref rootdata);
                                    //taskroot

                                    RuntimeProfilerData taskroot = new RuntimeProfilerData();
                                    taskroot.Id = AssetTreeManager.mIns.GetUniqueId();
                                    taskroot.DisplayName = "Tasks";
                                    taskroot.IconName = AssetTreeManager.mIns.GetIconName("Folder");
                                    datamodel.Add(ref taskroot);
                                    datamodel.AddChild(ref rootdata, ref taskroot);


                                    //bundleroot
                                    RuntimeProfilerData bundleroot = new RuntimeProfilerData();
                                    bundleroot.Id = AssetTreeManager.mIns.GetUniqueId();
                                    bundleroot.DisplayName = "AssetBundle";
                                    bundleroot.IconName = AssetTreeManager.mIns.GetIconName("Folder");
                                    datamodel.Add(ref bundleroot);
                                    datamodel.AddChild(ref rootdata, ref bundleroot);

                                    yield return AddProfilerData(datamodel, item, taskroot,bundleroot);

                                }
                            }
                        }
                    }
                    else
                    {
                        Debug.LogError("assetbundlefeature is Null");
                    }
                }

                Inited = true;
            }
        }

        IEnumerator AddProfilerData(AssetTreeModel<RuntimeProfilerData> datamodel, FrameProfilerData data, RuntimeProfilerData taskroot, RuntimeProfilerData bundleroot)
        {
            int step = 5;
            if(data.Tasks != null)
            {
                for (int i = 0; i < data.Tasks.Count; i++)
                {
                    var task = data.Tasks[i];

                    RuntimeProfilerData folderdata = new RuntimeProfilerData();
                    folderdata.Id = AssetTreeManager.mIns.GetUniqueId();
                    folderdata.DisplayName = Path.GetFileNameWithoutExtension(task.AssetPath);
                    folderdata.IconName = AssetTreeManager.mIns.GetIconName("Folder");
                    folderdata.Task = task;

                    datamodel.Add(ref folderdata);
                    datamodel.AddChild(ref taskroot, ref folderdata);

                    if(i % step == 0)
                    {
                        yield return null; 
                    }
                }
            }


            if(data.Histories != null)
            {
                for (int i = 0; i < data.Histories.Count; i++)
                {
                    var history = data.Histories[i];

                    RuntimeProfilerData folderdata = new RuntimeProfilerData();
                    folderdata.Id = AssetTreeManager.mIns.GetUniqueId();
                    folderdata.DisplayName = history.AssetBundlename;
                    folderdata.IconName = AssetTreeManager.mIns.GetIconName("Folder");
                    folderdata.History = history;

                    datamodel.Add(ref folderdata);
                    datamodel.AddChild(ref bundleroot, ref folderdata);

                    if (history.AssetsList != null)
                    {
                        foreach(var sub in history.AssetsList)
                        {
                            RuntimeProfilerData subdata = new RuntimeProfilerData();
                            subdata.Id = AssetTreeManager.mIns.GetUniqueId();
                            subdata.DisplayName = Path.GetFileNameWithoutExtension(sub.Path);
                            subdata.IconName = AssetTreeManager.mIns.GetIconName("GameObject");
                            subdata.AssetInfo = sub;

                            datamodel.Add(ref subdata);
                            datamodel.AddChild(ref folderdata, ref subdata);

                        }
                    }

                    if (i % step == 0)
                    {
                        yield return null;
                    }
                }
            }

            yield return null;
        }
    }
}

#endif