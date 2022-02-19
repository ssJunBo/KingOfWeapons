using System.Collections.Generic;
using bFrame.Game.Base;
using bFrame.Game.UIFrame.Base;

namespace bFrame.Game.UIFrame
{
    public class UiLogicManager : Singleton<UiLogicManager>
    {

        public UiLogicManager()
        {
            MessageDispatcher.Instance.RegisterMsgCallback((int)EDispatchMsg.Ui,OnDispatchMsg);
        }

        private void OnDispatchMsg(DelegateParam delegateParam)
        {

        }

        public void Release()
        {
            MessageDispatcher.Instance.UnRegisterMsgCallback((int)EDispatchMsg.Ui,OnDispatchMsg);
        }

        private readonly List<UiLogicBase> _ltUiLogicBse=new List<UiLogicBase>();
        
        public void AddUi(UiLogicBase ui)
        {
            if (!_ltUiLogicBse.Contains(ui))
            {
                _ltUiLogicBse.Add(ui);
                ui.DoOpen();
            }
        }

        public void RemoveUi(UiLogicBase ui)
        {
            if (_ltUiLogicBse.Contains(ui))
            {
                _ltUiLogicBse.Remove(ui);
            }
        }
    }
}
