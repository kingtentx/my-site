using System;

namespace MySite.Web.Models
{
    /// <summary>承载微信相关数据。</summary>
    public class WxMediaModel
    {
        /// <summary>主键。</summary>
        public long Id { get; set; }

        /// <summary>
        /// 图片名称
        /// </summary>

        public string FileName { get; set; }
        /// <summary>
        /// 介绍
        /// </summary>

        public string Introduction { get; set; }
        /// <summary>
        /// 新增的永久素材的media_id
        /// </summary>

        public string MediaId { get; set; }
        /// <summary>
        /// Type: news=1 image=2 voice=3 video=4 
        /// </summary>
        public int MediaType { get; set; }
        /// <summary>
        /// 本地服务器URL路径
        /// </summary>

        public string Url { get; set; }
        /// <summary>
        /// 封面
        /// </summary>

        public string CoverUrl { get; set; }
        /// <summary>
        /// 扩展名
        /// </summary>

        public string ExtensionName { get; set; }
        /// <summary>
        /// 文件大小
        /// </summary>
        public long Size { get; set; }
        /// <summary>
        /// 标签ID
        /// </summary>       
        public int TagId { get; set; }
        /// <summary>
        /// 创建人
        /// </summary>
        public DateTime? CreationTime { get; set; }
        /// <summary>创建人。</summary>
        public string CreationBy { get; set; }
        /// <summary>是否已软删除。</summary>
        public bool IsDelete { get; set; }
    }
}
