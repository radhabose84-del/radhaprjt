using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace FinanceManagement.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class ProfitCentreMaster : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "ProfitCentre",
                schema: "Finance",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    CompanyId = table.Column<int>(type: "int", nullable: false),
                    ProfitCentreCode = table.Column<string>(type: "varchar(20)", nullable: false),
                    ProfitCentreName = table.Column<string>(type: "varchar(150)", nullable: false),
                    LevelId = table.Column<int>(type: "int", nullable: false),
                    ParentProfitCentreId = table.Column<int>(type: "int", nullable: true),
                    ResponsibleHeadId = table.Column<int>(type: "int", nullable: true),
                    IsRevenueLinked = table.Column<bool>(type: "bit", nullable: false),
                    MidYearJustification = table.Column<string>(type: "varchar(500)", nullable: true),
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
                    table.PrimaryKey("PK_ProfitCentre", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ProfitCentre_MiscMaster_LevelId",
                        column: x => x.LevelId,
                        principalSchema: "Finance",
                        principalTable: "MiscMaster",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_ProfitCentre_ProfitCentre_ParentProfitCentreId",
                        column: x => x.ParentProfitCentreId,
                        principalSchema: "Finance",
                        principalTable: "ProfitCentre",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_ProfitCentre_CompanyId",
                schema: "Finance",
                table: "ProfitCentre",
                column: "CompanyId");

            migrationBuilder.CreateIndex(
                name: "IX_ProfitCentre_LevelId",
                schema: "Finance",
                table: "ProfitCentre",
                column: "LevelId");

            migrationBuilder.CreateIndex(
                name: "IX_ProfitCentre_ParentProfitCentreId",
                schema: "Finance",
                table: "ProfitCentre",
                column: "ParentProfitCentreId");

            migrationBuilder.CreateIndex(
                name: "IX_ProfitCentre_ProfitCentreCode",
                schema: "Finance",
                table: "ProfitCentre",
                column: "ProfitCentreCode",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ProfitCentre",
                schema: "Finance");
        }
    }
}
