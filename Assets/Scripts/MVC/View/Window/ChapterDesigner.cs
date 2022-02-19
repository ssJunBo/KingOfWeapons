using System.Collections.Generic;
using bFrame.Game.ResourceFrame;
using bFrame.Game.UIFrame.Base;
using MVC.Controller;
using UnityEngine;
using UnityEngine.UI;

namespace MVC.View.Window
{
    public class ChapterDesigner : UiDesignerBase
    {
        [SerializeField,Header("返回按钮")] private Button backBtn;
        [SerializeField] private Text titleTe;
        [SerializeField] private Transform contentTrs;

        private readonly List<GameObject> _itemLis = new List<GameObject>();

        public override void Init()
        {
            AddButtonClickListener(backBtn, OnClickBack);
            _itemLis.Clear();
            var count = 10;
            for (var i = 0; i < count; i++)
            {
//                _itemLis.Add(ObjectManager.Instance.SpwanObjFromPool(ConStr.bookItem_Path, targetTransform: contentTrs));
            }
        }

        private void OnClickBack()
        {
            foreach (var t in _itemLis)
            {
               
            }
        }
    }
}