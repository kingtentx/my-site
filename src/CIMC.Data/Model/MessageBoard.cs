using System.ComponentModel.DataAnnotations;
using CIMC.Data.ExtModel;

namespace CIMC.Data
{
    /// <summary>
    /// 留言板
    /// </summary>
    public class MessageBoard : ExtCreateModel
    {
        [Key]
        public long Id { get; set; }
        /// <summary>
        /// 姓名
        /// </summary>
        [StringLength(ModelUnits.Len_100)]
        public string UserName { get; set; }
        /// <summary>
        /// 邮箱
        /// </summary>
        [StringLength(ModelUnits.Len_250)]
        public string Email { get; set; }
        /// <summary>
        /// 电话
        /// </summary>
        [StringLength(ModelUnits.Len_50)]
        public string Phone { get; set; }
        /// <summary>
        /// 留言
        /// </summary>
        [StringLength(ModelUnits.Len_1000)]
        public string Message { get; set; }

        public bool IsRead { get; set; }
    }
}
