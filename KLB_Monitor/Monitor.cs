using BaseUtils;
using KLB_Monitor.Core;
using KLB_Monitor.HIS;
using KLB_Monitor.HIS.Model;
using KLB_Monitor.Model.Enum;
using KLB_Monitor.Model.Global;
using KLB_Monitor.Model.request;
using KLB_Monitor.window;
using log4net;
using Microsoft.VisualBasic.Logging;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.ComponentModel;
using System.Configuration;
using System.Diagnostics;
using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Reflection.Emit;
using System.Security.Policy;
using System.ServiceProcess;
using System.Text;
using System.Windows.Forms;
using System.Xml.Linq;
using static System.Windows.Forms.VisualStyles.VisualStyleElement;

namespace KLB_Monitor
{
    public partial class Monitor : Form
    {
        #region property
        private ILog _Logger = LogManager.GetLogger("Monitor");
        private string device_id = "";  //设备id
        private string HeartFile = "";  //壳体心跳地址
        private string server_url = ""; //服务端地址
        private string VersionFile = "VersionNumber.log";

        private DateTime LastRunTime = DateTime.Now;    //上一次重启壳体的时间
        private int ShellRestartFailCount = 0;          //壳体实际重启失败的次数
        private int ShellRetryCount = 0;                //壳体重启重试的次数
        private int HISRetryCount = 0;                  //HIS重启重试的次数
        private int HISRetryFailCount = 0;              //HIS重启失败的次数
        private int MiddleRetryFailCount = 0;           //中间件重启重试失败的次数
        private int MiddleRetryCount = 0;               //中间件重启重试的次数
        private int ConnectRetryCount = 0;              //连接失败重试的次数
        private int ConnectCount = 0;                   //连接次数
        private int MidlleConRetryCount = 0;            //中间件重连尝试次数

        //Task
        private BaseTask shutdownTask;
        private BaseTask shellMonitorTask;
        private BaseTask commandHandleTask;
        private BaseTask commandGainTask;
        private BaseTask connectTask;
        private BaseTask ordinaryPrinterTask;
        private BaseTask epsonPrinterTask;
        private BaseTask middleTask;
        private BaseTask hisTask;

        Server server = new Server();
        RedisHelper redisHandle = new RedisHelper();
        private Queue eventQueue = new Queue();

        #region 是否错误
        private bool IsErr = false;         //是否发生错误，总体的
        private bool IsSystemErr = false;   //错误范围：联网失败
        private bool IsThirdErr = false;    //错误范围：中间件、HIS、打印机监控异常
        private bool IsShellErr = false;    //错误范围：壳体系统异常
        private bool IsHisMonitor = false;  //是否启用HIS监控
        #endregion
        #endregion

        #region 初始化
        public Monitor()
        {
            InitializeComponent();
        }

        /// <summary>
        /// 加载
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Monitor_Load(object sender, EventArgs e)
        {
            Init();
            ParamInit();
            DeleteLog();    //清除日志
            UpdateLocalTime();
            TaskInit();     //last
        }

        /// <summary>
        /// 程序退出前处理
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Monitor_FormClosing(object sender, FormClosingEventArgs e)
        {
            DialogResult result = MessageBox.Show("是否退出？选否,最小化", "操作提示", MessageBoxButtons.YesNo, MessageBoxIcon.Question);
            if (result == DialogResult.Yes)
            {
                StopTask();
                eventQueue.ClearQueue();

                this.Dispose();
                //Application.Exit();
                Environment.Exit(0);
            }
            else if (result == DialogResult.No)
            {
                WindowState = FormWindowState.Minimized;
                //this.Hide();
                //隐藏任务栏区图标 
                this.ShowInTaskbar = false;
                e.Cancel = true;
            }
            else if (result == DialogResult.Cancel)
            {
                //什么事都不干
                e.Cancel = true;
            }
            
        }

