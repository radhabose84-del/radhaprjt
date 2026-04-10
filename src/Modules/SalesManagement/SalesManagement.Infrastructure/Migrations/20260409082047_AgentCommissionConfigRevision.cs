using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SalesManagement.Infrastructure.Data.Migrations
{
    /// <inheritdoc />
    public partial class AgentCommissionConfigRevision : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_AgentCommissionConfig_SalesSegment_SalesSegmentId",
                schema: "Sales",
                table: "AgentCommissionConfig");

            migrationBuilder.DropIndex(
                name: "IX_AgentCommissionConfig_AgentId_SalesSegmentId",
                schema: "Sales",
                table: "AgentCommissionConfig");

            migrationBuilder.RenameColumn(
                name: "SalesSegmentId",
                schema: "Sales",
                table: "AgentCommissionConfig",
                newName: "TriggerEventId");

            migrationBuilder.RenameColumn(
                name: "CurrencyId",
                schema: "Sales",
                table: "AgentCommissionConfig",
                newName: "SlabTypeId");

            migrationBuilder.RenameIndex(
                name: "IX_AgentCommissionConfig_SalesSegmentId",
                schema: "Sales",
                table: "AgentCommissionConfig",
                newName: "IX_AgentCommissionConfig_TriggerEventId");

            migrationBuilder.AlterColumn<DateTimeOffset>(
                name: "ValidityTo",
                schema: "Sales",
                table: "AgentCommissionConfig",
                type: "datetimeoffset",
                nullable: true,
                oldClrType: typeof(DateTimeOffset),
                oldType: "datetimeoffset");

            migrationBuilder.AlterColumn<int>(
                name: "CommissionBasisId",
                schema: "Sales",
                table: "AgentCommissionConfig",
                type: "int",
                nullable: false,
                defaultValue: 0,
                oldClrType: typeof(int),
                oldType: "int",
                oldNullable: true);

            migrationBuilder.AlterColumn<int>(
                name: "ApplicableLevelId",
                schema: "Sales",
                table: "AgentCommissionConfig",
                type: "int",
                nullable: false,
                defaultValue: 0,
                oldClrType: typeof(int),
                oldType: "int",
                oldNullable: true);

            migrationBuilder.AddColumn<int>(
                name: "CommissionSplitId",
                schema: "Sales",
                table: "AgentCommissionConfig",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateTable(
                name: "AgentCommissionPaymentTerm",
                schema: "Sales",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    AgentCommissionConfigId = table.Column<int>(type: "int", nullable: false),
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
                    table.PrimaryKey("PK_AgentCommissionPaymentTerm", x => x.Id);
                    table.ForeignKey(
                        name: "FK_AgentCommissionPaymentTerm_AgentCommissionConfig_AgentCommissionConfigId",
                        column: x => x.AgentCommissionConfigId,
                        principalSchema: "Sales",
                        principalTable: "AgentCommissionConfig",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "AgentCommissionSalesGroup",
                schema: "Sales",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    AgentCommissionConfigId = table.Column<int>(type: "int", nullable: false),
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
                    table.PrimaryKey("PK_AgentCommissionSalesGroup", x => x.Id);
                    table.ForeignKey(
                        name: "FK_AgentCommissionSalesGroup_AgentCommissionConfig_AgentCommissionConfigId",
                        column: x => x.AgentCommissionConfigId,
                        principalSchema: "Sales",
                        principalTable: "AgentCommissionConfig",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_AgentCommissionSalesGroup_SalesGroup_SalesGroupId",
                        column: x => x.SalesGroupId,
                        principalSchema: "Sales",
                        principalTable: "SalesGroup",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "AgentCommissionSlab",
                schema: "Sales",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    AgentCommissionConfigId = table.Column<int>(type: "int", nullable: false),
                    SlabOrder = table.Column<int>(type: "int", nullable: false),
                    FromDelay = table.Column<int>(type: "int", nullable: false),
                    ToDelay = table.Column<int>(type: "int", nullable: true),
                    CommissionTypeId = table.Column<int>(type: "int", nullable: false),
                    CommissionBasisId = table.Column<int>(type: "int", nullable: false),
                    CommissionValue = table.Column<decimal>(type: "decimal(18,4)", precision: 18, scale: 6, nullable: false),
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
                    table.PrimaryKey("PK_AgentCommissionSlab", x => x.Id);
                    table.ForeignKey(
                        name: "FK_AgentCommissionSlab_AgentCommissionConfig_AgentCommissionConfigId",
                        column: x => x.AgentCommissionConfigId,
                        principalSchema: "Sales",
                        principalTable: "AgentCommissionConfig",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_AgentCommissionSlab_MiscMaster_CommissionBasisId",
                        column: x => x.CommissionBasisId,
                        principalSchema: "Sales",
                        principalTable: "MiscMaster",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_AgentCommissionSlab_MiscMaster_CommissionTypeId",
                        column: x => x.CommissionTypeId,
                        principalSchema: "Sales",
                        principalTable: "MiscMaster",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_AgentCommissionConfig_AgentId_CommissionSplitId",
                schema: "Sales",
                table: "AgentCommissionConfig",
                columns: new[] { "AgentId", "CommissionSplitId" });

            migrationBuilder.CreateIndex(
                name: "IX_AgentCommissionConfig_CommissionSplitId",
                schema: "Sales",
                table: "AgentCommissionConfig",
                column: "CommissionSplitId");

            migrationBuilder.CreateIndex(
                name: "IX_AgentCommissionConfig_SlabTypeId",
                schema: "Sales",
                table: "AgentCommissionConfig",
                column: "SlabTypeId");

            migrationBuilder.CreateIndex(
                name: "IX_AgentCommissionPaymentTerm_AgentCommissionConfigId",
                schema: "Sales",
                table: "AgentCommissionPaymentTerm",
                column: "AgentCommissionConfigId");

            migrationBuilder.CreateIndex(
                name: "IX_AgentCommissionPaymentTerm_AgentCommissionConfigId_PaymentTermId",
                schema: "Sales",
                table: "AgentCommissionPaymentTerm",
                columns: new[] { "AgentCommissionConfigId", "PaymentTermId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_AgentCommissionPaymentTerm_PaymentTermId",
                schema: "Sales",
                table: "AgentCommissionPaymentTerm",
                column: "PaymentTermId");

            migrationBuilder.CreateIndex(
                name: "IX_AgentCommissionSalesGroup_AgentCommissionConfigId",
                schema: "Sales",
                table: "AgentCommissionSalesGroup",
                column: "AgentCommissionConfigId");

            migrationBuilder.CreateIndex(
                name: "IX_AgentCommissionSalesGroup_AgentCommissionConfigId_SalesGroupId",
                schema: "Sales",
                table: "AgentCommissionSalesGroup",
                columns: new[] { "AgentCommissionConfigId", "SalesGroupId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_AgentCommissionSalesGroup_SalesGroupId",
                schema: "Sales",
                table: "AgentCommissionSalesGroup",
                column: "SalesGroupId");

            migrationBuilder.CreateIndex(
                name: "IX_AgentCommissionSlab_AgentCommissionConfigId",
                schema: "Sales",
                table: "AgentCommissionSlab",
                column: "AgentCommissionConfigId");

            migrationBuilder.CreateIndex(
                name: "IX_AgentCommissionSlab_AgentCommissionConfigId_SlabOrder",
                schema: "Sales",
                table: "AgentCommissionSlab",
                columns: new[] { "AgentCommissionConfigId", "SlabOrder" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_AgentCommissionSlab_CommissionBasisId",
                schema: "Sales",
                table: "AgentCommissionSlab",
                column: "CommissionBasisId");

            migrationBuilder.CreateIndex(
                name: "IX_AgentCommissionSlab_CommissionTypeId",
                schema: "Sales",
                table: "AgentCommissionSlab",
                column: "CommissionTypeId");

            migrationBuilder.AddForeignKey(
                name: "FK_AgentCommissionConfig_CommissionSplit_CommissionSplitId",
                schema: "Sales",
                table: "AgentCommissionConfig",
                column: "CommissionSplitId",
                principalSchema: "Sales",
                principalTable: "CommissionSplit",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_AgentCommissionConfig_MiscMaster_SlabTypeId",
                schema: "Sales",
                table: "AgentCommissionConfig",
                column: "SlabTypeId",
                principalSchema: "Sales",
                principalTable: "MiscMaster",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_AgentCommissionConfig_MiscMaster_TriggerEventId",
                schema: "Sales",
                table: "AgentCommissionConfig",
                column: "TriggerEventId",
                principalSchema: "Sales",
                principalTable: "MiscMaster",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_AgentCommissionConfig_CommissionSplit_CommissionSplitId",
                schema: "Sales",
                table: "AgentCommissionConfig");

            migrationBuilder.DropForeignKey(
                name: "FK_AgentCommissionConfig_MiscMaster_SlabTypeId",
                schema: "Sales",
                table: "AgentCommissionConfig");

            migrationBuilder.DropForeignKey(
                name: "FK_AgentCommissionConfig_MiscMaster_TriggerEventId",
                schema: "Sales",
                table: "AgentCommissionConfig");

            migrationBuilder.DropTable(
                name: "AgentCommissionPaymentTerm",
                schema: "Sales");

            migrationBuilder.DropTable(
                name: "AgentCommissionSalesGroup",
                schema: "Sales");

            migrationBuilder.DropTable(
                name: "AgentCommissionSlab",
                schema: "Sales");

            migrationBuilder.DropIndex(
                name: "IX_AgentCommissionConfig_AgentId_CommissionSplitId",
                schema: "Sales",
                table: "AgentCommissionConfig");

            migrationBuilder.DropIndex(
                name: "IX_AgentCommissionConfig_CommissionSplitId",
                schema: "Sales",
                table: "AgentCommissionConfig");

            migrationBuilder.DropIndex(
                name: "IX_AgentCommissionConfig_SlabTypeId",
                schema: "Sales",
                table: "AgentCommissionConfig");

            migrationBuilder.DropColumn(
                name: "CommissionSplitId",
                schema: "Sales",
                table: "AgentCommissionConfig");

            migrationBuilder.RenameColumn(
                name: "TriggerEventId",
                schema: "Sales",
                table: "AgentCommissionConfig",
                newName: "SalesSegmentId");

            migrationBuilder.RenameColumn(
                name: "SlabTypeId",
                schema: "Sales",
                table: "AgentCommissionConfig",
                newName: "CurrencyId");

            migrationBuilder.RenameIndex(
                name: "IX_AgentCommissionConfig_TriggerEventId",
                schema: "Sales",
                table: "AgentCommissionConfig",
                newName: "IX_AgentCommissionConfig_SalesSegmentId");

            migrationBuilder.AlterColumn<DateTimeOffset>(
                name: "ValidityTo",
                schema: "Sales",
                table: "AgentCommissionConfig",
                type: "datetimeoffset",
                nullable: false,
                defaultValue: new DateTimeOffset(new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)),
                oldClrType: typeof(DateTimeOffset),
                oldType: "datetimeoffset",
                oldNullable: true);

            migrationBuilder.AlterColumn<int>(
                name: "CommissionBasisId",
                schema: "Sales",
                table: "AgentCommissionConfig",
                type: "int",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "int");

            migrationBuilder.AlterColumn<int>(
                name: "ApplicableLevelId",
                schema: "Sales",
                table: "AgentCommissionConfig",
                type: "int",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "int");

            migrationBuilder.CreateIndex(
                name: "IX_AgentCommissionConfig_AgentId_SalesSegmentId",
                schema: "Sales",
                table: "AgentCommissionConfig",
                columns: new[] { "AgentId", "SalesSegmentId" });

            migrationBuilder.AddForeignKey(
                name: "FK_AgentCommissionConfig_SalesSegment_SalesSegmentId",
                schema: "Sales",
                table: "AgentCommissionConfig",
                column: "SalesSegmentId",
                principalSchema: "Sales",
                principalTable: "SalesSegment",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }
    }
}
