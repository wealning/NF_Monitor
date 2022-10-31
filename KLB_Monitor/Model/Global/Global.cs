using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KLB_Monitor.Model.Global
{
    /// <summary>
    /// 尽量不动用
    /// </summary>
    public class Global
    {
        /// <summary>
        /// 
        /// </summary>
        public string server_url = "";

        /// <summary>
        /// 设备id
        /// </summary>
        public string device_id { get; set; }

        /// <summary>
        /// 是否已连接
        /// </summary>
        public static bool IsConnect { get; set; }

        /// <summary>
        /// 参数
        /// </summary>
        public static Param param { get; set; }
    }

    /// <summary>
    /// 参数 - 全局化
    /// </summary>
    public class Param
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
        /// 壳体程序名称
        /// </summary>
        public string his_exe_full_path { get; set; }

        /// <summary>
        /// 更新包下载地址
        /// </summary>
        public string download_url { get; set; }

        /// <summary>
        /// 更新包下载地址
        /// </summary>
        public string update_filePath { get; set; }

        /// <summary>
        /// 更新包真实版本
        /// </summary>
        public string update_fileVersion { get; set; }

        /// <summary>
        /// 打印机名称列表
        /// </summary>
        public List<string> printeNameList { get; set; }

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
}
