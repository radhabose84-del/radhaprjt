using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PurchaseManagement.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class RFQ : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "RfqActivityLog",
                schema: "Purchase",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    CreatedDate = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
                    EntityName = table.Column<string>(type: "nvarchar(128)", maxLength: 128, nullable: false),
                    EntityId = table.Column<int>(type: "int", nullable: false),
                    Action = table.Column<string>(type: "nvarchar(32)", maxLength: 32, nullable: false, defaultValue: "Update"),
                    PropertyName = table.Column<string>(type: "nvarchar(128)", maxLength: 128, nullable: false),
                    OldValue = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true),
                    NewValue = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true),
                    CreatedBy = table.Column<int>(type: "int", nullable: true),
                    CreatedByName = table.Column<string>(type: "nvarchar(128)", maxLength: 128, nullable: true),
                    CreatedIP = table.Column<string>(type: "nvarchar(64)", maxLength: 64, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_RfqActivityLog", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "RfqMaster",
                schema: "Purchase",
                columns: table => new
                {
                    UnitId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    RfqCode = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: false),
                    RfqStatus = table.Column<int>(type: "int", nullable: false),
                    InitiationType = table.Column<int>(type: "int", nullable: false),
                    IndentId = table.Column<int>(type: "int", nullable: true),
                    SubmittedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    SentAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    Id = table.Column<int>(type: "int", nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false),
                    CreatedBy = table.Column<int>(type: "int", nullable: false),
                    CreatedDate = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true),
                    CreatedByName = table.Column<string>(type: "varchar(50)", nullable: true),
                    CreatedIP = table.Column<string>(type: "varchar(50)", nullable: true),
                    ModifiedBy = table.Column<int>(type: "int", nullable: true),
                    ModifiedDate = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true),
                    ModifiedByName = table.Column<string>(type: "varchar(50)", nullable: true),
                    ModifiedIP = table.Column<string>(type: "varchar(50)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_RfqMaster", x => x.UnitId);
                });

            migrationBuilder.CreateTable(
                name: "RfqItem",
                schema: "Purchase",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    RfqId = table.Column<int>(type: "int", nullable: false),
                    ItemId = table.Column<int>(type: "int", nullable: false),
                    Quantity = table.Column<decimal>(type: "decimal(18,3)", precision: 18, scale: 3, nullable: false),
                    UomId = table.Column<int>(type: "int", nullable: false),
                    RequiredDate = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_RfqItem", x => x.Id);
                    table.ForeignKey(
                        name: "FK_RfqItem_RfqMaster_RfqId",
                        column: x => x.RfqId,
                        principalSchema: "Purchase",
                        principalTable: "RfqMaster",
                        principalColumn: "UnitId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "RfqSuppliers",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    RfqId = table.Column<int>(type: "int", nullable: false),
                    SupplierId = table.Column<int>(type: "int", nullable: true),
                    Name = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    Email = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: true),
                    Mobile = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Gst = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_RfqSuppliers", x => x.Id);
                    table.ForeignKey(
                        name: "FK_RfqSuppliers_RfqMaster_RfqId",
                        column: x => x.RfqId,
                        principalSchema: "Purchase",
                        principalTable: "RfqMaster",
                        principalColumn: "UnitId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_RfqActivityLog_EntityName_EntityId_CreatedDate",
                schema: "Purchase",
                table: "RfqActivityLog",
                columns: new[] { "EntityName", "EntityId", "CreatedDate" });

            migrationBuilder.CreateIndex(
                name: "IX_RfqItem_RfqId_ItemId_UomId_RequiredDate",
                schema: "Purchase",
                table: "RfqItem",
                columns: new[] { "RfqId", "ItemId", "UomId", "RequiredDate" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_RfqMaster_RfqCode",
                schema: "Purchase",
                table: "RfqMaster",
                column: "RfqCode",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_RfqSuppliers_RfqId",
                table: "RfqSuppliers",
                column: "RfqId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "RfqActivityLog",
                schema: "Purchase");

            migrationBuilder.DropTable(
                name: "RfqItem",
                schema: "Purchase");

            migrationBuilder.DropTable(
                name: "RfqSuppliers");

            migrationBuilder.DropTable(
                name: "RfqMaster",
                schema: "Purchase");
        }
    }
}
