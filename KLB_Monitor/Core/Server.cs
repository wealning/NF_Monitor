using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BaseUtils;
using KLB_Monitor.Model.Enum;
using KLB_Monitor.Model.Global;
using KLB_Monitor.Model.request;
using KLB_Monitor.Model.response;
using log4net;
using log4net.Repository.Hierarchy;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace KLB_Monitor.Core
{
    public class Server
    {
        private ILog _Logger = LogManager.GetLogger("Server");

        public Server()
        {

        }

        /// <summary>
        /// 服务端连接状态
        /// </summary>
        /// <returns></returns>
        public bool IsConnet(string url, string device_id, int status = 0)
        {
            bool IsConnect = false;
            var request = new connect_req
            {
                device_id = device_id,
                is_record = status,
            };

            try
            {
                var result = HttpUtil.Post($"{url}/third/updateHeartTime", JsonConvert.SerializeObject(request));
                if (status.Equals(1))
                {
                    _Logger.Debug($"检查连接状态 请求 status:1");
                }
                var data = JsonConvert.DeserializeObject<BaseRsp<string>>(result);
                //_Logger.Debug($"检查连接状态 返回：{result}");
                if (data != null && data.code == (int)EnumCommunicationStatus.Success)
                {
                    IsConnect = true;
                }
                else
                {
                    _Logger.Debug($"检查连接状态 返回：{result}");
                    _Logger.Error($"连接失败：{data?.code} - {data?.msg}");
                }
            }
            catch (Exception ex)
            {
                _Logger.Error($"服务端连接异常：{ex.Message}\r\n{ex.StackTrace}");
            }

            Global.IsConnect = IsConnect;
            return IsConnect;
        }


        /// <summary>
        /// 获取参数
        /// </summary>
        /// <param name="url"></param>
        /// <param name="device_id"></param>
        public string GetParameter(string url,string device_id)
        {
            if (!Global.IsConnect)
            {
                return "获取参数失败，服务端连接异常";
            }

            try
            {
                var result = HttpUtil.Get($"{url}/third/getMonitorPara?device_id={device_id.ToLong()}");
                _Logger.Debug($"获取设备参数 返回：{result}");
                var data = JsonConvert.DeserializeObject<BaseRsp<param_rsp>>(result);

                if(data != null && data.code == (int)EnumCommunicationStatus.Success)
                {
                    var para = data.data;
                    Global.param = new Param
                    {
                        device_name = para?.device_name ?? "",
                        version = para?.version ?? "",
                        download_url = para?.download_url ?? "",
                        auto_shutdown_time = para?.auto_shutdown_time ?? "",
                        cef_exe_full_path = para?.cef_exe_full_path ?? "",
                        his_exe_full_path = para?.his_exe_full_path ?? "",
                        printeNameList = new List<string>(),
                        middle_url = para?.middle_url ?? "",
                    };
                    //打印机名称拆分
                    if ((para?.printer_name ?? "").IsNotNullOrEmpty())
                    {
                        string[] strArr = para.printer_name.Split(',');
                        foreach (var item in strArr)
                        {
                            if (item.Trim().IsNotNullOrEmpty())
                            {
                                Global.param.printeNameList.Add(item.Trim());
                            }
                        }
                    }
                    //
                    if(Global.param.middle_url.IsNotNullOrEmpty()
                        && (Global.param.middle_url.EndsWith("/") || Global.param.middle_url.EndsWith("\\")))
                    {
                        Global.param.middle_url = Global.param.middle_url.Substring(0, Global.param.middle_url.Length - 1);
                    }

                }
                else
                {
                    Global.param = new Param();
                    _Logger.Error($"获取参数失败：{data?.code} - {data?.msg}");
                    return $"获取参数失败：{data?.code} - {data?.msg}";
                }
            }
            catch (Exception ex)
            {
                Global.param = new Param();
                _Logger.Error($"获取参数失败：{ex.Message}\r\n{ex.StackTrace}");
                return $"获取参数失败：{ex.Message}\r\n{ex.StackTrace}";
            }

            return "";
        }

        /// <summary>
        /// 回传版本号
        /// </summary>
        /// <param name="url"></param>
        /// <param name="device_id"></param>
        /// <param name="version"></param>
        /// <returns></returns>
        public bool FeedBackVersion(string url, string device_id, string version)
        {
            if (!Global.IsConnect)
            {
                return false;
            }

            try
            {
                var request = new feedback_req
                {
                    device_id = device_id,
                    version = version,
                };
                _Logger.Debug($"回传版本号 请求{JsonConvert.SerializeObject(request)}");
                var result = HttpUtil.Post($"{url}/third/updateVersion", JsonConvert.SerializeObject(request));
                _Logger.Debug($"回传版本号 返回：{result}");
                var data = JsonConvert.DeserializeObject<BaseRsp<string>>(result);

                if (data != null && data.code == (int)EnumCommunicationStatus.Success)
                {
                    return true;
                }
                else
                {
                    _Logger.Error($"回传版本号失败：{data?.code} - {data?.msg}");
                }
            }
            catch (Exception ex)
            {
                _Logger.Error($"回传版本号失败：{ex.Message}\r\n{ex.StackTrace}");
            }

            return false;
        }

        /// <summary>
        /// 获取指令
        /// </summary>
        /// <param name="url"></param>
        /// <param name="device_id"></param>
        public int GetCommand(string url, string device_id)
        {
            if (!Global.IsConnect)
            {
                return -1;
            }

            try
            {
                var result = HttpUtil.Get($"{url}/third/getCmd?device_id={device_id.ToLong()}");
                _Logger.Debug($"获取操作指令 返回：{result}");
                var data = JsonConvert.DeserializeObject<BaseRsp<command_rsp>>(result);

                if (data != null && data.code == (int)EnumCommunicationStatus.Success)
                {
                    if (data.data != null && data.data.fileName.IsNotNullOrEmpty())
                    {
                        Global.param.update_filePath = data.data?.filePath ?? "";
                        Global.param.update_fileVersion = data.data?.fileName ?? "";
                        //Global.param.update_version = Global.param.update_version.Replace("/", "\\");
                        if (Global.param.update_filePath.StartsWith("/"))
                        {
                            Global.param.update_filePath = Global.param.update_filePath.Substring(1, Global.param.update_filePath.Length - 1);
                        }

                    }

                    return (data?.data?.code ?? "").ToInt();
                }
                else
                {
                    _Logger.Error($"指令获取失败：{data?.code} - {data?.msg}");
                }
            }
            catch (Exception ex)
            {
                _Logger.Error($"获取指令失败：{ex.Message}\r\n{ex.StackTrace}");
            }

            return -1;
        }

        /// <summary>
        /// 获取服务端的时间
        /// </summary>
        /// <param name="url"></param>
        /// <param name="device_id"></param>
        /// <returns></returns>
        public string GetRemoteTime(string url, string device_id)
        {
            if (!Global.IsConnect)
            {
                return "";
            }

            try
            {
                _Logger.Debug($"{url}/third/getTime?device_id={device_id.ToLong()} 请求");
                var result = HttpUtil.Get($"{url}/third/getTime?device_id={device_id.ToLong()}");
                _Logger.Debug($"{url}/third/getTime?device_id={device_id.ToLong()} 返回：{result}");
                var data = JsonConvert.DeserializeObject<BaseRsp<time_rsp>>(result);

                if (data != null && data.code == (int)EnumCommunicationStatus.Success)
                {
                    return data.data.time;
                }
                else
                {
                    _Logger.Error($"获取服务端时间失败：{data?.code} - {data?.msg}");
                }
            }
            catch (Exception ex)
            {
                _Logger.Error($"获取服务端时间失败：{ex.Message}\r\n{ex.StackTrace}");
            }
            return "";
        }

        /// <summary>
        /// 文件上传
        /// </summary>
        /// <param name="url"></param>
        /// <param name="device_id"></param>
        /// <param name="base64"></param>
        /// <returns></returns>
        public bool UploadFile(string url, string device_id, string base64)
        {
            if (!Global.IsConnect)
            {
                return false;
            }

            try
            {
                var request = new upload_file_req
                {
                    device_id = device_id,
                    base64Str = base64,
                    fileFormat=".png",  //必须带"."
                };
                _Logger.Debug($"{url}/third/uploadFile 请求");
                var result = HttpUtil.Post($"{url}/third/uploadFile", JsonConvert.SerializeObject(request));
                _Logger.Debug($"{url}/third/uploadFile 返回：{result}");
                var data = JsonConvert.DeserializeObject<BaseRsp<string>>(result);

                if (data != null && data.code == (int)EnumCommunicationStatus.Success)
                {
                    return true;
                }
                else
                {
                    _Logger.Error($"文件上传失败：{data?.code} - {data?.msg}");
                }
            }
            catch (Exception ex)
            {
                _Logger.Error($"文件上传失败：{ex.Message}\r\n{ex.StackTrace}");
            }

            return false;
        }

        /// <summary>
        /// 校验中间件是否在线
        /// </summary>
        public bool CheckMiddleOnline()
        {
            bool isonline = false;

            try
            {
                var result = HttpUtil.Get($"{Global.param.middle_url}/common/systime");
                _Logger.Debug($"中间件监控请求：{Global.param.middle_url}/common/systime");

                var data = JsonConvert.DeserializeObject<BaseMiddleRsp<DateTime>>(result);
                if (data.success.Value && data.data != DateTime.MinValue)
                {
                    isonline = true;
                }
            }
            catch (Exception ex)
            {
                _Logger.Error($"校验中间件是否在线失败：{ex.Message}\r\n{ex.StackTrace}");
            }

            return isonline;
        }

    }
}
