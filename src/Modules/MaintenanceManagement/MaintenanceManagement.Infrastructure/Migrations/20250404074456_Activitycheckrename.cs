using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MaintenanceManagement.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class Activitycheckrename : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "ActivityChecklist",
                schema: "Maintenance",
                table: "ActivityCheckListMaster",
                newName: "ActivityCheckList");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "ActivityCheckList",
                schema: "Maintenance",
                table: "ActivityCheckListMaster",
                newName: "ActivityChecklist");
        }
    }
}
