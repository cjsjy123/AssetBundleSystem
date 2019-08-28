#if UNITY_EDITOR
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using CommonUtils;
using CommonUtils.Editor;
using UnityEditor;
using UnityEngine;
using UnityEngine.Profiling;
using UnityEngine.UI;
using Object = UnityEngine.Object;

namespace AssetBundleSystem.Editor
{
    class SpriteLogic : ILogicInterface
    {

        public AssetWindow.WindowMode TypeMode
        {
            get { return AssetWindow.WindowMode.ToolsSprite; }
        }

        private const string SelectText = "√";

        public const string BuildInTag = "Unity BuiltIn";
        public const string ResourcesTag = "Unity Resources";

        public bool Inited { get; private set; }

        public void Clear()
        {
            Inited = false;
            AssetTreeManager.mIns.GetModel<SpriteTrackData>().Clear();
        }

        public IEnumerator ReLoad()
        {
            var dataModel = AssetTreeManager.mIns.GetModel<SpriteTrackData>();
            //create root
            SpriteTrackData rootData = new SpriteTrackData();
            rootData.Id = AssetTreeManager.mIns.GetUniqueId();
            dataModel.Root = rootData;
            dataModel.Add(ref rootData);

            BuildUseablePaths();

            Dictionary<Sprite, SimpleSpriteInfo> sprCache = new Dictionary<Sprite, SimpleSpriteInfo>();
            Dictionary<Sprite, HashSet<Object>> allUsed = new Dictionary<Sprite, HashSet<Object>>();

            yield return CollectSceneInfo(dataModel, allUsed, sprCache);
            yield return CollectPrefabsInfo(dataModel, allUsed, sprCache);
            yield return CollectAtlasInfo(dataModel, allUsed, sprCache);

            Inited = true;
        }

        void BuildUseablePaths()
        {

            HashSet<string> pathHashSet = new HashSet<string>();
            pathHashSet.Add("Assets");

            string[] guids = AssetDatabase.FindAssets("t:GameObject", new string[] { "Assets" });

            foreach (var guid in guids)
            {
                string assetpath = AssetDatabase.GUIDToAssetPath(guid);

                AddPathRecursive(pathHashSet, assetpath);
                //build Tree?
            }

            Dictionary<string, bool> cache = new Dictionary<string, bool>(EditorContexts.mIns.GuiContext.SelectPaths);

            //
            EditorContexts.mIns.GuiContext.SelectPaths.Clear();
            EditorContexts.mIns.GuiContext.UsablePaths.Clear();

            foreach (var path in pathHashSet)
            {
                EditorContexts.mIns.GuiContext.UsablePaths.Add(path);

                bool toggle;
                if (cache.TryGetValue(path,out toggle))
                {
                    EditorContexts.mIns.GuiContext.SelectPaths[path] = toggle;
                }
                else
                {
                    EditorContexts.mIns.GuiContext.SelectPaths[path] = false;
                }
            }

            //replace
            EditorContexts.mIns.GuiContext.Menu = new GenericMenu();

            foreach (var pair in EditorContexts.mIns.GuiContext.SelectPaths)
            {
                AddItem(EditorContexts.mIns.GuiContext.Menu, pair.Key, pair.Value);
            }
        }

        void AddItem(GenericMenu menu, string path,bool toggle)
        {
            GUIContent guiContent = new GUIContent(path);
            menu.AddItem(guiContent, toggle, Toggle, guiContent);
        }

        void Toggle(object value)
        {
            GUIContent content =value as GUIContent;
            if (content != null)
            {
                string info = content.text;
                info = info.Replace(SelectText, "");

                bool show;
                EditorContexts.mIns.GuiContext.SelectPaths.TryGetValue(info, out show);
                //refresh
                show = !show;
                EditorContexts.mIns.GuiContext.SelectPaths[info] = show;

                if (show)
                {
                    content.text = string.Format("{0} {1}", SelectText, info);
                }
                else
                {
                    content.text = info;
                }

                EditorContexts.mIns.ForceModeChange(AssetWindow.WindowMode.ToolsSprite);
            }
        }

