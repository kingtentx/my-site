using System.ComponentModel.DataAnnotations;
using CIMC.Data.ExtModel;

namespace CIMC.Data
{
    /// <summary>招聘岗位内容及其分类信息。</summary>
    public class Job : ExtFullModifyModel, IActiveModel, IModifyModel
    {
        /// <summary>岗位主键。</summary>
        [Key]
        public int Id { get; set; }

        /// <summary>岗位中文名称。</summary>
        [Required, StringLength(ModelUnits.Len_250)]
        public string JobName { get; set; }

        /// <summary>岗位英文名称。</summary>
        [StringLength(ModelUnits.Len_250)]
        public string JobName_EN { get; set; }

        /// <summary>招聘内容作者。</summary>
        [StringLength(ModelUnits.Len_50)]
        public string Author { get; set; }

        /// <summary>岗位详情内容。</summary>
        public string Detail { get; set; }
        /// <summary>英文详情内容。</summary>
        public string Detail_EN { get; set; }
        /// <summary>分类的业务类型。</summary>
        public int TagType { get; set; }
        /// <summary>分类主键。</summary>
        public int TagId { get; set; }
        /// <summary>是否启用。</summary>
        public bool IsActive { get; set; }
        /// <summary>是否已软删除。</summary>
        public bool IsDelete { get; set; }
    }
}
