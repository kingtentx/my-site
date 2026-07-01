using CIMC.Data.ExtModel;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CIMC.Data
{
    /// <summary>
    /// 相册
    /// </summary>
    public class Album : ExtFullModifyModel, IActiveModel, IModifyModel
    {
        [Key]
        public int Id { get; set; }
        /// <summary>
        /// 图片
        /// </summary>
        [StringLength(ModelUnits.Len_500)]
        public string ImageUrl { get; set; }
        /// <summary>
        /// 跳转链接
        /// </summary>
        [StringLength(ModelUnits.Len_500)]
        public string LinkUrl { get; set; }
        /// <summary>
        /// 标题
        /// </summary>      
        [StringLength(ModelUnits.Len_250)]
        public string Title { get; set; }
        /// <summary>
        /// 标题（英文）
        /// </summary>
        [StringLength(ModelUnits.Len_250)]
        public string Title_EN { get; set; }
        /// <summary>
        /// 描述
        /// </summary>
        [StringLength(ModelUnits.Len_500)]
        public string Description { get; set; }
        /// <summary>
        /// 描述（英文）
        /// </summary>
        [StringLength(ModelUnits.Len_500)]
        public string Description_EN { get; set; }
        /// <summary>
        /// 详情内容，产品箱型可用于维护参数表
        /// </summary>
        public string Detail { get; set; }
        /// <summary>
        /// 详情内容（英文）
        /// </summary>
        public string Detail_EN { get; set; }
        /// <summary>
        /// 作者
        /// </summary>
        [StringLength(ModelUnits.Len_50)]
        public string Author { get; set; }
        /// <summary>
        /// 类型
        /// </summary>
        public int TagType { get; set; }
        /// <summary>
        /// 标签ID
        /// </summary>
        public int TagId { get; set; }
        /// <summary>
        /// 排序
        /// </summary>
        public int Sort { get; set; } = 0;
        /// <summary>
        /// 是否公开
        /// </summary>
        public bool IsActive { get; set; }
        /// <summary>
        /// 是否删除
        /// </summary>
        public bool IsDelete { get; set; }
    }
}
