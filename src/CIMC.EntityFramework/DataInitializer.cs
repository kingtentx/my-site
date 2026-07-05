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
            InitSiteConfig(context);
            InitFooter(context);
            InitNavigation(context);
            InitSitePages(context);
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

                var website = MenuSeedData.WebsiteMenu;
                context.Menu.Add(website);
                context.SaveChanges();

                var websiteMenus = MenuSeedData.GetWebsiteMenus(website.Id);
                context.Menu.AddRange(websiteMenus);
                context.SaveChanges();
            }

            InitSiteMenus(context);
        }

        private void InitSiteMenus(AppDbContext context)
        {
            var website = EnsureMenu(context, "网站管理", "Site", 0, "", "layui-icon-website", 1, 10);
            EnsureMenu(context, "站点设置", "Site_Info", website.Id, "/siteconfig/index", "layui-icon-set", 2, 11, "Edit");
            EnsureMenu(context, "页面管理", "Website_Page", website.Id, "/page/index", "layui-icon-template", 2, 12, "Add,Edit,Delete,Design,Publish");
            EnsureMenu(context, "导航管理", "Site_Navigation", website.Id, "/navigation/index", "layui-icon-nav", 2, 13, "Add,Edit,Delete");
            EnsureMenu(context, "页脚设置", "Site_Footer", website.Id, "/footer/index", "layui-icon-bottom", 2, 14, "Edit");

            var content = EnsureMenu(context, "内容管理", "Content", 0, "", "layui-icon-read", 1, 30);
            EnsureMenu(context, "新闻管理", "Content_Article", content.Id, "/article/index", "layui-icon-list", 2, 31, "Add,Edit,Delete");
            EnsureMenu(context, "产品分类", "Content_ProductCategory", content.Id, "/productcategory/index", "layui-icon-cols", 2, 32, "Add,Edit,Delete");
            EnsureMenu(context, "产品管理", "Content_Product", content.Id, "/product/index", "layui-icon-component", 2, 33, "Add,Edit,Delete");
            EnsureMenu(context, "招聘管理", "Content_Job", content.Id, "/job/index", "layui-icon-friends", 2, 34, "Add,Edit,Delete");
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

        private void InitSiteConfig(AppDbContext context)
        {
            var exists = context.WebsiteSiteConfig.FirstOrDefault(p => p.Id == 1);
            if (exists == null)
            {
                context.WebsiteSiteConfig.Add(new WebsiteSiteConfig
                {
                    Id = 1,
                    SiteName = "企业官网",
                    BrowserTitle = "企业官网 - 专业服务",
                    Keywords = "企业官网,产品,服务",
                    Description = "企业官网介绍企业产品与服务",
                    Phone = "400-000-0000",
                    Email = "contact@example.com",
                    Address = "中国",
                    Copyright = "Copyright © " + DateTime.Now.Year + " 企业官网. All Rights Reserved.",
                    Theme = "default",
                    Language = "zh-CN",
                    IsActive = true,
                    IsDelete = false,
                    CreationBy = "system",
                    CreationTime = DateTime.Now
                });
                context.SaveChanges();
            }
        }

        private void InitFooter(AppDbContext context)
        {
            var exists = context.WebsiteFooter.FirstOrDefault(p => p.Id == 1);
            if (exists == null)
            {
                context.WebsiteFooter.Add(new WebsiteFooter
                {
                    Id = 1,
                    CompanyName = "企业官网",
                    Intro = "专注企业数字化建设，提供专业的网站建设与数字化解决方案。",
                    Phone = "400-000-0000",
                    Email = "contact@example.com",
                    Address = "中国",
                    IcpNo = "",
                    PoliceNo = "",
                    Copyright = "Copyright © " + DateTime.Now.Year + " 企业官网. All Rights Reserved.",
                    FriendLinks = "[]",
                    BgColor = "#2c3e50",
                    TextColor = "#ffffff",
                    IsActive = true,
                    IsDelete = false,
                    CreationBy = "system",
                    CreationTime = DateTime.Now
                });
                context.SaveChanges();
            }
        }

        private void InitNavigation(AppDbContext context)
        {
            var exists = context.WebsiteNavigation.FirstOrDefault(p => p.Title == "首页" && p.Pid == 0);
            if (exists != null) return;

            var navs = new List<WebsiteNavigation>
            {
                new WebsiteNavigation { Pid = 0, Title = "首页", Path = "/", Sort = 1, IsShow = true, Target = 0, IsActive = true, IsDelete = false, CreationBy = "system", CreationTime = DateTime.Now },
                new WebsiteNavigation { Pid = 0, Title = "关于我们", Path = "/about", Sort = 2, IsShow = true, Target = 0, IsActive = true, IsDelete = false, CreationBy = "system", CreationTime = DateTime.Now },
                new WebsiteNavigation { Pid = 0, Title = "产品中心", Path = "/products", Sort = 3, IsShow = true, Target = 0, IsActive = true, IsDelete = false, CreationBy = "system", CreationTime = DateTime.Now },
                new WebsiteNavigation { Pid = 0, Title = "新闻中心", Path = "/news", Sort = 4, IsShow = true, Target = 0, IsActive = true, IsDelete = false, CreationBy = "system", CreationTime = DateTime.Now },
                new WebsiteNavigation { Pid = 0, Title = "招聘中心", Path = "/jobs", Sort = 5, IsShow = true, Target = 0, IsActive = true, IsDelete = false, CreationBy = "system", CreationTime = DateTime.Now },
                new WebsiteNavigation { Pid = 0, Title = "联系我们", Path = "/contact", Sort = 6, IsShow = true, Target = 0, IsActive = true, IsDelete = false, CreationBy = "system", CreationTime = DateTime.Now }
            };
            context.WebsiteNavigation.AddRange(navs);
            context.SaveChanges();
        }

        private void InitSitePages(AppDbContext context)
        {
            if (context.WebsitePage.Any(p => p.IsHome)) return;

            var homeJson = BuildHomeComponentsJson();
            var pages = new List<WebsitePage>
            {
                new WebsitePage { SiteId = 1, PageName = "首页", PageCode = "home", PagePath = "/", PageTitle = "企业官网 - 首页", LayoutJson = "{\"width\":\"full\",\"theme\":\"default\"}", ComponentJson = homeJson, Status = 1, IsHome = true, Sort = 1, IsActive = true, IsDelete = false, PublishTime = DateTime.Now, CreationBy = "system", CreationTime = DateTime.Now },
                new WebsitePage { SiteId = 1, PageName = "关于我们", PageCode = "about", PagePath = "/about", PageTitle = "关于我们", LayoutJson = "{\"width\":\"boxed\",\"theme\":\"default\"}", ComponentJson = BuildAboutComponentsJson(), Status = 1, IsHome = false, Sort = 2, IsActive = true, IsDelete = false, PublishTime = DateTime.Now, CreationBy = "system", CreationTime = DateTime.Now },
                new WebsitePage { SiteId = 1, PageName = "产品中心", PageCode = "products", PagePath = "/products", PageTitle = "产品中心", LayoutJson = "{\"width\":\"boxed\",\"theme\":\"default\"}", ComponentJson = BuildProductsComponentsJson(), Status = 1, IsHome = false, Sort = 3, IsActive = true, IsDelete = false, PublishTime = DateTime.Now, CreationBy = "system", CreationTime = DateTime.Now },
                new WebsitePage { SiteId = 1, PageName = "新闻中心", PageCode = "news", PagePath = "/news", PageTitle = "新闻中心", LayoutJson = "{\"width\":\"boxed\",\"theme\":\"default\"}", ComponentJson = BuildNewsComponentsJson(), Status = 1, IsHome = false, Sort = 4, IsActive = true, IsDelete = false, PublishTime = DateTime.Now, CreationBy = "system", CreationTime = DateTime.Now },
                new WebsitePage { SiteId = 1, PageName = "招聘中心", PageCode = "jobs", PagePath = "/jobs", PageTitle = "招聘中心", LayoutJson = "{\"width\":\"boxed\",\"theme\":\"default\"}", ComponentJson = BuildJobsComponentsJson(), Status = 1, IsHome = false, Sort = 5, IsActive = true, IsDelete = false, PublishTime = DateTime.Now, CreationBy = "system", CreationTime = DateTime.Now },
                new WebsitePage { SiteId = 1, PageName = "联系我们", PageCode = "contact", PagePath = "/contact", PageTitle = "联系我们", LayoutJson = "{\"width\":\"boxed\",\"theme\":\"default\"}", ComponentJson = BuildContactComponentsJson(), Status = 1, IsHome = false, Sort = 6, IsActive = true, IsDelete = false, PublishTime = DateTime.Now, CreationBy = "system", CreationTime = DateTime.Now }
            };
            context.WebsitePage.AddRange(pages);
            context.SaveChanges();

            foreach (var page in pages)
            {
                context.WebsitePageVersion.Add(new WebsitePageVersion
                {
                    PageId = page.Id,
                    VersionNo = 1,
                    DraftJson = page.ComponentJson,
                    PublishJson = page.ComponentJson,
                    Status = 1,
                    PublishTime = DateTime.Now,
                    CreateUserId = 1,
                    CreateUserName = "system",
                    CreationTime = DateTime.Now
                });
            }
            context.SaveChanges();
        }

        private string BuildHomeComponentsJson()
        {
            return "["
                + "{\"id\":\"nav_001\",\"type\":\"navigation\",\"name\":\"顶部导航\",\"sort\":1,\"visible\":true,\"locked\":false,\"props\":{},\"style\":{}},"
                + "{\"id\":\"banner_001\",\"type\":\"banner\",\"name\":\"首页Banner\",\"sort\":2,\"visible\":true,\"locked\":false,\"props\":{\"height\":520,\"autoplay\":true,\"interval\":5000,\"items\":[{\"title\":\"专注企业数字化建设\",\"subtitle\":\"为企业提供专业的网站建设与数字化解决方案\",\"image\":\"\",\"buttonText\":\"了解更多\",\"buttonLink\":\"/about\"}]},\"style\":{\"backgroundColor\":\"#ffffff\"}},"
                + "{\"id\":\"intro_001\",\"type\":\"richText\",\"name\":\"公司简介\",\"sort\":3,\"visible\":true,\"locked\":false,\"props\":{\"html\":\"<div style='text-align:center;padding:40px 20px'><h2>关于我们</h2><p>专注企业数字化建设，提供专业的网站建设与数字化解决方案，助力企业数字化转型。</p></div>\"},\"style\":{}},"
                + "{\"id\":\"product_001\",\"type\":\"product\",\"name\":\"产品中心\",\"sort\":4,\"visible\":true,\"locked\":false,\"props\":{\"categoryId\":0,\"pageSize\":8,\"showStyle\":\"grid\",\"showImage\":true,\"showSummary\":true,\"showMore\":true,\"moreLink\":\"/products\",\"columns\":4},\"style\":{}},"
                + "{\"id\":\"news_001\",\"type\":\"news\",\"name\":\"新闻中心\",\"sort\":5,\"visible\":true,\"locked\":false,\"props\":{\"categoryId\":0,\"pageSize\":6,\"showStyle\":\"list\",\"showCover\":true,\"showSummary\":true,\"showDate\":true,\"showMore\":true,\"moreLink\":\"/news\"},\"style\":{}},"
                + "{\"id\":\"job_001\",\"type\":\"job\",\"name\":\"招聘信息\",\"sort\":6,\"visible\":true,\"locked\":false,\"props\":{\"pageSize\":5,\"showLocation\":true,\"showSalary\":true,\"showCount\":true,\"showDate\":true,\"showApply\":false},\"style\":{}},"
                + "{\"id\":\"footer_001\",\"type\":\"footer\",\"name\":\"页脚\",\"sort\":99,\"visible\":true,\"locked\":false,\"props\":{},\"style\":{}}"
                + "]";
        }

        private string BuildAboutComponentsJson()
        {
            return "["
                + "{\"id\":\"nav_001\",\"type\":\"navigation\",\"name\":\"顶部导航\",\"sort\":1,\"visible\":true,\"locked\":false,\"props\":{},\"style\":{}},"
                + "{\"id\":\"banner_001\",\"type\":\"banner\",\"name\":\"Banner\",\"sort\":2,\"visible\":true,\"locked\":false,\"props\":{\"height\":300,\"autoplay\":false,\"items\":[{\"title\":\"关于我们\",\"subtitle\":\"\",\"image\":\"\",\"buttonText\":\"\",\"buttonLink\":\"\"}]},\"style\":{}},"
                + "{\"id\":\"intro_001\",\"type\":\"richText\",\"name\":\"公司简介\",\"sort\":3,\"visible\":true,\"locked\":false,\"props\":{\"html\":\"<div style='padding:40px 20px'><h2>公司简介</h2><p>我们是一家专注于企业数字化建设的公司，致力于为客户提供专业的网站建设与数字化解决方案。</p></div>\"},\"style\":{}},"
                + "{\"id\":\"footer_001\",\"type\":\"footer\",\"name\":\"页脚\",\"sort\":99,\"visible\":true,\"locked\":false,\"props\":{},\"style\":{}}"
                + "]";
        }

        private string BuildProductsComponentsJson()
        {
            return "["
                + "{\"id\":\"nav_001\",\"type\":\"navigation\",\"name\":\"顶部导航\",\"sort\":1,\"visible\":true,\"locked\":false,\"props\":{},\"style\":{}},"
                + "{\"id\":\"banner_001\",\"type\":\"banner\",\"name\":\"Banner\",\"sort\":2,\"visible\":true,\"locked\":false,\"props\":{\"height\":300,\"autoplay\":false,\"items\":[{\"title\":\"产品中心\",\"subtitle\":\"\",\"image\":\"\",\"buttonText\":\"\",\"buttonLink\":\"\"}]},\"style\":{}},"
                + "{\"id\":\"product_001\",\"type\":\"product\",\"name\":\"产品列表\",\"sort\":3,\"visible\":true,\"locked\":false,\"props\":{\"categoryId\":0,\"pageSize\":12,\"showStyle\":\"grid\",\"showImage\":true,\"showSummary\":true,\"showMore\":false,\"moreLink\":\"\",\"columns\":3},\"style\":{}},"
                + "{\"id\":\"footer_001\",\"type\":\"footer\",\"name\":\"页脚\",\"sort\":99,\"visible\":true,\"locked\":false,\"props\":{},\"style\":{}}"
                + "]";
        }

        private string BuildNewsComponentsJson()
        {
            return "["
                + "{\"id\":\"nav_001\",\"type\":\"navigation\",\"name\":\"顶部导航\",\"sort\":1,\"visible\":true,\"locked\":false,\"props\":{},\"style\":{}},"
                + "{\"id\":\"banner_001\",\"type\":\"banner\",\"name\":\"Banner\",\"sort\":2,\"visible\":true,\"locked\":false,\"props\":{\"height\":300,\"autoplay\":false,\"items\":[{\"title\":\"新闻中心\",\"subtitle\":\"\",\"image\":\"\",\"buttonText\":\"\",\"buttonLink\":\"\"}]},\"style\":{}},"
                + "{\"id\":\"news_001\",\"type\":\"news\",\"name\":\"新闻列表\",\"sort\":3,\"visible\":true,\"locked\":false,\"props\":{\"categoryId\":0,\"pageSize\":10,\"showStyle\":\"list\",\"showCover\":true,\"showSummary\":true,\"showDate\":true,\"showMore\":false,\"moreLink\":\"\"},\"style\":{}},"
                + "{\"id\":\"footer_001\",\"type\":\"footer\",\"name\":\"页脚\",\"sort\":99,\"visible\":true,\"locked\":false,\"props\":{},\"style\":{}}"
                + "]";
        }

        private string BuildJobsComponentsJson()
        {
            return "["
                + "{\"id\":\"nav_001\",\"type\":\"navigation\",\"name\":\"顶部导航\",\"sort\":1,\"visible\":true,\"locked\":false,\"props\":{},\"style\":{}},"
                + "{\"id\":\"banner_001\",\"type\":\"banner\",\"name\":\"Banner\",\"sort\":2,\"visible\":true,\"locked\":false,\"props\":{\"height\":300,\"autoplay\":false,\"items\":[{\"title\":\"招聘中心\",\"subtitle\":\"\",\"image\":\"\",\"buttonText\":\"\",\"buttonLink\":\"\"}]},\"style\":{}},"
                + "{\"id\":\"job_001\",\"type\":\"job\",\"name\":\"招聘列表\",\"sort\":3,\"visible\":true,\"locked\":false,\"props\":{\"pageSize\":10,\"showLocation\":true,\"showSalary\":true,\"showCount\":true,\"showDate\":true,\"showApply\":true},\"style\":{}},"
                + "{\"id\":\"footer_001\",\"type\":\"footer\",\"name\":\"页脚\",\"sort\":99,\"visible\":true,\"locked\":false,\"props\":{},\"style\":{}}"
                + "]";
        }

        private string BuildContactComponentsJson()
        {
            return "["
                + "{\"id\":\"nav_001\",\"type\":\"navigation\",\"name\":\"顶部导航\",\"sort\":1,\"visible\":true,\"locked\":false,\"props\":{},\"style\":{}},"
                + "{\"id\":\"banner_001\",\"type\":\"banner\",\"name\":\"Banner\",\"sort\":2,\"visible\":true,\"locked\":false,\"props\":{\"height\":300,\"autoplay\":false,\"items\":[{\"title\":\"联系我们\",\"subtitle\":\"\",\"image\":\"\",\"buttonText\":\"\",\"buttonLink\":\"\"}]},\"style\":{}},"
                + "{\"id\":\"contact_001\",\"type\":\"richText\",\"name\":\"联系方式\",\"sort\":3,\"visible\":true,\"locked\":false,\"props\":{\"html\":\"<div style='padding:40px 20px'><h2>联系方式</h2><p>电话：400-000-0000</p><p>邮箱：contact@example.com</p><p>地址：中国</p></div>\"},\"style\":{}},"
                + "{\"id\":\"footer_001\",\"type\":\"footer\",\"name\":\"页脚\",\"sort\":99,\"visible\":true,\"locked\":false,\"props\":{},\"style\":{}}"
                + "]";
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
