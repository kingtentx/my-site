using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CIMC.EntityFramework.Migrations
{
    /// <inheritdoc />
    public partial class FixAlbumPartnerTagAndRemoveDuplicates : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // 1. 将被错误归到传统业务线标签(Id=10)的合作客户数据修正为合作伙伴标签(Id=21)
            migrationBuilder.Sql("UPDATE Album SET TagId = 21 WHERE Title = '合作客户' AND TagId = 10 AND IsDelete = 0;");

            // 2. 将被错误归到其他产品标签的合作客户数据修正为合作伙伴标签(Id=21)
            migrationBuilder.Sql("UPDATE Album SET TagId = 21 WHERE Title = '合作客户' AND TagId IN (11, 12, 20) AND IsDelete = 0;");

            // 3. 软删除重复的合作客户数据（相同 ImageUrl 保留 Id 最小的一条）
            migrationBuilder.Sql(@"
                DELETE a FROM Album a
                INNER JOIN Album b
                  ON a.Title = '合作客户' AND b.Title = '合作客户'
                  AND a.ImageUrl = b.ImageUrl
                  AND a.Id > b.Id
                  AND a.IsDelete = 0 AND b.IsDelete = 0;
            ");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {

        }
    }
}