        /// <summary>
        /// 初始化
        /// </summary>
        private void Init()
        {
            this.WindowState = FormWindowState.Minimized;
            this.MinimizeBox = false;   //屏蔽最小化按钮
            this.MaximizeBox = false;   //屏蔽最大化按钮
            this.ShowInTaskbar = false;
            this.TopMost = false;

            listView1.Items.Clear();
            StepDetailsShow("程序开始运行");

            device_id = ConfigurationManager.AppSettings.Get("device_id") ?? "";
            if (device_id.IsNullOrEmpty())
            {
                StepDetailsShow("未设置当前设备id");
                return;
            }

            server_url = ConfigurationManager.AppSettings.Get("api_url") ?? "";
            if (server_url.IsNullOrEmpty())
            {
                StepDetailsShow("未设置服务端地址");
                return;
            }

            HeartFile = ConfigurationManager.AppSettings.Get("HeartFile") ?? "";
            if (HeartFile.IsNullOrEmpty())
            {
                StepDetailsShow("未设置壳体心跳地址");
                return;
            }

            //壳体重启尝试的次数
            var retrycount1 = ConfigurationManager.AppSettings.Get("ShellRetryCount") ?? "";
            if (retrycount1.IsNullOrEmpty())
            {
                ShellRetryCount = 3;
            }
            else
            {
                ShellRetryCount = retrycount1.ToInt();
            }

            //HIS重启尝试的次数
            var retrycount2 = ConfigurationManager.AppSettings.Get("HISRetryCount") ?? "";
            if (retrycount2.IsNullOrEmpty())
            {
                HISRetryCount = 3;
            }
            else
            {
                HISRetryCount = retrycount2.ToInt();
            }

            //中间件重启尝试的次数
            var retrycount3 = ConfigurationManager.AppSettings.Get("MiddleRetryCount") ?? "";
            if (retrycount3.IsNullOrEmpty())
            {
                MiddleRetryCount = 3;
            }
            else
            {
                MiddleRetryCount = retrycount3.ToInt();
            }

            var monitor_state  = ConfigurationManager.AppSettings.Get("IsHisMonitor") ?? "";
            if (monitor_state.IsNotNullOrEmpty() && monitor_state.ToLower().Equals("true"))
            {
                IsHisMonitor = true;
            }
            

            if (!server.IsConnet(server_url, device_id))
            {
                StepDetailsShow($"服务端{server_url}访问失败");
                return;
            }

        }

        /// <summary>
        /// 开启任务
        /// </summary>
        private void TaskInit()
        {
            shutdownTask = new BaseTask(ShutDown);              //自动关机
            shellMonitorTask = new BaseTask(ShellMonitor);      //壳体状态监控
            commandHandleTask = new BaseTask(CommandHanle);     //指令解析
            commandGainTask = new BaseTask(CommandGain);        //指令获取
            connectTask = new BaseTask(CheckConnect);           //服务端连接状态
            ordinaryPrinterTask = new BaseTask(PrinterMonitor); //普通打印机的状态监控
            epsonPrinterTask = new BaseTask(EpsonPrinterMonitor);   //epson打印机的状态监控
            middleTask = new BaseTask(MiddleMonitor);           //中间件监控
            hisTask = new BaseTask(HISMonitor);                 //HIS监控

            StartTask();
        }

        /// <summary>
        /// 开启任务
        /// </summary>
        private void StartTask()
        {
            shutdownTask.Start();
            shellMonitorTask.Start();
            commandGainTask.Start();
            commandHandleTask.Start();
            connectTask.Start();
            ordinaryPrinterTask.Start();
            epsonPrinterTask.Start();
            middleTask.Start();
            hisTask.Start();
        }

        /// <summary>
        /// 关闭任务
        /// </summary>
        private void StopTask()
        {
            shutdownTask.Stop();
            shellMonitorTask.Stop();
            commandGainTask.Stop();
            commandHandleTask.Stop();
            connectTask.Stop();
            ordinaryPrinterTask.Stop();
            epsonPrinterTask.Stop();
            middleTask.Stop();
            hisTask.Stop();
        }

        /// <summary>
        /// 获取参数
        /// </summary>
        private void ParamInit()
        {
            var err_msg = server.GetParameter(server_url, device_id);
            if (err_msg.IsNotNullOrEmpty())
            {
                StepDetailsShow(err_msg);
                UpdateShowDeviceName("");
                UpdateShowVerion("");
            }
            else
            {
                UpdateShowVerion(Global.param.version);
                UpdateShowDeviceName(Global.param.device_name);
            }
        }
        #endregion

