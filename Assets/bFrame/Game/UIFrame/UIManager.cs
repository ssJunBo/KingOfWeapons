using System;
using System.Collections.Generic;
using bFrame.Game.Base;
using bFrame.Game.ResourceFrame;
using bFrame.Game.UIFrame.Base;
using UnityEngine;
using UnityEngine.EventSystems;

namespace bFrame.Game.UIFrame
{
    public class UiManager
    {
        //UI节点
        private RectTransform _mUiRoot;

        //窗口节点
        public readonly RectTransform MWndRoot;

        //UI摄像机
        private Camera _mUiCamera;

        //EventSystem 节点
        private EventSystem _mEventSystem;

        //屏幕的宽高比
        private float _mCanvasRate = 0;

        //窗口预制体存放地址
        private string _mUiPrefabPath = "Assets/Prefabs/UI/Wnd/";

        /// <summary>
        /// 注册的字典
        /// </summary>
        private readonly Dictionary<string, Type> _mRegisterDic = new Dictionary<string, Type>();

        /// <summary>
        /// 所有打开的窗口
        /// </summary>
        private readonly Dictionary<string, UiDesignerBase> _mWindowDic = new Dictionary<string, UiDesignerBase>();

        /// <summary>
        /// 打开的窗口列表
        /// </summary>
        private readonly List<UiDesignerBase> _mWindowList = new List<UiDesignerBase>();

        public UiManager(RectTransform mWndRoot, Camera uiCamera, EventSystem eventSystem)
        {
            MWndRoot = mWndRoot;
            _mUiCamera = uiCamera;
            _mEventSystem = eventSystem;
            _mCanvasRate = Screen.height / (_mUiCamera.orthographicSize * 2);
        }

        /// <summary>
        /// 设置所有节目UI路径
        /// </summary>
        /// <param name="path"></param>
        public void SetUiPrefabPath(string path)
        {
            _mUiPrefabPath = path;
        }

        /// <summary>
        /// 显示或者隐藏所有UI
        /// </summary>
        public void SetUiState(bool show)
        {
            if (_mUiRoot != null)
            {
                _mUiRoot.gameObject.SetActive(show);
            }
        }
        
//        public void OnUpdate()
//        {
//            for (int i = 0; i < _mWindowList.Count; i++)
//            {
//                if (_mWindowList[i] != null)
//                {
//                    _mWindowList[i].OnUpdate();
//                }
//            }
//        }

        /// <summary>
        /// 窗口注册方法
        /// </summary>
        /// <typeparam name="T">窗口泛型类</typeparam>
        /// <param name="name">窗口名</param>
        public void Register<T>(string name) where T : UiDesignerBase
        {
            _mRegisterDic[name] = typeof(T);
        }

        /// <summary>
        /// 发送消息给窗口
        /// </summary>
        /// <param name="name">窗口名</param>
        /// <param name="msgId">消息ID</param>
        /// <param name="paraList">参数数组</param>
        /// <returns></returns>
        public bool SendMessageToWnd(string name, EuiMsgId msgId = 0, params object[] paraList)
        {
            var wnd = FindWndByName<UiDesignerBase>(name);
            
            return wnd != null && wnd.OnMessage(msgId, paraList);
        }

        /// <summary>
        /// 根据窗口名查找窗口
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="name"></param>
        /// <returns></returns>
        private T FindWndByName<T>(string name) where T : UiDesignerBase
        {
            if (_mWindowDic.TryGetValue(name, out var wnd))
            {
                return (T) wnd;
            }

            return null;
        }

