using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KLB_Monitor.HIS.Model
{
    public class MZ1001Request : BaseRequest<MZ1001Response>
    {
        public MZ1001 inpara { get; set; }

        public override string GetMethod()
        {
            return "MZ1001";
        }

        public override string GetJson()
        {
            inpara.TRANS_NO = GetMethod();
            return JsonConvert.SerializeObject(inpara);
        }
    }
}
