using BaseUtils;
using Microsoft.Extensions.Configuration;
using StackExchange.Redis;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KLB_Monitor.Core
{
    /// <summary>
    /// 
    /// </summary>
    public class RedisHelper
    {
        private static IDatabase db { get; set; }
        public RedisHelper() 
        {
            //获取链接地址
            IConfiguration configuration = new ConfigurationBuilder()
                .SetBasePath(Environment.CurrentDirectory)
                .AddJsonFile("appsetting.json", true, true)
                .AddInMemoryCollection()
                .Build();
            string connectStr = configuration["RedisConfig:ConnectionString"] ?? "";

            string dataBase = connectStr.Split("database")[0];
            ConnectionMultiplexer connection = ConnectionMultiplexer.Connect(dataBase.Substring(0, dataBase.Length - 1));
            db = connection.GetDatabase(connectStr.Split("database")[1].Split('=')[1].ToInt());//指定数据库
        }

        /// <summary>
        /// 赋值
        /// </summary>
        /// <param name="key"></param>
        /// <param name="value"></param>
        /// <returns></returns>
        public bool SetValue(string key, string value)
        {
            return db.StringSet(key, value);
        }

        /// <summary>
        /// 获取
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        public string GetValue(string key)
        {
            return db.StringGet(key);
        }

        /// <summary>
        /// 删除
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        public bool RemoveValue(string key)
        {
            return db.KeyDelete(key);
        }
    }
}
