using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CIMC.EntityFramework.Migrations
{
    /// <inheritdoc />
    public partial class AddSiteHeaderStyle : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "HeaderActiveColor",
                table: "WebsiteSiteConfig",
                type: "varchar(20)",
                maxLength: 20,
                nullable: true)
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AddColumn<string>(
                name: "HeaderBgColor",
                table: "WebsiteSiteConfig",
                type: "varchar(20)",
                maxLength: 20,
                nullable: true)
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AddColumn<bool>(
                name: "HeaderFixedTop",
                table: "WebsiteSiteConfig",
                type: "tinyint(1)",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<string>(
                name: "HeaderTextColor",
                table: "WebsiteSiteConfig",
                type: "varchar(20)",
                maxLength: 20,
                nullable: true)
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.Sql("UPDATE `WebsiteSiteConfig` SET `HeaderBgColor` = IFNULL(`HeaderBgColor`, '#ffffff'), `HeaderTextColor` = IFNULL(`HeaderTextColor`, '#333333'), `HeaderActiveColor` = IFNULL(`HeaderActiveColor`, '#1e9fff')");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "HeaderActiveColor",
                table: "WebsiteSiteConfig");

            migrationBuilder.DropColumn(
                name: "HeaderBgColor",
                table: "WebsiteSiteConfig");

            migrationBuilder.DropColumn(
                name: "HeaderFixedTop",
                table: "WebsiteSiteConfig");

            migrationBuilder.DropColumn(
                name: "HeaderTextColor",
                table: "WebsiteSiteConfig");
        }
    }
}