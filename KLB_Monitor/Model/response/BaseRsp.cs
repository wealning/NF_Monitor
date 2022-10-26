using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KLB_Monitor.Model.response
{
    public class BaseRsp<T>
    {
        /// <summary>
        /// 状态
        /// </summary>
        public int? code { get; set; }

        /// <summary>
        /// 描述
        /// </summary>
        public string msg { get; set; }

        /// <summary>
        /// 实际数据
        /// </summary>
        public T data { get; set; }
    }

    public class BaseMiddleRsp<T>
    {
        /// <summary>
        /// 
        /// </summary>
        public bool? success { get; set; }

        /// <summary>
        /// 
        /// </summary>
        public string msg { get; set; }

        /// <summary>
        /// 实际数据
        /// </summary>
        public T data { get; set; }
    }
}
