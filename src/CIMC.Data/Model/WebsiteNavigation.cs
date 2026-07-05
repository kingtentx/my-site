using System.ComponentModel.DataAnnotations;
using CIMC.Data.ExtModel;

namespace CIMC.Data
{
    /// <summary>
    /// 网站导航菜单
    /// </summary>
    public class WebsiteNavigation : ExtFullModifyModel, IActiveModel, ISortModel, IModifyModel
    {
        [Key]
        public int Id { get; set; }

        /// <summary>
        /// 父级 ID（0=顶级）
        /// </summary>
        public int Pid { get; set; } = 0;

        /// <summary>
        /// 标题
        /// </summary>
        [Required]
        [StringLength(ModelUnits.Len_50)]
        public string Title { get; set; }

        /// <summary>
        /// 跳转路径
        /// </summary>
        [StringLength(ModelUnits.Len_200)]
        public string Path { get; set; }

        /// <summary>
        /// 图标
        /// </summary>
        [StringLength(ModelUnits.Len_100)]
        public string Icon { get; set; }

        /// <summary>
        /// 跳转方式：0=本窗口，1=新窗口
        /// </summary>
        public int Target { get; set; } = 0;

        /// <summary>
        /// 排序
        /// </summary>
        public int Sort { get; set; } = 0;

        /// <summary>
        /// 是否显示
        /// </summary>
        public bool IsShow { get; set; } = true;

        public bool IsActive { get; set; } = true;

        public bool IsDelete { get; set; } = false;
    }
}
