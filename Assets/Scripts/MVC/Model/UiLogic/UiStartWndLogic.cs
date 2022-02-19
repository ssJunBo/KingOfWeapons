using bFrame.Game.UIFrame.Base;

namespace MVC.Model.UiLogic
{
    public class UiStartWndLogic : UiLogicBase
    {
        private const string MainWndPath = "Prefabs/Wnd/startWnd";

        public override void Open()
        {
            SetPath(MainWndPath);
            base.Open();
        }
    }
}