//Config.cs
using DotNetEnv;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace InboxMonitoringApp
{
    public static class Config
    {
        public static string ClientId { get; private set; }
        public static string ClientSecret { get; private set; }

        static Config()
        {
            Load();
        }

        public static void Load()
        {
            Env.Load(".env");
            ClientId = Environment.GetEnvironmentVariable("CLIENT_ID") ?? "";
            ClientSecret = Environment.GetEnvironmentVariable("CLIENT_SECRET") ?? "";

            if (string.IsNullOrEmpty(ClientId) || string.IsNullOrEmpty(ClientSecret))
            {
                throw new Exception("Environment variables CLIENT_ID or CLIENT_SECRET are not set.");
            }
        }

        public static void Reload()
        {
            Load();
        }
    }
}

