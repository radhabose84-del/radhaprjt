using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace FinanceManagement.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class ScheduleIIIHeaderDetailSplit : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ScheduleIIIMaster",
                schema: "Finance");

            migrationBuilder.CreateTable(
                name: "ScheduleIIIHeader",
                schema: "Finance",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    CompanyId = table.Column<int>(type: "int", nullable: false),
                    DivisionId = table.Column<int>(type: "int", nullable: false),
                    StatusId = table.Column<int>(type: "int", nullable: false),
                    TextileSplitEnabled = table.Column<bool>(type: "bit", nullable: false, defaultValue: false),
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
                    table.PrimaryKey("PK_ScheduleIIIHeader", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ScheduleIIIHeader_MiscMaster_StatusId",
                        column: x => x.StatusId,
                        principalSchema: "Finance",
                        principalTable: "MiscMaster",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "ScheduleIIIDetail",
                schema: "Finance",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ScheduleIIIHeaderId = table.Column<int>(type: "int", nullable: false),
                    ScheduleIIISectionId = table.Column<int>(type: "int", nullable: false),
                    ScheduleIIISectionItemId = table.Column<int>(type: "int", nullable: false),
                    DisplayOrder = table.Column<int>(type: "int", nullable: false, defaultValue: 0),
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
                    table.PrimaryKey("PK_ScheduleIIIDetail", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ScheduleIIIDetail_ScheduleIIIHeader_ScheduleIIIHeaderId",
                        column: x => x.ScheduleIIIHeaderId,
                        principalSchema: "Finance",
                        principalTable: "ScheduleIIIHeader",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_ScheduleIIIDetail_ScheduleIIISectionItem_ScheduleIIISectionItemId",
                        column: x => x.ScheduleIIISectionItemId,
                        principalSchema: "Finance",
                        principalTable: "ScheduleIIISectionItem",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_ScheduleIIIDetail_ScheduleIIISection_ScheduleIIISectionId",
                        column: x => x.ScheduleIIISectionId,
                        principalSchema: "Finance",
                        principalTable: "ScheduleIIISection",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_ScheduleIIIDetail_ScheduleIIIHeaderId_DisplayOrder",
                schema: "Finance",
                table: "ScheduleIIIDetail",
                columns: new[] { "ScheduleIIIHeaderId", "DisplayOrder" },
                unique: true,
                filter: "[IsDeleted] = 0");

            migrationBuilder.CreateIndex(
                name: "IX_ScheduleIIIDetail_ScheduleIIIHeaderId_ScheduleIIISectionItemId",
                schema: "Finance",
                table: "ScheduleIIIDetail",
                columns: new[] { "ScheduleIIIHeaderId", "ScheduleIIISectionItemId" },
                unique: true,
                filter: "[IsDeleted] = 0");

            migrationBuilder.CreateIndex(
                name: "IX_ScheduleIIIDetail_ScheduleIIISectionId",
                schema: "Finance",
                table: "ScheduleIIIDetail",
                column: "ScheduleIIISectionId");

            migrationBuilder.CreateIndex(
                name: "IX_ScheduleIIIDetail_ScheduleIIISectionItemId",
                schema: "Finance",
                table: "ScheduleIIIDetail",
                column: "ScheduleIIISectionItemId");

            migrationBuilder.CreateIndex(
                name: "IX_ScheduleIIIHeader_CompanyId_DivisionId",
                schema: "Finance",
                table: "ScheduleIIIHeader",
                columns: new[] { "CompanyId", "DivisionId" },
                unique: true,
                filter: "[IsDeleted] = 0");

            migrationBuilder.CreateIndex(
                name: "IX_ScheduleIIIHeader_StatusId",
                schema: "Finance",
                table: "ScheduleIIIHeader",
                column: "StatusId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ScheduleIIIDetail",
                schema: "Finance");

            migrationBuilder.DropTable(
                name: "ScheduleIIIHeader",
                schema: "Finance");

            migrationBuilder.CreateTable(
                name: "ScheduleIIIMaster",
                schema: "Finance",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ScheduleIIISectionItemId = table.Column<int>(type: "int", nullable: false),
                    StatusId = table.Column<int>(type: "int", nullable: false),
                    CompanyId = table.Column<int>(type: "int", nullable: false),
                    CreatedBy = table.Column<int>(type: "int", nullable: false),
                    CreatedByName = table.Column<string>(type: "varchar(100)", nullable: true),
                    CreatedDate = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true),
                    CreatedIP = table.Column<string>(type: "varchar(50)", nullable: true),
                    DisplayOrder = table.Column<int>(type: "int", nullable: false, defaultValue: 0),
                    DivisionId = table.Column<int>(type: "int", nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false),
                    ModifiedBy = table.Column<int>(type: "int", nullable: true),
                    ModifiedByName = table.Column<string>(type: "varchar(100)", nullable: true),
                    ModifiedDate = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true),
                    ModifiedIP = table.Column<string>(type: "varchar(50)", nullable: true),
                    TextileSplitEnabled = table.Column<bool>(type: "bit", nullable: false, defaultValue: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ScheduleIIIMaster", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ScheduleIIIMaster_MiscMaster_StatusId",
                        column: x => x.StatusId,
                        principalSchema: "Finance",
                        principalTable: "MiscMaster",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_ScheduleIIIMaster_ScheduleIIISectionItem_ScheduleIIISectionItemId",
                        column: x => x.ScheduleIIISectionItemId,
                        principalSchema: "Finance",
                        principalTable: "ScheduleIIISectionItem",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_ScheduleIIIMaster_CompanyId",
                schema: "Finance",
                table: "ScheduleIIIMaster",
                column: "CompanyId");

            migrationBuilder.CreateIndex(
                name: "IX_ScheduleIIIMaster_CompanyId_DivisionId_ScheduleIIISectionItemId",
                schema: "Finance",
                table: "ScheduleIIIMaster",
                columns: new[] { "CompanyId", "DivisionId", "ScheduleIIISectionItemId" },
                unique: true,
                filter: "[IsDeleted] = 0");

            migrationBuilder.CreateIndex(
                name: "IX_ScheduleIIIMaster_DivisionId",
                schema: "Finance",
                table: "ScheduleIIIMaster",
                column: "DivisionId");

            migrationBuilder.CreateIndex(
                name: "IX_ScheduleIIIMaster_ScheduleIIISectionItemId",
                schema: "Finance",
                table: "ScheduleIIIMaster",
                column: "ScheduleIIISectionItemId");

            migrationBuilder.CreateIndex(
                name: "IX_ScheduleIIIMaster_StatusId",
                schema: "Finance",
                table: "ScheduleIIIMaster",
                column: "StatusId");
        }
    }
}
