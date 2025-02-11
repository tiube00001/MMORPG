using System;
using System.IO;
using System.Reflection;

namespace GameServer.Tool
{
    public static class ResourceHelper
    {
        public static string LoadFile(string path)
        {
            var allPath = path;
            if (!Path.IsPathRooted(path))
            {
                string exePath = Assembly.GetExecutingAssembly().Location;
                string? exeDirectory = Path.GetDirectoryName(exePath);
                allPath = Path.Join(exeDirectory, path);
            }
            string content = File.ReadAllText(allPath);
            return content;
        }
    }
}
