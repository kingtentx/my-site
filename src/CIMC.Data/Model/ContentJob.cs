using System;
using System.ComponentModel.DataAnnotations;
using CIMC.Data.ExtModel;

namespace CIMC.Data
{
    /// <summary>
    /// 招聘岗位
    /// </summary>
    public class ContentJob : ExtFullModifyModel, IActiveModel, ISortModel, IModifyModel
    {
        [Key]
        public int Id { get; set; }

        /// <summary>
        /// 岗位名称
        /// </summary>
        [Required]
        [StringLength(ModelUnits.Len_100)]
        public string JobTitle { get; set; }

        /// <summary>
        /// 所属部门
        /// </summary>
        [StringLength(ModelUnits.Len_100)]
        public string Department { get; set; }

        /// <summary>
        /// 工作地点
        /// </summary>
        [StringLength(ModelUnits.Len_100)]
        public string WorkLocation { get; set; }

        /// <summary>
        /// 薪资范围
        /// </summary>
        [StringLength(ModelUnits.Len_100)]
        public string SalaryRange { get; set; }

        /// <summary>
        /// 招聘人数
        /// </summary>
        public int RecruitCount { get; set; } = 1;

        /// <summary>
        /// 工作类型（全职/兼职/实习）
        /// </summary>
        [StringLength(ModelUnits.Len_20)]
        public string JobType { get; set; }

        /// <summary>
        /// 岗位职责（富文本）
        /// </summary>
        public string Responsibilities { get; set; }

        /// <summary>
        /// 任职要求（富文本）
        /// </summary>
        public string Requirements { get; set; }

        /// <summary>
        /// 联系人
        /// </summary>
        [StringLength(ModelUnits.Len_50)]
        public string ContactName { get; set; }

        /// <summary>
        /// 联系电话
        /// </summary>
        [StringLength(ModelUnits.Len_50)]
        public string ContactPhone { get; set; }

        /// <summary>
        /// 联系邮箱
        /// </summary>
        [StringLength(ModelUnits.Len_100)]
        public string ContactEmail { get; set; }

        /// <summary>
        /// 排序
        /// </summary>
        public int Sort { get; set; } = 0;

        /// <summary>
        /// 是否发布
        /// </summary>
        public bool IsActive { get; set; } = true;

        /// <summary>
        /// 发布时间
        /// </summary>
        public DateTime? PublishTime { get; set; }

        public bool IsDelete { get; set; } = false;
    }
}
