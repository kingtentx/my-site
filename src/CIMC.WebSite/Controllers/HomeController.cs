using CIMC.Data;
using CIMC.EntityFramework;
using CIMC.Helper;
using CimcSite.Web.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Localization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Localization;
using Microsoft.AspNetCore.Http;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;

namespace CimcSite.Web.Controllers
{
    public class HomeController : Controller
    {
        private const int TraditionalProductTag = 10;
        private const int SpecialProductTag = 11;
        private const int NewProductTag = 12;
        private const int CertificateTag = 20;
        private const int PartnerTag = 21;
        private static readonly List<string> DefaultHomeHeroImages = new List<string>
        {
            "/syle/images/44162304.jpg",
            "/syle/images/44455521.jpg",
            "/syle/images/56781343.jpg"
        };

        private readonly IRepository<Article> _articleRepository;
        private readonly IRepository<Job> _jobRepository;
        private readonly IRepository<Album> _albumRepository;
        private readonly IRepository<MessageBoard> _messageRepository;
        private readonly IRepository<SiteModule> _moduleRepository;
        private readonly IRepository<SiteInfo> _siteInfoRepository;
        private readonly IRepository<FooterInfo> _footerInfoRepository;
        private readonly IRepository<Navigation> _navigationRepository;
        private readonly IRepository<Tag> _tagRepository;
        private readonly ICacheService _cache;
        private readonly IStringLocalizer<HomeController> _localizer;

        public HomeController(
            IRepository<Article> articleRepository,
            IRepository<Job> jobRepository,
            IRepository<Album> albumRepository,
            IRepository<MessageBoard> messageRepository,
            IRepository<SiteModule> moduleRepository,
            IRepository<SiteInfo> siteInfoRepository,
            IRepository<FooterInfo> footerInfoRepository,
            IRepository<Navigation> navigationRepository,
            IRepository<Tag> tagRepository,
            ICacheService cache,
            IStringLocalizer<HomeController> localizer)
        {
            _articleRepository = articleRepository;
            _jobRepository = jobRepository;
            _albumRepository = albumRepository;
            _messageRepository = messageRepository;
            _moduleRepository = moduleRepository;
            _siteInfoRepository = siteInfoRepository;
            _footerInfoRepository = footerInfoRepository;
            _navigationRepository = navigationRepository;
            _tagRepository = tagRepository;
            _cache = cache;
            _localizer = localizer;
        }

        public IActionResult Index()
        {
            LoadSiteConfig();
            var allHomeModules = GetAllModules("home");
            var heroModule = allHomeModules.FirstOrDefault(p => p.ModuleKey == "hero");
            var heroImages = ResolveModuleImages(heroModule, DefaultHomeHeroImages);
            var productsModule = allHomeModules.FirstOrDefault(p => p.ModuleKey == "products");
            var aboutModule = allHomeModules.FirstOrDefault(p => p.ModuleKey == "about");
            var careersModule = allHomeModules.FirstOrDefault(p => p.ModuleKey == "careers");
            ViewData["ProductsSettings"] = ParseSettings(productsModule);
            ViewData["AboutSettings"] = ParseSettings(aboutModule);
            ViewData["CareersSettings"] = ParseSettings(careersModule);
            var model = new PublicSiteHomeViewModel
            {
                SiteInfo = _siteInfoRepository.GetList().FirstOrDefault(),
                HeroModule = heroModule != null && heroModule.IsActive ? heroModule : null,
                HeroImages = heroImages,
                News = GetArticles(6),
                Jobs = _jobRepository.GetList(p => p.IsActive && !p.IsDelete, p => p.CreationTime, 4, false),
                TraditionalProducts = GetAlbums(TraditionalProductTag, 4),
                SpecialProducts = GetAlbums(SpecialProductTag, 4),
                NewProducts = GetAlbums(NewProductTag, 4),
                Partners = GetAlbums(PartnerTag, 12),
                Modules = allHomeModules.Where(p => p.IsActive).ToList()
            };
            return View(model);
        }

