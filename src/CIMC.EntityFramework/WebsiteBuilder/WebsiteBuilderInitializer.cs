using System;
using System.Linq;
using CIMC.Data.Entities.WebsiteBuilder;
using Microsoft.EntityFrameworkCore;

namespace CIMC.EntityFramework.WebsiteBuilder
{
    public class WebsiteBuilderInitializer
    {
        public void Create(WebsiteBuilderDbContext db)
        {
            CreateTables(db);
            SeedDefaults(db);
        }

        private static void CreateTables(WebsiteBuilderDbContext db)
        {
            var sqlList = new[]
            {
                @"CREATE TABLE IF NOT EXISTS `website_site_config` (`Id` int NOT NULL AUTO_INCREMENT,`SiteName` varchar(100) NULL,`Logo` varchar(500) NULL,`BrowserTitle` varchar(150) NULL,`Keywords` varchar(500) NULL,`Description` varchar(1000) NULL,`IcpNo` varchar(100) NULL,`PoliceNo` varchar(100) NULL,`Tel` varchar(50) NULL,`Email` varchar(100) NULL,`Address` varchar(300) NULL,`Copyright` varchar(300) NULL,`Theme` varchar(50) NULL,`Language` varchar(20) NULL,`IsEnabled` tinyint(1) NOT NULL DEFAULT 1,`CreateTime` datetime(6) NOT NULL,`UpdateTime` datetime(6) NOT NULL,PRIMARY KEY (`Id`)) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4;",
                @"CREATE TABLE IF NOT EXISTS `website_page` (`Id` int NOT NULL AUTO_INCREMENT,`SiteId` int NOT NULL DEFAULT 1,`PageName` varchar(100) NOT NULL,`PageCode` varchar(100) NOT NULL,`PagePath` varchar(200) NOT NULL,`PageTitle` varchar(150) NULL,`SeoKeywords` varchar(500) NULL,`SeoDescription` varchar(1000) NULL,`CanonicalUrl` varchar(500) NULL,`LayoutJson` longtext NULL,`ComponentJson` longtext NULL,`DraftJson` longtext NULL,`PublishJson` longtext NULL,`Status` int NOT NULL DEFAULT 0,`IsHome` tinyint(1) NOT NULL DEFAULT 0,`Sort` int NOT NULL DEFAULT 0,`IsDeleted` tinyint(1) NOT NULL DEFAULT 0,`CreateTime` datetime(6) NOT NULL,`UpdateTime` datetime(6) NOT NULL,`PublishTime` datetime(6) NULL,PRIMARY KEY (`Id`),INDEX `IX_website_page_PagePath` (`PagePath`)) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4;",
                @"CREATE TABLE IF NOT EXISTS `website_page_version` (`Id` int NOT NULL AUTO_INCREMENT,`PageId` int NOT NULL,`VersionNo` int NOT NULL,`DraftJson` longtext NULL,`PublishJson` longtext NULL,`Status` int NOT NULL,`CreateTime` datetime(6) NOT NULL,`PublishTime` datetime(6) NULL,`CreateUserId` int NULL,PRIMARY KEY (`Id`),INDEX `IX_website_page_version_PageId` (`PageId`)) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4;",
                @"CREATE TABLE IF NOT EXISTS `website_navigation` (`Id` int NOT NULL AUTO_INCREMENT,`SiteId` int NOT NULL DEFAULT 1,`ParentId` int NULL,`Title` varchar(100) NOT NULL,`LinkType` varchar(50) NULL,`PageId` int NULL,`LinkUrl` varchar(300) NULL,`Target` varchar(20) NULL,`Icon` varchar(100) NULL,`Sort` int NOT NULL DEFAULT 0,`IsEnabled` tinyint(1) NOT NULL DEFAULT 1,`IsDeleted` tinyint(1) NOT NULL DEFAULT 0,`CreateTime` datetime(6) NOT NULL,`UpdateTime` datetime(6) NOT NULL,PRIMARY KEY (`Id`),INDEX `IX_website_navigation_SiteId` (`SiteId`)) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4;",
                @"CREATE TABLE IF NOT EXISTS `website_banner` (`Id` int NOT NULL AUTO_INCREMENT,`SiteId` int NOT NULL DEFAULT 1,`Title` varchar(200) NOT NULL,`Subtitle` varchar(500) NULL,`ImageUrl` varchar(500) NULL,`VideoUrl` varchar(500) NULL,`ButtonText` varchar(100) NULL,`ButtonLink` varchar(300) NULL,`LinkUrl` varchar(300) NULL,`Height` int NOT NULL DEFAULT 520,`Sort` int NOT NULL DEFAULT 0,`AutoPlay` tinyint(1) NOT NULL DEFAULT 1,`Interval` int NOT NULL DEFAULT 5000,`IsEnabled` tinyint(1) NOT NULL DEFAULT 1,`IsDeleted` tinyint(1) NOT NULL DEFAULT 0,`BeginTime` datetime(6) NULL,`EndTime` datetime(6) NULL,`CreateTime` datetime(6) NOT NULL,`UpdateTime` datetime(6) NOT NULL,PRIMARY KEY (`Id`),INDEX `IX_website_banner_SiteId` (`SiteId`)) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4;",
                @"CREATE TABLE IF NOT EXISTS `website_footer` (`Id` int NOT NULL AUTO_INCREMENT,`SiteId` int NOT NULL DEFAULT 1,`Logo` varchar(500) NULL,`CompanyName` varchar(100) NULL,`Description` varchar(1000) NULL,`Tel` varchar(50) NULL,`Email` varchar(100) NULL,`Address` varchar(300) NULL,`QrCode` varchar(500) NULL,`IcpNo` varchar(100) NULL,`PoliceNo` varchar(100) NULL,`Copyright` varchar(300) NULL,`FriendLinksJson` longtext NULL,`BackgroundColor` varchar(50) NULL,`TextColor` varchar(50) NULL,`CreateTime` datetime(6) NOT NULL,`UpdateTime` datetime(6) NOT NULL,PRIMARY KEY (`Id`),INDEX `IX_website_footer_SiteId` (`SiteId`)) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4;",
                @"CREATE TABLE IF NOT EXISTS `content_news_category` (`Id` int NOT NULL AUTO_INCREMENT,`CategoryName` varchar(100) NOT NULL,`CategoryCode` varchar(100) NULL,`Sort` int NOT NULL DEFAULT 0,`IsEnabled` tinyint(1) NOT NULL DEFAULT 1,`CreateTime` datetime(6) NOT NULL,PRIMARY KEY (`Id`)) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4;",
                @"CREATE TABLE IF NOT EXISTS `content_news` (`Id` int NOT NULL AUTO_INCREMENT,`Title` varchar(200) NOT NULL,`CategoryId` int NULL,`CoverImage` varchar(500) NULL,`Summary` varchar(1000) NULL,`Content` longtext NULL,`Author` varchar(100) NULL,`Source` varchar(100) NULL,`Tags` varchar(500) NULL,`IsTop` tinyint(1) NOT NULL DEFAULT 0,`IsRecommend` tinyint(1) NOT NULL DEFAULT 0,`Status` int NOT NULL DEFAULT 0,`ViewCount` int NOT NULL DEFAULT 0,`PublishTime` datetime(6) NULL,`CreateTime` datetime(6) NOT NULL,`UpdateTime` datetime(6) NOT NULL,`IsDeleted` tinyint(1) NOT NULL DEFAULT 0,PRIMARY KEY (`Id`)) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4;",
                @"CREATE TABLE IF NOT EXISTS `content_product_category` (`Id` int NOT NULL AUTO_INCREMENT,`CategoryName` varchar(100) NOT NULL,`CategoryCode` varchar(100) NULL,`Sort` int NOT NULL DEFAULT 0,`IsEnabled` tinyint(1) NOT NULL DEFAULT 1,`CreateTime` datetime(6) NOT NULL,PRIMARY KEY (`Id`)) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4;",
                @"CREATE TABLE IF NOT EXISTS `content_product` (`Id` int NOT NULL AUTO_INCREMENT,`ProductName` varchar(200) NOT NULL,`CategoryId` int NULL,`CoverImage` varchar(500) NULL,`ImageList` longtext NULL,`Summary` varchar(1000) NULL,`Description` longtext NULL,`Specification` longtext NULL,`Feature` longtext NULL,`Sort` int NOT NULL DEFAULT 0,`IsRecommend` tinyint(1) NOT NULL DEFAULT 0,`Status` int NOT NULL DEFAULT 0,`CreateTime` datetime(6) NOT NULL,`UpdateTime` datetime(6) NOT NULL,`IsDeleted` tinyint(1) NOT NULL DEFAULT 0,PRIMARY KEY (`Id`)) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4;",
                @"CREATE TABLE IF NOT EXISTS `content_job` (`Id` int NOT NULL AUTO_INCREMENT,`JobTitle` varchar(200) NOT NULL,`Department` varchar(100) NULL,`WorkLocation` varchar(100) NULL,`SalaryRange` varchar(100) NULL,`RecruitCount` int NOT NULL DEFAULT 0,`JobType` varchar(100) NULL,`Responsibilities` longtext NULL,`Requirements` longtext NULL,`ContactName` varchar(100) NULL,`ContactPhone` varchar(50) NULL,`ContactEmail` varchar(100) NULL,`Status` int NOT NULL DEFAULT 0,`PublishTime` datetime(6) NULL,`CreateTime` datetime(6) NOT NULL,`UpdateTime` datetime(6) NOT NULL,`IsDeleted` tinyint(1) NOT NULL DEFAULT 0,PRIMARY KEY (`Id`)) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4;",
                @"CREATE TABLE IF NOT EXISTS `content_job_apply` (`Id` int NOT NULL AUTO_INCREMENT,`JobId` int NOT NULL,`ApplicantName` varchar(100) NULL,`Phone` varchar(50) NULL,`Email` varchar(100) NULL,`ResumeFile` varchar(500) NULL,`Remark` varchar(1000) NULL,`Status` int NOT NULL DEFAULT 0,`CreateTime` datetime(6) NOT NULL,PRIMARY KEY (`Id`),INDEX `IX_content_job_apply_JobId` (`JobId`)) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4;",
                @"CREATE TABLE IF NOT EXISTS `material_file` (`Id` int NOT NULL AUTO_INCREMENT,`FileName` varchar(200) NOT NULL,`FileUrl` varchar(500) NOT NULL,`FileType` varchar(50) NULL,`FileSize` bigint NOT NULL DEFAULT 0,`Category` varchar(100) NULL,`IsDeleted` tinyint(1) NOT NULL DEFAULT 0,`CreateTime` datetime(6) NOT NULL,PRIMARY KEY (`Id`)) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4;"
            };

            foreach (var sql in sqlList)
            {
                db.Database.ExecuteSqlRaw(sql);
            }
        }

