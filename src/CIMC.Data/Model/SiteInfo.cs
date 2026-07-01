using CIMC.Data.ExtModel;
using System.ComponentModel.DataAnnotations;

namespace CIMC.Data
{
    public class SiteInfo : ExtUpdateModel
    {
        [Key]
        public int Id { get; set; }

        [StringLength(ModelUnits.Len_250)]
        public string CompanyName { get; set; }

        [StringLength(ModelUnits.Len_250)]
        public string CompanyName_EN { get; set; }

        [StringLength(ModelUnits.Len_500)]
        public string Keywords { get; set; }

        [StringLength(ModelUnits.Len_500)]
        public string Keywords_EN { get; set; }

        [StringLength(ModelUnits.Len_500)]
        public string Description { get; set; }

        [StringLength(ModelUnits.Len_500)]
        public string Description_EN { get; set; }

        [StringLength(ModelUnits.Len_500)]
        public string Logo { get; set; }

        [StringLength(ModelUnits.Len_500)]
        public string Logo_H5 { get; set; }
    }
}