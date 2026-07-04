using System.Collections.Generic;

namespace MySite.Web.Models
{
    public class FormDataModel
    {
        public int Id { get; set; }
        public string Type { get; set; }
        public string ContentHtml { get; set; }
        public int PageSize { get; set; }
        public List<LabelModel> Data { get; set; }
        public string JsonData { get; set; }
    }

}
