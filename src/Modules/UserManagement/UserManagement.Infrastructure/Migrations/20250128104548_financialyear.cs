using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace UserManagement.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class Financialyear : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {

           // Drop the existing table if it exists
            migrationBuilder.DropTable(
                name: "FinancialYear",
                schema: "AppSecurity"
            );

            // Recreate the table
            migrationBuilder.CreateTable(
                name: "FinancialYear",
                schema: "AppData",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"), // Auto-increment
                    StartYear = table.Column<string>(nullable: false),
                    StartDate = table.Column<DateTime>(nullable: false),
                    EndDate = table.Column<DateTime>(nullable: false),
                    FinYearName = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    CreatedBy = table.Column<int>(type: "int", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreatedByName = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    CreatedIP = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    ModifiedBy = table.Column<int>(type: "int", nullable: true),
                    ModifiedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    ModifiedByName = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ModifiedIP = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_FinancialYear", x => x.Id);
                });
        }
       
        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {

           

        }
    }
}
