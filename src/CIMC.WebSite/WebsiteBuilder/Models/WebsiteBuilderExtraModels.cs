using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace MySite.Web.WebsiteBuilder.Models
{
    [Table("website_navigation")]
    public class WebsiteNavigation
    {
        [Key]
        public int Id { get; set; }
        public int SiteId { get; set; } = 1;
        public int? ParentId { get; set; }
        [Required, MaxLength(100)]
        public string Title { get; set; }
        [MaxLength(50)]
        public string LinkType { get; set; } = "page";
        public int? PageId { get; set; }
        [MaxLength(300)]
        public string LinkUrl { get; set; }
        [MaxLength(20)]
        public string Target { get; set; } = "_self";
        [MaxLength(100)]
        public string Icon { get; set; }
        public int Sort { get; set; }
        public bool IsEnabled { get; set; } = true;
        public bool IsDeleted { get; set; }
        public DateTime CreateTime { get; set; } = DateTime.Now;
        public DateTime UpdateTime { get; set; } = DateTime.Now;
    }

    [Table("website_banner")]
    public class WebsiteBanner
    {
        [Key]
        public int Id { get; set; }
        public int SiteId { get; set; } = 1;
        [Required, MaxLength(200)]
        public string Title { get; set; }
        [MaxLength(500)]
        public string Subtitle { get; set; }
        [MaxLength(500)]
        public string ImageUrl { get; set; }
        [MaxLength(500)]
        public string VideoUrl { get; set; }
        [MaxLength(100)]
        public string ButtonText { get; set; }
        [MaxLength(300)]
        public string ButtonLink { get; set; }
        [MaxLength(300)]
        public string LinkUrl { get; set; }
        public int Height { get; set; } = 520;
        public int Sort { get; set; }
        public bool AutoPlay { get; set; } = true;
        public int Interval { get; set; } = 5000;
        public bool IsEnabled { get; set; } = true;
        public bool IsDeleted { get; set; }
        public DateTime? BeginTime { get; set; }
        public DateTime? EndTime { get; set; }
        public DateTime CreateTime { get; set; } = DateTime.Now;
        public DateTime UpdateTime { get; set; } = DateTime.Now;
    }

    [Table("website_footer")]
    public class WebsiteFooter
    {
        [Key]
        public int Id { get; set; }
        public int SiteId { get; set; } = 1;
        [MaxLength(500)]
        public string Logo { get; set; }
        [MaxLength(100)]
        public string CompanyName { get; set; }
        [MaxLength(1000)]
        public string Description { get; set; }
        [MaxLength(50)]
        public string Tel { get; set; }
        [MaxLength(100)]
        public string Email { get; set; }
        [MaxLength(300)]
        public string Address { get; set; }
        [MaxLength(500)]
        public string QrCode { get; set; }
        [MaxLength(100)]
        public string IcpNo { get; set; }
        [MaxLength(100)]
        public string PoliceNo { get; set; }
        [MaxLength(300)]
        public string Copyright { get; set; }
        public string FriendLinksJson { get; set; }
        [MaxLength(50)]
        public string BackgroundColor { get; set; } = "#111827";
        [MaxLength(50)]
        public string TextColor { get; set; } = "#ffffff";
        public DateTime CreateTime { get; set; } = DateTime.Now;
        public DateTime UpdateTime { get; set; } = DateTime.Now;
    }

    public class WebsitePageVersionRollbackRequest
    {
        public int VersionId { get; set; }
    }
}