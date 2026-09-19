using Microsoft.Extensions.Configuration;

namespace MySite.Web
{
    /// <summary>读取应用程序的配置项。</summary>
    public static class AppSettingsReader
    {
        private static IConfiguration _configuration;
        // 静态构造函数  
        /// <summary>初始化应用设置。</summary>
        static AppSettingsReader()
        {
            // 这里可以通过依赖注入获取 IConfiguration 实例  
            // 但通常在静态类中不推荐直接使用 DI  
        }

        // 设置 IConfiguration 实例  
        /// <summary>设置应用配置来源。</summary>
        public static void SetConfiguration(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        // 读取配置的方法  
        /// <summary>读取指定应用设置。</summary>
        public static string GetSetting(string key)
        {
            return _configuration[key];
        }
    }
}
