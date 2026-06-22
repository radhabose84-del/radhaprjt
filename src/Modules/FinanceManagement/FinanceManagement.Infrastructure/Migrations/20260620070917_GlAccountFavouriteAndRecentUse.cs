using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace FinanceManagement.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class GlAccountFavouriteAndRecentUse : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "GlAccountFavourite",
                schema: "Finance",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    GlAccountMasterId = table.Column<int>(type: "int", nullable: false),
                    UserId = table.Column<int>(type: "int", nullable: false),
                    CompanyId = table.Column<int>(type: "int", nullable: false),
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
                    table.PrimaryKey("PK_GlAccountFavourite", x => x.Id);
                    table.ForeignKey(
                        name: "FK_GlAccountFavourite_GlAccountMaster_GlAccountMasterId",
                        column: x => x.GlAccountMasterId,
                        principalSchema: "Finance",
                        principalTable: "GlAccountMaster",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "GlAccountRecentUse",
                schema: "Finance",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    GlAccountMasterId = table.Column<int>(type: "int", nullable: false),
                    UserId = table.Column<int>(type: "int", nullable: false),
                    CompanyId = table.Column<int>(type: "int", nullable: false),
                    LastUsedDate = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
                    UseCount = table.Column<int>(type: "int", nullable: false),
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
                    table.PrimaryKey("PK_GlAccountRecentUse", x => x.Id);
                    table.ForeignKey(
                        name: "FK_GlAccountRecentUse_GlAccountMaster_GlAccountMasterId",
                        column: x => x.GlAccountMasterId,
                        principalSchema: "Finance",
                        principalTable: "GlAccountMaster",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_GlAccountFavourite_GlAccountMasterId",
                schema: "Finance",
                table: "GlAccountFavourite",
                column: "GlAccountMasterId");

            migrationBuilder.CreateIndex(
                name: "IX_GlAccountFavourite_UserId_CompanyId",
                schema: "Finance",
                table: "GlAccountFavourite",
                columns: new[] { "UserId", "CompanyId" });

            migrationBuilder.CreateIndex(
                name: "IX_GlAccountFavourite_UserId_CompanyId_GlAccountMasterId",
                schema: "Finance",
                table: "GlAccountFavourite",
                columns: new[] { "UserId", "CompanyId", "GlAccountMasterId" },
                unique: true,
                filter: "[IsDeleted] = 0");

            migrationBuilder.CreateIndex(
                name: "IX_GlAccountRecentUse_GlAccountMasterId",
                schema: "Finance",
                table: "GlAccountRecentUse",
                column: "GlAccountMasterId");

            migrationBuilder.CreateIndex(
                name: "IX_GlAccountRecentUse_UserId_CompanyId_GlAccountMasterId",
                schema: "Finance",
                table: "GlAccountRecentUse",
                columns: new[] { "UserId", "CompanyId", "GlAccountMasterId" },
                unique: true,
                filter: "[IsDeleted] = 0");

            migrationBuilder.CreateIndex(
                name: "IX_GlAccountRecentUse_UserId_CompanyId_LastUsedDate",
                schema: "Finance",
                table: "GlAccountRecentUse",
                columns: new[] { "UserId", "CompanyId", "LastUsedDate" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "GlAccountFavourite",
                schema: "Finance");

            migrationBuilder.DropTable(
                name: "GlAccountRecentUse",
                schema: "Finance");
        }
    }
}
