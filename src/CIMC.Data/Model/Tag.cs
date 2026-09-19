using System.ComponentModel.DataAnnotations;
using CIMC.Data.ExtModel;

namespace CIMC.Data
{
    /// <summary>新闻、产品和招聘共用的分类标签。</summary>
    public class Tag : ExtFullModifyModel, IActiveModel, ISortModel
    {
        /// <summary>分类主键。</summary>
        [Key]
        public int Id { get; set; }

        /// <summary>分类中文名称。</summary>
        [Required, StringLength(ModelUnits.Len_100)]
        public string TagName { get; set; }

        /// <summary>分类英文名称。</summary>
        [StringLength(ModelUnits.Len_100)]
        public string TagName_EN { get; set; }

        /// <summary>分类所属的业务类型。</summary>
        public int TagType { get; set; }
        /// <summary>排序值，数值越小越靠前。</summary>
        public int Sort { get; set; }
        /// <summary>是否启用。</summary>
        public bool IsActive { get; set; }
    }
}
