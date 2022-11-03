using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System.Diagnostics;
using System.Security.Principal;

namespace KLB_Monitor
{
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

            //2. �жϵ�ǰ�Ƿ����Թ���Ա��������е�
            WindowsIdentity identity = WindowsIdentity.GetCurrent();
            WindowsPrincipal principal = new WindowsPrincipal(identity);
            if (!principal.IsInRole(WindowsBuiltInRole.Administrator))
            {
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