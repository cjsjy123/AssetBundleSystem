#if UNITY_EDITOR
using System;
using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;

namespace AssetBundleSystem.Editor
{

    internal static class AssetWindowConfig
    {

        public static int ParseStep = 1;

        public static float WaitSeconds = 0.3f;

        public static string DefaultOutputPath = AssetBundleHelper.DataPath;

        public static List<Type> LoadSystems = new List<Type>()
        {
            typeof(EditorReadManiFestSystem),
            typeof(EditorWriteDepFileSystem)
        };

        public static List<KeyValuePair<int,Type>> WindowTypes = new List<KeyValuePair<int, Type>>()
        {
            new KeyValuePair<int, Type>(0,typeof(AssetLogic)),
            new KeyValuePair<int, Type>(1,typeof(DependencyLogic)),
            new KeyValuePair<int, Type>(8,typeof(BuildLogic)),
            new KeyValuePair<int, Type>(16,typeof(ProfilerLogic)),
            new KeyValuePair<int, Type>(32,typeof(SpriteLogic)),
        };

        public static List<KeyValuePair<int, Type>> WindowGuiTypes = new List<KeyValuePair<int, Type>>()
        {
            new KeyValuePair<int, Type>(0,typeof(AssetGuiLogic)),
            new KeyValuePair<int, Type>(1,typeof(AssetDependencyGuiLogic)),
            new KeyValuePair<int, Type>(8,typeof(BuildGuiLogic)),
            new KeyValuePair<int, Type>(16,typeof(ProfilerGuiLogic)),
            new KeyValuePair<int, Type>(32,typeof(SpriteGuiLogic)),
        };



        public static IParseDependencyWriter GetDepWriter()
        {
            return new DefaultParseDepWriter();
        }

    }
}
#endif

