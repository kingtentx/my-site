using System.ComponentModel.DataAnnotations;
using CIMC.Data.ExtModel;

namespace CIMC.Data
{
    /// <summary>
    /// 前台在线留言。
    /// </summary>
    public class MessageBoard : ExtCreateModel
    {
        /// <summary>主键。</summary>
        [Key]
        public long Id { get; set; }

        /// <summary>用户名。</summary>
        [Required]
        [StringLength(ModelUnits.Len_100)]
        public string UserName { get; set; }

        /// <summary>电子邮箱地址。</summary>
        [StringLength(ModelUnits.Len_250)]
        public string Email { get; set; }

        /// <summary>联系电话。</summary>
        [Required]
        [StringLength(ModelUnits.Len_50)]
        public string Phone { get; set; }

        /// <summary>结果消息。</summary>
        [Required]
        [StringLength(ModelUnits.Len_1000)]
        public string Message { get; set; }

        /// <summary>是否已读。</summary>
        public bool IsRead { get; set; }
    }
}
