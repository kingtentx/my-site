namespace VPhonor.AdminSite.Models
{
    public class JsSdkModel
    {
        /// <summary>
        ///  微信AppId
        /// </summary>
        public string appId { get; set; }
        ///时间戳 
        public string timestamp { get; set; }
        /// <summary>
        /// 随机码
        /// </summary>
        public string nonceStr { get; set; }
        /// <summary>
        /// 签名 
        /// </summary>
        public string signature { get; set; }
    }
}
