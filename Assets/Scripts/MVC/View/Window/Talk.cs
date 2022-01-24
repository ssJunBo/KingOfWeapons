using bFrame;
using bFrame.Game.Base;
using bFrame.Game.UIFrame;
using bFrame.Game.UIFrame.Base;
using MVC.Controller;
using UnityEngine;
using UnityEngine.UI;

namespace MVC.View.Window
{
    public class Talk : UiDesignerBase
    {
        public UICircularScrollView talkScrollView;
        public Button backBtn;

        private  int num = 0;

        public override void Init()
        {
            talkScrollView.ShowList(5);
            AddButtonClickListener(backBtn, OnClickBackBtn);
        }

        private void Update()
        {
            if (Input.GetKeyDown(KeyCode.Space))
            {
                num++;
                talkScrollView.ShowList(num);
                talkScrollView.GoBottom();
            }
        }

        private void NormalCallBack(GameObject cell, int index)
        {
            TalkItem talkItem = cell.GetComponent<TalkItem>();
            if (index % 2 != 0)
            {
                Tools.SetActive(talkItem.headLeft.gameObject, true);
                Tools.SetActive(talkItem.textLeft.gameObject, true);
                talkItem.textLeft.text = index.ToString() + "世界终究在我脚下";
            }
            else
            {
                Tools.SetActive(talkItem.headRight.gameObject, true);
                Tools.SetActive(talkItem.textRight.gameObject, true);
                talkItem.textRight.text = index.ToString() + "世界终究在我脚下";
            }
        }

        private void OnClickBackBtn()
        {
            GameManager.Instance.UiManager.HideWnd(ConStr._TalkPanel);
            GameManager.Instance.UiManager.PopUpWnd(ConStr._MenuPanel);
        }
    }
}