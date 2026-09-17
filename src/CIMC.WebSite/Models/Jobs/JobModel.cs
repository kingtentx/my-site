using System;
using System.Collections.Generic;
using System.Linq;

namespace MySite.Web.Models
{
    public class JobModel
    {
        public int Id { get; set; }
        public string JobName { get; set; }
        public string JobName_EN { get; set; }
        public string Author { get; set; }
        public string Detail { get; set; }
        public string Detail_EN { get; set; }
        public int TagType { get; set; }
        public int TagId { get; set; }
        public bool IsActive { get; set; }
        public bool IsDelete { get; set; }
        public DateTime? CreationTime { get; set; }
        public DateTime? UpdateTime { get; set; }
        public string CreateBy { get; set; }
        public string UpdateBy { get; set; }
        public List<TagModel> TagsList { get; set; } = new List<TagModel>();
        public string TagName => TagsList.FirstOrDefault(p => p.Id == TagId)?.TagName ?? string.Empty;
    }
}
