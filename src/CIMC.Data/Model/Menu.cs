using CIMC.Data.ExtModel;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CIMC.Data
{
    public class Menu : ExtFullModifyModel, IModifyModel, ISortModel
    {
        /// <summary>
        /// ID
        /// </summary>
        [Key]
        public int Id { get; set; }
        /// <summary>
        /// 菜单标题
        /// </summary>
        [StringLength(ModelUnits.Len_100)]
        public string Title { get; set; }
        /// <summary>
        /// 路径
        /// </summary>
        [StringLength(ModelUnits.Len_500)]
        public string Path { get; set; }
        /// <summary>
        /// 图标
        /// </summary>
        [StringLength(ModelUnits.Len_50)]
        public string Icon { get; set; }
        /// <summary>
        /// 菜单类型 1-模块 2-菜单
        /// </summary>      
        public int MenuType { get; set; }
        /// <summary>
        /// 父ID
        /// </summary>
        public int Pid { get; set; }
        /// <summary>
        /// 是否展开
        /// </summary>      
        public bool Spread { get; set; } = false;
        /// <summary>
        /// 权限代码
        /// </summary>
        [StringLength(ModelUnits.Len_100)]
        public string PermissionKey { get; set; }
        /// <summary>
        /// 按钮集合
        /// </summary>
        [StringLength(ModelUnits.Len_500)]
        public string Buttons { get; set; }
        /// <summary>
        /// 排序
        /// </summary>
        public int Sort { get; set; }
        /// <summary>
        /// 是否显示
        /// </summary>
        public bool IsShow { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public bool IsDelete { get; set; } = false;
     
    }
}
