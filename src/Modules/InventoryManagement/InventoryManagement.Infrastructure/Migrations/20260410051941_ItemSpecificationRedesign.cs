using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace InventoryManagement.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class ItemSpecificationRedesign : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Purge legacy variant data — incompatible with new ItemSpecification-based schema.
            // Old ItemVariantAttribute used (ItemId, AttributeId) as the unique key, but AttributeId
            // is being dropped. Existing rows can produce duplicate (ItemId, SpecificationMasterId)
            // pairs when VariantBasedOn is renamed to SpecificationMasterId.
            // ItemVariantValue must be deleted first because it FKs to ItemVariantAttribute.
            // Child ItemMaster rows (ParentItemId != NULL) are also removed as they were generated
            // from legacy variant combos and no longer have valid metadata.
            migrationBuilder.Sql(@"
                DELETE FROM [Inventory].[ItemVariantValue];
                DELETE FROM [Inventory].[ItemVariantAttribute];
                DELETE FROM [Inventory].[ItemMaster] WHERE ParentItemId IS NOT NULL AND ParentItemId > 0;
            ");

            migrationBuilder.DropForeignKey(
                name: "FK_ItemVariantAttribute_MiscMaster_AttributeId",
                schema: "Inventory",
                table: "ItemVariantAttribute");

            migrationBuilder.DropForeignKey(
                name: "FK_ItemVariantAttribute_MiscMaster_VariantBasedOn",
                schema: "Inventory",
                table: "ItemVariantAttribute");

            migrationBuilder.DropForeignKey(
                name: "FK_ItemVariantAttribute_MiscTypeMaster_AttributeGroupId",
                schema: "Inventory",
                table: "ItemVariantAttribute");

            migrationBuilder.DropIndex(
                name: "IX_ItemVariantValue_ItemId_VariantAttributeId_OptionValue",
                schema: "Inventory",
                table: "ItemVariantValue");

            migrationBuilder.DropIndex(
                name: "IX_ItemVariantAttribute_AttributeGroupId",
                schema: "Inventory",
                table: "ItemVariantAttribute");

            migrationBuilder.DropIndex(
                name: "IX_ItemVariantAttribute_AttributeId",
                schema: "Inventory",
                table: "ItemVariantAttribute");

            migrationBuilder.DropIndex(
                name: "IX_ItemVariantAttribute_ItemId_AttributeId",
                schema: "Inventory",
                table: "ItemVariantAttribute");

            migrationBuilder.DropColumn(
                name: "OptionValue",
                schema: "Inventory",
                table: "ItemVariantValue");

            migrationBuilder.DropColumn(
                name: "AttributeGroupId",
                schema: "Inventory",
                table: "ItemVariantAttribute");

            migrationBuilder.DropColumn(
                name: "AttributeId",
                schema: "Inventory",
                table: "ItemVariantAttribute");

            migrationBuilder.RenameColumn(
                name: "VariantBasedOn",
                schema: "Inventory",
                table: "ItemVariantAttribute",
                newName: "SpecificationMasterId");

            migrationBuilder.RenameIndex(
                name: "IX_ItemVariantAttribute_VariantBasedOn",
                schema: "Inventory",
                table: "ItemVariantAttribute",
                newName: "IX_ItemVariantAttribute_SpecificationMasterId");

            migrationBuilder.AddColumn<int>(
                name: "SpecificationValueId",
                schema: "Inventory",
                table: "ItemVariantValue",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateTable(
                name: "ItemSpecificationMaster",
                schema: "Inventory",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    SpecificationCode = table.Column<string>(type: "varchar(20)", nullable: false),
                    SpecificationName = table.Column<string>(type: "varchar(100)", nullable: false),
                    Order = table.Column<int>(type: "int", nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false),
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
                    table.PrimaryKey("PK_ItemSpecificationMaster", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "ItemSpecificationValue",
                schema: "Inventory",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    SpecificationMasterId = table.Column<int>(type: "int", nullable: false),
                    SpecificationValue = table.Column<string>(type: "varchar(100)", nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false),
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
                    table.PrimaryKey("PK_ItemSpecificationValue", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ItemSpecificationValue_ItemSpecificationMaster_SpecificationMasterId",
                        column: x => x.SpecificationMasterId,
                        principalSchema: "Inventory",
                        principalTable: "ItemSpecificationMaster",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "ItemItemSpecification",
                schema: "Inventory",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ItemId = table.Column<int>(type: "int", nullable: false),
                    SpecificationValueId = table.Column<int>(type: "int", nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false),
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
                    table.PrimaryKey("PK_ItemItemSpecification", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ItemItemSpecification_ItemMaster_ItemId",
                        column: x => x.ItemId,
                        principalSchema: "Inventory",
                        principalTable: "ItemMaster",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_ItemItemSpecification_ItemSpecificationValue_SpecificationValueId",
                        column: x => x.SpecificationValueId,
                        principalSchema: "Inventory",
                        principalTable: "ItemSpecificationValue",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_ItemVariantValue_ItemId_VariantAttributeId_SpecificationValueId",
                schema: "Inventory",
                table: "ItemVariantValue",
                columns: new[] { "ItemId", "VariantAttributeId", "SpecificationValueId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_ItemVariantValue_SpecificationValueId",
                schema: "Inventory",
                table: "ItemVariantValue",
                column: "SpecificationValueId");

            migrationBuilder.CreateIndex(
                name: "IX_ItemVariantAttribute_ItemId_SpecificationMasterId",
                schema: "Inventory",
                table: "ItemVariantAttribute",
                columns: new[] { "ItemId", "SpecificationMasterId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_ItemItemSpecification_ItemId_SpecificationValueId",
                schema: "Inventory",
                table: "ItemItemSpecification",
                columns: new[] { "ItemId", "SpecificationValueId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_ItemItemSpecification_SpecificationValueId",
                schema: "Inventory",
                table: "ItemItemSpecification",
                column: "SpecificationValueId");

            migrationBuilder.CreateIndex(
                name: "IX_ItemSpecificationMaster_Order",
                schema: "Inventory",
                table: "ItemSpecificationMaster",
                column: "Order",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_ItemSpecificationMaster_SpecificationCode",
                schema: "Inventory",
                table: "ItemSpecificationMaster",
                column: "SpecificationCode",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_ItemSpecificationValue_SpecificationMasterId_SpecificationValue",
                schema: "Inventory",
                table: "ItemSpecificationValue",
                columns: new[] { "SpecificationMasterId", "SpecificationValue" },
                unique: true);

            migrationBuilder.AddForeignKey(
                name: "FK_ItemVariantAttribute_ItemSpecificationMaster_SpecificationMasterId",
                schema: "Inventory",
                table: "ItemVariantAttribute",
                column: "SpecificationMasterId",
                principalSchema: "Inventory",
                principalTable: "ItemSpecificationMaster",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_ItemVariantValue_ItemSpecificationValue_SpecificationValueId",
                schema: "Inventory",
                table: "ItemVariantValue",
                column: "SpecificationValueId",
                principalSchema: "Inventory",
                principalTable: "ItemSpecificationValue",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_ItemVariantAttribute_ItemSpecificationMaster_SpecificationMasterId",
                schema: "Inventory",
                table: "ItemVariantAttribute");

            migrationBuilder.DropForeignKey(
                name: "FK_ItemVariantValue_ItemSpecificationValue_SpecificationValueId",
                schema: "Inventory",
                table: "ItemVariantValue");

            migrationBuilder.DropTable(
                name: "ItemItemSpecification",
                schema: "Inventory");

            migrationBuilder.DropTable(
                name: "ItemSpecificationValue",
                schema: "Inventory");

            migrationBuilder.DropTable(
                name: "ItemSpecificationMaster",
                schema: "Inventory");

            migrationBuilder.DropIndex(
                name: "IX_ItemVariantValue_ItemId_VariantAttributeId_SpecificationValueId",
                schema: "Inventory",
                table: "ItemVariantValue");

            migrationBuilder.DropIndex(
                name: "IX_ItemVariantValue_SpecificationValueId",
                schema: "Inventory",
                table: "ItemVariantValue");

            migrationBuilder.DropIndex(
                name: "IX_ItemVariantAttribute_ItemId_SpecificationMasterId",
                schema: "Inventory",
                table: "ItemVariantAttribute");

            migrationBuilder.DropColumn(
                name: "SpecificationValueId",
                schema: "Inventory",
                table: "ItemVariantValue");

            migrationBuilder.RenameColumn(
                name: "SpecificationMasterId",
                schema: "Inventory",
                table: "ItemVariantAttribute",
                newName: "VariantBasedOn");

            migrationBuilder.RenameIndex(
                name: "IX_ItemVariantAttribute_SpecificationMasterId",
                schema: "Inventory",
                table: "ItemVariantAttribute",
                newName: "IX_ItemVariantAttribute_VariantBasedOn");

            migrationBuilder.AddColumn<string>(
                name: "OptionValue",
                schema: "Inventory",
                table: "ItemVariantValue",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<int>(
                name: "AttributeGroupId",
                schema: "Inventory",
                table: "ItemVariantAttribute",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "AttributeId",
                schema: "Inventory",
                table: "ItemVariantAttribute",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateIndex(
                name: "IX_ItemVariantValue_ItemId_VariantAttributeId_OptionValue",
                schema: "Inventory",
                table: "ItemVariantValue",
                columns: new[] { "ItemId", "VariantAttributeId", "OptionValue" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_ItemVariantAttribute_AttributeGroupId",
                schema: "Inventory",
                table: "ItemVariantAttribute",
                column: "AttributeGroupId");

            migrationBuilder.CreateIndex(
                name: "IX_ItemVariantAttribute_AttributeId",
                schema: "Inventory",
                table: "ItemVariantAttribute",
                column: "AttributeId");

            migrationBuilder.CreateIndex(
                name: "IX_ItemVariantAttribute_ItemId_AttributeId",
                schema: "Inventory",
                table: "ItemVariantAttribute",
                columns: new[] { "ItemId", "AttributeId" },
                unique: true);

            migrationBuilder.AddForeignKey(
                name: "FK_ItemVariantAttribute_MiscMaster_AttributeId",
                schema: "Inventory",
                table: "ItemVariantAttribute",
                column: "AttributeId",
                principalSchema: "Inventory",
                principalTable: "MiscMaster",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_ItemVariantAttribute_MiscMaster_VariantBasedOn",
                schema: "Inventory",
                table: "ItemVariantAttribute",
                column: "VariantBasedOn",
                principalSchema: "Inventory",
                principalTable: "MiscMaster",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_ItemVariantAttribute_MiscTypeMaster_AttributeGroupId",
                schema: "Inventory",
                table: "ItemVariantAttribute",
                column: "AttributeGroupId",
                principalSchema: "Inventory",
                principalTable: "MiscTypeMaster",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }
    }
}
