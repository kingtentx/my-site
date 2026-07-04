namespace CimcSite.Web
{
    /// <summary>
    /// 通用常量
    /// </summary>
    public class CacheKey
    {
        public const int ExpirationTimeLen_2 = 2;

        public const int ExpirationTimeLen_5 = 5;

        public const int ExpirationTimeLen_100 = 100;




        #region 缓存

        /// <summary>
        /// 验证码
        /// </summary>
        public const string ValidateCode = "ValidateCode:";
        /// <summary>
        /// 当前页面布局配置
        /// </summary>
        public const string PageLayout = "PageLayout:";

        /// <summary>
        /// 菜单缓存键
        /// </summary>
        public const string PermissionMenu = "PermissionMenu:";

        /// <summary>
        /// 微信配置
        /// </summary>
        public const string WeiXin_Config = "WeiXin_Config";
        /// <summary>
        /// 微信appid键值
        /// </summary>
        public const string WeiXin_Token = "WeiXin_Token:";
        /// <summary>
        /// 微信菜单
        /// </summary>
        public const string WeiXin_Menu = "WeiXin_Menu";
        /// <summary>
        /// 微信菜单
        /// </summary>
        public const string WeiXin_AccessToken = "WeiXin_AccessToken:";
        /// <summary>
        /// Jsapi_Ticket 
        /// </summary>
        public const string WeiXin_JsapiTicket = "WeiXin_JsapiTicket:";

        #endregion
    }
}
