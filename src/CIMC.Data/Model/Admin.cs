using CIMC.Data.ExtModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace CIMC.Data
{
    /// <summary>
    /// 管理员
    /// </summary>  
    public class Admin : ExtUpdateModel, IActiveModel
    {
        /// <summary>
        /// 管理员ID
        /// </summary>
        [Key]
        public int Id { get; set; }
        /// <summary>
        /// 管理员名称
        /// </summary>
        [Required]
        [StringLength(ModelUnits.Len_50)]
        public string UserName { get; set; }
        /// <summary>
        /// 密码
        /// </summary>
        [Required]
        [StringLength(ModelUnits.Len_50)]
        public string Password { get; set; }
        /// <summary>
        /// 手机
        /// </summary>      
        [StringLength(ModelUnits.Len_50)]
        public string Telphone { get; set; }
        /// <summary>
        /// 真实姓名
        /// </summary>       
        [StringLength(ModelUnits.Len_50)]
        public string RealName { get; set; }
        /// <summary>
        /// 备注
        /// </summary>       
        [StringLength(ModelUnits.Len_500)]
        public string Remark { get; set; }
        /// <summary>
        /// 角色,多角色英文逗号隔开
        /// </summary>       
        [StringLength(ModelUnits.Len_500)]
        public string Roles { get; set; }

        /// <summary>
        /// 是否超级管理员
        /// </summary>
        public bool IsAdmin { get; set; } = false;
        /// <summary>
        /// 是否启用
        /// </summary>
        public bool IsActive { get; set; }     

    }
}
