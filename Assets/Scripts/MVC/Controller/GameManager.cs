using bFrame.Game.ResourceFrame;
using bFrame.Game.UIFrame;
using MVC.View.Window;
using UnityEngine;
using UnityEngine.EventSystems;

namespace MVC.Controller
{
    public class GameManager : MonoSingleton<GameManager>
    {
        [SerializeField] private bool loadFromAssetBundle;
        
        #region Manager 属性
        public UiManager UiManager { get; private set; }

        #endregion

        private void InitManager()
        {
            UiManager = new UiManager(transform.Find("UIRoot_2d") as RectTransform,
                transform.Find("UICamera").GetComponent<Camera>(),
                transform.Find("EventSystem").GetComponent<EventSystem>());

            new SceneManager(this);
        }

        protected override void Awake()
        {
            base.Awake();
            DontDestroyOnLoad(gameObject);
            InitManager();

            //从ab包加载就要先加载配置表
//            ResourceManager.Instance.MLoadFromAssetBundle = loadFromAssetBundle;
//            if (ResourceManager.Instance.MLoadFromAssetBundle)
//                AssetBundleManager.Instance.LoadAssetBundleConfig();
        
            ObjectManager.Instance.Init(transform.Find("RecyclePoolTrs"));
        }

        private void Start()
        {
            LoadConfig();

            //用到的窗口要进行注册
            RegisterUi();
            //预加载几个提示框
//            ObjectManager.Instance.PreLoadGameObject(ConStr.tipsItem_Path, 2);
//            ObjectManager.Instance.PreLoadGameObject(ConStr.talkItem_Path, 5);
        }

        private void Update()
        {
            if (Input.GetKeyDown(KeyCode.D))
            {
                ShowTips("你好啊全世界 ！" + Random.Range(0, 5).ToString());
            }
        }

        //注册ui窗口
        private void RegisterUi()
        {
            UiManager.Register<Menu>(ConStr._MenuPanel);
            UiManager.Register<Loading>(ConStr._LoadingPanel);
            UiManager.Register<ChapterDesigner>(ConStr._ChapterPanel);
            UiManager.Register<Talk>(ConStr._TalkPanel);
        }

        /// <summary>
        /// 加载配置表 需要什么配置表都在这里加载
        /// </summary>
        static void LoadConfig()
        {
            //ConfigerManager.Instance.LoadData<BuffData>(CFG.TABLE_BUFF);
            //ConfigerManager.Instance.LoadData<MonsterData>(CFG.TABLE_MONSTER);
        }

        /// <summary>
        /// 提示展示
        /// </summary>
        /// <param name="strContent"></param>
        public void ShowTips(string strContent)
        {
            GameObject tipObj = ObjectManager.Instance.SpwanObjFromPool(ConStr.tipsItem_Path, targetTransform: UiManager.MWndRoot);
            tipObj.GetComponent<TipsItem>().content.text = strContent;
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
