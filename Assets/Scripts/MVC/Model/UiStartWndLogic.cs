using bFrame.Game.UIFrame;
using bFrame.Game.UIFrame.Base;

namespace MVC.Model
{
    public class UiStartWndLogic : UiLogicBase
    {
        private const string MainWndPath = @"Assets/Resource/Prefabs/UI/Wnd/startWnd.prefab";

        protected override void Open()
        {
            AddNeedResources(MainWndPath, UiResourceType.EUi);
            base.Open();
        }

        public override void Close()
        {
            base.Close();
        }

    }
}