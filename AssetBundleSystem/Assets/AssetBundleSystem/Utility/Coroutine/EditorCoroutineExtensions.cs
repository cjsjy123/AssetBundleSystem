using System.Collections;
#if UNITY_EDITOR
using UnityEditor;

namespace CommonUtils.Editor
{
	public static class EditorCoroutineExtensions
	{
		public static EditorCoroutineGroup.EditorCoroutine StartCoroutine(this EditorWindow thisRef, IEnumerator coroutine)
		{
			return EditorCoroutineGroup.StartCoroutine(coroutine, thisRef);
		}

		public static EditorCoroutineGroup.EditorCoroutine StartCoroutine(this EditorWindow thisRef, string methodName)
		{
			return EditorCoroutineGroup.StartCoroutine(methodName, thisRef);
		}

		public static EditorCoroutineGroup.EditorCoroutine StartCoroutine(this EditorWindow thisRef, string methodName, object value)
		{
			return EditorCoroutineGroup.StartCoroutine(methodName, value, thisRef);
		}

		public static void StopCoroutine(this EditorWindow thisRef, IEnumerator coroutine)
		{
			EditorCoroutineGroup.StopCoroutine(coroutine, thisRef);
		}

		public static void StopCoroutine(this EditorWindow thisRef, string methodName)
		{
			EditorCoroutineGroup.StopCoroutine(methodName, thisRef);
		}

		public static void StopAllCoroutines(this EditorWindow thisRef)
		{
			EditorCoroutineGroup.StopAllCoroutines(thisRef);
		}
	}
}
#endif