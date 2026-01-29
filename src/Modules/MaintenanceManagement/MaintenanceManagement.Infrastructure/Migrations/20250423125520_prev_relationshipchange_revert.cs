using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MaintenanceManagement.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class prev_relationshipchange_revert : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_PreventiveSchedulerActivity_PreventiveSchedulerDetail_PreventiveSchedulerDetailId",
                schema: "Maintenance",
                table: "PreventiveSchedulerActivity");

            migrationBuilder.DropForeignKey(
                name: "FK_PreventiveSchedulerDetail_PreventiveSchedulerHeader_PreventiveSchedulerId",
                schema: "Maintenance",
                table: "PreventiveSchedulerDetail");

            migrationBuilder.DropForeignKey(
                name: "FK_PreventiveSchedulerItems_PreventiveSchedulerDetail_PreventiveSchedulerDetailId",
                schema: "Maintenance",
                table: "PreventiveSchedulerItems");

            migrationBuilder.RenameColumn(
                name: "PreventiveSchedulerDetailId",
                schema: "Maintenance",
                table: "PreventiveSchedulerItems",
                newName: "PreventiveSchedulerHeaderId");

            migrationBuilder.RenameIndex(
                name: "IX_PreventiveSchedulerItems_PreventiveSchedulerDetailId",
                schema: "Maintenance",
                table: "PreventiveSchedulerItems",
                newName: "IX_PreventiveSchedulerItems_PreventiveSchedulerHeaderId");

            migrationBuilder.RenameColumn(
                name: "PreventiveSchedulerId",
                schema: "Maintenance",
                table: "PreventiveSchedulerDetail",
                newName: "PreventiveSchedulerHeaderId");

            migrationBuilder.RenameIndex(
                name: "IX_PreventiveSchedulerDetail_PreventiveSchedulerId",
                schema: "Maintenance",
                table: "PreventiveSchedulerDetail",
                newName: "IX_PreventiveSchedulerDetail_PreventiveSchedulerHeaderId");

            migrationBuilder.RenameColumn(
                name: "PreventiveSchedulerDetailId",
                schema: "Maintenance",
                table: "PreventiveSchedulerActivity",
                newName: "PreventiveSchedulerHeaderId");

            migrationBuilder.RenameIndex(
                name: "IX_PreventiveSchedulerActivity_PreventiveSchedulerDetailId",
                schema: "Maintenance",
                table: "PreventiveSchedulerActivity",
                newName: "IX_PreventiveSchedulerActivity_PreventiveSchedulerHeaderId");

            migrationBuilder.AddForeignKey(
                name: "FK_PreventiveSchedulerActivity_PreventiveSchedulerHeader_PreventiveSchedulerHeaderId",
                schema: "Maintenance",
                table: "PreventiveSchedulerActivity",
                column: "PreventiveSchedulerHeaderId",
                principalSchema: "Maintenance",
                principalTable: "PreventiveSchedulerHeader",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_PreventiveSchedulerDetail_PreventiveSchedulerHeader_PreventiveSchedulerHeaderId",
                schema: "Maintenance",
                table: "PreventiveSchedulerDetail",
                column: "PreventiveSchedulerHeaderId",
                principalSchema: "Maintenance",
                principalTable: "PreventiveSchedulerHeader",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_PreventiveSchedulerItems_PreventiveSchedulerHeader_PreventiveSchedulerHeaderId",
                schema: "Maintenance",
                table: "PreventiveSchedulerItems",
                column: "PreventiveSchedulerHeaderId",
                principalSchema: "Maintenance",
                principalTable: "PreventiveSchedulerHeader",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_PreventiveSchedulerActivity_PreventiveSchedulerHeader_PreventiveSchedulerHeaderId",
                schema: "Maintenance",
                table: "PreventiveSchedulerActivity");

            migrationBuilder.DropForeignKey(
                name: "FK_PreventiveSchedulerDetail_PreventiveSchedulerHeader_PreventiveSchedulerHeaderId",
                schema: "Maintenance",
                table: "PreventiveSchedulerDetail");

            migrationBuilder.DropForeignKey(
                name: "FK_PreventiveSchedulerItems_PreventiveSchedulerHeader_PreventiveSchedulerHeaderId",
                schema: "Maintenance",
                table: "PreventiveSchedulerItems");

            migrationBuilder.RenameColumn(
                name: "PreventiveSchedulerHeaderId",
                schema: "Maintenance",
                table: "PreventiveSchedulerItems",
                newName: "PreventiveSchedulerDetailId");

            migrationBuilder.RenameIndex(
                name: "IX_PreventiveSchedulerItems_PreventiveSchedulerHeaderId",
                schema: "Maintenance",
                table: "PreventiveSchedulerItems",
                newName: "IX_PreventiveSchedulerItems_PreventiveSchedulerDetailId");

            migrationBuilder.RenameColumn(
                name: "PreventiveSchedulerHeaderId",
                schema: "Maintenance",
                table: "PreventiveSchedulerDetail",
                newName: "PreventiveSchedulerId");

            migrationBuilder.RenameIndex(
                name: "IX_PreventiveSchedulerDetail_PreventiveSchedulerHeaderId",
                schema: "Maintenance",
                table: "PreventiveSchedulerDetail",
                newName: "IX_PreventiveSchedulerDetail_PreventiveSchedulerId");

            migrationBuilder.RenameColumn(
                name: "PreventiveSchedulerHeaderId",
                schema: "Maintenance",
                table: "PreventiveSchedulerActivity",
                newName: "PreventiveSchedulerDetailId");

            migrationBuilder.RenameIndex(
                name: "IX_PreventiveSchedulerActivity_PreventiveSchedulerHeaderId",
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
                name: "FK_PreventiveSchedulerDetail_PreventiveSchedulerHeader_PreventiveSchedulerId",
                schema: "Maintenance",
                table: "PreventiveSchedulerDetail",
                column: "PreventiveSchedulerId",
                principalSchema: "Maintenance",
                principalTable: "PreventiveSchedulerHeader",
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
    }
}
