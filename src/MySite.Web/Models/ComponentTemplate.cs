namespace MySite.Web.Models;

public class ComponentTemplate
{
    public string Key { get; set; } = string.Empty;

    public string Name { get; set; } = string.Empty;

    public string Category { get; set; } = "基础组件";

    public string Description { get; set; } = string.Empty;

    public SiteSection DefaultSection { get; set; } = new();
}
