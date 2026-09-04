using CIMC.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CIMC.EntityFramework.Migrations
{
    [DbContext(typeof(AppDbContext))]
    [Migration("20260905010000_MergePageNavigationIntoPageTree")]
    public partial class MergePageNavigationIntoPageTree : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // 页面树已经承担前台导航，旧导航/旧 Footer 单例表不再属于运行模型。
            migrationBuilder.Sql("DROP TABLE IF EXISTS `WebsiteNavigation`;");
            migrationBuilder.Sql("DROP TABLE IF EXISTS `WebsiteFooter`;");

            // 这些字段已经迁移到全局 Header/Footer Builder；Theme/Language 旧配置也不再参与渲染。
            DropColumnIfExists(migrationBuilder, "WebsiteSiteConfig", "HeaderBgColor");
            DropColumnIfExists(migrationBuilder, "WebsiteSiteConfig", "HeaderTextColor");
            DropColumnIfExists(migrationBuilder, "WebsiteSiteConfig", "HeaderActiveColor");
            DropColumnIfExists(migrationBuilder, "WebsiteSiteConfig", "HeaderFixedTop");
            DropColumnIfExists(migrationBuilder, "WebsiteSiteConfig", "Theme");
            DropColumnIfExists(migrationBuilder, "WebsiteSiteConfig", "Language");

            migrationBuilder.Sql(@"
DELETE FROM `RoleMenu`
WHERE `Permission` = 'Site_Footer'
   OR `Permission` LIKE 'Site_Footer\_%'
   OR `Permission` = 'Site_Navigation'
   OR `Permission` LIKE 'Site_Navigation\_%';");

            migrationBuilder.Sql(@"
DELETE FROM `Menu`
WHERE `PermissionKey` IN ('Site_Footer', 'Site_Navigation')
   OR `Path` IN ('/footer/index', '/navigation/index');");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            // 本项目明确不兼容旧版 Site Builder。Down 不恢复已经废弃的数据结构，
            // 避免回滚时重新引入两套导航与 Footer 配置来源。
        }

        private static void DropColumnIfExists(MigrationBuilder migrationBuilder, string table, string column)
        {
            migrationBuilder.Sql($@"
SET @sql = IF(
    EXISTS(
        SELECT 1
        FROM information_schema.COLUMNS
        WHERE TABLE_SCHEMA = DATABASE()
          AND TABLE_NAME = '{table}'
          AND COLUMN_NAME = '{column}'
    ),
    'ALTER TABLE `{table}` DROP COLUMN `{column}`',
    'SELECT 1'
);
PREPARE stmt FROM @sql;
EXECUTE stmt;
DEALLOCATE PREPARE stmt;");
        }
    }
}
