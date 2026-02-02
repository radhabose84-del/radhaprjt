using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PurchaseManagement.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class putedColumnSqlinservicemaster : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<string>(
                name: "ServiceCode",
                schema: "Purchase",
                table: "ServiceMaster",
                type: "nvarchar(50)",
                maxLength: 50,
                nullable: false,
                computedColumnSql: "('SRV' + RIGHT(REPLICATE('0',4) + CONVERT(varchar(10), [Id]), 4))",
                stored: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(50)");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<string>(
                name: "ServiceCode",
                schema: "Purchase",
                table: "ServiceMaster",
                type: "nvarchar(50)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(50)",
                oldMaxLength: 50,
                oldComputedColumnSql: "('SRV' + RIGHT(REPLICATE('0',4) + CONVERT(varchar(10), [Id]), 4))");
        }
    }
}
