using CIMC.Data;
using CIMC.EntityFramework;
using CIMC.Helper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using MySite.Web.Models;

namespace MySite.Web.Controllers
{
    /// <summary>提供网站页面、产品、新闻及招聘的公开访问入口。</summary>
    public class HomeController : Controller
    {
        private readonly IRepository<WebsitePage> _pageRepository;
        private readonly IRepository<WebsitePageVersion> _versionRepository;
        private readonly IRepository<WebsiteSiteConfig> _siteConfigRepository;
        private readonly IRepository<Article> _articleRepository;
        private readonly IRepository<Product> _productRepository;
        private readonly IRepository<Tag> _productCategoryRepository;
        private readonly IRepository<Job> _jobRepository;
        private readonly IRepository<MessageBoard> _messageRepository;
        private readonly ICacheService _cache;

        /// <summary>初始化公开站点控制器及其数据仓储。</summary>
        public HomeController(
            IRepository<WebsitePage> pageRepository,
            IRepository<WebsitePageVersion> versionRepository,
            IRepository<WebsiteSiteConfig> siteConfigRepository,
            IRepository<Article> articleRepository,
            IRepository<Product> productRepository,
            IRepository<Tag> productCategoryRepository,
            IRepository<Job> jobRepository,
            IRepository<MessageBoard> messageRepository,
            ICacheService cache)
        {
            _pageRepository = pageRepository;
            _versionRepository = versionRepository;
            _siteConfigRepository = siteConfigRepository;
            _articleRepository = articleRepository;
            _productRepository = productRepository;
            _productCategoryRepository = productCategoryRepository;
            _jobRepository = jobRepository;
            _messageRepository = messageRepository;
            _cache = cache;
        }

        /// <summary>显示网站首页。</summary>
        public IActionResult Index()
        {
            var home = _pageRepository.GetOne(p => p.IsHome && !p.IsDelete && p.IsActive && p.Status == 1);
            var directoryRedirect = RedirectDirectory(home);
            if (directoryRedirect != null) return directoryRedirect;
            var model = BuildPage(p => p.IsHome && !p.IsDelete);
            return model == null ? View("NotFound") : View(model);
        }

        /// <summary>
        /// 为装修画布提供与公开站点相同的页面导航树。
        /// </summary>
        [Authorize]
        public IActionResult BuilderNavigation(string path = "/")
        {
            return Json(BuildNavigationTree(NormalizePath(path)));
        }

        /// <summary>
        /// 使用保存的页面及全局区域草稿预览；发布站点始终只读取各自发布版本。
        /// </summary>
        [Authorize]
        public IActionResult BuilderPreview(int id)
        {
            var page = _pageRepository.GetOne(id);
            if (page == null || page.IsDelete) return NotFound();
            if (!IsGlobalPage(page) && HasChildren(page)) return BadRequest("目录页面没有独立内容，请预览子页面。");

            BuilderDocumentModel document;
            if (string.IsNullOrWhiteSpace(page.ComponentJson))
            {
                document = new BuilderDocumentModel { Name = page.PageName };
            }
            else
            {
                try
                {
                    document = JsonConvert.DeserializeObject<BuilderDocumentModel>(page.ComponentJson);
                    if (document == null || document.SchemaVersion != 1) return BadRequest("页面结构版本不受支持");
                    document.Nodes = document.Nodes ?? new List<BuilderNodeModel>();
                }
                catch
                {
                    return BadRequest("页面草稿不是合法的 Builder 文档");
                }
            }

            var siteConfig = _siteConfigRepository.GetOne(1);
            var model = new PageRenderModel
            {
                PageId = page.Id,
                PageName = page.PageName,
                PagePath = page.PagePath,
                PageTitle = page.PageTitle,
                SeoKeywords = page.SeoKeywords,
                SeoDescription = page.SeoDescription,
                Document = document,
                HeaderDocument = LoadGlobalDocument(BuilderDocumentFactory.GlobalHeaderPageCode, previewDraft: true),
                FooterDocument = LoadGlobalDocument(BuilderDocumentFactory.GlobalFooterPageCode, previewDraft: true),
                SiteConfig = ToSiteConfigModel(siteConfig),
                Navigation = BuildNavigationTree(page.PagePath)
            };

            // Preview global drafts in the same surrounding page and layout as publication.
            if (IsGlobalPage(page))
            {
                var home = _pageRepository.GetOne(p => p.IsHome && !p.IsDelete && p.IsActive && p.Status == 1);
                model.Document = LoadPublishedDocument(home) ?? new BuilderDocumentModel();
                model.PagePath = home?.PagePath ?? "/";
                model.Navigation = BuildNavigationTree(model.PagePath);
                if (page.PageCode == BuilderDocumentFactory.GlobalHeaderPageCode) model.HeaderDocument = document;
                else if (page.PageCode == BuilderDocumentFactory.GlobalFooterPageCode) model.FooterDocument = document;
            }

            ViewData["Title"] = "草稿预览 - " + (page.PageTitle ?? page.PageName ?? "页面");
            ViewBag.IsBuilderPreview = true;
            ViewData["Keywords"] = page.SeoKeywords ?? siteConfig?.Keywords;
            ViewData["Description"] = page.SeoDescription ?? siteConfig?.Description;
            ViewBag.SiteConfig = model.SiteConfig;
            ViewBag.NavigationList = model.Navigation;
            ViewBag.HeaderDocument = model.HeaderDocument;
            ViewBag.FooterDocument = model.FooterDocument;
            ViewBag.NewsList = _articleRepository.GetList(a => !a.IsDelete && a.IsActive, a => a.CreationTime, false).Take(6).ToList();
            ViewBag.ProductList = _productRepository.GetList(p => !p.IsDelete && p.IsActive, p => p.Sort, true).Take(8).ToList();
            ViewBag.JobList = _jobRepository.GetList(j => !j.IsDelete && j.IsActive, j => j.CreationTime, false).ToList();
            return View("Index", model);
        }

