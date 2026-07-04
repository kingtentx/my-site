namespace MySite.Web
{
    public class RabbitMQConfig
    {
        /// <summary>
        /// 连接地址
        /// </summary>
        public string HostName { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public string UserName { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public string Password { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public string VirtualHost { get; set; } = "/";
        /// <summary>
        /// 
        /// </summary>
        public int Port { get; set; } = 5672;

        /// <summary>
        /// 连接字符串
        /// </summary>
        public string ConnectionString
        {
            get
            {
                return $"host={HostName}:{Port};virtualHost={VirtualHost};username={UserName};password={Password}";
            }
        }
    }
}
