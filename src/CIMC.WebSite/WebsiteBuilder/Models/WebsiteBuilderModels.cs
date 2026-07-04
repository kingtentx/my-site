using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace MySite.Web.WebsiteBuilder.Models
{
    public enum WebsiteContentStatus
    {
        Draft = 0,
        Published = 1,
        Offline = 2,
        Closed = 3
    }

    [Table("website_site_config")]
    public class WebsiteSiteConfig
    {
        [Key]
        public int Id { get; set; }

        [MaxLength(100)]
        public string SiteName { get; set; }

        [MaxLength(500)]
        public string Logo { get; set; }

        [MaxLength(150)]
        public string BrowserTitle { get; set; }

        [MaxLength(500)]
        public string Keywords { get; set; }

        [MaxLength(1000)]
        public string Description { get; set; }

        [MaxLength(100)]
        public string IcpNo { get; set; }

        [MaxLength(100)]
        public string PoliceNo { get; set; }

        [MaxLength(50)]
        public string Tel { get; set; }

        [MaxLength(100)]
        public string Email { get; set; }

        [MaxLength(300)]
        public string Address { get; set; }

        [MaxLength(300)]
        public string Copyright { get; set; }

        [MaxLength(50)]
        public string Theme { get; set; }

        [MaxLength(20)]
        public string Language { get; set; }

        public bool IsEnabled { get; set; } = true;

        public DateTime CreateTime { get; set; } = DateTime.Now;

        public DateTime UpdateTime { get; set; } = DateTime.Now;
    }

    [Table("website_page")]
    public class WebsitePage
    {
        [Key]
        public int Id { get; set; }

        public int SiteId { get; set; }

        [Required]
        [MaxLength(100)]
        public string PageName { get; set; }

        [Required]
        [MaxLength(100)]
        public string PageCode { get; set; }

        [Required]
        [MaxLength(200)]
        public string PagePath { get; set; }

        [MaxLength(150)]
        public string PageTitle { get; set; }

        [MaxLength(500)]
        public string SeoKeywords { get; set; }

        [MaxLength(1000)]
        public string SeoDescription { get; set; }

        [MaxLength(500)]
        public string CanonicalUrl { get; set; }

        public string LayoutJson { get; set; }

        public string ComponentJson { get; set; }

        public string DraftJson { get; set; }

        public string PublishJson { get; set; }

        public int Status { get; set; } = (int)WebsiteContentStatus.Draft;

        public bool IsHome { get; set; }

        public int Sort { get; set; }

        public bool IsDeleted { get; set; }

        public DateTime CreateTime { get; set; } = DateTime.Now;

        public DateTime UpdateTime { get; set; } = DateTime.Now;

        public DateTime? PublishTime { get; set; }
    }

    [Table("website_page_version")]
    public class WebsitePageVersion
    {
        [Key]
        public int Id { get; set; }

        public int PageId { get; set; }

        public int VersionNo { get; set; }

        public string DraftJson { get; set; }

        public string PublishJson { get; set; }

        public int Status { get; set; }

        public DateTime CreateTime { get; set; } = DateTime.Now;

        public DateTime? PublishTime { get; set; }

        public int? CreateUserId { get; set; }
    }

    [Table("content_news_category")]
    public class ContentNewsCategory
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [MaxLength(100)]
        public string CategoryName { get; set; }

        [MaxLength(100)]
        public string CategoryCode { get; set; }

        public int Sort { get; set; }

        public bool IsEnabled { get; set; } = true;

        public DateTime CreateTime { get; set; } = DateTime.Now;
    }

    [Table("content_news")]
    public class ContentNews
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [MaxLength(200)]
        public string Title { get; set; }

        public int? CategoryId { get; set; }

        [MaxLength(500)]
        public string CoverImage { get; set; }

        [MaxLength(1000)]
        public string Summary { get; set; }

        public string Content { get; set; }

        [MaxLength(100)]
        public string Author { get; set; }

        [MaxLength(100)]
        public string Source { get; set; }

        [MaxLength(500)]
        public string Tags { get; set; }

        public bool IsTop { get; set; }

        public bool IsRecommend { get; set; }

        public int Status { get; set; } = (int)WebsiteContentStatus.Draft;

        public int ViewCount { get; set; }

        public DateTime? PublishTime { get; set; }

        public DateTime CreateTime { get; set; } = DateTime.Now;

        public DateTime UpdateTime { get; set; } = DateTime.Now;

        public bool IsDeleted { get; set; }
    }

    [Table("content_product_category")]
    public class ContentProductCategory
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [MaxLength(100)]
        public string CategoryName { get; set; }

        [MaxLength(100)]
        public string CategoryCode { get; set; }

        public int Sort { get; set; }

        public bool IsEnabled { get; set; } = true;

        public DateTime CreateTime { get; set; } = DateTime.Now;
    }

    [Table("content_product")]
    public class ContentProduct
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [MaxLength(200)]
        public string ProductName { get; set; }

        public int? CategoryId { get; set; }

        [MaxLength(500)]
        public string CoverImage { get; set; }

        public string ImageList { get; set; }

        [MaxLength(1000)]
        public string Summary { get; set; }

        public string Description { get; set; }

        public string Specification { get; set; }

        public string Feature { get; set; }

        public int Sort { get; set; }

        public bool IsRecommend { get; set; }

        public int Status { get; set; } = (int)WebsiteContentStatus.Draft;

        public DateTime CreateTime { get; set; } = DateTime.Now;

        public DateTime UpdateTime { get; set; } = DateTime.Now;

        public bool IsDeleted { get; set; }
    }

    [Table("content_job")]
    public class ContentJob
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [MaxLength(200)]
        public string JobTitle { get; set; }

        [MaxLength(100)]
        public string Department { get; set; }

        [MaxLength(100)]
        public string WorkLocation { get; set; }

        [MaxLength(100)]
        public string SalaryRange { get; set; }

        public int RecruitCount { get; set; }

        [MaxLength(100)]
        public string JobType { get; set; }

        public string Responsibilities { get; set; }

        public string Requirements { get; set; }

        [MaxLength(100)]
        public string ContactName { get; set; }

        [MaxLength(50)]
        public string ContactPhone { get; set; }

        [MaxLength(100)]
        public string ContactEmail { get; set; }

        public int Status { get; set; } = (int)WebsiteContentStatus.Draft;

        public DateTime? PublishTime { get; set; }

        public DateTime CreateTime { get; set; } = DateTime.Now;

        public DateTime UpdateTime { get; set; } = DateTime.Now;

        public bool IsDeleted { get; set; }
    }

    [Table("content_job_apply")]
    public class ContentJobApply
    {
        [Key]
        public int Id { get; set; }

        public int JobId { get; set; }

        [MaxLength(100)]
        public string ApplicantName { get; set; }

        [MaxLength(50)]
        public string Phone { get; set; }

        [MaxLength(100)]
        public string Email { get; set; }

        [MaxLength(500)]
        public string ResumeFile { get; set; }

        [MaxLength(1000)]
        public string Remark { get; set; }

        public int Status { get; set; }

        public DateTime CreateTime { get; set; } = DateTime.Now;
    }

    [Table("material_file")]
    public class MaterialFile
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [MaxLength(200)]
        public string FileName { get; set; }

        [Required]
        [MaxLength(500)]
        public string FileUrl { get; set; }

        [MaxLength(50)]
        public string FileType { get; set; }

        public long FileSize { get; set; }

        [MaxLength(100)]
        public string Category { get; set; }

        public bool IsDeleted { get; set; }

        public DateTime CreateTime { get; set; } = DateTime.Now;
    }

    public class WebsitePageDesignRequest
    {
        public string LayoutJson { get; set; }
        public string ComponentJson { get; set; }
        public string DraftJson { get; set; }
    }

    public class WebsitePageRenderModel
    {
        public WebsiteSiteConfig SiteConfig { get; set; }
        public WebsitePage Page { get; set; }
        public IList<ContentNews> News { get; set; } = new List<ContentNews>();
        public IList<ContentProduct> Products { get; set; } = new List<ContentProduct>();
        public IList<ContentJob> Jobs { get; set; } = new List<ContentJob>();
        public bool IsPreview { get; set; }
    }
}