        public IActionResult About()
        {
            LoadSiteConfig();
            var bannerModule = GetActiveModule("about", "banner");
            var bannerImages = GetBannerImages("about", "/syle/images/44456253.jpeg");
            ViewData["AboutSettings"] = ParseSettings(bannerModule);
            var model = new PublicAboutViewModel
            {
                BannerTitle = IsEn && !string.IsNullOrWhiteSpace(bannerModule?.Title_EN) ? bannerModule.Title_EN
                    : (string.IsNullOrWhiteSpace(bannerModule?.Title) ? _localizer["关于我们"] : bannerModule.Title),
                BannerSubTitle = IsEn && !string.IsNullOrWhiteSpace(bannerModule?.SubTitle_EN) ? bannerModule.SubTitle_EN
                    : (string.IsNullOrWhiteSpace(bannerModule?.SubTitle) ? _localizer["了解上海中集洋山物流装备有限公司，推进智能制造与数字化工厂建设。"] : bannerModule.SubTitle),
                BannerImage = bannerImages.FirstOrDefault(),
                BannerImages = bannerImages,
                Certificates = GetAlbums(CertificateTag, 20),
                Modules = GetModules("about")
            };
            return View(model);
        }

        public IActionResult Contact()
        {
            LoadSiteConfig();
            var bannerModule = GetActiveModule("contact", "banner");
            var bannerImages = GetBannerImages("contact", "/syle/images/44806637.jpeg");
            var messageModule = GetActiveModule("contact", "message");
            ViewData["ContactSettings"] = ParseSettings(bannerModule);
            ViewData["ContactModule"] = bannerModule;
            ViewData["MessageSettings"] = ParseSettings(messageModule);
            ViewData["MessageModule"] = messageModule;
            var model = new PublicContactViewModel
            {
                BannerTitle = IsEn && !string.IsNullOrWhiteSpace(bannerModule?.Title_EN) ? bannerModule.Title_EN
                    : (string.IsNullOrWhiteSpace(bannerModule?.Title) ? _localizer["联系我们"] : bannerModule.Title),
                BannerSubTitle = IsEn && !string.IsNullOrWhiteSpace(bannerModule?.SubTitle_EN) ? bannerModule.SubTitle_EN
                    : (string.IsNullOrWhiteSpace(bannerModule?.SubTitle) ? _localizer["期待与客户、伙伴及优秀人才建立联系"] : bannerModule.SubTitle),
                BannerImage = bannerImages.FirstOrDefault(),
                BannerImages = bannerImages,
                Modules = GetModules("contact")
            };
            return View(model);
        }

        [HttpPost]
        public IActionResult Message(MessageBoard input, string ValidateKey, string ValidateCode)
        {
            var result = new ResultModel { Code = (int)ResultCode.ParmsError, Message = _localizer["请完善留言信息"] };

            var code = _cache.Get(CacheKey.ValidateCode + ValidateKey) ?? "";
            if (string.IsNullOrEmpty(ValidateCode) || ValidateCode.ToLower() != code.ToString().ToLower())
            {
                result.Message = _localizer["验证码错误"];
                return Json(result);
            }

            if (input == null || string.IsNullOrWhiteSpace(input.UserName) || string.IsNullOrWhiteSpace(input.Phone) || string.IsNullOrWhiteSpace(input.Message))
            {
                return Json(result);
            }

            input.UserName = Trim(input.UserName, 100);
            input.Phone = Trim(input.Phone, 50);
            input.Email = Trim(input.Email, 250);
            input.Message = Trim(input.Message, 1000);
            input.IsRead = false;
            input.CreationTime = DateTime.Now;

            _messageRepository.Add(input);
            result.Code = (int)ResultCode.Success;
            result.Message = _localizer["留言提交成功，我们会尽快与您联系"];
            return Json(result);
        }

