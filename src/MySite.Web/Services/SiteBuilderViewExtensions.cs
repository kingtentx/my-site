using System.Text.Json.Nodes;
using CIMC.WebSite.Models;

namespace CIMC.WebSite.Services;

public static class SiteBuilderViewExtensions
{
    public static string GetString(this SiteSectionDto section, string key, string fallback = "")
    {
        if (section.Settings.TryGetPropertyValue(key, out var node) && node != null)
        {
            return node.ToString();
        }

        return fallback;
    }

    public static JsonArray GetArray(this SiteSectionDto section, string key)
    {
        if (section.Settings.TryGetPropertyValue(key, out var node) && node is JsonArray array)
        {
            return array;
        }

        return new JsonArray();
    }
}
