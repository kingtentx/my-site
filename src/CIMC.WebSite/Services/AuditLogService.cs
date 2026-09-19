using CIMC.Data;
using CIMC.EntityFramework;
using CIMC.Helper;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace MySite.Web.Services
{
    /// <summary>提供审计日志相关服务。</summary>
    public class AuditLogService : IAuditLogService
    {
        private readonly IRepository<AuditLog> _repository;
        private readonly IAuditLogQueue _queue;

        /// <summary>初始化审计日志。</summary>
        public AuditLogService(IRepository<AuditLog> repository, IAuditLogQueue queue)
        {
            _repository = repository;
            _queue = queue;
        }

        /// <summary>异步记录审计日志。</summary>
        public async Task LogAsync(AuditLog auditLog)
        {
            if (auditLog == null) return;

            auditLog.DataHash = ComputeHash(auditLog);
            await _queue.EnqueueAsync(auditLog);
        }

        /// <summary>异步记录审计日志。</summary>
        public async Task LogAsync(string userId, string userName, string ipAddress,
            string operationType, string operationModule, string operationDesc,
            string requestUrl, string httpMethod,
            string requestData, string oldData, string newData,
            string resultStatus, string resultMessage, long duration)
        {
            var log = new AuditLog
            {
                UserId = userId,
                UserName = userName,
                IpAddress = ipAddress,
                OperationType = operationType,
                OperationModule = operationModule,
                OperationDesc = operationDesc,
                RequestUrl = requestUrl,
                HttpMethod = httpMethod,
                RequestData = Truncate(requestData, 8000),
                OldData = Truncate(oldData, 8000),
                NewData = Truncate(newData, 8000),
                ResultStatus = resultStatus,
                ResultMessage = resultMessage,
                OperationTime = DateTime.Now,
                Duration = duration
            };

            await LogAsync(log);
        }

        /// <summary>查询审计日志列表。</summary>
        public (List<AuditLog> List, int Count) GetList(string userId, string userName,
            string operationType, string operationModule, string resultStatus,
            string startTime, string endTime,
            int pageIndex, int pageSize)
        {
            var where = BuildWhere(userId, userName, operationType, operationModule, resultStatus, startTime, endTime);
            return _repository.GetList(where, p => p.Id, pageIndex, pageSize, false);
        }

        /// <summary>导出符合筛选条件的记录。</summary>
        public List<AuditLog> Export(string userId, string userName,
            string operationType, string operationModule, string resultStatus,
            string startTime, string endTime)
        {
            var where = BuildWhere(userId, userName, operationType, operationModule, resultStatus, startTime, endTime);
            return _repository.GetList(where, p => p.Id, false);
        }

        /// <summary>验证审计日志的哈希值。</summary>
        public bool VerifyHash(long id)
        {
            var log = _repository.GetOne(id);
            if (log == null) return false;

            var currentHash = ComputeHash(log);
            return currentHash == log.DataHash;
        }

        /// <summary>验证全部审计日志的哈希值。</summary>
        public (int Total, int Tampered) VerifyAllHashes()
        {
            var allLogs = _repository.GetList(p => !p.IsArchived);
            int tampered = 0;
            foreach (var log in allLogs)
            {
                var currentHash = ComputeHash(log);
                if (currentHash != log.DataHash)
                    tampered++;
            }
            return (allLogs.Count, tampered);
        }

        /// <summary>归档旧审计日志。</summary>
        public int ArchiveLogs(DateTime beforeDate)
        {
            var where = LambdaHelper.True<AuditLog>()
                .And(p => p.OperationTime < beforeDate)
                .And(p => !p.IsArchived);

            var logs = _repository.GetList(where);
            int count = 0;
            foreach (var log in logs)
            {
                log.IsArchived = true;
                _repository.Update(log);
                count++;
            }
            return count;
        }

        /// <summary>统计各操作类型的日志数量。</summary>
        public Dictionary<string, int> GetOperationTypeStats(string startTime, string endTime)
        {
            var where = LambdaHelper.True<AuditLog>();

            if (!string.IsNullOrWhiteSpace(startTime) && DateTime.TryParse(startTime, out var st))
                where = where.And(p => p.OperationTime >= st);
            if (!string.IsNullOrWhiteSpace(endTime) && DateTime.TryParse(endTime, out var et))
                where = where.And(p => p.OperationTime <= et);

            var logs = _repository.GetList(where);
            return logs.GroupBy(p => p.OperationType)
                       .ToDictionary(g => g.Key, g => g.Count());
        }

        /// <summary>统计各模块的日志数量。</summary>
        public Dictionary<string, int> GetModuleStats(string startTime, string endTime)
        {
            var where = LambdaHelper.True<AuditLog>();

            if (!string.IsNullOrWhiteSpace(startTime) && DateTime.TryParse(startTime, out var st))
                where = where.And(p => p.OperationTime >= st);
            if (!string.IsNullOrWhiteSpace(endTime) && DateTime.TryParse(endTime, out var et))
                where = where.And(p => p.OperationTime <= et);

            var logs = _repository.GetList(where);
            return logs.GroupBy(p => p.OperationModule)
                       .ToDictionary(g => g.Key, g => g.Count());
        }

        /// <summary>构建审计日志查询条件。</summary>
        private System.Linq.Expressions.Expression<Func<AuditLog, bool>> BuildWhere(
            string userId, string userName, string operationType, string operationModule,
            string resultStatus, string startTime, string endTime)
        {
            var where = LambdaHelper.True<AuditLog>();

            if (!string.IsNullOrWhiteSpace(userId))
                where = where.And(p => p.UserId == userId);

            if (!string.IsNullOrWhiteSpace(userName))
                where = where.And(p => p.UserName.Contains(userName));

            if (!string.IsNullOrWhiteSpace(operationType))
                where = where.And(p => p.OperationType == operationType);

            if (!string.IsNullOrWhiteSpace(operationModule))
                where = where.And(p => p.OperationModule.Contains(operationModule));

            if (!string.IsNullOrWhiteSpace(resultStatus))
                where = where.And(p => p.ResultStatus == resultStatus);

            if (!string.IsNullOrWhiteSpace(startTime) && DateTime.TryParse(startTime, out var st))
                where = where.And(p => p.OperationTime >= st);

            if (!string.IsNullOrWhiteSpace(endTime) && DateTime.TryParse(endTime, out var et))
                where = where.And(p => p.OperationTime <= et);

            return where;
        }

        /// <summary>计算审计日志的校验哈希值。</summary>
        private static string ComputeHash(AuditLog log)
        {
            var raw = $"{log.UserId}|{log.OperationTime:O}|{log.OperationType}|{log.OperationModule}|{log.RequestUrl}|{log.ResultStatus}|{log.OldData}|{log.NewData}";
            using var sha = SHA256.Create();
            var bytes = sha.ComputeHash(Encoding.UTF8.GetBytes(raw));
            return Convert.ToBase64String(bytes);
        }

        /// <summary>将文本截断到指定长度。</summary>
        private static string Truncate(string value, int maxLength)
        {
            if (string.IsNullOrEmpty(value)) return value;
            return value.Length <= maxLength ? value : value.Substring(0, maxLength);
        }
    }

    /// <summary>提供NoopAuditLog相关服务。</summary>
    public class NoopAuditLogService : IAuditLogService
    {
        /// <summary>异步记录审计日志。</summary>
        public Task LogAsync(AuditLog auditLog) => Task.CompletedTask;

        /// <summary>异步记录审计日志。</summary>
        public Task LogAsync(string userId, string userName, string ipAddress,
            string operationType, string operationModule, string operationDesc,
            string requestUrl, string httpMethod,
            string requestData, string oldData, string newData,
            string resultStatus, string resultMessage, long duration) => Task.CompletedTask;

        /// <summary>查询NoopAuditLog列表。</summary>
        public (List<AuditLog> List, int Count) GetList(string userId, string userName,
            string operationType, string operationModule, string resultStatus,
            string startTime, string endTime,
            int pageIndex, int pageSize) => (new List<AuditLog>(), 0);

        /// <summary>导出符合筛选条件的记录。</summary>
        public List<AuditLog> Export(string userId, string userName,
            string operationType, string operationModule, string resultStatus,
            string startTime, string endTime) => new List<AuditLog>();

        /// <summary>验证审计日志的哈希值。</summary>
        public bool VerifyHash(long id) => true;

        /// <summary>验证全部审计日志的哈希值。</summary>
        public (int Total, int Tampered) VerifyAllHashes() => (0, 0);

        /// <summary>归档旧审计日志。</summary>
        public int ArchiveLogs(DateTime beforeDate) => 0;

        /// <summary>统计各操作类型的日志数量。</summary>
        public Dictionary<string, int> GetOperationTypeStats(string startTime, string endTime) => new Dictionary<string, int>();

        /// <summary>统计各模块的日志数量。</summary>
        public Dictionary<string, int> GetModuleStats(string startTime, string endTime) => new Dictionary<string, int>();
    }
}
