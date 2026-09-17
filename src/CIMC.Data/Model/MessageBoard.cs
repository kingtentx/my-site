using System.ComponentModel.DataAnnotations;
using CIMC.Data.ExtModel;

namespace CIMC.Data
{
    /// <summary>
    /// 前台在线留言。
    /// </summary>
    public class MessageBoard : ExtCreateModel
    {
        [Key]
        public long Id { get; set; }

        [Required]
        [StringLength(ModelUnits.Len_100)]
        public string UserName { get; set; }

        [StringLength(ModelUnits.Len_250)]
        public string Email { get; set; }

        [Required]
        [StringLength(ModelUnits.Len_50)]
        public string Phone { get; set; }

        [Required]
        [StringLength(ModelUnits.Len_1000)]
        public string Message { get; set; }

        public bool IsRead { get; set; }
    }
}