        /// <summary>显示关于页面。</summary>
        public IActionResult About()
        {
            var directoryRedirect = RedirectDirectoryAtPath("/about");
            if (directoryRedirect != null) return directoryRedirect;
            var model = BuildPage(p => p.PagePath == "/about" && !p.IsDelete);
            return model == null ? View("NotFound") : View("Index", model);
        }

        /// <summary>显示产品页面和按分类筛选的产品列表。</summary>
        public IActionResult Products(string category)
        {
            var categoryPath = string.IsNullOrWhiteSpace(category) ? null : NormalizePath("/products/" + category);
            var directoryRedirect = RedirectDirectoryAtPath(categoryPath ?? "/products");
            if (directoryRedirect != null) return directoryRedirect;
            var model = categoryPath == null ? null : BuildPage(p => p.PagePath == categoryPath && !p.IsDelete);
            if (model == null && categoryPath != null)
            {
                directoryRedirect = RedirectDirectoryAtPath("/products");
                if (directoryRedirect != null) return directoryRedirect;
            }
            model ??= BuildPage(p => p.PagePath == "/products" && !p.IsDelete);
            if (model == null) return View("NotFound");

            List<Product> products;
            if (!string.IsNullOrWhiteSpace(category))
            {
                var categoryEntity = _productCategoryRepository.GetOne(c => c.TagName == category && c.TagType == (int)CIMC.Core.Enums.TagType.Product && c.IsActive);
                products = categoryEntity == null
                    ? new List<Product>()
                    : _productRepository.GetList(p => !p.IsDelete && p.IsActive && p.TagId == categoryEntity.Id, p => p.Sort, true);
            }
            else
            {
                products = _productRepository.GetList(p => !p.IsDelete && p.IsActive, p => p.Sort, true);
            }

            ViewBag.ProductList = products.Take(20).ToList();
            ViewBag.Categories = _productCategoryRepository.GetList(c => c.IsActive && c.TagType == (int)CIMC.Core.Enums.TagType.Product, c => c.Sort, true);
            ViewBag.CurrentCategory = category;
            return View("Index", model);
        }

        /// <summary>显示指定产品的详情。</summary>
        public IActionResult ProductDetail(int id)
        {
            if (!SiteEnabled()) return View("NotFound");
            var product = _productRepository.GetOne(id);
            if (product == null || product.IsDelete || !product.IsActive) return NotFound();
            LoadCommonViewBag(Request.Path.Value);

            ViewBag.ProductImages = string.IsNullOrWhiteSpace(product.ImageUrl) ? new List<string>() : new List<string> { product.ImageUrl };

            ViewBag.ProductCategory = product.TagId > 0
                ? _productCategoryRepository.GetOne(c => c.Id == product.TagId && c.IsActive)
                : null;
            return View(product);
        }

