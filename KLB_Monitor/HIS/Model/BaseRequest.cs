using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KLB_Monitor.HIS.Model
{
    public abstract class BaseRequest<T> : IRequest<T> where T : BaseResponse
    {
        public abstract string GetMethod();
        public abstract string GetJson();
    }
}
