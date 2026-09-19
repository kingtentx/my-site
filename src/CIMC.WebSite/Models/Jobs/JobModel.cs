using System;
using System.Collections.Generic;
using System.Linq;

namespace MySite.Web.Models
{
    /// <summary>承载招聘岗位相关数据。</summary>
    public class JobModel
    {
        /// <summary>主键。</summary>
        public int Id { get; set; }
        /// <summary>岗位名称。</summary>
        public string JobName { get; set; }
        /// <summary>英文岗位名称。</summary>
        public string JobName_EN { get; set; }
        /// <summary>内容作者。</summary>
        public string Author { get; set; }
        /// <summary>详情内容。</summary>
        public string Detail { get; set; }
        /// <summary>英文详情内容。</summary>
        public string Detail_EN { get; set; }
        /// <summary>分类的业务类型。</summary>
        public int TagType { get; set; }
        /// <summary>分类主键。</summary>
        public int TagId { get; set; }
        /// <summary>是否启用。</summary>
        public bool IsActive { get; set; }
        /// <summary>是否已软删除。</summary>
        public bool IsDelete { get; set; }
        /// <summary>创建时间。</summary>
        public DateTime? CreationTime { get; set; }
        /// <summary>最后更新时间。</summary>
        public DateTime? UpdateTime { get; set; }
        /// <summary>创建人。</summary>
        public string CreateBy { get; set; }
        /// <summary>最后更新人。</summary>
        public string UpdateBy { get; set; }
        /// <summary>可选择的分类列表。</summary>
        public List<TagModel> TagsList { get; set; } = new List<TagModel>();
        /// <summary>分类名称。</summary>
        public string TagName => TagsList.FirstOrDefault(p => p.Id == TagId)?.TagName ?? string.Empty;
    }
}
