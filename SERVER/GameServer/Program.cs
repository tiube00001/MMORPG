using System.Diagnostics;
using GameServer.Db;
using MMORPG.Common.Network;
using MMORPG.Common.Proto;
using GameServer.Manager;
using GameServer.Network;
using GameServer.NetService;
using GameServer.Tool;
using Newtonsoft.Json;
using Serilog;
using Serilog.Events;

namespace GameServer
{
    public class StartupCfg
    {
        public DbConfig DbConfig = new DbConfig();
        public string DataPath = "";

    }
    internal class Program
    {
        static async Task Main(string[] args)
        {
            
            string cfgPath = "";
            for (int i = 0; i < args.Length; i++)
            {
                if (args[i] == "--cfg" && i + 1 < args.Length)
                {
                    cfgPath = args[i + 1];
                }
            }

            if (cfgPath == "")
            {
                Console.WriteLine("--cfg is required");
                return;
            }

            string content = ResourceHelper.LoadFile(cfgPath);
            Debug.Assert(content != null);
            var sc = JsonConvert.DeserializeObject<StartupCfg>(content);
            Debug.Assert(sc != null);
            
            if (!sc.DbConfig.IsValid())
            {
                Console.WriteLine("配置文件中数据库配置不完整");
                return;
            }
            
            Console.WriteLine(sc.DbConfig.Host);
            Console.WriteLine(sc.DbConfig.Port);
            Console.WriteLine(sc.DbConfig.User);
            Console.WriteLine(sc.DbConfig.Password);
            SqlDb.InitFreeSql(sc.DbConfig);
            
            DataManager.Instance.SetDirPath(sc.DataPath);
            
            Log.Logger = new LoggerConfiguration()
                .MinimumLevel.Debug()
                .WriteTo.Async(a => a.Console())
                .WriteTo.Async(a => a.File("Logs/log-.txt", rollingInterval: RollingInterval.Day))
                .CreateLogger();


            //var character = new DbCharacter("jj", 1, 1, 1, 1, 1, 1, 1, 1);
            //SqlDb.Connection.Insert(character).ExecuteAffrows();
            
            GameServer server = new(NetConfig.ServerPort);
            await server.Run();
        }

    }
}