using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms.VisualStyles;

namespace KLB_Monitor.Model.request
{
    /// <summary>
    /// 与服务端之间的连接状态
    /// </summary>
    public class connect_req
    {
        /// <summary>
        /// 设备id
        /// </summary>
        public string device_id { get; set; }

        /// <summary>
        /// 是否更新状态
        /// </summary>
        public int is_record { get; set; }
    }

    /// <summary>
    /// 反馈是否更新成功
    /// </summary>
    public class feedback_req
    {
        /// <summary>
        /// 设备id
        /// </summary>
        public string device_id { get; set; }

        /// <summary>
        /// 版本号
        /// </summary>
        public string version { get; set; }
    }

    /// <summary>
    /// 文件上传请求
    /// </summary>
    public class upload_file_req
    {
        /// <summary>
        /// 设备id
        /// </summary>
        public string device_id { get; set; }

        /// <summary>
        /// base64串
        /// </summary>
        public string base64Str { get; set; }

        /// <summary>
        /// 文件类型
        /// </summary>
        public string fileFormat { get; set; }
    }
}
