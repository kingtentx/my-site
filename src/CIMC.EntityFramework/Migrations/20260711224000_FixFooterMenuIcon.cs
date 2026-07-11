using CIMC.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CIMC.EntityFramework.Migrations
{
    [DbContext(typeof(AppDbContext))]
    [Migration("20260711224000_FixFooterMenuIcon")]
    public partial class FixFooterMenuIcon : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(@"
UPDATE `Menu`
SET `Icon` = 'layui-icon-layouts'
WHERE `PermissionKey` = 'Site_Footer'
  AND (`Icon` IS NULL OR TRIM(`Icon`) = '');");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(@"
UPDATE `Menu`
SET `Icon` = NULL
WHERE `PermissionKey` = 'Site_Footer'
  AND `Icon` = 'layui-icon-layouts';");
        }
    }
}
