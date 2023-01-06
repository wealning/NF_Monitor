using BaseUtils;
using KLB_Monitor.Model.Global;
using log4net;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KLB_Monitor.Core
{
    /// <summary>
    /// 
    /// </summary>
    public class FileHelper
    {
        private static ILog _Logger = LogManager.GetLogger("FileHelper");

        /// <summary>
        /// 清除日志
        /// </summary>
        public static void DeleteLog()
        {
            var DeleteLogDay = ConfigurationManager.AppSettings.Get("DeleteLogDay").ToString();
            int day = DeleteLogDay.ToInt();
            if (day <= 0) {
                //默认是7天
                day = 7;
            }
            DateTime keepTime = DateTime.Now.AddDays(-day);
            #region 删除监控程序的日志
            _Logger.Info("监控程序日志清除开始");
            string monitor_file = Path.Combine(Environment.CurrentDirectory, "Logs");
            try 
            {
                DeleteDir(monitor_file, keepTime);
            }
            catch (Exception ex) {
                _Logger.Error($"{monitor_file}路径日志删除失败{ex.Message}");
            }
            _Logger.Info("监控程序日志清除结束");
            #endregion

            #region 删除壳体的日志
            _Logger.Info("壳体日志清除开始");
            if ((Global.param?.cef_exe_full_path ?? "").IsNotNullOrEmpty()) {
                try {
                    string filePath = Path.GetDirectoryName(Global.param.cef_exe_full_path);
                    string shell_file = Path.Combine(filePath, "Logs");
                    DeleteDir(shell_file, keepTime);
                }
                catch (Exception ex) {
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
        private static void DeleteDir(string file, DateTime keepTime)
        {
            try {
                //去除文件夹和子文件的只读属性
                //去除文件夹的只读属性
                DirectoryInfo fileInfo = new DirectoryInfo(file);
                fileInfo.Attributes = FileAttributes.Normal & FileAttributes.Directory;

                //去除文件的只读属性
                File.SetAttributes(file, FileAttributes.Normal);

                //判断文件夹是否还存在
                if (Directory.Exists(file)) {
                    foreach (string f in Directory.GetFileSystemEntries(file)) {
                        if (File.Exists(f)) {
                            FileInfo fl = new FileInfo(f);
                            //如果有子文件删除文件
                            if (keepTime > fl.LastWriteTime) {
                                //超时的删除
                                File.Delete(f);
                            }
                        }
                        else {
                            //循环递归删除子文件夹
                            DeleteDir(f, keepTime);
                        }
                    }

                    //删除空文件夹
                    if (!Directory.GetFileSystemEntries(file).Any()) {
                        Directory.Delete(file);
                    }
                }

            }
            catch (Exception ex) // 异常处理
            {
                _Logger.Error($"DeleteDir Fail{ex.Message}\r\n{ex.StackTrace}");
            }
        }
    }
}
