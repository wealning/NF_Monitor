using log4net;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace KLB_Monitor
{
    //=============================================================================
    //	Constant definitions
    //=============================================================================
    //-------------------------------------
    //	Error code
    //-------------------------------------
    class ERR
    {
        public const int BASE = 0;
        public const int PARAMETER = BASE - 1;
        //Values of necessary arguments have 
        //not been set.
        public const int INITIALIZE = BASE - 2;
        //Failure in initialization of 
        //bi-directional communication module. 
        //Or API has not been initialized.
        public const int NOTSUPPORT = BASE - 3;
        //Error before the start of 
        //communication (i.e. a port 
        //specification error etc.). 
        //Or other error occurred in 
        //bi-directional communication module.
        public const int PRINTER = BASE - 4;
        //Printer has not been registered
        public const int NOTFOUND = BASE - 5;
        //Communication cannot be opened. 
        //Communication trouble. Or there is 
        //no device information which can be 
        //acquired.
        public const int BUFFERSIZE = BASE - 6;
        //Specified buffer size value is too 
        //small.
        public const int TEMPORARY = BASE - 7;
        //Temporary storage memory used in API 
        //cannot be secured.
        public const int COMMUNICATION = BASE - 8;
        //Communication error occurred inside 
        //system of bi-directional 
        //communication module.
        public const int INVALIDDATA = BASE - 9;
        //Data acquired by API contains invalid
        //code, so data is not reliable.
        public const int CHANNEL = BASE - 10;
        //No usable communication channel for 
        //packet transmission/reception.
        public const int HANDLE = BASE - 11;
        //Handle of specified bi-directional 
        //communication module is invalid.
        public const int BUSY = BASE - 12;
        //Port could not be opened while 
        //printer is printing (communicating).
        public const int LOADDLL = BASE - 13;
        //Failure in loading bi-directional 
        //communication module.
        public const int DEVICEID = BASE - 14;
        //Specified DeviceID information is 
        //invalid.
        public const int PRNHANDLE = BASE - 15;
        //Specified printer handle is invalid.
        public const int PORT = BASE - 16;
        //Unsupported printer path name was 
        //specified.
        public const int TIMEOUT = BASE - 17;
        //Receive processing stopped due to a 
        //time out.
        public const int JOB1 = BASE - 18;
        //SNMP OID mismatch.
        public const int JOB2 = BASE - 19;
        //SNMP Bad value.
        public const int JOB3 = BASE - 20;
        //SNMP No such name.
        public const int SERVICE = BASE - 25;
        //Core service error.

        public const int OTHER = -1000; //Other error
    }

    class TYPE
    {
        public const int REMOTE = 1;        //Printer compatible with remote 
                                            //commands (INK/SIDM printer)
        public const int UNKNOWN = 100;     //Type cannot be determined.
    }

    class PATHTYPE                                  //Printer path type
    {
        public const int PORT = 0;      //Port
        public const int PRINTER = 1;       //Printer registration name
    }


    //-------------------------------------
    // Definitions relating to information 
    // acquire/set APIs using SNMP
    //-------------------------------------
    ///////////////////////////////////////
    //	SNMP command
    ///////////////////////////////////////
    class SNMP
    {
        public const int GET = 0x01;        //GET
        public const int GETNEXT = 0x02;        //GET_NEXT
        public const int SET = 0x03;        //SET
    }

    ///////////////////////////////////////
    //	SNMP data type
    ///////////////////////////////////////
    class DATATYPE
    {
        public const int INTEGER = 0x02;        //Integer
        public const int OCTETSTR = 0x04;       //Character string
        public const int UNKNOWN = 0xFE;        //Unknown
        public const int BUFFSIZE = 0xFF;       //Insufficient buffer size
    }

    //=============================================================================
    //	Structure definition
    //=============================================================================
#pragma warning disable 649
    //=====================================
    //	DeviceID information
    //=====================================
    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    struct DEVICEID01
    {
        public int Size;                        //Size
        public int Version;                 //Version
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 256)]
        public char[] MFG;                      //Manufacturer name
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 256)]
        public char[] CMD;                      //Support command type
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 256)]
        public char[] MDL;                      //Product name
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 256)]
        public char[] CLS;                      //Device type
        public int PrnTypes;                    //Printer type
    }

    //=====================================
    //	STATUS information
    //=====================================
    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    struct STATUSVERSION                            //Status information version
    {
        public ushort MajorVersion;             //Major version
        public ushort MinerVersion;             //miner version
    }

    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    struct STATUSHEADER                             // Status information common header
    {
        public uint Size;                       // Size of Structure
        public STATUSVERSION Version;               // Version of Structure
    }

    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    struct CARTRIDGESTATUS
    {                                               //Cartridge and ink information
        public byte CartridgeType;              //Cartridge name code
        public uint ColorType;                  //Cartridge color code
        public byte InkRest;                    //Ink rest information
        public byte InkDimension;               //Ink dimension information
    }


    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    struct PAPERPRINTCOUNT
    {
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 64)]
        public char[] PaperSizeName;
        public uint PrintedNumber;
    }

    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    struct PAPERTRAYREMAIN
    {
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 64)]
        public char[] PaperSizeName;
        public uint PaperSizeType;
        public uint PaperRemain;
    }

    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    struct STATUS_IJP
    {
        public uint Size;						                    // this struct size
        public STATUSVERSION Version;					            // struct version

        public byte StatusReplyType;			                    // status reply type
        public byte StatusCode;				                        // status code
        public byte ErrorCode;					                    // error code
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 16)]
        public byte[] WarningCode;			                        // warning code

        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 3)]
        public byte[] InkRemainInfo;			                    // ink remain information

        public uint MonochromePrintedNumber;	                    // monochrome printed number
        public uint ColorPrintedNumber;			                    // color printed number

        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 16)]
        public CARTRIDGESTATUS[] CartridgeStatus;                   // cartridge and ink information

        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 10)]
        public byte[] SerialNo;			                            // serial number       

        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 32)]
        public PAPERPRINTCOUNT[] PaperPrintCount;                   // printed count information of each paper size
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 8)]
        public PAPERTRAYREMAIN[] PaperTrayInfo;                     // paper tray information

    }

    //-------------------------------------
    //	OID information structure
    //-------------------------------------
    class OIDSTR
    {
        public const int SIZE = 128;                //Buffer size
    }
    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    struct OIDINFO
    {
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = OIDSTR.SIZE)]
        public char[] OID;                      //Area for storing OID as character 
                                                //string
        public int DataType;                    //Data type
        public IntPtr Buffer;                       //Buffer for value acquire/set
        public int BuffSize;                    //Size of Buffer
    }
