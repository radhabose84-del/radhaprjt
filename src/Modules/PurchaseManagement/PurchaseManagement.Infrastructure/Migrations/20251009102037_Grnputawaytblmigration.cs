using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PurchaseManagement.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class Grnputawaytblmigration : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "GrnPutAwayRule",
                schema: "Purchase",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    PutAwayDate = table.Column<DateTimeOffset>(type: "DatetimeOffset", nullable: false),
                    GrnDetailId = table.Column<int>(type: "int", nullable: false),
                    UnitId = table.Column<int>(type: "int", nullable: false),
                    QcAcceptedQuantity = table.Column<decimal>(type: "decimal(18,3)", nullable: false, defaultValue: 0.000m),
                    GrnId = table.Column<int>(type: "int", nullable: false),
                    PoId = table.Column<int>(type: "int", nullable: false),
                    PoSlNoLocal = table.Column<int>(type: "int", nullable: false),
                    ItemId = table.Column<int>(type: "int", nullable: false),
                    WarehouseId = table.Column<int>(type: "int", nullable: false),
                    StorageTypeId = table.Column<int>(type: "int", nullable: false),
                    TargetId = table.Column<int>(type: "int", nullable: false),
                    PriorityId = table.Column<int>(type: "int", nullable: false),
                    Override = table.Column<bool>(type: "bit", nullable: false),
                    CreatedBy = table.Column<int>(type: "int", nullable: false),
                    CreatedDate = table.Column<DateTimeOffset>(type: "DatetimeOffset", nullable: false),
                    CreatedByName = table.Column<string>(type: "varchar(50)", nullable: false),
                    CreatedIP = table.Column<string>(type: "varchar(20)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_GrnPutAwayRule", x => x.Id);
                    table.ForeignKey(
                        name: "FK_GrnPutAwayRule_GrnDetail_GrnDetailId",
                        column: x => x.GrnDetailId,
                        principalSchema: "Purchase",
                        principalTable: "GrnDetail",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_GrnPutAwayRule_GrnDetailId",
                schema: "Purchase",
                table: "GrnPutAwayRule",
                column: "GrnDetailId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "GrnPutAwayRule",
                schema: "Purchase");
        }
    }
}
