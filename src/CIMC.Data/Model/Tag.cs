using System.ComponentModel.DataAnnotations;
using CIMC.Data.ExtModel;

namespace CIMC.Data
{
    /// <summary>新闻、产品和招聘共用的分类标签。</summary>
    public class Tag : ExtFullModifyModel, IActiveModel, ISortModel
    {
        [Key] public int Id { get; set; }
        [Required, StringLength(ModelUnits.Len_100)] public string TagName { get; set; }
        [StringLength(ModelUnits.Len_100)] public string TagName_EN { get; set; }
        public int TagType { get; set; }
        public int Sort { get; set; }
        public bool IsActive { get; set; }
    }
}
