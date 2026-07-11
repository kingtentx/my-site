using System;
using System.ComponentModel.DataAnnotations;
using CIMC.Data.ExtModel;

namespace CIMC.Data
{
    /// <summary>
    /// 招聘岗位。
    /// </summary>
    public class ContentJob : ExtFullModifyModel, IActiveModel, ISortModel, IModifyModel
    {
        [Key]
        public int Id { get; set; }

        /// <summary>
        /// 招聘分类 ID。
        /// </summary>
        public int CategoryId { get; set; }

        [Required]
        [StringLength(ModelUnits.Len_100)]
        public string JobTitle { get; set; }

        [StringLength(ModelUnits.Len_100)]
        public string Department { get; set; }

        [StringLength(ModelUnits.Len_100)]
        public string WorkLocation { get; set; }

        [StringLength(ModelUnits.Len_100)]
        public string SalaryRange { get; set; }

        public int RecruitCount { get; set; } = 1;

        [StringLength(ModelUnits.Len_20)]
        public string JobType { get; set; }

        public string Responsibilities { get; set; }
        public string Requirements { get; set; }

        [StringLength(ModelUnits.Len_50)]
        public string ContactName { get; set; }

        [StringLength(ModelUnits.Len_50)]
        public string ContactPhone { get; set; }

        [StringLength(ModelUnits.Len_100)]
        public string ContactEmail { get; set; }

        public int Sort { get; set; }
        public bool IsActive { get; set; } = true;
        public DateTime? PublishTime { get; set; }
        public bool IsDelete { get; set; }
    }
}
