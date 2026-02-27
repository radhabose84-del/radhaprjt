using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SalesManagement.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class MarketingOfficerMaster : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "DispatchAddressMapping",
                schema: "Sales",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    PartyId = table.Column<int>(type: "int", nullable: false),
                    DispatchAddressId = table.Column<int>(type: "int", nullable: false),
                    UsageTypeId = table.Column<int>(type: "int", nullable: false),
                    IsDefault = table.Column<bool>(type: "bit", nullable: false, defaultValue: false),
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
                    table.PrimaryKey("PK_DispatchAddressMapping", x => x.Id);
                    table.ForeignKey(
                        name: "FK_DispatchAddressMapping_DispatchAddressMaster_DispatchAddressId",
                        column: x => x.DispatchAddressId,
                        principalSchema: "Sales",
                        principalTable: "DispatchAddressMaster",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_DispatchAddressMapping_MiscMaster_UsageTypeId",
                        column: x => x.UsageTypeId,
                        principalSchema: "Sales",
                        principalTable: "MiscMaster",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "MarketingOfficer",
                schema: "Sales",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    EmployeeNo = table.Column<string>(type: "varchar(20)", nullable: false),
                    EmployeeName = table.Column<string>(type: "varchar(100)", nullable: false),
                    MobileNo = table.Column<string>(type: "varchar(15)", nullable: true),
                    Email = table.Column<string>(type: "varchar(200)", nullable: true),
                    Unit = table.Column<string>(type: "varchar(100)", nullable: false),
                    Department = table.Column<string>(type: "varchar(100)", nullable: false),
                    Designation = table.Column<string>(type: "varchar(100)", nullable: false),
                    SalesOfficeId = table.Column<int>(type: "int", nullable: false),
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
                    table.PrimaryKey("PK_MarketingOfficer", x => x.Id);
                    table.ForeignKey(
                        name: "FK_MarketingOfficer_SalesOffice_SalesOfficeId",
                        column: x => x.SalesOfficeId,
                        principalSchema: "Sales",
                        principalTable: "SalesOffice",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "OfficerSalesGroup",
                schema: "Sales",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    MarketingOfficerId = table.Column<int>(type: "int", nullable: false),
                    SalesGroupId = table.Column<int>(type: "int", nullable: false),
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
                    table.PrimaryKey("PK_OfficerSalesGroup", x => x.Id);
                    table.ForeignKey(
                        name: "FK_OfficerSalesGroup_MarketingOfficer_MarketingOfficerId",
                        column: x => x.MarketingOfficerId,
                        principalSchema: "Sales",
                        principalTable: "MarketingOfficer",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_OfficerSalesGroup_SalesGroup_SalesGroupId",
                        column: x => x.SalesGroupId,
                        principalSchema: "Sales",
                        principalTable: "SalesGroup",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_DispatchAddressMapping_DispatchAddressId",
                schema: "Sales",
                table: "DispatchAddressMapping",
                column: "DispatchAddressId");

            migrationBuilder.CreateIndex(
                name: "IX_DispatchAddressMapping_PartyId",
                schema: "Sales",
                table: "DispatchAddressMapping",
                column: "PartyId");

            migrationBuilder.CreateIndex(
                name: "IX_DispatchAddressMapping_UsageTypeId",
                schema: "Sales",
                table: "DispatchAddressMapping",
                column: "UsageTypeId");

            migrationBuilder.CreateIndex(
                name: "UIX_DispatchAddressMapping_Composite",
                schema: "Sales",
                table: "DispatchAddressMapping",
                columns: new[] { "PartyId", "DispatchAddressId", "UsageTypeId" },
                filter: "[IsDeleted] = 0");

            migrationBuilder.CreateIndex(
                name: "IX_MarketingOfficer_EmployeeNo",
                schema: "Sales",
                table: "MarketingOfficer",
                column: "EmployeeNo",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_MarketingOfficer_SalesOfficeId",
                schema: "Sales",
                table: "MarketingOfficer",
                column: "SalesOfficeId");

            migrationBuilder.CreateIndex(
                name: "IX_OfficerSalesGroup_MarketingOfficerId",
                schema: "Sales",
                table: "OfficerSalesGroup",
                column: "MarketingOfficerId");

            migrationBuilder.CreateIndex(
                name: "IX_OfficerSalesGroup_MarketingOfficerId_SalesGroupId",
                schema: "Sales",
                table: "OfficerSalesGroup",
                columns: new[] { "MarketingOfficerId", "SalesGroupId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_OfficerSalesGroup_SalesGroupId",
                schema: "Sales",
                table: "OfficerSalesGroup",
                column: "SalesGroupId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "DispatchAddressMapping",
                schema: "Sales");

            migrationBuilder.DropTable(
                name: "OfficerSalesGroup",
                schema: "Sales");

            migrationBuilder.DropTable(
                name: "MarketingOfficer",
                schema: "Sales");
        }
    }
}
