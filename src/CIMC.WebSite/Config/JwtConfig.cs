namespace CimcSite.Web.Config
{
    public class JwtConfig
    {
        /// <summary>
        /// token是谁颁发的
        /// </summary>
        public string Issuer { get; set; }
        /// <summary>
        /// token可以给哪些客户端使用
        /// </summary>
        public string Audience { get; set; }
        /// <summary>
        /// 加密的key
        /// </summary>
        public string SecretKey { get; set; }
        /// <summary>
        /// 过期时间（分钟）
        /// </summary>
        public double Expiration { get; set; } = 30;
    }
}
