using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace MySite.Web.Models
{
    /// <summary>承载页面相关数据。</summary>
    public class PageModel
    {
        /// <summary>主键。</summary>
        public int Id { get; set; }
        /// <summary>所属站点的主键。</summary>
        public int SiteId { get; set; } = 1;

        /// <summary>父级记录的主键。</summary>
        [Display(Name = "父级页面")]
        public int ParentId { get; set; }

        /// <summary>页面名称。</summary>
        [Display(Name = "页面名称")]
        [Required(ErrorMessage = "请输入页面名称")]
        public string PageName { get; set; }

        /// <summary>页面编码。</summary>
        [Display(Name = "页面编码")]
        public string PageCode { get; set; }

        /// <summary>页面访问路径。</summary>
        [Display(Name = "页面路径")]
        [Required(ErrorMessage = "请输入页面路径")]
        public string PagePath { get; set; }

        /// <summary>浏览器页面标题。</summary>
        [Display(Name = "浏览器标题")]
        public string PageTitle { get; set; }

        /// <summary>页面 SEO 关键词。</summary>
        [Display(Name = "SEO关键词")]
        public string SeoKeywords { get; set; }

        /// <summary>页面 SEO 描述。</summary>
        [Display(Name = "SEO描述")]
        public string SeoDescription { get; set; }

        /// <summary>是否显示在网站导航中。</summary>
        [Display(Name = "显示在网站导航")]
        public bool ShowInNavigation { get; set; } = true;

        /// <summary>导航显示标题。</summary>
        [Display(Name = "导航标题")]
        public string NavigationTitle { get; set; }

        /// <summary>导航图标。</summary>
        [Display(Name = "导航图标")]
        public string NavigationIcon { get; set; }

        /// <summary>导航链接的打开方式。</summary>
        [Display(Name = "打开方式")]
        public int NavigationTarget { get; set; }

        /// <summary>是否启用。</summary>
        [Display(Name = "是否启用")]
        public bool IsActive { get; set; } = true;

        /// <summary>是否为首页。</summary>
        [Display(Name = "是否首页")]
        public bool IsHome { get; set; }

        /// <summary>排序值，数值越小越靠前。</summary>
        [Display(Name = "排序")]
        public int Sort { get; set; }

        /// <summary>当前状态。</summary>
        public int Status { get; set; }
        /// <summary>页面组件文档的 JSON 内容。</summary>
        public string ComponentJson { get; set; }
        /// <summary>发布时间。</summary>
        public DateTime? PublishTime { get; set; }
        /// <summary>创建时间。</summary>
        public DateTime? CreationTime { get; set; }
        /// <summary>创建人。</summary>
        public string CreationBy { get; set; }
        /// <summary>最后更新时间。</summary>
        public DateTime? UpdateTime { get; set; }
        /// <summary>最后更新人。</summary>
        public string UpdateBy { get; set; }
    }

    /// <summary>承载页面装修相关数据。</summary>
    public class BuilderDocumentModel
    {
        /// <summary>文档结构版本。</summary>
        public int SchemaVersion { get; set; } = 1;
        /// <summary>名称。</summary>
        public string Name { get; set; }
        /// <summary>页面节点集合。</summary>
        public List<BuilderNodeModel> Nodes { get; set; } = new List<BuilderNodeModel>();
        /// <summary>设置项集合。</summary>
        public Dictionary<string, object> Settings { get; set; } = new Dictionary<string, object>();
    }

    /// <summary>承载页面装修相关数据。</summary>
    public class BuilderNodeModel
    {
        /// <summary>主键。</summary>
        public string Id { get; set; }
        /// <summary>业务类型。</summary>
        public string Type { get; set; }
        /// <summary>版本号。</summary>
        public int Version { get; set; } = 1;
        /// <summary>名称。</summary>
        public string Name { get; set; }
        /// <summary>是否显示。</summary>
        public bool Visible { get; set; } = true;
        /// <summary>是否锁定。</summary>
        public bool Locked { get; set; }
        /// <summary>节点属性集合。</summary>
        public Dictionary<string, object> Props { get; set; } = new Dictionary<string, object>();
        /// <summary>样式配置。</summary>
        public Dictionary<string, object> Style { get; set; } = new Dictionary<string, object>();
        /// <summary>数据绑定集合。</summary>
        public Dictionary<string, object> Bindings { get; set; } = new Dictionary<string, object>();
        /// <summary>交互动作集合。</summary>
        public Dictionary<string, object> Actions { get; set; } = new Dictionary<string, object>();
        /// <summary>子节点集合。</summary>
        public List<BuilderNodeModel> Children { get; set; } = new List<BuilderNodeModel>();
        /// <summary>命名插槽集合。</summary>
        public Dictionary<string, List<BuilderNodeModel>> Slots { get; set; } = new Dictionary<string, List<BuilderNodeModel>>();
    }

    /// <summary>承载页面装修相关数据。</summary>
    public class BuilderNodeRenderModel
    {
        /// <summary>渲染后的样式。</summary>
        public string RenderedStyle { get; set; }
        /// <summary>当前页面节点。</summary>
        public BuilderNodeModel Node { get; set; }
        /// <summary>当前页面信息。</summary>
        public PageRenderModel Page { get; set; }
    }

    /// <summary>承载页面相关数据。</summary>
    public class PageRenderModel
    {
        /// <summary>页面主键。</summary>
        public int PageId { get; set; }
        /// <summary>页面名称。</summary>
        public string PageName { get; set; }
        /// <summary>页面访问路径。</summary>
        public string PagePath { get; set; }
        /// <summary>浏览器页面标题。</summary>
        public string PageTitle { get; set; }
        /// <summary>页面 SEO 关键词。</summary>
        public string SeoKeywords { get; set; }
        /// <summary>页面 SEO 描述。</summary>
        public string SeoDescription { get; set; }
        /// <summary>页面装修文档。</summary>
        public BuilderDocumentModel Document { get; set; } = new BuilderDocumentModel();
        /// <summary>全局页眉文档。</summary>
        public BuilderDocumentModel HeaderDocument { get; set; } = new BuilderDocumentModel { Name = "Header" };
        /// <summary>全局页脚文档。</summary>
        public BuilderDocumentModel FooterDocument { get; set; } = new BuilderDocumentModel { Name = "Footer" };
        /// <summary>站点配置。</summary>
        public SiteConfigModel SiteConfig { get; set; }
        /// <summary>导航数据。</summary>
        public List<NavigationModel> Navigation { get; set; } = new List<NavigationModel>();
    }

    /// <summary>承载Component相关数据。</summary>
    [Obsolete("旧版平铺组件模型已废弃，请使用 BuilderNodeModel。")]
    public class ComponentModel
    {
        /// <summary>主键。</summary>
        public string Id { get; set; }
        /// <summary>业务类型。</summary>
        public string Type { get; set; }
        /// <summary>名称。</summary>
        public string Name { get; set; }
        /// <summary>排序值，数值越小越靠前。</summary>
        public int Sort { get; set; }
        /// <summary>是否显示。</summary>
        public bool Visible { get; set; } = true;
        /// <summary>是否锁定。</summary>
        public bool Locked { get; set; }
        /// <summary>节点属性集合。</summary>
        public Dictionary<string, object> Props { get; set; } = new Dictionary<string, object>();
        /// <summary>样式配置。</summary>
        public Dictionary<string, object> Style { get; set; } = new Dictionary<string, object>();
    }
}