        #region 最小化任务栏图标
        /// <summary>
        /// 只支持左键双击
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void notifyIcon1_MouseDoubleClick(object sender, MouseEventArgs e)
        {
            if (this.WindowState == FormWindowState.Minimized && e.Button == MouseButtons.Left)
            {
                this.WindowState = FormWindowState.Normal;
                this.ShowInTaskbar = true;
                //this.Show();
            }
        }

        /// <summary>
        /// 右键功能
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void notifyIcon1_MouseClick(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Right)
            {
                contextMenuStrip1.Show(Control.MousePosition);
            }
        }

        /// <summary>
        /// 右键的退出
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void MenuItem_Quit_Click(object sender, EventArgs e)
        {
            this.Dispose();
            Application.Exit();
        }
        #endregion

        #region private
        
        /// <summary>
        /// 清除日志
        /// </summary>
        private void DeleteLog()
        {
            var DeleteLogDay = ConfigurationManager.AppSettings.Get("DeleteLogDay").ToString();
            int day = DeleteLogDay.ToInt();
            if(day <= 0) 
            {
                //默认是7天
                day = 7;
            }
            DateTime keepTime = DateTime.Now.AddDays(-day);
            #region 删除监控程序的日志
            _Logger.Info("监控程序日志清除开始");
            string monitor_file = Path.Combine(Environment.CurrentDirectory, "Logs");
            try
            {
                this.DeleteDir(monitor_file, keepTime);
            }
            catch (Exception ex)
            {
                _Logger.Error($"{monitor_file}路径日志删除失败{ex.Message}");
            }
            _Logger.Info("监控程序日志清除结束");
            #endregion

            #region 删除壳体的日志
            _Logger.Info("壳体日志清除开始");
            if((Global.param?.cef_exe_full_path ?? "").IsNotNullOrEmpty())
            {
                try
                {
                    string filePath = Path.GetDirectoryName(Global.param.cef_exe_full_path);
                    string shell_file = Path.Combine(filePath, "Logs");
                    this.DeleteDir(shell_file, keepTime);
                }
                catch (Exception ex)
                {
                    _Logger.Error($"壳体日志删除失败{ex.Message}");
                }
                _Logger.Info("壳体日志清除结束");
            }
            #endregion
        }

        /// <summary>
        /// 删除路径下所有的文件和文件夹
        /// </summary>
        /// <param name="file"></param>
        /// <param name="keepTime">删除多久之前的数据</param>
        private void DeleteDir(string file, DateTime keepTime)
        {
            try
            {
                //去除文件夹和子文件的只读属性
                //去除文件夹的只读属性
                DirectoryInfo fileInfo = new DirectoryInfo(file);
                fileInfo.Attributes = FileAttributes.Normal & FileAttributes.Directory;

                //去除文件的只读属性
                File.SetAttributes(file, FileAttributes.Normal);

                //判断文件夹是否还存在
                if (Directory.Exists(file))
                {
                    foreach (string f in Directory.GetFileSystemEntries(file))
                    {
                        if (File.Exists(f))
                        {
                            FileInfo fl = new FileInfo(f);
                            //如果有子文件删除文件
                            if(keepTime > fl.LastWriteTime)
                            {
                                //超时的删除
                                File.Delete(f);
                            }
                        }
                        else
                        {
                            //循环递归删除子文件夹
                            DeleteDir(f, keepTime);
                        }
                    }

                    //删除空文件夹
                    if (!Directory.GetFileSystemEntries(file).Any())
                    {
                        Directory.Delete(file);
                    }
                }

            }
            catch (Exception ex) // 异常处理
            {
                _Logger.Error($"DeleteDir Fail{ex.Message}\r\n{ex.StackTrace}");
            }
        }
        
        /// <summary>
        /// 自动关机
        /// </summary>
        private void ShutDown()
        {
            if (Global.param != null && Global.param.auto_shutdown_time.IsNotNullOrEmpty() 
                && Global.param.auto_shutdown_time.Length == 4 
                && string.Compare(DateTime.Now.ToString("HHmm"), Global.param.auto_shutdown_time) >= 0)
            {
                Process.Start("shutdown.exe", "-s");//关机
            }

            Thread.Sleep(10000);
        }

