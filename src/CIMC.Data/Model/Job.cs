using System.ComponentModel.DataAnnotations;
using CIMC.Data.ExtModel;

namespace CIMC.Data
{
    public class Job : ExtFullModifyModel, IActiveModel, IModifyModel
    {
        [Key]
        public int Id { get; set; }

        /// <summary>
        /// 标题
        /// </summary>
        [Required]
        [StringLength(ModelUnits.Len_250)]
        public string JobName { get; set; }
        /// <summary>
        /// 标题（英文）
        /// </summary>
        [StringLength(ModelUnits.Len_250)]
        public string JobName_EN { get; set; }
        /// <summary>
        /// 发布人
        /// </summary>
        [StringLength(ModelUnits.Len_50)]
        public string Author { get; set; }
        /// <summary>
        /// 详情
        /// </summary>      
        public string Detail { get; set; }
        /// <summary>
        /// 详情（英文）
        /// </summary>
        public string Detail_EN { get; set; }
        /// <summary>
        /// 类型
        /// </summary>
        public int TagType { get; set; }
        /// <summary>
        /// 标签ID
        /// </summary>
        public int TagId { get; set; }
        /// <summary>
        /// 是否公开
        /// </summary>
        public bool IsActive { get; set; }
        /// <summary>
        /// 是否删除
        /// </summary>
        public bool IsDelete { get; set; }
    }
}
