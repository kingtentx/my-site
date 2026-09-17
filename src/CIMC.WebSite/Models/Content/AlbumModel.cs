using System;
using System.Collections.Generic;
using System.Linq;

namespace MySite.Web.Models
{
    public class AlbumModel
    {
        public int Id { get; set; }
        public string ImageUrl { get; set; }
        public string LinkUrl { get; set; }
        public string Title { get; set; }
        public string Title_EN { get; set; }
        public string Description { get; set; }
        public string Description_EN { get; set; }
        public string Detail { get; set; }
        public string Detail_EN { get; set; }
        public string Author { get; set; }
        public int TagType { get; set; }
        public int TagId { get; set; }
        public int Sort { get; set; }
        public bool IsActive { get; set; }
        public bool IsDelete { get; set; }
        public DateTime? CreationTime { get; set; }
        public string CreationBy { get; set; }
        public DateTime? UpdateTime { get; set; }
        public string UpdateBy { get; set; }
        public List<TagModel> TagsList { get; set; } = new List<TagModel>();
        public List<string> ImageList { get; set; } = new List<string>();
        public string TagName => TagsList.FirstOrDefault(p => p.Id == TagId)?.TagName ?? string.Empty;
    }
}
