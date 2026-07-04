using System;

namespace MySite.Web.Models
{
    public class WeChatUserModel
    {
        public int Id { get; set; }
        /// <summary>
        /// 手机号码
        /// </summary>
        public string Telphone { get; set; }

        /// <summary>
        /// 用户的唯一标识
        /// </summary>
        public string Openid { get; set; }
        /// <summary>
        ///  用户昵称
        /// </summary>
        public string Nickname { get; set; }

        /// <summary>
        /// 用户的性别，值为1时是男性，值为2时是女性，值为0时是未知
        /// </summary>       
        public int Sex { get; set; }

        /// <summary>
        ///  用户个人资料填写的省份
        /// </summary>
        public string Province { get; set; }
        /// <summary>
        /// 普通用户个人资料填写的城市
        /// </summary>
        public string City { get; set; }

        /// <summary>
        /// 国家，如中国
        /// </summary>
        public string Country { get; set; }

        /// <summary>
        /// 用户头像，
        /// 最后一个数值代表正方形头像大小（有0、46、64、96、132数值可选，0代表640*640正方形头像）
        /// 用户没有头像时该项为空
        /// </summary>
        public string Headimgurl { get; set; }

        /// <summary>
        /// 用户是否订阅该公众号标识
        /// 值为0时，代表此用户没有关注该公众号，拉取不到其余信息。
        /// </summary>       
        public int Subscribe { get; set; }

        /// <summary>
        /// 用户关注时间，为时间戳。如果用户曾多次关注，则取最后关注时间
        /// </summary>     
        public long SubscribeTime { get; set; }

        /// <summary>
        /// 只有在用户将公众号绑定到微信开放平台帐号后，才会出现该字段。
        /// </summary>
        public string Unionid { get; set; }
        /// <summary>
        /// 公众号运营者对粉丝的备注，公众号运营者可在微信公众平台用户管理界面对粉丝添加备注
        /// </summary>
        public string Remark { get; set; }

        /// <summary>
        /// 用户所在的分组ID（兼容旧的用户分组接口）
        /// </summary>       
        public int Groupid { get; set; }

        /// <summary>
        /// 用户被打上的标签ID列表
        /// </summary>
        public virtual string TagidList { get; set; }

        /// <summary>
        ///  取消关注时间
        /// </summary>
        public DateTime? UnsubscribeTime { get; set; }
    }
}
