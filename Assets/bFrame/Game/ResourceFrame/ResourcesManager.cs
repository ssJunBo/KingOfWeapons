using System.Collections.Generic;
using System.Linq;
using bFrame.Game.Base;
using UnityEngine;
using Object = UnityEngine.Object;

namespace bFrame.Game.ResourceFrame
{
    /// <summary>
    /// asset bundle中加载出来的对象 还未实例化 例如图片之类可以直接使用  
    /// </summary>
    public class ResourceInfo
    {
        public bool IsFromResources = false;

        //资源路径
        public string Path;

        //资源对象
        public Object Obj;

        //是否跳场景清掉
        public bool ChangeSceneIsClear = true;

        //最后一次使用时间
        public float LastUseTime;

        //引用计数
        public int RefCount { get; set; }

        public long Guid;
    }

    /// <summary>
    /// asset bundle中 实例化出来的obj
    /// </summary>
    public class ResourceObj
    {
        public string Path;

        public ResourceInfo resInfo;
        
        public GameObject CloneObj;

        public bool IsClear;

        public bool Already;

        public int Guid;
        public void Reset()
        {
            
        }
    }


    public class ResourcesManager : Singleton<ResourcesManager>
    {
        private long _mGuid = 0;
        public const bool IsLoadFromAssetBundle = false;

        /// <summary>
        /// 缓存使用的资源列表  key = 路径 
        /// </summary>
        private readonly Dictionary<string, ResourceInfo> _assetDic = new Dictionary<string, ResourceInfo>();

        /// <summary>
        /// 缓存应用为零的资源列表，达到缓存最大的时 释放这个列表里面最早没用的资源
        /// </summary>
        private readonly CMapList<ResourceInfo> _mNoReferenceAssetMapList = new CMapList<ResourceInfo>();

        //最长连续卡着加载资源的时间 单位微秒
        private const long MaxLoadTime = 200000;

        //最大缓存个数 中配 500 高配 1000 低配 200 复杂处理（搜索 unity3d获取内存大小）
        private const int MaxCacheCount = 500;


        /// <summary>
        /// 创建唯一的GUID
        /// </summary>
        /// <returns></returns>
        public long CreateGuid()
        {
            return _mGuid++;
        }

        /// <summary>
        /// 清空缓存 一般用于跳场景
        /// </summary>
        public void ClearCache()
        {
            List<ResourceInfo> tempList = _assetDic.Values.Where(item => item.ChangeSceneIsClear).ToList();

            foreach (var item in tempList)
            {
                DestroyResourceItem(item, item.ChangeSceneIsClear);
            }

            tempList.Clear();
        }


        public delegate void DelegateResourceLoaded(string path, Object obj);

        /// <summary>
        /// 同步加载资源 针对给ObjectManager的接口
        /// </summary>
        /// <param name="path"></param>
        /// <param name="onLoaded"></param>
        /// <param name="isFromResources"></param>
        public void LoadResource(string path, DelegateResourceLoaded onLoaded, bool isFromResources = true)
        {
            ResourceInfo resourceItem = GetCacheResourceItem(path);

            if (resourceItem != null)
            {
                if (resourceItem.Obj != null)
                {
                    onLoaded?.Invoke(path, resourceItem.Obj);
                }
            }

            resourceItem = new ResourceInfo
            {
                IsFromResources = isFromResources,
                Path = path
            };

            resourceItem.RefCount++;

            _assetDic[path] = resourceItem;
        }

        /// <summary>
        /// 同步资源加载 外部直接调用 仅加载不需要实例化的资源 例如texture 音频等
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="path"></param>
        /// <returns></returns>
        public T LoadResource<T>(string path) where T : Object
        {
            if (string.IsNullOrEmpty(path))
            {
                return null;
            }

            ResourceInfo info = GetCacheResourceItem(path);

            if (info != null)
            {
                return info.Obj as T;
            }

#if UNITY_EDITOR
            info = new ResourceInfo {Path = path};
            var obj = LoadAssetByEditor<T>(path);
#endif

            CacheResource(path, ref info, obj);
            return obj;
        }


        /// <summary>
        /// 实例化出来的资源 根据ResourceObj卸载资源
        /// </summary>
        /// <param name="resObj"></param>
        /// <param name="isDestroyObj"></param>
        /// <returns></returns>
        public bool ReleaseResource(ResourceObj resObj, bool isDestroyObj = false)
        {
            if (resObj == null) return false;

            if (!_assetDic.TryGetValue(resObj.Path, out var info) || info == null)
            {
                Debug.Log("AssetDic里不存在该资源: " + resObj.CloneObj.name + " 可能释放多次！");
            }

            Object.Destroy(resObj.CloneObj);

            if (info != null)
            {
                info.RefCount--;
                DestroyResourceItem(info, isDestroyObj);
            }

            return true;
        }

