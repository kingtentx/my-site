using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CIMC.EntityFramework.Migrations
{
    /// <inheritdoc />
    public partial class RenameAlbumToProduct : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameTable(name: "Album", newName: "Product");
            migrationBuilder.RenameIndex(
                name: "IX_Album_TagType_TagId_Sort",
                table: "Product",
                newName: "IX_Product_TagType_TagId_Sort");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameTable(name: "Product", newName: "Album");
            migrationBuilder.RenameIndex(
                name: "IX_Product_TagType_TagId_Sort",
                table: "Album",
                newName: "IX_Album_TagType_TagId_Sort");
        }
    }
}
