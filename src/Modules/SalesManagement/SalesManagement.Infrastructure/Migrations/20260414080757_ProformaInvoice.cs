using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SalesManagement.Infrastructure.Data.Migrations
{
    /// <inheritdoc />
    public partial class ProformaInvoice : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "ProformaInvoice",
                schema: "Sales",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ProformaNumber = table.Column<string>(type: "varchar(30)", nullable: false),
                    ProformaDate = table.Column<DateOnly>(type: "date", nullable: false),
                    SalesOrderId = table.Column<int>(type: "int", nullable: false),
                    PartyId = table.Column<int>(type: "int", nullable: false),
                    ProformaAmount = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 6, nullable: false),
                    SOBalance = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 6, nullable: false),
                    PaymentReceivedAmount = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 6, nullable: false, defaultValue: 0m),
                    StatusId = table.Column<int>(type: "int", nullable: true),
                    Remarks = table.Column<string>(type: "varchar(500)", nullable: true),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false),
                    CreatedBy = table.Column<int>(type: "int", nullable: false),
                    CreatedDate = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true),
                    CreatedByName = table.Column<string>(type: "varchar(100)", nullable: true),
                    CreatedIP = table.Column<string>(type: "varchar(50)", nullable: true),
                    ModifiedBy = table.Column<int>(type: "int", nullable: true),
                    ModifiedDate = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true),
                    ModifiedByName = table.Column<string>(type: "varchar(100)", nullable: true),
                    ModifiedIP = table.Column<string>(type: "varchar(50)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ProformaInvoice", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ProformaInvoice_MiscMaster_StatusId",
                        column: x => x.StatusId,
                        principalSchema: "Sales",
                        principalTable: "MiscMaster",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_ProformaInvoice_SalesOrderHeader_SalesOrderId",
                        column: x => x.SalesOrderId,
                        principalSchema: "Sales",
                        principalTable: "SalesOrderHeader",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_ProformaInvoice_PartyId",
                schema: "Sales",
                table: "ProformaInvoice",
                column: "PartyId");

            migrationBuilder.CreateIndex(
                name: "IX_ProformaInvoice_ProformaDate",
                schema: "Sales",
                table: "ProformaInvoice",
                column: "ProformaDate");

            migrationBuilder.CreateIndex(
                name: "IX_ProformaInvoice_ProformaNumber",
                schema: "Sales",
                table: "ProformaInvoice",
                column: "ProformaNumber",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_ProformaInvoice_SalesOrderId",
                schema: "Sales",
                table: "ProformaInvoice",
                column: "SalesOrderId");

            migrationBuilder.CreateIndex(
                name: "IX_ProformaInvoice_StatusId",
                schema: "Sales",
                table: "ProformaInvoice",
                column: "StatusId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ProformaInvoice",
                schema: "Sales");
        }
    }
}
