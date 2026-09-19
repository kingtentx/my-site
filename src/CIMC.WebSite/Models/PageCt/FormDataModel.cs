using System.Collections.Generic;

namespace MySite.Web.Models
{
    /// <summary>承载表单数据相关数据。</summary>
    public class FormDataModel
    {
        /// <summary>主键。</summary>
        public int Id { get; set; }
        /// <summary>业务类型。</summary>
        public string Type { get; set; }
        /// <summary>内容 HTML。</summary>
        public string ContentHtml { get; set; }
        /// <summary>每页记录数。</summary>
        public int PageSize { get; set; }
        /// <summary>数据内容。</summary>
        public List<LabelModel> Data { get; set; }
        /// <summary>JSON 格式的数据。</summary>
        public string JsonData { get; set; }
    }

}
