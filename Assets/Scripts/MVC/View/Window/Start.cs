using bFrame.Game.Base;
using bFrame.Game.UIFrame;
using bFrame.Game.UIFrame.Base;
using MVC.Controller;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace MVC.View.Window
{
    public class Start : UiDesignerBase
    {
        #region UI挂点

        [SerializeField] private Button startBtn;
        [SerializeField] private CanvasGroup canvasGroup;

        #endregion

        public override void Init()
        {
            base.Init();

            //点击开始渐隐渐显效果
            Tools.PingPongAnim(canvasGroup);
            startBtn.onClick.AddListener(GoMenuScene);
        }

        private void GoMenuScene()
        {
            Debug.Log("按钮点击执行");
            UiLogic.Close();
        }
    }
}