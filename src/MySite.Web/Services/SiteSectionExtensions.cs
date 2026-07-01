using System.Text.Json.Nodes;
using MySite.Web.Models;

namespace MySite.Web.Services;

public static class SiteSectionExtensions
{
    public static string GetString(this SiteSection section, string key, string fallback = "")
    {
        if (section.Settings.TryGetPropertyValue(key, out var node) && node != null)
        {
            return node.ToString();
        }

        return fallback;
    }

    public static JsonArray GetArray(this SiteSection section, string key)
    {
        if (section.Settings.TryGetPropertyValue(key, out var node) && node is JsonArray array)
        {
            return array;
        }

        return new JsonArray();
    }
}
