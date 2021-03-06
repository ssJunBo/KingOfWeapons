using bFrame.Game.UIFrame.Base;
using MVC.Controller;
using MVC.Model.UiLogic;
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

        private UiStartWndLogic UiStartWndLogic; 
        public override void Init()
        {
            base.Init();

            UiStartWndLogic = (UiStartWndLogic)UiLogic;
            
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