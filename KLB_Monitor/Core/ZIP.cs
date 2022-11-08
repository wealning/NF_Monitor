using BaseUtils;
using ICSharpCode.SharpZipLib.Zip;
using log4net;
using log4net.Repository.Hierarchy;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KLB_Monitor.Core
{
    public static class ZIP
    {
        private static ILog _Logger = LogManager.GetLogger("ZIP");

        /// <summary>
        /// 解压
        /// </summary>
        /// <param name="zip_filename"></param>
        public static void UnZip(string zip_filename, string ShellExePath)
        {
            string unzip_path = ShellExePath;
            if (unzip_path[unzip_path.Length - 1] != '/')
            {
                unzip_path += "\\";
            }

            //  生成解压目录
            if (!Directory.Exists(unzip_path))
            {
                Directory.CreateDirectory(unzip_path);
            }

            using (ZipInputStream s = new ZipInputStream(File.OpenRead(zip_filename)))
            {
                ZipEntry theEntry;

                while ((theEntry = s.GetNextEntry()) != null)
                {
                    string file_name = System.IO.Path.GetFileName(theEntry.Name);
                    string path = System.IO.Path.GetDirectoryName(theEntry.Name) ?? "";
                    var write_file = Path.Combine(unzip_path, path, file_name);
                    DateTime dt = theEntry.DateTime;

                    //  生成解压目录
                    if (!Directory.Exists(Path.Combine(unzip_path, path)))
                    {
                        Directory.CreateDirectory(Path.Combine(unzip_path, path));
                    }
                    

                    if (file_name.IsNotNullOrEmpty())
                    {
                        try
                        {
                            //解压文件到指定的目录
                            FileStream streamWriter = File.Create(write_file);
                            int size = 2048;
                            byte[] data = new byte[2048];
                            while (true)
                            {
                                size = s.Read(data, 0, data.Length);
                                if (size > 0)
                                {
                                    streamWriter.Write(data, 0, size);
                                }
                                else
                                {
                                    break;
                                }
                            }
                            streamWriter.Close();

                            File.SetLastWriteTime(write_file, dt);
                            File.SetCreationTime(write_file, dt);
                        }
                        catch (Exception ex)
                        {
                            _Logger.Error($"解压失败：{ex.Message}\r\n{ex.StackTrace}");
                            continue;
                        }
                    }
                }
                s.Close();
            }
                
        }
    }
}