        /// <summary>显示新闻页面及新闻列表。</summary>
        public IActionResult News(string category)
        {
            var categoryPath = string.IsNullOrWhiteSpace(category) ? null : NormalizePath("/news/" + category);
            var directoryRedirect = RedirectDirectoryAtPath(categoryPath ?? "/news");
            if (directoryRedirect != null) return directoryRedirect;
            var model = categoryPath == null ? null : BuildPage(p => p.PagePath == categoryPath && !p.IsDelete);
            if (model == null && categoryPath != null)
            {
                directoryRedirect = RedirectDirectoryAtPath("/news");
                if (directoryRedirect != null) return directoryRedirect;
            }
            model ??= BuildPage(p => p.PagePath == "/news" && !p.IsDelete);
            if (model == null) return View("NotFound");
            ViewBag.NewsList = _articleRepository.GetList(a => !a.IsDelete && a.IsActive, a => a.CreationTime, false).Take(10).ToList();
            ViewBag.CurrentCategory = category;
            return View("Index", model);
        }

        /// <summary>显示指定文章的详情。</summary>
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

        /// <summary>预览指定文章。</summary>
        public IActionResult ArticlePreview(int id)
        {
            var article = _articleRepository.GetOne(id);
            if (article == null || article.IsDelete) return NotFound();
            LoadCommonViewBag(Request.Path.Value);
            return View("Article", article);
        }

        /// <summary>显示招聘页面及岗位列表。</summary>
        public IActionResult Jobs()
        {
            var directoryRedirect = RedirectDirectoryAtPath("/jobs");
            if (directoryRedirect != null) return directoryRedirect;
            var model = BuildPage(p => p.PagePath == "/jobs" && !p.IsDelete);
            if (model == null) return View("NotFound");
            ViewBag.JobList = _jobRepository.GetList(j => !j.IsDelete && j.IsActive, j => j.CreationTime, false);
            return View("Index", model);
        }

        /// <summary>显示联系页面。</summary>
        public IActionResult Contact()
        {
            var directoryRedirect = RedirectDirectoryAtPath("/contact");
            if (directoryRedirect != null) return directoryRedirect;
            var model = BuildPage(p => p.PagePath == "/contact" && !p.IsDelete);
            return model == null ? View("NotFound") : View("Index", model);
        }

        /// <summary>提交网站留言。</summary>
        [HttpPost]
        public IActionResult Message(MessageBoard input, string validateKey, string validateCode)
        {
            var result = new ResultModel { Code = (int)ResultCode.ParmsError, Message = "请完善留言信息" };
            var cacheKey = CacheKey.ValidateCode + (validateKey ?? string.Empty);
            var expectedCode = _cache.Get(cacheKey)?.ToString();
            if (string.IsNullOrWhiteSpace(validateKey)
                || string.IsNullOrWhiteSpace(validateCode)
                || !string.Equals(validateCode.Trim(), expectedCode, StringComparison.OrdinalIgnoreCase))
            {
                result.Message = "验证码错误或已过期";
                return Json(result);
            }

            if (input == null
                || string.IsNullOrWhiteSpace(input.UserName)
                || string.IsNullOrWhiteSpace(input.Phone)
                || string.IsNullOrWhiteSpace(input.Message))
            {
                return Json(result);
            }

            _cache.Remove(cacheKey);
            input.UserName = TrimTo(input.UserName, 100);
            input.Phone = TrimTo(input.Phone, 50);
            input.Email = TrimTo(input.Email, 250);
            input.Message = TrimTo(input.Message, 1000);
            input.IsRead = false;
            input.CreationTime = DateTime.Now;
            _messageRepository.Add(input);

            result.Code = (int)ResultCode.Success;
            result.Message = "留言提交成功，我们会尽快与您联系";
            return Json(result);
        }

        /// <summary>
        /// 支持页面管理中新建的任意自定义路径，例如 /about/company。
        /// MapFallbackToController 会将匹配到的 path 路由值传入该参数。
        /// </summary>
        public IActionResult DynamicPage(string path)
        {
            var normalized = NormalizePath(path);
            var directoryRedirect = RedirectDirectoryAtPath(normalized);
            if (directoryRedirect != null) return directoryRedirect;
            var model = BuildPage(p => p.PagePath == normalized && !p.IsDelete);
            return model == null ? View("NotFound") : View("Index", model);
        }

