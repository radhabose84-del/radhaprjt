using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PurchaseManagement.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class PoItemSno : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "ItemSno",
                schema: "Purchase",
                table: "PurchaseLocalDetail",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateTable(
                name: "GrnHeader",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    GrnNo = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    GrnDate = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
                    GateEntryId = table.Column<int>(type: "int", nullable: false),
                    GrnHeaderDetailsId = table.Column<int>(type: "int", nullable: false),
                    PartyId = table.Column<int>(type: "int", nullable: false),
                    InvoiceNo = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    InvoiceDate = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
                    DcNo = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    DcDate = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true),
                    ReceivingWarehouseId = table.Column<int>(type: "int", nullable: false),
                    Remarks = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    IsGrnGenerated = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_GrnHeader", x => x.Id);
                    table.ForeignKey(
                        name: "FK_GrnHeader_GateEntryHeader_GrnHeaderDetailsId",
                        column: x => x.GrnHeaderDetailsId,
                        principalSchema: "Purchase",
                        principalTable: "GateEntryHeader",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_GrnHeader_GrnHeaderDetailsId",
                table: "GrnHeader",
                column: "GrnHeaderDetailsId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "GrnHeader");

            migrationBuilder.DropColumn(
                name: "ItemSno",
                schema: "Purchase",
                table: "PurchaseLocalDetail");
        }
    }
}
