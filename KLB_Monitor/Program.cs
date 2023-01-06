using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System.Configuration;
using System.Diagnostics;
using System.Security.Principal;

namespace KLB_Monitor
{
    /// <summary>
    /// 
    /// </summary>
    static class Program
    {
        /// <summary>
        ///  Ӧ�ó������ڵ�
        /// </summary>
        [STAThread]
        static void Main()
        {
            // To customize application configuration such as set high DPI settings or default font,
            // see https://aka.ms/applicationconfiguration.
            //ApplicationConfiguration.Initialize();
            //Application.Run(new Monitor());

            //1. �жϵ�ǰ�����Ƿ��Ѿ����У�����Ѿ�������ֱ���˳�������ʾ
            bool createnew = false;
            Mutex mutex = new Mutex(true, "consoleTest", out createnew);
            if (!createnew)
            {
                return;
            }

            //2. ȷ����ǰ�Ƿ����Թ���Ա��������е�
            WindowsIdentity identity = WindowsIdentity.GetCurrent();
            WindowsPrincipal principal = new WindowsPrincipal(identity);
            if (!principal.IsInRole(WindowsBuiltInRole.Administrator)) {
                //������������
                ProcessStartInfo startInfo = new ProcessStartInfo();
                startInfo.UseShellExecute = true;
                startInfo.WorkingDirectory = Environment.CurrentDirectory;
                startInfo.FileName = Process.GetCurrentProcess()?.MainModule?.FileName;
                //������������,ȷ���Թ���Ա�������
                startInfo.Verb = "runas";
                try 
                {
                    Process.Start(startInfo);
                }
                catch 
                {
                    return;
                }
                //�˳�
                Environment.Exit(0);
            }

            BindExceptionHandler();
            log4net.Config.XmlConfigurator.Configure();

            ServiceCollection services = new ServiceCollection();
            ConfigureServices(services);

            var servicePorvider = services.BuildServiceProvider();
            var frm = servicePorvider.GetRequiredService<Monitor>();
            Application.Run(frm);

        }

        private static void ConfigureServices(ServiceCollection services)
        {
            services.AddScoped<Monitor>();  //�����ķ�ʽ��ʲô����
            //����ģʽ��AddSingleton����ע��һ�Σ�һֱ��Ч
            //������ģʽ��AddScoped����ͬһ��������ʵ����ͬһ��
            //˲ʱģʽ��AddTransient����

            IConfiguration builder = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("appsetting.json")
                .Build();

            services.AddSingleton<IConfiguration>(builder);
        }

        /// <summary>
        /// �󶨳����е��쳣����
        /// </summary>
        private static void BindExceptionHandler()
        {
            Application.SetUnhandledExceptionMode(UnhandledExceptionMode.CatchException);

            //����UI�߳��쳣
            Application.ThreadException += new ThreadExceptionEventHandler(Application_ThreadException);
            //����δ������쳣
            AppDomain.CurrentDomain.UnhandledException += new System.UnhandledExceptionEventHandler(Application_UnhandledExceptionEventHanlder);

        }

        /// <summary>
        /// ����δ������쳣
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private static void Application_UnhandledExceptionEventHanlder(object sender, System.UnhandledExceptionEventArgs e)
        {
        }

        /// <summary>
        /// ����UI�߳��쳣
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private static void Application_ThreadException(object sender, ThreadExceptionEventArgs e)
        {
        }

    }
}