using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace UserManagement.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class usercompany : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            

            migrationBuilder.CreateTable(
                name: "UserCompany",
                schema: "AppSecurity",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    UserId = table.Column<int>(type: "int", nullable: false),
                    CompanyId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UserCompany", x => x.Id);
                    table.ForeignKey(
                        name: "FK_UserCompany_Company_CompanyId",
                        column: x => x.CompanyId,
                        principalSchema: "AppData",
                        principalTable: "Company",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_UserCompany_Users_UserId",
                        column: x => x.UserId,
                        principalSchema: "AppSecurity",
                        principalTable: "Users",
                        principalColumn: "UserId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_UserCompany_CompanyId",
                schema: "AppSecurity",
                table: "UserCompany",
                column: "CompanyId");

            migrationBuilder.CreateIndex(
                name: "IX_UserCompany_UserId",
                schema: "AppSecurity",
                table: "UserCompany",
                column: "UserId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
           

            migrationBuilder.DropTable(
                name: "UserCompany",
                schema: "AppSecurity");
        }
    }
}