        /// <summary>
        /// 更新本地时间
        /// </summary>
        private void UpdateLocalTime()
        {
            try
            {
                //请求服务端的时间并进行解析
                //开始之前要先判断一下服务端之间是否连接正常
                var time = server.GetRemoteTime(server_url, device_id);
                if (time.IsNotNullOrEmpty() && time.Length == 14)
                {
                    int result = Win32API.SyscServiceTime(time);
                    if (result != 1)
                    {
                        StepDetailsShow($"获取服务器时间失败");
                        _Logger.Error($"获取服务器时间失败");
                    }
                }
            }
            catch (Exception ex)
            {
                StepDetailsShow($"获取服务器时间失败：{ex.Message}\r\n{ex.StackTrace}");
                _Logger.Error($"获取服务器时间失败：{ex.Message}\r\n{ex.StackTrace}");
            }
            
        }

        /// <summary>
        /// 指令解析
        /// </summary>
        private void CommandHanle()
        {
            int? command = eventQueue.Dequeue();
            if (command.HasValue)
            {
                _Logger.Debug($"指令解析结果：{command}");

                switch (command.Value)
                {
                    case (int)EnumCommand.TurnOff:
                        Process.Start("shutdown.exe", "-s");//关机
                        break;
                    case (int)EnumCommand.Reboot:
                        Process.Start("shutdown.exe", "-r");//重启
                        break;
                    case (int)EnumCommand.WriteOff:
                        Process.Start("shutdown.exe", "-l");//注销
                        break;
                    case (int)EnumCommand.UpdateVersion:
                        UpdateVersion();    //更新版本
                        break;
                    case (int)EnumCommand.ScreenShot:
                        ScreenShot();   //截屏并上传
                        break;
                    default:
                        break;
                }
            }

            Thread.Sleep(100);
        }

        /// <summary>
        /// 获取命令
        /// </summary>
        private void CommandGain()
        {
            try
            {
                var instruct = server.GetCommand(server_url, device_id);
                if (instruct > 0)
                {
                    eventQueue.Enqueue(instruct);
                }
            }
            catch (Exception ex)
            {
                _Logger.Error($"获取指令异常：{ex.Message}\r\n{ex.StackTrace}");
            }

            Thread.Sleep(3000);
        }

        /// <summary>
        /// 检查连接状态
        /// </summary>
        private void CheckConnect()
        {
            try
            {
                ConnectCount++;
                if(!server.IsConnet(server_url, device_id, ConnectCount >= 10 ? 1 : 0))
                {
                    StepDetailsShow("网络连接异常");
                    ConnectRetryCount++;

                    if(ConnectRetryCount >= 10 && !this.IsErr)
                    {
                        //_Logger.Info("start CheckConnect");
                        Task.Run(() => { OpenErrWindow("联网异常", (int)EnumErrorLevel.System); });
                    }
                }
                else
                {
                    if (ConnectCount >= 10)
                    {
                        ConnectCount = 0;
                    }
                    ConnectRetryCount = 0;
                }
            }
            catch (Exception ex)
            {
                _Logger.Error($"轮询检测服务端连接状态异常：{ex.Message}\r\n{ex.StackTrace}");
            }
            Thread.Sleep(1000);
        }
        #endregion

