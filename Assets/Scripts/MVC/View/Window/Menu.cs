using bFrame;
using bFrame.Game.Base;
using bFrame.Game.ResourceFrame;
using bFrame.Game.UIFrame;
using bFrame.Game.UIFrame.Base;
using MVC.Controller;
using UnityEngine;
using UnityEngine.UI;

namespace MVC.View.Window
{
    public class Menu : UiDesignerBase
    {
        [SerializeField] private Button pianzhangBtn;
        [SerializeField] private Button chengjiuBtn;
        [SerializeField] private Button newgameBtn;
        [SerializeField] private Button goonBtn;
        [SerializeField] private Image headImg;
        [SerializeField] private  Image coinImg;
        [SerializeField] private  Text coinTe;

        public override void Init()
        {

            AddButtonClickListener(pianzhangBtn, OnClickChapter);
            AddButtonClickListener(chengjiuBtn, OnClickAchievement);
            AddButtonClickListener(newgameBtn, OnClickNewGame);
            AddButtonClickListener(goonBtn, OnClickGoOn);

//            headImg.sprite =
//                ResourcesManager.Instance.LoadResource<Sprite>("Assets/GameData/UGUI/test/test2.png");


            //ResourceManager.Instance.AsyncLoadResource("Assets/GameData/UGUI/test/test1.png", OnLoadSpriteTest1, ELoadResPriority.RES_SLOW, true);
        }

        #region 测试异步加载资源

        private void LoadMonsterData()
        {
            var monsterData = ConfigerManager.Instance.LoadData<MonsterData>(CFG.TABLE_MONSTER);
            foreach (var t in monsterData.AllMonster)
            {
                Debug.Log($"ID:{t.Id} 名字:{t.Name} 外观:{t.OutLook} 高度:{t.Height} 稀有度:{t.Rare}");
            }
        }

       private void OnLoadSpriteTest1(string path, Object obj, object param1 = null, object param2 = null,
            object param3 = null)
        {
            if (obj != null)
            {
                Sprite sp = obj as Sprite;
            }
        }

        #endregion

        private void OnClickChapter()
        {
            GameManager.Instance.UiManager.PopUpWnd(ConStr._ChapterPanel);
        }

        private void OnClickAchievement()
        {
            GameManager.Instance.ShowTips("点击了成就按钮！");
        }

        private void OnClickNewGame()
        {
            GameManager.Instance.UiManager.PopUpWnd(ConStr._TalkPanel);
            GameManager.Instance.UiManager.HideWnd(ConStr._MenuPanel);
        }

        private void OnClickGoOn()
        {
            Debug.Log("点击了继续游戏");
        }
    }
}
