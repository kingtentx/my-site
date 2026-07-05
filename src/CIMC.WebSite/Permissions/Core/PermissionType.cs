using System.ComponentModel;

namespace MySite.Web
{
    public enum PermissionType
    {
        /// <summary>
        /// 查看
        /// </summary>
        [Description("查看")]
        View = 0,
        /// <summary>
        /// 增加
        /// </summary>
        [Description("新增")]
        Add = 1,
        /// <summary>
        /// 修改
        /// </summary>
        [Description("修改")]
        Edit = 2,
        /// <summary>
        /// 删除
        /// </summary>
        [Description("删除")]
        Delete = 3,
        /// <summary>
        /// 导入
        /// </summary>
        [Description("导入")]
        Import = 4,
        /// <summary>
        /// 导出
        /// </summary>
        [Description("导出")]
        Export = 5,
        /// <summary>
        /// 授权
        /// </summary>
        [Description("授权")]
        Authorize = 6,
        /// <summary>
        /// 配置页面
        /// </summary>
        [Description("配置页面")]
        Layout = 7,
        /// <summary>
        /// 页面装修
        /// </summary>
        [Description("页面装修")]
        Design = 8,
        /// <summary>
        /// 页面发布
        /// </summary>
        [Description("页面发布")]
        Publish = 9
    }
}
