using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace QCManagement.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddQCQualityParameter : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "QualityParameter",
                schema: "QC",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ParameterCode = table.Column<string>(type: "varchar(20)", nullable: false),
                    ParameterName = table.Column<string>(type: "varchar(100)", nullable: false),
                    ParameterGroupId = table.Column<int>(type: "int", nullable: false),
                    DataTypeId = table.Column<int>(type: "int", nullable: false),
                    UnitId = table.Column<int>(type: "int", nullable: true),
                    ValidationTypeId = table.Column<int>(type: "int", nullable: false),
                    Description = table.Column<string>(type: "varchar(500)", nullable: true),
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
                    table.PrimaryKey("PK_QualityParameter", x => x.Id);
                    table.ForeignKey(
                        name: "FK_QualityParameter_MiscMaster_DataTypeId",
                        column: x => x.DataTypeId,
                        principalSchema: "QC",
                        principalTable: "MiscMaster",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_QualityParameter_MiscMaster_ParameterGroupId",
                        column: x => x.ParameterGroupId,
                        principalSchema: "QC",
                        principalTable: "MiscMaster",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_QualityParameter_MiscMaster_ValidationTypeId",
                        column: x => x.ValidationTypeId,
                        principalSchema: "QC",
                        principalTable: "MiscMaster",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_QualityParameter_DataTypeId",
                schema: "QC",
                table: "QualityParameter",
                column: "DataTypeId");

            migrationBuilder.CreateIndex(
                name: "IX_QualityParameter_ParameterCode",
                schema: "QC",
                table: "QualityParameter",
                column: "ParameterCode",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_QualityParameter_ParameterGroupId",
                schema: "QC",
                table: "QualityParameter",
                column: "ParameterGroupId");

            migrationBuilder.CreateIndex(
                name: "IX_QualityParameter_ParameterName",
                schema: "QC",
                table: "QualityParameter",
                column: "ParameterName");

            migrationBuilder.CreateIndex(
                name: "IX_QualityParameter_UnitId",
                schema: "QC",
                table: "QualityParameter",
                column: "UnitId");

            migrationBuilder.CreateIndex(
                name: "IX_QualityParameter_ValidationTypeId",
                schema: "QC",
                table: "QualityParameter",
                column: "ValidationTypeId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "QualityParameter",
                schema: "QC");
        }
    }
}
