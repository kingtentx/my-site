using System.Collections.Generic;

namespace MySite.Web.Config
{
    /// <summary>保存审计日志的配置。</summary>
    public class AuditLogConfig
    {
        /// <summary>是否满足 Enabled 条件。</summary>
        public bool IsEnabled { get; set; } = true;

        /// <summary>需要记录审计日志的操作类型。</summary>
        public List<string> RecordOperations { get; set; } = new List<string>
        {
            "Login", "Logout", "Add", "Edit", "Delete", "Authorize", "Upload"
        };

        /// <summary>判断指定操作是否需要写入审计日志。</summary>
        public bool ShouldRecord(string operationType)
        {
            if (!IsEnabled) return false;
            if (RecordOperations == null || RecordOperations.Count == 0) return true;
            return RecordOperations.Contains(operationType);
        }
    }
}