        /// <summary>加载页面的发布版本并构建渲染模型。</summary>
        private PageRenderModel BuildPage(Expression<Func<WebsitePage, bool>> predicate)
        {
            var siteConfig = _siteConfigRepository.GetOne(1);
            if (siteConfig != null && (!siteConfig.IsActive || siteConfig.IsDelete)) return null;

            var page = _pageRepository.GetOne(predicate);
            if (page == null || page.Status != 1 || !page.IsActive || IsGlobalPage(page)) return null;
            if (HasChildren(page)) return null;

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
                HeaderDocument = LoadGlobalDocument(BuilderDocumentFactory.GlobalHeaderPageCode),
                FooterDocument = LoadGlobalDocument(BuilderDocumentFactory.GlobalFooterPageCode),
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
            ViewBag.JobList = _jobRepository.GetList(j => !j.IsDelete && j.IsActive, j => j.CreationTime, false).ToList();
            return model;
        }

        /// <summary>加载页面公共区域所需的视图数据。</summary>
        private void LoadCommonViewBag(string currentPath)
        {
            var siteConfig = _siteConfigRepository.GetOne(1);
            var navList = BuildNavigationTree(currentPath);
            ViewData["Title"] = siteConfig?.BrowserTitle ?? siteConfig?.SiteName ?? "企业官网";
            ViewData["Keywords"] = siteConfig?.Keywords;
            ViewData["Description"] = siteConfig?.Description;
            ViewBag.SiteConfig = ToSiteConfigModel(siteConfig);
            ViewBag.NavigationList = navList;
            ViewBag.HeaderDocument = LoadGlobalDocument(BuilderDocumentFactory.GlobalHeaderPageCode);
            ViewBag.FooterDocument = LoadGlobalDocument(BuilderDocumentFactory.GlobalFooterPageCode);
        }

        /// <summary>检查站点是否启用。</summary>
        private bool SiteEnabled()
        {
            var siteConfig = _siteConfigRepository.GetOne(1);
            return siteConfig == null || (siteConfig.IsActive && !siteConfig.IsDelete);
        }

        /// <summary>检查页面是否包含子页面。</summary>
        private bool HasChildren(WebsitePage page)
        {
            return page != null && _pageRepository.GetList(p => p.ParentId == page.Id && !p.IsDelete).Any();
        }

        /// <summary>将目录路径重定向到第一个可访问的内容页面。</summary>
        private IActionResult RedirectDirectoryAtPath(string path)
        {
            if (string.IsNullOrEmpty(path)) return null;
            var page = _pageRepository.GetOne(p => p.PagePath == path && !p.IsDelete && p.IsActive && p.Status == 1);
            return RedirectDirectory(page);
        }

        /// <summary>将目录页面重定向到第一个可访问的内容页面。</summary>
        private IActionResult RedirectDirectory(WebsitePage page)
        {
            if (page == null || IsGlobalPage(page) || !HasChildren(page)) return null;
            var targetPath = FindFirstContentPath(page, new HashSet<int>());
            return targetPath == null ? NotFound() : Redirect(targetPath);
        }

        /// <summary>递归查找目录下第一个可访问的内容路径。</summary>
        private string FindFirstContentPath(WebsitePage page, HashSet<int> visited)
        {
            if (!visited.Add(page.Id)) return null;
            if (!HasChildren(page)) return page.PagePath;
            var children = _pageRepository.GetList(p => p.ParentId == page.Id && !p.IsDelete && p.IsActive && p.Status == 1, p => p.Sort, true)
                .Where(p => !IsGlobalPage(p))
                .OrderByDescending(p => p.ShowInNavigation).ThenBy(p => p.Sort).ThenBy(p => p.Id);
            foreach (var child in children)
            {
                var targetPath = FindFirstContentPath(child, visited);
                if (targetPath != null) return targetPath;
            }
            return null;
        }