        #region 更新包下载
        /// <summary>
        /// 更新版本
        /// 在此期间需要注意软件不能启动
        /// </summary>
        private void UpdateVersion()
        {
            if (Global.param != null 
                && (Global.param.download_url.IsNullOrEmpty() 
                || Global.param.update_filePath.IsNullOrEmpty()
                || Global.param.update_fileVersion.IsNullOrEmpty()
                || Global.param.cef_exe_full_path.IsNullOrEmpty()))
            {
                StepDetailsShow($"下载地址、版本号、壳体程序完整地址等不能为空");
                return;
            }

            try
            {
                //UpdateProcess = true;
                shellMonitorTask.Stop();
                string cef_dir_path = Path.GetDirectoryName(Global.param.cef_exe_full_path);
                string cef_name = Path.GetFileNameWithoutExtension(Global.param.cef_exe_full_path);
                string extension = Path.GetExtension(Global.param.cef_exe_full_path);
                string file = $"{Global.param.download_url}/{Global.param.update_filePath}";

                #region 停止程序
                this.StopProcess();
                StepDetailsShow($"壳体程序终止完成");
                #endregion

                Thread.Sleep(2000); //等待程序完全关闭

                #region 下载
                var zip_filename = Path.GetFileNameWithoutExtension(file);
                var zip_extension = Path.GetExtension(file);
                var fileName = $"{zip_filename}{zip_extension}";
                if (!Directory.Exists(cef_dir_path))
                {
                    Directory.CreateDirectory(cef_dir_path);
                }
                var zip_path = Path.Combine(cef_dir_path, fileName);
                StepDetailsShow($"即将开始访问");
                WebRequest request = WebRequest.Create(file);
                WebResponse respone = request.GetResponse();
                var total = respone.ContentLength;
                Stream netStream = respone.GetResponseStream();
                Stream fileStream = new FileStream(zip_path, FileMode.Create);
                byte[] read = new byte[1024];
                long progressBarValue = 0;
                int realReadLen = netStream.Read(read, 0, read.Length);

                BarShow();
                StepDetailsShow($"开始下载更新包");
                while (realReadLen > 0)
                {
                    fileStream.Write(read, 0, realReadLen);
                    progressBarValue += realReadLen;
                    BarUpdate(Convert.ToInt32(Math.Floor(progressBarValue * 100.0 / total)));
                    realReadLen = netStream.Read(read, 0, read.Length);
                }
                netStream.Close();
                fileStream.Close();
                StepDetailsShow($"更新包下载完成");
                #endregion

                #region 解压
                ZIP.UnZip(zip_path, cef_dir_path);
                File.Delete(zip_path);
                StepDetailsShow($"更新包解压完成");
                #endregion

                #region 本地更新版本号
                StreamWriter sw = new StreamWriter(Path.Combine(cef_dir_path, VersionFile));
                sw.Write(Global.param.update_fileVersion);
                sw.Close();
                StepDetailsShow($"版本号更新完成");
                #endregion

                #region 重启壳体
                Restart();
                StepDetailsShow($"壳体程序重启完成");
                #endregion

                #region 回传版本信息
                if(!server.FeedBackVersion(server_url, device_id, Global.param.update_fileVersion))
                {
                    StepDetailsShow("回传版本号失败");
                }
                #endregion

            }
            catch (Exception ex)
            {
                _Logger.Error($"更新失败：{ex.Message}");
                StepDetailsShow($"更新失败：{ex.Message}");
            }
            finally
            {
                shellMonitorTask.Start();
                Global.param.update_filePath = string.Empty;
                Global.param.update_fileVersion = string.Empty;
            }
        }

        /// <summary>
        /// 重启壳体
        /// </summary>
        private void Restart()
        {
            try
            {
                #region 停止程序
                this.StopProcess();
                #endregion

                Process process = new Process();
                process.StartInfo.FileName = Global.param.cef_exe_full_path;
                process.Start();

                LastRunTime = DateTime.Now;
            }
            catch (Exception ex)
            {
                StepDetailsShow($"程序重启失败：{ex.Message}\r\n{ex.StackTrace}");
                _Logger.Error($"程序重启失败：{ex.Message}\r\n{ex.StackTrace}");
            }
        }

        /// <summary>
        /// 停止应用程序
        /// </summary>
        /// <param name="exeName"></param>
        private void StopProcess()
        {
            if (Global.param == null)
            {
                _Logger.Error("参数获取失败，无法重启壳体");
                return;
            }
            if (!File.Exists(Global.param.cef_exe_full_path))
            {
                return;
            }

            string fileName = Path.GetFileNameWithoutExtension(Global.param.cef_exe_full_path);
            Process[] arrayProcess = Process.GetProcessesByName(fileName);
            if (arrayProcess != null && arrayProcess.Any())
            {
                foreach (Process p in arrayProcess)
                {
                    p.Kill();
                }
            }
        }

        /// <summary>
        /// 截屏
        /// </summary>
        private void ScreenShot()
        {
            try
            {
                StepDetailsShow("开始截屏");
                ScreenShot sc = new ScreenShot();
                Image img = sc.CaptureScreen();
                string dir_path = Path.Combine(Application.StartupPath, "Shot");
                string file_path = Path.Combine(dir_path, "Shot.png");

                if (!Directory.Exists(dir_path))
                {
                    Directory.CreateDirectory(dir_path);
                }
                img.Save(file_path, System.Drawing.Imaging.ImageFormat.Png);

                ImageHelper image = new ImageHelper();
                string base64 = image.ConvertImageToBase64(file_path);
                if (!server.UploadFile(server_url, device_id, base64))
                {
                    _Logger.Error($"截屏文件上传失败");
                    StepDetailsShow("截屏文件上传失败");
                }

                File.Delete(file_path);
                StepDetailsShow("截屏完成");
            }
            catch (Exception ex)
            {
                StepDetailsShow($"截屏异常：{ex.Message} \r\n {ex.StackTrace}");
                _Logger.Error($"截屏异常：{ex.Message} \r\n {ex.StackTrace}");
            }
        }

