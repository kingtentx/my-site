namespace MySite.Web.Models
{

    /// <summary>承载公司相关数据。</summary>
    public class CompanyModel
    {
        /// <summary>主键。</summary>
        public string Id { get; set; }

        /// <summary>业务类型。</summary>
        public string Type { get; set; }

        /// <summary>是否满足 ShowMsg 条件。</summary>
        public bool IsShowMsg { get; set; }

        /// <summary>数据内容。</summary>
        public CompanyInfo Data { get; set; }
    }
    /// <summary>
    /// 联系我们
    /// </summary>
    public class CompanyInfo
    {
        /// <summary>
        /// 联系电话
        /// </summary>
        public string Phone { get; set; }
        /// <summary>
        /// 工作时间
        /// </summary>
        public string WorkTime { get; set; }
        /// <summary>
        /// 邮箱
        /// </summary>
        public string Email { get; set; }
        /// <summary>
        /// 公司地址
        /// </summary>
        public string Address { get; set; }
        /// <summary>
        /// 定位
        /// </summary>
        public string Location { get; set; }
        /// <summary>
        /// 是否开启留言
        /// </summary>
        public bool IsShowMsg { get; set; }
    }
}
