using CIMC.Data;
using CIMC.EntityFramework;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System;
using System.Threading;
using System.Threading.Channels;
using System.Threading.Tasks;

namespace MySite.Web.Services
{
    /// <summary>定义IAuditLog服务的接口。</summary>
    public interface IAuditLogQueue
    {
        /// <summary>将审计日志加入后台队列。</summary>
        ValueTask EnqueueAsync(AuditLog auditLog);
        /// <summary>从后台队列取出审计日志。</summary>
        ValueTask<AuditLog> DequeueAsync(CancellationToken cancellationToken);
    }

    /// <summary>管理审计日志处理队列。</summary>
    public class AuditLogQueue : IAuditLogQueue
    {
        private readonly Channel<AuditLog> _channel;

        /// <summary>初始化审计日志。</summary>
        public AuditLogQueue()
        {
            _channel = Channel.CreateBounded<AuditLog>(new BoundedChannelOptions(10000)
            {
                FullMode = BoundedChannelFullMode.DropOldest,
                SingleReader = true,
                SingleWriter = false
            });
        }

        /// <summary>将审计日志加入后台队列。</summary>
        public async ValueTask EnqueueAsync(AuditLog auditLog)
        {
            await _channel.Writer.WriteAsync(auditLog);
        }

        /// <summary>从后台队列取出审计日志。</summary>
        public async ValueTask<AuditLog> DequeueAsync(CancellationToken cancellationToken)
        {
            return await _channel.Reader.ReadAsync(cancellationToken);
        }
    }

    /// <summary>提供审计日志相关服务。</summary>
    public class AuditLogBackgroundService : BackgroundService
    {
        private readonly IAuditLogQueue _queue;
        private readonly IServiceScopeFactory _scopeFactory;
        private readonly ILogger<AuditLogBackgroundService> _logger;

        /// <summary>初始化审计日志。</summary>
        public AuditLogBackgroundService(
            IAuditLogQueue queue,
            IServiceScopeFactory scopeFactory,
            ILogger<AuditLogBackgroundService> logger)
        {
            _queue = queue;
            _scopeFactory = scopeFactory;
            _logger = logger;
        }

        /// <summary>运行后台处理任务。</summary>
        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation("审计日志后台服务已启动");

            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    var auditLog = await _queue.DequeueAsync(stoppingToken);

                    using var scope = _scopeFactory.CreateScope();
                    var repository = scope.ServiceProvider.GetRequiredService<IRepository<AuditLog>>();

                    auditLog.DataHash = ComputeHash(auditLog);
                    await repository.AddAsync(auditLog);
                }
                catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
                {
                    break;
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "审计日志写入失败");
                    await Task.Delay(1000, stoppingToken);
                }
            }

            _logger.LogInformation("审计日志后台服务已停止");
        }

        /// <summary>计算审计日志的校验哈希值。</summary>
        private static string ComputeHash(AuditLog log)
        {
            var raw = $"{log.UserId}|{log.OperationTime:O}|{log.OperationType}|{log.OperationModule}|{log.RequestUrl}|{log.ResultStatus}|{log.OldData}|{log.NewData}";
            using var sha = System.Security.Cryptography.SHA256.Create();
            var bytes = sha.ComputeHash(System.Text.Encoding.UTF8.GetBytes(raw));
            return Convert.ToBase64String(bytes);
        }
    }
}
