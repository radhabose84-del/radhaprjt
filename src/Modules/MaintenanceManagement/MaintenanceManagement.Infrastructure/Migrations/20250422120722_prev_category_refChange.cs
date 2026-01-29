using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MaintenanceManagement.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class prev_category_refChange : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_PreventiveSchedulerHeader_MaintenanceCategory_MaintenanceCategoryId",
                schema: "Maintenance",
                table: "PreventiveSchedulerHeader");

            migrationBuilder.AddColumn<string>(
                name: "OldCategoryDescription",
                schema: "Maintenance",
                table: "PreventiveSchedulerItems",
                type: "varchar(50)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "OldGroupName",
                schema: "Maintenance",
                table: "PreventiveSchedulerItems",
                type: "varchar(50)",
                nullable: true);

            migrationBuilder.AddForeignKey(
                name: "FK_PreventiveSchedulerHeader_MiscMaster_MaintenanceCategoryId",
                schema: "Maintenance",
                table: "PreventiveSchedulerHeader",
                column: "MaintenanceCategoryId",
                principalSchema: "Maintenance",
                principalTable: "MiscMaster",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_PreventiveSchedulerHeader_MiscMaster_MaintenanceCategoryId",
                schema: "Maintenance",
                table: "PreventiveSchedulerHeader");

            migrationBuilder.DropColumn(
                name: "OldCategoryDescription",
                schema: "Maintenance",
                table: "PreventiveSchedulerItems");

            migrationBuilder.DropColumn(
                name: "OldGroupName",
                schema: "Maintenance",
                table: "PreventiveSchedulerItems");

            migrationBuilder.AddForeignKey(
                name: "FK_PreventiveSchedulerHeader_MaintenanceCategory_MaintenanceCategoryId",
                schema: "Maintenance",
                table: "PreventiveSchedulerHeader",
                column: "MaintenanceCategoryId",
                principalSchema: "Maintenance",
                principalTable: "MaintenanceCategory",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }
    }
}