        /// <summary>
        /// 重启HIS程序
        /// </summary>
        private void RestartHIS(string filePath)
        {
            try
            {
                #region 如果有则杀进程
                if (!File.Exists(filePath))
                {
                    return;
                }

                string fileName = Path.GetFileNameWithoutExtension(filePath);
                Process[] arrayProcess = Process.GetProcessesByName(fileName);
                if (arrayProcess != null && arrayProcess.Any())
                {
                    foreach (Process p in arrayProcess)
                    {
                        p.Kill();
                    }
                }
                #endregion

                #region 启动exe
                Process process = new Process();
                process.StartInfo.FileName = filePath;
                process.Start();
                #endregion
            }
            catch (Exception ex)
            {
                _Logger.Error($"重启HIS失败：{ex.Message}\r\n{ex.StackTrace}");
            }
        }

        /// <summary>
        /// 重启中间件
        /// </summary>
        private void RestartMidlle(string service_name)
        {
            try
            {
                //先停止再开启
                StopWindowsServer(service_name, 10000);
                StartWindowsServer(service_name, 10000);
            }
            catch (Exception ex)
            {
                _Logger.Error($"重启中间件服务{service_name}失败：{ex.Message}\r\n{ex.StackTrace}");
            }
        }

        /// <summary>
        /// 启动计算机服务
        /// </summary>
        /// <param name="service_name">服务名称</param>
        /// <param name="timeout">超时时间</param>
        private void StartWindowsServer(string service_name, int timeout=1000)
        {
            try
            {
                ServiceController service = new ServiceController(service_name);

                TimeSpan ts = TimeSpan.FromMilliseconds(timeout);
                service.Start();
                service.WaitForStatus(ServiceControllerStatus.Running, ts);
            }
            catch (Exception ex)
            {
                _Logger.Error($"启动服务{service_name}失败：{ex.Message}\r\n{ex.StackTrace}");
            }
        }

        /// <summary>
        /// 停止计算机服务
        /// </summary>
        /// <param name="service_name">服务名称</param>
        /// <param name="timeout">超时时间</param>
        private void StopWindowsServer(string service_name,int timeout = 1000)
        {
            try
            {
                ServiceController service = new ServiceController(service_name);

                TimeSpan ts = TimeSpan.FromMilliseconds(timeout);
                service.Stop();
                service.WaitForStatus(ServiceControllerStatus.Stopped, ts);
            }
            catch (Exception ex)
            {
                _Logger.Error($"停止服务{service_name}失败：{ex.Message}\r\n{ex.StackTrace}");
            }
        }
        #endregion

        #region 监控
        /// <summary>
        /// 壳体监控
        /// </summary>
        private void ShellMonitor()
        {
            try
            {
                //_Logger.Info("正在进行自助机监控");
                FileInfo file = new FileInfo(HeartFile);
                //同时校验壳体上一次被重启的时间，避免频繁重启
                if (file.LastWriteTime.AddSeconds(10) < DateTime.Now && LastRunTime.AddSeconds(30) < DateTime.Now)
                {
                    ShellRestartFailCount++;
                    if (ShellRestartFailCount > ShellRetryCount)
                    {
                        if(!this.IsErr && !this.IsShellErr)
                        {
                            //_Logger.Info("start ShellMonitor");
                            Task.Run(() => { OpenErrWindow("壳体启动失败", (int)EnumErrorLevel.Shell); });
                        }
                    }
                    else
                    {
                        Restart();
                    }
                }
                else 
                {
                    ShellRestartFailCount = 0;
                }
            }
            catch (Exception ex)
            {
                _Logger.Error($"壳体重启失败：{ex.Message}\r\n{ex.StackTrace}");
                StepDetailsShow($"壳体重启失败：{ex.Message}\r\n{ex.StackTrace}");
            }
            Thread.Sleep(1000);
        }

