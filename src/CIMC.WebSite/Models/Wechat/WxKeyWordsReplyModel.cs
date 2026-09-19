using System;
using System.Collections.Generic;

namespace MySite.Web.Models
{
    /// <summary>承载微信相关数据。</summary>
    public class WxKeyWordsReplyModel
    {
        /// <summary>主键。</summary>
        public int Id { get; set; }
        /// <summary>
        /// 规则名称
        /// </summary>      
        public string KeyName { get; set; }
        /// <summary>
        /// 回复类型（多个默认其中一个）0-text 1-news 2-image 3-voice 4-video
        /// </summary>
        public string MsgType { get; set; }
        /// <summary>
        /// 回复方式  0:回复全部  1: 随机回复一条
        /// </summary>
        public int ReplyType { get; set; }

        /// <summary>
        /// 关键词
        /// </summary>
        public string KeyWordsJson { get; set; }

        /// <summary>
        /// 图文ID
        /// </summary>
        public long MeId { get; set; }
        /// <summary>
        /// 回复内容
        /// </summary>       
        public string Content { get; set; }
        /// <summary>
        /// 封面图片
        /// </summary>
        public string CoverUrl { get; set; }
        /// <summary>
        /// 图文素材
        /// </summary>      
        public string News_MediaId { get; set; }
        /// <summary>
        /// 图片素材
        /// </summary>      
        public string Image_MediaId { get; set; }
        /// <summary>
        /// 语音素材
        /// </summary>
        public string Voice_MediaId { get; set; }
        /// <summary>
        /// 视频素材
        /// </summary>

        public string Video_MediaId { get; set; }
        /// <summary>
        /// 是否关注回复
        /// </summary>
        public bool IsSubscribe { get; set; } = false;
        /// <summary>
        /// 是否启用
        /// </summary>
        public bool IsActive { get; set; } = false;

        /// <summary>创建时间。</summary>
        public DateTime? CreationTime { get; set; } = DateTime.Now;

        /// <summary>备注信息。</summary>
        public string Remark { get; set; }

        /// <summary>微信相关项目集合。</summary>
        public List<WxKeyWordsModel> WxKeyWordsList { get; set; } = new List<WxKeyWordsModel>();
    }

    /// <summary>承载微信相关数据。</summary>
    public class WxKeyWordsModel
    {
        /// <summary>
        /// Id
        /// </summary>       
        public int Id { get; set; }
        /// <summary>
        /// 规则ID
        /// </summary>
        public int RuleId { get; set; }
        /// <summary>
        /// 匹配类型  0:全匹配  1:半匹配
        /// </summary>
        public int KeyType { get; set; }
        /// <summary>
        /// 关键词
        /// </summary>          
        public string KeyWords { get; set; }

        /// <summary>创建时间。</summary>
        public DateTime? CreationTime { get; set; } = DateTime.Now;
    }
}
