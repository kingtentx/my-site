using System.ComponentModel.DataAnnotations;
using CIMC.Data.ExtModel;

namespace CIMC.Data
{
    public class Job : ExtFullModifyModel, IActiveModel, IModifyModel
    {
        [Key] public int Id { get; set; }
        [Required, StringLength(ModelUnits.Len_250)] public string JobName { get; set; }
        [StringLength(ModelUnits.Len_250)] public string JobName_EN { get; set; }
        [StringLength(ModelUnits.Len_50)] public string Author { get; set; }
        public string Detail { get; set; }
        public string Detail_EN { get; set; }
        public int TagType { get; set; }
        public int TagId { get; set; }
        public bool IsActive { get; set; }
        public bool IsDelete { get; set; }
    }
}
