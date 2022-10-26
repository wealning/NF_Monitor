using System.Runtime.InteropServices;
using System.Drawing.Printing;
using log4net;
using System;
using KLB_Monitor;
using System.Reflection.Metadata;
using Microsoft.VisualBasic.Logging;
using System.Collections.Generic;
using Newtonsoft.Json.Serialization;
using Newtonsoft.Json;

namespace KLB_Monitor.Core
{
    /// <summary>
    /// 普通打印机监控
    /// </summary>
    public static class PrinterHelper
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="pPrinterName"></param>
        /// <param name="hPrinter"></param>
        /// <param name="pDefault"></param>
        /// <returns></returns>
        [DllImport("winspool.drv", CharSet = CharSet.Auto, SetLastError = true)]
        private static extern bool OpenPrinter(string pPrinterName, out IntPtr hPrinter, IntPtr pDefault);

        /// <summary>
        /// 关闭指定的打印机对象
        /// </summary>
        /// <param name="hPrinter"></param>
        /// <returns></returns>
        [DllImport("winspool.drv", SetLastError = true)]
        private static extern bool ClosePrinter(IntPtr hPrinter);

        /// <summary>
        /// 获取指定的打印机数据
        /// </summary>
        /// <param name="hPrinter"></param>
        /// <param name="dwLevel"></param>
        /// <param name="pPrinter"></param>
        /// <param name="cbBuf"></param>
        /// <param name="pcbNeeded"></param>
        /// <returns></returns>
        [DllImport("winspool.drv", SetLastError = true)]
        private static extern bool GetPrinter(IntPtr hPrinter, int dwLevel, IntPtr pPrinter, int cbBuf, out int pcbNeeded);

