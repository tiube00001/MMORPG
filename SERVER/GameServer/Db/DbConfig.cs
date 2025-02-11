using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GameServer.Db
{
    public class DbConfig
    {
        /// <summary>
        /// 数据库Host
        /// </summary>
        public  string Host = "";
        /// <summary>
        /// 数据库端口
        /// </summary>
        public  int Port = 0;
        /// <summary>
        /// 数据库用户名
        /// </summary>
        public  string User = "";
        /// <summary>
        /// 数据库密码
        /// </summary>
        public  string Password = "";
        /// <summary>
        /// 数据库名称
        /// </summary>
        public  string DbName = "";

        public bool IsValid()
        {
            return !string.IsNullOrEmpty(Host) && !string.IsNullOrEmpty(User) && !string.IsNullOrEmpty(Password) && !string.IsNullOrEmpty(DbName);
        }
    }
}
