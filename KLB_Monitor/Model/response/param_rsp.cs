using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KLB_Monitor.Model.response
{
    /// <summary>
    /// 参数返回体
    /// </summary>
    public class param_rsp
    {
        /// <summary>
        /// 设备名称
        /// </summary>
        public string device_name { get; set; }

        /// <summary>
        /// 版本号
        /// </summary>
        public string version { get; set; }

        /// <summary>
        /// 自动关机时间
        /// </summary>
        public string auto_shutdown_time { get; set; }

        /// <summary>
        /// 壳体程序完整路径
        /// </summary>
        public string cef_exe_full_path { get; set; }

        /// <summary>
        /// 云his程序完整路径
        /// </summary>
        public string his_exe_full_path { get; set; }

        /// <summary>
        /// 更新包下载地址
        /// </summary>
        public string download_url { get; set; }

        /// <summary>
        /// 打印机名称；如果有多个，则以逗号隔开
        /// </summary>
        public string printer_name { get; set; }

        /// <summary>
        /// 中间件访问地址
        /// </summary>
        public string middle_url { get; set; }

        /// <summary>
        /// his访问地址
        /// </summary>
        public string his_url { get; set; }

        /// <summary>
        /// his终端号
        /// </summary>
        public string his_macAdd { get; set; }

        /// <summary>
        /// 测试用身份证号
        /// </summary>
        public string his_test_idcard { get; set; }
    }

    /// <summary>
    /// 指令返回体
    /// </summary>
    public class command_rsp
    {
        /// <summary>
        /// 指令
        /// </summary>
        public string code { get; set; }

        /// <summary>
        /// 更新包路径
        /// </summary>
        public string filePath { get; set; }

        /// <summary>
        /// 版本号
        /// </summary>
        public string fileName { get; set; }
    }

    /// <summary>
    /// 服务端时间返回体
    /// </summary>
    public class time_rsp
    {
        public string time { get; set; }
    }

    /// <summary>
    /// 中间件返回类型
    /// </summary>
    public class middle_rsp
    {
        /// <summary>
        /// 时间
        /// </summary>
        public DateTime time { get; set; }
    }
}
