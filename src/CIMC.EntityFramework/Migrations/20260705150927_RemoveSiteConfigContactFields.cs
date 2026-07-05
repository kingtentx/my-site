using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CIMC.EntityFramework.Migrations
{
    /// <inheritdoc />
    public partial class RemoveSiteConfigContactFields : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Address",
                table: "WebsiteSiteConfig");

            migrationBuilder.DropColumn(
                name: "Copyright",
                table: "WebsiteSiteConfig");

            migrationBuilder.DropColumn(
                name: "Email",
                table: "WebsiteSiteConfig");

            migrationBuilder.DropColumn(
                name: "IcpNo",
                table: "WebsiteSiteConfig");

            migrationBuilder.DropColumn(
                name: "Phone",
                table: "WebsiteSiteConfig");

            migrationBuilder.DropColumn(
                name: "PoliceNo",
                table: "WebsiteSiteConfig");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Address",
                table: "WebsiteSiteConfig",
                type: "varchar(250)",
                maxLength: 250,
                nullable: true)
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AddColumn<string>(
                name: "Copyright",
                table: "WebsiteSiteConfig",
                type: "varchar(250)",
                maxLength: 250,
                nullable: true)
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AddColumn<string>(
                name: "Email",
                table: "WebsiteSiteConfig",
                type: "varchar(100)",
                maxLength: 100,
                nullable: true)
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AddColumn<string>(
                name: "IcpNo",
                table: "WebsiteSiteConfig",
                type: "varchar(100)",
                maxLength: 100,
                nullable: true)
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AddColumn<string>(
                name: "Phone",
                table: "WebsiteSiteConfig",
                type: "varchar(50)",
                maxLength: 50,
                nullable: true)
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AddColumn<string>(
                name: "PoliceNo",
                table: "WebsiteSiteConfig",
                type: "varchar(100)",
                maxLength: 100,
                nullable: true)
                .Annotation("MySql:CharSet", "utf8mb4");
        }
    }
}
