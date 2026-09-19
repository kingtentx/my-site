using CIMC.Data.ExtModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace CIMC.Data
{
    /// <summary>
    /// 角色菜单
    /// </summary>  
    public class RoleMenu : ExtCreateModel
    {
        /// <summary>主键。</summary>
        [Key]
        public int Id { get; set; }
        /// <summary>
        /// 角色ID
        /// </summary>
        public int RoleId { get; set; }

        /// <summary>权限代码。</summary>
        [StringLength(ModelUnits.Len_100)]
        public string Permission { get; set; }

    }
}