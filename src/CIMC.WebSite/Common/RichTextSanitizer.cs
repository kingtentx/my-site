using System.Net;
using System.Text;
using System.Text.Encodings.Web;
using System.Text.RegularExpressions;

namespace MySite.Web.Common;

/// <summary>
/// 页面装修文本组件使用的轻量富文本清洗器。
/// 仅保留排版所需的少量标签；所有普通文本重新编码，链接只允许安全协议。
/// </summary>
public static class RichTextSanitizer
{
    private static readonly HashSet<string> AllowedTags = new(StringComparer.OrdinalIgnoreCase)
    {
        "p", "br", "strong", "b", "em", "i", "u", "s", "strike",
        "ul", "ol", "li", "blockquote", "h1", "h2", "h3", "h4", "a"
    };

    private static readonly Regex AllowedTagPattern = new(
        @"</?(?:p|br|strong|b|em|i|u|s|strike|ul|ol|li|blockquote|h[1-4]|a)\b",
        RegexOptions.IgnoreCase | RegexOptions.Compiled);

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

        // 兼容历史页面：旧文本组件保存的是纯文本，不要求用户手工迁移。
        if (!AllowedTagPattern.IsMatch(value))
        {
            return PlainTextToHtml(value);
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

    private static string PlainTextToHtml(string value)
    {
        var normalized = value.Replace("\r\n", "\n").Replace('\r', '\n');
        var lines = normalized.Split('\n');
        var result = new StringBuilder(normalized.Length + 32);

        foreach (var line in lines)
        {
            result.Append("<p>");
            if (line.Length == 0)
            {
                result.Append("<br>");
            }
            else
            {
                AppendEncodedText(result, line);
            }
            result.Append("</p>");
        }

        return result.ToString();
    }

    private static void AppendEncodedText(StringBuilder result, string text)
    {
        if (string.IsNullOrEmpty(text)) return;
        // 先解实体再编码，避免已有 &amp; 被重复编码，同时仍然保证最终是纯文本。
        result.Append(HtmlEncoder.Default.Encode(WebUtility.HtmlDecode(text)));
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
            if (!string.Equals(name, "br", StringComparison.OrdinalIgnoreCase))
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

        result.Append('<').Append(name);
        if (name == "a")
        {
            var attrs = match.Groups["attrs"].Value;
            var href = ReadAttribute(attrs, "href");
            if (IsSafeHref(href))
            {
                result.Append(" href=\"")
                    .Append(HtmlEncoder.Default.Encode(WebUtility.HtmlDecode(href).Trim()))
                    .Append('"');
            }

            var target = ReadAttribute(attrs, "target");
            if (string.Equals(target, "_blank", StringComparison.OrdinalIgnoreCase))
            {
                result.Append(" target=\"_blank\" rel=\"noopener noreferrer\"");
            }
        }
        result.Append('>');
    }

    private static string ReadAttribute(string attrs, string name)
    {
        if (string.IsNullOrWhiteSpace(attrs)) return string.Empty;
        var pattern = @"(?:^|\s)" + Regex.Escape(name) + @"\s*=\s*(?:\"(?<v>[^\"]*)\"|'(?<v>[^']*)'|(?<v>[^\s\"'=<>`]+))";
        var match = Regex.Match(attrs, pattern, RegexOptions.IgnoreCase);
        return match.Success ? match.Groups["v"].Value : string.Empty;
    }

    private static bool IsSafeHref(string href)
    {
        href = WebUtility.HtmlDecode(href ?? string.Empty).Trim();
        if (string.IsNullOrEmpty(href)) return false;
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
}
