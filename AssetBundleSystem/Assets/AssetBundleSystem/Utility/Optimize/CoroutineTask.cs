using System;
using System.Collections;
#if UNITY_EDITOR
using CommonUtils.Editor;
#endif
using UnityEngine;

namespace CommonUtils
{
    public class CoroutineTask
    {
        static IEnumerator YiledNull()
        {
            yield return null;
        }

        public static void NextCoFrame(object jobId)
        {
            if (Application.isPlaying)
            {
                if (jobId != null && jobId is MonoBehaviour)
                {
                    MonoBehaviour mb = jobId as MonoBehaviour;
                    mb.StartCoroutine(YiledNull());
                }
                else
                {
                    Debug.LogError("In Playing Mode, Job Runner Error");
                }
            }
            else
            {
                if (jobId != null)
                {
#if UNITY_EDITOR
                    EditorCoroutineGroup.StartCoroutine(YiledNull(), jobId);
#else
                    Debug.LogError("Cant Run Coroutine");
#endif
                }
                else
                {
                    Debug.LogError("In Editor Mode, Job Runner is Null");
                }
            }

        }

        public static void NextCoTask(object jobId, IEnumerator co)
        {
            if (Application.isPlaying)
            {
                if (jobId != null && jobId is MonoBehaviour)
                {
                    MonoBehaviour mb = jobId as MonoBehaviour;
                    mb.StartCoroutine(co);
                }
                else
                {
                    Debug.LogError("In Playing Mode, Job Runner Error");
                }
            }
            else
            {
                if (jobId != null)
                {
#if UNITY_EDITOR
                    EditorCoroutineGroup.StartCoroutine(co, jobId);
#else
                    Debug.LogError("Cant Run Coroutine");
#endif
                }
                else
                {
                    Debug.LogError("In Editor Mode, Job Runner is Null");
                }
            }
        }

    }
}
