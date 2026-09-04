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
        private readonly IRepository<WebsitePage> _pageRepository;
        private readonly IRepository<WebsitePageVersion> _versionRepository;
        private readonly IRepository<WebsiteSiteConfig> _siteConfigRepository;
        private readonly IRepository<Article> _articleRepository;
        private readonly IRepository<ContentProduct> _productRepository;
        private readonly IRepository<ContentProductCategory> _productCategoryRepository;
        private readonly IRepository<ContentJob> _jobRepository;

        public HomeController(
            IRepository<WebsitePage> pageRepository,
            IRepository<WebsitePageVersion> versionRepository,
            IRepository<WebsiteSiteConfig> siteConfigRepository,
            IRepository<Article> articleRepository,
            IRepository<ContentProduct> productRepository,
            IRepository<ContentProductCategory> productCategoryRepository,
            IRepository<ContentJob> jobRepository)
        {
            _pageRepository = pageRepository;
            _versionRepository = versionRepository;
            _siteConfigRepository = siteConfigRepository;
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
            else
            {
                products = _productRepository.GetList(p => !p.IsDelete && p.IsActive, p => p.Sort, true);
            }

            ViewBag.ProductList = products.Take(20).ToList();
            ViewBag.Categories = _productCategoryRepository.GetList(c => !c.IsDelete && c.IsActive && c.Pid == 0, c => c.Sort, true);
            ViewBag.CurrentCategory = category;
            return View("Index", model);
        }

        public IActionResult ProductDetail(int id)
        {
            if (!SiteEnabled()) return View("NotFound");
            var product = _productRepository.GetOne(id);
            if (product == null || product.IsDelete || !product.IsActive) return NotFound();
            LoadCommonViewBag(Request.Path.Value);

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
            if (!SiteEnabled()) return View("NotFound");
            var article = _articleRepository.GetOne(id);
            if (article == null || article.IsDelete || !article.IsActive) return NotFound();
            LoadCommonViewBag(Request.Path.Value);
            article.ViewCount = article.ViewCount + 1;
            _articleRepository.Update(article);
            return View(article);
        }

        public IActionResult ArticlePreview(int id)
        {
            var article = _articleRepository.GetOne(id);
            if (article == null || article.IsDelete) return NotFound();
            LoadCommonViewBag(Request.Path.Value);
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

        /// <summary>
        /// 支持页面管理中新建的任意自定义路径，例如 /about/company。
        /// </summary>
        public IActionResult DynamicPage(string path)
        {
            var normalized = NormalizePath(path);
            var model = BuildPage(p => p.PagePath == normalized && !p.IsDelete);
            return model == null ? View("NotFound") : View("Index", model);
        }

        private PageRenderModel BuildPage(Expression<Func<WebsitePage, bool>> predicate)
        {
            var siteConfig = _siteConfigRepository.GetOne(1);
            if (siteConfig != null && (!siteConfig.IsActive || siteConfig.IsDelete)) return null;

            var page = _pageRepository.GetOne(predicate);
            if (page == null || page.Status != 1 || !page.IsActive || IsGlobalPage(page)) return null;

            var document = LoadPublishedDocument(page);
            if (document == null) return null;

            var navigation = BuildNavigationTree(page.PagePath);
            var model = new PageRenderModel
            {
                PageId = page.Id,
                PageName = page.PageName,
                PagePath = page.PagePath,
                PageTitle = page.PageTitle,
                SeoKeywords = page.SeoKeywords,
                SeoDescription = page.SeoDescription,
                Document = document,
                HeaderDocument = LoadGlobalDocument(BuilderDocumentFactory.GlobalHeaderPageCode, BuilderDocumentFactory.CreateDefaultHeader()),
                FooterDocument = LoadGlobalDocument(BuilderDocumentFactory.GlobalFooterPageCode, BuilderDocumentFactory.CreateDefaultFooter()),
                SiteConfig = ToSiteConfigModel(siteConfig),
                Navigation = navigation
            };

            ViewData["Title"] = page.PageTitle ?? siteConfig?.BrowserTitle ?? siteConfig?.SiteName ?? "企业官网";
            ViewData["Keywords"] = page.SeoKeywords ?? siteConfig?.Keywords;
            ViewData["Description"] = page.SeoDescription ?? siteConfig?.Description;
            ViewBag.SiteConfig = model.SiteConfig;
            ViewBag.NavigationList = model.Navigation;
            ViewBag.HeaderDocument = model.HeaderDocument;
            ViewBag.FooterDocument = model.FooterDocument;
            ViewBag.NewsList = _articleRepository.GetList(a => !a.IsDelete && a.IsActive, a => a.CreationTime, false).Take(6).ToList();
            ViewBag.ProductList = _productRepository.GetList(p => !p.IsDelete && p.IsActive, p => p.Sort, true).Take(8).ToList();
            ViewBag.JobList = _jobRepository.GetList(j => !j.IsDelete && j.IsActive, j => j.Sort, true).ToList();
            return model;
        }

        private void LoadCommonViewBag(string currentPath)
        {
            var siteConfig = _siteConfigRepository.GetOne(1);
            var navList = BuildNavigationTree(currentPath);
            ViewData["Title"] = siteConfig?.BrowserTitle ?? siteConfig?.SiteName ?? "企业官网";
            ViewData["Keywords"] = siteConfig?.Keywords;
            ViewData["Description"] = siteConfig?.Description;
            ViewBag.SiteConfig = ToSiteConfigModel(siteConfig);
            ViewBag.NavigationList = navList;
            ViewBag.HeaderDocument = LoadGlobalDocument(BuilderDocumentFactory.GlobalHeaderPageCode, BuilderDocumentFactory.CreateDefaultHeader());
            ViewBag.FooterDocument = LoadGlobalDocument(BuilderDocumentFactory.GlobalFooterPageCode, BuilderDocumentFactory.CreateDefaultFooter());
        }

        private bool SiteEnabled()
        {
            var siteConfig = _siteConfigRepository.GetOne(1);
            return siteConfig == null || (siteConfig.IsActive && !siteConfig.IsDelete);
        }

        private List<NavigationModel> BuildNavigationTree(string currentPath)
        {
            var pages = _pageRepository
                .GetList(p => !p.IsDelete && p.IsActive && p.ShowInNavigation, p => p.Sort, true)
                .Where(p => !IsGlobalPage(p))
                .OrderBy(p => p.Sort)
                .ThenBy(p => p.Id)
                .ToList();

            var ids = pages.Select(p => p.Id).ToHashSet();
            var nodes = pages.ToDictionary(
                p => p.Id,
                p => new NavigationModel
                {
                    Id = p.Id,
                    Pid = ids.Contains(p.ParentId) ? p.ParentId : 0,
                    Title = string.IsNullOrWhiteSpace(p.NavigationTitle) ? p.PageName : p.NavigationTitle,
                    Path = p.PagePath,
                    Icon = p.NavigationIcon,
                    Target = p.NavigationTarget,
                    Sort = p.Sort,
                    IsShow = p.ShowInNavigation,
                    IsActive = p.IsActive,
                    IsCurrent = string.Equals(NormalizePath(currentPath), NormalizePath(p.PagePath), StringComparison.OrdinalIgnoreCase)
                });

            var roots = new List<NavigationModel>();
            foreach (var node in nodes.Values.OrderBy(n => n.Sort).ThenBy(n => n.Id))
            {
                if (node.Pid > 0 && nodes.TryGetValue(node.Pid, out var parent) && parent.Id != node.Id)
                {
                    parent.Children.Add(node);
                }
                else
                {
                    roots.Add(node);
                }
            }

            SortNavigation(roots);
            return roots;
        }

        private static void SortNavigation(List<NavigationModel> items)
        {
            items.Sort((a, b) => a.Sort != b.Sort ? a.Sort.CompareTo(b.Sort) : a.Id.CompareTo(b.Id));
            foreach (var item in items) SortNavigation(item.Children);
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
                IsActive = entity.IsActive
            };
        }

        private static bool IsGlobalPage(WebsitePage page)
        {
            if (page == null) return false;
            return (!string.IsNullOrWhiteSpace(page.PageCode) && page.PageCode.StartsWith("__GLOBAL_", StringComparison.OrdinalIgnoreCase))
                   || (!string.IsNullOrWhiteSpace(page.PagePath) && page.PagePath.StartsWith("/__global/", StringComparison.OrdinalIgnoreCase));
        }

        private static string NormalizePath(string path)
        {
            var value = (path ?? string.Empty).Trim();
            if (string.IsNullOrEmpty(value)) return "/";
            value = value.Replace("\\", "/");
            if (!value.StartsWith("/")) value = "/" + value;
            while (value.Contains("//")) value = value.Replace("//", "/");
            if (value.Length > 1) value = value.TrimEnd('/');
            return value;
        }
    }
}
