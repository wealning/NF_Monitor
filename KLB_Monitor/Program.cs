using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System.Diagnostics;
using System.Security.Principal;

namespace KLB_Monitor
{
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

            //2. 判断当前是否是以管理员的身份运行的
            WindowsIdentity identity = WindowsIdentity.GetCurrent();
            WindowsPrincipal principal = new WindowsPrincipal(identity);
            if (!principal.IsInRole(WindowsBuiltInRole.Administrator))
            {
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

            log4net.Config.XmlConfigurator.Configure();
            //
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);

            //ServiceCollection services = new ServiceCollection();
            //var serviceProvider = services.BuildServiceProvider();
            //var frm = serviceProvider.GetRequiredService<Monitor>();

            //Application.Run(frm);

            Application.Run(new Monitor());

        }
        
    }
}