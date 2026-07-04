namespace MySite.Web.Models
{

    public class CompanyModel
    {
        public string Id { get; set; }

        public string Type { get; set; }

        public bool IsShowMsg { get; set; }

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
