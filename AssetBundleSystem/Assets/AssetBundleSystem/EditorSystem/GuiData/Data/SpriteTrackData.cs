#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEditor;
using UnityEngine;
using Object = UnityEngine.Object;

namespace AssetBundleSystem.Editor
{
    internal enum SpriteShowMode
    {
        Scene,
        Prefabs,
        Atlas,
    }

    internal struct SpriteTrackData :ITreeData
    {
        public int Id { get; set; }

        public string DisplayName { get; set; }

        public string IconName { get; set; }

        public string FilePath { get; set; }

        public EditorAssetBundleInfo EditorInfo { get; set; }

        public SpriteShowMode ShowMode;

        public SceneObjectSpriteInfo SceneData;

        public GameObjectSpriteInfo GameObjectData;

        public AtlasReferenceData AtlasData;

        public int UsedRefCount;

    }

    internal struct SimpleSpriteInfo
    {
        public const string EmptyTag = "Missing Tag";

        public Sprite Sprite;

        public int InstanceId;

        public string PackingTag;

        public bool Mipmap;

        public string BundleName;

        public Vector2 TexSize;

        public string MemSize;

        public string TexMemSize;

        public string AssetPath;

        public string TexName;

        public string TexPath;

        public TextureImporter Importer;
    }

    internal struct SimpleGoData
    {
        public GameObject Go;

        public int InstanceId;

        public string AssetPath;

        public string Path;

        public PrefabType PrefabType;

        public string MemSize;
    }

    internal struct SimpleRefData
    {
        public Object Target;

        public int InstanceId;

        public string MemSize;

        public string Path;
    }

    internal struct SceneObjectSpriteInfo
    {
        public SimpleSpriteInfo SprData;

        public MonoBehaviour CsReferences;

        public int InstanceId;

        public string MemSize;

        public string Path;

    }

    internal struct GameObjectSpriteInfo
    {
        public SimpleSpriteInfo SprData;

        public SimpleGoData GoData;
    }

    internal struct AtlasReferenceData
    {
        public SimpleSpriteInfo SprData;

        public SimpleRefData RefData;
 
    }

}
#endif