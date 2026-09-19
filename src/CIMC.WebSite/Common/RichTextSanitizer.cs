using System;
using System.Collections.Generic;
using System.Net;
using System.Text;
using System.Text.Encodings.Web;
using System.Text.RegularExpressions;
using System.Text.Unicode;

namespace MySite.Web.Common;

/// <summary>
/// 页面装修文本组件使用的轻量富文本清洗器。
/// 仅保留排版所需的标签和预定义样式类；所有普通文本重新编码，链接只允许安全协议。
/// </summary>
public static class RichTextSanitizer
{
    // 与 Razor 的全局 WebEncoderOptions 保持一致：Unicode 原样输出，HTML 特殊字符继续安全转义。
    private static readonly HtmlEncoder UnicodeHtmlEncoder = HtmlEncoder.Create(UnicodeRanges.All);

    private static readonly HashSet<string> AllowedTags = new(StringComparer.OrdinalIgnoreCase)
    {
        "p", "br", "strong", "b", "em", "i", "u", "s", "strike",
        "ul", "ol", "li", "blockquote", "h1", "h2", "h3", "h4", "a", "span",
        "table", "thead", "tbody", "tr", "th", "td", "img"
    };

    private static readonly HashSet<string> InlineClasses = new(StringComparer.Ordinal)
    {
        "sb-rte-size-sm", "sb-rte-size-md", "sb-rte-size-lg",
        "sb-rte-color-red", "sb-rte-color-blue", "sb-rte-color-green"
    };

    private static readonly HashSet<string> AlignmentClasses = new(StringComparer.Ordinal)
    {
        "sb-rte-align-left", "sb-rte-align-center", "sb-rte-align-right", "sb-rte-align-justify"
    };

    private static readonly Regex HtmlTokenPattern = new(
        @"<!--[\s\S]*?-->|<![^>]*>|</?[A-Za-z][^>]*>",
        RegexOptions.Compiled);

    private static readonly Regex TagPattern = new(
        @"^<\s*(?<close>/?)\s*(?<name>[A-Za-z0-9]+)(?<attrs>[^>]*)>$",
        RegexOptions.Compiled);

    public static string Sanitize(string value)
    {
        value ??= string.Empty;
        if (string.IsNullOrWhiteSpace(value))
        {
            return string.Empty;
        }

        var result = new StringBuilder(value.Length + 32);
        var index = 0;

        foreach (Match token in HtmlTokenPattern.Matches(value))
        {
            if (token.Index > index)
            {
                AppendEncodedText(result, value[index..token.Index]);
            }

            AppendSafeTag(result, token.Value);
            index = token.Index + token.Length;
        }

        if (index < value.Length)
        {
            AppendEncodedText(result, value[index..]);
        }

        return result.ToString();
    }

    private static void AppendEncodedText(StringBuilder result, string text)
    {
        if (string.IsNullOrEmpty(text)) return;
        // 先解实体再编码，避免已有 &amp; 被重复编码，同时仍然保证最终是纯文本。
        result.Append(UnicodeHtmlEncoder.Encode(WebUtility.HtmlDecode(text)));
    }