#pragma warning restore 649

    //=============================================================================
    //	API definition
    //=============================================================================
    public static class EpsonAPI
    {
        /// <summary>
        /// 初始化SDK
        /// </summary>
        /// <returns></returns>
        [DllImport("/Printer/EPSON/EpsonNetSDK.dll")]
        public extern static int ENSInitialize();

        /// <summary>
        /// 释放SDK资源
        /// </summary>
        /// <returns></returns>
        [DllImport("/Printer/EPSON/EpsonNetSDK.dll")]
        public extern static int ENSRelease();

        /// <summary>
        /// 获取打印机的设备id
        /// </summary>
        /// <param name="PathType"></param>
        /// <param name="PrnPath"></param>
        /// <param name="IdBuff"></param>
        /// <param name="BuffLen"></param>
        /// <param name="StructVersion">目前只支持1</param>
        /// <returns></returns>
        [DllImport("/Printer/EPSON/EpsonNetSDK.dll")]
        public extern static int ENSGetDeviceID(
                                        int PathType,
                                        [MarshalAs(UnmanagedType.LPStr)] string PrnPath,
                                        [Out] IntPtr IdBuff,
                                        ref int BuffLen,
                                        int StructVersion);

        /// <summary>
        /// 打开指定打印机的通讯
        /// </summary>
        /// <param name="PathType"></param>
        /// <param name="PrnPath"></param>
        /// <param name="IdBuff"></param>
        /// <param name="PrnHandle"></param>
        /// <returns></returns>
        [DllImport("/Printer/EPSON/EpsonNetSDK.dll")]
        public extern static int ENSOpenCommunication(
                                        int PathType,
                                        [MarshalAs(UnmanagedType.LPStr)] string PrnPath,
                                        [In] IntPtr IdBuff,
                                        out IntPtr PrnHandle);

        /// <summary>
        /// 关闭指定打印机的通讯
        /// </summary>
        /// <param name="PrnHandle"></param>
        /// <returns></returns>
        [DllImport("/Printer/EPSON/EpsonNetSDK.dll")]
        public extern static int ENSCloseCommunication(IntPtr PrnHandle);

        /// <summary>
        /// 获取信息
        /// </summary>
        /// <param name="PrnHandle"></param>
        /// <param name="Command"></param>
        /// <param name="GetParam"></param>
        /// <param name="GetBuff"></param>
        /// <param name="BuffLen"></param>
        /// <returns></returns>
        [DllImport("/Printer/EPSON/EpsonNetSDK.dll")]
        public extern static int ENSGetInformation(
                                        IntPtr PrnHandle,
                                        [MarshalAs(UnmanagedType.LPStr)] string Command,
                                        [In] IntPtr GetParam,
                                        [Out] IntPtr GetBuff,
                                        ref int BuffLen);

        /// <summary>
        /// 通过指定 OID 获取和设置网络连接的打印机的值
        /// </summary>
        /// <param name="PrnHandle"></param>
        /// <param name="CommandType"></param>
        /// <param name="OidInfo"></param>
        /// <param name="OidNum"></param>
        /// <returns></returns>
        [DllImport("/Printer/EPSON/EpsonNetSDK.dll")]
        public extern static int ENSGetSetSNMPRequest(
                                        IntPtr PrnHandle,
                                        int CommandType,
                                        IntPtr OidInfo,
                                        int OidNum);
    }

    /// <summary>
    /// 指示打印机状态
    /// </summary>
    public enum EnumStatusCode
    {
        /// <summary>
        /// 错误状态
        /// </summary>
        错误状态 = 0,

        /// <summary>
        /// 自动打印模式
        /// </summary>
        自动打印模式 = 1,

        /// <summary>
        /// 忙碌
        /// </summary>
        忙碌 = 2,

        /// <summary>
        /// 等待（工作）
        /// </summary>
        等待 = 3,

        /// <summary>
        /// 空闲
        /// </summary>
        空闲 = 4,

        /// <summary>
        /// 未准备好
        /// </summary>
        未准备好 = 5,

        /// <summary>
        /// 
        /// </summary>
        油墨不足 = 6,

        /// <summary>
        /// 
        /// </summary>
        正在清理中 = 7,

        /// <summary>
        /// 工厂运输状态？
        /// </summary>
        Factory_Shipping_Status = 8,

        /// <summary>
        /// Motor energization OFF
        /// </summary>
        电机断电 = 9,

        /// <summary>
        /// Shutting down
        /// </summary>
        关机 = 10,

        /// <summary>
        /// Waiting for paper initialization start trigger
        /// </summary>
        等待纸张初始化 = 11,

        /// <summary>
        /// Paper initializing
        /// </summary>
        纸张初始化 = 12,

        /// <summary>
        /// Converting Black ink type
        /// </summary>
        转换油墨类型 = 13,

        /// <summary>
        /// Waiting for the heater to adjust to the setting temperature
        /// </summary>
        等待加热器调节到设定温度 = 14,

        /// <summary>
        /// Initializing printer startup
        /// </summary>
        初始化打印机启动 = 15,

        /// <summary>
        /// 其他
        /// </summary>
        其他 = 19,
    }

    /// <summary>
    /// 打印机错误状态
    /// </summary>
    public enum EnumErrorCode
    {
        /// <summary>
        /// 致命错误
        /// </summary>
        FatalError = 0,

        /// <summary>
        /// 从其他接口执行请求
        /// </summary>
        ExecutingTheRequestFromOtherInterface = 1,

        /// <summary>
        /// 封盖开启错误
        /// </summary>
        CoverOpenError = 2,

        /// <summary>
        /// 卡纸了
        /// </summary>
        PaperJam = 4,

        /// <summary>
        /// 没墨了
        /// </summary>
        OutOfInk = 5,

        /// <summary>
        /// 出纸
        /// </summary>
        PaperOut = 6,

        /// <summary>
        /// 未知错误
        /// </summary>
        UnknownError = 8,

        /// <summary>
        /// 纸张尺寸错误
        /// </summary>
        PaperSizeError = 10,

        /// <summary>
        /// 纸张类型不匹配
        /// </summary>
        PaperTypeMismatch = 12,

        /// <summary>
        /// 无法弹出纸张
        /// </summary>
        UnableToPaperEject = 14,

        /// <summary>
        /// 左侧维修箱满了
        /// </summary>
        MaintenanceBoxFull_Left = 16,

        /// <summary>
        /// 双给进错误
        /// </summary>
        DoubleFeedError = 18,

        /// <summary>
        /// 清楚不可能错误
        /// </summary>
        CleaningImpossibleError = 22,

        /// <summary>
        /// 纸张识别错误
        /// </summary>
        PaperRecognitionError = 23,

        /// <summary>
        /// 纸斜
        /// </summary>
        PaperSkew = 24,

        /// <summary>
        /// 清理计数溢出错误
        /// </summary>
        CleaningCountOverrunError = 25,

        /// <summary>
        /// 墨水盖打开
        /// </summary>
        InkCoverOpen = 26,

        /// <summary>
        /// 没有左侧维修箱
        /// </summary>
        NoMaintenanceBox_Left = 34,

        /// <summary>
        /// 墨盒组合错误
        /// </summary>
        InkCartridgeCombinationError = 35,

        /// <summary>
        /// 后盖开启误差
        /// </summary>
        RearCoverOpenError = 37,

        /// <summary>
        /// 自动调整发生不可能的错误
        /// </summary>
        AutomaticAdjustmentImpossibleError = 39,

        /// <summary>
        /// 清理失败错误
        /// </summary>
        CleaningFailureError = 40,

        /// <summary>
        /// 没有纸托盘
        /// </summary>
        NoPaperTray = 41,

        /// <summary>
        /// 读卡错误
        /// </summary>
        CardReadingError = 42,

        /// <summary>
        /// CD/DVD 读取错误
        /// </summary>
        CD_DVD_Reading_Error = 43,

        /// <summary>
        /// 右侧维修箱弹出
        /// </summary>
        MaintenanceBoxOut_Right = 45,

        /// <summary>
        /// 右侧维修箱满了
        /// </summary>
        MaintenanceBoxFull_Right = 47,

        /// <summary>
        /// 封盖开启误差(单封盖型号)
        /// </summary>
        CoverOpenError_Single = 55,

        /// <summary>
        /// 中间的维修箱已满
        /// </summary>
        MaintenanceBoxFull_Center = 57,

        /// <summary>
        /// 中间的维修箱弹出
        /// </summary>
        MaintenanceBoxOut_Center = 58,

        /// <summary>
        /// 不能打开油墨盖
        /// </summary>
        CannotOpenInkCover = 59,
    }

}
