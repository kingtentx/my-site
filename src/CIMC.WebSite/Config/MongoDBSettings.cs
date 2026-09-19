namespace MySite.Web
{
    /// <summary>保存MongoDB的配置。</summary>
    public class MongoDBSettings
    {
        /// <summary>数据库连接字符串。</summary>
        public string ConnectionString { get; set; }
        /// <summary>数据库名称。</summary>
        public string Database { get; set; }
    }
}
