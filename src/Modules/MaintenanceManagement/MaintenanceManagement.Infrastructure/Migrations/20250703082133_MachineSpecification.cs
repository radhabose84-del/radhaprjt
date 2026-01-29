using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MaintenanceManagement.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class MachineSpecification : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "MachineSpecification",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    SpecificationId = table.Column<int>(type: "int", nullable: false),
                    SpecificationIdMachineSpecId = table.Column<int>(type: "int", nullable: false),
                    MachineId = table.Column<int>(type: "int", nullable: false),
                    MachineMasterId = table.Column<int>(type: "int", nullable: true),
                    IsActive = table.Column<int>(type: "int", nullable: false),
                    IsDeleted = table.Column<int>(type: "int", nullable: false),
                    CreatedBy = table.Column<int>(type: "int", nullable: false),
                    CreatedDate = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true),
                    CreatedByName = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    CreatedIP = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ModifiedBy = table.Column<int>(type: "int", nullable: true),
                    ModifiedDate = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true),
                    ModifiedByName = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ModifiedIP = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MachineSpecification", x => x.Id);
                    table.ForeignKey(
                        name: "FK_MachineSpecification_MachineMaster_MachineMasterId",
                        column: x => x.MachineMasterId,
                        principalSchema: "Maintenance",
                        principalTable: "MachineMaster",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_MachineSpecification_MiscMaster_SpecificationIdMachineSpecId",
                        column: x => x.SpecificationIdMachineSpecId,
                        principalSchema: "Maintenance",
                        principalTable: "MiscMaster",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_MachineSpecification_MachineMasterId",
                table: "MachineSpecification",
                column: "MachineMasterId");

            migrationBuilder.CreateIndex(
                name: "IX_MachineSpecification_SpecificationIdMachineSpecId",
                table: "MachineSpecification",
                column: "SpecificationIdMachineSpecId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "MachineSpecification");
        }
    }
}
