using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace FinanceManagement.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class CoaFreezeState : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "CoaFreezeState",
                schema: "Finance",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    CompanyId = table.Column<int>(type: "int", nullable: false),
                    IsFrozen = table.Column<bool>(type: "bit", nullable: false),
                    FrozenByUserId = table.Column<int>(type: "int", nullable: true),
                    FrozenOn = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true),
                    UnfreezeWindowExpiry = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true),
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
                    table.PrimaryKey("PK_CoaFreezeState", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_CoaFreezeState_CompanyId",
                schema: "Finance",
                table: "CoaFreezeState",
                column: "CompanyId",
                unique: true,
                filter: "[IsDeleted] = 0");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "CoaFreezeState",
                schema: "Finance");
        }
    }
}