        private static void SeedDefaults(WebsiteBuilderDbContext db)
        {
            var now = DateTime.Now;
            if (!db.SiteConfigs.Any())
            {
                db.SiteConfigs.Add(new WebsiteSiteConfig { SiteName = "企业官网", BrowserTitle = "企业官网可视化建站系统", Keywords = "企业官网,拖拉拽建站,产品展示,新闻资讯,招聘官网", Description = "支持页面装修、新闻、产品、招聘和前台动态渲染的企业官网系统。", Tel = "400-000-0000", Email = "service@example.com", Address = "广东省深圳市", Copyright = "Copyright © 企业官网 All Rights Reserved.", Theme = "default", Language = "zh-CN", IsEnabled = true, CreateTime = now, UpdateTime = now });
                db.SaveChanges();
            }

            if (!db.NewsCategories.Any())
            {
                db.NewsCategories.Add(new ContentNewsCategory { CategoryName = "企业新闻", CategoryCode = "company", Sort = 1, CreateTime = now });
                db.SaveChanges();
            }

            if (!db.ProductCategories.Any())
            {
                db.ProductCategories.Add(new ContentProductCategory { CategoryName = "核心产品", CategoryCode = "core", Sort = 1, CreateTime = now });
                db.SaveChanges();
            }

            if (!db.Navigations.Any(x => !x.IsDeleted))
            {
                db.Navigations.AddRange(
                    new WebsiteNavigation { Title = "首页", LinkType = "page", LinkUrl = "/", Sort = 1, CreateTime = now, UpdateTime = now },
                    new WebsiteNavigation { Title = "关于我们", LinkType = "page", LinkUrl = "/about", Sort = 2, CreateTime = now, UpdateTime = now },
                    new WebsiteNavigation { Title = "产品中心", LinkType = "page", LinkUrl = "/products", Sort = 3, CreateTime = now, UpdateTime = now },
                    new WebsiteNavigation { Title = "新闻中心", LinkType = "page", LinkUrl = "/news", Sort = 4, CreateTime = now, UpdateTime = now },
                    new WebsiteNavigation { Title = "招聘中心", LinkType = "page", LinkUrl = "/jobs", Sort = 5, CreateTime = now, UpdateTime = now },
                    new WebsiteNavigation { Title = "联系我们", LinkType = "page", LinkUrl = "/contact", Sort = 6, CreateTime = now, UpdateTime = now });
                db.SaveChanges();
            }

            if (!db.Banners.Any(x => !x.IsDeleted))
            {
                db.Banners.Add(new WebsiteBanner { Title = "专注企业数字化建设", Subtitle = "可视化建站、内容管理、页面发布一体化", ButtonText = "了解更多", ButtonLink = "/about", Height = 520, Sort = 1, CreateTime = now, UpdateTime = now });
                db.SaveChanges();
            }

            if (!db.Footers.Any())
            {
                var site = db.SiteConfigs.AsNoTracking().FirstOrDefault();
                db.Footers.Add(new WebsiteFooter { CompanyName = site?.SiteName ?? "企业官网", Description = site?.Description ?? "企业官网可视化建站系统", Tel = site?.Tel, Email = site?.Email, Address = site?.Address, IcpNo = site?.IcpNo, PoliceNo = site?.PoliceNo, Copyright = site?.Copyright ?? "Copyright © 企业官网 All Rights Reserved.", FriendLinksJson = "[]", BackgroundColor = "#111827", TextColor = "#ffffff", CreateTime = now, UpdateTime = now });
                db.SaveChanges();
            }

            if (!db.News.Any())
            {
                db.News.Add(new ContentNews { Title = "企业官网可视化建站系统上线", CategoryId = db.NewsCategories.Select(x => x.Id).FirstOrDefault(), Summary = "后台支持页面拖拉拽装修、新闻产品招聘管理和前台动态渲染。", Content = "企业官网可视化建站系统提供页面管理、页面装修、内容管理、发布预览等能力。", Author = "系统管理员", Source = "本站", IsTop = true, IsRecommend = true, Status = (int)WebsiteContentStatus.Published, PublishTime = now, CreateTime = now, UpdateTime = now });
                db.SaveChanges();
            }

            if (!db.Products.Any())
            {
                db.Products.Add(new ContentProduct { ProductName = "企业数字化官网平台", CategoryId = db.ProductCategories.Select(x => x.Id).FirstOrDefault(), Summary = "面向企业官网、产品展示、新闻资讯和招聘门户的可视化建站平台。", Description = "支持页面组件化装修、内容统一管理、前台 JSON 动态渲染。", Specification = "组件化JSON;响应式布局;后台可视化装修", Feature = "快速建站;低维护成本;内容与页面解耦", Sort = 1, IsRecommend = true, Status = (int)WebsiteContentStatus.Published, CreateTime = now, UpdateTime = now });
                db.SaveChanges();
            }

            if (!db.Jobs.Any())
            {
                db.Jobs.Add(new ContentJob { JobTitle = ".NET 全栈开发工程师", Department = "研发中心", WorkLocation = "深圳", SalaryRange = "面议", RecruitCount = 2, JobType = "全职", Responsibilities = "负责企业官网建站平台后台接口、页面装修和前台渲染功能开发。", Requirements = "熟悉 .NET 8、EF Core、MySQL、前端基础和企业后台系统开发。", ContactName = "HR", ContactEmail = "hr@example.com", Status = (int)WebsiteContentStatus.Published, PublishTime = now, CreateTime = now, UpdateTime = now });
                db.SaveChanges();
            }

            if (!db.Pages.Any())
            {
                var layoutJson = "{\"width\":\"full\",\"theme\":\"default\"}";
                var homeComponents = DefaultHomeComponentJson();
                var draftJson = DefaultPageJson(1, "首页", "/", layoutJson, homeComponents);
                db.Pages.Add(new WebsitePage { SiteId = 1, PageName = "首页", PageCode = "home", PagePath = "/", PageTitle = "首页 - 企业官网", SeoKeywords = "企业官网,产品展示,新闻中心,招聘中心", SeoDescription = "企业官网首页，展示企业介绍、产品、新闻、招聘和联系方式。", LayoutJson = layoutJson, ComponentJson = homeComponents, DraftJson = draftJson, PublishJson = draftJson, Status = (int)WebsiteContentStatus.Published, IsHome = true, Sort = 1, CreateTime = now, UpdateTime = now, PublishTime = now });
                AddDraftPage(db, layoutJson, "关于我们", "about", "/about", 2, DefaultRichTextComponentJson("关于我们", "这里可以维护企业简介、发展历程、资质荣誉和企业文化。"));
                AddDraftPage(db, layoutJson, "新闻中心", "news", "/news", 3, DefaultListComponentJson("news", "新闻中心"));
                AddDraftPage(db, layoutJson, "产品中心", "products", "/products", 4, DefaultListComponentJson("product", "产品中心"));
                AddDraftPage(db, layoutJson, "招聘中心", "jobs", "/jobs", 5, DefaultListComponentJson("job", "招聘中心"));
                db.SaveChanges();
            }
        }

