using System.Collections.Generic;
using CIMC.Data.Entities.WebsiteBuilder;

namespace MySite.Web.WebsiteBuilder.Models
{
    public class WebsitePageDesignRequest
    {
        public string LayoutJson { get; set; }
        public string ComponentJson { get; set; }
        public string DraftJson { get; set; }
    }

    public class WebsitePageVersionRollbackRequest
    {
        public int VersionId { get; set; }
    }

    public class WebsitePageRenderModel
    {
        public WebsiteSiteConfig SiteConfig { get; set; }
        public WebsitePage Page { get; set; }
        public IList<ContentNews> News { get; set; } = new List<ContentNews>();
        public IList<ContentProduct> Products { get; set; } = new List<ContentProduct>();
        public IList<ContentJob> Jobs { get; set; } = new List<ContentJob>();
        public bool IsPreview { get; set; }
    }
}