using CIMC.Data;
using System.Collections.Generic;

namespace CimcSite.Web.Models
{
    public class PublicSiteHomeViewModel
    {
        public SiteInfo SiteInfo { get; set; }
        public SiteModule HeroModule { get; set; }
        public List<string> HeroImages { get; set; } = new List<string>();
        public List<Article> News { get; set; } = new List<Article>();
        public List<Job> Jobs { get; set; } = new List<Job>();
        public List<Album> TraditionalProducts { get; set; } = new List<Album>();
        public List<Album> SpecialProducts { get; set; } = new List<Album>();
        public List<Album> NewProducts { get; set; } = new List<Album>();
        public List<Album> Partners { get; set; } = new List<Album>();
        public List<SiteModule> Modules { get; set; } = new List<SiteModule>();
    }

    public class PublicSiteListViewModel<T>
    {
        public string Title { get; set; }
        public string SubTitle { get; set; }
        public string BannerImage { get; set; }
        public List<string> BannerImages { get; set; } = new List<string>();
        public List<T> Items { get; set; } = new List<T>();
        public List<TagModel> Categories { get; set; } = new List<TagModel>();
        public int CurrentTagId { get; set; }
        public string CurrentCategory { get; set; }
        public int PageIndex { get; set; }
        public int PageSize { get; set; }
        public int TotalCount { get; set; }
        public int TotalPages => PageSize <= 0 ? 0 : (TotalCount + PageSize - 1) / PageSize;
    }

    public class PublicProductViewModel
    {
        public string Title { get; set; }
        public string Intro { get; set; }
        public string BannerTitle { get; set; }
        public string BannerSubTitle { get; set; }
        public string BannerImage { get; set; }
        public List<string> BannerImages { get; set; } = new List<string>();
        public string Type { get; set; }
        public List<Album> Products { get; set; } = new List<Album>();
    }

    public class PublicProductDetailViewModel
    {
        public Album Product { get; set; }
        public string CategoryTitle { get; set; }
        public string CategoryType { get; set; }
        public List<Album> RelatedProducts { get; set; } = new List<Album>();
    }

    public class PublicAboutViewModel
    {
        public string BannerTitle { get; set; }
        public string BannerSubTitle { get; set; }
        public string BannerImage { get; set; }
        public List<string> BannerImages { get; set; } = new List<string>();
        public List<Album> Certificates { get; set; } = new List<Album>();
        public List<SiteModule> Modules { get; set; } = new List<SiteModule>();
    }

    public class PublicContactViewModel
    {
        public string BannerTitle { get; set; }
        public string BannerSubTitle { get; set; }
        public string BannerImage { get; set; }
        public List<string> BannerImages { get; set; } = new List<string>();
        public List<SiteModule> Modules { get; set; } = new List<SiteModule>();
    }
}
