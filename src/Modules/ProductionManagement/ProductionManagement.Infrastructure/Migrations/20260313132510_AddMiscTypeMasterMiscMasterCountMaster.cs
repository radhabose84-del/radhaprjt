using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ProductionManagement.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddMiscTypeMasterMiscMasterCountMaster : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.EnsureSchema(
                name: "Production");

            migrationBuilder.CreateTable(
                name: "MiscTypeMaster",
                schema: "Production",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    MiscTypeCode = table.Column<string>(type: "varchar(50)", nullable: true),
                    Description = table.Column<string>(type: "varchar(200)", nullable: true),
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
                    table.PrimaryKey("PK_MiscTypeMaster", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "MiscMaster",
                schema: "Production",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    MiscTypeId = table.Column<int>(type: "int", nullable: false),
                    Code = table.Column<string>(type: "varchar(100)", nullable: true),
                    Description = table.Column<string>(type: "varchar(200)", nullable: true),
                    SortOrder = table.Column<int>(type: "int", nullable: false),
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
                    table.PrimaryKey("PK_MiscMaster", x => x.Id);
                    table.ForeignKey(
                        name: "FK_MiscMaster_MiscTypeMaster_MiscTypeId",
                        column: x => x.MiscTypeId,
                        principalSchema: "Production",
                        principalTable: "MiscTypeMaster",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "CountMaster",
                schema: "Production",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    CountCode = table.Column<string>(type: "varchar(10)", nullable: false),
                    CountValue = table.Column<decimal>(type: "decimal(10,2)", nullable: false),
                    ShortName = table.Column<string>(type: "varchar(50)", nullable: true),
                    CountCategoryId = table.Column<int>(type: "int", nullable: true),
                    CountTypeId = table.Column<int>(type: "int", nullable: false),
                    CountDescription = table.Column<string>(type: "varchar(250)", nullable: false),
                    UOMId = table.Column<int>(type: "int", nullable: false),
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
                    table.PrimaryKey("PK_CountMaster", x => x.Id);
                    table.ForeignKey(
                        name: "FK_CountMaster_MiscMaster_CountCategoryId",
                        column: x => x.CountCategoryId,
                        principalSchema: "Production",
                        principalTable: "MiscMaster",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_CountMaster_MiscMaster_CountTypeId",
                        column: x => x.CountTypeId,
                        principalSchema: "Production",
                        principalTable: "MiscMaster",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_CountMaster_CountCategoryId",
                schema: "Production",
                table: "CountMaster",
                column: "CountCategoryId");

            migrationBuilder.CreateIndex(
                name: "IX_CountMaster_CountCode",
                schema: "Production",
                table: "CountMaster",
                column: "CountCode",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_CountMaster_CountTypeId",
                schema: "Production",
                table: "CountMaster",
                column: "CountTypeId");

            migrationBuilder.CreateIndex(
                name: "IX_CountMaster_UOMId",
                schema: "Production",
                table: "CountMaster",
                column: "UOMId");

            migrationBuilder.CreateIndex(
                name: "IX_MiscMaster_MiscTypeId",
                schema: "Production",
                table: "MiscMaster",
                column: "MiscTypeId");

            migrationBuilder.CreateIndex(
                name: "IX_MiscMaster_MiscTypeId_Code",
                schema: "Production",
                table: "MiscMaster",
                columns: new[] { "MiscTypeId", "Code" },
                unique: true,
                filter: "[Code] IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_MiscTypeMaster_MiscTypeCode",
                schema: "Production",
                table: "MiscTypeMaster",
                column: "MiscTypeCode",
                unique: true,
                filter: "[MiscTypeCode] IS NOT NULL");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "CountMaster",
                schema: "Production");

            migrationBuilder.DropTable(
                name: "MiscMaster",
                schema: "Production");

            migrationBuilder.DropTable(
                name: "MiscTypeMaster",
                schema: "Production");
        }
    }
}
