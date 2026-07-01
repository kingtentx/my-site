using System.Text.Json.Nodes;

namespace MySite.Web.Models;

public class SiteSection
{
    public string Id { get; set; } = Guid.NewGuid().ToString("N");

    public string Component { get; set; } = "rich-text";

    public string Name { get; set; } = "内容模块";

    public string Title { get; set; } = string.Empty;

    public string SubTitle { get; set; } = string.Empty;

    public string LinkText { get; set; } = string.Empty;

    public string LinkUrl { get; set; } = string.Empty;

    public List<string> Images { get; set; } = new();

    public int Sort { get; set; }

    public bool IsEnabled { get; set; } = true;

    public JsonObject Settings { get; set; } = new();
}
