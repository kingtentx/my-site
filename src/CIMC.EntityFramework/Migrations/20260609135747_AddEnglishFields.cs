using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CIMC.EntityFramework.Migrations
{
    /// <inheritdoc />
    public partial class AddEnglishFields : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "TagName_EN",
                table: "Tag",
                type: "varchar(100)",
                maxLength: 100,
                nullable: true)
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AddColumn<string>(
                name: "ModuleName_EN",
                table: "SiteModule",
                type: "varchar(100)",
                maxLength: 100,
                nullable: true)
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AddColumn<string>(
                name: "SettingsJson_EN",
                table: "SiteModule",
                type: "longtext",
                nullable: true)
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AddColumn<string>(
                name: "SubTitle_EN",
                table: "SiteModule",
                type: "varchar(500)",
                maxLength: 500,
                nullable: true)
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AddColumn<string>(
                name: "Title_EN",
                table: "SiteModule",
                type: "varchar(250)",
                maxLength: 250,
                nullable: true)
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AddColumn<string>(
                name: "Description_EN",
                table: "SiteInfo",
                type: "varchar(500)",
                maxLength: 500,
                nullable: true)
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AddColumn<string>(
                name: "Keywords_EN",
                table: "SiteInfo",
                type: "varchar(500)",
                maxLength: 500,
                nullable: true)
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AddColumn<string>(
                name: "NavigationName_EN",
                table: "Navigation",
                type: "varchar(100)",
                maxLength: 100,
                nullable: true)
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AddColumn<string>(
                name: "Detail_EN",
                table: "Job",
                type: "longtext",
                nullable: true)
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AddColumn<string>(
                name: "JobName_EN",
                table: "Job",
                type: "varchar(250)",
                maxLength: 250,
                nullable: true)
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AddColumn<string>(
                name: "Address_EN",
                table: "FooterInfo",
                type: "varchar(500)",
                maxLength: 500,
                nullable: true)
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AddColumn<string>(
                name: "CompanyInfo_EN",
                table: "FooterInfo",
                type: "longtext",
                nullable: true)
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AddColumn<string>(
                name: "Copyright_EN",
                table: "FooterInfo",
                type: "varchar(500)",
                maxLength: 500,
                nullable: true)
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AddColumn<string>(
                name: "Description_EN",
                table: "Article",
                type: "varchar(250)",
                maxLength: 250,
                nullable: true)
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AddColumn<string>(
                name: "Detail_EN",
                table: "Article",
                type: "longtext",
                nullable: true)
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AddColumn<string>(
                name: "Title_EN",
                table: "Article",
                type: "varchar(250)",
                maxLength: 250,
                nullable: true)
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AddColumn<string>(
                name: "Description_EN",
                table: "Album",
                type: "varchar(500)",
                maxLength: 500,
                nullable: true)
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AddColumn<string>(
                name: "Detail_EN",
                table: "Album",
                type: "longtext",
                nullable: true)
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AddColumn<string>(
                name: "Title_EN",
                table: "Album",
                type: "varchar(250)",
                maxLength: 250,
                nullable: true)
                .Annotation("MySql:CharSet", "utf8mb4");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "TagName_EN",
                table: "Tag");

            migrationBuilder.DropColumn(
                name: "ModuleName_EN",
                table: "SiteModule");

            migrationBuilder.DropColumn(
                name: "SettingsJson_EN",
                table: "SiteModule");

            migrationBuilder.DropColumn(
                name: "SubTitle_EN",
                table: "SiteModule");

            migrationBuilder.DropColumn(
                name: "Title_EN",
                table: "SiteModule");

            migrationBuilder.DropColumn(
                name: "Description_EN",
                table: "SiteInfo");

            migrationBuilder.DropColumn(
                name: "Keywords_EN",
                table: "SiteInfo");

            migrationBuilder.DropColumn(
                name: "NavigationName_EN",
                table: "Navigation");

            migrationBuilder.DropColumn(
                name: "Detail_EN",
                table: "Job");

            migrationBuilder.DropColumn(
                name: "JobName_EN",
                table: "Job");

            migrationBuilder.DropColumn(
                name: "Address_EN",
                table: "FooterInfo");

            migrationBuilder.DropColumn(
                name: "CompanyInfo_EN",
                table: "FooterInfo");

            migrationBuilder.DropColumn(
                name: "Copyright_EN",
                table: "FooterInfo");

            migrationBuilder.DropColumn(
                name: "Description_EN",
                table: "Article");

            migrationBuilder.DropColumn(
                name: "Detail_EN",
                table: "Article");

            migrationBuilder.DropColumn(
                name: "Title_EN",
                table: "Article");

            migrationBuilder.DropColumn(
                name: "Description_EN",
                table: "Album");

            migrationBuilder.DropColumn(
                name: "Detail_EN",
                table: "Album");

            migrationBuilder.DropColumn(
                name: "Title_EN",
                table: "Album");
        }
    }
}
