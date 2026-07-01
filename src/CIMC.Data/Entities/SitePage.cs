namespace CIMC.Data.Entities;

public class SitePage : BaseEntity
{
    public string PageKey { get; set; } = "home";

    public string Title { get; set; } = string.Empty;

    public string? Description { get; set; }

    public string? Keywords { get; set; }

    public List<SiteSection> Sections { get; set; } = new();
}
