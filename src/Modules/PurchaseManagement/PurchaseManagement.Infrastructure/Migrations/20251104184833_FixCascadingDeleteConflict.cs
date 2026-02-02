using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PurchaseManagement.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class FixCascadingDeleteConflict : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "ServiceEntrySheets",
                schema: "Purchase",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    SESDate = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
                    SESStatusId = table.Column<int>(type: "int", nullable: false),
                    PurchaseOrderId = table.Column<int>(type: "int", nullable: false),
                    PODate = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
                    VendorId = table.Column<int>(type: "int", nullable: false),
                    ServiceCategoryId = table.Column<int>(type: "int", nullable: true),
                    ContractTypeId = table.Column<int>(type: "int", nullable: true),
                    ValidityFrom = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true),
                    ValidityTo = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true),
                    UnitId = table.Column<int>(type: "int", nullable: false),
                    AttachmentFileName = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: true),
                    ServiceId = table.Column<int>(type: "int", nullable: false),
                    ServiceDescription = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ScheduleID = table.Column<int>(type: "int", nullable: false),
                    OccurrenceNo = table.Column<int>(type: "int", nullable: false),
                    OccurrencePeriod = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ScheduleStartDate = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
                    ScheduleEndDate = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
                    ActualQuantity = table.Column<decimal>(type: "decimal(18,3)", nullable: true),
                    ActualRate = table.Column<decimal>(type: "decimal(18,2)", nullable: true),
                    ActualValue = table.Column<decimal>(type: "decimal(18,2)", nullable: true),
                    DiscountTypeId = table.Column<int>(type: "int", nullable: true),
                    DiscountValue = table.Column<decimal>(type: "decimal(18,2)", nullable: true),
                    TaxPercentage = table.Column<decimal>(type: "decimal(18,2)", nullable: true),
                    TaxValue = table.Column<decimal>(type: "decimal(18,2)", nullable: true),
                    TotalValue = table.Column<decimal>(type: "decimal(18,2)", nullable: true),
                    WorkStartDate = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true),
                    WorkEndDate = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true),
                    DurationHrs = table.Column<decimal>(type: "decimal(18,2)", nullable: true),
                    LineRemarks = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true),
                    CompletionStatus = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    MiscMasterId = table.Column<int>(type: "int", nullable: true),
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
                    table.PrimaryKey("PK_ServiceEntrySheets", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ServiceEntrySheets_MiscMaster_ContractTypeId",
                        column: x => x.ContractTypeId,
                        principalSchema: "Purchase",
                        principalTable: "MiscMaster",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_ServiceEntrySheets_MiscMaster_DiscountTypeId",
                        column: x => x.DiscountTypeId,
                        principalSchema: "Purchase",
                        principalTable: "MiscMaster",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_ServiceEntrySheets_MiscMaster_MiscMasterId",
                        column: x => x.MiscMasterId,
                        principalSchema: "Purchase",
                        principalTable: "MiscMaster",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_ServiceEntrySheets_MiscMaster_SESStatusId",
                        column: x => x.SESStatusId,
                        principalSchema: "Purchase",
                        principalTable: "MiscMaster",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_ServiceEntrySheets_MiscMaster_ServiceCategoryId",
                        column: x => x.ServiceCategoryId,
                        principalSchema: "Purchase",
                        principalTable: "MiscMaster",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_ServiceEntrySheets_PurchaseOrderHeader_PurchaseOrderId",
                        column: x => x.PurchaseOrderId,
                        principalSchema: "Purchase",
                        principalTable: "PurchaseOrderHeader",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "ServiceEntryActivities",
                schema: "Purchase",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    EntrySheetId = table.Column<int>(type: "int", nullable: false),
                    ActivityTypeId = table.Column<int>(type: "int", nullable: true),
                    Description = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true),
                    PerformedById = table.Column<int>(type: "int", nullable: false),
                    PerformedByName = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    SESActivityStatusId = table.Column<int>(type: "int", nullable: true),
                    StatusRemarks = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
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
                    table.PrimaryKey("PK_ServiceEntryActivities", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ServiceEntryActivities_MiscMaster_ActivityTypeId",
                        column: x => x.ActivityTypeId,
                        principalSchema: "Purchase",
                        principalTable: "MiscMaster",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_ServiceEntryActivities_MiscMaster_SESActivityStatusId",
                        column: x => x.SESActivityStatusId,
                        principalSchema: "Purchase",
                        principalTable: "MiscMaster",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_ServiceEntryActivities_ServiceEntrySheets_EntrySheetId",
                        column: x => x.EntrySheetId,
                        principalSchema: "Purchase",
                        principalTable: "ServiceEntrySheets",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_ServiceEntryActivities_ActivityTypeId",
                schema: "Purchase",
                table: "ServiceEntryActivities",
                column: "ActivityTypeId");

            migrationBuilder.CreateIndex(
                name: "IX_ServiceEntryActivities_EntrySheetId",
                schema: "Purchase",
                table: "ServiceEntryActivities",
                column: "EntrySheetId");

            migrationBuilder.CreateIndex(
                name: "IX_ServiceEntryActivities_PerformedById",
                schema: "Purchase",
                table: "ServiceEntryActivities",
                column: "PerformedById");

            migrationBuilder.CreateIndex(
                name: "IX_ServiceEntryActivities_SESActivityStatusId",
                schema: "Purchase",
                table: "ServiceEntryActivities",
                column: "SESActivityStatusId");

            migrationBuilder.CreateIndex(
                name: "IX_ServiceEntrySheets_ContractTypeId",
                schema: "Purchase",
                table: "ServiceEntrySheets",
                column: "ContractTypeId");

            migrationBuilder.CreateIndex(
                name: "IX_ServiceEntrySheets_DiscountTypeId",
                schema: "Purchase",
                table: "ServiceEntrySheets",
                column: "DiscountTypeId");

            migrationBuilder.CreateIndex(
                name: "IX_ServiceEntrySheets_MiscMasterId",
                schema: "Purchase",
                table: "ServiceEntrySheets",
                column: "MiscMasterId");

            migrationBuilder.CreateIndex(
                name: "IX_ServiceEntrySheets_PurchaseOrderId",
                schema: "Purchase",
                table: "ServiceEntrySheets",
                column: "PurchaseOrderId");

            migrationBuilder.CreateIndex(
                name: "IX_ServiceEntrySheets_ServiceCategoryId",
                schema: "Purchase",
                table: "ServiceEntrySheets",
                column: "ServiceCategoryId");

            migrationBuilder.CreateIndex(
                name: "IX_ServiceEntrySheets_SESStatusId",
                schema: "Purchase",
                table: "ServiceEntrySheets",
                column: "SESStatusId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ServiceEntryActivities",
                schema: "Purchase");

            migrationBuilder.DropTable(
                name: "ServiceEntrySheets",
                schema: "Purchase");
        }
    }
}
