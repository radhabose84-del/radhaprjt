using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MaintenanceManagement.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class SubstoresTableNameChange : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropPrimaryKey(
                name: "PK_ItemTransactions",
                schema: "Maintenance",
                table: "ItemTransactions");

            migrationBuilder.RenameTable(
                name: "ItemTransactions",
                schema: "Maintenance",
                newName: "SubStores",
                newSchema: "Maintenance");

            migrationBuilder.RenameColumn(
                name: "Type",
                schema: "Maintenance",
                table: "SubStores",
                newName: "TransactionType");

            migrationBuilder.RenameColumn(
                name: "GrpName",
                schema: "Maintenance",
                table: "SubStores",
                newName: "GroupName");

            migrationBuilder.RenameColumn(
                name: "DocDt",
                schema: "Maintenance",
                table: "SubStores",
                newName: "DocDate");

            migrationBuilder.RenameColumn(
                name: "DepName",
                schema: "Maintenance",
                table: "SubStores",
                newName: "DepartmentName");

            migrationBuilder.RenameColumn(
                name: "CreatedDt",
                schema: "Maintenance",
                table: "SubStores",
                newName: "CreatedDate");

            migrationBuilder.RenameColumn(
                name: "CatDesc",
                schema: "Maintenance",
                table: "SubStores",
                newName: "CategoryDescription");

            migrationBuilder.AddPrimaryKey(
                name: "PK_SubStores",
                schema: "Maintenance",
                table: "SubStores",
                column: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropPrimaryKey(
                name: "PK_SubStores",
                schema: "Maintenance",
                table: "SubStores");

            migrationBuilder.RenameTable(
                name: "SubStores",
                schema: "Maintenance",
                newName: "ItemTransactions",
                newSchema: "Maintenance");

            migrationBuilder.RenameColumn(
                name: "TransactionType",
                schema: "Maintenance",
                table: "ItemTransactions",
                newName: "Type");

            migrationBuilder.RenameColumn(
                name: "GroupName",
                schema: "Maintenance",
                table: "ItemTransactions",
                newName: "GrpName");

            migrationBuilder.RenameColumn(
                name: "DocDate",
                schema: "Maintenance",
                table: "ItemTransactions",
                newName: "DocDt");

            migrationBuilder.RenameColumn(
                name: "DepartmentName",
                schema: "Maintenance",
                table: "ItemTransactions",
                newName: "DepName");

            migrationBuilder.RenameColumn(
                name: "CreatedDate",
                schema: "Maintenance",
                table: "ItemTransactions",
                newName: "CreatedDt");

            migrationBuilder.RenameColumn(
                name: "CategoryDescription",
                schema: "Maintenance",
                table: "ItemTransactions",
                newName: "CatDesc");

            migrationBuilder.AddPrimaryKey(
                name: "PK_ItemTransactions",
                schema: "Maintenance",
                table: "ItemTransactions",
                column: "Id");
        }
    }
}