        public IActionResult Products(string category = null, string type = "traditional")
        {
            LoadSiteConfig();
            var selectedType = string.IsNullOrWhiteSpace(category) ? type : category;
            var bannerModule = GetActiveModule("products", "banner");
            var bannerImages = GetBannerImages("products", "/syle/images/56781343.jpg");
            var bannerTitle = IsEn && !string.IsNullOrWhiteSpace(bannerModule?.Title_EN) ? bannerModule.Title_EN
                : (string.IsNullOrWhiteSpace(bannerModule?.Title) ? _localizer["产品服务"] : bannerModule.Title);
            var bannerSubTitle = IsEn && !string.IsNullOrWhiteSpace(bannerModule?.SubTitle_EN) ? bannerModule.SubTitle_EN
                : (string.IsNullOrWhiteSpace(bannerModule?.SubTitle) ? null : bannerModule.SubTitle);
            var productsSettings = ParseSettings(bannerModule);
            ViewData["ProductsSettings"] = productsSettings;

            string categoryTitle;
            string categoryIntro;
            int productTag;
            string bannerSubTitleResolved;

            var categoryToken = productsSettings?["categories"]?.FirstOrDefault(c => c["key"]?.ToString() == selectedType);
            if (categoryToken != null)
            {
                categoryTitle = categoryToken["label"]?.ToString() ?? GetDefaultCategoryTitle(selectedType);
                categoryIntro = categoryToken["intro"]?.ToString() ?? GetDefaultCategoryIntro(selectedType);
            }
            else
            {
                categoryTitle = GetDefaultCategoryTitle(selectedType);
                categoryIntro = GetDefaultCategoryIntro(selectedType);
            }

            productTag = selectedType switch
            {
                "special" => SpecialProductTag,
                "new" => NewProductTag,
                _ => TraditionalProductTag
            };
            bannerSubTitleResolved = bannerSubTitle ?? categoryTitle;

            var model = new PublicProductViewModel
            {
                Title = categoryTitle,
                Intro = categoryIntro,
                BannerTitle = bannerTitle,
                BannerSubTitle = bannerSubTitleResolved,
                BannerImage = bannerImages.FirstOrDefault(),
                BannerImages = bannerImages,
                Type = selectedType,
                Products = GetAlbums(productTag, 50)
            };

            return View(model);
        }

        public IActionResult ProductDetail(int id = 0, string legacyId = null)
        {
            LoadSiteConfig();
            var product = _albumRepository.GetOne(p =>
                p.IsActive && !p.IsDelete &&
                p.TagId >= TraditionalProductTag && p.TagId <= NewProductTag &&
                (p.Id == id || (!string.IsNullOrWhiteSpace(legacyId) && p.LinkUrl != null && p.LinkUrl.Contains(legacyId))));

            if (product == null)
            {
                return NotFound();
            }

            var model = new PublicProductDetailViewModel
            {
                Product = product,
                CategoryTitle = GetProductCategoryTitle(product.TagId),
                CategoryType = GetProductCategoryType(product.TagId),
                RelatedProducts = GetAlbums(product.TagId, 8).Where(p => p.Id != product.Id).ToList()
            };

            return View(model);
        }

        public IActionResult News(string category = null, int page = 1)
        {
            LoadSiteConfig();
            page = Math.Max(page, 1);
            const int pageSize = 8;
            var currentTagId = GetNewsTagId(category);
            var bannerModule = GetActiveModule("news", "banner");
            var bannerImages = GetBannerImages("news", "/syle/images/43966034.jpg");
            var result = currentTagId > 0
                ? _articleRepository.GetList(p => p.IsActive && !p.IsDelete && p.TagId == currentTagId, p => p.CreationTime, page, pageSize, false)
                : _articleRepository.GetList(p => p.IsActive && !p.IsDelete, p => p.CreationTime, page, pageSize, false);
            var model = new PublicSiteListViewModel<Article>
            {
                Title = IsEn && !string.IsNullOrWhiteSpace(bannerModule?.Title_EN) ? bannerModule.Title_EN
                    : (string.IsNullOrWhiteSpace(bannerModule?.Title) ? _localizer["新闻资讯"] : bannerModule.Title),
                SubTitle = IsEn && !string.IsNullOrWhiteSpace(bannerModule?.SubTitle_EN) ? bannerModule.SubTitle_EN
                    : (string.IsNullOrWhiteSpace(bannerModule?.SubTitle) ? _localizer["企业动态与最新资讯"] : bannerModule.SubTitle),
                BannerImage = bannerImages.FirstOrDefault(),
                BannerImages = bannerImages,
                Items = result.List,
                Categories = GetNewsCategories(),
                CurrentTagId = currentTagId,
                CurrentCategory = category,
                PageIndex = page,
                PageSize = pageSize,
                TotalCount = result.Count
            };

            return View(model);
        }

        public IActionResult Article(int id)
        {
            LoadSiteConfig();
            var idText = id.ToString();
            var article = _articleRepository.GetOne(p => p.IsActive && !p.IsDelete && (p.Id == id || p.SourceUrl.Contains(idText)));
            if (article == null)
            {
                return NotFound();
            }

            article.ViewCount += 1;
            _articleRepository.Update(article);
            ViewData["ArticleTagName"] = GetNewsCategoryName(article.TagId);
            ViewData["ArticleTagRoute"] = GetNewsCategoryRoute(article.TagId);

            return View(article);
        }

