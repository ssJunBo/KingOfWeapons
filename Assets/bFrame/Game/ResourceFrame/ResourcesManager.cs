//using System.Collections.Generic;
//using System.Linq;
//using bFrame.Game.Base;
//using UnityEngine;
//using Object = UnityEngine.Object;
//
//namespace bFrame.Game.ResourceFrame
//{
//    public class ResourceInfo
//    {
//        public bool BIsFromResources = false;
//        //资源路径
//        public string Path;
//
//        //资源对象
//        public Object MObj = null;
//
//        //是否跳场景清掉
//        public bool MClear = true;
//
//        public float LastUseTime;
//        //引用计数
//        public int RefCount { get; set; } = 0;
//    }
//    
//    
//   
//
//    public class ResourcesManager : Singleton<ResourcesManager>
//    {
//        private long _mGuid = 0;
//        public bool isLoadFromAssetBundle = false;
//
//        /// <summary>
//        /// 缓存使用的资源列表  key = 路径 
//        /// </summary>
//        private readonly Dictionary<string, ResourceInfo> _assetDic = new Dictionary<string, ResourceInfo>();
//
//        /// <summary>
//        /// 缓存应用为零的资源列表，达到缓存最大的时 释放这个列表里面最早没用的资源
//        /// </summary>
//        private readonly CMapList<ResourceInfo> _mNoReferenceAssetMapList = new CMapList<ResourceInfo>();
//
//        /// <summary>
//        /// 中间类 回调类 的类对象池
//        /// </summary>
////        private readonly ClassObjectPool<AsyncLoadResParam> _mAsyncLoadResParamPool =
////            new ClassObjectPool<AsyncLoadResParam>(50);
//
////        private readonly ClassObjectPool<AsyncCallBack> _mAsyncCallBackPool = new ClassObjectPool<AsyncCallBack>(100);
//
//        //正在异步加载的资源列表
////        private readonly List<AsyncLoadResParam>[] _mLoadingAssetList =
////            new List<AsyncLoadResParam>[(int) ELoadResPriority.Default];
//
//        //正在异步加载得dic
////        private readonly Dictionary<string, AsyncLoadResParam> _mLoadingAssetDic =
////            new Dictionary<string, AsyncLoadResParam>();
//
//        //最长连续卡着加载资源的时间 单位微秒
//        private const long MaxLoadTime = 200000;
//
//        //最大缓存个数 中配 500 高配 1000 低配 200 复杂处理（搜索 unity3d获取内存大小）
//        private const int MaxCacheCount = 500;
//
//
//        /// <summary>
//        /// 创建唯一的GUID
//        /// </summary>
//        /// <returns></returns>
//        public long CreateGuid()
//        {
//            return _mGuid++;
//        }
//
//        /// <summary>
//        /// 清空缓存 一般用于跳场景
//        /// </summary>
//        public void ClearCache()
//        {
//            List<ResourceInfo> tempList = _assetDic.Values.Where(item => item.MClear).ToList();
//
//            foreach (var item in tempList)
//            {
//                DestroyResourceItem(item, item.MClear);
//            }
//
//            tempList.Clear();
//        }
//
//        /// <summary>
//        /// 取消异步加载资源
//        /// </summary>
//        /// <returns></returns>
////        public bool CancelAsyncLoad(ResourceObj res)
////        {
////            if (_mLoadingAssetDic.TryGetValue(res.Path, out var para) &&
////                _mLoadingAssetList[(int) para.MPriority].Contains(para))
////            {
////                for (int i = para.MCallBackList.Count - 1; i >= 0; i--)
////                {
////                    AsyncCallBack tempCallBack = para.MCallBackList[i];
////                    if (tempCallBack != null && res == tempCallBack.MResObj)
////                    {
////                        tempCallBack.Reset();
////                        _mAsyncCallBackPool.Recycle(tempCallBack);
////                        para.MCallBackList.Remove(tempCallBack);
////                    }
////                }
////
////                if (para.MCallBackList.Count <= 0)
////                {
////                    para.Reset();
////                    _mLoadingAssetList[(int) para.MPriority].Remove(para);
////                    _mAsyncLoadResParamPool.Recycle(para);
////                    _mLoadingAssetDic.Remove(res.Path);
////                    return true;
////                }
////            }
////
////            return false;
////        }
//
//        /// <summary>
//        /// 根据ResObj增加引用计数
//        /// </summary>
//        /// <returns></returns>
//        public int IncreaseResourceRef(ResourceObj resObj, int count = 1)
//        {
//            return resObj != null ? IncreaseResourceRef(resObj.Path, count) : 0;
//        }
//
//        /// <summary>
//        /// 根据path增加引用计数
//        /// </summary>
//        /// <param name="path"></param>
//        /// <param name="count"></param>
//        /// <returns></returns>
//        public int IncreaseResourceRef(string path, int count = 1)
//        {
//            if (!_assetDic.TryGetValue(path, out var item) || item == null)
//                return 0;
//
//            item.RefCount += count;
//
//            //item. = Time.realtimeSinceStartup;//时间
//
//            return item.RefCount;
//        }
//
//        /// <summary>
//        /// 根据ResourceObj减少引用计数
//        /// </summary>
//        /// <param name="resObj"></param>
//        /// <param name="count"></param>
//        /// <returns></returns>
//        public int DecreaseResourceRef(ResourceObj resObj, int count = 1)
//        {
//            return resObj != null ? DecreaseResourceRef(resObj.Path, count) : 0;
//        }
//
//        /// <summary>
//        /// 根据路径减少引用计数
//        /// </summary>
//        /// <param name="path"></param>
//        /// <param name="count"></param>
//        /// <returns></returns>
//        private int DecreaseResourceRef(string path, int count = 1)
//        {
//            if (!_assetDic.TryGetValue(path, out var item) || item == null)
//            {
//                return 0;
//            }
//
//            item.RefCount -= count;
//
//            return item.RefCount;
//        }
//
//        /// <summary>
//        /// 预加载资源
//        /// </summary>
//        /// <param name="path"></param>
//        public void PreloadRes(string path)
//        {
//            if (string.IsNullOrEmpty(path))
//                return;
//
////            uint crc = CRC32.GetCRC32(path);
//            ResourceInfo info = GetCacheResourceItem(path);
//            if (info != null)
//            {
//                return;
//            }
//
//            Object obj = null;
//#if UNITY_EDITOR
//            if (!isLoadFromAssetBundle)
//            {
//                info = AssetBundleManager.Instance.FindResourceItem(path);
//                if (info != null && info.MObj != null)
//                {
//                    obj = info.MObj;
//                }
//                else
//                {
//                    if (info == null)
//                    {
//                        info = new ResourceInfo {Path = path};
//                    }
//
//                    obj = LoadAssetByEditor<Object>(path);
//                }
//            }
//#endif
//
//            if (obj == null)
//            {
//                info = AssetBundleManager.Instance.LoadResourceAssetBundle(path);
//                if (info != null && info.MAssetBundle != null)
//                {
//                    obj = info.MObj != null ? info.MObj : info.MAssetBundle.LoadAsset<Object>(info.MAssetName);
//                }
//            }
//
//            CacheResource(path, ref info, obj);
//
//            //跳场景不清空缓存
//            info.MClear = false;
//            ReleaseResource(obj, false);
//        }
//
//        public delegate void DelegateResourceLoaded(string path, Object obj);
//
//        /// <summary>
//        /// 同步加载资源 针对给ObjectManager的接口
//        /// </summary>
//        /// <param name="path"></param>
//        /// <param name="pri"></param>
//        /// <param name="onLoaded"></param>
//        /// <param name="isFromResources"></param>
//        /// <returns></returns>
//        public void LoadResource(string path, DelegateResourceLoaded onLoaded,
//            bool isFromResources = false)
//        {
//            ResourceInfo resourceItem = GetCacheResourceItem(path);
//            if (resourceItem != null)
//            {
////                if (pri > resourceItem.Priority)
////                {
////                    resourceItem.Priority = pri;
////                }
//
//                if (resourceItem.MObj != null)
//                {
//                    onLoaded?.Invoke(path, resourceItem.MObj);
//                }
//            }
//
//            resourceItem = new ResourceInfo
//            {
//                BIsFromResources = isFromResources,
//                Path = path
//            };
//            //            resourceItem.Priority = pri;
//            resourceItem.RefCount++;
//            _assetDic[path] = resourceItem;
//        }
//
//        /// <summary>
//        /// 同步资源加载 外部直接调用 仅加载不需要实例化的资源 例如texture 音频等
//        /// </summary>
//        /// <typeparam name="T"></typeparam>
//        /// <param name="path"></param>
//        /// <returns></returns>
//        public T LoadResource<T>(string path) where T : Object
//        {
//            if (string.IsNullOrEmpty(path))
//            {
//                return null;
//            }
//
//            ResourceInfo info = GetCacheResourceItem(path);
//            if (info != null)
//            {
//                return info.MObj as T;
//            }
//
//            T obj = null;
//#if UNITY_EDITOR
//            if (!isLoadFromAssetBundle)
//            {
//                info = AssetBundleManager.Instance.FindResourceItem(path);
//                if (info != null && info.MAssetBundle != null)
//                {
//                    if (info.MObj != null)
//                    {
//                        obj = (T) info.MObj;
//                    }
//                    else
//                    {
//                        obj = info.MAssetBundle.LoadAsset<T>(info.MAssetName);
//                    }
//                }
//                else
//                {
//                    if (info == null)
//                    {
//                        info = new ResourceInfo {Path = path};
//                    }
//
//                    obj = LoadAssetByEditor<T>(path);
//                }
//            }
//#endif
//
//            if (obj == null)
//            {
//                info = AssetBundleManager.Instance.LoadResourceAssetBundle(path);
//                if (info != null && info.MAssetBundle != null)
//                {
//                    if (info.MObj != null)
//                    {
//                        obj = info.MObj as T;
//                    }
//                    else
//                    {
//                        obj = info.MAssetBundle.LoadAsset<T>(info.MAssetName);
//                    }
//                }
//            }
//
//            CacheResource(path, ref info, obj);
//
//            return obj;
//        }
//
//
//        /// <summary>
//        /// 根据ResourceObj卸载资源
//        /// </summary>
//        /// <param name="resObj"></param>
//        /// <param name="isDestroyObj"></param>
//        /// <returns></returns>
//        public bool ReleaseResource(ResourceObj resObj, bool isDestroyObj = false)
//        {
//            if (resObj == null) return false;
//
//            if (!_assetDic.TryGetValue(resObj.Path, out var info) || info == null)
//            {
//                Debug.Log("AssetDic里不存在该资源: " + resObj.MCloneObj.name + " 可能释放多次！");
//            }
//
//            Object.Destroy(resObj.MCloneObj);
//
//            if (info != null)
//            {
//                info.RefCount--;
//                DestroyResourceItem(info, isDestroyObj);
//            }
//
//            return true;
//        }
//
//        /// <summary>
//        /// 不需要实例化的资源卸载，根据对象去释放
//        /// </summary>
//        /// <param name="obj"></param>
//        /// <param name="isDestroyObj"></param>
//        /// <returns></returns>
//        private bool ReleaseResource(Object obj, bool isDestroyObj = false)
//        {
//            if (obj == null)
//            {
//                return false;
//            }
//
//            ResourceInfo info = null;
//            foreach (var res in _assetDic.Values.Where(res => res.MGuid == obj.GetInstanceID()))
//            {
//                info = res;
//            }
//
//            if (info == null)
//            {
//                Debug.LogError("AssetDic 里不存在该资源：" + obj.name + " 可能释放了多次");
//                return false;
//            }
//
//            info.RefCount--;
//            DestroyResourceItem(info, isDestroyObj);
//            return true;
//        }
//
//        /// <summary>
//        /// 不需要实例化的资源卸载，根据路径
//        /// </summary>
//        /// <param name="path"></param>
//        /// <param name="isDestroyObj"></param>
//        /// <returns></returns>
//        public bool ReleaseResource(string path, bool isDestroyObj = false)
//        {
//            if (string.IsNullOrEmpty(path))
//            {
//                return false;
//            }
//
//            if (!_assetDic.TryGetValue(path, out var info) || info == null)
//            {
//                Debug.LogError(" AssetDic 里不存在该资源 ：" + path + " 可 能 释 放 了 多");
//            }
//
//            if (info != null)
//            {
//                info.RefCount--;
//                DestroyResourceItem(info, isDestroyObj);
//            }
//
//            return true;
//        }
//
//        /// <summary>
//        /// 缓存加载的资源
//        /// </summary>
//        /// <param name="path"></param>
//        /// <param name="info"></param>
//        /// <param name="obj"></param>
//        /// <param name="addRefCount"></param>
//        private void CacheResource(string path, ref ResourceInfo info, Object obj, int addRefCount = 1)
//        {
//            //缓存太多 清除最早没有使用的资源
//            WashOut();
//
//            if (info == null)
//            {
//                Debug.LogError("Resource Load is null , path : " + path);
//            }
//
//            if (obj == null)
//            {
//                Debug.LogError("Resource Load Fail : " + path);
//            }
//
//            info.MObj = obj;
//            info.MGuid = obj.GetInstanceID();
//            info.LastUseTime = Time.realtimeSinceStartup;
//            info.RefCount += addRefCount;
//
//            if (_assetDic.TryGetValue(info.Path, out _))
//            {
//                _assetDic[info.Path] = info;
//            }
//            else
//            {
//                _assetDic.Add(info.Path, info);
//            }
//        }
//
//        /// <summary>
//        /// 缓存太多清除最早没有使用的资源
//        /// </summary>
//        private void WashOut()
//        {
//            //当大于缓存个数时进行一半释放
//            while (_mNoReferenceAssetMapList.Size() >= MaxCacheCount)
//            {
//                for (int i = 0; i < MaxCacheCount / 2; i++)
//                {
//                    ResourceInfo info = _mNoReferenceAssetMapList.Back();
//                    DestroyResourceItem(info, true);
//                }
//            }
//        }
//
//        /// <summary>
//        /// 回收一个资源
//        /// </summary>
//        /// <param name="info"></param>
//        /// <param name="destroyCache"></param>
//        private void DestroyResourceItem(ResourceInfo info, bool destroyCache = false)
//        {
//            if (info == null || info.RefCount > 0)
//            {
//                return;
//            }
//
//            if (!destroyCache)
//            {
//                _mNoReferenceAssetMapList.InsertToHead(info);
//                return;
//            }
//
//            if (!_assetDic.Remove(info.Path))
//            {
//                return;
//            }
//
//            _mNoReferenceAssetMapList.Remove(info);
//
//            //释放asset bundle引用
//            AssetBundleManager.Instance.ReleaseAsset(info);
//
//            //清空资源对应的对象池
//            ObjectManager.Instance.ClearPoolObject(info.Path);
//
//            if (info.MObj != null)
//            {
//                info.MObj = null;
//#if UNITY_EDITOR
//                Resources.UnloadUnusedAssets();
//#endif
//            }
//        }
//
//#if UNITY_EDITOR
//        private T LoadAssetByEditor<T>(string path) where T : Object
//        {
//            return UnityEditor.AssetDatabase.LoadAssetAtPath<T>(path);
//        }
//#endif
//
//        /// <summary>
//        /// 从资源池获取缓存资源
//        /// </summary>
//        /// <param name="path"></param>
//        /// <param name="addRefCount"></param>
//        /// <returns></returns>
//        private ResourceInfo GetCacheResourceItem(string path, int addRefCount = 1)
//        {
//            if (_assetDic.TryGetValue(path, out var item))
//            {
//                if (item != null)
//                {
//                    item.RefCount += addRefCount;
//                    item.LastUseTime = Time.realtimeSinceStartup;
//
//                    //if (item.RefCount<=1)
//                    //{
//                    //    m_NoRefrenceAssetMapList.Remove(item);
//                    //}
//                }
//            }
//
//            return item;
//        }
//    }
//}