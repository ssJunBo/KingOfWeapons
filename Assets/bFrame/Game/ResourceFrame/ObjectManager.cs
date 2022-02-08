using System;
using System.Collections.Generic;
using System.Linq;
using bFrame.Game.Base;
using UnityEngine;
using Object = UnityEngine.Object;

namespace bFrame.Game.ResourceFrame
{
    public class ObjectManager : Singleton<ObjectManager>
    {
        //对象池节点
        private Transform _recyclePoolTrs;
        //对象池
        private readonly Dictionary<string, List<ResourceObj>> _mObjectPoolDic = new Dictionary<string, List<ResourceObj>>();
        //暂存ResObj的Dic
        private readonly Dictionary<int, ResourceObj> resourceObjDic = new Dictionary<int, ResourceObj>();
        //ResourceObj的类对象池
        private ClassObjectPool<ResourceObj> _mResourceObjClassPool = null;
        //根据异步的guid储存ResourceObj，来判断是都正在异步加载
        private readonly Dictionary<long, ResourceObj> _mAsyncResObjs = new Dictionary<long, ResourceObj>();

        /// <summary>
        /// 初始化
        /// </summary>
        /// <param name="recycleTrs">回收节点</param>
        public void Init(Transform recycleTrs)
        {
            _mResourceObjClassPool = GetOrCreateClassPool<ResourceObj>(1000);
            _recyclePoolTrs = recycleTrs;
        }

        /// <summary>
        /// 清空对象池
        /// </summary>
        public void ClearCache()
        {
            List<string> tempList = new List<string>();
            foreach (string key in _mObjectPoolDic.Keys)
            {
                List<ResourceObj> st = _mObjectPoolDic[key];
                for (int i = st.Count - 1; i >= 0; i--)
                {
                    ResourceObj resObj = st[i];
                    if (!ReferenceEquals(resObj.CloneObj, null) && resObj.IsClear)
                    {
                        Object.Destroy(resObj.CloneObj);
                        resourceObjDic.Remove(resObj.CloneObj.GetInstanceID());
                        resObj.Reset();
                        _mResourceObjClassPool.Recycle(resObj);
                        st.Remove(resObj);
                    }
                }

                if (st.Count <= 0)
                {
                    tempList.Add(key);
                }
            }
            
            foreach (var temp in tempList.Where(temp => _mObjectPoolDic.ContainsKey(temp)))
            {
                _mObjectPoolDic.Remove(temp);
            }

            tempList.Clear();
        }

        /// <summary>
        /// 清除某个资源在对象池中所有的对象
        /// </summary>
        /// <param name="path"></param>
        public void ClearPoolObject(string path)
        {
            if (!_mObjectPoolDic.TryGetValue(path, out var st) || st == null)
            {
                return;
            }

            for (int i = st.Count - 1; i >= 0; i--)
            {
                ResourceObj resObj = st[i];
                if (resObj.IsClear)
                {
                    st.Remove(resObj);
                    int tempId = resObj.CloneObj.GetInstanceID();
                    Object.Destroy(resObj.CloneObj);
                    resObj.Reset();
                    resourceObjDic.Remove(tempId);
                    _mResourceObjClassPool.Recycle(resObj);
                }
            }

            if (st.Count <= 0)
            {
                _mObjectPoolDic.Remove(path);
            }
        }

        /// <summary>
        /// 从对象池取对象
        /// </summary>
        /// <returns></returns>
        private ResourceObj GetObjectFromPool(string path)
        {
            if (_mObjectPoolDic.TryGetValue(path, out var st) && st != null && st.Count > 0)
            {
                ResourcesManager.Instance.IncreaseResourceRef(path);
                ResourceObj resObj = st[0];
                st.RemoveAt(0);
                GameObject obj = resObj.CloneObj as GameObject;
                
                if (!ReferenceEquals(obj, null))
                {
                    resObj.Already = false;
#if UNITY_EDITOR
                    if (obj.name.EndsWith("(Recycle)"))
                    {
                        obj.name = obj.name.Replace("(Recycle)", "");
                    }
#endif
                }
                return resObj;
            }
            return null;
        }


        /// <summary>
        /// 对象是否是对象池创建
        /// </summary>
        /// <returns></returns>
        public bool IsObjectManagerCreate(GameObject obj)
        {
            return resourceObjDic[obj.GetInstanceID()] != null;
        }
        

