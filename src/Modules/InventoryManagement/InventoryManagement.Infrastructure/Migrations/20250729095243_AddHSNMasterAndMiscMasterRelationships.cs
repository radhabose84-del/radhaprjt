using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace InventoryManagement.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddHSNMasterAndMiscMasterRelationships : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
           
            migrationBuilder.CreateTable(
                name: "HSNMaster",
                schema: "Inventory",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Type = table.Column<int>(type: "int", nullable: false),
                    HSNCode = table.Column<string>(type: "varchar(10)", nullable: false),
                    Description = table.Column<string>(type: "varchar(250)", nullable: false),
                    GSTCategoryId = table.Column<int>(type: "int", nullable: false),
                    GstPercentage = table.Column<decimal>(type: "decimal(5,2)", nullable: false),
                    CgstPercentage = table.Column<decimal>(type: "decimal(5,2)", nullable: false),
                    SgstPercentage = table.Column<decimal>(type: "decimal(5,2)", nullable: false),
                    IgstPercentage = table.Column<decimal>(type: "decimal(5,2)", nullable: false),
                    ValidFrom = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
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
                    table.PrimaryKey("PK_HSNMaster", x => x.Id);
                    table.ForeignKey(
                        name: "FK_HSNMaster_MiscMaster_GSTCategoryId",
                        column: x => x.GSTCategoryId,
                        principalSchema: "Inventory",
                        principalTable: "MiscMaster",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });
           
            migrationBuilder.CreateIndex(
                name: "IX_HSNMaster_GSTCategoryId",
                schema: "Inventory",
                table: "HSNMaster",
                column: "GSTCategoryId");

            migrationBuilder.CreateIndex(
                name: "IX_HSNMaster_HSNCode",
                schema: "Inventory",
                table: "HSNMaster",
                column: "HSNCode",
                unique: true);

           
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
           
            migrationBuilder.DropTable(
                name: "HSNMaster",
                schema: "Inventory");        

           

            
        }
    }
}
