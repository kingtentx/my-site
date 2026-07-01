using System;

namespace CimcSite.Web.Models
{
    public class MessageBoardModel
    {
        public long Id { get; set; }
        /// <summary>
        /// 姓名
        /// </summary>

        public string UserName { get; set; }
        /// <summary>
        /// 邮箱
        /// </summary>

        public string Email { get; set; }
        /// <summary>
        /// 电话
        /// </summary>

        public string Phone { get; set; }
        /// <summary>
        /// 留言
        /// </summary>

        public string Message { get; set; }

        public bool IsRead { get; set; }

        public DateTime? CreationTime { get; set; }

        public string ValidateKey { get; set; }

        public string ValidateCode { get; set; }

    }
}
