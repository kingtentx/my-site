using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace MySite.Web.Models
{
    /// <summary>
    /// 页面编辑视图模型
    /// </summary>
    public class PageModel
    {
        public int Id { get; set; }

        public int SiteId { get; set; } = 1;

        [Display(Name = "页面名称")]
        [Required(ErrorMessage = "请输入页面名称")]
        public string PageName { get; set; }

        [Display(Name = "页面编码")]
        public string PageCode { get; set; }

        [Display(Name = "页面路径")]
        [Required(ErrorMessage = "请输入页面路径")]
        public string PagePath { get; set; }

        [Display(Name = "浏览器标题")]
        public string PageTitle { get; set; }

        [Display(Name = "SEO关键词")]
        public string SeoKeywords { get; set; }

        [Display(Name = "SEO描述")]
        public string SeoDescription { get; set; }

        [Display(Name = "是否启用")]
        public bool IsActive { get; set; } = true;

        [Display(Name = "是否首页")]
        public bool IsHome { get; set; }

        [Display(Name = "排序")]
        public int Sort { get; set; } = 0;

        public int Status { get; set; } = 0;

        public string ComponentJson { get; set; }

        public System.DateTime? PublishTime { get; set; }

        public System.DateTime? CreationTime { get; set; }

        public string CreationBy { get; set; }

        public System.DateTime? UpdateTime { get; set; }

        public string UpdateBy { get; set; }
    }

    /// <summary>
    /// 设计器组件 JSON DTO
    /// </summary>
    public class ComponentModel
    {
        public string Id { get; set; }
        public string Type { get; set; }
        public string Name { get; set; }
        public int Sort { get; set; }
        public bool Visible { get; set; } = true;
        public bool Locked { get; set; }
        public Dictionary<string, object> Props { get; set; } = new Dictionary<string, object>();
        public Dictionary<string, object> Style { get; set; } = new Dictionary<string, object>();
    }

    /// <summary>
    /// 前台页面渲染模型
    /// </summary>
    public class PageRenderModel
    {
        public int PageId { get; set; }
        public string PageName { get; set; }
        public string PagePath { get; set; }
        public string PageTitle { get; set; }
        public string SeoKeywords { get; set; }
        public string SeoDescription { get; set; }
        public List<ComponentModel> Components { get; set; } = new List<ComponentModel>();
        public SiteConfigModel SiteConfig { get; set; }
        public List<NavigationModel> Navigation { get; set; } = new List<NavigationModel>();
        public FooterModel Footer { get; set; }
    }
}
