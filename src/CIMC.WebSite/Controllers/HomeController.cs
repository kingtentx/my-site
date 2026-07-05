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
        private readonly IRepository<WebsiteNavigation> _navigationRepository;
        private readonly IRepository<WebsiteFooter> _footerRepository;
        private readonly IRepository<Article> _articleRepository;
        private readonly IRepository<ContentProduct> _productRepository;
        private readonly IRepository<ContentProductCategory> _productCategoryRepository;
        private readonly IRepository<ContentJob> _jobRepository;

        public HomeController(
            IRepository<WebsitePage> pageRepository,
            IRepository<WebsitePageVersion> versionRepository,
            IRepository<WebsiteSiteConfig> siteConfigRepository,
            IRepository<WebsiteNavigation> navigationRepository,
            IRepository<WebsiteFooter> footerRepository,
            IRepository<Article> articleRepository,
            IRepository<ContentProduct> productRepository,
            IRepository<ContentProductCategory> productCategoryRepository,
            IRepository<ContentJob> jobRepository)
        {
            _pageRepository = pageRepository;
            _versionRepository = versionRepository;
            _siteConfigRepository = siteConfigRepository;
            _navigationRepository = navigationRepository;
            _footerRepository = footerRepository;
            _articleRepository = articleRepository;
            _productRepository = productRepository;
            _productCategoryRepository = productCategoryRepository;
            _jobRepository = jobRepository;
        }

        public IActionResult Index()
        {
            var model = BuildPage(p => p.IsHome && !p.IsDelete);
            if (model == null)
            {
                return View("NotFound");
            }
            return View(model);
        }

        public IActionResult About()
        {
            var model = BuildPage(p => p.PagePath == "/about" && !p.IsDelete);
            if (model == null)
            {
                return View("NotFound");
            }
            return View("Index", model);
        }

        public IActionResult Products(string category)
        {
            var model = BuildPage(p => p.PagePath == "/products" && !p.IsDelete);
            if (model == null)
            {
                return View("NotFound");
            }

            List<ContentProduct> products;
            if (!string.IsNullOrWhiteSpace(category))
            {
                var catEntity = _productCategoryRepository.GetOne(c => c.Name == category && !c.IsDelete && c.IsActive);
                if (catEntity != null)
                {
                    products = _productRepository.GetList(p => !p.IsDelete && p.IsActive && p.CategoryId == catEntity.Id,
                        p => p.Sort, true);
                }
                else
                {
                    products = new List<ContentProduct>();
                }
            }
            else
            {
                products = _productRepository.GetList(p => !p.IsDelete && p.IsActive, p => p.Sort, true);
            }
            ViewBag.ProductList = products.Take(20).ToList();
            ViewBag.Categories = _productCategoryRepository.GetList(c => !c.IsDelete && c.IsActive && c.Pid == 0,
                c => c.Sort, true);
            ViewBag.CurrentCategory = category;
            return View("Index", model);
        }

        public IActionResult ProductDetail(int id)
        {
            var product = _productRepository.GetOne(id);
            if (product == null || product.IsDelete || !product.IsActive)
            {
                return NotFound();
            }

            LoadCommonViewBag();
            if (!string.IsNullOrEmpty(product.ImageList))
            {
                try
                {
                    ViewBag.ProductImages = JsonConvert.DeserializeObject<List<string>>(product.ImageList) ?? new List<string>();
                }
                catch
                {
                    ViewBag.ProductImages = new List<string>();
                }
            }
            else
            {
                ViewBag.ProductImages = new List<string>();
            }

            var category = product.CategoryId > 0
                ? _productCategoryRepository.GetOne(c => c.Id == product.CategoryId && !c.IsDelete && c.IsActive)
                : null;
            ViewBag.ProductCategory = category;

            return View(product);
        }

        public IActionResult News(string category)
        {
            var model = BuildPage(p => p.PagePath == "/news" && !p.IsDelete);
            if (model == null)
            {
                return View("NotFound");
            }

            var newsList = _articleRepository.GetList(a => !a.IsDelete && a.IsActive,
                a => a.CreationTime, false);
            ViewBag.NewsList = newsList.Take(10).ToList();
            ViewBag.CurrentCategory = category;
            return View("Index", model);
        }

        public IActionResult Article(int id)
        {
            var article = _articleRepository.GetOne(id);
            if (article == null || article.IsDelete || !article.IsActive)
            {
                return NotFound();
            }

            LoadCommonViewBag();
            article.ViewCount = article.ViewCount + 1;
            _articleRepository.Update(article);
            return View(article);
        }

        public IActionResult ArticlePreview(int id)
        {
            var article = _articleRepository.GetOne(id);
            if (article == null || article.IsDelete)
            {
                return NotFound();
            }

            LoadCommonViewBag();
            return View("Article", article);
        }

        public IActionResult Jobs()
        {
            var model = BuildPage(p => p.PagePath == "/jobs" && !p.IsDelete);
            if (model == null)
            {
                return View("NotFound");
            }

            var jobs = _jobRepository.GetList(j => !j.IsDelete && j.IsActive, j => j.Sort, true);
            ViewBag.JobList = jobs;
            return View("Index", model);
        }

        public IActionResult Contact()
        {
            var model = BuildPage(p => p.PagePath == "/contact" && !p.IsDelete);
            if (model == null)
            {
                return View("NotFound");
            }
            return View("Index", model);
        }

        private PageRenderModel BuildPage(Expression<Func<WebsitePage, bool>> predicate)
        {
            var page = _pageRepository.GetOne(predicate);
            if (page == null || page.Status != 1 || !page.IsActive)
            {
                return null;
            }

            var publishedVersion = _versionRepository.GetList(v => v.PageId == page.Id && v.Status == 1)
                .OrderByDescending(v => v.VersionNo)
                .FirstOrDefault();
            var componentJson = publishedVersion?.PublishJson ?? page.ComponentJson ?? "[]";

            List<ComponentModel> components;
            try
            {
                components = JsonConvert.DeserializeObject<List<ComponentModel>>(componentJson) ?? new List<ComponentModel>();
                components = components
                    .Where(c => c != null && !string.Equals(c.Type, "navigation", StringComparison.OrdinalIgnoreCase) && !string.Equals(c.Type, "footer", StringComparison.OrdinalIgnoreCase))
                    .ToList();
            }
            catch
            {
                components = new List<ComponentModel>();
            }

            var siteConfig = _siteConfigRepository.GetOne(1);
            var footer = _footerRepository.GetOne(1);
            var navigations = _navigationRepository.GetList(
                n => !n.IsDelete && n.IsActive && n.IsShow,
                n => n.Sort, true);

            var model = new PageRenderModel
            {
                PageId = page.Id,
                PageName = page.PageName,
                PagePath = page.PagePath,
                PageTitle = page.PageTitle,
                SeoKeywords = page.SeoKeywords,
                SeoDescription = page.SeoDescription,
                Components = components,
                SiteConfig = ToSiteConfigModel(siteConfig),
                Navigation = navigations.Select(n => ToNavigationModel(n)).ToList(),
                Footer = ToFooterModel(footer)
            };

            ViewData["Title"] = page.PageTitle ?? siteConfig?.BrowserTitle ?? siteConfig?.SiteName ?? "企业官网";
            ViewData["Keywords"] = page.SeoKeywords ?? siteConfig?.Keywords;
            ViewData["Description"] = page.SeoDescription ?? siteConfig?.Description;
            ViewBag.SiteConfig = model.SiteConfig;
            ViewBag.NavigationList = model.Navigation;
            ViewBag.Footer = model.Footer;
            ViewBag.NewsList = _articleRepository.GetList(a => !a.IsDelete && a.IsActive,
                a => a.CreationTime, false).Take(6).ToList();
            ViewBag.ProductList = _productRepository.GetList(p => !p.IsDelete && p.IsActive,
                p => p.Sort, true).Take(8).ToList();
            ViewBag.JobList = _jobRepository.GetList(j => !j.IsDelete && j.IsActive,
                j => j.Sort, true).ToList();

            return model;
        }

        private void LoadCommonViewBag()
        {
            var siteConfig = _siteConfigRepository.GetOne(1);
            var footer = _footerRepository.GetOne(1);
            var navigations = _navigationRepository.GetList(
                n => !n.IsDelete && n.IsActive && n.IsShow,
                n => n.Sort, true);

            var siteConfigModel = ToSiteConfigModel(siteConfig);
            var footerModel = ToFooterModel(footer);
            var navList = navigations.Select(n => ToNavigationModel(n)).ToList();

            ViewData["Title"] = siteConfigModel?.BrowserTitle ?? siteConfigModel?.SiteName ?? "企业官网";
            ViewData["Keywords"] = siteConfigModel?.Keywords;
            ViewData["Description"] = siteConfigModel?.Description;
            ViewBag.SiteConfig = siteConfigModel;
            ViewBag.NavigationList = navList;
            ViewBag.Footer = footerModel;
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
                IcpNo = entity.IcpNo,
                PoliceNo = entity.PoliceNo,
                Phone = entity.Phone,
                Email = entity.Email,
                Address = entity.Address,
                Copyright = entity.Copyright,
                Theme = entity.Theme,
                Language = entity.Language,
                HeaderBgColor = string.IsNullOrWhiteSpace(entity.HeaderBgColor) ? "#ffffff" : entity.HeaderBgColor,
                HeaderTextColor = string.IsNullOrWhiteSpace(entity.HeaderTextColor) ? "#333333" : entity.HeaderTextColor,
                HeaderActiveColor = string.IsNullOrWhiteSpace(entity.HeaderActiveColor) ? "#1e9fff" : entity.HeaderActiveColor,
                HeaderFixedTop = entity.HeaderFixedTop,
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

        private static FooterModel ToFooterModel(WebsiteFooter entity)
        {
            if (entity == null) return new FooterModel();
            return new FooterModel
            {
                Id = entity.Id,
                Logo = entity.Logo,
                CompanyName = entity.CompanyName,
                Intro = entity.Intro,
                Phone = entity.Phone,
                Email = entity.Email,
                Address = entity.Address,
                Qrcode = entity.Qrcode,
                IcpNo = entity.IcpNo,
                PoliceNo = entity.PoliceNo,
                Copyright = entity.Copyright,
                FriendLinks = entity.FriendLinks ?? "[]",
                BgColor = entity.BgColor ?? "#2c3e50",
                TextColor = entity.TextColor ?? "#ffffff",
                IsActive = entity.IsActive
            };
        }
    }
}