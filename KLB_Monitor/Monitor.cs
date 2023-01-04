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
        private string device_id = "";  //�豸id
        private string HeartFile = "";  //����������ַ
        private string server_url = ""; //����˵�ַ
        private string VersionFile = "VersionNumber.log";

        private DateTime LastRunTime = DateTime.Now;    //��һ�����������ʱ��
        private int ShellRestartFailCount = 0;          //����ʵ������ʧ�ܵĴ���
        private int ShellRetryCount = 0;                //�����������ԵĴ���
        private int HISRetryCount = 0;                  //HIS�������ԵĴ���
        private int HISRetryFailCount = 0;              //HIS����ʧ�ܵĴ���
        private int MiddleRetryFailCount = 0;           //�м����������ʧ�ܵĴ���
        private int MiddleRetryCount = 0;               //�м���������ԵĴ���
        private int ConnectRetryCount = 0;              //����ʧ�����ԵĴ���
        private int ConnectCount = 0;                   //���Ӵ���
        private int MidlleConRetryCount = 0;            //�м���������Դ���

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

        #region �Ƿ����
        private bool IsErr = false;         //�Ƿ������������
        private bool IsSystemErr = false;   //����Χ������ʧ��
        private bool IsThirdErr = false;    //����Χ���м����HIS����ӡ������쳣
        private bool IsShellErr = false;    //����Χ������ϵͳ�쳣
        private bool IsHisMonitor = false;  //�Ƿ�����HIS���
        #endregion
        #endregion

        #region ��ʼ��
        public Monitor()
        {
            InitializeComponent();
        }

        /// <summary>
        /// ����
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Monitor_Load(object sender, EventArgs e)
        {
            Init();
            ParamInit();
            DeleteLog();    //�����־
            UpdateLocalTime();
            TaskInit();     //last
        }

        /// <summary>
        /// �����˳�ǰ����
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Monitor_FormClosing(object sender, FormClosingEventArgs e)
        {
            DialogResult result = MessageBox.Show("�Ƿ��˳���ѡ��,��С��", "������ʾ", MessageBoxButtons.YesNo, MessageBoxIcon.Question);
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
                //������������ͼ�� 
                this.ShowInTaskbar = false;
                e.Cancel = true;
            }
            else if (result == DialogResult.Cancel)
            {
                //ʲô�¶�����
                e.Cancel = true;
            }
            
        }

        /// <summary>
        /// ��ʼ��
        /// </summary>
        private void Init()
        {
            this.WindowState = FormWindowState.Minimized;
            this.MinimizeBox = false;   //������С����ť
            this.MaximizeBox = false;   //������󻯰�ť
            this.ShowInTaskbar = false;
            this.TopMost = false;

            listView1.Items.Clear();
            StepDetailsShow("����ʼ����");

            device_id = ConfigurationManager.AppSettings.Get("device_id") ?? "";
            if (device_id.IsNullOrEmpty())
            {
                StepDetailsShow("δ���õ�ǰ�豸id");
                return;
            }

            server_url = ConfigurationManager.AppSettings.Get("api_url") ?? "";
            if (server_url.IsNullOrEmpty())
            {
                StepDetailsShow("δ���÷���˵�ַ");
                return;
            }

            HeartFile = ConfigurationManager.AppSettings.Get("HeartFile") ?? "";
            if (HeartFile.IsNullOrEmpty())
            {
                StepDetailsShow("δ���ÿ���������ַ");
                return;
            }

            //�����������ԵĴ���
            var retrycount1 = ConfigurationManager.AppSettings.Get("ShellRetryCount") ?? "";
            if (retrycount1.IsNullOrEmpty())
            {
                ShellRetryCount = 3;
            }
            else
            {
                ShellRetryCount = retrycount1.ToInt();
            }

            //HIS�������ԵĴ���
            var retrycount2 = ConfigurationManager.AppSettings.Get("HISRetryCount") ?? "";
            if (retrycount2.IsNullOrEmpty())
            {
                HISRetryCount = 3;
            }
            else
            {
                HISRetryCount = retrycount2.ToInt();
            }

            //�м���������ԵĴ���
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
                StepDetailsShow($"�����{server_url}����ʧ��");
                return;
            }

        }

        /// <summary>
        /// ��������
        /// </summary>
        private void TaskInit()
        {
            shutdownTask = new BaseTask(ShutDown);              //�Զ��ػ�
            shellMonitorTask = new BaseTask(ShellMonitor);      //����״̬���
            commandHandleTask = new BaseTask(CommandHanle);     //ָ�����
            commandGainTask = new BaseTask(CommandGain);        //ָ���ȡ
            connectTask = new BaseTask(CheckConnect);           //���������״̬
            ordinaryPrinterTask = new BaseTask(PrinterMonitor); //��ͨ��ӡ����״̬���
            epsonPrinterTask = new BaseTask(EpsonPrinterMonitor);   //epson��ӡ����״̬���
            middleTask = new BaseTask(MiddleMonitor);           //�м�����
            hisTask = new BaseTask(HISMonitor);                 //HIS���

            StartTask();
        }

        /// <summary>
        /// ��������
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
        /// �ر�����
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
        /// ��ȡ����
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

        #region ��С��������ͼ��
        /// <summary>
        /// ֻ֧�����˫��
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
        /// �Ҽ�����
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
        /// �Ҽ����˳�
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
        /// �����־
        /// </summary>
        private void DeleteLog()
        {
            var DeleteLogDay = ConfigurationManager.AppSettings.Get("DeleteLogDay").ToString();
            int day = DeleteLogDay.ToInt();
            if(day <= 0) 
            {
                //Ĭ����7��
                day = 7;
            }
            DateTime keepTime = DateTime.Now.AddDays(-day);
            #region ɾ����س������־
            _Logger.Info("��س�����־�����ʼ");
            string monitor_file = Path.Combine(Environment.CurrentDirectory, "Logs");
            try
            {
                this.DeleteDir(monitor_file, keepTime);
            }
            catch (Exception ex)
            {
                _Logger.Error($"{monitor_file}·����־ɾ��ʧ��{ex.Message}");
            }
            _Logger.Info("��س�����־�������");
            #endregion

            #region ɾ���������־
            _Logger.Info("������־�����ʼ");
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
                    _Logger.Error($"������־ɾ��ʧ��{ex.Message}");
                }
                _Logger.Info("������־�������");
            }
            #endregion
        }

        /// <summary>
        /// ɾ��·�������е��ļ����ļ���
        /// </summary>
        /// <param name="file"></param>
        /// <param name="keepTime">ɾ�����֮ǰ������</param>
        private void DeleteDir(string file, DateTime keepTime)
        {
            try
            {
                //ȥ���ļ��к����ļ���ֻ������
                //ȥ���ļ��е�ֻ������
                DirectoryInfo fileInfo = new DirectoryInfo(file);
                fileInfo.Attributes = FileAttributes.Normal & FileAttributes.Directory;

                //ȥ���ļ���ֻ������
                File.SetAttributes(file, FileAttributes.Normal);

                //�ж��ļ����Ƿ񻹴���
                if (Directory.Exists(file))
                {
                    foreach (string f in Directory.GetFileSystemEntries(file))
                    {
                        if (File.Exists(f))
                        {
                            FileInfo fl = new FileInfo(f);
                            //��������ļ�ɾ���ļ�
                            if(keepTime > fl.LastWriteTime)
                            {
                                //��ʱ��ɾ��
                                File.Delete(f);
                            }
                        }
                        else
                        {
                            //ѭ���ݹ�ɾ�����ļ���
                            DeleteDir(f, keepTime);
                        }
                    }

                    //ɾ�����ļ���
                    if (!Directory.GetFileSystemEntries(file).Any())
                    {
                        Directory.Delete(file);
                    }
                }

            }
            catch (Exception ex) // �쳣����
            {
                _Logger.Error($"DeleteDir Fail{ex.Message}\r\n{ex.StackTrace}");
            }
        }
        
        /// <summary>
        /// �Զ��ػ�
        /// </summary>
        private void ShutDown()
        {
            if (Global.param != null && Global.param.auto_shutdown_time.IsNotNullOrEmpty() 
                && Global.param.auto_shutdown_time.Length == 4 
                && string.Compare(DateTime.Now.ToString("HHmm"), Global.param.auto_shutdown_time) >= 0)
            {
                Process.Start("shutdown.exe", "-s");//�ػ�
            }

            Thread.Sleep(10000);
        }

        /// <summary>
        /// ���±���ʱ��
        /// </summary>
        private void UpdateLocalTime()
        {
            try
            {
                //�������˵�ʱ�䲢���н���
                //��ʼ֮ǰҪ���ж�һ�·����֮���Ƿ���������
                var time = server.GetRemoteTime(server_url, device_id);
                if (time.IsNotNullOrEmpty() && time.Length == 14)
                {
                    int result = Win32API.SyscServiceTime(time);
                    if (result != 1)
                    {
                        StepDetailsShow($"��ȡ������ʱ��ʧ��");
                        _Logger.Error($"��ȡ������ʱ��ʧ��");
                    }
                }
            }
            catch (Exception ex)
            {
                StepDetailsShow($"��ȡ������ʱ��ʧ�ܣ�{ex.Message}\r\n{ex.StackTrace}");
                _Logger.Error($"��ȡ������ʱ��ʧ�ܣ�{ex.Message}\r\n{ex.StackTrace}");
            }
            
        }

        /// <summary>
        /// ָ�����
        /// </summary>
        private void CommandHanle()
        {
            int? command = eventQueue.Dequeue();
            if (command.HasValue)
            {
                _Logger.Debug($"ָ����������{command}");

                switch (command.Value)
                {
                    case (int)EnumCommand.TurnOff:
                        Process.Start("shutdown.exe", "-s");//�ػ�
                        break;
                    case (int)EnumCommand.Reboot:
                        Process.Start("shutdown.exe", "-r");//����
                        break;
                    case (int)EnumCommand.WriteOff:
                        Process.Start("shutdown.exe", "-l");//ע��
                        break;
                    case (int)EnumCommand.UpdateVersion:
                        UpdateVersion();    //���°汾
                        break;
                    case (int)EnumCommand.ScreenShot:
                        ScreenShot();   //�������ϴ�
                        break;
                    default:
                        break;
                }
            }

            Thread.Sleep(100);
        }

        /// <summary>
        /// ��ȡ����
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
                _Logger.Error($"��ȡָ���쳣��{ex.Message}\r\n{ex.StackTrace}");
            }

            Thread.Sleep(3000);
        }

        /// <summary>
        /// �������״̬
        /// </summary>
        private void CheckConnect()
        {
            try
            {
                ConnectCount++;
                if(!server.IsConnet(server_url, device_id, ConnectCount >= 10 ? 1 : 0))
                {
                    StepDetailsShow("���������쳣");
                    ConnectRetryCount++;

                    if(ConnectRetryCount >= 10 && !this.IsErr)
                    {
                        //_Logger.Info("start CheckConnect");
                        Task.Run(() => { OpenErrWindow("�����쳣", (int)EnumErrorLevel.System); });
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
                _Logger.Error($"��ѯ�����������״̬�쳣��{ex.Message}\r\n{ex.StackTrace}");
            }
            Thread.Sleep(1000);
        }
        #endregion

        #region ���°�����
        /// <summary>
        /// ���°汾
        /// �ڴ��ڼ���Ҫע�������������
        /// </summary>
        private void UpdateVersion()
        {
            if (Global.param != null 
                && (Global.param.download_url.IsNullOrEmpty() 
                || Global.param.update_filePath.IsNullOrEmpty()
                || Global.param.update_fileVersion.IsNullOrEmpty()
                || Global.param.cef_exe_full_path.IsNullOrEmpty()))
            {
                StepDetailsShow($"���ص�ַ���汾�š��������������ַ�Ȳ���Ϊ��");
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

                #region ֹͣ����
                this.StopProcess();
                StepDetailsShow($"���������ֹ���");
                #endregion

                Thread.Sleep(2000); //�ȴ�������ȫ�ر�

                #region ����
                var zip_filename = Path.GetFileNameWithoutExtension(file);
                var zip_extension = Path.GetExtension(file);
                var fileName = $"{zip_filename}{zip_extension}";
                if (!Directory.Exists(cef_dir_path))
                {
                    Directory.CreateDirectory(cef_dir_path);
                }
                var zip_path = Path.Combine(cef_dir_path, fileName);
                StepDetailsShow($"������ʼ����");
                WebRequest request = WebRequest.Create(file);
                WebResponse respone = request.GetResponse();
                var total = respone.ContentLength;
                Stream netStream = respone.GetResponseStream();
                Stream fileStream = new FileStream(zip_path, FileMode.Create);
                byte[] read = new byte[1024];
                long progressBarValue = 0;
                int realReadLen = netStream.Read(read, 0, read.Length);

                BarShow();
                StepDetailsShow($"��ʼ���ظ��°�");
                while (realReadLen > 0)
                {
                    fileStream.Write(read, 0, realReadLen);
                    progressBarValue += realReadLen;
                    BarUpdate(Convert.ToInt32(Math.Floor(progressBarValue * 100.0 / total)));
                    realReadLen = netStream.Read(read, 0, read.Length);
                }
                netStream.Close();
                fileStream.Close();
                StepDetailsShow($"���°��������");
                #endregion

                #region ��ѹ
                ZIP.UnZip(zip_path, cef_dir_path);
                File.Delete(zip_path);
                StepDetailsShow($"���°���ѹ���");
                #endregion

                #region ���ظ��°汾��
                StreamWriter sw = new StreamWriter(Path.Combine(cef_dir_path, VersionFile));
                sw.Write(Global.param.update_fileVersion);
                sw.Close();
                StepDetailsShow($"�汾�Ÿ������");
                #endregion

                #region ��������
                Restart();
                StepDetailsShow($"��������������");
                #endregion

                #region �ش��汾��Ϣ
                if(!server.FeedBackVersion(server_url, device_id, Global.param.update_fileVersion))
                {
                    StepDetailsShow("�ش��汾��ʧ��");
                }
                #endregion

            }
            catch (Exception ex)
            {
                _Logger.Error($"����ʧ�ܣ�{ex.Message}");
                StepDetailsShow($"����ʧ�ܣ�{ex.Message}");
            }
            finally
            {
                shellMonitorTask.Start();
                Global.param.update_filePath = string.Empty;
                Global.param.update_fileVersion = string.Empty;
            }
        }

        /// <summary>
        /// ��������
        /// </summary>
        private void Restart()
        {
            try
            {
                #region ֹͣ����
                this.StopProcess();
                #endregion

                Process process = new Process();
                process.StartInfo.FileName = Global.param.cef_exe_full_path;
                process.Start();

                LastRunTime = DateTime.Now;
            }
            catch (Exception ex)
            {
                StepDetailsShow($"��������ʧ�ܣ�{ex.Message}\r\n{ex.StackTrace}");
                _Logger.Error($"��������ʧ�ܣ�{ex.Message}\r\n{ex.StackTrace}");
            }
        }

        /// <summary>
        /// ֹͣӦ�ó���
        /// </summary>
        /// <param name="exeName"></param>
        private void StopProcess()
        {
            if (Global.param == null)
            {
                _Logger.Error("������ȡʧ�ܣ��޷���������");
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
        /// ����
        /// </summary>
        private void ScreenShot()
        {
            try
            {
                StepDetailsShow("��ʼ����");
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
                    _Logger.Error($"�����ļ��ϴ�ʧ��");
                    StepDetailsShow("�����ļ��ϴ�ʧ��");
                }

                File.Delete(file_path);
                StepDetailsShow("�������");
            }
            catch (Exception ex)
            {
                StepDetailsShow($"�����쳣��{ex.Message} \r\n {ex.StackTrace}");
                _Logger.Error($"�����쳣��{ex.Message} \r\n {ex.StackTrace}");
            }
        }

        /// <summary>
        /// ����HIS����
        /// </summary>
        private void RestartHIS(string filePath)
        {
            try
            {
                #region �������ɱ����
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

                #region ����exe
                Process process = new Process();
                process.StartInfo.FileName = filePath;
                process.Start();
                #endregion
            }
            catch (Exception ex)
            {
                _Logger.Error($"����HISʧ�ܣ�{ex.Message}\r\n{ex.StackTrace}");
            }
        }

        /// <summary>
        /// �����м��
        /// </summary>
        private void RestartMidlle(string service_name)
        {
            try
            {
                //��ֹͣ�ٿ���
                StopWindowsServer(service_name, 10000);
                StartWindowsServer(service_name, 10000);
            }
            catch (Exception ex)
            {
                _Logger.Error($"�����м������{service_name}ʧ�ܣ�{ex.Message}\r\n{ex.StackTrace}");
            }
        }

        /// <summary>
        /// �������������
        /// </summary>
        /// <param name="service_name">��������</param>
        /// <param name="timeout">��ʱʱ��</param>
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
                _Logger.Error($"��������{service_name}ʧ�ܣ�{ex.Message}\r\n{ex.StackTrace}");
            }
        }

        /// <summary>
        /// ֹͣ���������
        /// </summary>
        /// <param name="service_name">��������</param>
        /// <param name="timeout">��ʱʱ��</param>
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
                _Logger.Error($"ֹͣ����{service_name}ʧ�ܣ�{ex.Message}\r\n{ex.StackTrace}");
            }
        }
        #endregion

        #region ���
        /// <summary>
        /// ������
        /// </summary>
        private void ShellMonitor()
        {
            try
            {
                //_Logger.Info("���ڽ������������");
                FileInfo file = new FileInfo(HeartFile);
                //ͬʱУ�������һ�α�������ʱ�䣬����Ƶ������
                if (file.LastWriteTime.AddSeconds(10) < DateTime.Now && LastRunTime.AddSeconds(30) < DateTime.Now)
                {
                    ShellRestartFailCount++;
                    if (ShellRestartFailCount > ShellRetryCount)
                    {
                        if(!this.IsErr && !this.IsShellErr)
                        {
                            //_Logger.Info("start ShellMonitor");
                            Task.Run(() => { OpenErrWindow("��������ʧ��", (int)EnumErrorLevel.Shell); });
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
                _Logger.Error($"��������ʧ�ܣ�{ex.Message}\r\n{ex.StackTrace}");
                StepDetailsShow($"��������ʧ�ܣ�{ex.Message}\r\n{ex.StackTrace}");
            }
            Thread.Sleep(1000);
        }

        /// <summary>
        /// HIS���
        /// ���ʧ����Ҫȥ���Ե�
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
                        CARD_TYPE = "14",   //����-���֤
                        GET_SIGN_INFO = "0",
                        GET_PLAT_INFO = 1,
                    };

                    CloudHelper helper = new CloudHelper();
                    helper.MZ1001($"{Global.param.his_url}", request);
                    HISRetryFailCount = 0;
                }
                catch (Exception ex)
                {
                    _Logger.Error($"HIS�ӿ������쳣��{ex.Message}\r\n{ex.StackTrace}");

                    if (HISRetryFailCount >= HISRetryCount && !this.IsErr)
                    {
                        //_Logger.Info("start HISMonitor");
                        Task.Run(() => { OpenErrWindow("HIS�����쳣������ϵ������Ա", (int)EnumErrorLevel.Third); });
                    }
                    else
                    {
                        //���������м������
                        RestartHIS(Global.param.his_exe_full_path);
                        Thread.Sleep(60000);
                    }
                    
                    HISRetryFailCount++;
                }
                
                
            }
            catch (Exception ex)
            {
                _Logger.Error($"HIS����쳣��{ex.Message}\r\n{ex.StackTrace}");
                throw;
            }
            Thread.Sleep(10000);
        }

        /// <summary>
        /// �м�����
        /// ���������ʽ
        /// ���ʧ����Ҫȥ���Ե�
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
                            Task.Run(() => { OpenErrWindow("�м�������쳣", (int)EnumErrorLevel.Third); });
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
                        //���ü���
                        MidlleConRetryCount = 0;
                        MiddleRetryFailCount = 0;
                    }
                }
            }
            catch (Exception ex)
            {
                _Logger.Error($"�м��״̬����쳣��{ex.Message}\r\n{ex.StackTrace}");
            }

            Thread.Sleep(10000);
        }

        /// <summary>
        /// ��ӡ��״̬���
        /// �ڴ˾����ܴ����epson��ӡ�������
        /// ����м����HIS����ӡ״̬��һ�����쳣���������м��
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
                            //����з�������Ϊ���쳣���
                            Task.Run(() => { OpenErrWindow($"��ӡ��{orinary}�쳣��{err_msg}", (int)EnumErrorLevel.Third); });
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
                _Logger.Error($"��ͨ��ӡ��״̬����쳣��{ex.Message}\r\n{ex.StackTrace}");
            }

            Thread.Sleep(10000);
        }

        /// <summary>
        /// epson��ӡ����״̬���
        /// ����м����HIS����ӡ״̬��һ�����쳣���������м��
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
                        //��� EPSON���ʹ�ӡ��״̬
                        err_msg = EpsonPrinterHelper.GetInfo(epson);
                        if (err_msg.IsNotNullOrEmpty() && !this.IsErr)
                        {
                            //_Logger.Info("start EpsonPrinterMonitor");
                            Task.Run(() => { OpenErrWindow($"��ӡ��{epson}�쳣��{err_msg}", (int)EnumErrorLevel.Third); });
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
                _Logger.Error($"EPSON��ӡ��״̬����쳣��{ex.Message}\r\n{ex.StackTrace}");
            }

            Thread.Sleep(10000);
        }
        #endregion

        #region window
        /// <summary>
        /// ���½���������
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
        /// ���½�����չʾ
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
        /// ��������չʾ
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
        /// ���µ�ǰչʾ�İ汾��
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
        /// ���µ�ǰչʾ���豸����
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
        /// ���쳣����
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
                _Logger.Error($"�쳣����������⣺{ex.Message}\r\n{ex.StackTrace}");
            }
            
        }

        /// <summary>
        /// ����쳣
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
                _Logger.Error($"err init fail��{ex.Message}\r\n{ex.StackTrace}");
            }
            
        }
        #endregion
    }
}