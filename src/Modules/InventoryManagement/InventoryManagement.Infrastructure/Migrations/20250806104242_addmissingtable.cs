using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace InventoryManagement.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class addmissingtable : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
        
          
           
          
           
          
           
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "BudgetLog",
                schema: "Inventory");

            migrationBuilder.DropTable(
                name: "HSNMaster",
                schema: "Inventory");

            migrationBuilder.DropTable(
                name: "ItemCategory",
                schema: "Inventory");

            migrationBuilder.DropTable(
                name: "UOMConversion",
                schema: "Inventory");

            migrationBuilder.DropTable(
                name: "BudgetDetail",
                schema: "Inventory");

            migrationBuilder.DropTable(
                name: "ItemGroup",
                schema: "Inventory");

            migrationBuilder.DropTable(
                name: "UOM",
                schema: "Inventory");

            migrationBuilder.DropTable(
                name: "BudgetMaster",
                schema: "Inventory");

            migrationBuilder.EnsureSchema(
                name: "Purchase");

            migrationBuilder.RenameTable(
                name: "MiscTypeMaster",
                schema: "Inventory",
                newName: "MiscTypeMaster",
                newSchema: "Purchase");

            migrationBuilder.RenameTable(
                name: "MiscMaster",
                schema: "Inventory",
                newName: "MiscMaster",
                newSchema: "Purchase");
        }
    }
}