        /// <summary>
        /// HIS监控
        /// 如果失败了要去重试的
        /// </summary>
        private void HISMonitor()
        {
            try
            {
                if (Global.param == null || this.IsThirdErr || !IsHisMonitor)
                {
                    return;
                }

                try
                {
                    MZ1001 request = new MZ1001
                    {
                        MAC_ADD = Global.param.his_macAdd,
                        CARD_DATA = Global.param.his_test_idcard,
                        CARD_TYPE = "14",   //类型-身份证
                        GET_SIGN_INFO = "0",
                        GET_PLAT_INFO = 1,
                    };

                    CloudHelper helper = new CloudHelper();
                    helper.MZ1001($"{Global.param.his_url}", request);
                    HISRetryFailCount = 0;
                }
                catch (Exception ex)
                {
                    _Logger.Error($"HIS接口请求异常：{ex.Message}\r\n{ex.StackTrace}");

                    if (HISRetryFailCount >= HISRetryCount && !this.IsErr)
                    {
                        //_Logger.Info("start HISMonitor");
                        Task.Run(() => { OpenErrWindow("HIS访问异常，请联系管理人员", (int)EnumErrorLevel.Third); });
                    }
                    else
                    {
                        //尝试重启中间件程序
                        RestartHIS(Global.param.his_exe_full_path);
                        Thread.Sleep(60000);
                    }
                    
                    HISRetryFailCount++;
                }
                
                
            }
            catch (Exception ex)
            {
                _Logger.Error($"HIS监控异常：{ex.Message}\r\n{ex.StackTrace}");
                throw;
            }
            Thread.Sleep(10000);
        }

        /// <summary>
        /// 中间件监控
        /// 仅服务的形式
        /// 如果失败了要去重试的
        /// </summary>
        private void MiddleMonitor()
        {
            try
            {
                if (Global.param != null && Global.param.middle_url.IsNotNullOrEmpty() && !this.IsThirdErr)
                {
                    var online = server.CheckMiddleOnline();
                    if (!online)
                    {
                        if (MiddleRetryFailCount >= MiddleRetryCount && !this.IsErr)
                        {
                            //_Logger.Info("start MiddleMonitor");
                            Task.Run(() => { OpenErrWindow("中间件运行异常", (int)EnumErrorLevel.Third); });
                        }

                        MidlleConRetryCount++;
                        if (MidlleConRetryCount >= 3)
                        {
                            RestartMidlle("MiddleWare");
                            MiddleRetryFailCount++;
                        }
                        
                    }
                    else
                    {
                        //重置计数
                        MidlleConRetryCount = 0;
                        MiddleRetryFailCount = 0;
                    }
                }
            }
            catch (Exception ex)
            {
                _Logger.Error($"中间件状态监控异常：{ex.Message}\r\n{ex.StackTrace}");
            }

            Thread.Sleep(10000);
        }

