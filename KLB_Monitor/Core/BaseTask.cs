using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KLB_Monitor.Core
{
    public class BaseTask
    {
        public CancellationTokenSource tokenSource;
        public CancellationToken token;
        public Task task;
        public Action work;

        public BaseTask(Action Work)
        {
            this.work = Work;
        }

        public void NewTask()
        {
            tokenSource = new CancellationTokenSource();
            token = tokenSource.Token;
            task = new Task(() => 
            {
                while (!token.IsCancellationRequested)
                {
                    work();
                }
            }, token);
        }

        public void Start()
        {
            bool flag = false;
            while (!flag)
            {
                if (task == null)
                {
                    NewTask();
                    flag = false;
                }

                if (task?.Status == TaskStatus.Created)
                {
                    task.Start();
                    flag = true;
                }

                if (task?.Status == TaskStatus.RanToCompletion 
                    || task?.Status == TaskStatus.Faulted 
                    || task?.Status == TaskStatus.Canceled)
                {
                    NewTask();
                    task.Start();
                    flag = true;
                }
                if (task?.Status == TaskStatus.Running)
                {
                    flag = true;
                }

            }
        }

        public void Stop()
        {
            try
            {
                tokenSource.Cancel();
            }
            catch (Exception)
            {
            }
        }
    }
}
