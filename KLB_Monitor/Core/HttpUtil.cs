using BaseUtils;
using log4net;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Security;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading.Tasks;
using System.Web;

namespace KLB_Monitor.Core
{

    public static class HttpUtil
    {

        private static bool CheckValidationResult(object sender, X509Certificate certificate, X509Chain chain, SslPolicyErrors errors)
        {
            return true;
        }

        public static string Post(Uri url, string json, string contentType = "text/xml")
        {
            WebRequest webRequest = WebRequest.Create(url);
            byte[] bytes = Encoding.UTF8.GetBytes(json);
            webRequest.Method = "POST";
            webRequest.ContentType = contentType;
            webRequest.ContentLength = bytes.Length;
            using (Stream stream = webRequest.GetRequestStream())
            {
                stream.Write(bytes, 0, bytes.Length);
                stream.Close();
            }

            string result = "";
            using (HttpWebResponse httpWebResponse = (HttpWebResponse)webRequest.GetResponse())
            {
                using StreamReader streamReader = new StreamReader(httpWebResponse.GetResponseStream(), Encoding.UTF8);
                result = streamReader.ReadToEnd();
            }

            return result;
        }

        public static string Post(string url, string json, string contentType = "text/xml", int timeout = 5000)
        {
            WebRequest webRequest = WebRequest.Create(url);
            byte[] bytes = Encoding.UTF8.GetBytes(json);
            webRequest.Method = "POST";
            webRequest.ContentType = contentType;
            webRequest.ContentLength = bytes.Length;
            webRequest.Timeout = timeout;
            using (Stream stream = webRequest.GetRequestStream())
            {
                stream.Write(bytes, 0, bytes.Length);
                stream.Close();
            }

            string result = "";
            using (HttpWebResponse httpWebResponse = (HttpWebResponse)webRequest.GetResponse())
            {
                using StreamReader streamReader = new StreamReader(httpWebResponse.GetResponseStream(), Encoding.UTF8);
                result = streamReader.ReadToEnd();
            }

            return result;
        }

        /// <summary>
        /// 需要重写
        /// </summary>
        /// <param name="url"></param>
        /// <param name="param"></param>
        /// <param name="timeout"></param>
        /// <returns></returns>
        public static string Post(string url, Dictionary<string, object> param, int timeout = 0)
        {
            var mfdc = new System.Net.Http.MultipartFormDataContent();
            mfdc.Headers.Add("ContentType", "multipart/form-data");
            foreach (var model in param)
            {
                mfdc.Add(new StringContent(model.Value.ToString()), model.Key);
            }

            string resultStr = string.Empty;
            var client = new HttpClient().PostAsync(url, mfdc);
            client.Wait();

            if (client.Result.IsSuccessStatusCode)
            {
                var result = client.Result.Content.ReadAsStringAsync();
                result.Wait();
                resultStr = result.Result;
            }
            else
            {

            }
            return resultStr;
        }

        public static string Post(string url, Dictionary<string, object> header, string json, string contentType = "")
        {
            WebRequest webRequest = WebRequest.Create(url);
            foreach (string item in header.Keys.ToList())
            {
                webRequest.Headers.Set(item, header[item].ToString());
            }

            byte[] bytes = Encoding.UTF8.GetBytes(json);
            webRequest.Method = "POST";
            if (!contentType.IsNotNullOrEmpty())
            {
                webRequest.ContentType = contentType;
            }
            else
            {
                webRequest.ContentType = "text/xml;charset=UTF-8";
            }

            webRequest.ContentLength = bytes.Length;
            using (Stream stream = webRequest.GetRequestStream())
            {
                stream.Write(bytes, 0, bytes.Length);
                stream.Close();
            }

            string result = "";
            using (HttpWebResponse httpWebResponse = (HttpWebResponse)webRequest.GetResponse())
            {
                using StreamReader streamReader = new StreamReader(httpWebResponse.GetResponseStream(), Encoding.UTF8);
                result = streamReader.ReadToEnd();
            }

            return result;
        }

        public static string Post(string url, Dictionary<string, object> header, Dictionary<string, object> body)
        {
            string result = "";
            HttpWebRequest httpWebRequest = (HttpWebRequest)WebRequest.Create(url);
            foreach (string item in header.Keys.ToList())
            {
                httpWebRequest.Headers.Set(item, header[item].ToString());
            }

            httpWebRequest.ContentType = "application/x-www-form-urlencoded";
            httpWebRequest.Method = "POST";
            StringBuilder stringBuilder = new StringBuilder();
            int num = 0;
            foreach (string item2 in body.Keys.ToList())
            {
                if (num > 0)
                {
                    stringBuilder.Append("&");
                }

                stringBuilder.AppendFormat("{0}={1}", item2, body[item2].ToString());
                num++;
            }

            byte[] bytes = Encoding.UTF8.GetBytes(stringBuilder.ToString());
            httpWebRequest.ContentLength = bytes.Length;
            using (Stream stream = httpWebRequest.GetRequestStream())
            {
                stream.Write(bytes, 0, bytes.Length);
                stream.Close();
            }

            HttpWebResponse httpWebResponse = (HttpWebResponse)httpWebRequest.GetResponse();
            Stream responseStream = httpWebResponse.GetResponseStream();
            using (StreamReader streamReader = new StreamReader(responseStream, Encoding.UTF8))
            {
                result = streamReader.ReadToEnd();
            }

            return result;
        }