        [Authorize]
        public IActionResult ArticlePreview(int id)
        {
            LoadSiteConfig();
            var article = _articleRepository.GetOne(p => !p.IsDelete && p.Id == id);
            if (article == null)
            {
                return NotFound();
            }

            ViewData["ArticleTagName"] = GetNewsCategoryName(article.TagId);
            ViewData["ArticleTagRoute"] = GetNewsCategoryRoute(article.TagId);
            return View("Article", article);
        }

        public IActionResult Jobs()
        {
            LoadSiteConfig();
            var jobs = _jobRepository.GetList(p => p.IsActive && !p.IsDelete, p => p.CreationTime, false);
            var bannerModule = GetActiveModule("jobs", "banner");
            var bannerImages = GetBannerImages("jobs", "/syle/images/44555004.jpeg");
            ViewData["JobsSettings"] = ParseSettings(bannerModule);
            ViewData["JobsModule"] = bannerModule;
            var model = new PublicSiteListViewModel<Job>
            {
                Title = IsEn && !string.IsNullOrWhiteSpace(bannerModule?.Title_EN) ? bannerModule.Title_EN
                    : (string.IsNullOrWhiteSpace(bannerModule?.Title) ? _localizer["人才招聘"] : bannerModule.Title),
                SubTitle = IsEn && !string.IsNullOrWhiteSpace(bannerModule?.SubTitle_EN) ? bannerModule.SubTitle_EN
                    : (string.IsNullOrWhiteSpace(bannerModule?.SubTitle) ? _localizer["与中集洋山一起拓展物流装备制造的新可能"] : bannerModule.SubTitle),
                BannerImage = bannerImages.FirstOrDefault(),
                BannerImages = bannerImages,
                Items = jobs,
                PageIndex = 1,
                PageSize = jobs.Count,
                TotalCount = jobs.Count
            };

            return View(model);
        }

        private bool IsEn => System.Threading.Thread.CurrentThread.CurrentUICulture.Name.StartsWith("en");

        private JObject ParseSettings(SiteModule module)
        {
            var json = IsEn && !string.IsNullOrWhiteSpace(module?.SettingsJson_EN)
                ? module.SettingsJson_EN
                : module?.SettingsJson;

            if (string.IsNullOrWhiteSpace(json))
            {
                return null;
            }

            try
            {
                return JObject.Parse(json);
            }
            catch
            {
                return null;
            }
        }

        private T ParseSettings<T>(SiteModule module)
        {
            var json = IsEn && !string.IsNullOrWhiteSpace(module?.SettingsJson_EN)
                ? module.SettingsJson_EN
                : module?.SettingsJson;

            if (string.IsNullOrWhiteSpace(json))
            {
                return default(T);
            }

            try
            {
                return JsonConvert.DeserializeObject<T>(json);
            }
            catch
            {
                return default(T);
            }
        }

        private void LoadSiteConfig()
        {
            ViewData["SiteInfo"] = _siteInfoRepository.GetList().FirstOrDefault();
            ViewData["FooterInfo"] = _footerInfoRepository.GetList().FirstOrDefault();
            ViewData["Navigations"] = _navigationRepository.GetList(
                p => p.IsActive && p.IsShow && !p.IsDelete,
                p => p.Sort,
                true);
        }

        private List<Article> GetArticles(int top)
        {
            return _articleRepository.GetList(p => p.IsActive && !p.IsDelete, p => p.CreationTime, top, false);
        }

        private List<Album> GetAlbums(int tagId, int top)
        {
            return _albumRepository.GetList(p => p.IsActive && !p.IsDelete && p.TagId == tagId, p => p.Sort, top, true);
        }

        private List<SiteModule> GetModules(string pageKey)
        {
            return _moduleRepository.GetList(p => p.IsActive && !p.IsDelete && p.PageKey == pageKey, p => p.Sort, true);
        }

        private List<SiteModule> GetAllModules(string pageKey)
        {
            return _moduleRepository.GetList(p => !p.IsDelete && p.PageKey == pageKey, p => p.Sort, true);
        }

        private SiteModule GetActiveModule(string pageKey, string moduleKey)
        {
            return _moduleRepository.GetOne(p => p.IsActive && !p.IsDelete && p.PageKey == pageKey && p.ModuleKey == moduleKey);
        }

        private List<string> GetBannerImages(string pageKey, string fallback)
        {
            var module = GetActiveModule(pageKey, "banner");
            var images = SplitImages(module?.ImageUrl);
            if (images.Count == 0 && !string.IsNullOrWhiteSpace(fallback))
            {
                images.Add(fallback);
            }

            return images;
        }

