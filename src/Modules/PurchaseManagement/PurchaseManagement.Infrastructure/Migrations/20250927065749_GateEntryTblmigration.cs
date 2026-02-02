using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PurchaseManagement.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class GateEntryTblmigration : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "GateEntryHeader",
                schema: "Purchase",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    GateEntryNo = table.Column<string>(type: "nvarchar(100)", nullable: false),
                    GateEntryDate = table.Column<DateTimeOffset>(type: "DatetimeOffset", nullable: false),
                    UnitId = table.Column<int>(type: "int", nullable: false),
                    PoTypeId = table.Column<int>(type: "int", nullable: false),
                    PartyId = table.Column<int>(type: "int", nullable: false),
                    VehicleNumber = table.Column<string>(type: "nvarchar(100)", nullable: false),
                    DriverName = table.Column<string>(type: "nvarchar(100)", nullable: true),
                    GrossWeight = table.Column<decimal>(type: "decimal(18,3)", nullable: true, defaultValue: 0.000m),
                    TareWeight = table.Column<decimal>(type: "decimal(18,3)", nullable: true, defaultValue: 0.000m),
                    NetWeight = table.Column<decimal>(type: "decimal(18,3)", nullable: true, defaultValue: 0.000m),
                    ImagePath = table.Column<string>(type: "nvarchar(200)", nullable: true),
                    Remarks = table.Column<string>(type: "nvarchar(250)", nullable: true),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false),
                    CreatedBy = table.Column<int>(type: "int", nullable: false),
                    CreatedDate = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true),
                    CreatedByName = table.Column<string>(type: "varchar(50)", nullable: false),
                    CreatedIP = table.Column<string>(type: "varchar(20)", nullable: false),
                    ModifiedBy = table.Column<int>(type: "int", nullable: true),
                    ModifiedDate = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true),
                    ModifiedByName = table.Column<string>(type: "varchar(50)", nullable: true),
                    ModifiedIP = table.Column<string>(type: "varchar(20)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_GateEntryHeader", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "GateEntryDetail",
                schema: "Purchase",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    GateEntryHeaderId = table.Column<int>(type: "int", nullable: false),
                    PoId = table.Column<int>(type: "int", nullable: false),
                    PODate = table.Column<DateTimeOffset>(type: "DatetimeOffset", nullable: false),
                    POCreatedBy = table.Column<string>(type: "varchar(100)", nullable: false),
                    GSTNumber = table.Column<string>(type: "nvarchar(100)", nullable: false),
                    ContactDetails = table.Column<string>(type: "nvarchar(100)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_GateEntryDetail", x => x.Id);
                    table.ForeignKey(
                        name: "FK_GateEntryDetail_GateEntryHeader_GateEntryHeaderId",
                        column: x => x.GateEntryHeaderId,
                        principalSchema: "Purchase",
                        principalTable: "GateEntryHeader",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_GateEntryDetail_GateEntryHeaderId",
                schema: "Purchase",
                table: "GateEntryDetail",
                column: "GateEntryHeaderId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "GateEntryDetail",
                schema: "Purchase");

            migrationBuilder.DropTable(
                name: "GateEntryHeader",
                schema: "Purchase");
        }
    }
}
