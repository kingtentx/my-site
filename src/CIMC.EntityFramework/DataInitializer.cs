using CIMC.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text.Json;
using System.Text.RegularExpressions;
using Microsoft.EntityFrameworkCore;

namespace CIMC.Data
{
    /// <summary>
    /// 初始化数据
    /// </summary>
    public class DataInitializer
    {
        public void Create(AppDbContext context)
        {
            EnsureSiteModuleTable(context);
            EnsureAlbumColumns(context);
            InitUser(context);
            InitMenu(context);
            InitTags(context);
            InitSiteContent(context);
        }

        private void InitTags(AppDbContext context)
        {
            var tagNameMap = new Dictionary<string, (int Type, int Sort)>
            {
                { "公司新闻", (1, 1) }, { "行业动态", (1, 2) },
                { "箱型-传统业务线", (2, 10) }, { "箱型-特种业务线", (2, 11) }, { "箱型-新业务线", (2, 12) },
                { "资质证书", (2, 20) }, { "合作客户", (2, 21) },
                { "社会招聘", (3, 30) }
            };

            if (!context.Tag.Any())
            {
                var tags = new[]
                {
                    new Tag { Id = 1, TagName = "公司新闻", TagType = 1, Sort = 1, IsActive = true, CreationTime = DateTime.Now, CreationBy = "system" },
                    new Tag { Id = 2, TagName = "行业动态", TagType = 1, Sort = 2, IsActive = true, CreationTime = DateTime.Now, CreationBy = "system" },
                    new Tag { Id = 10, TagName = "箱型-传统业务线", TagType = 2, Sort = 10, IsActive = true, CreationTime = DateTime.Now, CreationBy = "system" },
                    new Tag { Id = 11, TagName = "箱型-特种业务线", TagType = 2, Sort = 11, IsActive = true, CreationTime = DateTime.Now, CreationBy = "system" },
                    new Tag { Id = 12, TagName = "箱型-新业务线", TagType = 2, Sort = 12, IsActive = true, CreationTime = DateTime.Now, CreationBy = "system" },
                    new Tag { Id = 20, TagName = "资质证书", TagType = 2, Sort = 20, IsActive = true, CreationTime = DateTime.Now, CreationBy = "system" },
                    new Tag { Id = 21, TagName = "合作客户", TagType = 2, Sort = 21, IsActive = true, CreationTime = DateTime.Now, CreationBy = "system" },
                    new Tag { Id = 30, TagName = "社会招聘", TagType = 3, Sort = 30, IsActive = true, CreationTime = DateTime.Now, CreationBy = "system" }
                };
                context.Tag.AddRange(tags);
                context.SaveChanges();
                return;
            }

            var existingTags = context.Tag.ToList();
            bool tagChanged = false;
            foreach (var tag in existingTags)
            {
                if (tagNameMap.TryGetValue(tag.TagName, out var info) && tag.TagType != info.Type)
                {
                    tag.TagType = info.Type;
                    tagChanged = true;
                }
            }
            if (tagChanged) context.SaveChanges();

            var tagsByType = existingTags.GroupBy(t => t.TagType)
                .ToDictionary(g => g.Key, g => g.Select(t => t.Id).ToHashSet());
            var tagFirstByType = existingTags.GroupBy(t => t.TagType)
                .ToDictionary(g => g.Key, g => g.OrderBy(t => t.Sort).First().Id);

            bool FixTagId(int tagType, int oldTagId, out int newTagId)
            {
                newTagId = oldTagId;
                if (tagType == 0 || oldTagId == 0) return false;
                if (!tagsByType.TryGetValue(tagType, out var ids)) return false;
                if (ids.Contains(oldTagId)) return false;
                newTagId = tagFirstByType[tagType];
                return true;
            }

            bool changed = false;
            foreach (var a in context.Article.Where(a => a.TagId > 0 && a.TagType > 0).ToList())
            {
                if (FixTagId(a.TagType, a.TagId, out var newId)) { a.TagId = newId; changed = true; }
            }
            foreach (var a in context.Album.Where(a => a.TagId > 0 && a.TagType > 0).ToList())
            {
                if (FixTagId(a.TagType, a.TagId, out var newId)) { a.TagId = newId; changed = true; }
            }
            foreach (var j in context.Job.Where(j => j.TagId > 0 && j.TagType > 0).ToList())
            {
                if (FixTagId(j.TagType, j.TagId, out var newId)) { j.TagId = newId; changed = true; }
            }
            if (changed) context.SaveChanges();
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

        private void InitSiteContent(AppDbContext context)
        {
            InitArticles(context);
            InitJobs(context);
            InitProducts(context);
            InitCertificates(context);
            InitPartners(context);
            InitSiteModules(context);
            RepairSiteAssetPaths(context);
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
                    TagId = GetNewsTagId(context, item.Category),
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

        private void InitJobs(AppDbContext context)
        {
            var jsonPath = GetSeedDataPath("jobs-data.json");
            if (!File.Exists(jsonPath))
            {
                return;
            }

            var json = File.ReadAllText(jsonPath);
            var items = JsonSerializer.Deserialize<List<JobSeedItem>>(json, new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            });

            if (items == null || items.Count == 0)
            {
                return;
            }

            foreach (var item in items)
            {
                if (string.IsNullOrWhiteSpace(item.JobTitle))
                {
                    continue;
                }

                var exists = context.Job.FirstOrDefault(p => p.JobName == item.JobTitle);
                if (exists != null)
                {
                    continue;
                }

                context.Job.Add(new Job
                {
                    JobName = item.JobTitle,
                    Author = "人力资源部",
                    Detail = string.IsNullOrWhiteSpace(item.ContentHtml) ? HtmlParagraphs(item.ContentText) : item.ContentHtml,
                    TagType = 3,
                    TagId = GetTagId(context, "社会招聘", 30),
                    IsActive = true,
                    IsDelete = false,
                    CreationBy = "system",
                    CreationTime = DateTime.Now
                });
            }

            context.SaveChanges();
        }

        private void InitProducts(AppDbContext context)
        {
            var tagTraditional = GetTagId(context, "箱型-传统业务线", 10);
            var tagSpecial = GetTagId(context, "箱型-特种业务线", 11);
            var tagNew = GetTagId(context, "箱型-新业务线", 12);

            // 先修复被错误归到产品标签的合作客户数据
            var tagPartner = GetTagId(context, "合作客户", 21);
            var productTagIds = new[] { tagTraditional, tagSpecial, tagNew }.Where(id => id > 0).ToHashSet();
            if (tagPartner > 0)
            {
                var misplaced = context.Album
                    .Where(a => !a.IsDelete && a.Title == "合作客户" && productTagIds.Contains(a.TagId))
                    .ToList();
                foreach (var item in misplaced)
                {
                    item.TagId = tagPartner;
                }
                if (misplaced.Count > 0) context.SaveChanges();
            }

            // 按产品 Title 精确判断是否已初始化，避免合作客户数据干扰
            var productTitles = new[] { "20GP箱", "40HC箱", "折叠箱", "污水处理箱" };
            if (context.Album.Any(p => p.Author == "中集洋山官网" && productTagIds.Contains(p.TagId) && productTitles.Contains(p.Title)))
            {
                UpdateProductDetails(context);
                return;
            }

            var products = new List<Album>
            {
                Product("20GP箱", "标准干货集装箱，适用于多式联运和通用货物运输。", "/syle/images/44869223.png", tagTraditional, Product20GpDetail(), "/product/2172988.html", 1),
                Product("20HC箱", "高箱产品，兼顾标准化运输与更大装载空间。", "/syle/images/43974676.png", tagTraditional),
                Product("40GP箱", "成熟稳定的传统业务线产品，为全球航运客户提供长期服务。", "/syle/images/43971817.png", tagTraditional),
                Product("40HC箱", "高强度结构与精益制造结合，满足大批量交付需求。", "/syle/images/44656636.png", tagTraditional),

                Product("折叠箱", "面向特种运输场景的定制化箱体装备。", "/syle/images/43974836.png", tagSpecial),
                Product("罐箱框架", "适配特殊装载与复杂工况的结构化产品。", "/syle/images/43974835.png", tagSpecial),
                Product("模块化装备", "通过模块化设计提升交付效率和场景适配能力。", "/syle/images/43974844.png", tagSpecial),
                Product("冷链装备", "服务温控物流与高附加值运输场景。", "/syle/images/44161415.jpg", tagSpecial),

                Product("污水处理箱", "面向环保装备领域的集成式箱体解决方案。", "/syle/images/45999506.png", tagNew),
                Product("环保箱", "绿色低碳场景下的模块化环保装备。", "/syle/images/44068098.png", tagNew),
                Product("风力发电箱", "服务新能源领域的专业化装备产品。", "/syle/images/44068097.png", tagNew),
                Product("储能箱", "面向储能系统集成和能源转型应用。", "/syle/images/45999504.png", tagNew),
                Product("氢能设备箱", "聚焦氢能装备集成及新能源业务拓展。", "/syle/images/49759069.png", tagNew),
                Product("营房箱", "模块化建筑与临时设施场景应用。", "/syle/images/44458197.png", tagNew)
            };

            context.Album.AddRange(products);
            context.SaveChanges();
            UpdateProductDetails(context);
        }

        private void InitCertificates(AppDbContext context)
        {
            var tagCert = GetTagId(context, "资质证书", 20);
            if (context.Album.Any(p => p.Author == "中集洋山官网" && p.TagId == tagCert))
            {
                return;
            }

            var certificates = new List<Album>
            {
                Product("DNV认证", "国际船级社认证，体现公司产品质量与制造体系能力。", "/syle/images/49764457.png", tagCert),
                Product("压力管道元件制造资质", "压力管道元件制造相关资质证书。", "/syle/images/49764461.png", tagCert),
                Product("职业健康与安全管理体系认证 ISO45001:2018", "职业健康安全管理体系认证。", "/syle/images/44064957.jpg", tagCert),
                Product("环境管理体系认证 ISO14001:2015", "环境管理体系认证。", "/syle/images/44064956.jpg", tagCert),
                Product("能源管理体系认证证书", "能源管理体系认证。", "/syle/images/44463398.jpg", tagCert),
                Product("合规管理体系认证证书", "合规管理体系认证。", "/syle/images/44463400.jpg", tagCert),
                Product("国家信息安全等级保护认证证书", "国家信息安全等级保护认证。", "/syle/images/44463396.png", tagCert)
            };

            context.Album.AddRange(certificates);
            context.SaveChanges();
        }

        private void InitPartners(AppDbContext context)
        {
            var tagPartner = GetTagId(context, "合作客户", 21);

            // 先修复被错误归到其他标签的合作客户数据
            var tagTraditional = GetTagId(context, "箱型-传统业务线", 10);
            var tagSpecial = GetTagId(context, "箱型-特种业务线", 11);
            var tagNew = GetTagId(context, "箱型-新业务线", 12);
            var tagCert = GetTagId(context, "资质证书", 20);

            var wrongTagIds = new[] { tagTraditional, tagSpecial, tagNew, tagCert }.Where(id => id > 0).ToHashSet();
            if (tagPartner > 0 && wrongTagIds.Count > 0)
            {
                var misplaced = context.Album
                    .Where(a => !a.IsDelete && a.Title == "合作客户" && wrongTagIds.Contains(a.TagId))
                    .ToList();
                foreach (var item in misplaced)
                {
                    item.TagId = tagPartner;
                }
                if (misplaced.Count > 0) context.SaveChanges();
            }

            // 基于 ImageUrl 去重，避免重复插入
            var partnerImages = new[]
            {
                "/syle/images/43968717.png", "/syle/images/43968718.png", "/syle/images/43968719.png",
                "/syle/images/43968720.png", "/syle/images/43968721.png", "/syle/images/43968723.png",
                "/syle/images/43968725.png", "/syle/images/43968726.png", "/syle/images/43968727.png",
                "/syle/images/43968763.png", "/syle/images/43968730.png", "/syle/images/43968729.png"
            };

            var existingImages = context.Album
                .Where(p => p.Author == "中集洋山官网" && p.Title == "合作客户" && !p.IsDelete)
                .Select(p => p.ImageUrl)
                .ToHashSet();

            var newPartners = new List<Album>();
            for (int i = 0; i < partnerImages.Length; i++)
            {
                if (!existingImages.Contains(partnerImages[i]))
                {
                    newPartners.Add(Product("合作客户", "合作客户客户标识", partnerImages[i], tagPartner, "", "", i + 1));
                }
            }

            if (newPartners.Count > 0)
            {
                context.Album.AddRange(newPartners);
                context.SaveChanges();
            }

            // 删除重复的合作客户数据（相同 ImageUrl 保留最早的一条）
            RemoveDuplicateAlbums(context, "合作客户");
        }

        /// <summary>
        /// 删除 Album 表中指定 Title 的重复数据，相同 ImageUrl 只保留最早的一条
        /// </summary>
        private void RemoveDuplicateAlbums(AppDbContext context, string title)
        {
            var albums = context.Album
                .Where(a => !a.IsDelete && a.Title == title)
                .OrderBy(a => a.Id)
                .ToList();

            var seen = new HashSet<string>();
            var toDelete = new List<Album>();
            foreach (var item in albums)
            {
                if (seen.Contains(item.ImageUrl))
                {
                    toDelete.Add(item);
                }
                else
                {
                    seen.Add(item.ImageUrl);
                }
            }

            if (toDelete.Count == 0) return;

            foreach (var item in toDelete)
            {
                item.IsDelete = true;
            }

            context.SaveChanges();
        }

        private void InitSiteModules(AppDbContext context)
        {
            var homeProductsSettings = """{"items":[{"image":"/syle/images/43974676.png","title":"传统业务线","description":"具备全方位的标箱设计、生产制造能力，覆盖 20GP、20HC、40GP、40HC 等多种常规箱型。","link":"/products/traditional"},{"image":"/syle/images/43974836.png","title":"特种业务线","description":"面向复杂装载、模块化运输和定制场景，提供结构设计、制造与交付能力。","link":"/products/special"},{"image":"/syle/images/45999504.png","title":"新业务线","description":"聚焦能源转型、低碳产业，布局储能、氢能、环保和模块化建筑等装备领域。","link":"/products/new"}]}""";
            var homeAboutSettings = """{"metrics":[{"value":"25万","label":"TEU 设计年产能"},{"value":"33.41万㎡","label":"总占地面积"},{"value":"3条","label":"完整生产线"}],"description":"中集洋山响应传统制造业\u201c转型升级\u201d号召，坚定不移地走信息化和工业化的高层次深度融合之路。公司落地\u201c龙腾计划\u201d智能制造项目，推进 SAP、MES、精益工时、IoT 物联网、AI 智能检测等数字化系统建设。"}""";
            var homeCareersSettings = """{"description":"人员培养有管理序列、专业序列和技能序列三个发展通道，通过职层教育、技能教育、精益教育和 OJD 实现员工能力培养。"}""";
            var productsBannerSettings = """{"categories":[{"key":"traditional","label":"传统业务线","intro":"具备全方位的标箱设计、生产制造能力，以标准干货集装箱为基础，设计生产 20GP、20HC、40GP、40HC 等多种常规箱型。"},{"key":"special","label":"特种业务线","intro":"围绕特种集装箱、模块化物流装备、定制化载具等场景，为客户提供结构设计、生产制造和交付服务。"},{"key":"new","label":"新业务线","intro":"中集洋山围绕智能化、绿色化、高端化，聚焦能源转型、低碳产业，布局新能源装备、储能装备、模块化建筑、环保装备等业务领域。"}]}""";
            var aboutBannerSettings = """{"profile":{"images":["/syle/images/43968452.jpg","/syle/images/43968450.jpg"],"eyebrow":"Company Profile","title":"上海中集洋山物流装备有限公司","paragraphs":["上海中集洋山物流装备有限公司（简称\u201c中集洋山\u201d），是中国国际海运集装箱（集团）股份有限公司（简称\u201c中集集团\u201d）的全资子公司，注册资本 2,948 万美元，投资总额 6,325 万美元，设计年产能为 25 万 TEU。","公司于 2006 年入驻临港新片区先进智造片区，总占地面积 33.41 万㎡，拥有 3 条完整的集装箱及先进装备制造生产线。主营各类集装箱及现代化物流装备产品的设计、生产、销售业务，客户遍及欧美、日韩、澳洲等全球各地。","作为中集集团在华东区域的核心工厂，中集洋山秉承中集集团\u201c诚信正直、成就客户、开拓创新、持续改善、合作共赢、结果导向\u201d的核心价值观。"]},"capabilities":[{"title":"智能制造","description":"冲压、焊接、总装、涂装等自动化产线持续升级"},{"title":"数字工厂","description":"SAP、MES、IoT、AI 智能检测等系统协同推进"},{"title":"绿色制造","description":"打造智能化、精益化绿色集装箱制造基地"},{"title":"全球服务","description":"为世界头部航运公司提供长期产品与服务"}],"partnersTitle":"客户与合作","partnersEyebrow":"Partners"}""";
            var jobsBannerSettings = """{"intro":{"title":"招聘职位","description":"人员培养有管理序列、专业序列和技能序列三个发展通道，通过职层教育、技能教育、精益教育和 OJD 实现员工能力培养。","contactInfo":"联系人：沈先生 / 常女士<br/>电话：021-61186770 / 021-61186880"}}""";
            var contactBannerSettings = """{"cards":[{"icon":"/syle/images/7811699.png","title":"公司地址","content":"上海市浦东新区临港新片区层林路77号"},{"icon":"/syle/images/7811698.png","title":"联系电话","content":"021-61186770<br/>021-61186880"},{"icon":"/syle/images/29595705.png","title":"电子邮箱","content":"changhao.shen@cimc.com<br/>jing.chang_syle@cimc.com"}],"location":{"mapImage":"/syle/images/44701954.png","title":"区位优势","description":"公司位于中国（上海）自贸实验区临港新片区先进智造片区，距离铁路芦潮港集装箱中心站 9KM，洋山深水港 55KM，外高桥港区 85KM，具备黄金港口等便利条件。"}}""";
            var contactMessageSettings = """{"description":"如需产品咨询、商务合作、招聘沟通或参观交流，请留下联系方式。后台会保存留言记录，便于工作人员跟进处理。"}""";

            var modules = new List<SiteModule>
            {
                Module("home", "hero", "首页Banner", "banner", "上海中集洋山物流装备有限公司", "专业从事标准干货集装箱、特种集装箱及新能源装备的设计、生产与制造，打造智能化、精益化绿色集装箱制造基地。", "/products/traditional", "/syle/images/44162304.jpg,/syle/images/44455521.jpg,/syle/images/56781343.jpg", 1),
                Module("home", "products", "产品服务", "section", "产品服务", "Product Service", "/products", "", 2, homeProductsSettings),
                Module("home", "about", "企业介绍", "section", "中集洋山物流装备有限公司", "About SYLE", "/about", "/syle/images/44162304.jpg", 3, homeAboutSettings),
                Module("home", "partners", "合作客户", "section", "合作客户", "Partners", "", "", 4),
                Module("home", "news", "新闻资讯", "section", "新闻资讯", "News", "/news", "", 5),
                Module("home", "careers", "人才招聘", "section", "人才招聘", "Join Us", "/jobs", "", 6, homeCareersSettings),
                Module("products", "banner", "产品中心Banner", "banner", "产品服务", "Product Service", "/products/traditional", "/syle/images/56781343.jpg", 1, productsBannerSettings),
                Module("news", "banner", "新闻资讯Banner", "banner", "新闻资讯", "News", "/news", "/syle/images/43966034.jpg", 1),
                Module("jobs", "banner", "人才招聘Banner", "banner", "人才招聘", "Recruitment", "/jobs", "/syle/images/44555004.jpeg", 1, jobsBannerSettings),
                Module("about", "banner", "关于我们Banner", "banner", "关于我们", "About SYLE", "/about", "/syle/images/44456253.jpeg,/syle/images/43968452.jpg", 1, aboutBannerSettings),
                Module("about", "certificates", "资质证书", "carousel", "资质证书", "Certificates", "", "", 6),
                Module("contact", "banner", "联系我们Banner", "banner", "联系我们", "Contact", "/contact", "/syle/images/44806637.jpeg", 1, contactBannerSettings),
                Module("contact", "message", "在线留言", "form", "在线留言", "Message", "", "", 7, contactMessageSettings)
            };

            foreach (var item in modules)
            {
                var anyExists = context.SiteModule.Any(p => p.PageKey == item.PageKey && p.ModuleKey == item.ModuleKey);
                if (!anyExists)
                {
                    context.SiteModule.Add(item);
                }
                else if (!string.IsNullOrWhiteSpace(item.SettingsJson))
                {
                    var existing = context.SiteModule.FirstOrDefault(p => p.PageKey == item.PageKey && p.ModuleKey == item.ModuleKey && !p.IsDelete);
                    if (existing != null && string.IsNullOrWhiteSpace(existing.SettingsJson))
                    {
                        existing.SettingsJson = item.SettingsJson;
                    }
                }
            }

            var hero = context.SiteModule.FirstOrDefault(p => p.PageKey == "home" && p.ModuleKey == "hero" && !p.IsDelete);
            if (hero != null && string.Equals(hero.ImageUrl, "/syle/images/44162304.jpg", StringComparison.OrdinalIgnoreCase))
            {
                hero.ModuleName = "首页Banner";
                hero.ModuleType = "banner";
                hero.ImageUrl = "/syle/images/44162304.jpg,/syle/images/44455521.jpg,/syle/images/56781343.jpg";
            }

            var navMap = new Dictionary<string, int?>();
            var navigations = context.Navigation.Where(p => !p.IsDelete && p.IsActive && !string.IsNullOrWhiteSpace(p.RewriteName)).ToList();
            foreach (var nav in navigations)
            {
                if (!navMap.ContainsKey(nav.RewriteName))
                {
                    navMap[nav.RewriteName] = nav.Id;
                }
            }
            navMap.TryAdd("home", null);

            foreach (var module in context.SiteModule.Where(p => !p.IsDelete))
            {
                if (!module.NavigationId.HasValue && navMap.TryGetValue(module.PageKey, out var navId))
                {
                    module.NavigationId = navId;
                }
            }

            context.SaveChanges();
        }

        private void UpdateProductDetails(AppDbContext context)
        {
            var tagTraditional = GetTagId(context, "箱型-传统业务线", 10);
            var tagSpecial = GetTagId(context, "箱型-特种业务线", 11);
            var tagNew = GetTagId(context, "箱型-新业务线", 12);
            var productTagIds = new[] { tagTraditional, tagSpecial, tagNew };

            var hasChanges = false;
            var product = context.Album.FirstOrDefault(p => !p.IsDelete && p.TagId == tagTraditional && p.Title == "20GP箱");
            if (product != null)
            {
                if (string.IsNullOrWhiteSpace(product.Detail))
                {
                    product.Detail = Product20GpDetail();
                    hasChanges = true;
                }

                if (string.IsNullOrWhiteSpace(product.LinkUrl))
                {
                    product.LinkUrl = "/product/2172988.html";
                    hasChanges = true;
                }
            }

            foreach (var item in context.Album.Where(p => !p.IsDelete && productTagIds.Contains(p.TagId) && string.IsNullOrWhiteSpace(p.Detail)))
            {
                if (string.IsNullOrWhiteSpace(item.Detail))
                {
                    item.Detail = DefaultProductDetail(item.Title, item.Description);
                    hasChanges = true;
                }
            }

            if (hasChanges)
            {
                context.SaveChanges();
            }
        }

        private Album Product(string title, string description, string imageUrl, int tagId, string detail = "", string linkUrl = "", int sort = 0)
        {
            return new Album
            {
                Title = title,
                Description = description,
                Detail = string.IsNullOrWhiteSpace(detail) && tagId >= 10 && tagId <= 12 ? DefaultProductDetail(title, description) : detail,
                ImageUrl = imageUrl,
                LinkUrl = linkUrl,
                Author = "中集洋山官网",
                TagType = 2,
                TagId = tagId,
                Sort = sort,
                IsActive = true,
                IsDelete = false,
                CreationBy = "system",
                CreationTime = DateTime.Now
            };
        }

        private SiteModule Module(string pageKey, string moduleKey, string moduleName, string moduleType, string title, string subTitle, string linkUrl, string imageUrl, int sort, string settingsJson = null)
        {
            return new SiteModule
            {
                PageKey = pageKey,
                ModuleKey = moduleKey,
                ModuleName = moduleName,
                ModuleType = moduleType,
                Title = title,
                SubTitle = subTitle,
                LinkUrl = linkUrl,
                ImageUrl = imageUrl,
                SettingsJson = settingsJson,
                Sort = sort,
                IsActive = true,
                IsDelete = false,
                CreationBy = "system",
                CreationTime = DateTime.Now
            };
        }

        private void InitSiteMenus(AppDbContext context)
        {
            var site = EnsureMenu(context, "网站管理", "Site", 0, "", "layui-icon-website", 1, 20);
            EnsureMenu(context, "网站信息", "Site_Info", site.Id, "/admin/siteinfo", "layui-icon-set", 2, 21, "Edit");
            EnsureMenu(context, "底部信息", "Site_Footer", site.Id, "/admin/footerinfo", "layui-icon-template-1", 2, 22, "Edit");
            EnsureMenu(context, "用户留言", "Site_Message", site.Id, "/message/index", "layui-icon-dialogue", 2, 23, "Delete");

            var content = EnsureMenu(context, "内容管理", "Content", 0, "", "layui-icon-read", 1, 30);
            EnsureMenu(context, "新闻管理", "Content_Article", content.Id, "/article/index", "layui-icon-list", 2, 31, "Add,Edit,Delete");
            EnsureMenu(context, "产品管理", "Content_Album", content.Id, "/album/index", "layui-icon-picture", 2, 32, "Add,Edit,Delete");
            EnsureMenu(context, "招聘管理", "Content_Job", content.Id, "/job/index", "layui-icon-user", 2, 33, "Add,Edit,Delete");
            EnsureMenu(context, "素材管理", "Content_Images", content.Id, "/images/index", "layui-icon-picture", 2, 35, "Add,Edit,Delete");
            var pageModuleMenu = context.Menu.FirstOrDefault(p => p.PermissionKey == "Content_Module");
            if (pageModuleMenu == null)
            {
                pageModuleMenu = new Menu
                {
                    Title = "页面模块",
                    PermissionKey = "Content_Module",
                    Pid = content.Id,
                    Path = "/sitemodule/index",
                    Icon = "layui-icon-component",
                    Buttons = "Add,Edit,Delete",
                    MenuType = 2,
                    IsShow = false,
                    Spread = false,
                    Sort = 34,
                    CreationTime = DateTime.Now,
                    CreationBy = "system"
                };
                context.Menu.Add(pageModuleMenu);
                context.SaveChanges();
            }
            else
            {
                pageModuleMenu.Path = "/sitemodule/index";
                pageModuleMenu.Icon = "layui-icon-component";
                pageModuleMenu.Buttons = "Add,Edit,Delete";
                pageModuleMenu.IsShow = false;
                pageModuleMenu.Sort = 34;
                context.SaveChanges();
            }
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

        private void EnsureSiteModuleTable(AppDbContext context)
        {
            context.Database.ExecuteSqlRaw(@"
                    CREATE TABLE IF NOT EXISTS `SiteModule` (
                      `Id` int NOT NULL AUTO_INCREMENT,
                      `PageKey` varchar(100) NULL,
                      `ModuleKey` varchar(100) NULL,
                      `ModuleName` varchar(100) NULL,
                      `ModuleType` varchar(50) NULL,
                      `Title` varchar(250) NULL,
                      `SubTitle` varchar(500) NULL,
                      `LinkUrl` varchar(500) NULL,
                      `ImageUrl` varchar(500) NULL,
                      `SettingsJson` longtext NULL,
                      `Sort` int NOT NULL DEFAULT 0,
                      `IsActive` tinyint(1) NOT NULL DEFAULT 1,
                      `IsDelete` tinyint(1) NOT NULL DEFAULT 0,
                      `CreationBy` varchar(50) NULL,
                      `CreationTime` datetime(6) NULL,
                      `UpdateBy` varchar(50) NULL,
                      `UpdateTime` datetime(6) NULL,
                      PRIMARY KEY (`Id`)
                    );");
            context.Database.ExecuteSqlRaw("ALTER TABLE `SiteModule` MODIFY COLUMN `ImageUrl` varchar(5000) NULL;");
            AddColumnIfMissing(context, "SiteModule", "NavigationId", "`NavigationId` int NULL");
        }

        private void EnsureAlbumColumns(AppDbContext context)
        {
            AddColumnIfMissing(context, "Album", "LinkUrl", "`LinkUrl` varchar(500) NULL");
            AddColumnIfMissing(context, "Album", "Detail", "`Detail` longtext NULL");
            AddColumnIfMissing(context, "Album", "Sort", "`Sort` int NOT NULL DEFAULT 0");
        }

        private void AddColumnIfMissing(AppDbContext context, string tableName, string columnName, string columnDefinition)
        {
            var connection = context.Database.GetDbConnection();
            var shouldClose = connection.State != System.Data.ConnectionState.Open;
            if (shouldClose)
            {
                connection.Open();
            }

            try
            {
                using var command = connection.CreateCommand();
                command.CommandText = "SELECT COUNT(*) FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_SCHEMA = DATABASE() AND TABLE_NAME = @tableName AND COLUMN_NAME = @columnName";

                var tableParam = command.CreateParameter();
                tableParam.ParameterName = "@tableName";
                tableParam.Value = tableName;
                command.Parameters.Add(tableParam);

                var columnParam = command.CreateParameter();
                columnParam.ParameterName = "@columnName";
                columnParam.Value = columnName;
                command.Parameters.Add(columnParam);

                var exists = Convert.ToInt32(command.ExecuteScalar()) > 0;
                if (!exists)
                {
                    context.Database.ExecuteSqlRaw($"ALTER TABLE `{tableName}` ADD COLUMN {columnDefinition};");
                }
            }
            finally
            {
                if (shouldClose)
                {
                    connection.Close();
                }
            }
        }

        private int GetTagId(AppDbContext context, string tagName, int defaultId)
        {
            var tag = context.Tag.FirstOrDefault(t => t.TagName == tagName);
            return tag?.Id ?? defaultId;
        }

        private int GetNewsTagId(AppDbContext context, string category)
        {
            if (string.Equals(category, "行业动态", StringComparison.OrdinalIgnoreCase))
            {
                return GetTagId(context, "行业动态", 2);
            }

            return GetTagId(context, "公司新闻", 1);
        }

        private string Product20GpDetail()
        {
            return @"
<div class=""product-param-table"">
    <table>
        <tbody>
            <tr><th colspan=""4"">External 箱外</th><th colspan=""4"">Weights 重量</th></tr>
            <tr><td>Length 长度</td><td colspan=""2"">Width 宽度</td><td>Height 高度</td><td>Max Gross Weight 最大总重</td><td colspan=""2"">Tare Weight 自重</td><td>Max Payload 最大载重</td></tr>
            <tr><td>6,058 mm</td><td colspan=""2"">2,438 mm</td><td>2,591 mm</td><td>30,480 Kg</td><td colspan=""2"">2,100 Kg</td><td>28,380 Kg</td></tr>
            <tr><td>20'</td><td colspan=""2"">8'</td><td>8'-6&quot;</td><td>67,200 Lbs</td><td colspan=""2"">4,630 Lbs</td><td>62,570 Lbs</td></tr>
            <tr><th colspan=""4"">Internal 箱内</th><th colspan=""4"">Door Opening 开门尺寸</th></tr>
            <tr><td>Length 长度</td><td colspan=""2"">Width 宽度</td><td>Height 高度</td><td colspan=""2"">Width 宽度</td><td colspan=""2"">Height 高度</td></tr>
            <tr><td>5,898 mm</td><td colspan=""2"">2,352 mm</td><td>2,393 mm</td><td colspan=""2"">2,340 mm</td><td colspan=""2"">2,280 mm</td></tr>
            <tr><td>19'-4 13/64&quot;</td><td colspan=""2"">7'-8 19/32&quot;</td><td>7'-10 7/32&quot;</td><td colspan=""2"">7'-8 1/8&quot;</td><td colspan=""2"">7'-5 49/64&quot;</td></tr>
            <tr><th colspan=""4"">Allowable Stacking Weight 允许堆码重量</th><th colspan=""4"">Cubic Capacity 容积</th></tr>
            <tr><td colspan=""2"">216,000 Kg</td><td colspan=""2"">476,190 Lbs</td><td colspan=""2"">33.2 M3</td><td colspan=""2"">1,173 FT3</td></tr>
            <tr><th colspan=""8"">Special Feature 特点</th></tr>
            <tr><td colspan=""8"">1. The container is used for marine/rail/road transportation. 用于海运、铁路、公路运输。</td></tr>
            <tr><td colspan=""8"">2. Conform to ISO 1496-1/668/6346/1161. 符合 ISO 1496-1/668/6346/1161 标准。</td></tr>
        </tbody>
    </table>
</div>";
        }

        private string DefaultProductDetail(string title, string description)
        {
            return $@"
<div class=""product-param-table"">
    <table>
        <tbody>
            <tr><th colspan=""2"">{System.Net.WebUtility.HtmlEncode(title)} 参数配置</th></tr>
            <tr><td>产品类别</td><td>箱型产品</td></tr>
            <tr><td>产品说明</td><td>{System.Net.WebUtility.HtmlEncode(description)}</td></tr>
            <tr><td>应用场景</td><td>支持按客户运输、装载、集成和交付要求进行配置。</td></tr>
            <tr><td>后台维护</td><td>可在图片列表管理中编辑该箱型详情，维护参数表、图文说明和展示图片。</td></tr>
        </tbody>
    </table>
</div>";
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

        private class JobSeedItem
        {
            public string Id { get; set; }
            public string JobTitle { get; set; }
            public string ContentHtml { get; set; }
            public string ContentText { get; set; }
        }
    }
}
