using System;
using System.Collections.Generic;
using System.Linq;

namespace CimcSite.Web.Models
{
    public class JobModel
    {

        public int Id { get; set; }

        /// <summary>
        /// 标题
        /// </summary>

        public string JobName { get; set; }
        /// <summary>
        /// 标题-EN
        /// </summary>
        public string JobName_EN { get; set; }
        /// <summary>
        /// 发布人
        /// </summary>

        public string Author { get; set; }
        /// <summary>
        /// 详情
        /// </summary>

        public string Detail { get; set; }
        /// <summary>
        /// 详情-EN
        /// </summary>
        public string Detail_EN { get; set; }
        /// <summary>
        /// 类型
        /// </summary>
        public int TagType { get; set; }
        /// <summary>
        /// 标签ID
        /// </summary>
        public int TagId { get; set; }
        /// <summary>
        /// 是否公开
        /// </summary>
        public bool IsActive { get; set; }
        /// <summary>
        /// 是否删除
        /// </summary>
        public bool IsDelete { get; set; }

        public DateTime? CreationTime { get; set; }

        /// <summary>
        /// 更新时间
        /// </summary>
        public DateTime? UpdateTime { get; set; }
        /// <summary>
        /// 创建人
        /// </summary>
        public string CreateBy { get; set; }
        /// <summary>
        /// 更新人
        /// </summary>       
        public string UpdateBy { get; set; }

        /// <summary>
        /// 标签名称
        /// </summary>
        public virtual string TagName
        {
            get
            {
                var name = string.Empty;
                if (TagsList.Count() > 0)
                {
                    var query = TagsList.Where(p => p.Id == TagId);
                    name = query != null ? query.FirstOrDefault().TagName : "";
                }
                return name;
            }
        }
        /// <summary>
        /// 分类列表
        /// </summary>
        public virtual List<TagModel> TagsList { get; set; } = new List<TagModel>();
    }
}