        /// <summary>
        /// 不需要实例化的资源卸载，根据对象去释放
        /// </summary>
        /// <param name="obj"></param>
        /// <param name="isDestroyObj"></param>
        /// <returns></returns>
        private bool ReleaseResource(Object obj, bool isDestroyObj = false)
        {
            if (obj == null)
            {
                return false;
            }

            ResourceInfo info = null;
            foreach (var res in _assetDic.Values.Where(res => res.Guid == obj.GetInstanceID()))
            {
                info = res;
            }

            if (info == null)
            {
                Debug.LogError("AssetDic 里不存在该资源：" + obj.name + " 可能释放了多次");
                return false;
            }

            info.RefCount--;
            DestroyResourceItem(info, isDestroyObj);
            return true;
        }

        /// <summary>
        /// 不需要实例化的资源卸载，根据路径
        /// </summary>
        /// <param name="path"></param>
        /// <param name="isDestroyObj"></param>
        /// <returns></returns>
        public bool ReleaseResource(string path, bool isDestroyObj = false)
        {
            if (string.IsNullOrEmpty(path))
            {
                return false;
            }

            if (!_assetDic.TryGetValue(path, out var info) || info == null)
            {
                Debug.LogError(" AssetDic 里不存在该资源 ：" + path + " 可 能 释 放 了 多");
            }

            if (info != null)
            {
                info.RefCount--;
                DestroyResourceItem(info, isDestroyObj);
            }

            return true;
        }

        /// <summary>
        /// 缓存加载的资源
        /// </summary>
        /// <param name="path"></param>
        /// <param name="info"></param>
        /// <param name="obj"></param>
        /// <param name="addRefCount"></param>
        private void CacheResource(string path, ref ResourceInfo info, Object obj, int addRefCount = 1)
        {
            //缓存太多 清除最早没有使用的资源
            WashOut();

            if (info == null)
            {
                Debug.LogError("Resource Load is null , path : " + path);
            }

            if (obj == null)
            {
                Debug.LogError("Resource Load Fail : " + path);
            }

            if (info != null)
            {
                info.Obj = obj;
                info.Guid = obj.GetInstanceID();
                info.LastUseTime = Time.realtimeSinceStartup;
                info.RefCount += addRefCount;

                if (_assetDic.TryGetValue(info.Path, out _))
                {
                    _assetDic[info.Path] = info;
                }
                else
                {
                    _assetDic.Add(info.Path, info);
                }
            }
        }

        /// <summary>
        /// 缓存太多清除最早没有使用的资源
        /// </summary>
        private void WashOut()
        {
            //当大于缓存个数时进行一半释放
//            while (_mNoReferenceAssetMapList.Size() >= MaxCacheCount)
//            {
//                for (int i = 0; i < MaxCacheCount / 2; i++)
//                {
//                    ResourceInfo info = _mNoReferenceAssetMapList.Back();
//                    DestroyResourceItem(info, true);
//                }
//            }
        }

        /// <summary>
        /// 回收一个资源
        /// </summary>
        /// <param name="info"></param>
        /// <param name="destroyCache"></param>
        private void DestroyResourceItem(ResourceInfo info, bool destroyCache = false)
        {
            if (info == null || info.RefCount > 0)
            {
                return;
            }

            if (!destroyCache)
            {
                _mNoReferenceAssetMapList.InsertToHead(info);
                return;
            }

            if (!_assetDic.Remove(info.Path))
            {
                return;
            }

            _mNoReferenceAssetMapList.Remove(info);

            //清空资源对应的对象池
            ObjectManager.Instance.ClearPoolObject(info.Path);

            if (info.Obj != null)
            {
                info.Obj = null;
#if UNITY_EDITOR
                Resources.UnloadUnusedAssets();
#endif
            }
        }

#if UNITY_EDITOR
        private T LoadAssetByEditor<T>(string path) where T : Object
        {
            return UnityEditor.AssetDatabase.LoadAssetAtPath<T>(path);
        }
#endif

        /// <summary>
        /// 从资源池获取缓存资源
        /// </summary>
        /// <param name="path"></param>
        /// <param name="addRefCount"></param>
        /// <returns></returns>
        private ResourceInfo GetCacheResourceItem(string path, int addRefCount = 1)
        {
            if (_assetDic.TryGetValue(path, out var item))
            {
                if (item != null)
                {
                    item.RefCount += addRefCount;
                    item.LastUseTime = Time.realtimeSinceStartup;
                }
            }

            return item;
        }

        public void IncreaseResourceRef(string path)
        {
            
        }
    }
}