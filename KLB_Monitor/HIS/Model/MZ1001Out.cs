using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KLB_Monitor.HIS.Model
{
    public class MZ1001Out
    {
        /// <summary>
        /// 患者编号
        /// </summary>
        public decimal pat_id { get; set; }
        /// <summary>
        /// 病人姓名
        /// </summary>
        public string pat_name { get; set; }
        /// <summary>
        /// 性别 1=男/2=女/3=不详/9=其它
        /// </summary>
        public decimal sex { get; set; }
        /// <summary>
        /// 年龄
        /// </summary>
        public string age { get; set; }
        /// <summary>
        /// 出生日期  yyyy-mm-dd
        /// </summary>
        public string birth { get; set; }
        /// <summary>
        /// 联系电话
        /// </summary>
        public string tel { get; set; }
        /// <summary>
        /// 身份证号
        /// </summary>
        public string id_card { get; set; }
        /// <summary>
        /// 门诊号
        /// </summary>
        public string opc_id { get; set; }
        /// <summary>
        /// 病人性质
        /// </summary>
        public decimal fee_nature { get; set; } = 0;
        /// <summary>
        /// 病人性质名称
        /// </summary>
        public string fee_nature_name { get; set; } = "未知";
    }
}