        void AddPathRecursive(HashSet<string> hashset, string path)
        {
            if (path != Application.dataPath)
            {
                path = path.Replace('\\', '/');
                string subpath = path;
                Stack<string> stack =  StackPool<string>.Get();
     
                while(true)
                {
                    int index = subpath.LastIndexOf('/');
                    if (index >= 0)
                    {
                        subpath = path.Substring(0, index);
                        stack.Push(subpath);
                    }
                    else
                    {
                        subpath = null;
                    }

                    if (string.IsNullOrEmpty(subpath) || subpath == Application.dataPath)
                    {
                        break;
                    }
                }
                

                while (stack.Count >0)
                {
                    string result = stack.Pop();
                    
                    hashset.Add(result);
                }

                StackPool<string>.Release(stack);
            }
        }

        SpriteTrackData CreateDisplayRoot(string name)
        {
            SpriteTrackData rootData = new SpriteTrackData();
            rootData.Id = AssetTreeManager.mIns.GetUniqueId();
            rootData.IconName = AssetTreeManager.mIns.GetIconName(AssetBundleResType.Folder);
            rootData.DisplayName = name;
            return rootData;
        }

        IEnumerator CollectSceneInfo(AssetTreeModel<SpriteTrackData> dataModel, Dictionary<Sprite, HashSet<Object>> allUsed, Dictionary<Sprite, SimpleSpriteInfo> sprCache)
        {
            var displayRoot = CreateDisplayRoot("Scene");
            displayRoot.ShowMode = SpriteShowMode.Scene;
            dataModel.Add(ref displayRoot);
            dataModel.AddChild(dataModel.Root.Id, displayRoot.Id);

            var allScripts = GameObject.FindObjectsOfType<MonoBehaviour>();

            Dictionary<Sprite, HashSet<Object>> allRecords = new Dictionary<Sprite, HashSet<Object>>();

            int i = 0;
            foreach (MonoBehaviour script in allScripts)
            {
                CollectSprite(script, allRecords);

                yield return DisplayProgressBar("Parse Scene", "Parsing script " + script.name, (float)i / allScripts.Length);
                i++;
            }

            foreach (var pair in allRecords)
            {
                allUsed[pair.Key] = pair.Value;//new HashSet<Object>(pair.Value);
            }

            i = 0;

            Dictionary<Object, List<Sprite>> sprRefDictionary = Convert(allRecords);
            foreach (var pair in sprRefDictionary)
            {
                SpriteTrackData sprData = new SpriteTrackData();
                sprData.ShowMode = SpriteShowMode.Scene;
                sprData.Id = AssetTreeManager.mIns.GetUniqueId();
                sprData.DisplayName = pair.Key.name;
                sprData.IconName = AssetTreeManager.mIns.GetIconName(AssetBundleResType.Cs);
                sprData.SceneData.CsReferences = pair.Key as MonoBehaviour;
                sprData.SceneData.InstanceId = pair.Key.GetInstanceID();
                sprData.SceneData.MemSize = AssetBundleEditorHelper.ConvertSize(Profiler.GetRuntimeMemorySizeLong(pair.Key));


                if (sprData.SceneData.CsReferences)
                {
                    sprData.SceneData.Path = GetPath(sprData.SceneData.CsReferences.gameObject);
                }

                dataModel.Add(ref sprData);

                HashSet<string> hashset = HashSetPool<string>.Get();

                foreach (var refsprite in pair.Value)
                {
                    SpriteTrackData refData = BuildData(refsprite, pair.Key, SpriteShowMode.Scene, sprCache);
                    refData.UsedRefCount = 1;

                    dataModel.Add(ref refData);
                    dataModel.AddChild(ref sprData, ref refData);

                    hashset.Add(refData.SceneData.SprData.PackingTag);
                }

                sprData.UsedRefCount = hashset.Count;
                //refresh
                dataModel.Add(ref sprData);

                HashSetPool<string>.Release(hashset);
                dataModel.AddChild(ref displayRoot, ref sprData);

                yield return DisplayProgressBar("Parse Scene", "Parsing  Sprite", (float)i / sprRefDictionary.Count);
                i++;
            }
        }

