using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace ShowNow
{
    public class LockedQueue<T>
    {
        private Queue<T> queue = new Queue<T>();
        private static readonly object queueLock = new object();

        public int Count()
        {
            return queue.Count;
        }
        public void Enqueue(T bean)
        {
            lock (queueLock)
            {
                queue.Enqueue(bean);
            }
        }
        public T Dequeue()
        {
            lock (queueLock)
            {
                return queue.Dequeue();
            }
        }
        public T Peek()
        {
            lock (queueLock)
            {
                if (queue.Count > 0)
                    return queue.Peek();
                else
                    return default(T);
            }
        }
        public void Clear()
        {
            lock (queueLock)
            {
                queue.Clear();
            }
        }
    }
}
public class Common 
{
  
}
