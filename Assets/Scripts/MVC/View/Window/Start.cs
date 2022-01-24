using bFrame.Game.Base;
using bFrame.Game.UIFrame;
using bFrame.Game.UIFrame.Base;
using MVC.Controller;
using UnityEngine;
using UnityEngine.UI;

namespace MVC.View.Window
{
    public class Start : UiDesignerBase
    {
        #region UI挂点

        [SerializeField] private Button startBtn;
        [SerializeField] private GameObject beginTe;

        #endregion

        public override void Init()
        {
            base.Init();

            //点击开始渐隐渐显效果
            Tools.PingpongTxt(beginTe.gameObject, 1, 0, 1);
            startBtn.onClick.AddListener(GoMenuScene);
        }

        private void GoMenuScene()
        {
            UiLogic.Close();
        }
    }
}