#pragma warning disable
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using AssetBundleSystem;
#if UNITY_EDITOR
using UnityEditor;
#endif
using UnityEngine.SceneManagement;

public class LoadTest : MonoBehaviour
{
    private Transform Test
    {
        get
        {
            var canvas = Object.FindObjectOfType<Canvas>();
            if (canvas == null)
                return null;
            return canvas.transform;
        }
    }

    public int Count = 1;
    private int value;
    private int _index;

    private Sprite _cachetex;

    [SerializeField]
    private GameObject _allPrefab;
    [SerializeField]
    private Sprite _testSprite;
    [SerializeField]
    private Object _test1Scene;
    [SerializeField]
    private Object _test2Scene;
    [SerializeField]
    private Object _emptyScene;

    // Use this for initialization
    void Start () {
        DontDestroyOnLoad(gameObject);
     //   AssetBundleLoadManager.mIns.LoadAsset("Assets/TTT/Allinfo.prefab").SetCallBack(LoadFinish).Excute();
	    //DebugLogger.Log("--------succss end");
    }

    void LoadFinish(ref ResultArgs loadResultArgs)
    {
        GameObject ins = loadResultArgs.Instantiate();
        ins.name = _index.ToString();
        _index++;
        ins.transform.SetParent(Test, false);

    }

    void LoadSceneDone(ref ResultArgs loadResultArgs)
    {
        Debug.Log("scene succss");

        loadResultArgs.LoadScene();
    }

    void LoadFinishSetSpr(ref ResultArgs args)
    {
        this._cachetex = (Sprite)args.GetObject();
        args.PinnedThis(new GameObject("pin"));
    }

    void LoadSceneEnd(ref ResultArgs args)
    {
        args.LoadScene();
        var scene = SceneManager.GetActiveScene();
        Debug.Log("scene = " + scene + " name " + scene.name);
        var allscenes = SceneManager.GetAllScenes();
        Debug.Log("allscenes = " + allscenes.Length );
    }

    string GetAllPrefabPath()
    {
#if UNITY_EDITOR
        return AssetDatabase.GetAssetPath(_allPrefab);
#else
        //Fill Full AssetPath
        return "Assets/Demo/Res/bearbutton.prefab";
#endif

    }

    string GetTestSpritePath()
    {
#if UNITY_EDITOR
        return AssetDatabase.GetAssetPath(_testSprite);
#else
//Fill Full AssetPath
        return null;
#endif
    }

    string GetTest1ScenePath()
    {
#if UNITY_EDITOR
        return AssetDatabase.GetAssetPath(_test1Scene);
#else
//Fill Full AssetPath
        return null;
#endif
    }

    string GetTest2ScenePath()
    {
#if UNITY_EDITOR
        return AssetDatabase.GetAssetPath(_test2Scene);
#else
//Fill Full AssetPath
        return null;
#endif
    }

    string GetEmptyScenePath()
    {
#if UNITY_EDITOR
        return AssetDatabase.GetAssetPath(_emptyScene);
#else
//Fill Full AssetPath
        return null;
#endif
    }

    private void OnGUI()
    {
        if(GUILayout.Button("Create"))
        {
            for (int i = 0; i < Count; i++)
            {
                AssetBundleLoadManager.mIns.LoadGameObject(GetAllPrefabPath(), Test, new Vector3(100, 100, 0)).Excute();
            }
           
        }

        if (GUILayout.Button("Create Async"))
        {
            for (int i = 0; i < Count; i++)
                AssetBundleLoadManager.mIns.LoadGameObjectAsync(GetAllPrefabPath(), Test, new Vector3(-100, 20+i, 0)).SetCallBack(LoadFinish);
        }

        if(GUILayout.Button("PreLoad Asset"))
        {
            AssetBundleLoadManager.mIns.PreLoadAsset(GetAllPrefabPath(), 10).Excute();
        }

        if (GUILayout.Button("PreLoadAsync Asset"))
        {
            AssetBundleLoadManager.mIns.PreLoadAssetAsync(GetAllPrefabPath(), 10);
        }

        if (GUILayout.Button("Load Asset"))
        {
            AssetBundleLoadManager.mIns.LoadAsset(GetTestSpritePath()).SetCallBack(LoadFinishSetSpr);
        }

        if (GUILayout.Button("LoadAsync Asset"))
        {
            AssetBundleLoadManager.mIns.LoadAssetAsync(GetTestSpritePath()).SetCallBack(LoadFinishSetSpr);
        }

        if (GUILayout.Button("Free Asset"))
        {
            this._cachetex = null;
            GameObject pinobject = GameObject.Find("pin");
            if(pinobject != null)
            {
                Destroy(pinobject);
            }
        }


        if (GUILayout.Button("log Scene"))
        {
            var scene = SceneManager.GetActiveScene();
            Debug.Log("scene = " + scene + " name " + scene.name);
            var allscenes = SceneManager.GetAllScenes();
            Debug.Log("allscenes = " + allscenes.Length);
        }


        if (GUILayout.Button("Create Scene"))
        {
            value++;
            if(value % 2 == 0)
            {
                AssetBundleLoadManager.mIns.LoadScene(GetTest1ScenePath());
            }
            else
            {
                AssetBundleLoadManager.mIns.LoadScene(GetTest2ScenePath());
            }
        }

        if (GUILayout.Button("Create Scene Async"))
        {
            value++;
            if (value % 2 == 0)
            {
                AssetBundleLoadManager.mIns.LoadSceneAsync(GetTest1ScenePath());
            }
            else
            {
                AssetBundleLoadManager.mIns.LoadSceneAsync(GetTest2ScenePath());
            }
        }

        if (GUILayout.Button("To Empty Scene"))
        {
            AssetBundleLoadManager.mIns.LoadSceneAsync(GetEmptyScenePath()).SetActive(true);
        }

        if (GUILayout.Button("To Empty Scene Add"))
        {
            AssetBundleLoadManager.mIns.LoadSceneAsync(GetEmptyScenePath(), LoadSceneMode.Additive).SetActive(true).SetCallBack(LoadSceneEnd);
        }

        if (GUILayout.Button("Unload Empty Scene"))
        {

            AssetBundleLoadManager.mIns.UnLoadSceneAsync(GetEmptyScenePath());
        }

        if (GUILayout.Button("Create Scene Async"))
        {
            value++;
            if (value % 2 == 0)
            {
                AssetBundleLoadManager.mIns.LoadSceneAsync(GetTest1ScenePath()).SetCallBack(LoadSceneDone);
            }
            else
            {
                AssetBundleLoadManager.mIns.LoadSceneAsync(GetTest2ScenePath()).SetCallBack(LoadSceneDone);
            }
        }

        if (GUILayout.Button("clear"))
        {
            int childcnt = Test.transform.childCount;
            for (int i = childcnt - 1; i >=0; i--)
            {
                var tr = Test.transform.GetChild(i);
                if(tr != null)
                {
                    Destroy(tr.gameObject);
                }
            }
        }

        if (GUILayout.Button("UnLoadUnUsed"))
        {
            Resources.UnloadUnusedAssets();
        }

        if (GUILayout.Button("UnLoad"))
        {
            AssetBundleLoadManager.mIns.ReLoad();
        }

        if (GUILayout.Button("Destroy"))
        {
            GameObject.Destroy(gameObject);
        }
    }

}
#pragma warning restore
