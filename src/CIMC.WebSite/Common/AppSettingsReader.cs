using Microsoft.Extensions.Configuration;

namespace MySite.Web
{
    public static class AppSettingsReader
    {
        private static IConfiguration _configuration;
        // 静态构造函数  
        static AppSettingsReader()
        {
            // 这里可以通过依赖注入获取 IConfiguration 实例  
            // 但通常在静态类中不推荐直接使用 DI  
        }

        // 设置 IConfiguration 实例  
        public static void SetConfiguration(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        // 读取配置的方法  
        public static string GetSetting(string key)
        {
            return _configuration[key];
        }
    }
}
