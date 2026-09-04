using CIMC.Data;
using CIMC.EntityFramework;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using MySite.Web.Models;

namespace MySite.Web.Controllers
{
    public class HomeController : Controller
    {
        public const string GlobalHeaderCode = "__GLOBAL_HEADER__";
        public const string GlobalFooterCode = "__GLOBAL_FOOTER__";

        private readonly IRepository<WebsitePage> _pageRepository;
        private readonly IRepository<WebsitePageVersion> _versionRepository;
        private readonly IRepository<WebsiteSiteConfig> _siteConfigRepository;
        private readonly IRepository<WebsiteNavigation> _navigationRepository;
        private readonly IRepository<Article> _articleRepository;
        private readonly IRepository<ContentProduct> _productRepository;
        private readonly IRepository<ContentProductCategory> _productCategoryRepository;
        private readonly IRepository<ContentJob> _jobRepository;

        public HomeController(
            IRepository<WebsitePage> pageRepository,
            IRepository<WebsitePageVersion> versionRepository,
            IRepository<WebsiteSiteConfig> siteConfigRepository,
            IRepository<WebsiteNavigation> navigationRepository,
            IRepository<Article> articleRepository,
            IRepository<ContentProduct> productRepository,
            IRepository<ContentProductCategory> productCategoryRepository,
            IRepository<ContentJob> jobRepository)
        {
            _pageRepository = pageRepository;
            _versionRepository = versionRepository;
            _siteConfigRepository = siteConfigRepository;
            _navigationRepository = navigationRepository;
            _articleRepository = articleRepository;
            _productRepository = productRepository;
            _productCategoryRepository = productCategoryRepository;
            _jobRepository = jobRepository;
        }

        public IActionResult Index()
        {
            var model = BuildPage(p => p.IsHome && !p.IsDelete);
            return model == null ? View("NotFound") : View(model);
        }

        public IActionResult About()
        {
            var model = BuildPage(p => p.PagePath == "/about" && !p.IsDelete);
            return model == null ? View("NotFound") : View("Index", model);
        }

        public IActionResult Products(string category)
        {
            var model = BuildPage(p => p.PagePath == "/products" && !p.IsDelete);
            if (model == null) return View("NotFound");

            List<ContentProduct> products;
            if (!string.IsNullOrWhiteSpace(category))
            {
                var categoryEntity = _productCategoryRepository.GetOne(c => c.Name == category && !c.IsDelete && c.IsActive);
                products = categoryEntity == null
                    ? new List<ContentProduct>()
                    : _productRepository.GetList(p => !p.IsDelete && p.IsActive && p.CategoryId == categoryEntity.Id, p => p.Sort, true);
            }
            else products = _productRepository.GetList(p => !p.IsDelete && p.IsActive, p => p.Sort, true);

            ViewBag.ProductList = products.Take(20).ToList();
            ViewBag.Categories = _productCategoryRepository.GetList(c => !c.IsDelete && c.IsActive && c.Pid == 0, c => c.Sort, true);
            ViewBag.CurrentCategory = category;
            return View("Index", model);
        }

        public IActionResult ProductDetail(int id)
        {
            var product = _productRepository.GetOne(id);
            if (product == null || product.IsDelete || !product.IsActive) return NotFound();
            LoadCommonViewBag();

            if (!string.IsNullOrEmpty(product.ImageList))
            {
                try { ViewBag.ProductImages = JsonConvert.DeserializeObject<List<string>>(product.ImageList) ?? new List<string>(); }
                catch { ViewBag.ProductImages = new List<string>(); }
            }
            else ViewBag.ProductImages = new List<string>();

            ViewBag.ProductCategory = product.CategoryId > 0
                ? _productCategoryRepository.GetOne(c => c.Id == product.CategoryId && !c.IsDelete && c.IsActive)
                : null;
            return View(product);
        }

        public IActionResult News(string category)
        {
            var model = BuildPage(p => p.PagePath == "/news" && !p.IsDelete);
            if (model == null) return View("NotFound");
            ViewBag.NewsList = _articleRepository.GetList(a => !a.IsDelete && a.IsActive, a => a.CreationTime, false).Take(10).ToList();
            ViewBag.CurrentCategory = category;
            return View("Index", model);
        }

        public IActionResult Article(int id)
        {
            var article = _articleRepository.GetOne(id);
            if (article == null || article.IsDelete || !article.IsActive) return NotFound();
            LoadCommonViewBag();
            article.ViewCount = article.ViewCount + 1;
            _articleRepository.Update(article);
            return View(article);
        }

