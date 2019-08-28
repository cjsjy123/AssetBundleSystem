#if UNITY_EDITOR
using System;
using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;

namespace AssetBundleSystem.Editor
{

    internal class AssetTreeManager
    {
        private static AssetTreeManager _mIns;
        public static AssetTreeManager mIns
        {
            get
            {
                if (_mIns == null)
                {
                    _mIns = new AssetTreeManager();
                }
                return _mIns;
            }
        }

        private int _counter;
        private readonly Dictionary<int, List<IAssetGuiInterface>> _guidictionary = new Dictionary<int, List<IAssetGuiInterface>>();
        private readonly Dictionary<int, List<ILogicInterface>> _dictionary = new Dictionary<int, List<ILogicInterface>>();
        private readonly Dictionary<string,string> _iconDictionary = new Dictionary<string, string>();
        private readonly Dictionary<int,string> _resTypeDictionary = new Dictionary<int, string>();
        private readonly Dictionary<Type, ITreeModel> _modelDictionary = new Dictionary<Type, ITreeModel>();

        AssetTreeManager()
        {
            InitWindowType();
            InitGuiWindows();

            InitIcons();
            InitResType();

        }

        void InitWindowType()
        {
            int endval = (int)AssetWindow.WindowMode.End;
            for (int i = 0; i < AssetWindowConfig.WindowTypes.Count; i++)
            {
                var tuple = AssetWindowConfig.WindowTypes[i];
                if (tuple.Key >= 0 && tuple.Key < endval && !_dictionary.ContainsKey(tuple.Key))
                {
                    var logic = Activator.CreateInstance(tuple.Value) as ILogicInterface;
                    Add2Dictionary(_dictionary, tuple.Key, logic);
                    int groupid = tuple.Key / 8;
                    if ((tuple.Key) % 8 != 0)
                    {
                        List<ILogicInterface> list;
                        if (_dictionary.TryGetValue(groupid * 8, out list) && list.Count > 0)
                        {
                            Add2DictionaryFirst(_dictionary, tuple.Key, list[0]);
                        }
                    }
                }
                else
                {
                    Debug.LogErrorFormat("key error for create :{0}", tuple.Value.Name);
                }
            }
        }

        void InitGuiWindows()
        {
            int endval = (int) AssetWindow.WindowMode.End;
            for (int i = 0; i < AssetWindowConfig.WindowGuiTypes.Count; i++)
            {
                var tuple = AssetWindowConfig.WindowGuiTypes[i];
                if (tuple.Key >= 0 && tuple.Key < endval && !_guidictionary.ContainsKey(tuple.Key))
                {
                    var logic = Activator.CreateInstance(tuple.Value) as IAssetGuiInterface;
                    Add2Dictionary(_guidictionary, tuple.Key, logic);
                    int groupid = tuple.Key / 8;
                    if ((tuple.Key) % 8 != 0)
                    {
                        List<IAssetGuiInterface> list;
                        if (_guidictionary.TryGetValue(groupid * 8, out list) && list.Count > 0)
                        {
                            Add2DictionaryFirst(_guidictionary, tuple.Key, list[0]);
                        }
                    }
                }
                else
                {
                    Debug.LogErrorFormat("key error for create :{0}",tuple.Value.Name);
                }
            }
    
        }

        void Add2Dictionary<T,U>(Dictionary<U,List<T>> dict,U key, T data)
        {
            List<T> list;
            if (dict.TryGetValue(key, out list))
            {
                list.Add(data);
            }
            else
            {
                dict.Add(key,new List<T>(){data});
            }
        }

        void Add2DictionaryFirst<T, U>(Dictionary<U, List<T>> dict, U key, T data)
        {
            List<T> list;
            if (dict.TryGetValue(key, out list))
            {
                list.Insert(0,data);
            }
            else
            {
                dict.Add(key, new List<T>() { data });
            }
        }

        void InitIcons()
        {
            //
            _iconDictionary["Camera"] = "Camera Icon";
            _iconDictionary["Folder"] = "Folder Icon";
            _iconDictionary["AudioSource"] = "AudioClip Icon";
            _iconDictionary["GameObject"] = "GameObject Icon";
            _iconDictionary["Prefab"] = "PrefabNormal Icon";
            _iconDictionary["cs"] = "cs Script Icon";
            _iconDictionary["scene"] = "SceneAsset Icon";
            _iconDictionary["Material"] = "Material Icon";
            _iconDictionary["Shader"] = "Shader Icon";
            _iconDictionary["Text"] = "TextAsset Icon";
            _iconDictionary["cginc"] = "CGProgram Icon";
            _iconDictionary["physicmaterial"] = "PhysicMaterial Icon";
            _iconDictionary["asset"] = "GameManager Icon";
            _iconDictionary["anim"] = "Animation Icon";
            _iconDictionary["font"] = "Font Icon";
            _iconDictionary["image"] = "Texture Icon";
            _iconDictionary["mesh"] = "Mesh Icon";
            _iconDictionary["video"] = "MovieTexture Icon";
            _iconDictionary["warning"] = "d_console.warnicon";
            _iconDictionary["scriptableobject"] = "ScriptableObject Icon";
            _iconDictionary["sprite"] = "Sprite Icon";

            _iconDictionary["Default"] = "DefaultAsset Icon";
            _iconDictionary["Unknown"] = "DefaultAsset Icon";
        }

