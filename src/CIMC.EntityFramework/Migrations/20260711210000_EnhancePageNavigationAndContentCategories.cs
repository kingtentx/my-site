using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CIMC.EntityFramework.Migrations
{
    public partial class EnhancePageNavigationAndContentCategories : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "ParentId",
                table: "WebsitePage",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<bool>(
                name: "ShowInNavigation",
                table: "WebsitePage",
                type: "tinyint(1)",
                nullable: false,
                defaultValue: true);

            migrationBuilder.AddColumn<string>(
                name: "NavigationTitle",
                table: "WebsitePage",
                type: "varchar(100)",
                maxLength: 100,
                nullable: true)
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AddColumn<string>(
                name: "NavigationIcon",
                table: "WebsitePage",
                type: "varchar(100)",
                maxLength: 100,
                nullable: true)
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AddColumn<int>(
                name: "NavigationTarget",
                table: "WebsitePage",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "CategoryId",
                table: "ContentJob",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateIndex(
                name: "IX_WebsitePage_ParentId",
                table: "WebsitePage",
                column: "ParentId");

            migrationBuilder.CreateIndex(
                name: "IX_ContentJob_CategoryId",
                table: "ContentJob",
                column: "CategoryId");

            // 兼容已有导航数据：按路径回填页面的导航属性和父页面关系。
            migrationBuilder.Sql(@"
UPDATE `WebsitePage` p
LEFT JOIN `WebsiteNavigation` n ON n.`Path` = p.`PagePath` AND n.`IsDelete` = 0
SET p.`ShowInNavigation` = IFNULL(n.`IsShow`, 1),
    p.`NavigationTitle` = NULLIF(n.`Title`, ''),
    p.`NavigationIcon` = n.`Icon`,
    p.`NavigationTarget` = IFNULL(n.`Target`, 0);

UPDATE `WebsitePage` childPage
INNER JOIN `WebsiteNavigation` childNav ON childNav.`Path` = childPage.`PagePath` AND childNav.`IsDelete` = 0
INNER JOIN `WebsiteNavigation` parentNav ON parentNav.`Id` = childNav.`Pid` AND parentNav.`IsDelete` = 0
INNER JOIN `WebsitePage` parentPage ON parentPage.`PagePath` = parentNav.`Path` AND parentPage.`IsDelete` = 0
SET childPage.`ParentId` = parentPage.`Id`;");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(name: "IX_WebsitePage_ParentId", table: "WebsitePage");
            migrationBuilder.DropIndex(name: "IX_ContentJob_CategoryId", table: "ContentJob");
            migrationBuilder.DropColumn(name: "ParentId", table: "WebsitePage");
            migrationBuilder.DropColumn(name: "ShowInNavigation", table: "WebsitePage");
            migrationBuilder.DropColumn(name: "NavigationTitle", table: "WebsitePage");
            migrationBuilder.DropColumn(name: "NavigationIcon", table: "WebsitePage");
            migrationBuilder.DropColumn(name: "NavigationTarget", table: "WebsitePage");
            migrationBuilder.DropColumn(name: "CategoryId", table: "ContentJob");
        }
    }
}
