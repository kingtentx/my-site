//using CIMC.Helper;
//using Microsoft.AspNetCore.Mvc;
//using Microsoft.Extensions.Configuration;

//namespace CimcSite.Web
//{
//    public static class UrlHelperExtensions
//    {
//        private static IConfiguration _configuration;

//        // 设置 IConfiguration 实例  
//        public static void SetConfiguration(IConfiguration configuration)
//        {
//            _configuration = configuration;
//        }

//        public static string CustomContent(this IUrlHelper urlHelper, string contentPath)
//        {
//            // 这里可以添加自定义逻辑  
//            var route = AppSettingsReader.GetSetting("App:RouteName");
//            if (route.IsNullOrEmpty())
//            {
//                return urlHelper.Content(contentPath);
//            }
//            else
//            {
//                return urlHelper.Content($"{contentPath}/{route}");
//            }

//        }
//    }
//}
