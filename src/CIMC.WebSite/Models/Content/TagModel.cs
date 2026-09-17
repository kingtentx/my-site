using System;
using CIMC.Core.Enums;

namespace MySite.Web.Models
{
    public class TagModel
    {
        public int Id { get; set; }
        public string TagName { get; set; }
        public string TagName_EN { get; set; }
        public int TagType { get; set; }
        public int Sort { get; set; }
        public bool IsActive { get; set; }
        public DateTime? CreationTime { get; set; }
        public string CreationBy { get; set; }
        public DateTime? UpdateTime { get; set; }
        public string UpdateBy { get; set; }
        public string TypeName => TagType > 0 ? CIMC.Helper.EnumHelper.GetDescription((TagType)TagType) : string.Empty;
    }
}
