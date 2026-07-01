using CIMC.Data.ExtModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace CIMC.Data
{
    /// <summary>
    /// 角色
    /// </summary> 
    public class Role : ExtCreateModel, IActiveModel
    {
        /// <summary>
        /// 角色ID
        /// </summary>
        [Key]
        public int Id { get; set; }
        /// <summary>
        /// 角色名称
        /// </summary>
        [StringLength(ModelUnits.Len_50)]
        public string RoleName { get; set; }
        /// <summary>
        /// 描述
        /// </summary>
        [StringLength(ModelUnits.Len_500)]
        public string Description { get; set; }

        /// <summary>
        /// 是否启用
        /// </summary>
        public bool IsActive { get; set; }
        /// <summary>
        /// 角色类型 0-管理员 1-商家
        /// </summary>
        public int RoleType { get; set; }
    }
}