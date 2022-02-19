using bFrame.Game.ResourceFrame;
using MVC.Controller;
using UnityEngine;
using Object = UnityEngine.Object;

namespace bFrame.Game.UIFrame.Base
{
    public class UiLogicBase
    {
        private bool IsShowing;

        private bool _beforeOpen;

        protected UiDesignerBase mDesigner;

        private Vector3 _vector3 = new Vector3(-9999, -9999, -9999);

        private string mainPath;

        protected void SetPath(string path)
        {
            mainPath = path;
        }
        //实际打开
        public virtual void Open()
        {
            if (IsShowing || _beforeOpen)
                return;

            _beforeOpen = true;
            
            UiLogicManager.Instance.AddUi(this);
        }
        

        public virtual void Close()
        {
            UiLogicManager.Instance.RemoveUi(this);

            if (mDesigner != null)
            {
                mDesigner.Release();
                mDesigner = null;
            }
        }

        protected internal void DoOpen()
        {
            _beforeOpen = false;
            
            IsShowing = true;

            ResourcesManager.Instance.LoadResource(mainPath, HandleUiResourceOk);
        }

        private void HandleUiResourceOk(string path, Object obj)
        {
            if (!IsShowing) return;

            if (obj != null)
            {
                GameObject mainObj = Object.Instantiate(obj,GameManager.Instance.Ui2DTransform) as GameObject;

                if (mainObj)
                {
                    mDesigner = mainObj.GetComponent<UiDesignerBase>();

                    if (mDesigner == null)
                    {
                        Debug.LogError("cant find designer component : " + obj.name);
                        return;
                    }
                    else
                    {
                        InitLogic();

                        mDesigner.SetLogic(this);

                        mDesigner.Init();
                        //延迟一帧，当ui真正绘制出来以后，在调用ShowFinished 这样一些坐标转换，和一些UI操作才不会出错
                        //UI的显示操作都应该放在ShowFinished中去做，而不应该在Init中去做 

                        //TODO 
                        mDesigner.ShowFinished();
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
