using Quartz;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KLB_Monitor.Job
{
    public class EpsonMonitorJob : IJob
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="context"></param>
        /// <returns></returns>
        public Task Execute(IJobExecutionContext context)
        {
            //注意try...catch
            return Task.Factory.StartNew(new Action(() => Console.WriteLine($"当前时间： {DateTime.Now.ToString()}")));
        }
    }
}
