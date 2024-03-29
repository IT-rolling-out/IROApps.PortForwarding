using System;
using Microsoft.Extensions.Configuration;

namespace IRO.PortForwarding.ServerApp
{
    public static class AppSettings
    {
        static IConfiguration Configuration { get; set; }

        public static readonly TimeSpan RequestExpireTime = TimeSpan.FromMinutes(5);

        public static string ADMIN_KEY => Configuration["ADMIN_KEY"];

        public static void Init(IConfiguration configuration)
        {
            Configuration = configuration;
        }
    }
}