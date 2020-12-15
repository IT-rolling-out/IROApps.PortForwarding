using System;
using Microsoft.Extensions.Configuration;

namespace IROApps.PortForwarding
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