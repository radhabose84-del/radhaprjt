using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SalesManagement.Infrastructure.Data.Migrations
{
    /// <inheritdoc />
    public partial class DiscountMaster : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "DiscountMaster",
                schema: "Sales",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    DiscountCode = table.Column<string>(type: "varchar(20)", nullable: false),
                    DiscountName = table.Column<string>(type: "varchar(100)", nullable: false),
                    DiscountTypeId = table.Column<int>(type: "int", nullable: false),
                    ApplicableLevelId = table.Column<int>(type: "int", nullable: false),
                    TriggerEventId = table.Column<int>(type: "int", nullable: false),
                    RequiresApproval = table.Column<bool>(type: "bit", nullable: false),
                    MaxDiscountLimitTypeId = table.Column<int>(type: "int", nullable: true),
                    ValueTypeId = table.Column<int>(type: "int", nullable: false),
                    DiscountValue = table.Column<decimal>(type: "decimal(18,4)", precision: 18, scale: 6, nullable: true),
                    SlabTypeId = table.Column<int>(type: "int", nullable: true),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false),
                    CreatedBy = table.Column<int>(type: "int", nullable: false),
                    CreatedDate = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true),
                    CreatedByName = table.Column<string>(type: "varchar(100)", nullable: true),
                    CreatedIP = table.Column<string>(type: "varchar(50)", nullable: true),
                    ModifiedBy = table.Column<int>(type: "int", nullable: true),
                    ModifiedDate = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true),
                    ModifiedByName = table.Column<string>(type: "varchar(100)", nullable: true),
                    ModifiedIP = table.Column<string>(type: "varchar(50)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DiscountMaster", x => x.Id);
                    table.ForeignKey(
                        name: "FK_DiscountMaster_MiscMaster_ApplicableLevelId",
                        column: x => x.ApplicableLevelId,
                        principalSchema: "Sales",
                        principalTable: "MiscMaster",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_DiscountMaster_MiscMaster_DiscountTypeId",
                        column: x => x.DiscountTypeId,
                        principalSchema: "Sales",
                        principalTable: "MiscMaster",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_DiscountMaster_MiscMaster_MaxDiscountLimitTypeId",
                        column: x => x.MaxDiscountLimitTypeId,
                        principalSchema: "Sales",
                        principalTable: "MiscMaster",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_DiscountMaster_MiscMaster_SlabTypeId",
                        column: x => x.SlabTypeId,
                        principalSchema: "Sales",
                        principalTable: "MiscMaster",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_DiscountMaster_MiscMaster_TriggerEventId",
                        column: x => x.TriggerEventId,
                        principalSchema: "Sales",
                        principalTable: "MiscMaster",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_DiscountMaster_MiscMaster_ValueTypeId",
                        column: x => x.ValueTypeId,
                        principalSchema: "Sales",
                        principalTable: "MiscMaster",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "DiscountPaymentTerm",
                schema: "Sales",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    DiscountMasterId = table.Column<int>(type: "int", nullable: false),
                    PaymentTermId = table.Column<int>(type: "int", nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false),
                    CreatedBy = table.Column<int>(type: "int", nullable: false),
                    CreatedDate = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true),
                    CreatedByName = table.Column<string>(type: "varchar(100)", nullable: true),
                    CreatedIP = table.Column<string>(type: "varchar(50)", nullable: true),
                    ModifiedBy = table.Column<int>(type: "int", nullable: true),
                    ModifiedDate = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true),
                    ModifiedByName = table.Column<string>(type: "varchar(100)", nullable: true),
                    ModifiedIP = table.Column<string>(type: "varchar(50)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DiscountPaymentTerm", x => x.Id);
                    table.ForeignKey(
                        name: "FK_DiscountPaymentTerm_DiscountMaster_DiscountMasterId",
                        column: x => x.DiscountMasterId,
                        principalSchema: "Sales",
                        principalTable: "DiscountMaster",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "DiscountSalesGroup",
                schema: "Sales",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    DiscountMasterId = table.Column<int>(type: "int", nullable: false),
                    SalesGroupId = table.Column<int>(type: "int", nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false),
                    CreatedBy = table.Column<int>(type: "int", nullable: false),
                    CreatedDate = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true),
                    CreatedByName = table.Column<string>(type: "varchar(100)", nullable: true),
                    CreatedIP = table.Column<string>(type: "varchar(50)", nullable: true),
                    ModifiedBy = table.Column<int>(type: "int", nullable: true),
                    ModifiedDate = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true),
                    ModifiedByName = table.Column<string>(type: "varchar(100)", nullable: true),
                    ModifiedIP = table.Column<string>(type: "varchar(50)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DiscountSalesGroup", x => x.Id);
                    table.ForeignKey(
                        name: "FK_DiscountSalesGroup_DiscountMaster_DiscountMasterId",
                        column: x => x.DiscountMasterId,
                        principalSchema: "Sales",
                        principalTable: "DiscountMaster",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_DiscountSalesGroup_SalesGroup_SalesGroupId",
                        column: x => x.SalesGroupId,
                        principalSchema: "Sales",
                        principalTable: "SalesGroup",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "DiscountSlab",
                schema: "Sales",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    DiscountMasterId = table.Column<int>(type: "int", nullable: false),
                    SlabOrder = table.Column<int>(type: "int", nullable: false),
                    FromValue = table.Column<decimal>(type: "decimal(18,4)", precision: 18, scale: 6, nullable: false),
                    ToValue = table.Column<decimal>(type: "decimal(18,4)", precision: 18, scale: 6, nullable: true),
                    DiscountValue = table.Column<decimal>(type: "decimal(18,4)", precision: 18, scale: 6, nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false),
                    CreatedBy = table.Column<int>(type: "int", nullable: false),
                    CreatedDate = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true),
                    CreatedByName = table.Column<string>(type: "varchar(100)", nullable: true),
                    CreatedIP = table.Column<string>(type: "varchar(50)", nullable: true),
                    ModifiedBy = table.Column<int>(type: "int", nullable: true),
                    ModifiedDate = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true),
                    ModifiedByName = table.Column<string>(type: "varchar(100)", nullable: true),
                    ModifiedIP = table.Column<string>(type: "varchar(50)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DiscountSlab", x => x.Id);
                    table.ForeignKey(
                        name: "FK_DiscountSlab_DiscountMaster_DiscountMasterId",
                        column: x => x.DiscountMasterId,
                        principalSchema: "Sales",
                        principalTable: "DiscountMaster",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_DiscountMaster_ApplicableLevelId",
                schema: "Sales",
                table: "DiscountMaster",
                column: "ApplicableLevelId");

            migrationBuilder.CreateIndex(
                name: "IX_DiscountMaster_DiscountCode",
                schema: "Sales",
                table: "DiscountMaster",
                column: "DiscountCode",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_DiscountMaster_DiscountTypeId",
                schema: "Sales",
                table: "DiscountMaster",
                column: "DiscountTypeId");

            migrationBuilder.CreateIndex(
                name: "IX_DiscountMaster_MaxDiscountLimitTypeId",
                schema: "Sales",
                table: "DiscountMaster",
                column: "MaxDiscountLimitTypeId");

            migrationBuilder.CreateIndex(
                name: "IX_DiscountMaster_SlabTypeId",
                schema: "Sales",
                table: "DiscountMaster",
                column: "SlabTypeId");

            migrationBuilder.CreateIndex(
                name: "IX_DiscountMaster_TriggerEventId",
                schema: "Sales",
                table: "DiscountMaster",
                column: "TriggerEventId");

            migrationBuilder.CreateIndex(
                name: "IX_DiscountMaster_ValueTypeId",
                schema: "Sales",
                table: "DiscountMaster",
                column: "ValueTypeId");

            migrationBuilder.CreateIndex(
                name: "IX_DiscountPaymentTerm_DiscountMasterId",
                schema: "Sales",
                table: "DiscountPaymentTerm",
                column: "DiscountMasterId");

            migrationBuilder.CreateIndex(
                name: "IX_DiscountPaymentTerm_DiscountMasterId_PaymentTermId",
                schema: "Sales",
                table: "DiscountPaymentTerm",
                columns: new[] { "DiscountMasterId", "PaymentTermId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_DiscountPaymentTerm_PaymentTermId",
                schema: "Sales",
                table: "DiscountPaymentTerm",
                column: "PaymentTermId");

            migrationBuilder.CreateIndex(
                name: "IX_DiscountSalesGroup_DiscountMasterId",
                schema: "Sales",
                table: "DiscountSalesGroup",
                column: "DiscountMasterId");

            migrationBuilder.CreateIndex(
                name: "IX_DiscountSalesGroup_DiscountMasterId_SalesGroupId",
                schema: "Sales",
                table: "DiscountSalesGroup",
                columns: new[] { "DiscountMasterId", "SalesGroupId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_DiscountSalesGroup_SalesGroupId",
                schema: "Sales",
                table: "DiscountSalesGroup",
                column: "SalesGroupId");

            migrationBuilder.CreateIndex(
                name: "IX_DiscountSlab_DiscountMasterId",
                schema: "Sales",
                table: "DiscountSlab",
                column: "DiscountMasterId");

            migrationBuilder.CreateIndex(
                name: "IX_DiscountSlab_DiscountMasterId_SlabOrder",
                schema: "Sales",
                table: "DiscountSlab",
                columns: new[] { "DiscountMasterId", "SlabOrder" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "DiscountPaymentTerm",
                schema: "Sales");

            migrationBuilder.DropTable(
                name: "DiscountSalesGroup",
                schema: "Sales");

            migrationBuilder.DropTable(
                name: "DiscountSlab",
                schema: "Sales");

            migrationBuilder.DropTable(
                name: "DiscountMaster",
                schema: "Sales");
        }
    }
}
