using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.Extensions.Configuration;
using System.IO;

namespace CIMC.EntityFrameworkCore
{
    /// <summary>创建AppDbContext相关对象。</summary>
    public class AppDbContextFactory : IDesignTimeDbContextFactory<AppDbContext>
    {
        /// <summary>为设计时工具创建数据库上下文。</summary>
        public AppDbContext CreateDbContext(string[] args)
        {
            var configuration = BuildConfiguration();

            var builder = new DbContextOptionsBuilder<AppDbContext>()
                .UseMySql(configuration.GetConnectionString("Default"),
                MySqlServerVersion.LatestSupportedServerVersion,
                mysqlOptions => mysqlOptions.EnableRetryOnFailure());

            return new AppDbContext(builder.Options);
        }

        /// <summary>构建数据库迁移使用的应用配置。</summary>
        private static IConfigurationRoot BuildConfiguration()
        {
            var builder = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("appsettings.json", optional: false);

            return builder.Build();
        }
    }
}
