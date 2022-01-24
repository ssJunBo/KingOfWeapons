using System.Collections.Generic;
using bFrame.Game.ResourceFrame;
using UnityEngine;
using UnityEngine.UI;

namespace bFrame.Game.UIFrame.Base
{
    public enum EuiMsgId
    {
        None = 0,
    }

    public class UiDesignerBase : MonoBehaviour
    {
        //引用GameObject
        public GameObject GameObject { get; set; }

        protected readonly UiLogicBase UiLogic;
        //引用Transform
        public Transform Transform { get; set; }

        //prefab的名字
        public string Name { get; set; }

        //所有的Button 
        private readonly List<Button> _mAllButton = new List<Button>();

        //所有的Toggle
        private readonly List<Toggle> _mAllToggle = new List<Toggle>();

//        public UiDesignerBase(UiLogicBase uiLogic)
        {
            UiLogic = uiLogic;
        }

        public virtual bool OnMessage(EuiMsgId msgId, params object[] paraList)
        {
            return true;
        }

        public virtual void Init()
        {
        }

        public virtual void ShowFinished()
        {
            
        }

        public virtual void Release()
        {
            RemoveAllButtonListener();
            RemoveAllToggleListener();
            _mAllButton.Clear();
            _mAllToggle.Clear();
        }

        /// <summary>
        /// 同步替换图片
        /// </summary>
        /// <param name="path"></param>
        /// <param name="image"></param>
        /// <param name="setNativeSize"></param>
        /// <returns></returns>
        public bool ChangeImageSprite(string path, Image image, bool setNativeSize = false)
        {
            if (image == null) return false;
            Sprite sp =ResourcesManager.Instance.LoadResource<Sprite>(path);
            if (sp != null)
            {
                if (image.sprite != null)
                    image.sprite = null;

                image.sprite = sp;
                if (setNativeSize)
                {
                    image.SetNativeSize();
                }

                return true;
            }

            return false;
        }


        /// <summary>
        /// 图片加载完成
        /// </summary>
        /// <param name="path"></param>
        /// <param name="obj"></param>
        /// <param name="image"></param>
        /// <param name="setNativeSize"></param>
        protected void OnLoadSpriteFinish(string path, Object obj, Image image, bool setNativeSize)
        {
            if (obj == null) return;

            Sprite sp = obj as Sprite;

            if (image == null) return;

            if (image.sprite != null)
                image.sprite = null;

            image.sprite = sp;

            if (setNativeSize)
            {
                image.SetNativeSize();
            }
        }

        /// <summary>
        /// 移除所有的Button事件
        /// </summary>
        private void RemoveAllButtonListener()
        {
            foreach (Button btn in _mAllButton)
            {
                btn.onClick.RemoveAllListeners();
            }
        }

        /// <summary>
        /// 移除所有的toggle事件
        /// </summary>
        private void RemoveAllToggleListener()
        {
            foreach (Toggle toggle in _mAllToggle)
            {
                toggle.onValueChanged.RemoveAllListeners();
            }
        }

        /// <summary>
        /// 添加Button事件监听 
        /// </summary>
        /// <param name="btn"></param>
        /// <param name="action"></param>
        protected void AddButtonClickListener(Button btn, UnityEngine.Events.UnityAction action)
        {
            if (btn == null) return;
            
            if (!_mAllButton.Contains(btn))
            {
                _mAllButton.Add(btn);
            }

            btn.onClick.RemoveAllListeners();
            btn.onClick.AddListener(action);
            btn.onClick.AddListener(() =>
            {
                //TODO 放在声音系统中去处理
            });
        }

        /// <summary>
        /// Toggle事件监听
        /// </summary>
        /// <param name="toggle"></param>
        /// <param name="action"></param>
        public void AddToggleClickListener(Toggle toggle, UnityEngine.Events.UnityAction<bool> action)
        {
            if (toggle != null)
            {
                if (!_mAllToggle.Contains(toggle))
                {
                    _mAllToggle.Add(toggle);
                }

                toggle.onValueChanged.RemoveAllListeners();
                toggle.onValueChanged.AddListener(action);
                toggle.onValueChanged.AddListener((call) =>
                {
                    //TODO 放在声音系统中去处理
                });
            }
        }
    }
}