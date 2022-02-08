using bFrame;
using bFrame.Game.Base;
using bFrame.Game.UIFrame;
using bFrame.Game.UIFrame.Base;
using MVC.Controller;
using UnityEngine;
using UnityEngine.UI;

namespace MVC.View.Window
{
    public class Loading : UiDesignerBase
    {
        private string _mSceneName;

        [SerializeField] private Slider mSlider;
        [SerializeField] private Text mText;
    

        public override void Init()
        {
            
        }

        public  void Update()
        {
            mSlider.value = SceneManager.LoadingProgress / 100.0f;
            mText.text = $"{SceneManager.LoadingProgress}%";
            if (SceneManager.LoadingProgress >= 99)
            {
                LoadOtherScene();
            }
        }

        /// <summary>
        /// 加载对应场景第一个ui
        /// </summary>
        private void LoadOtherScene()
        {
            //根据场景名字打开对应场景第一个界面
            switch (_mSceneName)
            {
                case ConStr.MENUSCENE:
                    GameManager.Instance.UiManager.PopUpWnd(ConStr._MenuPanel);
                    break;
            }
            GameManager.Instance.UiManager.CloseWnd(ConStr._LoadingPanel);
        }
    }
}