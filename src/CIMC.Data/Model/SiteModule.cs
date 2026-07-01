using System.ComponentModel.DataAnnotations;
using CIMC.Data.ExtModel;

namespace CIMC.Data
{
    public class SiteModule : ExtFullModifyModel, IActiveModel, ISortModel, IModifyModel
    {
        [Key]
        public int Id { get; set; }

        [StringLength(ModelUnits.Len_100)]
        public string PageKey { get; set; }

        [StringLength(ModelUnits.Len_100)]
        public string ModuleKey { get; set; }

        [StringLength(ModelUnits.Len_100)]
        public string ModuleName { get; set; }

        [StringLength(ModelUnits.Len_100)]
        public string ModuleName_EN { get; set; }

        [StringLength(ModelUnits.Len_50)]
        public string ModuleType { get; set; }

        [StringLength(ModelUnits.Len_250)]
        public string Title { get; set; }

        [StringLength(ModelUnits.Len_250)]
        public string Title_EN { get; set; }

        [StringLength(ModelUnits.Len_500)]
        public string SubTitle { get; set; }

        [StringLength(ModelUnits.Len_500)]
        public string SubTitle_EN { get; set; }

        [StringLength(ModelUnits.Len_500)]
        public string LinkUrl { get; set; }

        [StringLength(ModelUnits.Len_5000)]
        public string ImageUrl { get; set; }

        public string SettingsJson { get; set; }

        public string SettingsJson_EN { get; set; }

        public int? NavigationId { get; set; }

        public int Sort { get; set; }

        public bool IsActive { get; set; }

        public bool IsDelete { get; set; }
    }
}
