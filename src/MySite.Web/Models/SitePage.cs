namespace MySite.Web.Models;

public class SitePage
{
    public string PageKey { get; set; } = "home";

    public string Title { get; set; } = "通用门户网站";

    public string Description { get; set; } = string.Empty;

    public string Keywords { get; set; } = string.Empty;

    public DateTime UpdatedAt { get; set; } = DateTime.Now;

    public List<SiteSection> Sections { get; set; } = new();
}
