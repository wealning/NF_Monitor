using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KLB_Monitor.HIS.Model
{
    public abstract class BaseResponse
    {
        public string ret_code { get; set; } = "";
        public string ret_info { get; set; } = "";
    }
}
