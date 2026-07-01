using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CIMC.Core.Enums
{
    /// <summary>
    /// 菜单类型
    /// </summary>
    public enum AppSettinsType
    {
        /// <summary>
        /// 模块
        /// </summary>
        [Description("站点信息")]
        SiteInfo = 1,

        /// <summary>
        /// 站点底部
        /// </summary>
        [Description("页脚信息")]
        Footer = 2,

        /// <summary>
        /// 邮箱信息
        /// </summary>
        [Description("邮箱信息")]
        Email = 3,
        /// <summary>
        /// 微信信息
        /// </summary>
        [Description("微信信息")]
        Wecat = 4,

    }
}
