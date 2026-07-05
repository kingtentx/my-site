using System;
using System.ComponentModel.DataAnnotations;

namespace MySite.Web.Models
{
    public class JobModel
    {
        public int Id { get; set; }

        [Display(Name = "岗位名称")]
        [Required(ErrorMessage = "请输入岗位名称")]
        public string JobTitle { get; set; }

        [Display(Name = "所属部门")]
        public string Department { get; set; }

        [Display(Name = "工作地点")]
        public string WorkLocation { get; set; }

        [Display(Name = "薪资范围")]
        public string SalaryRange { get; set; }

        [Display(Name = "招聘人数")]
        public int RecruitCount { get; set; } = 1;

        [Display(Name = "工作类型")]
        public string JobType { get; set; }

        [Display(Name = "岗位职责")]
        public string Responsibilities { get; set; }

        [Display(Name = "任职要求")]
        public string Requirements { get; set; }

        [Display(Name = "联系人")]
        public string ContactName { get; set; }

        [Display(Name = "联系电话")]
        public string ContactPhone { get; set; }

        [Display(Name = "联系邮箱")]
        public string ContactEmail { get; set; }

        [Display(Name = "排序")]
        public int Sort { get; set; } = 0;

        [Display(Name = "是否发布")]
        public bool IsActive { get; set; } = true;

        public DateTime? PublishTime { get; set; }

        public DateTime? CreationTime { get; set; }

        public string CreationBy { get; set; }

        public DateTime? UpdateTime { get; set; }

        public string UpdateBy { get; set; }
    }
}
