using System;

namespace MySite.Web.Models
{
    public class WxArticleModel
    {
        public long Id { get; set; }

        /// <summary>
        /// MeId
        /// </summary>
        public long MeId { get; set; } = 0;
        /// <summary>
        /// 封面
        /// </summary>
        public string CoverUrl { get; set; }
        /// <summary>
        /// 排序
        /// </summary>
        public int Sort { get; set; } = 0;

        public DateTime? CreationTime { get; set; }
        /// <summary>
        /// 更新时间
        /// </summary>
        public DateTime? UpdateTime { get; set; }
        /// <summary>
        /// 创建人
        /// </summary>      
        public string CreationBy { get; set; }
        /// <summary>
        /// 更新人
        /// </summary>    
        public string UpdateBy { get; set; }

        /// <summary>
        /// 标题
        /// </summary>
        public string Title { get; set; }
        /// <summary>
        /// 图文消息的封面图片素材id（必须是永久mediaID）
        /// </summary>
        public string ThumbMediaId { get; set; }
        /// <summary>
        /// 作者 [非必填]
        /// </summary>
        public string Author { get; set; }
        /// <summary>
        /// 图文消息的摘要，仅有单图文消息才有摘要，多图文此处为空。
        /// 如果本字段为没有填写，则默认抓取正文前64个字  [非必填]
        /// </summary>
        public string Digest { get; set; }
        /// <summary>
        /// 是否显示封面，0为false，即不显示，1为true，即显示
        /// </summary>
        public bool ShowCoverPic { get; set; }
        /// <summary>
        /// 图文消息的具体内容，支持HTML标签，必须少于2万字符，小于1M，且此处会去除JS,
        /// 涉及图片url必须来源 "上传图文消息内的图片获取URL"接口获取。外部图片url将被过滤。        
        /// </summary>
        public string Content { get; set; }
        /// <summary>
        /// 图文消息的原文地址，即点击“阅读原文”后的URL
        /// </summary>
        public string ContentSourceUrl { get; set; }
        /// <summary>
        /// Uint32	是否打开评论，0不打开，1打开
        /// </summary>
        public int NeedOpenComment { get; set; }
        /// <summary>
        /// Uint32	是否粉丝才可评论，0所有人可评论，1粉丝才可评论
        /// </summary>
        public int OnlyFansCanComment { get; set; }

        public bool IsDelete { get; set; }
    }
}
