using System.Text.Json.Nodes;

namespace CIMC.WebSite.Models;

public class SitePageDto
{
    public string PageKey { get; set; } = "home";
    public string Title { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string? Keywords { get; set; }
    public List<SiteSectionDto> Sections { get; set; } = new();
}

public class SiteSectionDto
{
    public string Id { get; set; } = Guid.NewGuid().ToString("N");
    public string Component { get; set; } = "rich-text";
    public string Name { get; set; } = string.Empty;
    public string? Title { get; set; }
    public string? SubTitle { get; set; }
    public string? LinkText { get; set; }
    public string? LinkUrl { get; set; }
    public List<string> Images { get; set; } = new();
    public int Sort { get; set; }
    public bool IsEnabled { get; set; } = true;
    public JsonObject Settings { get; set; } = new();
}

public class ComponentTemplateDto
{
    public string Key { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string Category { get; set; } = "基础组件";
    public string Description { get; set; } = string.Empty;
    public SiteSectionDto DefaultSection { get; set; } = new();
}

public class LoginInput
{
    public string UserName { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
}
