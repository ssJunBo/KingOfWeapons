using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.NetworkInformation;
using bFrame.Game.Base;

namespace bFrame.Game.Tools
{
    public class TimeCallback : MonoSingleton<TimeCallback>
    {
        /// <summary>
        /// 计时器列表 存储所有开启的计时器 计时完成后 从列表中移除
        /// </summary>
        private readonly List<CallBackInfo> _mLtCallbackInfo = new List<CallBackInfo>();

        /// <summary>
        /// 计时完成列表 短暂存完成计时的 info 
        /// </summary>
        private readonly List<CallBackInfo> _mLtFinish = new List<CallBackInfo>();

        /// <summary>
        /// 延时几帧后执行callback
        /// </summary>
        /// <param name="value"></param>
        /// <param name="callback"></param>
        public void DelayHowManySecondsAfterCallBack(int duration, Action callback)
        {
            CallBackInfo info=new FrameCallBackInfo();
            info.Duration = duration;
            info._eventFinishAct = callback;
            _mLtCallbackInfo.Add(info);
        }
        
        public void DelayHowManyFramesAfterCallBack(int howMnayFrames, Action callback)
        {
            FrameCallBackInfo info = new FrameCallBackInfo {HowManyFrames = howMnayFrames, _eventFinishAct = callback};
            _mLtCallbackInfo.Add(info);
        }

        private void Update()
        {
            if (_mLtCallbackInfo != null && _mLtCallbackInfo.Count > 0)
            {
                foreach (var aInfo in _mLtCallbackInfo.Where(aInfo => aInfo != null))
                {
                    if (aInfo.TickCheckFinish())
                    {
                        _mLtFinish.Add(aInfo);
                    }
                }

                foreach (var aInfo in _mLtFinish)
                {
                    aInfo?.Finish();

                    _mLtCallbackInfo.Remove(aInfo);
                }

                _mLtFinish.Clear();
            }
        }
    }

    /// <summary>
    /// 常规倒计时基类
    /// </summary>
    public abstract class CallBackInfo
    {
        private float _mDuration;

        public float Duration
        {
            set
            {
                _mDuration = value;
                EndTime = (int) (_mDuration * 1000) + CTools.TickCount();
            }
        }

        public Action _eventFinishAct;
        
        private int EndTime { get; set; }

        protected virtual void OnTick()
        {
        }

        /// <summary>
        /// 每帧检测 是否完成
        /// </summary>
        public virtual bool TickCheckFinish()
        {
            return CTools.TickCount() > EndTime;
        }

        public void Finish()
        {
            _eventFinishAct?.Invoke();
        }
    }

    /// <summary>
    /// 延迟帧 信息类
    /// </summary>
    public class FrameCallBackInfo : CallBackInfo
    {
        public int HowManyFrames;

        #region override函数
        protected override void OnTick()
        {
            HowManyFrames--;
        }

        public override bool TickCheckFinish()
        {
            OnTick();
            return HowManyFrames > 0;
        }
        #endregion
    }
}
