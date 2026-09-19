using CIMC.Data;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace MySite.Web.Services
{
    /// <summary>定义IAuditLog服务的接口。</summary>
    public interface IAuditLogService
    {
        /// <summary>异步记录审计日志。</summary>
        Task LogAsync(AuditLog auditLog);

        /// <summary>异步记录审计日志。</summary>
        Task LogAsync(string userId, string userName, string ipAddress,
            string operationType, string operationModule, string operationDesc,
            string requestUrl, string httpMethod,
            string requestData, string oldData, string newData,
            string resultStatus, string resultMessage, long duration);

        /// <summary>查询IAuditLog列表。</summary>
        (List<AuditLog> List, int Count) GetList(string userId, string userName,
            string operationType, string operationModule, string resultStatus,
            string startTime, string endTime,
            int pageIndex, int pageSize);

        /// <summary>导出符合筛选条件的记录。</summary>
        List<AuditLog> Export(string userId, string userName,
            string operationType, string operationModule, string resultStatus,
            string startTime, string endTime);

        /// <summary>验证审计日志的哈希值。</summary>
        bool VerifyHash(long id);

        /// <summary>验证全部审计日志的哈希值。</summary>
        (int Total, int Tampered) VerifyAllHashes();

        /// <summary>归档旧审计日志。</summary>
        int ArchiveLogs(DateTime beforeDate);

        /// <summary>统计各操作类型的日志数量。</summary>
        Dictionary<string, int> GetOperationTypeStats(string startTime, string endTime);

        /// <summary>统计各模块的日志数量。</summary>
        Dictionary<string, int> GetModuleStats(string startTime, string endTime);
    }
}
