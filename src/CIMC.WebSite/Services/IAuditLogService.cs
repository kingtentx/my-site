using CIMC.Data;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace CimcSite.Web.Services
{
    public interface IAuditLogService
    {
        Task LogAsync(AuditLog auditLog);

        Task LogAsync(string userId, string userName, string ipAddress,
            string operationType, string operationModule, string operationDesc,
            string requestUrl, string httpMethod,
            string requestData, string oldData, string newData,
            string resultStatus, string resultMessage, long duration);

        (List<AuditLog> List, int Count) GetList(string userId, string userName,
            string operationType, string operationModule, string resultStatus,
            string startTime, string endTime,
            int pageIndex, int pageSize);

        List<AuditLog> Export(string userId, string userName,
            string operationType, string operationModule, string resultStatus,
            string startTime, string endTime);

        bool VerifyHash(long id);

        (int Total, int Tampered) VerifyAllHashes();

        int ArchiveLogs(DateTime beforeDate);

        Dictionary<string, int> GetOperationTypeStats(string startTime, string endTime);

        Dictionary<string, int> GetModuleStats(string startTime, string endTime);
    }
}
