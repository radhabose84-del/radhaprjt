using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MaintenanceManagement.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class MachineSpecTableAlter : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_MachineSpecification_MachineMaster_MachineMasterId",
                table: "MachineSpecification");

            migrationBuilder.DropForeignKey(
                name: "FK_MachineSpecification_MiscMaster_SpecificationIdMachineSpecId",
                table: "MachineSpecification");

            migrationBuilder.DropIndex(
                name: "IX_MachineSpecification_MachineMasterId",
                table: "MachineSpecification");

            migrationBuilder.DropIndex(
                name: "IX_MachineSpecification_SpecificationIdMachineSpecId",
                table: "MachineSpecification");

            migrationBuilder.DropColumn(
                name: "MachineMasterId",
                table: "MachineSpecification");

            migrationBuilder.DropColumn(
                name: "SpecificationIdMachineSpecId",
                table: "MachineSpecification");

            migrationBuilder.RenameTable(
                name: "MachineSpecification",
                newName: "MachineSpecification",
                newSchema: "Maintenance");

            migrationBuilder.CreateIndex(
                name: "IX_MachineSpecification_MachineId",
                schema: "Maintenance",
                table: "MachineSpecification",
                column: "MachineId");

            migrationBuilder.CreateIndex(
                name: "IX_MachineSpecification_SpecificationId",
                schema: "Maintenance",
                table: "MachineSpecification",
                column: "SpecificationId");

            migrationBuilder.AddForeignKey(
                name: "FK_MachineSpecification_MachineMaster_MachineId",
                schema: "Maintenance",
                table: "MachineSpecification",
                column: "MachineId",
                principalSchema: "Maintenance",
                principalTable: "MachineMaster",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_MachineSpecification_MiscMaster_SpecificationId",
                schema: "Maintenance",
                table: "MachineSpecification",
                column: "SpecificationId",
                principalSchema: "Maintenance",
                principalTable: "MiscMaster",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_MachineSpecification_MachineMaster_MachineId",
                schema: "Maintenance",
                table: "MachineSpecification");

            migrationBuilder.DropForeignKey(
                name: "FK_MachineSpecification_MiscMaster_SpecificationId",
                schema: "Maintenance",
                table: "MachineSpecification");

            migrationBuilder.DropIndex(
                name: "IX_MachineSpecification_MachineId",
                schema: "Maintenance",
                table: "MachineSpecification");

            migrationBuilder.DropIndex(
                name: "IX_MachineSpecification_SpecificationId",
                schema: "Maintenance",
                table: "MachineSpecification");

            migrationBuilder.RenameTable(
                name: "MachineSpecification",
                schema: "Maintenance",
                newName: "MachineSpecification");

            migrationBuilder.AddColumn<int>(
                name: "MachineMasterId",
                table: "MachineSpecification",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "SpecificationIdMachineSpecId",
                table: "MachineSpecification",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateIndex(
                name: "IX_MachineSpecification_MachineMasterId",
                table: "MachineSpecification",
                column: "MachineMasterId");

            migrationBuilder.CreateIndex(
                name: "IX_MachineSpecification_SpecificationIdMachineSpecId",
                table: "MachineSpecification",
                column: "SpecificationIdMachineSpecId");

            migrationBuilder.AddForeignKey(
                name: "FK_MachineSpecification_MachineMaster_MachineMasterId",
                table: "MachineSpecification",
                column: "MachineMasterId",
                principalSchema: "Maintenance",
                principalTable: "MachineMaster",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_MachineSpecification_MiscMaster_SpecificationIdMachineSpecId",
                table: "MachineSpecification",
                column: "SpecificationIdMachineSpecId",
                principalSchema: "Maintenance",
                principalTable: "MiscMaster",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
