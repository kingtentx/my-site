using CIMC.Core.Enums;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;


namespace CimcSite.Web.Models
{
    public class PageConfigModel
    {
        /// <summary>
        /// ID
        /// </summary>

        public int Id { get; set; }
        /// <summary>
        /// 导航
        /// </summary>
        public int NavigationId { get; set; }

        public string ControlJson { get; set; }
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

        //public List<PageControlModel> PageControlList { get; set; } = new List<PageControlModel>();

        public List<PageControlModel> PageControlList
        {
            get
            {
                if (!string.IsNullOrWhiteSpace(ControlJson))
                    return JsonConvert.DeserializeObject<List<PageControlModel>>(ControlJson);
                else
                    return controlJson;
            }
            set
            {
                controlJson = value;
            }
        }

        private List<PageControlModel> controlJson { get; set; } = new List<PageControlModel>();
    }

    public class PageControlModel
    {
        public int Id { get; set; }
        /// <summary>
        /// 控件类型
        /// </summary>
        public ControlType Type { get; set; }
        /// <summary>
        /// 分类标签
        /// </summary>
        public virtual List<LabelModel> Labels { get; set; } = new List<LabelModel>();
        /// <summary>
        /// 页大小
        /// </summary>
        public int PageSize { get; set; }
        /// <summary>
        /// html
        /// </summary>
        public string ContentHtml { get; set; }
        /// <summary>
        /// 数据
        /// </summary>
        public string JsonData { get; set; }
    }

    /// <summary>
    /// 标签表
    /// </summary>
    public class LabelModel
    {
        /// <summary>
        /// 标签ID
        /// </summary>    
        public int TagId { get; set; }
        /// <summary>
        /// 标签名称
        /// </summary>     
        public string TagName { get; set; }
    }


}
