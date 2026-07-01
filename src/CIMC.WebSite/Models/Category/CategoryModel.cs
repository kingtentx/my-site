using System;

namespace CimcSite.Web.Models
{
    public class CategoryModel
    {
        /// <summary>
        /// ID
        /// </summary>       
        public int Id { get; set; }
        /// <summary>
        /// 父ID
        /// </summary>       
        public int Pid { get; set; }
        /// <summary>
        /// 名称
        /// </summary>       
        public string CategoryName { get; set; }
        /// <summary>
        /// 类型
        /// </summary>
        public int CategoryType { get; set; }
        /// <summary>
        /// 排序
        /// </summary>
        public int Sort { get; set; }
        /// <summary>
        /// 是否激活
        /// </summary>
        public bool IsActive { get; set; }

        public bool IsDelete { get; set; }
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


    }
}
