using System.Collections.Generic;
using System.Linq;
using bFrame.Game.ResourceFrame;
using UnityEngine;

namespace bFrame.Game.UIFrame.Base
{
    public class UiLogicBase
    {
        protected enum UiResourceType
        {
            EUi, //2D ngui
        }

        protected class UiResourcesInfo
        {
            public UiResourceType EType = UiResourceType.EUi;
            public string StrResourcePath;
            public bool BIsFromResources = false; //是否是Resource文件夹加载
            public bool BNeedLoad = true;

            public Object ResObj { get; set; }
        }

        public delegate void DelegateClose();

        public DelegateClose EventClose = null;

        protected readonly List<UiResourcesInfo> LtNeedResources = new List<UiResourcesInfo>();

        private bool _mBShowing = false;

        private bool _beforeOpen = false;

        private object[] Paras { get; } = null;

        //实际打开
        protected virtual void Open()
        {
            if (_mBShowing || _beforeOpen)
                return;

            _beforeOpen = true;
            UiLogicManager.Instance.AddUi(this);
        }

        protected virtual void AnalysParas(int nPara1, int nPara2)
        {

        }

        public virtual void Close()
        {

        }

        public virtual void Update(float fDeltaTime)
        {
        }

        protected void AddNeedResources(string strPath, UiResourceType eType, bool bIsFromResources = true)
        {
            if (LtNeedResources.Any(t => t.StrResourcePath == strPath))
                return;

            UiResourcesInfo info = new UiResourcesInfo
            {
                StrResourcePath = strPath,
                EType = eType,
                BIsFromResources = bIsFromResources
            };

            LtNeedResources.Add(info);
        }

        public void DoOpen()
        {
            #region ---解析传入参数 暂时支持两个int类型

            int nPara1 = 0;
            int nPara2 = 0;
            if (Paras != null)
            {
                if (Paras.Length >= 1)
                {
                    if (Paras[0] is int)
                    {
                        nPara1 = (int) Paras[0];
                    }
                }

                if (Paras.Length >= 2)
                {
                    if (Paras[1] is int)
                    {
                        nPara2 = (int) Paras[1];
                    }
                }
            }

            AnalysParas(nPara1, nPara2);

            #endregion

            _beforeOpen = false;
            _mBShowing = true;

            //MessageDispatcher.Instance.DispatchMessage(EDispatchMsg.UiOpen, Bind);
            foreach (var res in LtNeedResources.Where(res => res.ResObj == null))
            {
                res.BNeedLoad = true;
                ResourcesManager.Instance.LoadResource(res.StrResourcePath, OnLoaded);
            }
        }

        public virtual EUiForType UiType()
        {
            return EUiForType.UiNone;
        }

        private void OnLoaded(string path, Object obj)
        {
            var item = LtNeedResources.Find(uiResourcesInfo => uiResourcesInfo.StrResourcePath == path);
            item.ResObj = obj;
        }
    }
}
