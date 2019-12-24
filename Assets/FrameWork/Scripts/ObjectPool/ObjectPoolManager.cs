using System.Collections;
using System.Collections.Generic;
using UnityEngine;


namespace MyFramework
{
    public class ObjectPoolManager : ManagerBase
    {
        /// <summary>
        /// 默认对象池容量
        /// </summary>
        private const int DefaultCapacity = int.MaxValue;

        /// <summary>
        /// 默认对象过期秒数
        /// </summary>
        private const float DefaultExpireTime = float.MaxValue;

        /// <summary>
        /// 对象池字典
        /// </summary>
        private Dictionary<string, IObjectPool> m_ObjectPools;

        public override int Priority
        {
            get
            {
                return 90;
            }
        }

        /// <summary>
        /// 对象池数量
        /// </summary>
        public int Count
        {
            get
            {
                return m_ObjectPools.Count;
            }
        }

        public ObjectPoolManager()
        {
            m_ObjectPools = new Dictionary<string, IObjectPool>();
        }
        public override void Init()
        {

        }

        public override void Shutdown()
        {
            foreach (IObjectPool objectPool in m_ObjectPools.Values)
            {
                objectPool.Shutdown();
            }
            m_ObjectPools.Clear();
        }

        public override void Update(float elapseSeconds, float realElapseSeconds)
        {
            foreach (IObjectPool objectPool in m_ObjectPools.Values)
            {
                objectPool.Update(elapseSeconds, realElapseSeconds);
            }
        }
        /// <summary>
        /// 检查对象池
        /// </summary>
        public bool HasObjectPool<T>() where T : ObjectBase
        {
            return m_ObjectPools.ContainsKey(typeof(T).FullName);
        }
        /// <summary>
        /// 创建对象池
        /// </summary>
        public ObjectPool<T> CreateObjectPool<T>(int capacity = DefaultCapacity, float exprireTime = DefaultExpireTime, bool allowMultiSpawn = false) where T : ObjectBase
        {
            string name = typeof(T).FullName;
            if (HasObjectPool<T>())
            {
                Debug.LogError("要创建的对象池已存在");
                return null;
            }
            ObjectPool<T> objectPool = new ObjectPool<T>(name, capacity, exprireTime, allowMultiSpawn);
            m_ObjectPools.Add(name, objectPool);
            return objectPool;
        }

        /// <summary>
        /// 获取对象池
        /// </summary>
        public ObjectPool<T> GetObjectPool<T>() where T : ObjectBase
        {
            IObjectPool objectPool = null;
            m_ObjectPools.TryGetValue(typeof(T).FullName, out objectPool);
            return objectPool as ObjectPool<T>;
        }

        /// <summary>
        /// 销毁对象池
        /// </summary>
        public bool DestroyObjectPool<T>()
        {
            IObjectPool objectPool = null;
            if (m_ObjectPools.TryGetValue(typeof(T).FullName, out objectPool))
            {
                objectPool.Shutdown();
                return m_ObjectPools.Remove(typeof(T).FullName);
            }

            return false;
        }
        /// <summary>
        /// 释放所有对象池中的可释放对象。
        /// </summary>
        public void Release()
        {
            foreach (IObjectPool objectPool in m_ObjectPools.Values)
            {
                objectPool.Release();
            }
        }

        /// <summary>
        /// 释放所有对象池中的未使用对象。
        /// </summary>
        public void ReleaseAllUnused()
        {
            foreach (IObjectPool objectPool in m_ObjectPools.Values)
            {
                objectPool.ReleaseAllUnused();
            }
        }
    }

}