        IEnumerator CollectPrefabsInfo(AssetTreeModel<SpriteTrackData> dataModel, Dictionary<Sprite, HashSet<Object>> allUsed, Dictionary<Sprite, SimpleSpriteInfo> sprCache)
        {
            var displayRoot = CreateDisplayRoot("Prefabs");
            displayRoot.ShowMode = SpriteShowMode.Prefabs;
            dataModel.Add(ref displayRoot);
            dataModel.AddChild(dataModel.Root.Id, displayRoot.Id);
            yield return DisplayProgressBar("Parse Prefab", "Start Parse " , 0);

            if (EditorContexts.mIns.GuiContext.SelectPaths != null && EditorContexts.mIns.GuiContext.SelectPaths.Count > 0)
            {
                List<string> selects = ListPool<string>.Get();

                foreach (var pair in EditorContexts.mIns.GuiContext.SelectPaths)
                {
                    if (pair.Value)
                    {
                        selects.Add(pair.Key);
                    }
                }

                if (selects.Count == 0)
                {
                    ListPool<string>.Release(selects);
                }
                else
                {
                    string[] guids = AssetDatabase.FindAssets("t:GameObject", selects.ToArray());
                    ListPool<string>.Release(selects);

                    yield return DisplayProgressBar("Parse Prefab", "Start Parse Select Path", 0.05f);
                    HashSet<string> pathHashSet = new HashSet<string>();

                    for (int i = 0; i < guids.Length; i++)
                    {
                        string assetpath = AssetDatabase.GUIDToAssetPath(guids[i]);
                        pathHashSet.Add(assetpath);
                        yield return DisplayProgressBar("Parse Prefab", "Add Path " + assetpath, (float)i / guids.Length);

                    }

                    int m = 0;
                    foreach (var assetpath in pathHashSet)
                    {
                        SpriteTrackData sprData = BuildGoData(dataModel, allUsed, sprCache, assetpath);

                        dataModel.Add(ref sprData);
                        dataModel.AddChild(ref displayRoot, ref sprData);

                        yield return DisplayProgressBar("Parse Prefab", "Parse " + assetpath, (float)m / guids.Length);
                        m++;
                    }
                }

            }
        }

        SpriteTrackData BuildGoData(AssetTreeModel<SpriteTrackData> dataModel, Dictionary<Sprite, HashSet<Object>> allUsed, Dictionary<Sprite, SimpleSpriteInfo> sprCache, string assetpath)
        {
            GameObject gameObject = AssetDatabase.LoadAssetAtPath<GameObject>(assetpath);
            if (gameObject)
            {
                HashSet<string> allHashSet = HashSetPool<string>.Get();

                SpriteTrackData data = new SpriteTrackData();
                data.ShowMode = SpriteShowMode.Prefabs;
                data.Id = AssetTreeManager.mIns.GetUniqueId();
                data.IconName = AssetTreeManager.mIns.GetIconName(AssetBundleResType.GameObject);
                data.DisplayName = Path.GetFileNameWithoutExtension(assetpath);

                data.GameObjectData.GoData = GetGoData(gameObject);

                string bundleName = "";
                AssetImporter importer = AssetImporter.GetAtPath(assetpath);
                if (importer)
                {
                    bundleName = importer.assetBundleName;
                }

                data.GameObjectData.SprData.BundleName = bundleName;
                data.GameObjectData.SprData.AssetPath = assetpath;

                var allScripts = gameObject.GetComponentsInChildren<MonoBehaviour>(true);
                Dictionary<Sprite, HashSet<Object>> allRecords = new Dictionary<Sprite, HashSet<Object>>();

                foreach (MonoBehaviour script in allScripts)
                {
                    CollectSprite(script, allRecords);
                }

                foreach (var pair in allRecords)
                {
                    HashSet<Object> hashset;
                    if (allUsed.TryGetValue(pair.Key, out hashset))
                    {
                        foreach (var subdata in pair.Value)
                        {
                            hashset.Add(subdata);
                        }
                    }
                    else
                    {
                        allUsed[pair.Key] = pair.Value;//new HashSet<Object>(pair.Value);
                    }
                }

                HashSet<string> atlasHashset = HashSetPool<string>.Get();
                Dictionary<Object, List<Sprite>> sprRefDictionary = Convert(allRecords);
                foreach (var pair in sprRefDictionary)
                {
                    atlasHashset.Clear();

                    SpriteTrackData sprData = new SpriteTrackData();
                    sprData.ShowMode = SpriteShowMode.Prefabs;
                    sprData.Id = AssetTreeManager.mIns.GetUniqueId();
                    sprData.DisplayName = MonoScript.FromMonoBehaviour(pair.Key as MonoBehaviour).name;
                    sprData.IconName = AssetTreeManager.mIns.GetIconName(AssetBundleResType.Cs);
                    sprData.GameObjectData.GoData = GetGoData(pair.Key);
                    sprData.GameObjectData.SprData.BundleName = bundleName;
                    sprData.GameObjectData.SprData.AssetPath = AssetDatabase.GetAssetPath(pair.Key);

                    dataModel.Add(ref sprData);

                    foreach (var refsprite in pair.Value)
                    {
                        SpriteTrackData refData = BuildData(refsprite, pair.Key, SpriteShowMode.Prefabs, sprCache);
                        refData.UsedRefCount = 1;

                        dataModel.Add(ref refData);
                        dataModel.AddChild(ref sprData, ref refData);

                        atlasHashset.Add(refData.GameObjectData.SprData.PackingTag);
                        allHashSet.Add(refData.GameObjectData.SprData.PackingTag);
                    }

                    sprData.UsedRefCount = atlasHashset.Count;
                    //refresh
                    dataModel.Add(ref sprData);
                    //
                    dataModel.AddChild(ref data, ref sprData);
                }

                data.UsedRefCount = allHashSet.Count;
                //refresh
                dataModel.Add(ref data);

                HashSetPool<string>.Release(atlasHashset);
                HashSetPool<string>.Release(allHashSet);

                return data;
            }
            else
            {
                SpriteTrackData data = new SpriteTrackData();
                data.ShowMode = SpriteShowMode.Prefabs;
                data.Id = AssetTreeManager.mIns.GetUniqueId();
                data.IconName = AssetTreeManager.mIns.GetIconName(AssetBundleResType.Prefab);
                data.DisplayName = "Load Failed";

                return data;
            }
        }

