using CIMC.Data.ExtModel;
using System;
using System.ComponentModel.DataAnnotations;

namespace CIMC.Data
{
    /// <summary>记录后台操作及其请求和结果信息。</summary>
    public class AuditLog
    {
        /// <summary>主键。</summary>
        [Key]
        public long Id { get; set; }

        /// <summary>用户主键。</summary>
        [StringLength(ModelUnits.Len_50)]
        public string UserId { get; set; }

        /// <summary>用户名。</summary>
        [StringLength(ModelUnits.Len_100)]
        public string UserName { get; set; }

        /// <summary>操作时间。</summary>
        public DateTime OperationTime { get; set; }

        /// <summary>请求来源 IP 地址。</summary>
        [StringLength(ModelUnits.Len_50)]
        public string IpAddress { get; set; }

        /// <summary>操作类型。</summary>
        [StringLength(ModelUnits.Len_20)]
        public string OperationType { get; set; }

        /// <summary>操作所属模块。</summary>
        [StringLength(ModelUnits.Len_100)]
        public string OperationModule { get; set; }

        /// <summary>操作说明。</summary>
        [StringLength(ModelUnits.Len_200)]
        public string OperationDesc { get; set; }

        /// <summary>请求访问地址。</summary>
        [StringLength(ModelUnits.Len_500)]
        public string RequestUrl { get; set; }

        /// <summary>请求使用的 HTTP 方法。</summary>
        [StringLength(ModelUnits.Len_20)]
        public string HttpMethod { get; set; }

        /// <summary>请求数据。</summary>
        public string RequestData { get; set; }

        /// <summary>修改前的数据。</summary>
        public string OldData { get; set; }

        /// <summary>修改后的数据。</summary>
        public string NewData { get; set; }

        /// <summary>操作执行状态。</summary>
        [StringLength(ModelUnits.Len_20)]
        public string ResultStatus { get; set; }

        /// <summary>操作结果说明。</summary>
        [StringLength(ModelUnits.Len_500)]
        public string ResultMessage { get; set; }

        /// <summary>当前日志的数据哈希值。</summary>
        [StringLength(ModelUnits.Len_64)]
        public string DataHash { get; set; }

        /// <summary>操作耗时。</summary>
        public long Duration { get; set; }

        /// <summary>客户端 User-Agent 信息。</summary>
        [StringLength(ModelUnits.Len_500)]
        public string UserAgent { get; set; }

        /// <summary>操作对应的数据表。</summary>
        [StringLength(ModelUnits.Len_100)]
        public string OperationTable { get; set; }

        /// <summary>操作记录的主键。</summary>
        [StringLength(ModelUnits.Len_50)]
        public string RecordId { get; set; }

        /// <summary>是否满足 Archived 条件。</summary>
        public bool IsArchived { get; set; }

        /// <summary>上一条日志的哈希值。</summary>
        [StringLength(ModelUnits.Len_64)]
        public string PreviousHash { get; set; }
    }
}
