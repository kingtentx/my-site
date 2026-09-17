using System.ComponentModel.DataAnnotations;
using CIMC.Data.ExtModel;

namespace CIMC.Data
{
    /// <summary>产品图集。</summary>
    public class Album : ExtFullModifyModel, IActiveModel, IModifyModel
    {
        [Key] public int Id { get; set; }
        [StringLength(ModelUnits.Len_500)] public string ImageUrl { get; set; }
        [StringLength(ModelUnits.Len_500)] public string LinkUrl { get; set; }
        [StringLength(ModelUnits.Len_250)] public string Title { get; set; }
        [StringLength(ModelUnits.Len_250)] public string Title_EN { get; set; }
        [StringLength(ModelUnits.Len_500)] public string Description { get; set; }
        [StringLength(ModelUnits.Len_500)] public string Description_EN { get; set; }
        public string Detail { get; set; }
        public string Detail_EN { get; set; }
        [StringLength(ModelUnits.Len_50)] public string Author { get; set; }
        public int TagType { get; set; }
        public int TagId { get; set; }
        public int Sort { get; set; }
        public bool IsActive { get; set; }
        public bool IsDelete { get; set; }
    }
}
