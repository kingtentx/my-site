using CIMC.Data.ExtModel;
using System.ComponentModel.DataAnnotations;

namespace CIMC.Data
{
    public class FooterInfo : ExtUpdateModel
    {
        [Key]
        public int Id { get; set; }

        [StringLength(ModelUnits.Max)]
        public string CompanyInfo { get; set; }

        public string CompanyInfo_EN { get; set; }

        [StringLength(ModelUnits.Len_500)]
        public string ImageA { get; set; }

        [StringLength(ModelUnits.Len_500)]
        public string ImageB { get; set; }

        [StringLength(ModelUnits.Len_50)]
        public string Phone { get; set; }

        [StringLength(ModelUnits.Len_250)]
        public string Email { get; set; }

        [StringLength(ModelUnits.Len_500)]
        public string Address { get; set; }

        [StringLength(ModelUnits.Len_500)]
        public string Address_EN { get; set; }

        [StringLength(ModelUnits.Len_100)]
        public string RecordNo { get; set; }

        [StringLength(ModelUnits.Len_500)]
        public string Copyright { get; set; }

        [StringLength(ModelUnits.Len_500)]
        public string Copyright_EN { get; set; }
    }
}