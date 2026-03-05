using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SalesManagement.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class StoTypeMaster : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "StoTypeMaster",
                schema: "Sales",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    StoTypeCode = table.Column<string>(type: "varchar(10)", nullable: false),
                    StoTypeName = table.Column<string>(type: "varchar(100)", nullable: false),
                    Description = table.Column<string>(type: "varchar(250)", nullable: true),
                    PgiMovementTypeId = table.Column<int>(type: "int", nullable: false),
                    GrMovementTypeId = table.Column<int>(type: "int", nullable: false),
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
                    table.PrimaryKey("PK_StoTypeMaster", x => x.Id);
                    table.ForeignKey(
                        name: "FK_StoTypeMaster_MovementTypeConfig_GrMovementTypeId",
                        column: x => x.GrMovementTypeId,
                        principalSchema: "Sales",
                        principalTable: "MovementTypeConfig",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_StoTypeMaster_MovementTypeConfig_PgiMovementTypeId",
                        column: x => x.PgiMovementTypeId,
                        principalSchema: "Sales",
                        principalTable: "MovementTypeConfig",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_StoTypeMaster_GrMovementTypeId",
                schema: "Sales",
                table: "StoTypeMaster",
                column: "GrMovementTypeId");

            migrationBuilder.CreateIndex(
                name: "IX_StoTypeMaster_PgiMovementTypeId",
                schema: "Sales",
                table: "StoTypeMaster",
                column: "PgiMovementTypeId");

            migrationBuilder.CreateIndex(
                name: "IX_StoTypeMaster_StoTypeCode",
                schema: "Sales",
                table: "StoTypeMaster",
                column: "StoTypeCode",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "StoTypeMaster",
                schema: "Sales");
        }
    }
}
