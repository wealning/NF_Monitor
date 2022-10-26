using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KLB_Monitor.HIS.Model
{
    public interface IRequest<out T> where T : BaseResponse
    {
        string GetMethod();
    }
}