        IEnumerator CollectAtlasInfo(AssetTreeModel<SpriteTrackData> dataModel, Dictionary<Sprite, HashSet<Object>> allUsed, Dictionary<Sprite, SimpleSpriteInfo> sprCache)
        {
            var displayRoot = CreateDisplayRoot("Atlas");
            displayRoot.ShowMode = SpriteShowMode.Atlas;
            dataModel.Add(ref displayRoot);
            dataModel.AddChild(dataModel.Root.Id, displayRoot.Id);

            Dictionary<string, Dictionary<Sprite, HashSet<Object>>> dict = GetAtlasInfo(allUsed, sprCache);

            int i = 0;
            foreach (var pair in dict)
            {
                SpriteTrackData sprData = new SpriteTrackData();
                sprData.ShowMode = SpriteShowMode.Atlas;
                sprData.Id = AssetTreeManager.mIns.GetUniqueId();
                sprData.DisplayName = pair.Key;
                sprData.IconName = AssetTreeManager.mIns.GetIconName(AssetBundleResType.Image);

                dataModel.Add(ref sprData);
                dataModel.AddChild(ref displayRoot, ref sprData);


                BuildAtlasSprData(dataModel, pair.Value, sprCache, ref sprData);

                yield return DisplayProgressBar("Parse Atlas", "Parsing " + pair.Key, (float)i / dict.Count);
                i++;
            }
        }

        void BuildAtlasSprData(AssetTreeModel<SpriteTrackData> dataModel, Dictionary<Sprite, HashSet<Object>> dict, Dictionary<Sprite, SimpleSpriteInfo> sprCache, ref SpriteTrackData displayRoot)
        {
            foreach (var pair in dict)
            {
                SpriteTrackData rootData = new SpriteTrackData();
                rootData.ShowMode = SpriteShowMode.Atlas;
                rootData.Id = AssetTreeManager.mIns.GetUniqueId();
                rootData.IconName = AssetTreeManager.mIns.GetIconName(AssetBundleResType.Sprite);
                rootData.DisplayName = pair.Key.name;
                rootData.AtlasData.RefData.Target = pair.Key;
                rootData.AtlasData.RefData.InstanceId = pair.Key.GetInstanceID();

                rootData.AtlasData.SprData = GetSpriteData(sprCache, pair.Key);

                dataModel.Add(ref rootData);

                foreach (var refTarget in pair.Value)
                {
                    SpriteTrackData refData = BuildData(pair.Key, refTarget, SpriteShowMode.Atlas, sprCache);
                    dataModel.Add(ref refData);
                    dataModel.AddChild(ref rootData, ref refData);
                }

                dataModel.AddChild(ref displayRoot, ref rootData);
            }

        }

