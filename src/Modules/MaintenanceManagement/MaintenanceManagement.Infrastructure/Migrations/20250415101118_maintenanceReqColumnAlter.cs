using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MaintenanceManagement.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class maintenanceReqColumnAlter : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<string>(
                name: "Remarks",
                schema: "Maintenance",
                table: "MaintenanceRequest",
                type: "nvarchar(max)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar",
                oldNullable: true);

            migrationBuilder.AddColumn<int>(
                name: "CompanyId",
                schema: "Maintenance",
                table: "MaintenanceRequest",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<decimal>(
                name: "EstimatedServiceCost",
                schema: "Maintenance",
                table: "MaintenanceRequest",
                type: "decimal(18,0)",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "EstimatedSpareCost",
                schema: "Maintenance",
                table: "MaintenanceRequest",
                type: "decimal(18,2)",
                nullable: true);

            migrationBuilder.AddColumn<DateTimeOffset>(
                name: "ExpectedDispatchDate",
                schema: "Maintenance",
                table: "MaintenanceRequest",
                type: "DateTimeOffset",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "ModeOfDispatchId",
                schema: "Maintenance",
                table: "MaintenanceRequest",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "RequestStatusId",
                schema: "Maintenance",
                table: "MaintenanceRequest",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "ServiceLocationId",
                schema: "Maintenance",
                table: "MaintenanceRequest",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "ServiceTypeId",
                schema: "Maintenance",
                table: "MaintenanceRequest",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "SparesTypeId",
                schema: "Maintenance",
                table: "MaintenanceRequest",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "UnitId",
                schema: "Maintenance",
                table: "MaintenanceRequest",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateIndex(
                name: "IX_MaintenanceRequest_ModeOfDispatchId",
                schema: "Maintenance",
                table: "MaintenanceRequest",
                column: "ModeOfDispatchId");

            migrationBuilder.CreateIndex(
                name: "IX_MaintenanceRequest_RequestStatusId",
                schema: "Maintenance",
                table: "MaintenanceRequest",
                column: "RequestStatusId");

            migrationBuilder.CreateIndex(
                name: "IX_MaintenanceRequest_ServiceLocationId",
                schema: "Maintenance",
                table: "MaintenanceRequest",
                column: "ServiceLocationId");

            migrationBuilder.CreateIndex(
                name: "IX_MaintenanceRequest_ServiceTypeId",
                schema: "Maintenance",
                table: "MaintenanceRequest",
                column: "ServiceTypeId");

            migrationBuilder.CreateIndex(
                name: "IX_MaintenanceRequest_SparesTypeId",
                schema: "Maintenance",
                table: "MaintenanceRequest",
                column: "SparesTypeId");

            migrationBuilder.AddForeignKey(
                name: "FK_MaintenanceRequest_MiscMaster_ModeOfDispatchId",
                schema: "Maintenance",
                table: "MaintenanceRequest",
                column: "ModeOfDispatchId",
                principalSchema: "Maintenance",
                principalTable: "MiscMaster",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_MaintenanceRequest_MiscMaster_RequestStatusId",
                schema: "Maintenance",
                table: "MaintenanceRequest",
                column: "RequestStatusId",
                principalSchema: "Maintenance",
                principalTable: "MiscMaster",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_MaintenanceRequest_MiscMaster_ServiceLocationId",
                schema: "Maintenance",
                table: "MaintenanceRequest",
                column: "ServiceLocationId",
                principalSchema: "Maintenance",
                principalTable: "MiscMaster",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_MaintenanceRequest_MiscMaster_ServiceTypeId",
                schema: "Maintenance",
                table: "MaintenanceRequest",
                column: "ServiceTypeId",
                principalSchema: "Maintenance",
                principalTable: "MiscMaster",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_MaintenanceRequest_MiscMaster_SparesTypeId",
                schema: "Maintenance",
                table: "MaintenanceRequest",
                column: "SparesTypeId",
                principalSchema: "Maintenance",
                principalTable: "MiscMaster",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_MaintenanceRequest_MiscMaster_ModeOfDispatchId",
                schema: "Maintenance",
                table: "MaintenanceRequest");

            migrationBuilder.DropForeignKey(
                name: "FK_MaintenanceRequest_MiscMaster_RequestStatusId",
                schema: "Maintenance",
                table: "MaintenanceRequest");

            migrationBuilder.DropForeignKey(
                name: "FK_MaintenanceRequest_MiscMaster_ServiceLocationId",
                schema: "Maintenance",
                table: "MaintenanceRequest");

            migrationBuilder.DropForeignKey(
                name: "FK_MaintenanceRequest_MiscMaster_ServiceTypeId",
                schema: "Maintenance",
                table: "MaintenanceRequest");

            migrationBuilder.DropForeignKey(
                name: "FK_MaintenanceRequest_MiscMaster_SparesTypeId",
                schema: "Maintenance",
                table: "MaintenanceRequest");

            migrationBuilder.DropIndex(
                name: "IX_MaintenanceRequest_ModeOfDispatchId",
                schema: "Maintenance",
                table: "MaintenanceRequest");

            migrationBuilder.DropIndex(
                name: "IX_MaintenanceRequest_RequestStatusId",
                schema: "Maintenance",
                table: "MaintenanceRequest");

            migrationBuilder.DropIndex(
                name: "IX_MaintenanceRequest_ServiceLocationId",
                schema: "Maintenance",
                table: "MaintenanceRequest");

            migrationBuilder.DropIndex(
                name: "IX_MaintenanceRequest_ServiceTypeId",
                schema: "Maintenance",
                table: "MaintenanceRequest");

            migrationBuilder.DropIndex(
                name: "IX_MaintenanceRequest_SparesTypeId",
                schema: "Maintenance",
                table: "MaintenanceRequest");

            migrationBuilder.DropColumn(
                name: "CompanyId",
                schema: "Maintenance",
                table: "MaintenanceRequest");

            migrationBuilder.DropColumn(
                name: "EstimatedServiceCost",
                schema: "Maintenance",
                table: "MaintenanceRequest");

            migrationBuilder.DropColumn(
                name: "EstimatedSpareCost",
                schema: "Maintenance",
                table: "MaintenanceRequest");

            migrationBuilder.DropColumn(
                name: "ExpectedDispatchDate",
                schema: "Maintenance",
                table: "MaintenanceRequest");

            migrationBuilder.DropColumn(
                name: "ModeOfDispatchId",
                schema: "Maintenance",
                table: "MaintenanceRequest");

            migrationBuilder.DropColumn(
                name: "RequestStatusId",
                schema: "Maintenance",
                table: "MaintenanceRequest");

            migrationBuilder.DropColumn(
                name: "ServiceLocationId",
                schema: "Maintenance",
                table: "MaintenanceRequest");

            migrationBuilder.DropColumn(
                name: "ServiceTypeId",
                schema: "Maintenance",
                table: "MaintenanceRequest");

            migrationBuilder.DropColumn(
                name: "SparesTypeId",
                schema: "Maintenance",
                table: "MaintenanceRequest");

            migrationBuilder.DropColumn(
                name: "UnitId",
                schema: "Maintenance",
                table: "MaintenanceRequest");

            migrationBuilder.AlterColumn<string>(
                name: "Remarks",
                schema: "Maintenance",
                table: "MaintenanceRequest",
                type: "nvarchar",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)",
                oldNullable: true);
        }
    }
}
