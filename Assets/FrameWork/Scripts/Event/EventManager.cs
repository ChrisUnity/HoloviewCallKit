using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MyFramework {
    /// <summary>
    /// 事件管理器
    /// </summary>
    public class EventManager : ManagerBase
    {
        /// <summary>
        /// 事件池
        /// </summary>
        private EventPool<GlobalEventArgs> m_EventPool;

        public override int Priority
        {
            get
            {
                return 100;
            }
        }

        public EventManager()
        {
            m_EventPool = new EventPool<GlobalEventArgs>();
        }

        public override void Init()
        {

        }

        public override void Shutdown()
        {
            //关闭并清理事件池
            m_EventPool.Shutdown();
        }

        /// <summary>
        /// 轮询事件池
        /// </summary>
        public override void Update(float elapseSeconds, float realElapseSeconds)
        {
            m_EventPool.Update(elapseSeconds, realElapseSeconds);
        }
        /// <summary>
        /// 检查订阅事件处理方法是否存在
        /// </summary>
        public bool Check(int id, EventHandler<GlobalEventArgs> handler)
        {
            return m_EventPool.Check(id, handler);
        }

        /// <summary>
        /// 订阅事件
        /// </summary>
        public void Subscribe(int id, EventHandler<GlobalEventArgs> handler)
        {
            m_EventPool.Subscribe(id, handler);
        }

        /// <summary>
        /// 取消订阅事件
        /// </summary>
        public void Unsubscribe(int id, EventHandler<GlobalEventArgs> handler)
        {
            m_EventPool.Unsubscribe(id, handler);
        }
        /// <summary>
        /// 抛出事件（线程安全）
        /// </summary>
        public void Fire(object sender, GlobalEventArgs e)
        {
            m_EventPool.Fire(sender, e);
        }

        /// <summary>
        /// 抛出事件（线程不安全）
        /// </summary>
        public void FireNow(object sender, GlobalEventArgs e)
        {
            m_EventPool.FireNow(sender, e);
        }
    }
}

