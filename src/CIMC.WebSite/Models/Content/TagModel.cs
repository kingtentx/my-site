using System;
using CIMC.Core.Enums;

namespace MySite.Web.Models
{
    /// <summary>承载分类标签相关数据。</summary>
    public class TagModel
    {
        /// <summary>主键。</summary>
        public int Id { get; set; }
        /// <summary>分类名称。</summary>
        public string TagName { get; set; }
        /// <summary>英文分类名称。</summary>
        public string TagName_EN { get; set; }
        /// <summary>分类的业务类型。</summary>
        public int TagType { get; set; }
        /// <summary>排序值，数值越小越靠前。</summary>
        public int Sort { get; set; }
        /// <summary>是否启用。</summary>
        public bool IsActive { get; set; }
        /// <summary>创建时间。</summary>
        public DateTime? CreationTime { get; set; }
        /// <summary>创建人。</summary>
        public string CreationBy { get; set; }
        /// <summary>最后更新时间。</summary>
        public DateTime? UpdateTime { get; set; }
        /// <summary>最后更新人。</summary>
        public string UpdateBy { get; set; }
        /// <summary>分类标签的名称。</summary>
        public string TypeName => TagType > 0 ? CIMC.Helper.EnumHelper.GetDescription((TagType)TagType) : string.Empty;
    }
}
