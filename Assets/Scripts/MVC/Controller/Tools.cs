using System.Collections;
using DG.Tweening;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

namespace MVC.Controller
{
    public class Tools
    {
        #region 定时回调系统

        public static Coroutine TimeCallback(MonoBehaviour mono, float time, UnityAction callBack, float time2 = -1,
            UnityAction callback2 = null)
        {
            return mono.StartCoroutine(Coroutine(time, callBack, time2, callback2));
        }

        private static IEnumerator Coroutine(float time, UnityAction callback, float time2 = -1,
            UnityAction callback2 = null)
        {
            yield return new WaitForSeconds(time);
            callback?.Invoke();

            if (time2 != -1)
            {
                yield return new WaitForSeconds(time2);
                callback2?.Invoke();
            }
        }

        #endregion

        #region 隐藏与事件添加

        /// <summary>
        /// 设置物体显隐
        /// </summary>
        /// <param name="go"></param>
        /// <param name="bActive"></param>
        public static void SetActive(GameObject go, bool bActive)
        {
            if (go == null)
                return;
            if (go.activeSelf != bActive)
                go.SetActive(bActive);
        }

        /// <summary>
        /// 给GameObject添加监听
        /// </summary>
        /// <param name="go"></param>
        /// <param name="action"></param>
        public static void AddListenObj(GameObject go, UnityAction action)
        {
            Button btn = go.GetComponent<Button>();
            if (btn != null)
                btn.onClick.AddListener(action);
        }

        #endregion

        #region DoTween一些常用动画

        /// <summary>
        /// 渐隐渐现
        /// </summary>
        /// <param name="go"></param>
        /// <param name="fromVal"></param>
        /// <param name="toVal"></param>
        /// <param name="duration"></param>
        public static void PingpongTxt(GameObject go, float fromVal, float toVal, float duration)
        {
            Text te = go.GetComponent<Text>();
            if (te == null) return;
            te.DOFade(toVal, duration).onComplete = () => { PingpongTxt(go, toVal, fromVal, duration); };
        }

        #endregion
    }
}