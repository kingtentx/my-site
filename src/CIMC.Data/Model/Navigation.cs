using CIMC.Data.ExtModel;
using System.ComponentModel.DataAnnotations;

namespace CIMC.Data
{
    public class Navigation : ExtFullModifyModel, IActiveModel, ISortModel, IModifyModel
    {
        [Key]
        public int Id { get; set; }

        public int Pid { get; set; }

        [Required]
        [StringLength(ModelUnits.Len_100)]
        public string NavigationName { get; set; }

        [StringLength(ModelUnits.Len_100)]
        public string NavigationName_EN { get; set; }

        [StringLength(ModelUnits.Len_100)]
        public string RewriteName { get; set; }

        [StringLength(ModelUnits.Len_500)]
        public string Description { get; set; }

        public bool IsEnableLink { get; set; }

        [StringLength(ModelUnits.Len_500)]
        public string LinkUrl { get; set; }

        public int Sort { get; set; }

        public bool IsHomePage { get; set; }

        public bool IsShow { get; set; }

        public bool IsActive { get; set; }

        public bool IsDelete { get; set; }
    }
}