        private List<string> ResolveModuleImages(SiteModule module, List<string> fallbackImages)
        {
            if (module == null)
            {
                return fallbackImages.ToList();
            }

            if (!module.IsActive)
            {
                return new List<string>();
            }

            return SplitImages(module.ImageUrl);
        }

        private List<string> SplitImages(string imageUrl)
        {
            if (string.IsNullOrWhiteSpace(imageUrl))
            {
                return new List<string>();
            }

            return imageUrl
                .Split(new[] { ',', ';', '\n', '\r' }, StringSplitOptions.RemoveEmptyEntries)
                .Select(p => p.Trim())
                .Where(p => !string.IsNullOrWhiteSpace(p))
                .Distinct(StringComparer.OrdinalIgnoreCase)
                .ToList();
        }

        private string Trim(string value, int maxLength)
        {
            if (string.IsNullOrWhiteSpace(value))
            {
                return string.Empty;
            }

            value = value.Trim();
            return value.Length <= maxLength ? value : value.Substring(0, maxLength);
        }

        private List<TagModel> GetNewsCategories()
        {
            return _tagRepository.GetList(
                LambdaHelper.True<Tag>().And(p => p.IsActive && p.TagType == (int)CIMC.Core.Enums.TagType.Article),
                p => p.Sort, 1, 100, true).List
                .Select(p => new TagModel { Id = p.Id, TagName = p.TagName, TagName_EN = p.TagName_EN, Sort = p.Sort })
                .ToList();
        }

        private int GetNewsTagId(string category)
        {
            var tags = GetNewsCategories();
            if (string.Equals(category, "company", StringComparison.OrdinalIgnoreCase) && tags.Count > 0)
            {
                return tags[0].Id;
            }

            if (string.Equals(category, "industry", StringComparison.OrdinalIgnoreCase) && tags.Count > 1)
            {
                return tags[1].Id;
            }

            return 0;
        }

        private string GetNewsCategoryName(int tagId)
        {
            var tag = _tagRepository.GetOne(tagId);
            if (tag == null) return _localizer["公司新闻"];
            return IsEn && !string.IsNullOrWhiteSpace(tag.TagName_EN) ? tag.TagName_EN : tag.TagName;
        }

        private string GetNewsCategoryRoute(int tagId)
        {
            var tags = GetNewsCategories();
            if (tags.Count > 1 && tagId == tags[1].Id) return "industry";
            return "company";
        }

        private string GetProductCategoryTitle(int tagId)
        {
            var tag = _tagRepository.GetOne(tagId);
            if (tag == null) return _localizer["传统业务线"];
            return IsEn && !string.IsNullOrWhiteSpace(tag.TagName_EN) ? tag.TagName_EN : tag.TagName;
        }

        private string GetProductCategoryType(int tagId)
        {
            return tagId switch
            {
                SpecialProductTag => "special",
                NewProductTag => "new",
                _ => "traditional"
            };
        }

        private string GetDefaultCategoryTitle(string selectedType)
        {
            return selectedType switch
            {
                "special" => _localizer["特种业务线"],
                "new" => _localizer["新业务线"],
                _ => _localizer["传统业务线"]
            };
        }

        private string GetDefaultCategoryIntro(string selectedType)
        {
            return selectedType switch
            {
                "special" => _localizer["围绕特种集装箱、模块化物流装备、定制化载具等场景，为客户提供结构设计、生产制造和交付服务。"],
                "new" => _localizer["中集洋山围绕智能化、绿色化、高端化，聚焦能源转型、低碳产业，布局新能源装备、储能装备、模块化建筑、环保装备等业务领域。"],
                _ => _localizer["具备全方位的标箱设计、生产制造能力，以标准干货集装箱为基础，设计生产 20GP、20HC、40GP、40HC 等多种常规箱型。"]
            };
        }

        [HttpPost]
        public IActionResult SetLanguage(string culture, string returnUrl)
        {
            Response.Cookies.Append(
                CookieRequestCultureProvider.DefaultCookieName,
                CookieRequestCultureProvider.MakeCookieValue(new RequestCulture(culture)),
                new CookieOptions { Expires = DateTimeOffset.UtcNow.AddYears(1) }
            );

            return LocalRedirect(returnUrl ?? "/");
        }
    }
}
