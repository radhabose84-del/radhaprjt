using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace FinanceManagement.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class ScheduleIIIGlobalCatalogAndMasterLine : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_ScheduleIIILineItem_ScheduleIIILineItem_ParentLineId",
                schema: "Finance",
                table: "ScheduleIIILineItem");

            migrationBuilder.DropForeignKey(
                name: "FK_ScheduleIIILineItem_ScheduleIIIStructure_StructureId",
                schema: "Finance",
                table: "ScheduleIIILineItem");

            migrationBuilder.DropForeignKey(
                name: "FK_ScheduleIIISection_ScheduleIIIStructure_StructureId",
                schema: "Finance",
                table: "ScheduleIIISection");

            migrationBuilder.DropForeignKey(
                name: "FK_ScheduleIIISubTotal_ScheduleIIIStructure_StructureId",
                schema: "Finance",
                table: "ScheduleIIISubTotal");

            migrationBuilder.DropTable(
                name: "ScheduleIIIStructure",
                schema: "Finance");

            migrationBuilder.DropIndex(
                name: "IX_ScheduleIIISection_StructureId",
                schema: "Finance",
                table: "ScheduleIIISection");

            migrationBuilder.DropIndex(
                name: "IX_ScheduleIIILineItem_ParentLineId",
                schema: "Finance",
                table: "ScheduleIIILineItem");

            migrationBuilder.DropIndex(
                name: "IX_ScheduleIIILineItem_StructureId",
                schema: "Finance",
                table: "ScheduleIIILineItem");

            migrationBuilder.DropColumn(
                name: "SubTotalName",
                schema: "Finance",
                table: "ScheduleIIISubTotal");

            migrationBuilder.DropColumn(
                name: "DisplayOrder",
                schema: "Finance",
                table: "ScheduleIIISection");

            migrationBuilder.DropColumn(
                name: "StructureId",
                schema: "Finance",
                table: "ScheduleIIISection");

            migrationBuilder.DropColumn(
                name: "DisplayOrder",
                schema: "Finance",
                table: "ScheduleIIILineItem");

            migrationBuilder.DropColumn(
                name: "ParentLineId",
                schema: "Finance",
                table: "ScheduleIIILineItem");

            migrationBuilder.DropColumn(
                name: "StructureId",
                schema: "Finance",
                table: "ScheduleIIILineItem");

            migrationBuilder.DropColumn(
                name: "SubClassification",
                schema: "Finance",
                table: "ScheduleIIILineItem");

            migrationBuilder.RenameColumn(
                name: "StructureId",
                schema: "Finance",
                table: "ScheduleIIISubTotal",
                newName: "SubTotalTypeId");

            migrationBuilder.RenameIndex(
                name: "IX_ScheduleIIISubTotal_StructureId",
                schema: "Finance",
                table: "ScheduleIIISubTotal",
                newName: "IX_ScheduleIIISubTotal_SubTotalTypeId");

            migrationBuilder.AddColumn<int>(
                name: "ScheduleIIIMasterId",
                schema: "Finance",
                table: "ScheduleIIISubTotal",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateTable(
                name: "ScheduleIIIMaster",
                schema: "Finance",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    CompanyId = table.Column<int>(type: "int", nullable: false),
                    DivisionId = table.Column<int>(type: "int", nullable: false),
                    StructureStatusId = table.Column<int>(type: "int", nullable: false),
                    TextileSplitEnabled = table.Column<bool>(type: "bit", nullable: false, defaultValue: false),
                    VersionNo = table.Column<int>(type: "int", nullable: false, defaultValue: 1),
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
                    table.PrimaryKey("PK_ScheduleIIIMaster", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ScheduleIIIMaster_MiscMaster_StructureStatusId",
                        column: x => x.StructureStatusId,
                        principalSchema: "Finance",
                        principalTable: "MiscMaster",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "ScheduleIIIMasterLine",
                schema: "Finance",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ScheduleIIIMasterId = table.Column<int>(type: "int", nullable: false),
                    ScheduleIIILineItemId = table.Column<int>(type: "int", nullable: false),
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
                    table.PrimaryKey("PK_ScheduleIIIMasterLine", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ScheduleIIIMasterLine_ScheduleIIILineItem_ScheduleIIILineItemId",
                        column: x => x.ScheduleIIILineItemId,
                        principalSchema: "Finance",
                        principalTable: "ScheduleIIILineItem",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_ScheduleIIIMasterLine_ScheduleIIIMaster_ScheduleIIIMasterId",
                        column: x => x.ScheduleIIIMasterId,
                        principalSchema: "Finance",
                        principalTable: "ScheduleIIIMaster",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_ScheduleIIISubTotal_ScheduleIIIMasterId",
                schema: "Finance",
                table: "ScheduleIIISubTotal",
                column: "ScheduleIIIMasterId");

            migrationBuilder.CreateIndex(
                name: "IX_ScheduleIIILineItem_LineCode",
                schema: "Finance",
                table: "ScheduleIIILineItem",
                column: "LineCode");

            migrationBuilder.CreateIndex(
                name: "IX_ScheduleIIIMaster_CompanyId",
                schema: "Finance",
                table: "ScheduleIIIMaster",
                column: "CompanyId");

            migrationBuilder.CreateIndex(
                name: "IX_ScheduleIIIMaster_CompanyId_DivisionId",
                schema: "Finance",
                table: "ScheduleIIIMaster",
                columns: new[] { "CompanyId", "DivisionId" },
                unique: true,
                filter: "[IsDeleted] = 0");

            migrationBuilder.CreateIndex(
                name: "IX_ScheduleIIIMaster_DivisionId",
                schema: "Finance",
                table: "ScheduleIIIMaster",
                column: "DivisionId");

            migrationBuilder.CreateIndex(
                name: "IX_ScheduleIIIMaster_StructureStatusId",
                schema: "Finance",
                table: "ScheduleIIIMaster",
                column: "StructureStatusId");

            migrationBuilder.CreateIndex(
                name: "IX_ScheduleIIIMasterLine_ScheduleIIILineItemId",
                schema: "Finance",
                table: "ScheduleIIIMasterLine",
                column: "ScheduleIIILineItemId");

            migrationBuilder.CreateIndex(
                name: "IX_ScheduleIIIMasterLine_ScheduleIIIMasterId_ScheduleIIILineItemId",
                schema: "Finance",
                table: "ScheduleIIIMasterLine",
                columns: new[] { "ScheduleIIIMasterId", "ScheduleIIILineItemId" },
                unique: true,
                filter: "[IsDeleted] = 0");

            migrationBuilder.AddForeignKey(
                name: "FK_ScheduleIIISubTotal_ScheduleIIIMaster_ScheduleIIIMasterId",
                schema: "Finance",
                table: "ScheduleIIISubTotal",
                column: "ScheduleIIIMasterId",
                principalSchema: "Finance",
                principalTable: "ScheduleIIIMaster",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_ScheduleIIISubTotal_ScheduleIIIMaster_ScheduleIIIMasterId",
                schema: "Finance",
                table: "ScheduleIIISubTotal");

            migrationBuilder.DropTable(
                name: "ScheduleIIIMasterLine",
                schema: "Finance");

            migrationBuilder.DropTable(
                name: "ScheduleIIIMaster",
                schema: "Finance");

            migrationBuilder.DropIndex(
                name: "IX_ScheduleIIISubTotal_ScheduleIIIMasterId",
                schema: "Finance",
                table: "ScheduleIIISubTotal");

            migrationBuilder.DropIndex(
                name: "IX_ScheduleIIILineItem_LineCode",
                schema: "Finance",
                table: "ScheduleIIILineItem");

            migrationBuilder.DropColumn(
                name: "ScheduleIIIMasterId",
                schema: "Finance",
                table: "ScheduleIIISubTotal");

            migrationBuilder.RenameColumn(
                name: "SubTotalTypeId",
                schema: "Finance",
                table: "ScheduleIIISubTotal",
                newName: "StructureId");

            migrationBuilder.RenameIndex(
                name: "IX_ScheduleIIISubTotal_SubTotalTypeId",
                schema: "Finance",
                table: "ScheduleIIISubTotal",
                newName: "IX_ScheduleIIISubTotal_StructureId");

            migrationBuilder.AddColumn<string>(
                name: "SubTotalName",
                schema: "Finance",
                table: "ScheduleIIISubTotal",
                type: "varchar(120)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<int>(
                name: "DisplayOrder",
                schema: "Finance",
                table: "ScheduleIIISection",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "StructureId",
                schema: "Finance",
                table: "ScheduleIIISection",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "DisplayOrder",
                schema: "Finance",
                table: "ScheduleIIILineItem",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "ParentLineId",
                schema: "Finance",
                table: "ScheduleIIILineItem",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "StructureId",
                schema: "Finance",
                table: "ScheduleIIILineItem",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<string>(
                name: "SubClassification",
                schema: "Finance",
                table: "ScheduleIIILineItem",
                type: "varchar(120)",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "ScheduleIIIStructure",
                schema: "Finance",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    StructureStatusId = table.Column<int>(type: "int", nullable: false),
                    CompanyId = table.Column<int>(type: "int", nullable: false),
                    CreatedBy = table.Column<int>(type: "int", nullable: false),
                    CreatedByName = table.Column<string>(type: "varchar(100)", nullable: true),
                    CreatedDate = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true),
                    CreatedIP = table.Column<string>(type: "varchar(50)", nullable: true),
                    DivisionId = table.Column<int>(type: "int", nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false),
                    ModifiedBy = table.Column<int>(type: "int", nullable: true),
                    ModifiedByName = table.Column<string>(type: "varchar(100)", nullable: true),
                    ModifiedDate = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true),
                    ModifiedIP = table.Column<string>(type: "varchar(50)", nullable: true),
                    TextileSplitEnabled = table.Column<bool>(type: "bit", nullable: false, defaultValue: false),
                    VersionNo = table.Column<int>(type: "int", nullable: false, defaultValue: 1)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ScheduleIIIStructure", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ScheduleIIIStructure_MiscMaster_StructureStatusId",
                        column: x => x.StructureStatusId,
                        principalSchema: "Finance",
                        principalTable: "MiscMaster",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_ScheduleIIISection_StructureId",
                schema: "Finance",
                table: "ScheduleIIISection",
                column: "StructureId");

            migrationBuilder.CreateIndex(
                name: "IX_ScheduleIIILineItem_ParentLineId",
                schema: "Finance",
                table: "ScheduleIIILineItem",
                column: "ParentLineId");

            migrationBuilder.CreateIndex(
                name: "IX_ScheduleIIILineItem_StructureId",
                schema: "Finance",
                table: "ScheduleIIILineItem",
                column: "StructureId");

            migrationBuilder.CreateIndex(
                name: "IX_ScheduleIIIStructure_CompanyId",
                schema: "Finance",
                table: "ScheduleIIIStructure",
                column: "CompanyId");

            migrationBuilder.CreateIndex(
                name: "IX_ScheduleIIIStructure_CompanyId_DivisionId",
                schema: "Finance",
                table: "ScheduleIIIStructure",
                columns: new[] { "CompanyId", "DivisionId" },
                unique: true,
                filter: "[IsDeleted] = 0");

            migrationBuilder.CreateIndex(
                name: "IX_ScheduleIIIStructure_DivisionId",
                schema: "Finance",
                table: "ScheduleIIIStructure",
                column: "DivisionId");

            migrationBuilder.CreateIndex(
                name: "IX_ScheduleIIIStructure_StructureStatusId",
                schema: "Finance",
                table: "ScheduleIIIStructure",
                column: "StructureStatusId");

            migrationBuilder.AddForeignKey(
                name: "FK_ScheduleIIILineItem_ScheduleIIILineItem_ParentLineId",
                schema: "Finance",
                table: "ScheduleIIILineItem",
                column: "ParentLineId",
                principalSchema: "Finance",
                principalTable: "ScheduleIIILineItem",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_ScheduleIIILineItem_ScheduleIIIStructure_StructureId",
                schema: "Finance",
                table: "ScheduleIIILineItem",
                column: "StructureId",
                principalSchema: "Finance",
                principalTable: "ScheduleIIIStructure",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_ScheduleIIISection_ScheduleIIIStructure_StructureId",
                schema: "Finance",
                table: "ScheduleIIISection",
                column: "StructureId",
                principalSchema: "Finance",
                principalTable: "ScheduleIIIStructure",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_ScheduleIIISubTotal_ScheduleIIIStructure_StructureId",
                schema: "Finance",
                table: "ScheduleIIISubTotal",
                column: "StructureId",
                principalSchema: "Finance",
                principalTable: "ScheduleIIIStructure",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }
    }
}