        public IActionResult ArticlePreview(int id)
        {
            var article = _articleRepository.GetOne(id);
            if (article == null || article.IsDelete) return NotFound();
            LoadCommonViewBag();
            return View("Article", article);
        }

        public IActionResult Jobs()
        {
            var model = BuildPage(p => p.PagePath == "/jobs" && !p.IsDelete);
            if (model == null) return View("NotFound");
            ViewBag.JobList = _jobRepository.GetList(j => !j.IsDelete && j.IsActive, j => j.Sort, true);
            return View("Index", model);
        }

        public IActionResult Contact()
        {
            var model = BuildPage(p => p.PagePath == "/contact" && !p.IsDelete);
            return model == null ? View("NotFound") : View("Index", model);
        }

        private PageRenderModel BuildPage(Expression<Func<WebsitePage, bool>> predicate)
        {
            var page = _pageRepository.GetOne(predicate);
            if (page == null || page.Status != 1 || !page.IsActive) return null;

            var document = LoadPublishedDocument(page);
            if (document == null) return null;

            var siteConfig = _siteConfigRepository.GetOne(1);
            var navigations = _navigationRepository.GetList(n => !n.IsDelete && n.IsActive && n.IsShow, n => n.Sort, true);
            var model = new PageRenderModel
            {
                PageId = page.Id,
                PageName = page.PageName,
                PagePath = page.PagePath,
                PageTitle = page.PageTitle,
                SeoKeywords = page.SeoKeywords,
                SeoDescription = page.SeoDescription,
                Document = document,
                HeaderDocument = LoadGlobalDocument(GlobalHeaderCode, CreateDefaultHeaderDocument()),
                FooterDocument = LoadGlobalDocument(GlobalFooterCode, CreateDefaultFooterDocument()),
                SiteConfig = ToSiteConfigModel(siteConfig),
                Navigation = navigations.Select(ToNavigationModel).ToList()
            };

            ViewData["Title"] = page.PageTitle ?? (siteConfig == null ? null : siteConfig.BrowserTitle) ?? (siteConfig == null ? null : siteConfig.SiteName) ?? "企业官网";
            ViewData["Keywords"] = page.SeoKeywords ?? (siteConfig == null ? null : siteConfig.Keywords);
            ViewData["Description"] = page.SeoDescription ?? (siteConfig == null ? null : siteConfig.Description);
            ViewBag.SiteConfig = model.SiteConfig;
            ViewBag.NavigationList = model.Navigation;
            ViewBag.HeaderDocument = model.HeaderDocument;
            ViewBag.FooterDocument = model.FooterDocument;
            ViewBag.NewsList = _articleRepository.GetList(a => !a.IsDelete && a.IsActive, a => a.CreationTime, false).Take(6).ToList();
            ViewBag.ProductList = _productRepository.GetList(p => !p.IsDelete && p.IsActive, p => p.Sort, true).Take(8).ToList();
            ViewBag.JobList = _jobRepository.GetList(j => !j.IsDelete && j.IsActive, j => j.Sort, true).ToList();
            return model;
        }

        private void LoadCommonViewBag()
        {
            var siteConfig = _siteConfigRepository.GetOne(1);
            var navigations = _navigationRepository.GetList(n => !n.IsDelete && n.IsActive && n.IsShow, n => n.Sort, true);
            var navList = navigations.Select(ToNavigationModel).ToList();
            ViewData["Title"] = siteConfig == null ? "企业官网" : (siteConfig.BrowserTitle ?? siteConfig.SiteName ?? "企业官网");
            ViewData["Keywords"] = siteConfig == null ? null : siteConfig.Keywords;
            ViewData["Description"] = siteConfig == null ? null : siteConfig.Description;
            ViewBag.SiteConfig = ToSiteConfigModel(siteConfig);
            ViewBag.NavigationList = navList;
            ViewBag.HeaderDocument = LoadGlobalDocument(GlobalHeaderCode, CreateDefaultHeaderDocument());
            ViewBag.FooterDocument = LoadGlobalDocument(GlobalFooterCode, CreateDefaultFooterDocument());
        }

