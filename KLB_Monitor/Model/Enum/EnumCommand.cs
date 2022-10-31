using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KLB_Monitor.Model.Enum
{
    /// <summary>
    /// 指令
    /// </summary>
    public enum EnumCommand
    {
        /// <summary>
        /// 关机
        /// </summary>
        TurnOff = 1001,

        /// <summary>
        /// 重启
        /// </summary>
        Reboot = 1002,

        /// <summary>
        /// 注销
        /// </summary>
        WriteOff = 1003,
            
        /// <summary>
        /// 更新壳体版本
        /// </summary>
        UpdateVersion = 2001,

        /// <summary>
        /// 截屏
        /// </summary>
        ScreenShot = 1004,
    }

    /// <summary>
    /// 通讯状态
    /// </summary>
    public enum EnumCommunicationStatus
    {
        /// <summary>
        /// 
        /// </summary>
        Success = 0,
    }

    /// <summary>
    /// 错误等级
    /// </summary>
    public enum EnumErrorLevel
    {
        /// <summary>
        /// 汇总的
        /// </summary>
        Summary = 0,

        /// <summary>
        /// 系统级的
        /// </summary>
        System = 1,
        
        /// <summary>
        /// 第三方的
        /// </summary>
        Third = 2,

        /// <summary>
        /// 壳体
        /// </summary>
        Shell = 3,
    }
}
