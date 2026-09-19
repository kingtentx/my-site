using System.ComponentModel.DataAnnotations;
using CIMC.Data.ExtModel;

namespace CIMC.Data
{
    /// <summary>产品内容及其展示信息。</summary>
    public class Product : ExtFullModifyModel, IActiveModel, IModifyModel
    {
        /// <summary>产品主键。</summary>
        [Key]
        public int Id { get; set; }

        /// <summary>产品封面图片地址。</summary>
        [StringLength(ModelUnits.Len_500)]
        public string ImageUrl { get; set; }

        /// <summary>产品跳转链接地址。</summary>
        [StringLength(ModelUnits.Len_500)]
        public string LinkUrl { get; set; }

        /// <summary>产品中文标题。</summary>
        [StringLength(ModelUnits.Len_250)]
        public string Title { get; set; }

        /// <summary>产品英文标题。</summary>
        [StringLength(ModelUnits.Len_250)]
        public string Title_EN { get; set; }

        /// <summary>产品中文摘要。</summary>
        [StringLength(ModelUnits.Len_500)]
        public string Description { get; set; }

        /// <summary>产品英文摘要。</summary>
        [StringLength(ModelUnits.Len_500)]
        public string Description_EN { get; set; }

        /// <summary>产品中文详情内容。</summary>
        public string Detail { get; set; }

        /// <summary>产品英文详情内容。</summary>
        public string Detail_EN { get; set; }

        /// <summary>产品内容作者。</summary>
        [StringLength(ModelUnits.Len_50)]
        public string Author { get; set; }

        /// <summary>分类类型，产品对应的类型值为 2。</summary>
        public int TagType { get; set; }

        /// <summary>所属分类的主键。</summary>
        public int TagId { get; set; }

        /// <summary>展示排序值，数值越小越靠前。</summary>
        public int Sort { get; set; }

        /// <summary>是否已发布。</summary>
        public bool IsActive { get; set; }

        /// <summary>是否已软删除。</summary>
        public bool IsDelete { get; set; }
    }
}
