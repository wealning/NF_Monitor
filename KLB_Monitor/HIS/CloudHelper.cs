using KLB_Monitor.Core;
using KLB_Monitor.HIS.Model;
using log4net;
using log4net.Repository.Hierarchy;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KLB_Monitor.HIS
{
    /// <summary>
    /// 云HIS
    /// </summary>
    public class CloudHelper
    {
        private ILog _Logger = LogManager.GetLogger("CloudHelper");

        /// <summary>
        /// 获取病人信息
        /// </summary>
        /// <param name="hisurl"></param>
        /// <param name="MZ1001"></param>
        /// <returns></returns>
        /// <exception cref="Exception"></exception>
        public MZ1001Out MZ1001(string hisurl, MZ1001 model)
        {
            MZ1001Request request = new MZ1001Request()
            {
                inpara = model,
            };

            MZ1001Response MZ1001rsp = new MZ1001Response();

            try
            {
                string url = $"{hisurl}/out/trans";
                MZ1001rsp = Trans(request, url);
                if (MZ1001rsp.ret_code != "0")
                {
                    throw new Exception(MZ1001rsp.ret_info);
                }
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
            return MZ1001rsp.data;
        }

        /// <summary>
        /// 交易
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="request"></param>
        /// <param name="hishttpurl"></param>
        /// <param name="timeout"></param>
        /// <returns></returns>
        private T Trans<T>(BaseRequest<T> request, string hishttpurl, int timeout = 0) where T : BaseResponse
        {
            Dictionary<string, object> dic = new Dictionary<string, object>();
            dic.Add("param", request.GetJson());
            _Logger.Debug($"HIS入参：{request.GetJson()}|{timeout}|{hishttpurl}");
            string result = string.Empty;

            var post_way = ConfigurationManager.AppSettings.Get("HIS:CloudPostWay") ?? "0";

            switch (post_way)
            {
                case "1":
                    {
                        result = HttpUtil.Post(hishttpurl, request.GetJson(), "application/json");
                        break;
                    }
                case "2":
                    {
                        result = HttpUtil.Post(hishttpurl, dic, timeout);
                        break;
                    }
                case "0":
                default:
                    {
                        result = HttpUtil.Post(hishttpurl, dic, true, timeout);
                        break;
                    }
            }

            
            _Logger.Debug($"HIS出参：{result}");
            

            T rsp = null;
            try
            {
                rsp = JsonConvert.DeserializeObject<T>(result);
            }
            catch (Exception e)
            {
                _Logger.Error("HIS json转换失败:" + e.Message);
            }

            return rsp;
        }
    }
}
