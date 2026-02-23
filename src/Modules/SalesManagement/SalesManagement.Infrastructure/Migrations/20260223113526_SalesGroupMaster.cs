using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SalesManagement.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class SalesGroupMaster : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "SalesGroup",
                schema: "Sales",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    SalesGroupName = table.Column<string>(type: "varchar(100)", nullable: false),
                    SalesOfficeId = table.Column<int>(type: "int", nullable: false),
                    ResponsibleManager = table.Column<string>(type: "varchar(100)", nullable: true),
                    ProductCategoryId = table.Column<int>(type: "int", nullable: true),
                    RegionTerritory = table.Column<string>(type: "varchar(100)", nullable: true),
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
                    table.PrimaryKey("PK_SalesGroup", x => x.Id);
                    table.ForeignKey(
                        name: "FK_SalesGroup_SalesOffice_SalesOfficeId",
                        column: x => x.SalesOfficeId,
                        principalSchema: "Sales",
                        principalTable: "SalesOffice",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_SalesGroup_ProductCategoryId",
                schema: "Sales",
                table: "SalesGroup",
                column: "ProductCategoryId");

            migrationBuilder.CreateIndex(
                name: "IX_SalesGroup_SalesOfficeId",
                schema: "Sales",
                table: "SalesGroup",
                column: "SalesOfficeId");

            migrationBuilder.CreateIndex(
                name: "IX_SalesGroup_SalesOfficeId_SalesGroupName",
                schema: "Sales",
                table: "SalesGroup",
                columns: new[] { "SalesOfficeId", "SalesGroupName" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "SalesGroup",
                schema: "Sales");
        }
    }
}
