using System.ComponentModel.DataAnnotations;
using CIMC.Data.ExtModel;

namespace CIMC.Data
{
    public class Images : ExtCreateModel, ICreateByModel
    {
        [Key]
        public long Id { get; set; }

        [StringLength(ModelUnits.Len_250)]
        public string FileName { get; set; }

        [StringLength(ModelUnits.Len_500)]
        public string Url { get; set; }

        [StringLength(ModelUnits.Len_10)]
        public string ExtensionName { get; set; }

        public long Size { get; set; }

        [StringLength(ModelUnits.Len_100)]
        public string CreationBy { get; set; }
    }
}
