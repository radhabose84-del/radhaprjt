using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SalesManagement.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class MovementTypeConfigMaster : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "MovementTypeConfig",
                schema: "Sales",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    MovementCode = table.Column<string>(type: "varchar(4)", nullable: false),
                    MovementDescription = table.Column<string>(type: "varchar(100)", nullable: false),
                    MovementCategoryId = table.Column<int>(type: "int", nullable: false),
                    FromStockTypeId = table.Column<int>(type: "int", nullable: false),
                    ToStockTypeId = table.Column<int>(type: "int", nullable: false),
                    QuantityUpdateFlag = table.Column<bool>(type: "bit", nullable: false, defaultValue: true),
                    ValueUpdateFlag = table.Column<bool>(type: "bit", nullable: false, defaultValue: false),
                    AccountModifier = table.Column<string>(type: "varchar(50)", nullable: true),
                    BatchRequiredFlag = table.Column<bool>(type: "bit", nullable: false, defaultValue: false),
                    NegativeStockAllowed = table.Column<bool>(type: "bit", nullable: false, defaultValue: false),
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
                    table.PrimaryKey("PK_MovementTypeConfig", x => x.Id);
                    table.ForeignKey(
                        name: "FK_MovementTypeConfig_MiscMaster_FromStockTypeId",
                        column: x => x.FromStockTypeId,
                        principalSchema: "Sales",
                        principalTable: "MiscMaster",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_MovementTypeConfig_MiscMaster_MovementCategoryId",
                        column: x => x.MovementCategoryId,
                        principalSchema: "Sales",
                        principalTable: "MiscMaster",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_MovementTypeConfig_MiscMaster_ToStockTypeId",
                        column: x => x.ToStockTypeId,
                        principalSchema: "Sales",
                        principalTable: "MiscMaster",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_MovementTypeConfig_FromStockTypeId",
                schema: "Sales",
                table: "MovementTypeConfig",
                column: "FromStockTypeId");

            migrationBuilder.CreateIndex(
                name: "IX_MovementTypeConfig_MovementCategoryId",
                schema: "Sales",
                table: "MovementTypeConfig",
                column: "MovementCategoryId");

            migrationBuilder.CreateIndex(
                name: "IX_MovementTypeConfig_MovementCode",
                schema: "Sales",
                table: "MovementTypeConfig",
                column: "MovementCode",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_MovementTypeConfig_ToStockTypeId",
                schema: "Sales",
                table: "MovementTypeConfig",
                column: "ToStockTypeId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "MovementTypeConfig",
                schema: "Sales");
        }
    }
}
