using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace FinanceManagement.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AccountGroupHierarchy : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "AccountGroup",
                schema: "Finance",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    CompanyId = table.Column<int>(type: "int", nullable: false),
                    GroupCode = table.Column<string>(type: "varchar(50)", nullable: false),
                    GroupName = table.Column<string>(type: "varchar(150)", nullable: false),
                    ParentAccountGroupId = table.Column<int>(type: "int", nullable: true),
                    Level = table.Column<int>(type: "int", nullable: false),
                    IsLeaf = table.Column<bool>(type: "bit", nullable: false),
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
                    table.PrimaryKey("PK_AccountGroup", x => x.Id);
                    table.ForeignKey(
                        name: "FK_AccountGroup_AccountGroup_ParentAccountGroupId",
                        column: x => x.ParentAccountGroupId,
                        principalSchema: "Finance",
                        principalTable: "AccountGroup",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_AccountGroup_CompanyId",
                schema: "Finance",
                table: "AccountGroup",
                column: "CompanyId");

            migrationBuilder.CreateIndex(
                name: "IX_AccountGroup_GroupCode",
                schema: "Finance",
                table: "AccountGroup",
                column: "GroupCode",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_AccountGroup_Level",
                schema: "Finance",
                table: "AccountGroup",
                column: "Level");

            migrationBuilder.CreateIndex(
                name: "IX_AccountGroup_ParentAccountGroupId",
                schema: "Finance",
                table: "AccountGroup",
                column: "ParentAccountGroupId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "AccountGroup",
                schema: "Finance");
        }
    }
}
