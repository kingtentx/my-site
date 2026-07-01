using CIMC.Data;
using CIMC.EntityFramework;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System;
using System.Threading;
using System.Threading.Channels;
using System.Threading.Tasks;

namespace CimcSite.Web.Services
{
    public interface IAuditLogQueue
    {
        ValueTask EnqueueAsync(AuditLog auditLog);
        ValueTask<AuditLog> DequeueAsync(CancellationToken cancellationToken);
    }

    public class AuditLogQueue : IAuditLogQueue
    {
        private readonly Channel<AuditLog> _channel;

        public AuditLogQueue()
        {
            _channel = Channel.CreateBounded<AuditLog>(new BoundedChannelOptions(10000)
            {
                FullMode = BoundedChannelFullMode.DropOldest,
                SingleReader = true,
                SingleWriter = false
            });
        }

        public async ValueTask EnqueueAsync(AuditLog auditLog)
        {
            await _channel.Writer.WriteAsync(auditLog);
        }

        public async ValueTask<AuditLog> DequeueAsync(CancellationToken cancellationToken)
        {
            return await _channel.Reader.ReadAsync(cancellationToken);
        }
    }

    public class AuditLogBackgroundService : BackgroundService
    {
        private readonly IAuditLogQueue _queue;
        private readonly IServiceScopeFactory _scopeFactory;
        private readonly ILogger<AuditLogBackgroundService> _logger;

        public AuditLogBackgroundService(
            IAuditLogQueue queue,
            IServiceScopeFactory scopeFactory,
            ILogger<AuditLogBackgroundService> logger)
        {
            _queue = queue;
            _scopeFactory = scopeFactory;
            _logger = logger;
        }

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

        private static string ComputeHash(AuditLog log)
        {
            var raw = $"{log.UserId}|{log.OperationTime:O}|{log.OperationType}|{log.OperationModule}|{log.RequestUrl}|{log.ResultStatus}|{log.OldData}|{log.NewData}";
            using var sha = System.Security.Cryptography.SHA256.Create();
            var bytes = sha.ComputeHash(System.Text.Encoding.UTF8.GetBytes(raw));
            return Convert.ToBase64String(bytes);
        }
    }
}
