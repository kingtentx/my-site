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
            // ===== 已有种子（保持不变）=====
            InitUser(context);
            InitMenu(context);
            InitArticles(context);
            InitArticlesFallback(context);
            RepairSiteAssetPaths(context);
            InitSiteConfig(context);
            InitFooter(context);
            InitNavigation(context);
            InitSitePages(context);

            // ===== 新增种子（按依赖顺序）=====
            InitRoles(context);
            InitRoleMenus(context);
            InitProductCategories(context);
            InitProducts(context);
            InitJobs(context);
            InitImages(context);
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

        private void InitArticlesFallback(AppDbContext context)
        {
            if (context.Article.Any())
            {
                return;
            }

            var fallbackImage = "/upload/image/202607/cb3f48bad9b145eb9615eccd8984b85b.jpg";

            var articles = new List<Article>
            {
                new Article
                {
                    Title = "公司荣获 2026 年度企业数字化服务大奖",
                    Keyword = "企业荣誉,数字化服务",
                    Description = "公司凭借在企业数字化领域的深厚积累与卓越服务，荣获 2026 年度企业数字化服务大奖。",
                    Detail = "<p>近日，2026 年度企业数字化服务评选结果揭晓，公司凭借在企业数字化建设领域的深厚积累与卓越服务能力，荣获年度企业数字化服务大奖。</p><p>此次获奖是对公司技术实力与服务能力的再次认可，也是对公司持续创新的肯定。</p>",
                    Author = "市场部",
                    Source = "企业官网",
                    SourceUrl = "",
                    ImageUrl = fallbackImage,
                    TagType = 1,
                    TagId = 0,
                    Sort = 1,
                    ViewCount = 568,
                    ShareCount = 24,
                    IsHot = true,
                    IsActive = true,
                    IsDelete = false,
                    CreationBy = "system",
                    CreationTime = DateTime.Now.AddDays(-5),
                    UpdateTime = DateTime.Now
                },
                new Article
                {
                    Title = "全新一代企业官网建站系统正式发布",
                    Keyword = "产品发布,建站系统",
                    Description = "公司正式发布全新一代企业官网建站系统，支持可视化拖拽装修与多端适配。",
                    Detail = "<p>经过两年的研发迭代，公司正式发布全新一代企业官网建站系统。</p><p>新版本在性能、易用性、扩展性上均有显著提升，支持可视化拖拽装修、多端适配、SEO 优化等核心能力。</p>",
                    Author = "产品中心",
                    Source = "企业官网",
                    SourceUrl = "",
                    ImageUrl = fallbackImage,
                    TagType = 1,
                    TagId = 0,
                    Sort = 2,
                    ViewCount = 432,
                    ShareCount = 18,
                    IsHot = true,
                    IsActive = true,
                    IsDelete = false,
                    CreationBy = "system",
                    CreationTime = DateTime.Now.AddDays(-10),
                    UpdateTime = DateTime.Now
                },
                new Article
                {
                    Title = "公司与某大型制造企业签署数字化战略合作协议",
                    Keyword = "战略合作,制造业",
                    Description = "公司与某大型制造企业签署数字化战略合作协议，共同推进制造业数字化转型。",
                    Detail = "<p>近日，公司与某大型制造企业正式签署数字化战略合作协议。</p><p>双方将在智能制造、工业互联网等领域展开深度合作，共同推进制造业数字化转型。</p>",
                    Author = "市场部",
                    Source = "企业官网",
                    SourceUrl = "",
                    ImageUrl = fallbackImage,
                    TagType = 1,
                    TagId = 0,
                    Sort = 3,
                    ViewCount = 320,
                    ShareCount = 12,
                    IsHot = false,
                    IsActive = true,
                    IsDelete = false,
                    CreationBy = "system",
                    CreationTime = DateTime.Now.AddDays(-15),
                    UpdateTime = DateTime.Now
                },
                new Article
                {
                    Title = "公司受邀参加 2026 中国 SaaS 行业峰会",
                    Keyword = "行业活动,SaaS",
                    Description = "公司受邀参加 2026 中国 SaaS 行业峰会，分享企业服务领域实践。",
                    Detail = "<p>2026 中国 SaaS 行业峰会近日在上海举办，公司作为受邀企业代表出席。</p><p>公司 CTO 在会上作了主题演讲，分享了公司企业服务领域的实践案例。</p>",
                    Author = "市场部",
                    Source = "企业官网",
                    SourceUrl = "",
                    ImageUrl = fallbackImage,
                    TagType = 1,
                    TagId = 0,
                    Sort = 4,
                    ViewCount = 256,
                    ShareCount = 8,
                    IsHot = false,
                    IsActive = true,
                    IsDelete = false,
                    CreationBy = "system",
                    CreationTime = DateTime.Now.AddDays(-20),
                    UpdateTime = DateTime.Now
                },
                new Article
                {
                    Title = "公司启动 2026 校园招聘计划",
                    Keyword = "校园招聘,人才",
                    Description = "公司启动 2026 校园招聘计划，面向应届毕业生开放多个岗位。",
                    Detail = "<p>为持续吸引优秀人才，公司正式启动 2026 校园招聘计划。</p><p>本次招聘覆盖研发、产品、设计等多个方向，欢迎应届毕业生关注公司招聘官网。</p>",
                    Author = "人力资源部",
                    Source = "企业官网",
                    SourceUrl = "",
                    ImageUrl = fallbackImage,
                    TagType = 1,
                    TagId = 0,
                    Sort = 5,
                    ViewCount = 198,
                    ShareCount = 6,
                    IsHot = false,
                    IsActive = true,
                    IsDelete = false,
                    CreationBy = "system",
                    CreationTime = DateTime.Now.AddDays(-25),
                    UpdateTime = DateTime.Now
                },
                new Article
                {
                    Title = "公司通过 CMMI5 级认证",
                    Keyword = "资质认证,CMMI",
                    Description = "公司顺利通过 CMMI5 级软件能力成熟度认证，标志软件研发能力达国际顶尖水平。",
                    Detail = "<p>近日，公司正式通过 CMMI5 级软件能力成熟度模型集成认证。</p><p>这是公司继 ISO9001 之后的又一项重要资质，标志公司软件研发能力达到国际顶尖水平。</p>",
                    Author = "质量部",
                    Source = "企业官网",
                    SourceUrl = "",
                    ImageUrl = fallbackImage,
                    TagType = 1,
                    TagId = 0,
                    Sort = 6,
                    ViewCount = 142,
                    ShareCount = 4,
                    IsHot = false,
                    IsActive = true,
                    IsDelete = false,
                    CreationBy = "system",
                    CreationTime = DateTime.Now.AddDays(-30),
                    UpdateTime = DateTime.Now
                }
            };

            context.Article.AddRange(articles);
            context.SaveChanges();
        }

        private void InitRoles(AppDbContext context)
        {
            if (context.Role.Any(p => p.RoleName == "内容编辑" || p.RoleName == "内容审核" || p.RoleName == "运营管理员"))
            {
                return;
            }

            var roles = new List<Role>
            {
                new Role
                {
                    RoleName = "内容编辑",
                    Description = "负责新闻、产品、招聘内容的录入与维护",
                    IsActive = true,
                    RoleType = 0,
                    CreationTime = DateTime.Now
                },
                new Role
                {
                    RoleName = "内容审核",
                    Description = "负责内容审核与发布，仅查看权限",
                    IsActive = true,
                    RoleType = 0,
                    CreationTime = DateTime.Now
                },
                new Role
                {
                    RoleName = "运营管理员",
                    Description = "负责网站内容与站点配置的运营管理",
                    IsActive = true,
                    RoleType = 0,
                    CreationTime = DateTime.Now
                }
            };

            context.Role.AddRange(roles);
            context.SaveChanges();
        }

        private void InitRoleMenus(AppDbContext context)
        {
            var editorRole = context.Role.FirstOrDefault(p => p.RoleName == "内容编辑");
            var auditRole = context.Role.FirstOrDefault(p => p.RoleName == "内容审核");
            var opsRole = context.Role.FirstOrDefault(p => p.RoleName == "运营管理员");
            if (editorRole == null && auditRole == null && opsRole == null)
            {
                return;
            }

            var roleIds = new[] { editorRole?.Id, auditRole?.Id, opsRole?.Id }
                .Where(i => i.HasValue).Select(i => i.Value).ToList();
            if (roleIds.Any() && context.RoleMenu.Any(p => roleIds.Contains(p.RoleId)))
            {
                return;
            }

            var list = new List<RoleMenu>();

            if (editorRole != null)
            {
                var editorPermissions = new[]
                {
                    "Content",
                    "Content_Article", "Content_Article_Add", "Content_Article_Edit", "Content_Article_Delete",
                    "Content_ProductCategory", "Content_ProductCategory_Add", "Content_ProductCategory_Edit", "Content_ProductCategory_Delete",
                    "Content_Product", "Content_Product_Add", "Content_Product_Edit", "Content_Product_Delete",
                    "Content_Job", "Content_Job_Add", "Content_Job_Edit", "Content_Job_Delete",
                    "Content_Images", "Content_Images_Add", "Content_Images_Edit", "Content_Images_Delete"
                };
                foreach (var perm in editorPermissions)
                {
                    list.Add(new RoleMenu { RoleId = editorRole.Id, Permission = perm, CreationTime = DateTime.Now });
                }
            }

            if (auditRole != null)
            {
                var auditPermissions = new[]
                {
                    "Content",
                    "Content_Article",
                    "Content_ProductCategory",
                    "Content_Product",
                    "Content_Job",
                    "Content_Images"
                };
                foreach (var perm in auditPermissions)
                {
                    list.Add(new RoleMenu { RoleId = auditRole.Id, Permission = perm, CreationTime = DateTime.Now });
                }
            }

            if (opsRole != null)
            {
                var opsPermissions = new[]
                {
                    "Content",
                    "Content_Article", "Content_Article_Add", "Content_Article_Edit", "Content_Article_Delete",
                    "Content_ProductCategory", "Content_ProductCategory_Add", "Content_ProductCategory_Edit", "Content_ProductCategory_Delete",
                    "Content_Product", "Content_Product_Add", "Content_Product_Edit", "Content_Product_Delete",
                    "Content_Job", "Content_Job_Add", "Content_Job_Edit", "Content_Job_Delete",
                    "Content_Images", "Content_Images_Add", "Content_Images_Edit", "Content_Images_Delete",
                    "Site",
                    "Site_Info", "Site_Info_Edit",
                    "Website_Page", "Website_Page_Add", "Website_Page_Edit", "Website_Page_Delete", "Website_Page_Design", "Website_Page_Publish",
                    "Site_Navigation", "Site_Navigation_Add", "Site_Navigation_Edit", "Site_Navigation_Delete",
                    "Site_Footer", "Site_Footer_Edit"
                };
                foreach (var perm in opsPermissions)
                {
                    list.Add(new RoleMenu { RoleId = opsRole.Id, Permission = perm, CreationTime = DateTime.Now });
                }
            }

            if (list.Any())
            {
                context.RoleMenu.AddRange(list);
                context.SaveChanges();
            }
        }

        private void InitProductCategories(AppDbContext context)
        {
            if (context.ContentProductCategory.Any(p => !p.IsDelete))
            {
                return;
            }

            var topCategories = new List<ContentProductCategory>
            {
                new ContentProductCategory { Pid = 0, Name = "智能硬件", Sort = 1, IsActive = true, IsDelete = false, CreationBy = "system", CreationTime = DateTime.Now },
                new ContentProductCategory { Pid = 0, Name = "软件应用", Sort = 2, IsActive = true, IsDelete = false, CreationBy = "system", CreationTime = DateTime.Now },
                new ContentProductCategory { Pid = 0, Name = "云服务", Sort = 3, IsActive = true, IsDelete = false, CreationBy = "system", CreationTime = DateTime.Now },
                new ContentProductCategory { Pid = 0, Name = "数字化咨询", Sort = 4, IsActive = true, IsDelete = false, CreationBy = "system", CreationTime = DateTime.Now }
            };
            context.ContentProductCategory.AddRange(topCategories);
            context.SaveChanges();

            var hardwareId = topCategories.First(c => c.Name == "智能硬件").Id;
            var softwareId = topCategories.First(c => c.Name == "软件应用").Id;
            var cloudId = topCategories.First(c => c.Name == "云服务").Id;

            var subCategories = new List<ContentProductCategory>
            {
                new ContentProductCategory { Pid = hardwareId, Name = "智能终端", Sort = 1, IsActive = true, IsDelete = false, CreationBy = "system", CreationTime = DateTime.Now },
                new ContentProductCategory { Pid = hardwareId, Name = "物联网设备", Sort = 2, IsActive = true, IsDelete = false, CreationBy = "system", CreationTime = DateTime.Now },
                new ContentProductCategory { Pid = softwareId, Name = "企业管理软件", Sort = 1, IsActive = true, IsDelete = false, CreationBy = "system", CreationTime = DateTime.Now },
                new ContentProductCategory { Pid = softwareId, Name = "行业解决方案", Sort = 2, IsActive = true, IsDelete = false, CreationBy = "system", CreationTime = DateTime.Now },
                new ContentProductCategory { Pid = cloudId, Name = "云部署服务", Sort = 1, IsActive = true, IsDelete = false, CreationBy = "system", CreationTime = DateTime.Now }
            };
            context.ContentProductCategory.AddRange(subCategories);
            context.SaveChanges();
        }

        private void InitProducts(AppDbContext context)
        {
            if (context.ContentProduct.Any(p => !p.IsDelete))
            {
                return;
            }

            var categories = context.ContentProductCategory.Where(p => !p.IsDelete && p.IsActive).ToList();
            if (categories.Count == 0)
            {
                return;
            }

            int Cat(string name) => categories.FirstOrDefault(c => c.Name == name)?.Id ?? categories.First().Id;
            var cover1 = "/upload/image/202607/cb3f48bad9b145eb9615eccd8984b85b.jpg";
            var cover2 = "/upload/image/202607/52ad28ed82c34e04b84dfe6864f4d6f1.png";

            var products = new List<ContentProduct>
            {
                new ContentProduct
                {
                    ProductName = "企业官网建站系统",
                    CategoryId = Cat("企业管理软件"),
                    CoverImage = cover1,
                    ImageList = "[\"" + cover1 + "\",\"" + cover2 + "\"]",
                    Summary = "一站式企业官网建站解决方案，支持可视化拖拽装修、多端适配、SEO 优化，助力企业快速建立品牌门户。",
                    Description = "<p>企业官网建站系统提供可视化页面装修、多模板选择、多端适配能力。</p><p>支持自定义组件、SEO 优化、表单收集等核心功能，帮助企业快速建立专业品牌门户。</p>",
                    Specification = "<table><tr><th>版本</th><td>企业版</td></tr><tr><th>并发</th><td>1000 QPS</td></tr><tr><th>存储</th><td>500GB</td></tr></table>",
                    Feature = "可视化装修；多端适配；SEO 优化；表单收集",
                    Sort = 1, IsRecommend = true, IsActive = true, ViewCount = 1280,
                    IsDelete = false, CreationBy = "system", CreationTime = DateTime.Now.AddDays(-30), UpdateTime = DateTime.Now
                },
                new ContentProduct
                {
                    ProductName = "电商小程序平台",
                    CategoryId = Cat("企业管理软件"),
                    CoverImage = cover2,
                    ImageList = "[\"" + cover2 + "\"]",
                    Summary = "面向零售企业的电商小程序解决方案，覆盖商品、订单、会员、营销全链路。",
                    Description = "<p>电商小程序平台提供商品管理、订单履约、会员体系、营销活动等完整能力。</p><p>支持多门店、拼团秒杀、积分会员等电商核心场景。</p>",
                    Specification = "<ul><li>支持微信小程序</li><li>支持多门店</li><li>支持拼团/秒杀</li></ul>",
                    Feature = "多端同步；营销插件丰富；支付能力完善",
                    Sort = 2, IsRecommend = true, IsActive = true, ViewCount = 980,
                    IsDelete = false, CreationBy = "system", CreationTime = DateTime.Now.AddDays(-28), UpdateTime = DateTime.Now
                },
                new ContentProduct
                {
                    ProductName = "ERP 企业资源管理系统",
                    CategoryId = Cat("企业管理软件"),
                    CoverImage = cover1,
                    ImageList = "[]",
                    Summary = "整合采购、库存、销售、财务的全流程 ERP 系统，提升企业运营效率。",
                    Description = "<p>ERP 系统覆盖采购管理、库存管理、销售管理、财务管理四大核心模块。</p><p>支持多组织架构、多币种、多税率为企业提供一体化资源管理能力。</p>",
                    Specification = "<p>模块：采购/库存/销售/财务/报表</p>",
                    Feature = "全流程贯通；多组织支持；财务对接",
                    Sort = 3, IsRecommend = false, IsActive = true, ViewCount = 654,
                    IsDelete = false, CreationBy = "system", CreationTime = DateTime.Now.AddDays(-25), UpdateTime = DateTime.Now
                },
                new ContentProduct
                {
                    ProductName = "智能客服平台",
                    CategoryId = Cat("行业解决方案"),
                    CoverImage = cover2,
                    ImageList = "[\"" + cover1 + "\"]",
                    Summary = "AI 驱动的智能客服平台，支持多渠道接入、智能问答、工单流转。",
                    Description = "<p>智能客服平台集成自然语言处理能力，支持网站、App、微信多渠道统一接待。</p><p>提供智能问答、工单流转、坐席管理、数据分析等核心能力。</p>",
                    Specification = "<p>渠道：Web/App/微信/电话</p>",
                    Feature = "AI 智能问答；多渠道接入；工单流转",
                    Sort = 1, IsRecommend = true, IsActive = true, ViewCount = 1120,
                    IsDelete = false, CreationBy = "system", CreationTime = DateTime.Now.AddDays(-20), UpdateTime = DateTime.Now
                },
                new ContentProduct
                {
                    ProductName = "工业物联网网关",
                    CategoryId = Cat("物联网设备"),
                    CoverImage = cover1,
                    ImageList = "[]",
                    Summary = "工业级物联网网关，支持多协议接入、边缘计算、云端同步。",
                    Description = "<p>工业物联网网关支持 Modbus、OPC UA、MQTT 等主流工业协议。</p><p>具备边缘计算能力，可在网络中断时本地存储数据，恢复后自动同步至云端。</p>",
                    Specification = "<p>接口：RS485/Ethernet/4G</p>",
                    Feature = "多协议接入；边缘计算；远程运维",
                    Sort = 1, IsRecommend = false, IsActive = true, ViewCount = 432,
                    IsDelete = false, CreationBy = "system", CreationTime = DateTime.Now.AddDays(-18), UpdateTime = DateTime.Now
                },
                new ContentProduct
                {
                    ProductName = "智能自助终端",
                    CategoryId = Cat("智能终端"),
                    CoverImage = cover2,
                    ImageList = "[\"" + cover1 + "\"]",
                    Summary = "面向政务、零售、医疗等场景的自助服务终端，支持定制化开发。",
                    Description = "<p>智能自助终端提供 24 小时无人值守服务能力，支持触摸交互、人脸识别、打印等。</p><p>已广泛应用于政务大厅、零售门店、医院等场景。</p>",
                    Specification = "<p>屏幕：21.5寸/32寸可选</p>",
                    Feature = "工业级设计；模块化扩展；远程管理",
                    Sort = 2, IsRecommend = true, IsActive = true, ViewCount = 760,
                    IsDelete = false, CreationBy = "system", CreationTime = DateTime.Now.AddDays(-15), UpdateTime = DateTime.Now
                },
                new ContentProduct
                {
                    ProductName = "云原生部署服务",
                    CategoryId = Cat("云部署服务"),
                    CoverImage = cover1,
                    ImageList = "[]",
                    Summary = "基于 Kubernetes 的云原生部署与运维服务，支持混合云、多集群管理。",
                    Description = "<p>云原生部署服务提供从架构设计、容器化改造到运维监控的全生命周期支持。</p><p>支持灰度发布、弹性伸缩、全链路监控，保障业务稳定运行。</p>",
                    Specification = "<p>支持：K8s/Istio/Prometheus</p>",
                    Feature = "弹性伸缩；灰度发布；全链路监控",
                    Sort = 1, IsRecommend = true, IsActive = true, ViewCount = 890,
                    IsDelete = false, CreationBy = "system", CreationTime = DateTime.Now.AddDays(-10), UpdateTime = DateTime.Now
                },
                new ContentProduct
                {
                    ProductName = "数字化战略咨询服务",
                    CategoryId = Cat("数字化咨询"),
                    CoverImage = cover2,
                    ImageList = "[]",
                    Summary = "为企业提供数字化战略规划、业务流程梳理、技术架构设计的端到端咨询服务。",
                    Description = "<p>数字化战略咨询服务由资深顾问团队提供，覆盖业务诊断、流程优化、技术选型。</p><p>帮助企业制定可落地的数字化战略，避免盲目投入与重复建设。</p>",
                    Specification = "<p>交付物：战略规划报告/架构蓝图/实施路线图</p>",
                    Feature = "资深顾问团队；行业方法论；可落地实施",
                    Sort = 1, IsRecommend = false, IsActive = true, ViewCount = 320,
                    IsDelete = false, CreationBy = "system", CreationTime = DateTime.Now.AddDays(-5), UpdateTime = DateTime.Now
                }
            };

            context.ContentProduct.AddRange(products);
            context.SaveChanges();
        }

        private void InitJobs(AppDbContext context)
        {
            if (context.ContentJob.Any(p => !p.IsDelete))
            {
                return;
            }

            var jobs = new List<ContentJob>
            {
                new ContentJob
                {
                    JobTitle = "高级 .NET 开发工程师",
                    Department = "研发中心",
                    WorkLocation = "上海",
                    SalaryRange = "25k-40k",
                    RecruitCount = 2,
                    JobType = "全职",
                    Responsibilities = "<p>1. 负责核心业务系统的设计与开发；</p><p>2. 参与系统架构设计和技术方案评审；</p><p>3. 解决线上技术问题，保障系统稳定运行。</p>",
                    Requirements = "<p>1. 5 年以上 .NET 开发经验，熟悉 .NET Core/8.0；</p><p>2. 熟悉 EF Core、MySQL、Redis；</p><p>3. 具备良好的架构设计和问题解决能力。</p>",
                    ContactName = "张女士",
                    ContactPhone = "021-88888888",
                    ContactEmail = "hr@example.com",
                    Sort = 1, IsActive = true, PublishTime = DateTime.Now.AddDays(-7),
                    IsDelete = false, CreationBy = "system", CreationTime = DateTime.Now.AddDays(-7), UpdateTime = DateTime.Now
                },
                new ContentJob
                {
                    JobTitle = "前端工程师",
                    Department = "研发中心",
                    WorkLocation = "上海",
                    SalaryRange = "18k-30k",
                    RecruitCount = 3,
                    JobType = "全职",
                    Responsibilities = "<p>1. 负责 Web 前端开发与维护；</p><p>2. 与后端协作完成产品功能；</p><p>3. 优化前端性能与用户体验。</p>",
                    Requirements = "<p>1. 3 年以上前端开发经验；</p><p>2. 精通 Vue 或 React；</p><p>3. 熟悉 TypeScript、Webpack。</p>",
                    ContactName = "张女士",
                    ContactPhone = "021-88888888",
                    ContactEmail = "hr@example.com",
                    Sort = 2, IsActive = true, PublishTime = DateTime.Now.AddDays(-6),
                    IsDelete = false, CreationBy = "system", CreationTime = DateTime.Now.AddDays(-6), UpdateTime = DateTime.Now
                },
                new ContentJob
                {
                    JobTitle = "产品经理",
                    Department = "产品中心",
                    WorkLocation = "上海",
                    SalaryRange = "20k-35k",
                    RecruitCount = 1,
                    JobType = "全职",
                    Responsibilities = "<p>1. 负责 SaaS 产品的规划与设计；</p><p>2. 收集用户需求，输出 PRD；</p><p>3. 推动产品迭代与上线。</p>",
                    Requirements = "<p>1. 3 年以上 B 端产品经验；</p><p>2. 熟悉企业服务领域；</p><p>3. 具备数据驱动思维。</p>",
                    ContactName = "李先生",
                    ContactPhone = "021-88888888",
                    ContactEmail = "hr@example.com",
                    Sort = 3, IsActive = true, PublishTime = DateTime.Now.AddDays(-5),
                    IsDelete = false, CreationBy = "system", CreationTime = DateTime.Now.AddDays(-5), UpdateTime = DateTime.Now
                },
                new ContentJob
                {
                    JobTitle = "UI 设计师",
                    Department = "设计中心",
                    WorkLocation = "上海",
                    SalaryRange = "15k-25k",
                    RecruitCount = 2,
                    JobType = "全职",
                    Responsibilities = "<p>1. 负责 Web/App 产品的视觉设计；</p><p>2. 制定设计规范并持续优化；</p><p>3. 配合前端实现设计还原。</p>",
                    Requirements = "<p>1. 3 年以上 UI 设计经验；</p><p>2. 精通 Figma/Sketch；</p><p>3. 有 B 端产品经验优先。</p>",
                    ContactName = "李先生",
                    ContactPhone = "021-88888888",
                    ContactEmail = "hr@example.com",
                    Sort = 4, IsActive = true, PublishTime = DateTime.Now.AddDays(-4),
                    IsDelete = false, CreationBy = "system", CreationTime = DateTime.Now.AddDays(-4), UpdateTime = DateTime.Now
                },
                new ContentJob
                {
                    JobTitle = "销售经理",
                    Department = "市场部",
                    WorkLocation = "北京",
                    SalaryRange = "12k-20k+提成",
                    RecruitCount = 3,
                    JobType = "全职",
                    Responsibilities = "<p>1. 负责区域客户开发与维护；</p><p>2. 完成销售业绩目标；</p><p>3. 收集市场反馈。</p>",
                    Requirements = "<p>1. 3 年以上软件销售经验；</p><p>2. 熟悉企业服务市场；</p><p>3. 较强的沟通能力。</p>",
                    ContactName = "王女士",
                    ContactPhone = "010-66666666",
                    ContactEmail = "sales@example.com",
                    Sort = 5, IsActive = true, PublishTime = DateTime.Now.AddDays(-3),
                    IsDelete = false, CreationBy = "system", CreationTime = DateTime.Now.AddDays(-3), UpdateTime = DateTime.Now
                },
                new ContentJob
                {
                    JobTitle = "实习生（前端方向）",
                    Department = "研发中心",
                    WorkLocation = "上海",
                    SalaryRange = "200元/天",
                    RecruitCount = 2,
                    JobType = "实习",
                    Responsibilities = "<p>1. 协助前端组件开发；</p><p>2. 参与代码 review；</p><p>3. 学习生产环境最佳实践。</p>",
                    Requirements = "<p>1. 在校本科或研究生；</p><p>2. 熟悉 HTML/CSS/JS；</p><p>3. 每周实习 4 天以上。</p>",
                    ContactName = "张女士",
                    ContactPhone = "021-88888888",
                    ContactEmail = "hr@example.com",
                    Sort = 6, IsActive = true, PublishTime = DateTime.Now.AddDays(-2),
                    IsDelete = false, CreationBy = "system", CreationTime = DateTime.Now.AddDays(-2), UpdateTime = DateTime.Now
                }
            };

            context.ContentJob.AddRange(jobs);
            context.SaveChanges();
        }

        private void InitImages(AppDbContext context)
        {
            if (context.Images.Any())
            {
                return;
            }

            var images = new List<Images>
            {
                new Images { FileName = "首页Banner",      Url = "/uploads/seed/banner-home.jpg",         ExtensionName = ".jpg", Size = 524288, CreationBy = "system", CreationTime = DateTime.Now.AddDays(-10) },
                new Images { FileName = "关于我们配图",    Url = "/uploads/seed/about-us.jpg",            ExtensionName = ".jpg", Size = 312640, CreationBy = "system", CreationTime = DateTime.Now.AddDays(-9) },
                new Images { FileName = "产品封面图-官网", Url = "/uploads/seed/product-website.jpg",    ExtensionName = ".jpg", Size = 204800, CreationBy = "system", CreationTime = DateTime.Now.AddDays(-8) },
                new Images { FileName = "产品封面图-电商", Url = "/uploads/seed/product-ecommerce.jpg",  ExtensionName = ".jpg", Size = 215040, CreationBy = "system", CreationTime = DateTime.Now.AddDays(-8) },
                new Images { FileName = "新闻配图",        Url = "/uploads/seed/news-cover.jpg",          ExtensionName = ".jpg", Size = 153600, CreationBy = "system", CreationTime = DateTime.Now.AddDays(-7) },
                new Images { FileName = "招聘海报",        Url = "/uploads/seed/jobs-poster.jpg",         ExtensionName = ".jpg", Size = 614400, CreationBy = "system", CreationTime = DateTime.Now.AddDays(-6) },
                new Images { FileName = "二维码",          Url = "/uploads/seed/qrcode.png",              ExtensionName = ".png", Size = 10240,  CreationBy = "system", CreationTime = DateTime.Now.AddDays(-5) },
                new Images { FileName = "公司Logo",        Url = "/uploads/seed/logo.png",                ExtensionName = ".png", Size = 8192,   CreationBy = "system", CreationTime = DateTime.Now.AddDays(-4) }
            };

            context.Images.AddRange(images);
            context.SaveChanges();
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
