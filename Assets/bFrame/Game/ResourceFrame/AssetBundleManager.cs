using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Serialization.Formatters.Binary;
using bFrame.Game.Base;
using UnityEngine;

namespace bFrame.Game.ResourceFrame
{
    public class AssetBundleManager : Singleton<AssetBundleManager>
    {
        private string m_ABConfigABName = "assetbundleconfig";

        //资源关系依赖配表 可以根据路径来找到对应的资源块
        private readonly Dictionary<string, ResourceInfo> _mResourceItemDic = new Dictionary<string, ResourceInfo>();

        //储存已加载的ab包，key为名字
        private readonly Dictionary<string, AssetBundleItem> _mAssetBundleItemDic = new Dictionary<string, AssetBundleItem>();

        //AssetBundleItem类对象池
        private readonly ClassObjectPool<AssetBundleItem> _mAssetBundleItemPool =
            ObjectManager.Instance.GetOrCreateClassPool<AssetBundleItem>(500);

        private string ABLoadPath => Application.streamingAssetsPath + "/";

        /// <summary>
        /// 加载ab配置表
        /// </summary>
        /// <returns></returns>
        public bool LoadAssetBundleConfig()
        {
#if UNITY_EDITOR
            if (!ResourcesManager.Instance.isLoadFromAssetBundle)
                return false;
#endif

            _mResourceItemDic.Clear();
            string configPath = ABLoadPath + m_ABConfigABName;
            AssetBundle configAb = AssetBundle.LoadFromFile(configPath);
            TextAsset textAsset = configAb.LoadAsset<TextAsset>(m_ABConfigABName);
            if (textAsset == null)
            {
                Debug.LogError("AssetBundleConfig is no exist !");
                return false;
            }

            //解析 反序列化
            MemoryStream stream = new MemoryStream(textAsset.bytes);
            BinaryFormatter bf = new BinaryFormatter();
            AssetBundleConfig config = (AssetBundleConfig) bf.Deserialize(stream);
            stream.Close();

            foreach (var item in config.ABList.Select(abBase => new ResourceInfo
            {
                Path = abBase.Path,
                MAssetName = abBase.AssetName,
                MAbName = abBase.ABName,
                MDependAssetBundle = abBase.ABDependce
            }))
            {
                if (_mResourceItemDic.ContainsKey(item.Path))
                {
                    Debug.LogError("重复的CRC：资源名：" + item.MAbName + " ab包名" + item.MAbName);
                }
                else
                {
                    _mResourceItemDic.Add(item.Path, item);
                }
            }

            return true;
        }

       /// <summary>
       /// 根据路径加载中间类ResourceItem
       /// </summary>
       /// <param name="path"></param>
       /// <returns></returns>
        public ResourceInfo LoadResourceAssetBundle(string path)
        {
            if (!_mResourceItemDic.TryGetValue(path, out var item) || item == null)
            {
                Debug.LogError($"LoadResourceAssetBundle error : can not find path {path} in AssetBundleConfig");
                return null;
            }

            if (item.MAssetBundle != null)
            {
                return item;
            }

            item.MAssetBundle = LoadAssetBundle(item.MAbName);
            if (item.MDependAssetBundle != null)
            {
                foreach (var depItem in item.MDependAssetBundle)
                {
                    LoadAssetBundle(depItem);
                }
            }

            return item;
        }

        /// <summary>
        /// 加载单个AssetBundle根据名字
        /// </summary>
        /// <param name="abName"></param>
        /// <returns></returns>
        private AssetBundle LoadAssetBundle(string abName)
        {
            if (!_mAssetBundleItemDic.TryGetValue(abName, out var item))
            {
                string fullPath = ABLoadPath + abName;
                var assetBundle = AssetBundle.LoadFromFile(fullPath);
                if (assetBundle == null)
                {
                    Debug.LogError(" Load AssetBundle Error : " + fullPath);
                }

                item = _mAssetBundleItemPool.Spawn(true);
                item.assetBundle = assetBundle;
                item.RefCount++;
                _mAssetBundleItemDic.Add(abName, item);
            }
            else
            {
                item.RefCount++;
            }

            return item.assetBundle;
        }

        /// <summary>
        /// 释放资源
        /// </summary>
        /// <param name="info"></param>
        public void ReleaseAsset(ResourceInfo info)
        {
            if (info == null)
            {
                return;
            }

            if (info.MDependAssetBundle != null && info.MDependAssetBundle.Count > 0)
            {
                foreach (var depItem in info.MDependAssetBundle)
                {
                    UnLoadAssetBundle(depItem);
                }
            }

            UnLoadAssetBundle(info.MAbName);
        }

        private void UnLoadAssetBundle(string abName)
        {
            if (_mAssetBundleItemDic.TryGetValue(abName, out var item))
            {
                item.RefCount--;
                if (item.RefCount <= 0 && item.assetBundle != null)
                {
                    item.assetBundle.Unload(true);
                    item.Rest();
                    _mAssetBundleItemPool.Recycle(item);
                    _mAssetBundleItemDic.Remove(abName);
                }
            }
        }

        /// <summary>
        /// 根据crc查找ResourceItem
        /// </summary>
        /// <param name="crc"></param>
        /// <returns></returns>
        public ResourceInfo FindResourceItem(string path)
        {
            _mResourceItemDic.TryGetValue(path, out var item);
            return item;
        }
    }

    public class AssetBundleItem
    {
        public AssetBundle assetBundle = null;
        public int RefCount;

        public void Rest()
        {
            assetBundle = null;
            RefCount = 0;
        }
    }

   
}