namespace MySite.Web
{
    /// <summary>后台菜单及操作权限的代码常量。</summary>
    public class MenuCode
    {
        #region 系统菜单
        /// <summary>系统管理权限分组代码。</summary>
        public const string System = "System";
        /// <summary>系统管理下的管理员权限代码。</summary>
        public const string System_Admin = "System_Admin";// 管理员
        /// <summary>系统管理下的角色权限代码。</summary>
        public const string System_Role = "System_Role";// 角色
        /// <summary>系统管理下的菜单权限代码。</summary>
        public const string System_Menu = "System_Menu";// 菜单
        /// <summary>系统管理下的审计日志权限代码。</summary>
        public const string System_AuditLog = "System_AuditLog";// 审计日志
        #endregion

        #region 网站管理
        /// <summary>站点权限分组代码。</summary>
        public const string Site = "Site";
        /// <summary>站点基础信息的权限代码。</summary>
        public const string Site_Info = "Site_Info";// 站点基础信息
        /// <summary>站点下的留言权限代码。</summary>
        public const string Site_Message = "Site_Message";// 用户留言
        /// <summary>页面树、导航和页面装修的权限代码。</summary>
        public const string Website_Page = "Website_Page";// 页面树、网站导航与页面装修
        #endregion

        #region 内容管理
        /// <summary>内容权限分组代码。</summary>
        public const string Content = "Content";
        /// <summary>内容下的文章权限代码。</summary>
        public const string Content_Article = "Content_Article";//文章管理
        /// <summary>产品管理权限代码。</summary>
        public const string Content_Product = "Content_Product";
        /// <summary>内容下的招聘岗位权限代码。</summary>
        public const string Content_Job = "Content_Job";//招聘管理
        /// <summary>内容下的模块权限代码。</summary>
        public const string Content_Module = "Content_Module";//页面模块
        /// <summary>内容下的分类标签权限代码。</summary>
        public const string Content_Tags = "Content_Tags";//标签管理
        /// <summary>内容下的分类权限代码。</summary>
        public const string Content_Category = "Content_Category";//分类管理
        /// <summary>内容附件的权限代码。</summary>
        public const string Content_Attachments = "Content_Attachments";//附件管理
        /// <summary>内容下的素材图片权限代码。</summary>
        public const string Content_Images = "Content_Images";//素材管理
        #endregion
    }
}
