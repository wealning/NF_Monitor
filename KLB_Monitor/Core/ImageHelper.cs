using log4net;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KLB_Monitor.Core
{
    /// <summary>
    /// 图片帮助类
    /// </summary>
    public class ImageHelper
    {
        private ILog _Logger = LogManager.GetLogger("Image");

        /// <summary>
        /// 将图片转换为base64串
        /// </summary>
        /// <param name="file"></param>
        /// <returns></returns>
        public string ConvertImageToBase64(string imagePath)
        {
            try
            {
                
                byte[] bytes = new byte[8];
                using (Bitmap bmp = new Bitmap(imagePath))
                {
                    MemoryStream ms = new MemoryStream();
                    bmp.Save(ms, System.Drawing.Imaging.ImageFormat.Png);
                    bytes = new byte[ms.Length];
                    ms.Position = 0;
                    ms.Read(bytes, 0, (int)ms.Length);
                    ms.Close();
                }
                
                return Convert.ToBase64String(bytes);
            }
            catch (Exception ex)
            {
                _Logger.Error($"ConvertImageToBase64:{ex.Message}\r\n{ex.StackTrace}");
            }
            return "";
        }
    }
}
