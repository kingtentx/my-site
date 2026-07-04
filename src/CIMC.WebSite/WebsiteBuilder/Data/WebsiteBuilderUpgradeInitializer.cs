using System;
using System.Linq;
using Microsoft.EntityFrameworkCore;
using MySite.Web.WebsiteBuilder.Models;

namespace MySite.Web.WebsiteBuilder.Data
{
    public class WebsiteBuilderUpgradeInitializer
    {
        public void Create(WebsiteBuilderDbContext db)
        {
            CreateTables(db);
            SeedStructure(db);
        }

        private static void CreateTables(WebsiteBuilderDbContext db)
        {
            var sqlList = new[]
            {
                @"CREATE TABLE IF NOT EXISTS `website_navigation` (`Id` int NOT NULL AUTO_INCREMENT,`SiteId` int NOT NULL DEFAULT 1,`ParentId` int NULL,`Title` varchar(100) NOT NULL,`LinkType` varchar(50) NULL,`PageId` int NULL,`LinkUrl` varchar(300) NULL,`Target` varchar(20) NULL,`Icon` varchar(100) NULL,`Sort` int NOT NULL DEFAULT 0,`IsEnabled` tinyint(1) NOT NULL DEFAULT 1,`IsDeleted` tinyint(1) NOT NULL DEFAULT 0,`CreateTime` datetime(6) NOT NULL,`UpdateTime` datetime(6) NOT NULL,PRIMARY KEY (`Id`),INDEX `IX_website_navigation_SiteId` (`SiteId`)) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4;",
                @"CREATE TABLE IF NOT EXISTS `website_banner` (`Id` int NOT NULL AUTO_INCREMENT,`SiteId` int NOT NULL DEFAULT 1,`Title` varchar(200) NOT NULL,`Subtitle` varchar(500) NULL,`ImageUrl` varchar(500) NULL,`VideoUrl` varchar(500) NULL,`ButtonText` varchar(100) NULL,`ButtonLink` varchar(300) NULL,`LinkUrl` varchar(300) NULL,`Height` int NOT NULL DEFAULT 520,`Sort` int NOT NULL DEFAULT 0,`AutoPlay` tinyint(1) NOT NULL DEFAULT 1,`Interval` int NOT NULL DEFAULT 5000,`IsEnabled` tinyint(1) NOT NULL DEFAULT 1,`IsDeleted` tinyint(1) NOT NULL DEFAULT 0,`BeginTime` datetime(6) NULL,`EndTime` datetime(6) NULL,`CreateTime` datetime(6) NOT NULL,`UpdateTime` datetime(6) NOT NULL,PRIMARY KEY (`Id`),INDEX `IX_website_banner_SiteId` (`SiteId`)) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4;",
                @"CREATE TABLE IF NOT EXISTS `website_footer` (`Id` int NOT NULL AUTO_INCREMENT,`SiteId` int NOT NULL DEFAULT 1,`Logo` varchar(500) NULL,`CompanyName` varchar(100) NULL,`Description` varchar(1000) NULL,`Tel` varchar(50) NULL,`Email` varchar(100) NULL,`Address` varchar(300) NULL,`QrCode` varchar(500) NULL,`IcpNo` varchar(100) NULL,`PoliceNo` varchar(100) NULL,`Copyright` varchar(300) NULL,`FriendLinksJson` longtext NULL,`BackgroundColor` varchar(50) NULL,`TextColor` varchar(50) NULL,`CreateTime` datetime(6) NOT NULL,`UpdateTime` datetime(6) NOT NULL,PRIMARY KEY (`Id`),INDEX `IX_website_footer_SiteId` (`SiteId`)) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4;",
                @"CREATE TABLE IF NOT EXISTS `content_job_apply` (`Id` int NOT NULL AUTO_INCREMENT,`JobId` int NOT NULL,`ApplicantName` varchar(100) NULL,`Phone` varchar(50) NULL,`Email` varchar(100) NULL,`ResumeFile` varchar(500) NULL,`Remark` varchar(1000) NULL,`Status` int NOT NULL DEFAULT 0,`CreateTime` datetime(6) NOT NULL,PRIMARY KEY (`Id`),INDEX `IX_content_job_apply_JobId` (`JobId`)) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4;",
                @"CREATE TABLE IF NOT EXISTS `material_file` (`Id` int NOT NULL AUTO_INCREMENT,`FileName` varchar(200) NOT NULL,`FileUrl` varchar(500) NOT NULL,`FileType` varchar(50) NULL,`FileSize` bigint NOT NULL DEFAULT 0,`Category` varchar(100) NULL,`IsDeleted` tinyint(1) NOT NULL DEFAULT 0,`CreateTime` datetime(6) NOT NULL,PRIMARY KEY (`Id`)) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4;"
            };

            foreach (var sql in sqlList)
            {
                db.Database.ExecuteSqlRaw(sql);
            }
        }

        private static void SeedStructure(WebsiteBuilderDbContext db)
        {
            var now = DateTime.Now;
            if (!db.Navigations.Any(x => !x.IsDeleted))
            {
                db.Navigations.Add(new WebsiteNavigation { Title = "首页", LinkType = "page", LinkUrl = "/", Sort = 1, CreateTime = now, UpdateTime = now });
                db.Navigations.Add(new WebsiteNavigation { Title = "关于我们", LinkType = "page", LinkUrl = "/about", Sort = 2, CreateTime = now, UpdateTime = now });
                db.Navigations.Add(new WebsiteNavigation { Title = "产品中心", LinkType = "page", LinkUrl = "/products", Sort = 3, CreateTime = now, UpdateTime = now });
                db.Navigations.Add(new WebsiteNavigation { Title = "新闻中心", LinkType = "page", LinkUrl = "/news", Sort = 4, CreateTime = now, UpdateTime = now });
                db.Navigations.Add(new WebsiteNavigation { Title = "招聘中心", LinkType = "page", LinkUrl = "/jobs", Sort = 5, CreateTime = now, UpdateTime = now });
                db.Navigations.Add(new WebsiteNavigation { Title = "联系我们", LinkType = "page", LinkUrl = "/contact", Sort = 6, CreateTime = now, UpdateTime = now });
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
        }
    }
}