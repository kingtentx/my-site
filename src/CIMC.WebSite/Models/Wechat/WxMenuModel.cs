using System.Collections.Generic;

namespace MySite.Web.Models
{
    /// <summary>承载微信相关数据。</summary>
    public class WxMenuModel
    {
        /// <summary>记录主键。</summary>
        public int id { get; set; }
        /// <summary>父级记录主键。</summary>
        public int pid { get; set; }

        /// <summary>名称。</summary>
        public string name { get; set; }

        /// <summary>类型。</summary>
        public string type { get; set; }

        /// <summary>值。</summary>
        public string value { get; set; }

        /// <summary>排序值。</summary>
        public int sort { get; set; }

        /// <summary>层级。</summary>
        public int level { get; set; }

        /// <summary>微信菜单的子按钮集合。</summary>
        public IList<WxMenuModel> sub_button { get; set; }
    }
}
