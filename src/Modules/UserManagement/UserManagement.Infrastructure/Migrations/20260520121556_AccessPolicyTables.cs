using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BSOFT.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AccessPolicyTables : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "AccessPolicy",
                schema: "AppSecurity",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    PolicyCode = table.Column<string>(type: "varchar(50)", nullable: false),
                    PolicyName = table.Column<string>(type: "varchar(100)", nullable: false),
                    EntityName = table.Column<string>(type: "varchar(100)", nullable: false),
                    FieldName = table.Column<string>(type: "varchar(100)", nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    CreatedBy = table.Column<int>(type: "int", nullable: false, defaultValue: 0),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreatedByName = table.Column<string>(type: "varchar(50)", nullable: true),
                    CreatedIP = table.Column<string>(type: "varchar(255)", nullable: true),
                    ModifiedBy = table.Column<int>(type: "int", nullable: true),
                    ModifiedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    ModifiedByName = table.Column<string>(type: "varchar(50)", nullable: true),
                    ModifiedIP = table.Column<string>(type: "varchar(255)", nullable: true),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AccessPolicy", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "RoleAccessPolicy",
                schema: "AppSecurity",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    AccessPolicyId = table.Column<int>(type: "int", nullable: false),
                    RoleId = table.Column<int>(type: "int", nullable: false),
                    ValueId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_RoleAccessPolicy", x => x.Id);
                    table.ForeignKey(
                        name: "FK_RoleAccessPolicy_AccessPolicy_AccessPolicyId",
                        column: x => x.AccessPolicyId,
                        principalSchema: "AppSecurity",
                        principalTable: "AccessPolicy",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_RoleAccessPolicy_UserRole_RoleId",
                        column: x => x.RoleId,
                        principalSchema: "AppSecurity",
                        principalTable: "UserRole",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_AccessPolicy_PolicyCode",
                schema: "AppSecurity",
                table: "AccessPolicy",
                column: "PolicyCode",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_RoleAccessPolicy_AccessPolicyId_RoleId_ValueId",
                schema: "AppSecurity",
                table: "RoleAccessPolicy",
                columns: new[] { "AccessPolicyId", "RoleId", "ValueId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_RoleAccessPolicy_RoleId",
                schema: "AppSecurity",
                table: "RoleAccessPolicy",
                column: "RoleId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "RoleAccessPolicy",
                schema: "AppSecurity");

            migrationBuilder.DropTable(
                name: "AccessPolicy",
                schema: "AppSecurity");
        }
    }
}
