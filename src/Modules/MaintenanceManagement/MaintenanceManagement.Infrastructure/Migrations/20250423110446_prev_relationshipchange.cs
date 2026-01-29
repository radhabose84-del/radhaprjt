using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MaintenanceManagement.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class prev_relationshipchange : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_PreventiveSchedulerActivity_PreventiveSchedulerHeader_PreventiveSchedulerHdrId",
                schema: "Maintenance",
                table: "PreventiveSchedulerActivity");

            migrationBuilder.DropForeignKey(
                name: "FK_PreventiveSchedulerItems_PreventiveSchedulerHeader_PreventiveSchedulerId",
                schema: "Maintenance",
                table: "PreventiveSchedulerItems");

            migrationBuilder.RenameColumn(
                name: "PreventiveSchedulerId",
                schema: "Maintenance",
                table: "PreventiveSchedulerItems",
                newName: "PreventiveSchedulerDetailId");

            migrationBuilder.RenameIndex(
                name: "IX_PreventiveSchedulerItems_PreventiveSchedulerId",
                schema: "Maintenance",
                table: "PreventiveSchedulerItems",
                newName: "IX_PreventiveSchedulerItems_PreventiveSchedulerDetailId");

            migrationBuilder.RenameColumn(
                name: "PreventiveSchedulerHdrId",
                schema: "Maintenance",
                table: "PreventiveSchedulerActivity",
                newName: "PreventiveSchedulerDetailId");

            migrationBuilder.RenameIndex(
                name: "IX_PreventiveSchedulerActivity_PreventiveSchedulerHdrId",
                schema: "Maintenance",
                table: "PreventiveSchedulerActivity",
                newName: "IX_PreventiveSchedulerActivity_PreventiveSchedulerDetailId");

            migrationBuilder.AddForeignKey(
                name: "FK_PreventiveSchedulerActivity_PreventiveSchedulerDetail_PreventiveSchedulerDetailId",
                schema: "Maintenance",
                table: "PreventiveSchedulerActivity",
                column: "PreventiveSchedulerDetailId",
                principalSchema: "Maintenance",
                principalTable: "PreventiveSchedulerDetail",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_PreventiveSchedulerItems_PreventiveSchedulerDetail_PreventiveSchedulerDetailId",
                schema: "Maintenance",
                table: "PreventiveSchedulerItems",
                column: "PreventiveSchedulerDetailId",
                principalSchema: "Maintenance",
                principalTable: "PreventiveSchedulerDetail",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_PreventiveSchedulerActivity_PreventiveSchedulerDetail_PreventiveSchedulerDetailId",
                schema: "Maintenance",
                table: "PreventiveSchedulerActivity");

            migrationBuilder.DropForeignKey(
                name: "FK_PreventiveSchedulerItems_PreventiveSchedulerDetail_PreventiveSchedulerDetailId",
                schema: "Maintenance",
                table: "PreventiveSchedulerItems");

            migrationBuilder.RenameColumn(
                name: "PreventiveSchedulerDetailId",
                schema: "Maintenance",
                table: "PreventiveSchedulerItems",
                newName: "PreventiveSchedulerId");

            migrationBuilder.RenameIndex(
                name: "IX_PreventiveSchedulerItems_PreventiveSchedulerDetailId",
                schema: "Maintenance",
                table: "PreventiveSchedulerItems",
                newName: "IX_PreventiveSchedulerItems_PreventiveSchedulerId");

            migrationBuilder.RenameColumn(
                name: "PreventiveSchedulerDetailId",
                schema: "Maintenance",
                table: "PreventiveSchedulerActivity",
                newName: "PreventiveSchedulerHdrId");

            migrationBuilder.RenameIndex(
                name: "IX_PreventiveSchedulerActivity_PreventiveSchedulerDetailId",
                schema: "Maintenance",
                table: "PreventiveSchedulerActivity",
                newName: "IX_PreventiveSchedulerActivity_PreventiveSchedulerHdrId");

            migrationBuilder.AddForeignKey(
                name: "FK_PreventiveSchedulerActivity_PreventiveSchedulerHeader_PreventiveSchedulerHdrId",
                schema: "Maintenance",
                table: "PreventiveSchedulerActivity",
                column: "PreventiveSchedulerHdrId",
                principalSchema: "Maintenance",
                principalTable: "PreventiveSchedulerHeader",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_PreventiveSchedulerItems_PreventiveSchedulerHeader_PreventiveSchedulerId",
                schema: "Maintenance",
                table: "PreventiveSchedulerItems",
                column: "PreventiveSchedulerId",
                principalSchema: "Maintenance",
                principalTable: "PreventiveSchedulerHeader",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }
    }
}
