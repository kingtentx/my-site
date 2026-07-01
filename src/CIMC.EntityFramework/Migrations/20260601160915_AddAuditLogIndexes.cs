using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CIMC.EntityFramework.Migrations
{
    /// <inheritdoc />
    public partial class AddAuditLogIndexes : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateIndex(
                name: "IX_AuditLog_OperationModule",
                table: "AuditLog",
                column: "OperationModule");

            migrationBuilder.CreateIndex(
                name: "IX_AuditLog_OperationTime",
                table: "AuditLog",
                column: "OperationTime");

            migrationBuilder.CreateIndex(
                name: "IX_AuditLog_OperationTime_OperationType",
                table: "AuditLog",
                columns: new[] { "OperationTime", "OperationType" });

            migrationBuilder.CreateIndex(
                name: "IX_AuditLog_OperationType",
                table: "AuditLog",
                column: "OperationType");

            migrationBuilder.CreateIndex(
                name: "IX_AuditLog_UserId",
                table: "AuditLog",
                column: "UserId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_AuditLog_OperationModule",
                table: "AuditLog");

            migrationBuilder.DropIndex(
                name: "IX_AuditLog_OperationTime",
                table: "AuditLog");

            migrationBuilder.DropIndex(
                name: "IX_AuditLog_OperationTime_OperationType",
                table: "AuditLog");

            migrationBuilder.DropIndex(
                name: "IX_AuditLog_OperationType",
                table: "AuditLog");

            migrationBuilder.DropIndex(
                name: "IX_AuditLog_UserId",
                table: "AuditLog");
        }
    }
}
