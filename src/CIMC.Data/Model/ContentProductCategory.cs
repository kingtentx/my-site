using System.ComponentModel.DataAnnotations;
using CIMC.Data.ExtModel;

namespace CIMC.Data
{
    /// <summary>
    /// 产品分类
    /// </summary>
    public class ContentProductCategory : ExtFullModifyModel, IActiveModel, ISortModel, IModifyModel
    {
        [Key]
        public int Id { get; set; }

        /// <summary>
        /// 父级 ID（0=顶级）
        /// </summary>
        public int Pid { get; set; } = 0;

        /// <summary>
        /// 分类名称
        /// </summary>
        [Required]
        [StringLength(ModelUnits.Len_100)]
        public string Name { get; set; }

        /// <summary>
        /// 排序
        /// </summary>
        public int Sort { get; set; } = 0;

        public bool IsActive { get; set; } = true;

        public bool IsDelete { get; set; } = false;
    }
}
