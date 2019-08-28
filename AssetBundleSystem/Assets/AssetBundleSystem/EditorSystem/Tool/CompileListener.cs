#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEditor;
using System.Reflection.Emit;

namespace AssetBundleSystem.Editor
{

    [InitializeOnLoad]
    class CompileListener
    {
        static CompileListener()
        {
            EditorUtility.ClearProgressBar();
        }
    }

}
#endif