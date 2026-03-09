using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SalesManagement.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class TransactionTypeMasterMaster : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.EnsureSchema(
                name: "Finance");

            migrationBuilder.CreateTable(
                name: "TransactionTypeMaster",
                schema: "Finance",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    UnitId = table.Column<int>(type: "int", nullable: false),
                    ModuleId = table.Column<int>(type: "int", nullable: false),
                    TypeName = table.Column<string>(type: "varchar(100)", nullable: false),
                    ShortName = table.Column<string>(type: "varchar(50)", nullable: false),
                    Description = table.Column<string>(type: "varchar(255)", nullable: true),
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
                    table.PrimaryKey("PK_TransactionTypeMaster", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_TransactionTypeMaster_ModuleId",
                schema: "Finance",
                table: "TransactionTypeMaster",
                column: "ModuleId");

            migrationBuilder.CreateIndex(
                name: "IX_TransactionTypeMaster_ShortName",
                schema: "Finance",
                table: "TransactionTypeMaster",
                column: "ShortName",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_TransactionTypeMaster_TypeName",
                schema: "Finance",
                table: "TransactionTypeMaster",
                column: "TypeName",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_TransactionTypeMaster_UnitId",
                schema: "Finance",
                table: "TransactionTypeMaster",
                column: "UnitId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "TransactionTypeMaster",
                schema: "Finance");
        }
    }
}
