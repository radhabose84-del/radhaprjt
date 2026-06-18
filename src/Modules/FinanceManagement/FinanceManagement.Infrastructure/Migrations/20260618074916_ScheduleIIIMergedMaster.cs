using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace FinanceManagement.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class ScheduleIIIMergedMaster : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_AccountGroup_ScheduleIIILineItem_ScheduleIIILineItemId",
                schema: "Finance",
                table: "AccountGroup");

            migrationBuilder.DropForeignKey(
                name: "FK_ScheduleIIIMaster_MiscMaster_StructureStatusId",
                schema: "Finance",
                table: "ScheduleIIIMaster");

            migrationBuilder.DropForeignKey(
                name: "FK_ScheduleIIISubTotal_ScheduleIIIMaster_ScheduleIIIMasterId",
                schema: "Finance",
                table: "ScheduleIIISubTotal");

            migrationBuilder.DropTable(
                name: "ScheduleIIIMasterLine",
                schema: "Finance");

            migrationBuilder.DropTable(
                name: "ScheduleIIILineItem",
                schema: "Finance");

            migrationBuilder.DropIndex(
                name: "IX_ScheduleIIISubTotal_ScheduleIIIMasterId",
                schema: "Finance",
                table: "ScheduleIIISubTotal");

            migrationBuilder.DropIndex(
                name: "IX_ScheduleIIIMaster_CompanyId_DivisionId",
                schema: "Finance",
                table: "ScheduleIIIMaster");

            migrationBuilder.DropColumn(
                name: "VersionNo",
                schema: "Finance",
                table: "ScheduleIIIMaster");

            migrationBuilder.RenameColumn(
                name: "ScheduleIIIMasterId",
                schema: "Finance",
                table: "ScheduleIIISubTotal",
                newName: "DivisionId");

            migrationBuilder.RenameColumn(
                name: "StructureStatusId",
                schema: "Finance",
                table: "ScheduleIIIMaster",
                newName: "StatusId");

            migrationBuilder.RenameIndex(
                name: "IX_ScheduleIIIMaster_StructureStatusId",
                schema: "Finance",
                table: "ScheduleIIIMaster",
                newName: "IX_ScheduleIIIMaster_StatusId");

            migrationBuilder.RenameColumn(
                name: "ScheduleIIILineItemId",
                schema: "Finance",
                table: "AccountGroup",
                newName: "ScheduleIIISectionItemId");

            migrationBuilder.RenameIndex(
                name: "IX_AccountGroup_ScheduleIIILineItemId",
                schema: "Finance",
                table: "AccountGroup",
                newName: "IX_AccountGroup_ScheduleIIISectionItemId");

            migrationBuilder.AddColumn<int>(
                name: "CompanyId",
                schema: "Finance",
                table: "ScheduleIIISubTotal",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "DisplayOrder",
                schema: "Finance",
                table: "ScheduleIIIMaster",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "ScheduleIIISectionItemId",
                schema: "Finance",
                table: "ScheduleIIIMaster",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateTable(
                name: "ScheduleIIISectionItem",
                schema: "Finance",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    SectionId = table.Column<int>(type: "int", nullable: false),
                    LineCode = table.Column<string>(type: "varchar(20)", nullable: true),
                    LineName = table.Column<string>(type: "varchar(200)", nullable: false),
                    NoteReference = table.Column<string>(type: "varchar(30)", nullable: true),
                    IsSplitLine = table.Column<bool>(type: "bit", nullable: false, defaultValue: false),
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
                    table.PrimaryKey("PK_ScheduleIIISectionItem", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ScheduleIIISectionItem_ScheduleIIISection_SectionId",
                        column: x => x.SectionId,
                        principalSchema: "Finance",
                        principalTable: "ScheduleIIISection",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_ScheduleIIISubTotal_CompanyId_DivisionId",
                schema: "Finance",
                table: "ScheduleIIISubTotal",
                columns: new[] { "CompanyId", "DivisionId" });

            migrationBuilder.CreateIndex(
                name: "IX_ScheduleIIIMaster_CompanyId_DivisionId_ScheduleIIISectionItemId",
                schema: "Finance",
                table: "ScheduleIIIMaster",
                columns: new[] { "CompanyId", "DivisionId", "ScheduleIIISectionItemId" },
                unique: true,
                filter: "[IsDeleted] = 0");

            migrationBuilder.CreateIndex(
                name: "IX_ScheduleIIIMaster_ScheduleIIISectionItemId",
                schema: "Finance",
                table: "ScheduleIIIMaster",
                column: "ScheduleIIISectionItemId");

            migrationBuilder.CreateIndex(
                name: "IX_ScheduleIIISectionItem_LineCode",
                schema: "Finance",
                table: "ScheduleIIISectionItem",
                column: "LineCode");

            migrationBuilder.CreateIndex(
                name: "IX_ScheduleIIISectionItem_SectionId",
                schema: "Finance",
                table: "ScheduleIIISectionItem",
                column: "SectionId");

            migrationBuilder.AddForeignKey(
                name: "FK_AccountGroup_ScheduleIIISectionItem_ScheduleIIISectionItemId",
                schema: "Finance",
                table: "AccountGroup",
                column: "ScheduleIIISectionItemId",
                principalSchema: "Finance",
                principalTable: "ScheduleIIISectionItem",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_ScheduleIIIMaster_MiscMaster_StatusId",
                schema: "Finance",
                table: "ScheduleIIIMaster",
                column: "StatusId",
                principalSchema: "Finance",
                principalTable: "MiscMaster",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_ScheduleIIIMaster_ScheduleIIISectionItem_ScheduleIIISectionItemId",
                schema: "Finance",
                table: "ScheduleIIIMaster",
                column: "ScheduleIIISectionItemId",
                principalSchema: "Finance",
                principalTable: "ScheduleIIISectionItem",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_AccountGroup_ScheduleIIISectionItem_ScheduleIIISectionItemId",
                schema: "Finance",
                table: "AccountGroup");

            migrationBuilder.DropForeignKey(
                name: "FK_ScheduleIIIMaster_MiscMaster_StatusId",
                schema: "Finance",
                table: "ScheduleIIIMaster");

            migrationBuilder.DropForeignKey(
                name: "FK_ScheduleIIIMaster_ScheduleIIISectionItem_ScheduleIIISectionItemId",
                schema: "Finance",
                table: "ScheduleIIIMaster");

            migrationBuilder.DropTable(
                name: "ScheduleIIISectionItem",
                schema: "Finance");

            migrationBuilder.DropIndex(
                name: "IX_ScheduleIIISubTotal_CompanyId_DivisionId",
                schema: "Finance",
                table: "ScheduleIIISubTotal");

            migrationBuilder.DropIndex(
                name: "IX_ScheduleIIIMaster_CompanyId_DivisionId_ScheduleIIISectionItemId",
                schema: "Finance",
                table: "ScheduleIIIMaster");

            migrationBuilder.DropIndex(
                name: "IX_ScheduleIIIMaster_ScheduleIIISectionItemId",
                schema: "Finance",
                table: "ScheduleIIIMaster");

            migrationBuilder.DropColumn(
                name: "CompanyId",
                schema: "Finance",
                table: "ScheduleIIISubTotal");

            migrationBuilder.DropColumn(
                name: "DisplayOrder",
                schema: "Finance",
                table: "ScheduleIIIMaster");

            migrationBuilder.DropColumn(
                name: "ScheduleIIISectionItemId",
                schema: "Finance",
                table: "ScheduleIIIMaster");

            migrationBuilder.RenameColumn(
                name: "DivisionId",
                schema: "Finance",
                table: "ScheduleIIISubTotal",
                newName: "ScheduleIIIMasterId");

            migrationBuilder.RenameColumn(
                name: "StatusId",
                schema: "Finance",
                table: "ScheduleIIIMaster",
                newName: "StructureStatusId");

            migrationBuilder.RenameIndex(
                name: "IX_ScheduleIIIMaster_StatusId",
                schema: "Finance",
                table: "ScheduleIIIMaster",
                newName: "IX_ScheduleIIIMaster_StructureStatusId");

            migrationBuilder.RenameColumn(
                name: "ScheduleIIISectionItemId",
                schema: "Finance",
                table: "AccountGroup",
                newName: "ScheduleIIILineItemId");

            migrationBuilder.RenameIndex(
                name: "IX_AccountGroup_ScheduleIIISectionItemId",
                schema: "Finance",
                table: "AccountGroup",
                newName: "IX_AccountGroup_ScheduleIIILineItemId");

            migrationBuilder.AddColumn<int>(
                name: "VersionNo",
                schema: "Finance",
                table: "ScheduleIIIMaster",
                type: "int",
                nullable: false,
                defaultValue: 1);

            migrationBuilder.CreateTable(
                name: "ScheduleIIILineItem",
                schema: "Finance",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    SectionId = table.Column<int>(type: "int", nullable: false),
                    CreatedBy = table.Column<int>(type: "int", nullable: false),
                    CreatedByName = table.Column<string>(type: "varchar(100)", nullable: true),
                    CreatedDate = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true),
                    CreatedIP = table.Column<string>(type: "varchar(50)", nullable: true),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false),
                    IsSplitLine = table.Column<bool>(type: "bit", nullable: false, defaultValue: false),
                    LineCode = table.Column<string>(type: "varchar(20)", nullable: true),
                    LineName = table.Column<string>(type: "varchar(200)", nullable: false),
                    ModifiedBy = table.Column<int>(type: "int", nullable: true),
                    ModifiedByName = table.Column<string>(type: "varchar(100)", nullable: true),
                    ModifiedDate = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true),
                    ModifiedIP = table.Column<string>(type: "varchar(50)", nullable: true),
                    NoteReference = table.Column<string>(type: "varchar(30)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ScheduleIIILineItem", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ScheduleIIILineItem_ScheduleIIISection_SectionId",
                        column: x => x.SectionId,
                        principalSchema: "Finance",
                        principalTable: "ScheduleIIISection",
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
                    ScheduleIIILineItemId = table.Column<int>(type: "int", nullable: false),
                    ScheduleIIIMasterId = table.Column<int>(type: "int", nullable: false),
                    CreatedBy = table.Column<int>(type: "int", nullable: false),
                    CreatedByName = table.Column<string>(type: "varchar(100)", nullable: true),
                    CreatedDate = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true),
                    CreatedIP = table.Column<string>(type: "varchar(50)", nullable: true),
                    DisplayOrder = table.Column<int>(type: "int", nullable: false, defaultValue: 0),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false),
                    ModifiedBy = table.Column<int>(type: "int", nullable: true),
                    ModifiedByName = table.Column<string>(type: "varchar(100)", nullable: true),
                    ModifiedDate = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true),
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
                name: "IX_ScheduleIIIMaster_CompanyId_DivisionId",
                schema: "Finance",
                table: "ScheduleIIIMaster",
                columns: new[] { "CompanyId", "DivisionId" },
                unique: true,
                filter: "[IsDeleted] = 0");

            migrationBuilder.CreateIndex(
                name: "IX_ScheduleIIILineItem_LineCode",
                schema: "Finance",
                table: "ScheduleIIILineItem",
                column: "LineCode");

            migrationBuilder.CreateIndex(
                name: "IX_ScheduleIIILineItem_SectionId",
                schema: "Finance",
                table: "ScheduleIIILineItem",
                column: "SectionId");

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
                name: "FK_AccountGroup_ScheduleIIILineItem_ScheduleIIILineItemId",
                schema: "Finance",
                table: "AccountGroup",
                column: "ScheduleIIILineItemId",
                principalSchema: "Finance",
                principalTable: "ScheduleIIILineItem",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_ScheduleIIIMaster_MiscMaster_StructureStatusId",
                schema: "Finance",
                table: "ScheduleIIIMaster",
                column: "StructureStatusId",
                principalSchema: "Finance",
                principalTable: "MiscMaster",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

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
    }
}