        private BuilderDocumentModel LoadPublishedDocument(WebsitePage page)
        {
            if (page == null) return null;
            var version = _versionRepository.GetList(v => v.PageId == page.Id && v.Status == 1).OrderByDescending(v => v.VersionNo).FirstOrDefault();
            var json = version == null ? page.ComponentJson : version.PublishJson;
            if (string.IsNullOrWhiteSpace(json)) return new BuilderDocumentModel { Name = page.PageName };
            try
            {
                var document = JsonConvert.DeserializeObject<BuilderDocumentModel>(json);
                if (document == null || document.SchemaVersion != 1) return null;
                document.Nodes = document.Nodes ?? new List<BuilderNodeModel>();
                return document;
            }
            catch { return null; }
        }

        private BuilderDocumentModel LoadGlobalDocument(string code, BuilderDocumentModel fallback)
        {
            var page = _pageRepository.GetOne(p => p.PageCode == code && !p.IsDelete && p.IsActive && p.Status == 1);
            return LoadPublishedDocument(page) ?? fallback;
        }

        private static BuilderDocumentModel CreateDefaultHeaderDocument()
        {
            return new BuilderDocumentModel
            {
                Name = "Header",
                Nodes = new List<BuilderNodeModel>
                {
                    Node("section", null, new Dictionary<string, object>{{"paddingTop","16px"},{"paddingBottom","16px"},{"backgroundColor","#ffffff"}},
                        Node("container", null, null,
                            Node("grid", new Dictionary<string, object>{{"columns",3}}, new Dictionary<string, object>{{"gap","20px"}},
                                Node("column", null, null, Node("logo", new Dictionary<string, object>{{"text","企业名称"},{"href","/"}}, null)),
                                Node("column", null, new Dictionary<string, object>{{"textAlign","center"}}, Node("navigation", new Dictionary<string, object>{{"menuKey","main"}}, null)),
                                Node("column", null, new Dictionary<string, object>{{"textAlign","right"}}, Node("button", new Dictionary<string, object>{{"text","联系我们"},{"href","/contact"},{"variant","outline"}}, null)))))
                }
            };
        }

        private static BuilderDocumentModel CreateDefaultFooterDocument()
        {
            return new BuilderDocumentModel
            {
                Name = "Footer",
                Nodes = new List<BuilderNodeModel>
                {
                    Node("section", null, new Dictionary<string, object>{{"paddingTop","48px"},{"paddingBottom","24px"},{"backgroundColor","#111827"},{"color","#ffffff"}},
                        Node("container", null, null,
                            Node("grid", new Dictionary<string, object>{{"columns",3}}, new Dictionary<string, object>{{"gap","36px"}},
                                Node("column", null, null, Node("logo", new Dictionary<string, object>{{"text","企业名称"},{"href","/"}}, null)),
                                Node("column", null, null, Node("navigation", new Dictionary<string, object>{{"menuKey","footer"},{"direction","vertical"}}, null)),
                                Node("column", null, null, Node("contact", new Dictionary<string, object>(), null))),
                            Node("divider", null, null),
                            Node("copyright", new Dictionary<string, object>{{"text","© 2026 企业名称 版权所有"}}, new Dictionary<string, object>{{"textAlign","center"}})))
                }
            };
        }

        private static BuilderNodeModel Node(string type, Dictionary<string, object> props, Dictionary<string, object> style, params BuilderNodeModel[] children)
        {
            return new BuilderNodeModel
            {
                Id = type + "_" + Guid.NewGuid().ToString("N").Substring(0, 10),
                Type = type,
                Name = type,
                Props = props ?? new Dictionary<string, object>(),
                Style = style ?? new Dictionary<string, object>(),
                Children = children == null ? new List<BuilderNodeModel>() : children.ToList()
            };
        }

        private static SiteConfigModel ToSiteConfigModel(WebsiteSiteConfig entity)
        {
            if (entity == null) return new SiteConfigModel();
            return new SiteConfigModel
            {
                Id = entity.Id,
                SiteName = entity.SiteName,
                Logo = entity.Logo,
                BrowserTitle = entity.BrowserTitle,
                Keywords = entity.Keywords,
                Description = entity.Description,
                Theme = entity.Theme,
                Language = entity.Language,
                IsActive = entity.IsActive
            };
        }

        private static NavigationModel ToNavigationModel(WebsiteNavigation entity)
        {
            if (entity == null) return new NavigationModel();
            return new NavigationModel
            {
                Id = entity.Id,
                Pid = entity.Pid,
                Title = entity.Title,
                Path = entity.Path,
                Icon = entity.Icon,
                Target = entity.Target,
                Sort = entity.Sort,
                IsShow = entity.IsShow,
                IsActive = entity.IsActive
            };
        }
    }
}