        /// <summary>
        /// 打印机状态监控
        /// 在此尽可能处理非epson打印机的情况
        /// 如果中间件、HIS、打印状态任一出现异常，都不进行监控
        /// </summary>
        private void PrinterMonitor()
        {
            try
            {
                string err_msg = string.Empty;

                if (Global.param != null && Global.param.printeNameList != null && Global.param.printeNameList.Any() && !this.IsThirdErr)
                {
                    var ordinary_prints = Global.param.printeNameList.Where(x => !x.ToUpper().StartsWith("EPSON")).ToList();

                    foreach (var orinary in ordinary_prints)
                    {
                        err_msg = PrinterHelper.GetPrinterStatus(orinary);
                        if (err_msg.IsNotNullOrEmpty() && !this.IsErr)
                        {
                            //如果有返回则认为有异常情况
                            Task.Run(() => { OpenErrWindow($"打印机{orinary}异常：{err_msg}", (int)EnumErrorLevel.Third); });
                            redisHandle.SetValue(orinary, err_msg);
                        }
                        else 
                        {
                            redisHandle.RemoveValue(orinary);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                _Logger.Error($"普通打印机状态监控异常：{ex.Message}\r\n{ex.StackTrace}");
            }

            Thread.Sleep(10000);
        }

        /// <summary>
        /// epson打印机的状态监控
        /// 如果中间件、HIS、打印状态任一出现异常，都不进行监控
        /// </summary>
        private void EpsonPrinterMonitor()
        {
            try
            {
                string err_msg = string.Empty;
                if (Global.param != null && Global.param.printeNameList != null && Global.param.printeNameList.Any() && !this.IsThirdErr)
                {
                    var epson_prints = Global.param.printeNameList.Where(x => x.ToUpper().StartsWith("EPSON")).ToList();

                    foreach (var epson in epson_prints)
                    {
                        //监控 EPSON类型打印机状态
                        err_msg = EpsonPrinterHelper.GetInfo(epson);
                        if (err_msg.IsNotNullOrEmpty() && !this.IsErr)
                        {
                            //_Logger.Info("start EpsonPrinterMonitor");
                            Task.Run(() => { OpenErrWindow($"打印机{epson}异常：{err_msg}", (int)EnumErrorLevel.Third); });
                            redisHandle.SetValue(epson, err_msg);
                        }
                        else 
                        {
                            redisHandle.RemoveValue(epson);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                _Logger.Error($"EPSON打印机状态监控异常：{ex.Message}\r\n{ex.StackTrace}");
            }

            Thread.Sleep(10000);
        }
        #endregion

        #region window
        /// <summary>
        /// 更新进度条进度
        /// </summary>
        /// <param name="value"></param>
        private void BarUpdate(int value)
        {
            this.Invoke(new Action(() =>
            {
                progressBar1.Value = value;
                label5.Text = $"{value}%";
            }));
            Application.DoEvents();
        }

        /// <summary>
        /// 更新进度条展示
        /// </summary>
        private void BarShow()
        {
            this.Invoke(new Action(() =>
            {
                progressBar1.Value = 0;
                progressBar1.Visible = true;
                label5.Visible = true;
            }));
            Application.DoEvents();
        }

        /// <summary>
        /// 步骤详情展示
        /// </summary>
        /// <param name="msg"></param>
        private void StepDetailsShow(string msg)
        {
            this.Invoke(new Action(() =>
            {
                listView1.Items.Add(msg);
            }));
            Application.DoEvents();
        }

        /// <summary>
        /// 更新当前展示的版本号
        /// </summary>
        /// <param name="msg"></param>
        private void UpdateShowVerion(string msg)
        {
            this.Invoke(new Action(() =>
            {
                versionLabel.Text = msg;
            }));
            Application.DoEvents();
        }

        /// <summary>
        /// 更新当前展示的设备名称
        /// </summary>
        /// <param name="msg"></param>
        private void UpdateShowDeviceName(string msg)
        {
            this.Invoke(new Action(() =>
            {
                deviceNameLabel.Text = msg;
            }));
            Application.DoEvents();
        }

        /// <summary>
        /// 打开异常界面
        /// </summary>
        /// <param name="msg"></param>
        /// <param name="errType"></param>
        private void OpenErrWindow(string msg, int errType)
        { 
            try
            {
                this.Invoke(new Action(() =>
                {
                    switch (errType)
                    {
                        case (int)EnumErrorLevel.System:
                            this.IsSystemErr = true;
                            break;
                        case (int)EnumErrorLevel.Third:
                            this.IsThirdErr = true;
                            break;
                        case (int)EnumErrorLevel.Shell:
                            this.IsShellErr = true;
                            break;
                        default:
                            break;
                    }
                    this.IsErr = true;
                    FrmErr frm = new FrmErr();
                    frm.Owner = this;
                    frm.TipMsg = msg;
                    frm._ClearErrInfo = ClearErrInfo;
                    this.Visible = false;
                    frm.ShowDialog();
                    //this.Visible = true;
                    frm.Dispose();
                }));
                Application.DoEvents();
            }
            catch (Exception ex)
            {
                _Logger.Error($"异常界面操作问题：{ex.Message}\r\n{ex.StackTrace}");
            }
            
        }

        /// <summary>
        /// 清除异常
        /// </summary>
        private void ClearErrInfo()
        {
            try
            {
                this.IsErr = false;
                this.IsSystemErr = false;
                this.IsThirdErr = false;
                this.IsShellErr = false;
            }
            catch (Exception ex)
            {
                _Logger.Error($"err init fail：{ex.Message}\r\n{ex.StackTrace}");
            }
            
        }
        #endregion
    }
}