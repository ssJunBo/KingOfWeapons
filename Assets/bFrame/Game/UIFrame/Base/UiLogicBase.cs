using System.Threading;
using bFrame.Game.ResourceFrame;
using bFrame.Game.Tools;
using MVC.Controller;
using UnityEngine;
using Object = UnityEngine.Object;

namespace bFrame.Game.UIFrame.Base
{
    public class UiLogicBase
    {
        private bool _isShowing;

        private bool _beforeOpen;

        private UiDesignerBase _mDesigner;

        private string _mainPath;

        protected void SetPath(string path)
        {
            _mainPath = path;
        }

        //实际打开
        public virtual void Open()
        {
            if (_isShowing || _beforeOpen)
                return;

            _beforeOpen = true;

            UiLogicManager.Instance.AddUi(this);
        }


        public virtual void Close()
        {
            UiLogicManager.Instance.RemoveUi(this);

            if (_mDesigner != null)
            {
                _mDesigner.Release();
                _mDesigner = null;
            }
        }

        protected internal void DoOpen()
        {
            _beforeOpen = false;

            _isShowing = true;

            ResourcesManager.Instance.LoadResource(_mainPath, HandleUiResourceOk);
        }

        private void HandleUiResourceOk(string path, Object obj)
        {
            if (!_isShowing) return;

            if (obj != null)
            {
                GameObject mainObj = Object.Instantiate(obj, GameManager.Instance.Ui2DTransform) as GameObject;

                if (mainObj!=null)
                {
                    _mDesigner = mainObj.GetComponent<UiDesignerBase>();

                    if (_mDesigner == null)
                    {
                        Debug.LogError("cant find designer component : " + obj.name);
                    }
                    else
                    {
                        InitLogic();

                        _mDesigner.SetLogic(this);

                        _mDesigner.Init();
                        Debug.LogError("前 "+CTools.TickCount());
                        //延迟一帧，当ui真正绘制出来以后，在调用ShowFinished 这样一些坐标转换，和一些UI操作才不会出错
                        //UI的显示操作都应该放在ShowFinished中去做，而不应该在Init中去做 
                        TimeCallback.Instance.DelayHowManySecondsAfterCallBack(1, () =>
                        {
                            _mDesigner.ShowFinished();
                            Debug.LogError("后 " + CTools.TickCount());
                        });
                        
                        //TODO 
                        _mDesigner.ShowFinished();
                    }
                }
                else
                {
                    //加载窗口失败，返回初始化失败
                    Close();
                    Debug.LogError("加载窗口失败！path = " + path);
                }
            }
        }

        //注册游戏逻辑的委托事件
        protected virtual void InitLogic()
        {

        }
    }
}
