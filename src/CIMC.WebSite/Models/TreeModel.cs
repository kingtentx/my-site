using System.Collections.Generic;

namespace MySite.Web.Models
{
    /// <summary>承载树形数据相关数据。</summary>
    public class TreeModel
    {
        /// <summary>主键。</summary>
        public int Id { get; set; }

        /// <summary>名称。</summary>
        public string Name { get; set; }

        /// <summary>父级记录的主键。</summary>
        public int ParentId { get; set; }

        /// <summary>排序值，数值越小越靠前。</summary>
        public int Sort { get; set; }

        /// <summary>子节点集合。</summary>
        public List<TreeModel> Children { get; set; }

    }
}