        Dictionary<string, Dictionary<Sprite, HashSet<Object>>> GetAtlasInfo(Dictionary<Sprite, HashSet<Object>> allUsed, Dictionary<Sprite, SimpleSpriteInfo> sprCache)
        {
            Dictionary<string, Dictionary<Sprite, HashSet<Object>>> infoDictionary = new Dictionary<string, Dictionary<Sprite, HashSet<Object>>>();

            foreach (var pair in allUsed)
            {
                Sprite spr = pair.Key;
                SimpleSpriteInfo sprData = sprCache[spr];

                Dictionary<Sprite, HashSet<Object>> dict;
                if (!infoDictionary.TryGetValue(sprData.PackingTag, out dict))
                {
                    dict = new Dictionary<Sprite, HashSet<Object>>();
                    infoDictionary.Add(sprData.PackingTag, dict);
                }

                //
                HashSet<Object> references;
                if (!dict.TryGetValue(spr, out references))
                {
                    references = pair.Value;
                    dict.Add(spr, references);
                }
                else
                {
                    Debug.LogError("Dupliate Sprite Data ", spr);
                }
            }

            return infoDictionary;
        }

        SpriteTrackData BuildData(Sprite key, Object reference, SpriteShowMode mode, Dictionary<Sprite, SimpleSpriteInfo> sprCache)
        {
            SpriteTrackData data = new SpriteTrackData();
            data.ShowMode = mode;
            data.IconName = AssetTreeManager.mIns.GetIconName(AssetBundleResType.Sprite);
            data.Id = AssetTreeManager.mIns.GetUniqueId();
            data.FilePath = AssetDatabase.GetAssetPath(key);
            data.DisplayName = key ? key.name : "Null Sprite";

            if (mode == SpriteShowMode.Scene)
            {
                data.SceneData.SprData = GetSpriteData(sprCache, key);
                data.SceneData.CsReferences = reference as MonoBehaviour;
                if (data.SceneData.CsReferences)
                {
                    data.SceneData.Path = GetPath(data.SceneData.CsReferences.gameObject);
                }
                data.SceneData.InstanceId = reference.GetInstanceID();
                data.SceneData.MemSize = AssetBundleEditorHelper.ConvertSize(Profiler.GetRuntimeMemorySizeLong(reference));
            }
            else if (mode == SpriteShowMode.Prefabs)
            {
                data.GameObjectData.SprData = GetSpriteData(sprCache, key);
                data.GameObjectData.GoData = GetGoData(reference);
            }
            else if (mode == SpriteShowMode.Atlas)
            {

                if (reference is GameObject)
                {
                    data.DisplayName = reference ? reference.name : "Null";
                    data.IconName = AssetTreeManager.mIns.GetIconName(AssetBundleResType.GameObject);
                    data.AtlasData.RefData.Path = GetPath(((GameObject)reference));
                }
                else if (reference is MonoBehaviour)
                {
                    data.IconName = AssetTreeManager.mIns.GetIconName(AssetBundleResType.Cs);
                    data.DisplayName = reference ? MonoScript.FromMonoBehaviour(reference as MonoBehaviour).name : "Null";
                    data.AtlasData.RefData.Path = AssetDatabase.GetAssetPath(MonoScript.FromMonoBehaviour((MonoBehaviour)reference));
                }
                else
                {
                    data.DisplayName = reference ? reference.name : "Null";
                    data.IconName = AssetTreeManager.mIns.GetIconName(AssetBundleResType.Folder);
                }

                data.AtlasData.RefData.Target = reference;
                data.AtlasData.RefData.InstanceId = reference.GetInstanceID();
                data.AtlasData.RefData.MemSize = AssetBundleEditorHelper.ConvertSize(Profiler.GetRuntimeMemorySizeLong(reference));


                data.AtlasData.SprData = GetSpriteData(sprCache, key);
            }

            return data;
        }


        SimpleGoData GetGoData(Object target)
        {
            GameObject gameObject = null;
            if (target is GameObject)
            {
                gameObject = target as GameObject;
            }
            else if (target is MonoBehaviour)
            {
                gameObject = (target as MonoBehaviour).gameObject;
            }

            SimpleGoData data = new SimpleGoData();
            data.Go = gameObject;
            data.InstanceId = target.GetInstanceID();
            data.AssetPath = AssetDatabase.GetAssetPath(gameObject);
            data.Path = GetPath(gameObject);
            data.PrefabType = PrefabUtility.GetPrefabType(gameObject);
            data.MemSize = AssetBundleEditorHelper.ConvertSize(Profiler.GetRuntimeMemorySizeLong(target));

            return data;
        }

