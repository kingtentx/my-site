using System;

namespace CimcSite.Web.Models
{
    public class NavigationModel
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
        public string NavigationName { get; set; }
        /// <summary>
        /// 英文名称
        /// </summary>       
        public string NavigationName_EN { get; set; }
        /// <summary>
        /// 新名称
        /// </summary>      
        public string RewriteName { get; set; }
        /// <summary>
        /// 描述
        /// </summary>      
        public string Description { get; set; }
        /// <summary>
        /// 启用外部链接
        /// </summary>
        public bool IsEnableLink { get; set; }
        /// <summary>
        /// 指定跳转链接
        /// </summary>     
        public string LinkUrl { get; set; }
        /// <summary>
        /// 排序越大越靠后
        /// </summary>
        public int Sort { get; set; }
        /// <summary>
        /// 是否主页
        /// </summary>
        public bool IsHomePage { get; set; }
        /// <summary>
        /// 是否显示
        /// </summary>
        public bool IsShow { get; set; }
        /// <summary>
        /// 是否启用
        /// </summary>
        public bool IsActive { get; set; }
        /// <summary>
        /// 是否有子集
        /// </summary>
        public bool IsHasSubset { get; set; }
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

        public bool IsDelete { get; set; }
    }
}
