using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace InventoryManagement.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class ItemMasters : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "ItemLogs",
                schema: "Inventory",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    CreatedDate = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false, defaultValueSql: "SYSUTCDATETIME()"),
                    EntityName = table.Column<string>(type: "varchar(128)", nullable: false),
                    EntityId = table.Column<int>(type: "int", nullable: false),
                    Action = table.Column<string>(type: "varchar(32)", nullable: false, defaultValue: "Update"),
                    ChangesJson = table.Column<string>(type: "nvarchar(max)", nullable: false, defaultValue: "{}"),
                    CreatedBy = table.Column<int>(type: "int", nullable: true),
                    CreatedByName = table.Column<string>(type: "varchar(64)", nullable: true),
                    CreatedIP = table.Column<string>(type: "varchar(45)", nullable: true),
                    CorrelationId = table.Column<string>(type: "varchar(64)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ItemLogs", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "ItemMaster",
                schema: "Inventory",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    UnitId = table.Column<int>(type: "int", nullable: false),
                    ItemCode = table.Column<string>(type: "varchar(50)", nullable: false),
                    ItemName = table.Column<string>(type: "varchar(200)", nullable: false),
                    HSNId = table.Column<int>(type: "int", nullable: true),
                    ItemGroupId = table.Column<int>(type: "int", nullable: true),
                    ItemCategoryId = table.Column<int>(type: "int", nullable: true),
                    StockUomId = table.Column<int>(type: "int", nullable: true),
                    ItemClassificationId = table.Column<int>(type: "int", nullable: true),
                    Description = table.Column<string>(type: "varchar(500)", nullable: true),
                    ValidFrom = table.Column<DateOnly>(type: "date", nullable: true),
                    XPlantMaterialStatusId = table.Column<int>(type: "int", nullable: true),
                    IsStockItem = table.Column<bool>(type: "bit", nullable: false),
                    MaintainStock = table.Column<bool>(type: "bit", nullable: false),
                    HasVariants = table.Column<bool>(type: "bit", nullable: false),
                    ParentItemId = table.Column<int>(type: "int", nullable: true),
                    ItemImage = table.Column<string>(type: "nvarchar(255)", nullable: true),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false),
                    CreatedBy = table.Column<int>(type: "int", nullable: false),
                    CreatedDate = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true),
                    CreatedByName = table.Column<string>(type: "varchar(50)", nullable: true),
                    CreatedIP = table.Column<string>(type: "varchar(50)", nullable: true),
                    ModifiedBy = table.Column<int>(type: "int", nullable: true),
                    ModifiedDate = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true),
                    ModifiedByName = table.Column<string>(type: "varchar(50)", nullable: true),
                    ModifiedIP = table.Column<string>(type: "varchar(50)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ItemMaster", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ItemMaster_HSNMaster_HSNId",
                        column: x => x.HSNId,
                        principalSchema: "Inventory",
                        principalTable: "HSNMaster",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_ItemMaster_ItemCategory_ItemCategoryId",
                        column: x => x.ItemCategoryId,
                        principalSchema: "Inventory",
                        principalTable: "ItemCategory",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_ItemMaster_ItemGroup_ItemGroupId",
                        column: x => x.ItemGroupId,
                        principalSchema: "Inventory",
                        principalTable: "ItemGroup",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_ItemMaster_ItemMaster_ParentItemId",
                        column: x => x.ParentItemId,
                        principalSchema: "Inventory",
                        principalTable: "ItemMaster",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_ItemMaster_MiscMaster_ItemClassificationId",
                        column: x => x.ItemClassificationId,
                        principalSchema: "Inventory",
                        principalTable: "MiscMaster",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_ItemMaster_MiscMaster_XPlantMaterialStatusId",
                        column: x => x.XPlantMaterialStatusId,
                        principalSchema: "Inventory",
                        principalTable: "MiscMaster",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_ItemMaster_UOM_StockUomId",
                        column: x => x.StockUomId,
                        principalSchema: "Inventory",
                        principalTable: "UOM",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "ItemInventory",
                schema: "Inventory",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ItemId = table.Column<int>(type: "int", nullable: false),
                    Weight = table.Column<decimal>(type: "decimal(18,4)", nullable: true),
                    WeightUomId = table.Column<int>(type: "int", nullable: true),
                    DefaultMaterialRequestTypeId = table.Column<int>(type: "int", nullable: true),
                    ValuationMethodId = table.Column<int>(type: "int", nullable: true),
                    ShelfLife = table.Column<int>(type: "int", nullable: true),
                    UpperTolerance = table.Column<decimal>(type: "decimal(18,4)", nullable: true),
                    LowerTolerance = table.Column<decimal>(type: "decimal(18,4)", nullable: true),
                    BatchNumberSeries = table.Column<string>(type: "varchar(100)", nullable: true),
                    SerialNumberSeries = table.Column<string>(type: "varchar(100)", nullable: true),
                    ReorderLevel = table.Column<int>(type: "int", nullable: true),
                    ReorderQty = table.Column<int>(type: "int", nullable: true),
                    RequestTypeId = table.Column<int>(type: "int", nullable: true),
                    AllowNegativeStock = table.Column<bool>(type: "bit", nullable: false),
                    BatchManagement = table.Column<bool>(type: "bit", nullable: false),
                    ApplyBatchNumber = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ItemInventory", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ItemInventory_ItemMaster_ItemId",
                        column: x => x.ItemId,
                        principalSchema: "Inventory",
                        principalTable: "ItemMaster",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_ItemInventory_MiscMaster_DefaultMaterialRequestTypeId",
                        column: x => x.DefaultMaterialRequestTypeId,
                        principalSchema: "Inventory",
                        principalTable: "MiscMaster",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_ItemInventory_MiscMaster_RequestTypeId",
                        column: x => x.RequestTypeId,
                        principalSchema: "Inventory",
                        principalTable: "MiscMaster",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_ItemInventory_MiscMaster_ValuationMethodId",
                        column: x => x.ValuationMethodId,
                        principalSchema: "Inventory",
                        principalTable: "MiscMaster",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_ItemInventory_UOM_WeightUomId",
                        column: x => x.WeightUomId,
                        principalSchema: "Inventory",
                        principalTable: "UOM",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "ItemManufacture",
                schema: "Inventory",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ItemId = table.Column<int>(type: "int", nullable: false),
                    UnitId = table.Column<int>(type: "int", nullable: false),
                    ManufacturingTypeId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ItemManufacture", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ItemManufacture_ItemMaster_ItemId",
                        column: x => x.ItemId,
                        principalSchema: "Inventory",
                        principalTable: "ItemMaster",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_ItemManufacture_MiscMaster_ManufacturingTypeId",
                        column: x => x.ManufacturingTypeId,
                        principalSchema: "Inventory",
                        principalTable: "MiscMaster",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "ItemPurchase",
                schema: "Inventory",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ItemId = table.Column<int>(type: "int", nullable: false),
                    PurchaseUomId = table.Column<int>(type: "int", nullable: true),
                    LeadTimeDays = table.Column<int>(type: "int", nullable: true),
                    SafetyStock = table.Column<int>(type: "int", nullable: true),
                    GrProcessingTimeDays = table.Column<int>(type: "int", nullable: true),
                    AutomaticPo = table.Column<bool>(type: "bit", nullable: false),
                    OriginCountryId = table.Column<int>(type: "int", nullable: true),
                    TariffNumber = table.Column<string>(type: "varchar(50)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ItemPurchase", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ItemPurchase_ItemMaster_ItemId",
                        column: x => x.ItemId,
                        principalSchema: "Inventory",
                        principalTable: "ItemMaster",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_ItemPurchase_UOM_PurchaseUomId",
                        column: x => x.PurchaseUomId,
                        principalSchema: "Inventory",
                        principalTable: "UOM",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "ItemQuality",
                schema: "Inventory",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ItemId = table.Column<int>(type: "int", nullable: false),
                    InspectionTemplateId = table.Column<int>(type: "int", nullable: true),
                    CertificateTypeId = table.Column<int>(type: "int", nullable: true),
                    InspLotProcessingTime = table.Column<int>(type: "int", nullable: true),
                    InspectionRequired = table.Column<bool>(type: "bit", nullable: false),
                    QualityInspectionFree = table.Column<bool>(type: "bit", nullable: false),
                    IsCertificateRequiredFromSupplier = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ItemQuality", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ItemQuality_ItemMaster_ItemId",
                        column: x => x.ItemId,
                        principalSchema: "Inventory",
                        principalTable: "ItemMaster",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_ItemQuality_MiscMaster_CertificateTypeId",
                        column: x => x.CertificateTypeId,
                        principalSchema: "Inventory",
                        principalTable: "MiscMaster",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "ItemSupplier",
                schema: "Inventory",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ItemId = table.Column<int>(type: "int", nullable: false),
                    SupplierId = table.Column<int>(type: "int", nullable: false),
                    UnitId = table.Column<int>(type: "int", nullable: false),
                    SupplierPartNo = table.Column<string>(type: "varchar(100)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ItemSupplier", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ItemSupplier_ItemMaster_ItemId",
                        column: x => x.ItemId,
                        principalSchema: "Inventory",
                        principalTable: "ItemMaster",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "ItemUOM",
                schema: "Inventory",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ItemId = table.Column<int>(type: "int", nullable: false),
                    ConversionUOMId = table.Column<int>(type: "int", nullable: false),
                    ConversionRate = table.Column<decimal>(type: "decimal(18,6)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ItemUOM", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ItemUOM_ItemMaster_ItemId",
                        column: x => x.ItemId,
                        principalSchema: "Inventory",
                        principalTable: "ItemMaster",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_ItemUOM_UOM_ConversionUOMId",
                        column: x => x.ConversionUOMId,
                        principalSchema: "Inventory",
                        principalTable: "UOM",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "ItemVariantValue",
                schema: "Inventory",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ItemId = table.Column<int>(type: "int", nullable: false),
                    VariantBasedOn = table.Column<int>(type: "int", nullable: false),
                    AttributeId = table.Column<int>(type: "int", nullable: false),
                    OptionValue = table.Column<string>(type: "varchar(100)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ItemVariantValue", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ItemVariantValue_ItemMaster_ItemId",
                        column: x => x.ItemId,
                        principalSchema: "Inventory",
                        principalTable: "ItemMaster",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_ItemVariantValue_MiscMaster_AttributeId",
                        column: x => x.AttributeId,
                        principalSchema: "Inventory",
                        principalTable: "MiscMaster",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_ItemVariantValue_MiscMaster_VariantBasedOn",
                        column: x => x.VariantBasedOn,
                        principalSchema: "Inventory",
                        principalTable: "MiscMaster",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateIndex(
                name: "IX_ItemInventory_DefaultMaterialRequestTypeId",
                schema: "Inventory",
                table: "ItemInventory",
                column: "DefaultMaterialRequestTypeId");

            migrationBuilder.CreateIndex(
                name: "IX_ItemInventory_ItemId",
                schema: "Inventory",
                table: "ItemInventory",
                column: "ItemId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_ItemInventory_RequestTypeId",
                schema: "Inventory",
                table: "ItemInventory",
                column: "RequestTypeId");

            migrationBuilder.CreateIndex(
                name: "IX_ItemInventory_ValuationMethodId",
                schema: "Inventory",
                table: "ItemInventory",
                column: "ValuationMethodId");

            migrationBuilder.CreateIndex(
                name: "IX_ItemInventory_WeightUomId",
                schema: "Inventory",
                table: "ItemInventory",
                column: "WeightUomId");

            migrationBuilder.CreateIndex(
                name: "IX_ItemLogs_CorrelationId",
                schema: "Inventory",
                table: "ItemLogs",
                column: "CorrelationId");

            migrationBuilder.CreateIndex(
                name: "IX_ItemLogs_Entity_ThenDate",
                schema: "Inventory",
                table: "ItemLogs",
                columns: new[] { "EntityName", "EntityId", "CreatedDate" });

            migrationBuilder.CreateIndex(
                name: "IX_ItemManufacture_ItemId",
                schema: "Inventory",
                table: "ItemManufacture",
                column: "ItemId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_ItemManufacture_ManufacturingTypeId",
                schema: "Inventory",
                table: "ItemManufacture",
                column: "ManufacturingTypeId");

            migrationBuilder.CreateIndex(
                name: "IX_ItemMaster_HSNId",
                schema: "Inventory",
                table: "ItemMaster",
                column: "HSNId");

            migrationBuilder.CreateIndex(
                name: "IX_ItemMaster_ItemCategoryId",
                schema: "Inventory",
                table: "ItemMaster",
                column: "ItemCategoryId");

            migrationBuilder.CreateIndex(
                name: "IX_ItemMaster_ItemClassificationId",
                schema: "Inventory",
                table: "ItemMaster",
                column: "ItemClassificationId");

            migrationBuilder.CreateIndex(
                name: "IX_ItemMaster_ItemGroupId",
                schema: "Inventory",
                table: "ItemMaster",
                column: "ItemGroupId");

            migrationBuilder.CreateIndex(
                name: "IX_ItemMaster_ParentItemId",
                schema: "Inventory",
                table: "ItemMaster",
                column: "ParentItemId");

            migrationBuilder.CreateIndex(
                name: "IX_ItemMaster_StockUomId",
                schema: "Inventory",
                table: "ItemMaster",
                column: "StockUomId");

            migrationBuilder.CreateIndex(
                name: "IX_ItemMaster_XPlantMaterialStatusId",
                schema: "Inventory",
                table: "ItemMaster",
                column: "XPlantMaterialStatusId");

            migrationBuilder.CreateIndex(
                name: "IX_ItemPurchase_ItemId",
                schema: "Inventory",
                table: "ItemPurchase",
                column: "ItemId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_ItemPurchase_PurchaseUomId",
                schema: "Inventory",
                table: "ItemPurchase",
                column: "PurchaseUomId");

            migrationBuilder.CreateIndex(
                name: "IX_ItemQuality_CertificateTypeId",
                schema: "Inventory",
                table: "ItemQuality",
                column: "CertificateTypeId");

            migrationBuilder.CreateIndex(
                name: "IX_ItemQuality_ItemId",
                schema: "Inventory",
                table: "ItemQuality",
                column: "ItemId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_ItemSupplier_ItemId",
                schema: "Inventory",
                table: "ItemSupplier",
                column: "ItemId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_ItemUOM_ConversionUOMId",
                schema: "Inventory",
                table: "ItemUOM",
                column: "ConversionUOMId");

            migrationBuilder.CreateIndex(
                name: "IX_ItemUOM_ItemId",
                schema: "Inventory",
                table: "ItemUOM",
                column: "ItemId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_ItemVariantValue_AttributeId",
                schema: "Inventory",
                table: "ItemVariantValue",
                column: "AttributeId");

            migrationBuilder.CreateIndex(
                name: "IX_ItemVariantValue_ItemId",
                schema: "Inventory",
                table: "ItemVariantValue",
                column: "ItemId");

            migrationBuilder.CreateIndex(
                name: "IX_ItemVariantValue_VariantBasedOn",
                schema: "Inventory",
                table: "ItemVariantValue",
                column: "VariantBasedOn");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ItemInventory",
                schema: "Inventory");

            migrationBuilder.DropTable(
                name: "ItemLogs",
                schema: "Inventory");

            migrationBuilder.DropTable(
                name: "ItemManufacture",
                schema: "Inventory");

            migrationBuilder.DropTable(
                name: "ItemPurchase",
                schema: "Inventory");

            migrationBuilder.DropTable(
                name: "ItemQuality",
                schema: "Inventory");

            migrationBuilder.DropTable(
                name: "ItemSupplier",
                schema: "Inventory");

            migrationBuilder.DropTable(
                name: "ItemUOM",
                schema: "Inventory");

            migrationBuilder.DropTable(
                name: "ItemVariantValue",
                schema: "Inventory");

            migrationBuilder.DropTable(
                name: "ItemMaster",
                schema: "Inventory");
        }
    }
}
