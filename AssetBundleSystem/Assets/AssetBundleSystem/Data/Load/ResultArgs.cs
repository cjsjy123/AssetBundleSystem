using UnityEngine;
using System.Collections;
using System;
using Object = UnityEngine.Object;

namespace AssetBundleSystem
{
    public struct ResultArgs
    {

        public Vector3 Position;

        public Quaternion Rotation;

        public Vector3 Scale;

        public Transform Parent;

        public bool Result;

        #region
        internal bool Hide;

        internal LoadReturnArgs.LoadCallback ResultCallback;

        internal LoadReturnArgs.ProgressCallback ProgresCallback;

        internal Object LoadObject;

        internal delegate void RefCallback(ref AssetBundleInfo ab, Object obj);

        internal AssetBundleInfo RuntimeInfo;

        internal RefCallback UpdateReferenceAction;

        internal AsyncOperation scene;

        #endregion

        public void LoadScene()
        {
            if(scene != null )
            {
                scene.allowSceneActivation = true;
                scene = null;
            }

        }

        public Object GetObject()
        {
            if(LoadObject is GameObject)
            {
                //if you want to use gameobject please use instantiate
                return null;
            }
            return LoadObject;
        }

        public void PinnedThis(Object binder)
        {
            if(LoadObject != null && UpdateReferenceAction != null)
            {
                UpdateReferenceAction(ref RuntimeInfo, binder);
            }
            else
            {
                Debug.LogErrorFormat("Pin {0} Failed", RuntimeInfo.UnityPath);
            }
        }

        public GameObject Instantiate()
        {
            if (LoadObject is GameObject)
            {
                GameObject gameobject = LoadObject as GameObject;
                GameObject instance = Object.Instantiate(gameobject);
                if(Parent != null)
                    instance.transform.SetParent(Parent);

                if(Hide && instance.activeSelf)
                {
                    instance.SetActive(false);
                }
                else if(!Hide && !instance.activeSelf)
                {
                    instance.SetActive(true);
                }

                instance.transform.localPosition = Position;
                instance.transform.localRotation = Rotation;
                instance.transform.localScale = Scale;

                if (Parent != null)
                {
                    instance.transform.tag = Parent.tag;
                    instance.layer = Parent.gameObject.layer;
                }

                if(UpdateReferenceAction != null)
                {
                    UpdateReferenceAction(ref RuntimeInfo, instance);
                }
                else
                {
                    Debug.LogErrorFormat("not found reference action :{0}", instance);
                }
                return instance;
            }

            return null;
        }
    }

}
