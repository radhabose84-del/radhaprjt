using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MaintenanceManagement.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class MaintenanceRequestaddrequestid : Migration
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

            migrationBuilder.AddColumn<string>(
                name: "RequestId",
                schema: "Maintenance",
                table: "MaintenanceRequest",
                type: "nvarchar(max)",
                nullable: true);

            
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            
            migrationBuilder.DropColumn(
                name: "RequestId",
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