        private static void AddDraftPage(WebsiteBuilderDbContext db, string layoutJson, string name, string code, string path, int sort, string componentJson)
        {
            db.Pages.Add(new WebsitePage { SiteId = 1, PageName = name, PageCode = code, PagePath = path, PageTitle = name + " - 企业官网", LayoutJson = layoutJson, ComponentJson = componentJson, DraftJson = DefaultPageJson(0, name, path, layoutJson, componentJson), Status = (int)WebsiteContentStatus.Draft, Sort = sort, CreateTime = DateTime.Now, UpdateTime = DateTime.Now });
        }

        private static string DefaultPageJson(int pageId, string pageName, string pagePath, string layoutJson, string componentJson)
        {
            return $$"""
            { "pageId": {{pageId}}, "pageName": "{{pageName}}", "pagePath": "{{pagePath}}", "layout": {{layoutJson}}, "components": {{componentJson}} }
            """;
        }

        private static string DefaultHomeComponentJson()
        {
            return """
            [
              { "id": "nav_001", "type": "navigation", "name": "顶部导航", "sort": 1, "visible": true, "props": { }, "style": { } },
              { "id": "banner_001", "type": "banner", "name": "首页 Banner", "sort": 2, "visible": true, "props": { "height": 520, "title": "专注企业数字化建设", "subtitle": "通过可视化建站、内容管理和组件化渲染提升官网维护效率", "buttonText": "了解更多", "buttonLink": "/about" }, "style": { "backgroundColor": "#0d6efd", "textColor": "#ffffff" } },
              { "id": "richtext_001", "type": "richText", "name": "公司简介", "sort": 3, "visible": true, "props": { "title": "公司简介", "content": "这里展示企业介绍、核心能力、服务优势和发展方向。" }, "style": { "backgroundColor": "#ffffff", "paddingTop": 64, "paddingBottom": 64 } },
              { "id": "product_001", "type": "product", "name": "产品中心", "sort": 4, "visible": true, "props": { "title": "产品中心", "count": 6 }, "style": { "backgroundColor": "#f7f9fc", "paddingTop": 64, "paddingBottom": 64 } },
              { "id": "news_001", "type": "news", "name": "新闻中心", "sort": 5, "visible": true, "props": { "title": "新闻中心", "count": 6 }, "style": { "backgroundColor": "#ffffff", "paddingTop": 64, "paddingBottom": 64 } },
              { "id": "job_001", "type": "job", "name": "招聘信息", "sort": 6, "visible": true, "props": { "title": "招聘信息", "count": 3 }, "style": { "backgroundColor": "#f7f9fc", "paddingTop": 64, "paddingBottom": 64 } },
              { "id": "footer_001", "type": "footer", "name": "页脚", "sort": 7, "visible": true, "props": { }, "style": { "backgroundColor": "#111827", "textColor": "#ffffff" } }
            ]
            """;
        }

