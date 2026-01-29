using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MaintenanceManagement.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class preventiveScheduleColumnnamechange : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_PreventiveSchedulerHeader_MiscMaster_DueTypeId",
                schema: "Maintenance",
                table: "PreventiveSchedulerHeader");

            migrationBuilder.DropForeignKey(
                name: "FK_PreventiveSchedulerHeader_MiscMaster_FrequencyId",
                schema: "Maintenance",
                table: "PreventiveSchedulerHeader");

            migrationBuilder.RenameColumn(
                name: "FrequencyId",
                schema: "Maintenance",
                table: "PreventiveSchedulerHeader",
                newName: "FrequencyUnitId");

            migrationBuilder.RenameColumn(
                name: "DueTypeId",
                schema: "Maintenance",
                table: "PreventiveSchedulerHeader",
                newName: "FrequencyTypeId");

            migrationBuilder.RenameColumn(
                name: "DuePeriod",
                schema: "Maintenance",
                table: "PreventiveSchedulerHeader",
                newName: "FrequencyInterval");

            migrationBuilder.RenameIndex(
                name: "IX_PreventiveSchedulerHeader_FrequencyId",
                schema: "Maintenance",
                table: "PreventiveSchedulerHeader",
                newName: "IX_PreventiveSchedulerHeader_FrequencyUnitId");

            migrationBuilder.RenameIndex(
                name: "IX_PreventiveSchedulerHeader_DueTypeId",
                schema: "Maintenance",
                table: "PreventiveSchedulerHeader",
                newName: "IX_PreventiveSchedulerHeader_FrequencyTypeId");

            migrationBuilder.AddForeignKey(
                name: "FK_PreventiveSchedulerHeader_MiscMaster_FrequencyTypeId",
                schema: "Maintenance",
                table: "PreventiveSchedulerHeader",
                column: "FrequencyTypeId",
                principalSchema: "Maintenance",
                principalTable: "MiscMaster",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_PreventiveSchedulerHeader_MiscMaster_FrequencyUnitId",
                schema: "Maintenance",
                table: "PreventiveSchedulerHeader",
                column: "FrequencyUnitId",
                principalSchema: "Maintenance",
                principalTable: "MiscMaster",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_PreventiveSchedulerHeader_MiscMaster_FrequencyTypeId",
                schema: "Maintenance",
                table: "PreventiveSchedulerHeader");

            migrationBuilder.DropForeignKey(
                name: "FK_PreventiveSchedulerHeader_MiscMaster_FrequencyUnitId",
                schema: "Maintenance",
                table: "PreventiveSchedulerHeader");

            migrationBuilder.RenameColumn(
                name: "FrequencyUnitId",
                schema: "Maintenance",
                table: "PreventiveSchedulerHeader",
                newName: "FrequencyId");

            migrationBuilder.RenameColumn(
                name: "FrequencyTypeId",
                schema: "Maintenance",
                table: "PreventiveSchedulerHeader",
                newName: "DueTypeId");

            migrationBuilder.RenameColumn(
                name: "FrequencyInterval",
                schema: "Maintenance",
                table: "PreventiveSchedulerHeader",
                newName: "DuePeriod");

            migrationBuilder.RenameIndex(
                name: "IX_PreventiveSchedulerHeader_FrequencyUnitId",
                schema: "Maintenance",
                table: "PreventiveSchedulerHeader",
                newName: "IX_PreventiveSchedulerHeader_FrequencyId");

            migrationBuilder.RenameIndex(
                name: "IX_PreventiveSchedulerHeader_FrequencyTypeId",
                schema: "Maintenance",
                table: "PreventiveSchedulerHeader",
                newName: "IX_PreventiveSchedulerHeader_DueTypeId");

            migrationBuilder.AddForeignKey(
                name: "FK_PreventiveSchedulerHeader_MiscMaster_DueTypeId",
                schema: "Maintenance",
                table: "PreventiveSchedulerHeader",
                column: "DueTypeId",
                principalSchema: "Maintenance",
                principalTable: "MiscMaster",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_PreventiveSchedulerHeader_MiscMaster_FrequencyId",
                schema: "Maintenance",
                table: "PreventiveSchedulerHeader",
                column: "FrequencyId",
                principalSchema: "Maintenance",
                principalTable: "MiscMaster",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }
    }
}
