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
        ///  应用程序的入口点
        /// </summary>
        [STAThread]
        static void Main()
        {
            // To customize application configuration such as set high DPI settings or default font,
            // see https://aka.ms/applicationconfiguration.
            //ApplicationConfiguration.Initialize();
            //Application.Run(new Monitor());

            //1. 判断当前程序是否已经运行；如果已经运行则直接退出，不提示
            bool createnew = false;
            Mutex mutex = new Mutex(true, "consoleTest", out createnew);
            if (!createnew)
            {
                return;
            }

            //2. 确保当前是否是以管理员的身份运行的
            WindowsIdentity identity = WindowsIdentity.GetCurrent();
            WindowsPrincipal principal = new WindowsPrincipal(identity);
            if (!principal.IsInRole(WindowsBuiltInRole.Administrator)) {
                //创建启动对象
                ProcessStartInfo startInfo = new ProcessStartInfo();
                startInfo.UseShellExecute = true;
                startInfo.WorkingDirectory = Environment.CurrentDirectory;
                startInfo.FileName = Process.GetCurrentProcess()?.MainModule?.FileName;
                //设置启动动作,确保以管理员身份运行
                startInfo.Verb = "runas";
                try 
                {
                    Process.Start(startInfo);
                }
                catch 
                {
                    return;
                }
                //退出
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
            services.AddScoped<Monitor>();  //其他的方式有什么区别？
            //单例模式（AddSingleton）：注入一次，一直有效
            //作用域模式（AddScoped）：同一个请求中实例是同一个
            //瞬时模式（AddTransient）：

            IConfiguration builder = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("appsetting.json")
                .Build();

            services.AddSingleton<IConfiguration>(builder);
        }

        /// <summary>
        /// 绑定程序中的异常处理
        /// </summary>
        private static void BindExceptionHandler()
        {
            Application.SetUnhandledExceptionMode(UnhandledExceptionMode.CatchException);

            //处理UI线程异常
            Application.ThreadException += new ThreadExceptionEventHandler(Application_ThreadException);
            //处理未捕获的异常
            AppDomain.CurrentDomain.UnhandledException += new System.UnhandledExceptionEventHandler(Application_UnhandledExceptionEventHanlder);

        }

        /// <summary>
        /// 处理未捕获的异常
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private static void Application_UnhandledExceptionEventHanlder(object sender, System.UnhandledExceptionEventArgs e)
        {
        }

        /// <summary>
        /// 处理UI线程异常
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private static void Application_ThreadException(object sender, ThreadExceptionEventArgs e)
        {
        }

    }
}