    private static void AppendSafeTag(StringBuilder result, string token)
    {
        var match = TagPattern.Match(token);
        if (!match.Success) return;

        var name = match.Groups["name"].Value.ToLowerInvariant();
        if (!AllowedTags.Contains(name)) return;

        var closing = match.Groups["close"].Value.Length > 0;
        if (closing)
        {
            if (name != "br" && name != "img")
            {
                result.Append("</").Append(name).Append('>');
            }
            return;
        }

        if (name == "br")
        {
            result.Append("<br>");
            return;
        }

        var imageSrc = name == "img" ? ReadAttribute(match.Groups["attrs"].Value, "src") : string.Empty;
        if (name == "img" && !IsSafeImageSrc(imageSrc)) return;

        result.Append('<').Append(name);
        HashSet<string> permittedClasses = name == "span" ? InlineClasses :
            name is "p" or "h1" or "h2" or "h3" or "h4" or "blockquote" or "li" or "th" or "td" ? AlignmentClasses : null;
        if (permittedClasses != null)
        {
            var classes = ReadAttribute(match.Groups["attrs"].Value, "class")
                .Split((char[])null, StringSplitOptions.RemoveEmptyEntries);
            var safeClasses = new List<string>();
            foreach (var className in classes)
            {
                if (permittedClasses.Contains(className) && !safeClasses.Contains(className))
                    safeClasses.Add(className);
            }
            if (safeClasses.Count > 0)
                result.Append(" class=\"").Append(string.Join(' ', safeClasses)).Append('"');
        }
        if (name == "a")
        {
            var attrs = match.Groups["attrs"].Value;
            var href = ReadAttribute(attrs, "href");
            if (IsSafeHref(href))
            {
                result.Append(" href=\"")
                    .Append(UnicodeHtmlEncoder.Encode(WebUtility.HtmlDecode(href).Trim()))
                    .Append('"');
            }

            var target = ReadAttribute(attrs, "target");
            if (string.Equals(target, "_blank", StringComparison.OrdinalIgnoreCase))
            {
                result.Append(" target=\"_blank\" rel=\"noopener noreferrer\"");
            }
        }
        else if (name == "img")
        {
            result.Append(" src=\"")
                .Append(UnicodeHtmlEncoder.Encode(WebUtility.HtmlDecode(imageSrc).Trim()))
                .Append('"');
            var alt = ReadAttribute(match.Groups["attrs"].Value, "alt");
            result.Append(" alt=\"").Append(UnicodeHtmlEncoder.Encode(WebUtility.HtmlDecode(alt))).Append('"');
        }
        else if (name is "td" or "th")
        {
            foreach (var attribute in new[] { "colspan", "rowspan" })
            {
                var value = ReadAttribute(match.Groups["attrs"].Value, attribute);
                if (int.TryParse(value, out var count) && count >= 2 && count <= 12)
                    result.Append(' ').Append(attribute).Append("=\"").Append(count).Append('"');
            }
        }
        result.Append('>');
    }

    private static string ReadAttribute(string attrs, string name)
    {
        if (string.IsNullOrWhiteSpace(attrs)) return string.Empty;
        var pattern = "(?:^|\\s)" + Regex.Escape(name) + "\\s*=\\s*(?:\"(?<v>[^\"]*)\"|'(?<v>[^']*)'|(?<v>[^\\s\"'=<>`]+))";
        var match = Regex.Match(attrs, pattern, RegexOptions.IgnoreCase);
        return match.Success ? match.Groups["v"].Value : string.Empty;
    }

    private static bool IsSafeHref(string href)
    {
        href = WebUtility.HtmlDecode(href ?? string.Empty).Trim();
        if (string.IsNullOrEmpty(href) || href.StartsWith("//", StringComparison.Ordinal)) return false;
        if (href.StartsWith("#", StringComparison.Ordinal) ||
            href.StartsWith("/", StringComparison.Ordinal) ||
            href.StartsWith("./", StringComparison.Ordinal) ||
            href.StartsWith("../", StringComparison.Ordinal))
        {
            return true;
        }

        return Uri.TryCreate(href, UriKind.Absolute, out var uri) &&
               (uri.Scheme.Equals(Uri.UriSchemeHttp, StringComparison.OrdinalIgnoreCase) ||
                uri.Scheme.Equals(Uri.UriSchemeHttps, StringComparison.OrdinalIgnoreCase) ||
                uri.Scheme.Equals(Uri.UriSchemeMailto, StringComparison.OrdinalIgnoreCase) ||
                uri.Scheme.Equals("tel", StringComparison.OrdinalIgnoreCase));
    }

    private static bool IsSafeImageSrc(string src)
    {
        src = WebUtility.HtmlDecode(src ?? string.Empty).Trim();
        if (string.IsNullOrEmpty(src) || src.StartsWith("//", StringComparison.Ordinal)) return false;
        if (src.StartsWith("/", StringComparison.Ordinal) ||
            src.StartsWith("./", StringComparison.Ordinal) ||
            src.StartsWith("../", StringComparison.Ordinal)) return true;
        return Uri.TryCreate(src, UriKind.Absolute, out var uri) &&
               (uri.Scheme.Equals(Uri.UriSchemeHttp, StringComparison.OrdinalIgnoreCase) ||
                uri.Scheme.Equals(Uri.UriSchemeHttps, StringComparison.OrdinalIgnoreCase));
    }
}