        public static string Post(string url, Dictionary<string, object> param, bool urlencode = false, int timeout = 0)
        {
            string result = "";
            string text = "";
            foreach (string item in param.Keys.ToList())
            {
                text = ((!urlencode) ? ((text.IsNotNullOrEmpty()) ? (text + "&" + item + "=" + param[item].ToString()) : (text + item + "=" + param[item].ToString())) : ((!text.IsNullOrEmpty()) ? (text + "&" + item + "=" + HttpUtility.UrlEncode(param[item].ToString())) : (text + item + "=" + HttpUtility.UrlEncode(param[item].ToString()))));
            }

            HttpWebRequest httpWebRequest = null;
            if (url.StartsWith("https", StringComparison.OrdinalIgnoreCase))
            {
                ServicePointManager.ServerCertificateValidationCallback = CheckValidationResult;
                httpWebRequest = WebRequest.Create(url) as HttpWebRequest;
            }
            else
            {
                httpWebRequest = (HttpWebRequest)WebRequest.Create(url);
            }

            byte[] bytes = Encoding.UTF8.GetBytes(text);
            httpWebRequest.Method = "POST";
            httpWebRequest.ContentType = "application/x-www-form-urlencoded";
            httpWebRequest.ContentLength = bytes.Length;
            if (timeout != 0)
            {
                httpWebRequest.Timeout = timeout;
            }

            using (Stream stream = httpWebRequest.GetRequestStream())
            {
                stream.Write(bytes, 0, bytes.Length);
                stream.Close();
            }

            using (HttpWebResponse httpWebResponse = (HttpWebResponse)httpWebRequest.GetResponse())
            {
                using StreamReader streamReader = new StreamReader(httpWebResponse.GetResponseStream(), Encoding.UTF8);
                result = streamReader.ReadToEnd();
            }

            return result;
        }

        public static string Get(string url, Dictionary<string, object> param = null)
        {
            string result = "";
            if (param != null && param.Count > 0)
            {
                string text = "";
                foreach (string item in param.Keys.ToList())
                {
                    text = ((!text.IsNullOrEmpty()) ? (text + "&" + item + "=" + param[item]) : (text + item + "=" + param[item]));
                }

                url = url + "?" + text;
            }

            HttpWebRequest httpWebRequest = WebRequest.Create(url) as HttpWebRequest;
            httpWebRequest.Method = "GET";
            using (HttpWebResponse httpWebResponse = (HttpWebResponse)httpWebRequest.GetResponse())
            {
                using StreamReader streamReader = new StreamReader(httpWebResponse.GetResponseStream(), Encoding.UTF8);
                result = streamReader.ReadToEnd();
            }

            return result;
        }

        public static string Get(string baseUrl, string nextUrl)
        {
            string result = "";

            var url = new Uri(new Uri(baseUrl), nextUrl);
            HttpWebRequest httpWebRequest = WebRequest.Create(url) as HttpWebRequest;
            httpWebRequest.Method = "GET";
            using (HttpWebResponse httpWebResponse = (HttpWebResponse)httpWebRequest.GetResponse()) {
                using StreamReader streamReader = new StreamReader(httpWebResponse.GetResponseStream(), Encoding.UTF8);
                result = streamReader.ReadToEnd();
            }

            return result;
        }

        public static string Get(string url, Dictionary<string, object> header, Dictionary<string, object> param, int timeout, ref string statusCode)
        {
            string result = "";
            if (param != null && param.Count > 0)
            {
                string text = "";
                foreach (string item in param.Keys.ToList())
                {
                    text = ((!text.IsNullOrEmpty()) ? (text + "&" + item + "=" + param[item]) : (text + item + "=" + param[item]));
                }

                url = url + "?" + text;
            }

            HttpWebRequest httpWebRequest = WebRequest.Create(url) as HttpWebRequest;
            httpWebRequest.Method = "GET";
            httpWebRequest.ContentType = "text/html;charset=UTF-8";
            httpWebRequest.Timeout = timeout;
            foreach (string item2 in header.Keys.ToList())
            {
                httpWebRequest.Headers.Set(item2, header[item2].ToString());
            }

            using (HttpWebResponse httpWebResponse = (HttpWebResponse)httpWebRequest.GetResponse())
            {
                statusCode = httpWebResponse.StatusCode.ToString();
                using StreamReader streamReader = new StreamReader(httpWebResponse.GetResponseStream(), Encoding.UTF8);
                result = streamReader.ReadToEnd();
            }

            return result;
        }
    }

    //public class HttpClientUtil
    //{
    //    public static async Task<string> Get(string baseUrl, string nextUri)
    //    {
    //        var client = new HttpClient();
    //        client.BaseAddress = new Uri(baseUrl);
    //        await client.GetAsync(nextUri);
    //    }
    //}
}
