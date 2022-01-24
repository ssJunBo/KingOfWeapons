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
        public Transform RecyclePoolTrs;
        //场景节点
        public Transform SceneTrs;
        //对象池
        private Dictionary<string, List<ResourceObj>> _mObjectPoolDic = new Dictionary<string, List<ResourceObj>>();
        //暂存ResObj的Dic
        private Dictionary<int, ResourceObj> _mResourceObjDic = new Dictionary<int, ResourceObj>();
        //ResourceObj的类对象池
        private ClassObjectPool<ResourceObj> _mResourceObjClassPool = null;
        //根据异步的guid储存ResourceObj，来判断是都正在异步加载
        private Dictionary<long, ResourceObj> _mAsyncResObjs = new Dictionary<long, ResourceObj>();

        /// <summary>
        /// 初始化
        /// </summary>
        /// <param name="recycleTrs">回收节点</param>
        /// <param name="sceneTrs">场景默认节点</param>
        public void Init(Transform recycleTrs, Transform sceneTrs)
        {
            _mResourceObjClassPool = GetOrCreateClassPool<ResourceObj>(1000);
            RecyclePoolTrs = recycleTrs;
            SceneTrs = sceneTrs;
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
                    if (!ReferenceEquals(resObj.MCloneObj, null) && resObj.MbClear)
                    {
                        Object.Destroy(resObj.MCloneObj);
                        _mResourceObjDic.Remove(resObj.MCloneObj.GetInstanceID());
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
            if (!_mObjectPoolDic.TryGetValue(path, out var st)||st==null)
            {
                return;
            }
            
            for (int i = st.Count-1; i>=0 ; i--)
            {
                ResourceObj resObj = st[i];
                if (resObj.MbClear)
                {
                    st.Remove(resObj);
                    int tempID = resObj.MCloneObj.GetInstanceID();
                    Object.Destroy(resObj.MCloneObj);
                    resObj.Reset();
                    _mResourceObjDic.Remove(tempID);
                    _mResourceObjClassPool.Recycle(resObj);
                }
            }
            
            if (st.Count<=0)
            {
                _mObjectPoolDic.Remove(path);
            }
        }

        /// <summary>
        /// 从对象池取对象
        /// </summary>
        /// <param name="crc"></param>
        /// <returns></returns>
        private ResourceObj GetObjectFromPool(string path)
        {
            if (_mObjectPoolDic.TryGetValue(path, out var st) && st != null && st.Count > 0)
            {
                ResourcesManager.Instance.IncreaseResourceRef(path);
                ResourceObj resObj = st[0];
                st.RemoveAt(0);
                GameObject obj = resObj.MCloneObj;
                if (!ReferenceEquals(obj, null))
                {
                    resObj.MAlready = false;
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
        /// 取消异步加载
        /// </summary>
        /// <param name="guid"></param>
        public void CancelLoad(long guid)
        {
            if (_mAsyncResObjs.TryGetValue(guid, out var resObj) && ResourcesManager.Instance.CancelAsyncLoad(resObj))
            {
                _mAsyncResObjs.Remove(guid);
                resObj.Reset();
                _mResourceObjClassPool.Recycle(resObj);
            }
        }

        /// <summary>
        /// 是否正在异步加载
        /// </summary>
        /// <param name="guid"></param>
        /// <returns></returns>
        public bool IsAsyncLoading(long guid)
        {
            return _mAsyncResObjs[guid] != null;
        }

        /// <summary>
        /// 对象是否是对象池创建
        /// </summary>
        /// <returns></returns>
        public bool IsObjectManagerCreate(GameObject obj)
        {
            ResourceObj resObj = _mResourceObjDic[obj.GetInstanceID()];
            return resObj != null;
        }

        /// <summary>
        /// 预加载GameObject
        /// </summary>
        /// <param name="path">路径</param>
        /// <param name="count">预加载个数</param>
        /// <param name="clear">跳场景是否清楚</param>
        public void PreLoadGameObject(string path, int count = 1, bool clear = false)
        {
            List<GameObject> tempGameObjectList = new List<GameObject>();
            for (int i = 0; i < count; i++)
            {
                GameObject obj = SpwanObjFromPool(path, false, bClear: clear);
                tempGameObjectList.Add(obj);
            }

            for (int i = 0; i < count; i++)
            {
                GameObject obj = tempGameObjectList[i];
                ReleaseObject(obj);
                obj = null;
            }
            tempGameObjectList.Clear();
        }

        /// <summary>
        /// 对象池中取出obj，没有就新例化一个 同步加载
        /// </summary>
        /// <param name="path">资源路径</param>
        /// <param name="setSceneObj">是否设置到场景对象池管理位置</param>
        /// <param name="bClear"></param>
        /// <param name="targetTransform">实例化到此tranform下</param>
        /// <returns></returns>
        public GameObject SpwanObjFromPool(string path, bool setSceneObj = false, bool bClear = true,Transform targetTransform=null)
        {
            ResourceObj resourceObj = GetObjectFromPool(path);
            if (resourceObj == null)
            {
                resourceObj = _mResourceObjClassPool.Spawn(true);
                resourceObj.MbClear = bClear;
                //ResourceManager提供加载方法
                resourceObj = ResourcesManager.Instance.LoadResource(path, resourceObj);

                if (resourceObj.MResInfo.MObj != null)
                {
                    resourceObj.MCloneObj = Object.Instantiate(resourceObj.MResInfo.MObj) as GameObject;
                }
            }
            if (targetTransform!=null)
            {
                if (resourceObj.MCloneObj != null) 
                    resourceObj.MCloneObj.transform.SetParent(targetTransform, true);
            }
            else
            {
                if (setSceneObj)
                {
                    if (resourceObj.MCloneObj != null) 
                        resourceObj.MCloneObj.transform.SetParent(SceneTrs, false);
                }
            }

            if (resourceObj.MCloneObj != null)
            {
                int tempId = resourceObj.MCloneObj.GetInstanceID();
                if (!_mResourceObjDic.ContainsKey(tempId))
                {
                    _mResourceObjDic.Add(tempId, resourceObj);
                }
            }

            return resourceObj.MCloneObj;
        }

        /// <summary>
        /// 异步对象加载
        /// </summary>
        /// <param name="path"></param>
        /// <param name="dealFinish"></param>
        /// <param name="priority"></param>
        /// <param name="setSceneObject"></param>
        /// <param name="param1"></param>
        /// <param name="param2"></param>
        /// <param name="param3"></param>
        /// <param name="bClear"></param>
        public long InstantiateObjectAsync(string path, OnAsyncObjFinish dealFinish, ELoadResPriority priority, bool setSceneObject = false, object param1 = null, object param2 = null, object param3 = null, bool bClear = true)
        {
            if (string.IsNullOrEmpty(path))
            {
                return 0;
            }
            ResourceObj resObj = GetObjectFromPool(path);
            if (resObj != null)
            {
                if (setSceneObject)
                {
                    resObj.MCloneObj.transform.SetParent(SceneTrs, false);
                }

                dealFinish?.Invoke(path, resObj.MCloneObj, param1, param2, param3);

                return resObj.MGuid;
            }

            long guid = ResourcesManager.Instance.CreateGuid();
            _mAsyncResObjs.Add(guid, resObj);

            resObj = _mResourceObjClassPool.Spawn(true);
            resObj.MSetSceneParent = setSceneObject;
            resObj.MbClear = bClear;
            resObj.MDealFinis = dealFinish;
            resObj.MParam1 = param1;
            resObj.MParam2 = param2;
            resObj.MParam3 = param3;

            //调用ResourceManager的异步加载接口
            ResourcesManager.Instance.AsyncLoadResource(path, resObj, OnLoadResourceObjFinish, priority);
            return guid;
        }

        /// <summary>
        /// 资源加载完成回调
        /// </summary>
        /// <param name="path">路径</param>
        /// <param name="resObj">中间类</param>
        /// <param name="param1">参数1</param>
        /// <param name="param2">参数2</param>
        /// <param name="param3">参数3</param>
        private void OnLoadResourceObjFinish(string path, ResourceObj resObj, object param1 = null, object param2 = null, object param3 = null)
        {
            if (resObj == null)
                return;

            if (resObj.MResInfo.MObj == null)
            {
#if UNITY_EDITOR
                Debug.LogError("异步资源加载的资源为空：" + path);
#endif
            }
            else
            {
                resObj.MCloneObj = Object.Instantiate(resObj.MResInfo.MObj) as GameObject;
            }

            //加载完成就从正在加载的异步中移除
            if (_mAsyncResObjs.ContainsKey(resObj.MGuid))
            {
                _mAsyncResObjs.Remove(resObj.MGuid);
            }

            if (resObj.MCloneObj != null && resObj.MSetSceneParent)
            {
                resObj.MCloneObj.transform.SetParent(SceneTrs, false);
            }

            if (resObj.MDealFinis != null)
            {
                if (resObj.MCloneObj != null)
                {
                    int tempId = resObj.MCloneObj.GetInstanceID();
                    if (!_mResourceObjDic.ContainsKey(tempId))
                    {
                        _mResourceObjDic.Add(tempId, resObj);
                    }
                }

                resObj.MDealFinis(path, resObj.MCloneObj, resObj.MParam1, resObj.MParam2, resObj.MParam3);
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
            if (!_mResourceObjDic.TryGetValue(tempId, out var resObj))
            {
                Debug.LogError(obj.name + " 对象不是ObjectManager创建的");
                return;
            }

            if (resObj == null)
            {
                Debug.LogError("缓存的ResourceObj为空！");
            }

            if (resObj != null && resObj.MAlready)
            {
                Debug.LogError("该对象已经放回对象池，检查自己是否清空引用！");
                return;
            }
#if UNITY_EDITOR
            obj.name += "(Recycle)";
#endif
            if (maxCacheCount == 0)
            {
                _mResourceObjDic.Remove(tempId);
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

                    if (resObj.MCloneObj)
                    {
                        if (recycleParent)
                        {
                            resObj.MCloneObj.transform.SetParent(RecyclePoolTrs);
                        }
                        else
                        {
                            resObj.MCloneObj.SetActive(false);
                        }
                    }

                    if (maxCacheCount <= 0 || st.Count < maxCacheCount)
                    {
                        st.Add(resObj);
                        resObj.MAlready = true;
                        //ResourceManger做一个引用计数 
                        ResourcesManager.Instance.DecreaseResourceRef(resObj);
                    }
                    else
                    {
                        _mResourceObjDic.Remove(tempId);
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
        /// <typeparam name="T"></typeparam>
        /// <param name="maxCount"></param>
        /// <returns></returns>
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
