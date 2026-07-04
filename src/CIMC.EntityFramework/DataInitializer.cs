using CIMC.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text.Json;
using System.Text.RegularExpressions;

namespace CIMC.Data
{
    /// <summary>
    /// 初始化数据
    /// </summary>
    public class DataInitializer
    {
        public void Create(AppDbContext context)
        {
            InitUser(context);
            InitMenu(context);
            InitArticles(context);
            RepairSiteAssetPaths(context);
        }

        private void InitUser(AppDbContext context)
        {
            const string SuperAdmin = "admin";
            var system_user = context.Admin.FirstOrDefault(p => p.UserName.Equals(SuperAdmin));
            if (system_user == null)
            {
                var admin = new Admin
                {
                    UserName = SuperAdmin,
                    RealName = "超级管理员",
                    Password = ToMD5("123qwe"),
                    IsAdmin = true,
                    IsActive = true
                };
                context.Admin.Add(admin);
                context.SaveChanges();
            }
        }

        private void InitMenu(AppDbContext context)
        {
            var exists = context.Menu.FirstOrDefault(p => p.Title.Equals(MenuSeedData.MainMenu.Title));
            if (exists == null)
            {
                var main = MenuSeedData.MainMenu;
                context.Menu.Add(main);
                context.SaveChanges();

                var system = MenuSeedData.SystemMenu;
                context.Menu.Add(system);
                context.SaveChanges();

                var subMenus = MenuSeedData.GetSubMenus(system.Id);
                context.Menu.AddRange(subMenus);
                context.SaveChanges();
            }

            InitSiteMenus(context);
        }

        private void InitSiteMenus(AppDbContext context)
        {
            var content = EnsureMenu(context, "内容管理", "Content", 0, "", "layui-icon-read", 1, 30);
            EnsureMenu(context, "新闻管理", "Content_Article", content.Id, "/article/index", "layui-icon-list", 2, 31, "Add,Edit,Delete");
            EnsureMenu(context, "素材管理", "Content_Images", content.Id, "/images/index", "layui-icon-picture", 2, 35, "Add,Edit,Delete");
        }

        private Menu EnsureMenu(AppDbContext context, string title, string permissionKey, int pid, string path, string icon, int menuType, int sort, string buttons = null)
        {
            var menu = context.Menu.FirstOrDefault(p => p.PermissionKey == permissionKey || (p.Title == title && p.Pid == pid));
            if (menu == null)
            {
                menu = new Menu
                {
                    Title = title,
                    PermissionKey = permissionKey,
                    Pid = pid,
                    Path = path,
                    Icon = icon,
                    Buttons = buttons,
                    MenuType = menuType,
                    IsShow = true,
                    Spread = false,
                    Sort = sort,
                    CreationTime = DateTime.Now,
                    CreationBy = "system"
                };
                context.Menu.Add(menu);
                context.SaveChanges();
            }
            else
            {
                menu.Path = path;
                menu.Icon = icon;
                menu.Buttons = buttons;
                menu.MenuType = menuType;
                menu.IsShow = true;
                menu.Sort = sort;
                context.SaveChanges();
            }

            return menu;
        }

        private void InitArticles(AppDbContext context)
        {
            var jsonPath = GetSeedDataPath("news-data.json");
            if (!File.Exists(jsonPath))
            {
                return;
            }

            var json = File.ReadAllText(jsonPath);
            var items = JsonSerializer.Deserialize<List<NewsSeedItem>>(json, new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            });

            if (items == null || items.Count == 0)
            {
                return;
            }

            foreach (var item in items)
            {
                if (string.IsNullOrWhiteSpace(item.Title))
                {
                    continue;
                }

                var sourceUrl = $"/newsinfo/{item.Id}.html";
                var exists = context.Article.FirstOrDefault(p => p.SourceUrl == sourceUrl || p.Title == item.Title);
                if (exists != null)
                {
                    continue;
                }

                context.Article.Add(new Article
                {
                    Title = item.Title,
                    Description = TrimText(item.Desc, 240),
                    Detail = NormalizeSiteHtml(string.IsNullOrWhiteSpace(item.Content) ? HtmlParagraphs(item.ContentText) : item.Content),
                    Author = "中集洋山",
                    Source = "中集洋山官网",
                    SourceUrl = sourceUrl,
                    ImageUrl = NormalizeSitePath(item.Img),
                    TagType = 1,
                    TagId = 0,
                    Sort = 0,
                    ViewCount = item.Hits,
                    IsHot = false,
                    IsActive = true,
                    IsDelete = false,
                    CreationBy = "system",
                    CreationTime = ParseDate(item.Date)
                });
            }

