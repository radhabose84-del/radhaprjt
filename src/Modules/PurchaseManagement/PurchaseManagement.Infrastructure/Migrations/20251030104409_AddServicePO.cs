using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PurchaseManagement.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddServicePO : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
          

            migrationBuilder.CreateTable(
                name: "PurchaseOrderServiceHeader",
                schema: "Purchase",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    PurchaseOrderId = table.Column<int>(type: "int", nullable: false),
                    ServiceCategoryId = table.Column<int>(type: "int", nullable: false),
                    ContractTypeId = table.Column<int>(type: "int", nullable: false),
                    FrequencyId = table.Column<int>(type: "int", nullable: false),
                    ValidityFrom = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
                    ValidityTo = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
                    TotalOccurrences = table.Column<int>(type: "int", nullable: false),
                    OverallLimit = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: true),
                    SLATerms = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    AttachmentPath = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
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
                    table.PrimaryKey("PK_PurchaseOrderServiceHeader", x => x.Id);
                    table.ForeignKey(
                        name: "FK_PurchaseOrderServiceHeader_MiscMaster_ContractTypeId",
                        column: x => x.ContractTypeId,
                        principalSchema: "Purchase",
                        principalTable: "MiscMaster",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_PurchaseOrderServiceHeader_MiscMaster_FrequencyId",
                        column: x => x.FrequencyId,
                        principalSchema: "Purchase",
                        principalTable: "MiscMaster",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_PurchaseOrderServiceHeader_MiscMaster_ServiceCategoryId",
                        column: x => x.ServiceCategoryId,
                        principalSchema: "Purchase",
                        principalTable: "MiscMaster",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_PurchaseOrderServiceHeader_PurchaseOrderHeader_PurchaseOrderId",
                        column: x => x.PurchaseOrderId,
                        principalSchema: "Purchase",
                        principalTable: "PurchaseOrderHeader",
                        principalColumn: "Id");
                });

          
            migrationBuilder.CreateTable(
                name: "PurchaseOrderServiceLine",
                schema: "Purchase",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    PurchaseOrderId = table.Column<int>(type: "int", nullable: false),
                    ServicePoHeaderId = table.Column<int>(type: "int", nullable: false),
                    LineNo = table.Column<int>(type: "int", nullable: false),
                    RequestId = table.Column<int>(type: "int", nullable: true),
                    ServiceId = table.Column<int>(type: "int", nullable: true),
                    ServiceDescription = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    UOMId = table.Column<int>(type: "int", nullable: true),
                    PlannedQuantity = table.Column<decimal>(type: "decimal(18,3)", precision: 18, scale: 3, nullable: false),
                    PlannedRate = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    DiscountType = table.Column<string>(type: "nvarchar(1)", maxLength: 1, nullable: true),
                    Discount = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: true),
                    ItemCost = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: true),
                    OtherCost = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: true),
                    OtherCharges = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: true),
                    GstPercent = table.Column<decimal>(type: "decimal(5,2)", precision: 5, scale: 2, nullable: true),
                    Remarks = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
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
                    table.PrimaryKey("PK_PurchaseOrderServiceLine", x => x.Id);
                    table.ForeignKey(
                        name: "FK_PurchaseOrderServiceLine_PurchaseOrderServiceHeader_ServicePoHeaderId",
                        column: x => x.ServicePoHeaderId,
                        principalSchema: "Purchase",
                        principalTable: "PurchaseOrderServiceHeader",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "PurchaseOrderServiceSchedule",
                schema: "Purchase",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    PurchaseOrderId = table.Column<int>(type: "int", nullable: false),
                    ServicePoHeaderId = table.Column<int>(type: "int", nullable: false),
                    ServicePoHeaderId1 = table.Column<int>(type: "int", nullable: true),
                    ServiceItemId = table.Column<int>(type: "int", nullable: false),
                    ScheduleNo = table.Column<int>(type: "int", nullable: false),
                    OccurrencePeriod = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    OccurrenceDescription = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    ActivityTypeId = table.Column<int>(type: "int", nullable: true),
                    PlannedDurationHrs = table.Column<decimal>(type: "decimal(9,2)", precision: 9, scale: 2, nullable: true),
                    DueDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    ServiceStartDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    ServiceEndDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    PlannedQuantity = table.Column<decimal>(type: "decimal(18,3)", precision: 18, scale: 3, nullable: true),
                    PlannedRate = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: true),
                    PlannedValue = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: true),
                    AutoGenerateSES = table.Column<bool>(type: "bit", nullable: false),
                    Remarks = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
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
                    table.PrimaryKey("PK_PurchaseOrderServiceSchedule", x => x.Id);
                    table.ForeignKey(
                        name: "FK_PurchaseOrderServiceSchedule_PurchaseOrderServiceHeader_ServicePoHeaderId",
                        column: x => x.ServicePoHeaderId,
                        principalSchema: "Purchase",
                        principalTable: "PurchaseOrderServiceHeader",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_PurchaseOrderServiceSchedule_PurchaseOrderServiceHeader_ServicePoHeaderId1",
                        column: x => x.ServicePoHeaderId1,
                        principalSchema: "Purchase",
                        principalTable: "PurchaseOrderServiceHeader",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_PurchaseOrderServiceSchedule_PurchaseOrderServiceLine_ServiceItemId",
                        column: x => x.ServiceItemId,
                        principalSchema: "Purchase",
                        principalTable: "PurchaseOrderServiceLine",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_PurchaseOrderServiceHeader_ContractTypeId",
                schema: "Purchase",
                table: "PurchaseOrderServiceHeader",
                column: "ContractTypeId");

            migrationBuilder.CreateIndex(
                name: "IX_PurchaseOrderServiceHeader_FrequencyId",
                schema: "Purchase",
                table: "PurchaseOrderServiceHeader",
                column: "FrequencyId");

            migrationBuilder.CreateIndex(
                name: "IX_PurchaseOrderServiceHeader_ServiceCategoryId",
                schema: "Purchase",
                table: "PurchaseOrderServiceHeader",
                column: "ServiceCategoryId");

            migrationBuilder.CreateIndex(
                name: "UX_PurchaseOrderServiceHeader_PurchaseOrderId",
                schema: "Purchase",
                table: "PurchaseOrderServiceHeader",
                column: "PurchaseOrderId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_PurchaseOrderServiceLine_ServicePoHeaderId",
                schema: "Purchase",
                table: "PurchaseOrderServiceLine",
                column: "ServicePoHeaderId");

            migrationBuilder.CreateIndex(
                name: "IX_PurchaseOrderServiceSchedule_ServiceItemId",
                schema: "Purchase",
                table: "PurchaseOrderServiceSchedule",
                column: "ServiceItemId");

            migrationBuilder.CreateIndex(
                name: "IX_PurchaseOrderServiceSchedule_ServicePoHeaderId",
                schema: "Purchase",
                table: "PurchaseOrderServiceSchedule",
                column: "ServicePoHeaderId");

            migrationBuilder.CreateIndex(
                name: "IX_PurchaseOrderServiceSchedule_ServicePoHeaderId1",
                schema: "Purchase",
                table: "PurchaseOrderServiceSchedule",
                column: "ServicePoHeaderId1");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "PurchaseOrderServiceSchedule",
                schema: "Purchase");

           

            migrationBuilder.DropTable(
                name: "PurchaseOrderServiceLine",
                schema: "Purchase");

            migrationBuilder.DropTable(
                name: "PurchaseOrderServiceHeader",
                schema: "Purchase");


        

           

          

         


     
        }
    }
}
