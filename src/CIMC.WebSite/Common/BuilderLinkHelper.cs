using System;
using CIMC.Data;
using CIMC.EntityFramework;
using Newtonsoft.Json.Linq;

namespace MySite.Web.Common;

public static class BuilderLinkHelper
{
    public static string Resolve(object value, IRepository<WebsitePage> pages)
    {
        if (value == null || value is string) return null;
        var link = JToken.FromObject(value) as JObject;
        if (link == null) return null;
        if (link.Value<string>("type") == "page")
        {
            if (!int.TryParse(link["pageId"]?.ToString(), out var id) || id <= 0) return null;
            var page = pages.GetOne(id);
            if (page == null || page.IsDelete || !page.IsActive || page.Status != 1) return null;
            var path = page.PagePath;
            return !string.IsNullOrEmpty(path) && path.StartsWith("/", StringComparison.Ordinal)
                && !path.StartsWith("//", StringComparison.Ordinal) && !path.Contains('\\')
                && !path.StartsWith("/__global/", StringComparison.OrdinalIgnoreCase) ? path : null;
        }
        var url = link.Value<string>("url")?.Trim();
        return link.Value<string>("type") == "external" && Uri.TryCreate(url, UriKind.Absolute, out var uri)
            && (uri.Scheme == Uri.UriSchemeHttp || uri.Scheme == Uri.UriSchemeHttps) ? url : null;
    }
}
