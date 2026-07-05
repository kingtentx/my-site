using System;
using System.ComponentModel.DataAnnotations;
using CIMC.Data.ExtModel;

namespace CIMC.Data
{
    /// <summary>
    /// 网站页面版本（草稿/发布分离）
    /// </summary>
    public class WebsitePageVersion : ExtCreateModel
    {
        [Key]
        public int Id { get; set; }

        /// <summary>
        /// 页面 ID
        /// </summary>
        public int PageId { get; set; }

        /// <summary>
        /// 版本号
        /// </summary>
        public int VersionNo { get; set; } = 1;

        /// <summary>
        /// 草稿 JSON
        /// </summary>
        public string DraftJson { get; set; }

        /// <summary>
        /// 发布 JSON
        /// </summary>
        public string PublishJson { get; set; }

        /// <summary>
        /// 状态：0=草稿，1=已发布
        /// </summary>
        public int Status { get; set; } = 0;

        /// <summary>
        /// 发布时间
        /// </summary>
        public DateTime? PublishTime { get; set; }

        /// <summary>
        /// 创建人 ID
        /// </summary>
        public int CreateUserId { get; set; }

        /// <summary>
        /// 创建人
        /// </summary>
        [StringLength(ModelUnits.Len_50)]
        public string CreateUserName { get; set; }
    }
}
