using System.Collections.Generic;
using System.Linq;
using bFrame.Game.UIFrame.Base;
using NUnit.Framework;

namespace bFrame.Game.UIFrame
{
    public class BindUi
    {
        public BindUi(EUiForType type,int id)
        {
            Type = type;
            Id = id;
        }

        public EUiForType Type { get; } = EUiForType.UiNone;

        public int Id { get; }

        public bool EqualsWith(BindUi other)
        {
            return other.Id == Id && other.Type == Type;
        }
    }
    public class UiLogicManager : bFrame.Game.Base.Singleton<UiLogicManager>
    {
        /// <summary>
        /// 在UI上派发的消息
        /// </summary>
        public class UiMessage
        {
            public enum EDispatchType
            {
                None,
                UiAll,        //向所以UI派发
                UiDirect,     //向单个UI派发
            }

            public enum EMessageType
            {
                None,
                OpenStart,
                OpenFinish,
                Close,
            }

            public EDispatchType MessageType = EDispatchType.None;
            public int UiNo;        //UI编号，使用UI枚举
        }

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

        public void Update(float fDeltaTime)
        {
            foreach (var uiLogicBase in _ltUiLogicBse.Where(t => t!=null))
            {
                uiLogicBase.Update(fDeltaTime);
            }
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
    }

    public enum EUiForType
    {
        UiNone,
        UiPackage,
    }
}
