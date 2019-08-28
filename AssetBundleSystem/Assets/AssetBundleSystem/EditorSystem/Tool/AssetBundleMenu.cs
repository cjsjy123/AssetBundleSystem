#if UNITY_EDITOR
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using UnityEditor;

namespace AssetBundleSystem.Editor
{

    internal static class AssetBundleMenu 
    {
        [MenuItem("Assets/Show Dependency")]
        static void ShowDependency()
        {
            var select = Selection.assetGUIDs;
            if (select.Length > 0)
            {
                var assetpath = AssetDatabase.GUIDToAssetPath(select[0]);

                var depinfo = EditorUtility.CollectDependencies(new Object[]
                    {AssetDatabase.LoadAssetAtPath <Object>(assetpath)});

                StringBuilder sb = new StringBuilder();
                

                HashSet<string> hashSet = new HashSet<string>();
                foreach (var dep in depinfo)
                {
                    if (dep != null)
                    {
                        hashSet.Add(AssetDatabase.GetAssetPath(dep));
                    }

                }
                sb.AppendLine("-------Dependency------ "+ hashSet.Count+"\n");
                foreach (var info in hashSet)
                {
                    sb.AppendLine(info);
                }

                Debug.Log(sb.ToString());
            }
        }
    }
}

#endif