        string GetPath(GameObject gameObject)
        {
            Transform trans = gameObject.transform;
            Stack<Transform> stack = StackPool<Transform>.Get();
            stack.Push(trans);

            while (trans.parent != null)
            {
                trans = trans.parent;
                stack.Push(trans);
            }

            string path = "";

            while (stack.Count > 0)
            {
                var node = stack.Pop();
                path += node.name;

                if (stack.Count > 0)
                {
                    path += "/";
                }
            }

            StackPool<Transform>.Release(stack);
            return path;
        }


        Dictionary<Object, List<Sprite>> Convert(Dictionary<Sprite, HashSet<Object>> dict)
        {
            Dictionary<Object, List<Sprite>> converted = new Dictionary<Object, List<Sprite>>();

            foreach (var pair in dict)
            {
                Sprite spr = pair.Key;
                foreach (var data in pair.Value)
                {
                    List<Sprite> list;
                    if (converted.TryGetValue(data, out list))
                    {
                        if (!list.Contains(spr))
                        {
                            list.Add(spr);
                        }
                    }
                    else
                    {
                        list = new List<Sprite> { spr };
                        converted.Add(data, list);
                    }
                }
            }

            return converted;
        }


        SimpleSpriteInfo GetSpriteData(Dictionary<Sprite, SimpleSpriteInfo> sprCache, Sprite spr)
        {
            SimpleSpriteInfo data = new SimpleSpriteInfo();
            if (sprCache.TryGetValue(spr, out data))
            {
                return data;
            }
            else
            {
                data.Sprite = spr;
                data.MemSize = AssetBundleEditorHelper.ConvertSize(Profiler.GetRuntimeMemorySizeLong(spr));
                data.TexMemSize = AssetBundleEditorHelper.ConvertSize(Profiler.GetRuntimeMemorySizeLong(spr.texture));
                data.InstanceId = spr.GetInstanceID();
                data.TexSize = spr.rect.size;
                data.AssetPath = AssetDatabase.GetAssetPath(spr);
                data.TexName = spr.texture.name;
                data.TexPath = AssetDatabase.GetAssetPath(spr.texture);

                data.Importer = (TextureImporter)TextureImporter.GetAtPath(data.TexPath);
                if (data.Importer)
                {
                    data.Mipmap = data.Importer.mipmapEnabled;
                    data.BundleName = data.Importer.assetBundleName;

                    if (string.IsNullOrEmpty(data.Importer.spritePackingTag))
                    {
                        data.PackingTag = SimpleSpriteInfo.EmptyTag;
                    }
                    else
                    {
                        data.PackingTag = data.Importer.spritePackingTag;
                    }
                }
                else if (data.TexPath == "Resources/unity_builtin_extra")
                {
                    data.PackingTag = BuildInTag;
                }
                else if (data.TexPath.EndsWith("/Resources") || data.TexPath.Contains("/Resources/"))
                {
                    data.PackingTag = ResourcesTag;
                }
                else
                {
                    Debug.LogError("Missing " + data.TexPath);
                    data.PackingTag = "Missing";
                }
                sprCache.Add(spr, data);
            }

            return data;
        }

        bool CollectSprite(Object target, Dictionary<Sprite, HashSet<Object>> allRecords)
        {
            if (!target)
                return false;

            SerializedObject serializedObject = new SerializedObject(target);
            SerializedProperty it = serializedObject.GetIterator();

            bool contain = false;
            //Or Next
            while (it.NextVisible(true))
            {
                if (it.propertyType == SerializedPropertyType.ObjectReference && it.objectReferenceValue != null && it.objectReferenceValue is Sprite)
                {
                    Sprite spr = it.objectReferenceValue as Sprite;
                    if (spr)
                    {
                        contain = true;

                        //record
                        HashSet<Object> references;
                        if (allRecords.TryGetValue(spr, out references))
                        {
                            references.Add(target);
                        }
                        else
                        {
                            references = new HashSet<Object>();
                            references.Add(target);
                            allRecords.Add(spr, references);
                        }
                    }
                }
            }
            return contain;
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