        /// <summary>从页面数据构建站点导航树。</summary>
        private List<NavigationModel> BuildNavigationTree(string currentPath)
        {
            var allPages = _pageRepository
                .GetList(p => !p.IsDelete && p.IsActive && p.Status == 1, p => p.Sort, true)
                .Where(p => !IsGlobalPage(p))
                .OrderBy(p => p.Sort)
                .ThenBy(p => p.Id)
                .ToList();

            var byId = allPages.ToDictionary(p => p.Id);
            bool IsVisible(WebsitePage page)
            {
                if (!page.ShowInNavigation) return false;
                var current = page;
                var guard = 0;
                while (current.ParentId > 0 && guard++ < 100)
                {
                    if (!byId.TryGetValue(current.ParentId, out var parent)) return false;
                    if (!parent.ShowInNavigation) return false;
                    current = parent;
                }
                return true;
            }

            // 目录没有可访问的已发布内容时，不在导航中显示。
            var directoryTargets = allPages.Where(HasChildren)
                .ToDictionary(p => p.Id, p => FindFirstContentPath(p, new HashSet<int>()));
            var pages = allPages.Where(p => IsVisible(p) &&
                (!directoryTargets.TryGetValue(p.Id, out var target) || target != null)).ToList();
            var ids = pages.Select(p => p.Id).ToHashSet();
            var nodes = pages.ToDictionary(
                p => p.Id,
                p => new NavigationModel
                {
                    Id = p.Id,
                    Pid = ids.Contains(p.ParentId) ? p.ParentId : 0,
                    Title = string.IsNullOrWhiteSpace(p.NavigationTitle) ? p.PageName : p.NavigationTitle,
                    Path = directoryTargets.TryGetValue(p.Id, out var targetPath) ? targetPath : p.PagePath,
                    Icon = p.NavigationIcon,
                    Target = p.NavigationTarget,
                    Sort = p.Sort,
                    IsShow = true,
                    IsActive = true,
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
            SetDirectoryTargets(roots);
            return roots;
        }

        /// <summary>让目录导航项指向首个子页面。</summary>
        private static void SetDirectoryTargets(List<NavigationModel> items)
        {
            foreach (var item in items)
            {
                SetDirectoryTargets(item.Children);
                if (item.Children.Count == 0) continue;
                item.Path = item.Children[0].Path;
                item.IsCurrent |= item.Children.Any(child => child.IsCurrent);
            }
        }

        /// <summary>按排序值和主键排列导航项。</summary>
        private static void SortNavigation(List<NavigationModel> items)
        {
            items.Sort((a, b) => a.Sort != b.Sort ? a.Sort.CompareTo(b.Sort) : a.Id.CompareTo(b.Id));
            foreach (var item in items) SortNavigation(item.Children);
        }

        /// <summary>读取页面的发布文档。</summary>
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

        /// <summary>读取全局区域的草稿或发布文档。</summary>
        private BuilderDocumentModel LoadGlobalDocument(string code, bool previewDraft = false)
        {
            var empty = new BuilderDocumentModel();
            var page = _pageRepository.GetOne(p => p.PageCode == code && !p.IsDelete && p.IsActive);
            if (page == null) return empty;
            if (!previewDraft) return page.Status == 1 ? LoadPublishedDocument(page) ?? empty : empty;
            try
            {
                var draft = JsonConvert.DeserializeObject<BuilderDocumentModel>(page.ComponentJson ?? "{}");
                return draft?.SchemaVersion == 1 ? draft : empty;
            }
            catch (JsonException) { return empty; }
        }

        /// <summary>将站点配置实体转换为页面模型。</summary>
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

        /// <summary>判断页面是否为全局区域。</summary>
        private static bool IsGlobalPage(WebsitePage page)
        {
            if (page == null) return false;
            return (!string.IsNullOrWhiteSpace(page.PageCode) && page.PageCode.StartsWith("__GLOBAL_", StringComparison.OrdinalIgnoreCase))
                   || (!string.IsNullOrWhiteSpace(page.PagePath) && page.PagePath.StartsWith("/__global/", StringComparison.OrdinalIgnoreCase));
        }

        /// <summary>规范化网站页面路径。</summary>
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

        /// <summary>将文本截断到指定的最大长度。</summary>
        private static string TrimTo(string value, int maxLength)
        {
            var text = (value ?? string.Empty).Trim();
            return text.Length <= maxLength ? text : text.Substring(0, maxLength);
        }
    }
}
