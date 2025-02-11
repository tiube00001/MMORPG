using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;
using Serilog;

namespace GameServer.Db
{

    public class SqlDb
    {
        private static DbConfig _dbConfig = new DbConfig();
        private static IFreeSql freeSql;

        public static IFreeSql FreeSql()
        {
            return freeSql;
        }

        public static void InitFreeSql(DbConfig dc)
        {
            _dbConfig = dc;

            Console.WriteLine("test");
            Console.WriteLine(_dbConfig.Host);

            freeSql = new FreeSql.FreeSqlBuilder()
                .UseConnectionString(global::FreeSql.DataType.MySql,
                    $"Data Source={_dbConfig.Host};Port={_dbConfig.Port};User Id={_dbConfig.User};Password={_dbConfig.Password};")
                .UseAutoSyncStructure(true)
                .Build();

            // 检查数据库是否存在
            var exists =
                freeSql.Ado.QuerySingle<int>(
                    $"SELECT COUNT(*) FROM information_schema.schemata WHERE schema_name = '{_dbConfig.DbName}'") > 0;
            if (!exists)
            {
                freeSql.Ado.ExecuteNonQuery($"CREATE DATABASE {_dbConfig.DbName}");
                Log.Information($"数据库“{_dbConfig.DbName}”不存在，已自动创建");
            }

            // 重新链接
            freeSql = new FreeSql.FreeSqlBuilder()
                .UseConnectionString(global::FreeSql.DataType.MySql,
                    $"Data Source={_dbConfig.Host};Port={_dbConfig.Port};User Id={_dbConfig.User};Password={_dbConfig.Password};" +
                    $"Initial Catalog={_dbConfig.DbName};Charset=utf8;SslMode=none;Max pool size=10")
                .UseAutoSyncStructure(true)
                .Build();

            exists = freeSql.DbFirst.GetTablesByDatabase(_dbConfig.DbName).Exists(t => t.Name == "user");

            if (!exists)
            {
                // FreeSql.CodeFirst.SyncStructure<DbUser>();
                freeSql.Insert(new DbUser("root", "1234567890", Authoritys.Administrator)).ExecuteAffrows();
                Log.Information($"数据库“{_dbConfig.DbName}”中的“user”表不存在，已自动创建，并添加一个管理员账号（账号=root，密码=1234567890）");
            }
        }
    }
}