        /// <summary>
        /// 打印机的详细信息
        /// </summary>
        [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi)]
        public struct PRINTER_INFO_2
        {
            public string pServerName;
            public string pPrinterName;
            public string pShareName;
            public string pPortName;
            public string pDriverName;
            public string pComment;
            public string pLocation;
            public IntPtr pDevMode;
            public string pSepFile;
            public string pPrintProcessor;
            public string pDatatype;
            public string pParameters;
            public IntPtr pSecurityDescriptor;
            public uint Attributes;
            public uint Priority;
            public uint DefaultPriority;
            public uint StartTime;
            public uint UntilTime;
            public uint Status;
            public uint cJobs;
            public uint AveragePPM;
        }

        /// <summary>
        /// 获取打印机的状态并转换为描述
        /// </summary>
        /// <param name="PrinterName">打印机名称</param>
        /// <returns></returns>
        public static string GetPrinterStatus(string PrinterName)
        {
            int intValue = GetPrinterStatusInt(PrinterName);
            string strRet = string.Empty;
            switch (intValue)
            {
                case 0:
                    //strRet = "准备就绪（Ready）";
                    strRet = "";
                    break;
                case 0x00000200:
                    strRet = "忙(Busy）";
                    break;
                case 0x00400000:
                    strRet = "被打开（Printer Door Open）";
                    break;
                case 0x00000002:
                    strRet = "错误(Printer Error）";
                    break;
                case 0x0008000:
                    strRet = "初始化(Initializing）";
                    break;
                case 0x00000100:
                    strRet = "正在输入,输出（I/O Active）";
                    break;
                case 0x00000020:
                    strRet = "手工送纸（Manual Feed）";
                    break;
                case 0x00040000:
                    strRet = "无墨粉（No Toner）";
                    break;
                case 0x00001000:
                    strRet = "不可用（Not Available）";
                    break;
                case 0x00000080:
                    strRet = "脱机（Off Line）";
                    break;
                case 0x00200000:
                    strRet = "内存溢出（Out of Memory）";
                    break;
                case 0x00000800:
                    strRet = "输出口已满（Output Bin Full）";
                    break;
                case 0x00080000:
                    strRet = "当前页无法打印（Page Punt）";
                    break;
                case 0x00000008:
                    strRet = "塞纸（Paper Jam）";
                    break;
                case 0x00000010:
                    strRet = "打印纸用完（Paper Out）";
                    break;
                case 0x00000040:
                    strRet = "纸张问题（Page Problem）";
                    break;
                case 0x00000001:
                    strRet = "暂停（Paused）";
                    break;
                case 0x00000004:
                    strRet = "正在删除（Pending Deletion）";
                    break;
                case 0x00000400:
                    strRet = "正在打印（Printing）";
                    break;
                case 0x00004000:
                    strRet = "正在处理（Processing）";
                    break;
                case 0x00020000:
                    strRet = "墨粉不足（Toner Low）";
                    break;
                case 0x00100000:
                    strRet = "需要用户干预（User Intervention）";
                    break;
                case 0x20000000:
                    strRet = "等待（Waiting）";
                    break;
                case 0x00010000:
                    strRet = "热机中（Warming Up）";
                    break;
                default:
                    //strRet = "未知状态（Unknown Status）";
                    strRet = "";
                    break;
            }
            return strRet;
        }

        /// <summary>
        /// 获取打印机的状态
        /// </summary>
        /// <param name="PrinterName"></param>
        /// <returns></returns>
        private static int GetPrinterStatusInt(string PrinterName)
        {
            int intRet = 0;
            IntPtr hPrinter;


            if (OpenPrinter(PrinterName, out hPrinter, IntPtr.Zero))
            {
                int cbNeeded = 0;
                bool bolRet = GetPrinter(hPrinter, 2, IntPtr.Zero, 0, out cbNeeded);
                if (cbNeeded > 0)
                {
                    IntPtr pAddr = Marshal.AllocHGlobal((int)cbNeeded);
                    bolRet = GetPrinter(hPrinter, 2, pAddr, cbNeeded, out cbNeeded);
                    if (bolRet)
                    {
                        PRINTER_INFO_2 Info2 = new PRINTER_INFO_2();


                        Info2 = (PRINTER_INFO_2)Marshal.PtrToStructure(pAddr, typeof(PRINTER_INFO_2));


                        intRet = System.Convert.ToInt32(Info2.Status);
                    }
                    Marshal.FreeHGlobal(pAddr);
                }
                ClosePrinter(hPrinter);
            }


            return intRet;
        }

    }

    /// <summary>
    /// EPSON打印机监控
    /// </summary>
    public class EpsonPrinterHelper
    {
        private static ILog _Logger = LogManager.GetLogger("EPSON");

        /// <summary>
        /// 
        /// </summary>
        /// <param name="printer_name"></param>
        public static string GetInfo(string printer_name)
        {
            string err_msg = string.Empty;
            #region 初始化
            int initRet = EpsonAPI.ENSInitialize();
            if (initRet != 0)
            {
                _Logger.Error($"EPSON初始化失败：{initRet}");
                return "初始化失败";
            }
            _Logger.Debug($"EPSON初始化完成");
            #endregion

            #region 
            
            IntPtr handle = IntPtr.Zero;    //通讯的句柄
            int BuffLen = Marshal.SizeOf(typeof(DEVICEID01));
            IntPtr IdBuff = Marshal.AllocHGlobal(BuffLen);

            try
            {
                try
                {
                    //获取设备id
                    int deviceRet = EpsonAPI.ENSGetDeviceID(PATHTYPE.PRINTER, printer_name, IdBuff, ref BuffLen, 1);
                    if (deviceRet != 0)
                    {
                        _Logger.Error($"EPSON获取打印机的设备id：{deviceRet}");
                        return "获取设备信息失败";
                    }
                    _Logger.Debug($"EPSON获取打印机的设备id");

                    //打开通讯
                    int openRet = EpsonAPI.ENSOpenCommunication(PATHTYPE.PRINTER, printer_name, IdBuff, out handle);
                    if (openRet != 0)
                    {
                        _Logger.Error($"EPSON打开通讯连接失败：{openRet}");
                        return "打开通讯失败";
                    }
                    _Logger.Debug("EPSON打开通讯连接完成");
                }
                finally
                {
                    //释放为非托管ANSI字符串分配的内存和非托管内存块
                    Marshal.FreeHGlobal(IdBuff);
                }

                BuffLen = 0;
                IntPtr pGetBuff = IntPtr.Zero;
                //获取信息
                int getRet = EpsonAPI.ENSGetInformation(handle, "STATUS_IJP", IntPtr.Zero, IntPtr.Zero, ref BuffLen);
                if (getRet != 0)
                {
                    if (getRet == ERR.BUFFERSIZE)
                    {
                        pGetBuff = Marshal.AllocHGlobal(BuffLen);
                        getRet = EpsonAPI.ENSGetInformation(handle, "STATUS_IJP", IntPtr.Zero, pGetBuff, ref BuffLen);
                    }

                    if (getRet != 0)
                    {
                        _Logger.Error($"EPSON获取信息失败：{getRet}");
                        return $"获取打印机状态失败:{getRet}";
                    }
                }
                _Logger.Debug($"EPSON获取信息完成");

                if(pGetBuff != IntPtr.Zero)
                {
                    STATUSHEADER pStatusHeader = (STATUSHEADER)Marshal.PtrToStructure(pGetBuff, typeof(STATUSHEADER));
                    _Logger.Debug($"pStatusHeader：{JsonConvert.SerializeObject(pStatusHeader)}");
                    if (pStatusHeader.Version.MajorVersion == 0X0300 && pStatusHeader.Version.MinerVersion == 0x0010)
                    {
                        STATUS_IJP pStatusIJP = (STATUS_IJP)Marshal.PtrToStructure(pGetBuff, typeof(STATUS_IJP));
                        _Logger.Debug($"pStatusIJP：{JsonConvert.SerializeObject(pStatusIJP)}");

                        if (pStatusIJP.ErrorCode >= 0x00 && pStatusIJP.ErrorCode <= 0x83)
                        {
                            switch (pStatusIJP.ErrorCode)
                            {
                                case 0x00:
                                    err_msg = "致命性错误";
                                    break;
                                case 0x01:
                                    _Logger.Debug("pStatusIJP 处理其他接口请求");
                                    break;
                                case 0x04:
                                    err_msg = "卡纸了";
                                    break;
                                case 0x06:
                                case 0x0A:
                                case 0X0C:
                                case 0x0E:
                                case 0x17:
                                case 0x18:
                                case 0x66:
                                    err_msg = "缺纸或装纸不正确";
                                    break;
                                case 0x05:
                                case 0x3D:
                                    err_msg = "油墨不足";
                                    break;
                                default:
                                    err_msg = $"{pStatusIJP.StatusCode} - {pStatusIJP.ErrorCode}";
                                    break;
                            }
                        }
                        
                    }
                }
            }
            catch (Exception ex)
            {
                _Logger.Error($"EPSON获取信息异常：{ex.Message}\r\n{ex.StackTrace}");
            }
            finally
            {
                //关闭通讯
                int closeRet = EpsonAPI.ENSCloseCommunication(handle);
                if (closeRet != 0)
                {
                    _Logger.Error($"EPSON关闭通讯连接失败：{closeRet}");
                }
                _Logger.Debug($"EPSON关闭通讯连接完成");
                //释放资源
                int releaseRet = EpsonAPI.ENSRelease();
                if (releaseRet != 0)
                {
                    _Logger.Error($"EPSON释放失败：{releaseRet}");
                }
                _Logger.Debug("EPSON资源释放完成");
            }

            #endregion
            return err_msg;
        }

        /// <summary>
        /// 
        /// </summary>
        private static void ii()
        {

        }
    }
}
