using System.Collections.Generic;

namespace CimcSite.Web.Models
{
    public class SiteViewModel
    {
        /// <summary>
        /// 标题
        /// </summary>
        public string Title { get; set; }
        /// <summary>
        /// 主标题
        /// </summary>
        public string MainTitle { get; set; }
        /// <summary>
        /// 二级标题
        /// </summary>
        public string SubTitle { get; set; }
        /// <summary>
        /// 当前导航
        /// </summary>
        public NavigationModel CurrentNav { get; set; }
        /// <summary>
        /// 站点信息
        /// </summary>
        public SiteInfoModel SiteInfo { get; set; }
        /// <summary>
        /// 底部信息
        /// </summary>
        public FooterModel FooterInfo { get; set; }
        /// <summary>
        /// 导航列表
        /// </summary>
        public List<NavigationModel> Navlist = new List<NavigationModel>();

    }
}
