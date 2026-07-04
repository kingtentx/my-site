namespace MySite.Web.Models
{
    public class WecatConfigModel
    {
        /// <summary>
        /// 微信appid
        /// </summary>
        public string AppId { get; set; } = string.Empty;
        /// <summary>
        /// 微信secret
        /// </summary>
        public string AppSecret { get; set; } = string.Empty;
        /// <summary>
        /// 微信token
        /// </summary>
        public string Token { get; set; } = string.Empty;
        /// <summary>
        /// 微信加密key
        /// </summary>
        public string EncodingAESKey { get; set; } = string.Empty;
    }
}
