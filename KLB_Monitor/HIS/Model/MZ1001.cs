using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KLB_Monitor.HIS.Model
{
    public class MZ1001
    {
        /// <summary>
        /// 交易代码
        /// </summary>
        public string TRANS_NO { get; set; }
        /// <summary>
        /// 终端号
        /// </summary>
        public string MAC_ADD { get; set; }

        
        /// <summary>
        /// 就诊卡号
        /// </summary>
        public string CARD_DATA { get; set; }
        /// <summary>
        /// 就诊卡类型
        /// </summary>
        public string CARD_TYPE { get; set; }

        /// <summary>
        /// 医保卡行政区划，医保卡的行政区划代码
        /// </summary>
        public string AREA_CODE { get; set; } = "";

        /// <summary>
        /// 卡内数据
        /// </summary>
        public string CARD_INSIDE_DATA { get; set; }

        /// <summary>
        /// 是否获取签约信息
        /// </summary>
        public string GET_SIGN_INFO { get; set; }

        /// <summary>
        /// 是否获取平台信息
        /// </summary>
        public int? GET_PLAT_INFO { get; set; }
    }
}