        void InitResType()
        {
            //restype
            _resTypeDictionary[(int)AssetBundleResType.Font] = "font";
            _resTypeDictionary[(int)AssetBundleResType.Image] = "image";
            _resTypeDictionary[(int)AssetBundleResType.Sound] = "AudioSource";
            _resTypeDictionary[(int)AssetBundleResType.Scene] = "scene";
            _resTypeDictionary[(int)AssetBundleResType.AnimationControl] = "anim";
            _resTypeDictionary[(int)AssetBundleResType.Animation] = "anim";
            _resTypeDictionary[(int)AssetBundleResType.Bytes] = "Text";
            _resTypeDictionary[(int)AssetBundleResType.Material] = "Material";
            _resTypeDictionary[(int)AssetBundleResType.Prefab] = "Prefab";
            _resTypeDictionary[(int)AssetBundleResType.RenderTexture] = "image";
            _resTypeDictionary[(int)AssetBundleResType.Text] = "Text";
            _resTypeDictionary[(int)AssetBundleResType.Shader] = "Shader";
            _resTypeDictionary[(int)AssetBundleResType.Fbx] = "mesh";
            _resTypeDictionary[(int)AssetBundleResType.Folder] = "Folder";
            _resTypeDictionary[(int)AssetBundleResType.CustomAsset] = "asset";
            _resTypeDictionary[(int)AssetBundleResType.GameObject] = "GameObject";
            _resTypeDictionary[(int)AssetBundleResType.Cs] = "cs";
            _resTypeDictionary[(int)AssetBundleResType.Sprite] = "sprite";
            _resTypeDictionary[(int)AssetBundleResType.Unknown] = "Unknown";
        }

        internal T Pick<T>(int index =0) where T:ILogicInterface
        {
            for (int i = 0; i < AssetWindowConfig.WindowTypes.Count; i++)
            {
                var tuple = AssetWindowConfig.WindowTypes[i];
                if (tuple.Value == typeof(T))
                {
                    List<ILogicInterface> list;
                    if (_dictionary.TryGetValue(tuple.Key,out list))
                    {
                        if (list.Count > index)
                        {
                            return (T)list[index];
                        }
                    }
                }
            }
            return default(T);
        }

        public void Clear()
        {
            _modelDictionary.Clear();
        }

        public int GetUniqueId()
        {
            return ++_counter;
        }

        public AssetTreeModel<T> GetModel<T>() where T : ITreeData
        {
            ITreeModel model;
            if (_modelDictionary.TryGetValue(typeof(T), out model))
            {
                return (AssetTreeModel<T>)model;
            }
            model = new AssetTreeModel<T>();
            _modelDictionary.Add(typeof(T), model);
            return (AssetTreeModel<T>)model;
        }

        public List<ILogicInterface> Get(AssetWindow.WindowMode md) 
        {
            List<ILogicInterface> data;
            if (_dictionary.TryGetValue((int)md, out data))
            {
                return data;
            }
            return null;
        }

        public List<IAssetGuiInterface> GetGuiRender(AssetWindow.WindowMode md)
        {
            List<IAssetGuiInterface> data;
            if (_guidictionary.TryGetValue((int)md, out data))
            {
                return data;
            }
            return null;
        }

        public string GetIconName(string str)
        {
            string result;
            if (_iconDictionary.TryGetValue(str, out result))
            {
                return result;
            }
            return "";
        }

        public string GetIconName(AssetBundleResType resType)
        {
            string result;
            if (_resTypeDictionary.TryGetValue((int)resType, out result))
            {
                return GetIconName(result);
            }
            return GetIconName("Unknown");
        }

        public Texture2D GetEditorTexture(AssetBundleResType resType)
        {
            return EditorGUIUtility.FindTexture(GetIconName(resType));
        }

        public Texture2D GetEditorTexture(string key)
        {
            return EditorGUIUtility.FindTexture(GetIconName(key));
        }
    }

}
#endif