        /// <summary>
        /// 打开窗口 如果不存在则加载 存在直接show出来
        /// </summary>
        /// <param name="wndName"></param>
        /// <returns></returns>
        public UiDesignerBase PopUpWnd(string wndName)
        {
            var wnd = FindWndByName<UiDesignerBase>(wndName);
            if (wnd == null || wnd.GameObject.activeSelf)
            {
                if (_mRegisterDic.TryGetValue(wndName, out var tp))
                {
                    //继承自mono的无法用此方法实例化出来
                    wnd = Activator.CreateInstance(tp) as UiDesignerBase;
                }
                else
                {
                    Debug.LogError("找不到窗口对应的脚本，窗口名是：" + wndName);
                    return null;
                }

                GameObject wndObj = ObjectManager.Instance.SpwanObjFromPool(_mUiPrefabPath + wndName, false, false);
                if (wndObj == null)
                {
                    Debug.Log("创建创建口Prefab失败：" + wndName);
                    return null;
                }

                if (!_mWindowDic.ContainsKey(wndName))
                {
                    _mWindowDic.Add(wndName, wnd);
                    _mWindowList.Add(wnd);
                }

                //可以不用写 减少GC
                //wnd.name = wndName;
                if (wnd != null)
                {
                    wnd.GameObject = wndObj;
                    wnd.Transform = wndObj.transform;
                    wnd.Name = wndName;
                    wnd.Init();
                    wndObj.transform.SetParent(MWndRoot, false);
                    wndObj.transform.SetAsLastSibling();

                    wnd.ShowFinished();

                    if (wnd.GameObject != null && !wnd.GameObject.activeSelf)
                    {
                        wnd.GameObject.SetActive(true);
                    }
                }
            }
            else
            {
                ShowWnd(wndName);
            }

            return wnd;
        }

        /// <summary>
        /// 根据窗口名关闭窗口
        /// </summary>
        /// <param name="name"></param>
        /// <param name="destroy"></param>
        public void CloseWnd(string name, bool destroy = false)
        {
            UiDesignerBase wnd = FindWndByName<UiDesignerBase>(name);
            CloseWnd(wnd, destroy);
        }

        /// <summary>
        /// 根据窗口对象关闭窗口 从字典中移除
        /// </summary>
        /// <param name="window"></param>
        /// <param name="destroy"></param>
        public void CloseWnd(UiDesignerBase window, bool destroy = false)
        {
            if (window == null) return;
            window.Release();
            if (_mWindowDic.ContainsKey(window.Name))
            {
                _mWindowDic.Remove(window.Name);
                _mWindowList.Remove(window);
            }

            if (destroy)
            {

                ObjectManager.Instance.ReleaseObject(window.GameObject, 0, true);
            }
            else
            {
                ObjectManager.Instance.ReleaseObject(window.GameObject, recycleParent: false);
            }

            window.GameObject = null;
            window = null;
        }

        /// <summary>
        /// 关闭所有窗口
        /// </summary>
        public void CloseAllWnd()
        {
            for (int i = _mWindowList.Count - 1; i >= 0; i--)
            {
                CloseWnd(_mWindowList[i]);
            }
        }

        /// <summary>
        /// 切换到唯一窗口
        /// </summary>
        public void SwitchStateByName(string name)
        {
            CloseAllWnd();
            PopUpWnd(name);
        }

        /// <summary>
        /// 根据名字隐藏窗口
        /// </summary>
        /// <param name="name"></param>
        public void HideWnd(string name)
        {
            UiDesignerBase wnd = FindWndByName<UiDesignerBase>(name);
            HideWnd(wnd);
        }

        /// <summary>
        /// 根据窗口对象隐藏窗口
        /// </summary>
        /// <param name="wnd"></param>
        public void HideWnd(UiDesignerBase wnd)
        {
            if (wnd != null)
            {
                wnd.GameObject.SetActive(false);
            }
        }

        /// <summary>
        /// 根据窗口名字显示窗口
        /// </summary>
        /// <param name="name"></param>
        /// <param name="bTop"></param>
        /// <param name="paraList"></param>
        public void ShowWnd(string name, bool bTop = true, params object[] paraList)
        {
            UiDesignerBase wnd = FindWndByName<UiDesignerBase>(name);
            ShowWnd(wnd, bTop, paraList);
        }

        /// <summary>
        /// 根据窗口对象显示窗口
        /// </summary>
        /// <param name="wnd"></param>
        /// <param name="bTop"></param>
        /// <param name="paraList"></param>
        public void ShowWnd(UiDesignerBase wnd, bool bTop = true, params object[] paraList)
        {
            if (wnd != null)
            {
                if (wnd.GameObject != null && !wnd.GameObject.activeSelf)
                {
                    wnd.GameObject.SetActive(true);
                }

                if (bTop) wnd.Transform.SetAsLastSibling();

                //if (!ObjectManager.Instance.IsObjectManagerCreate(wnd.GameObject))
                //{
                //    Debug.Log("不是从对象池创建的 ？？？ ");
                //}
                wnd.Init();
            }
        }
    }
}