        /// <summary>
        /// 对象池中取出obj，没有就新例化一个 同步加载
        /// </summary>
        /// <param name="path">资源路径</param>
        /// <param name="setSceneObj">是否设置到场景对象池管理位置</param>
        /// <param name="bClear"></param>
        /// <param name="targetTransform">实例化到此transform下</param>
        /// <returns></returns>
        public GameObject SpwanObjFromPool(string path, bool setSceneObj = false, bool bClear = true,Transform targetTransform=null)
        {
            ResourceObj resourceObj = GetObjectFromPool(path);
            if (resourceObj == null)
            {
                resourceObj = _mResourceObjClassPool.Spawn(true);
                resourceObj.IsClear = bClear;
                //ResourceManager提供加载方法
//                resourceObj = ResourcesManager.Instance.LoadResource(path, resourceObj);

                if (resourceObj.resInfo.Obj != null)
                {
                    resourceObj.CloneObj = Object.Instantiate(resourceObj.resInfo.Obj) as GameObject;
                }
            }
            if (targetTransform!=null)
            {
                if (resourceObj.CloneObj != null) 
                    resourceObj.CloneObj.transform.SetParent(targetTransform, true);
            }
            else
            {
                if (setSceneObj)
                {
                    if (resourceObj.CloneObj != null) 
                        resourceObj.CloneObj.transform.SetParent(_recyclePoolTrs, false);
                }
            }

            if (resourceObj.CloneObj != null)
            {
//                int tempId = resourceObj.CloneObj.GetInstanceID();
//                if (!_mResourceObjDic.ContainsKey(tempId))
//                {
//                    _mResourceObjDic.Add(tempId, resourceObj);
//                }
            }

            return resourceObj.CloneObj;
        }


        /// <summary>
        /// 资源加载完成回调
        /// </summary>
        /// <param name="path">路径</param>
        /// <param name="resObj">中间类</param>
        private void OnLoadResourceObjFinish(string path, ResourceObj resObj)
        {
            if (resObj == null)
                return;

            if (resObj.resInfo.Obj == null)
            {
#if UNITY_EDITOR
                Debug.LogError("异步资源加载的资源为空：" + path);
#endif
            }
            else
            {
                resObj.CloneObj = Object.Instantiate(resObj.resInfo.Obj) as GameObject;
            }

            //加载完成就从正在加载的异步中移除
            if (_mAsyncResObjs.ContainsKey(resObj.Guid))
            {
                _mAsyncResObjs.Remove(resObj.Guid);
            }
        }

        /// <summary>
        /// 回收资源
        /// </summary>
        /// <param name="obj"></param>
        /// <param name="maxCacheCount"></param>
        /// <param name="destroyCache"></param>
        /// <param name="recycleParent"></param>
        public void ReleaseObject(GameObject obj, int maxCacheCount = -1, bool destroyCache = false,
            bool recycleParent = true)
        {
            if (obj == null)
            {
                return;
            }

            int tempId = obj.GetInstanceID();
            if (!resourceObjDic.TryGetValue(tempId, out var resObj))
            {
                Debug.LogError(obj.name + " 对象不是ObjectManager创建的");
                return;
            }

            if (resObj == null)
            {
                Debug.LogError("缓存的ResourceObj为空！");
            }

            if (resObj != null && resObj.Already)
            {
                Debug.LogError("该对象已经放回对象池，检查自己是否清空引用！");
                return;
            }
#if UNITY_EDITOR
            obj.name += "(Recycle)";
#endif
            if (maxCacheCount == 0)
            {
                resourceObjDic.Remove(tempId);
                ResourcesManager.Instance.ReleaseResource(resObj, destroyCache);
                if (resObj != null)
                {
                    resObj.Reset();
                    _mResourceObjClassPool.Recycle(resObj);
                }
            }
            else //回收到对象池
            {
                if (resObj != null)
                {
                    List<ResourceObj> st = null;
                    if (!_mObjectPoolDic.TryGetValue(resObj.Path, out st) || st == null)
                    {
                        st = new List<ResourceObj>();
                        _mObjectPoolDic.Add(resObj.Path, st);
                    }

                    if (resObj.CloneObj)
                    {
                        if (recycleParent)
                        {
//                            resObj.CloneObj.transform.SetParent(_recyclePoolTrs);
                        }
                        else
                        {
//                            resObj.CloneObj.SetActive(false);
                        }
                    }

                    if (maxCacheCount <= 0 || st.Count < maxCacheCount)
                    {
                        st.Add(resObj);
                        resObj.Already = true;
                        //ResourceManger做一个引用计数 
//                        ResourcesManager.Instance.DecreaseResourceRef(resObj);
                    }
                    else
                    {
                        resourceObjDic.Remove(tempId);
                        ResourcesManager.Instance.ReleaseResource(resObj, destroyCache);
                        resObj.Reset();
                        _mResourceObjClassPool.Recycle(resObj);
                    }
                }
            }
        }

        #region 类对象池使用

        private readonly Dictionary<Type, object> _mClassPoolDic = new Dictionary<Type, object>();

        /// <summary>
        /// 创建类对象池，创建完成后外面可以保存ClassObjectPool<T>，然后调用spwan和recycle来创建和回收类对象
        /// </summary>
        public ClassObjectPool<T> GetOrCreateClassPool<T>(int maxCount) where T : class, new()
        {
            Type type = typeof(T);
            if (!_mClassPoolDic.TryGetValue(type, out var outObj) || outObj == null)
            {
                ClassObjectPool<T> newPool = new ClassObjectPool<T>(maxCount);
                _mClassPoolDic.Add(type, newPool);
                return newPool;
            }
            return outObj as ClassObjectPool<T>;
        }
        #endregion  
    }
}
