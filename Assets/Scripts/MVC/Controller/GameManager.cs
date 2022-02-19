using bFrame.Game.ResourceFrame;
using MVC.Model;
using UnityEngine;

namespace MVC.Controller
{
    public class GameManager : MonoSingleton<GameManager>
    {
        [SerializeField] private bool loadFromAssetBundle;

        [Header("普通 Designer 放在此节点下"),Space]public Transform Ui2DTransform;

        [Header("对象池回收节点"),Space]
        public Transform RecyclePoolTrs;

        
        #region moudle play
        private CMoudlePlay mMoudlePlay;

        public CMoudlePlay MoudlePlay
        {
            get
            {
                if (mMoudlePlay==null)
                {
                    mMoudlePlay=new CMoudlePlay();
                }
                return mMoudlePlay;
            }
        }
        #endregion

        protected override void Awake()
        {
            base.Awake();
           
            DontDestroyOnLoad(gameObject);
            
            InitManager();

            //从ab包加载就要先加载配置表
//            ResourceManager.Instance.MLoadFromAssetBundle = loadFromAssetBundle;
//            if (ResourceManager.Instance.MLoadFromAssetBundle)
//                AssetBundleManager.Instance.LoadAssetBundleConfig();
        
        }

        private void Start()
        {
            LoadConfig();

            MoudlePlay.UiStartWndLogic.Open();
        }
        
        private void InitManager()
        {
          
        }

        /// <summary>
        /// 加载配置表 需要什么配置表都在这里加载
        /// </summary>
        static void LoadConfig()
        {
            //ConfigerManager.Instance.LoadData<BuffData>(CFG.TABLE_BUFF);
            //ConfigerManager.Instance.LoadData<MonsterData>(CFG.TABLE_MONSTER);
        }
        

        private void OnApplicationQuit()
        {
#if UNITY_EDITOR
            ResourcesManager.Instance.ClearCache();
            Resources.UnloadUnusedAssets();
            Debug.Log("application退出操作，同时清 空 编 辑 器 缓 存 ！");
#endif
        }
    }
}