            context.SaveChanges();
        }

        private string NormalizeSiteHtml(string html)
        {
            if (string.IsNullOrWhiteSpace(html))
            {
                return string.Empty;
            }

            var value = html.Replace("\\", "/");
            value = Regex.Replace(value, @"(?:/syle/)+images/", "/syle/images/", RegexOptions.IgnoreCase);
            value = Regex.Replace(value, @"(?<!/syle/)(?:\.\./)*images/news/", "/syle/images/news/", RegexOptions.IgnoreCase);
            value = Regex.Replace(value, @"(?<!/syle/)(?:\.\./)*images/", "/syle/images/", RegexOptions.IgnoreCase);
            value = Regex.Replace(value, @"(?:/syle/)+images/", "/syle/images/", RegexOptions.IgnoreCase);

            value = Regex.Replace(value, "<script[\\s\\S]*?</script>", string.Empty, RegexOptions.IgnoreCase);
            return value;
        }

        private string NormalizeSitePath(string path)
        {
            if (string.IsNullOrWhiteSpace(path))
            {
                return "/syle/images/174376305.png";
            }

            var value = path.Trim().Replace("\\", "/");
            value = Regex.Replace(value, @"(?:/syle/)+images/", "/syle/images/", RegexOptions.IgnoreCase);
            value = Regex.Replace(value, @"^/?(?:\.\./)*images/news/", "/syle/images/news/", RegexOptions.IgnoreCase);
            value = Regex.Replace(value, @"^/?(?:\.\./)*images/", "/syle/images/", RegexOptions.IgnoreCase);

            if (value.StartsWith("syle/images/", StringComparison.OrdinalIgnoreCase))
            {
                value = "/" + value;
            }

            return Regex.Replace(value, @"(?:/syle/)+images/", "/syle/images/", RegexOptions.IgnoreCase);
        }

        private void RepairSiteAssetPaths(AppDbContext context)
        {
            var hasChanges = false;

            foreach (var article in context.Article.Where(p => !p.IsDelete && (p.ImageUrl.Contains("//syle") || p.ImageUrl.Contains("../images") || p.ImageUrl.Contains("images/") || p.Detail.Contains("//syle") || p.Detail.Contains("../images"))))
            {
                var imageUrl = NormalizeSitePath(article.ImageUrl);
                var detail = NormalizeSiteHtml(article.Detail);

                if (article.ImageUrl != imageUrl)
                {
                    article.ImageUrl = imageUrl;
                    hasChanges = true;
                }

                if (article.Detail != detail)
                {
                    article.Detail = detail;
                    hasChanges = true;
                }
            }

            if (hasChanges)
            {
                context.SaveChanges();
            }
        }

        private string HtmlParagraphs(string text)
        {
            if (string.IsNullOrWhiteSpace(text))
            {
                return string.Empty;
            }

            var paragraphs = text.Split('\n', StringSplitOptions.RemoveEmptyEntries)
                .Select(p => $"<p>{System.Net.WebUtility.HtmlEncode(p.Trim())}</p>");
            return string.Join(Environment.NewLine, paragraphs);
        }

        private DateTime? ParseDate(string value)
        {
            return DateTime.TryParse(value, out var result) ? result : DateTime.Now;
        }

        private string TrimText(string value, int maxLength)
        {
            if (string.IsNullOrWhiteSpace(value))
            {
                return string.Empty;
            }

            return value.Length <= maxLength ? value : value.Substring(0, maxLength);
        }

        private string ToMD5(string str)
        {
            MD5 md5 = MD5.Create();
            byte[] bytes_out = md5.ComputeHash(System.Text.Encoding.UTF8.GetBytes(str));
            string result = BitConverter.ToString(bytes_out).Replace("-", "");
            return result;
        }

        private string GetSeedDataPath(string fileName)
        {
            var current = Directory.GetCurrentDirectory();
            var candidates = new[]
            {
                Path.Combine(current, "wwwroot", "syle", "data", fileName),
                Path.Combine(AppContext.BaseDirectory, "wwwroot", "syle", "data", fileName)
            };

            return candidates.FirstOrDefault(File.Exists) ?? candidates[0];
        }

        private class NewsSeedItem
        {
            public string Id { get; set; }
            public string Title { get; set; }
            public string Desc { get; set; }
            public string Img { get; set; }
            public string Date { get; set; }
            public int Hits { get; set; }
            public string Category { get; set; }
            public string Content { get; set; }
            public string ContentText { get; set; }
        }
    }
}
