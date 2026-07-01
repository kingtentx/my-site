using CIMC.Core.Enums;
using System;
namespace CimcSite.Web.Models
{
    public class TagModel
    {
        /// <summary>
        /// 标签ID
        /// </summary>     
        public int Id { get; set; }
        /// <summary>
        /// 标签名称
        /// </summary>
        public string TagName { get; set; }
        /// <summary>
        /// 标签名称（英文）
        /// </summary>
        public string TagName_EN { get; set; }
        /// <summary>
        /// 标签类型
        /// </summary>
        public int TagType { get; set; }
        /// <summary>
        /// 排序
        /// </summary>
        public int Sort { get; set; }
        /// <summary>
        /// 是否激活
        /// </summary>
        public bool IsActive { get; set; }

        /// <summary>
        /// 更新时间
        /// </summary>
        public DateTime? CreationTime { get; set; }
        /// <summary>
        /// 创建人
        /// </summary>          
        public string CreationBy { get; set; }
        /// <summary>
        /// 更新时间
        /// </summary>
        public DateTime? UpdateTime { get; set; }

        /// <summary>
        /// 更新人
        /// </summary>           
        public string UpdateBy { get; set; }

        public virtual string TypeName
        {
            get
            {
                var name = string.Empty;
                if (TagType > 0)
                {
                    name = CIMC.Helper.EnumHelper.GetDescription((TagType)TagType);
                }
                return name;
            }
        }
    }
}
