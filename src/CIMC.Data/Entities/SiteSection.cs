namespace CIMC.Data.Entities;

public class SiteSection : BaseEntity
{
    public int SitePageId { get; set; }

    public SitePage? SitePage { get; set; }

    public string SectionKey { get; set; } = Guid.NewGuid().ToString("N");

    public string Component { get; set; } = "rich-text";

    public string Name { get; set; } = string.Empty;

    public string? Title { get; set; }

    public string? SubTitle { get; set; }

    public string? LinkText { get; set; }

    public string? LinkUrl { get; set; }

    public string? ImagesJson { get; set; }

    public string? SettingsJson { get; set; }

    public int Sort { get; set; }

    public bool IsEnabled { get; set; } = true;
}
