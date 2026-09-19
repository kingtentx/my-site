using System;
using System.ComponentModel.DataAnnotations;

namespace CIMC.Data.ExtModel
{
    /// <summary>
    /// 创建时间
    /// </summary>
    public class ExtCreateModel
    {
        /// <summary>
        /// 创建时间
        /// </summary>
        public DateTime? CreationTime { get; set; } = DateTime.Now;

    };

    /// <summary>
    /// 更新时间 [创建时间]
    /// </summary>
    public class ExtUpdateModel : ExtCreateModel
    {
        /// <summary>最后更新时间。</summary>
        public DateTime? UpdateTime { get; set; }

    }

    /// <summary>
    /// 修改[创建时间，创建人，更新时间，更新人]
    /// </summary>
    public class ExtFullModifyModel : ExtCreateModel, ICreateByModel, IUpdateByModel
    {
        /// <summary>
        /// 更新时间
        /// </summary>
        public DateTime? UpdateTime { get; set; }
        /// <summary>
        /// 创建人
        /// </summary>
        [StringLength(ModelUnits.Len_50)]
        public string CreationBy { get; set; }
        /// <summary>
        /// 更新人
        /// </summary>
        [StringLength(ModelUnits.Len_50)]
        public string UpdateBy { get; set; }

    }

    /// <summary>
    /// 创建人
    /// </summary>
    public interface ICreateByModel
    {
        /// <summary>创建人。</summary>
        [StringLength(ModelUnits.Len_50)]
        string CreationBy { get; set; }
    }

    /// <summary>
    /// 更新人
    /// </summary>
    public interface IUpdateByModel
    {
        /// <summary>最后更新人。</summary>
        [StringLength(ModelUnits.Len_50)]
        string UpdateBy { get; set; }
    }

    /// <summary>
    /// 是否删除
    /// </summary>
    public interface IModifyModel
    {
        /// <summary>是否已软删除。</summary>
        bool IsDelete { get; set; }
    }

    /// <summary>
    /// 是否激活
    /// </summary>
    public interface IActiveModel
    {
        /// <summary>是否启用。</summary>
        bool IsActive { get; set; }
    }
    /// <summary>
    /// 排序
    /// </summary>
    public interface ISortModel
    {
        /// <summary>排序值，数值越小越靠前。</summary>
        int Sort { get; set; }
    }
}