        private static string DefaultRichTextComponentJson(string title, string content)
        {
            return $$"""
            [{ "id": "nav_001", "type": "navigation", "name": "顶部导航", "sort": 1, "visible": true, "props": {}, "style": {} },{ "id": "content_001", "type": "richText", "name": "{{title}}", "sort": 2, "visible": true, "props": { "title": "{{title}}", "content": "{{content}}" }, "style": { "backgroundColor": "#ffffff", "paddingTop": 64, "paddingBottom": 64 } },{ "id": "footer_001", "type": "footer", "name": "页脚", "sort": 3, "visible": true, "props": {}, "style": {} }]
            """;
        }

        private static string DefaultListComponentJson(string type, string title)
        {
            return $$"""
            [{ "id": "nav_001", "type": "navigation", "name": "顶部导航", "sort": 1, "visible": true, "props": {}, "style": {} },{ "id": "list_001", "type": "{{type}}", "name": "{{title}}", "sort": 2, "visible": true, "props": { "title": "{{title}}", "count": 12 }, "style": { "backgroundColor": "#ffffff", "paddingTop": 64, "paddingBottom": 64 } },{ "id": "footer_001", "type": "footer", "name": "页脚", "sort": 3, "visible": true, "props": {}, "style": {} }]
            """;
        }
    }
}