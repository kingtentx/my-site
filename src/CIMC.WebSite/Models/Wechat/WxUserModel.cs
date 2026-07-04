using Newtonsoft.Json;
using System;

namespace MySite.Web.Models
{
    public class WxUserModel
    {
        public int Id { get; set; }
        /// <summary>
        /// 手机号码
        /// </summary>     
        public string Telphone { get; set; }
        /// <summary>
        /// 用户的唯一标识
        /// </summary>       
        [JsonProperty("openid")]
        public string Openid { get; set; }
        /// <summary>
        ///  用户昵称
        /// </summary>     
        [JsonProperty("nickname")]
        public string Nickname { get; set; }
        /// <summary>
        /// 用户的性别，值为1时是男性，值为2时是女性，值为0时是未知
        /// </summary>
        [JsonProperty("sex")]
        public int Sex { get; set; }
        /// <summary>
        /// 语言地区，如中国为zh_CN
        /// </summary>     
        [JsonProperty("language")]
        public string Language { get; set; }
        /// <summary>
        ///  用户个人资料填写的省份
        /// </summary>      
        [JsonProperty("province")]
        public string Province { get; set; }
        /// <summary>
        /// 普通用户个人资料填写的城市
        /// </summary>       
        [JsonProperty("city")]
        public string City { get; set; }
        /// <summary>
        /// 国家，如中国
        /// </summary>       
        [JsonProperty("country")]
        public string Country { get; set; }
        /// <summary>
        /// 用户头像，
        /// 最后一个数值代表正方形头像大小（有0、46、64、96、132数值可选，0代表640*640正方形头像）
        /// 用户没有头像时该项为空
        /// </summary>       
        [JsonProperty("headimgurl")]
        public string Headimgurl { get; set; }

        /// <summary>
        /// 用户是否订阅该公众号标识
        /// 值为0时，代表此用户没有关注该公众号，拉取不到其余信息。
        /// </summary>
        [JsonProperty("subscribe")]
        public int Subscribe { get; set; }
        /// <summary>
        /// 用户关注时间，为时间戳。如果用户曾多次关注，则取最后关注时间
        /// </summary>
        [JsonProperty("subscribe_time")]
        public long SubscribeTime { get; set; }
        /// <summary>
        /// 只有在用户将公众号绑定到微信开放平台帐号后，才会出现该字段。
        /// </summary>       
        [JsonProperty("unionid")]
        public string Unionid { get; set; }
        /// <summary>
        /// 公众号运营者对粉丝的备注，公众号运营者可在微信公众平台用户管理界面对粉丝添加备注
        /// </summary>      
        [JsonProperty("remark")]
        public string Remark { get; set; }
        /// <summary>
        /// 用户所在的分组ID（兼容旧的用户分组接口）
        /// </summary>
        [JsonProperty("groupid")]
        public int Groupid { get; set; }
        /// <summary>
        /// 返回用户关注的渠道来源，
        /// ADD_SCENE_SEARCH 公众号搜索，
        /// ADD_SCENE_ACCOUNT_MIGRATION 公众号迁移，
        /// ADD_SCENE_PROFILE_CARD 名片分享，
        /// ADD_SCENE_QR_CODE 扫描二维码，
        /// ADD_SCENE_PROFILE_LINK 图文页内名称点击，
        /// ADD_SCENE_PROFILE_ITEM 图文页右上角菜单，
        /// ADD_SCENE_PAID 支付后关注，
        /// ADD_SCENE_OTHERS 其他
        /// </summary>       
        [JsonProperty("subscribe_scene")]
        public string SubscribeScene { get; set; }
        /// <summary>
        /// 用户被打上的标签ID列表
        /// </summary>       
        [JsonProperty("tagid_list")]
        public string[] TagidList { get; set; }

        /// <summary>
        /// 二维码扫码场景（开发者自定义）
        /// </summary>      
        [JsonProperty("qr_scene")]
        public int QrScene { get; set; }
        /// <summary>
        /// 二维码扫码场景描述（开发者自定义）
        /// </summary>       
        [JsonProperty("qr_scene_str")]
        public string QrSceneStr { get; set; }
        /// <summary>
        ///  取消关注时间
        /// </summary>
        public DateTime? UnsubscribeTime { get; set; }
        /// <summary>
        /// 创建时间
        /// </summary>
        public DateTime CreationTime { get; set; }
    }
}
