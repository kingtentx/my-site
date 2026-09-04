using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace MySite.Web.Models
{
    public class PageModel
    {
        public int Id { get; set; }
        public int SiteId { get; set; } = 1;

        [Display(Name = "父级页面")]
        public int ParentId { get; set; }

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

        // 页面与菜单已解耦；以下字段仅保留到页面管理界面完成下一步清理，不参与新版 Builder 渲染。
        [Display(Name = "显示在导航")]
        public bool ShowInNavigation { get; set; } = true;
        [Display(Name = "导航标题")]
        public string NavigationTitle { get; set; }
        [Display(Name = "导航图标")]
        public string NavigationIcon { get; set; }
        [Display(Name = "打开方式")]
        public int NavigationTarget { get; set; }

        [Display(Name = "是否启用")]
        public bool IsActive { get; set; } = true;
        [Display(Name = "是否首页")]
        public bool IsHome { get; set; }
        [Display(Name = "排序")]
        public int Sort { get; set; }

        public int Status { get; set; }
        public string ComponentJson { get; set; }
        public DateTime? PublishTime { get; set; }
        public DateTime? CreationTime { get; set; }
        public string CreationBy { get; set; }
        public DateTime? UpdateTime { get; set; }
        public string UpdateBy { get; set; }
    }

    public class BuilderDocumentModel
    {
        public int SchemaVersion { get; set; } = 1;
        public string Name { get; set; }
        public List<BuilderNodeModel> Nodes { get; set; } = new List<BuilderNodeModel>();
        public Dictionary<string, object> Settings { get; set; } = new Dictionary<string, object>();
    }

    public class BuilderNodeModel
    {
        public string Id { get; set; }
        public string Type { get; set; }
        public int Version { get; set; } = 1;
        public string Name { get; set; }
        public bool Visible { get; set; } = true;
        public bool Locked { get; set; }
        public Dictionary<string, object> Props { get; set; } = new Dictionary<string, object>();
        public Dictionary<string, object> Style { get; set; } = new Dictionary<string, object>();
        public Dictionary<string, object> Bindings { get; set; } = new Dictionary<string, object>();
        public Dictionary<string, object> Actions { get; set; } = new Dictionary<string, object>();
        public List<BuilderNodeModel> Children { get; set; } = new List<BuilderNodeModel>();
        public Dictionary<string, List<BuilderNodeModel>> Slots { get; set; } = new Dictionary<string, List<BuilderNodeModel>>();
    }

    public class BuilderNodeRenderModel
    {
        public BuilderNodeModel Node { get; set; }
        public PageRenderModel Page { get; set; }
    }

    public class PageRenderModel
    {
        public int PageId { get; set; }
        public string PageName { get; set; }
        public string PagePath { get; set; }
        public string PageTitle { get; set; }
        public string SeoKeywords { get; set; }
        public string SeoDescription { get; set; }
        public BuilderDocumentModel Document { get; set; } = new BuilderDocumentModel();
        public BuilderDocumentModel HeaderDocument { get; set; } = new BuilderDocumentModel { Name = "Header" };
        public BuilderDocumentModel FooterDocument { get; set; } = new BuilderDocumentModel { Name = "Footer" };
        public SiteConfigModel SiteConfig { get; set; }
        public List<NavigationModel> Navigation { get; set; } = new List<NavigationModel>();
    }

    // 仅用于尚未删除的旧 Razor 文件通过编译；新版控制器、保存、预览和发布均不再读取该结构。
    [Obsolete("旧版平铺组件模型已废弃，请使用 BuilderNodeModel。")]
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
}