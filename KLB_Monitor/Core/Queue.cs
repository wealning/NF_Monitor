using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KLB_Monitor.Core
{
    public class Queue
    {
        public List<int> queueList;

        public Queue()
        {
            queueList = new List<int>();
        }

        /// <summary>
        /// 入队列
        /// </summary>
        /// <param name="element"></param>
        public void Enqueue(int element)
        {
            if (queueList == null)
            {
                queueList = new List<int>();
            }

            queueList.Add(element);
        }

        /// <summary>
        /// 出队列
        /// </summary>
        public int? Dequeue()
        {
            if (queueList == null || !queueList.Any())
            {
                return null;
            }

            int element = queueList.FirstOrDefault();
            queueList.Remove(element);
            return element;
        }

        /// <summary>
        /// 清除队列
        /// </summary>
        public void ClearQueue()
        {
            if (queueList != null && queueList.Any())
            {
                queueList.Clear();
            }
        }

        /// <summary>
        /// 获取当前队列中的数据
        /// </summary>
        /// <returns></returns>
        public int GetCount()
        {
            if (queueList == null || !queueList.Any())
            {
                return 0;
            }
            return queueList.Count;
        }
    }
}
