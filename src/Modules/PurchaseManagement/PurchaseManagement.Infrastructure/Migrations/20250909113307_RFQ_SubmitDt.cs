using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PurchaseManagement.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class RFQ_SubmitDt : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "RfqActivityLog",
                schema: "Purchase");

            migrationBuilder.DropIndex(
                name: "IX_RfqItem_RfqId_ItemId_UomId_RequiredDate",
                schema: "Purchase",
                table: "RfqItem");

            migrationBuilder.DropColumn(
                name: "SentAt",
                schema: "Purchase",
                table: "RfqMaster");

            migrationBuilder.DropColumn(
                name: "SubmittedAt",
                schema: "Purchase",
                table: "RfqMaster");

            migrationBuilder.DropColumn(
                name: "RequiredDate",
                schema: "Purchase",
                table: "RfqItem");

            migrationBuilder.AddColumn<DateTimeOffset>(
                name: "LastSubmitDate",
                schema: "Purchase",
                table: "RfqMaster",
                type: "datetimeoffset",
                nullable: false,
                defaultValue: new DateTimeOffset(new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)));

            migrationBuilder.CreateIndex(
                name: "IX_RfqItem_RfqId_ItemId_UomId",
                schema: "Purchase",
                table: "RfqItem",
                columns: new[] { "RfqId", "ItemId", "UomId" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_RfqItem_RfqId_ItemId_UomId",
                schema: "Purchase",
                table: "RfqItem");

            migrationBuilder.DropColumn(
                name: "LastSubmitDate",
                schema: "Purchase",
                table: "RfqMaster");

            migrationBuilder.AddColumn<DateTime>(
                name: "SentAt",
                schema: "Purchase",
                table: "RfqMaster",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "SubmittedAt",
                schema: "Purchase",
                table: "RfqMaster",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "RequiredDate",
                schema: "Purchase",
                table: "RfqItem",
                type: "datetime2",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.CreateTable(
                name: "RfqActivityLog",
                schema: "Purchase",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Action = table.Column<string>(type: "nvarchar(32)", maxLength: 32, nullable: false, defaultValue: "Update"),
                    CreatedBy = table.Column<int>(type: "int", nullable: true),
                    CreatedByName = table.Column<string>(type: "nvarchar(128)", maxLength: 128, nullable: true),
                    CreatedDate = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
                    CreatedIP = table.Column<string>(type: "nvarchar(64)", maxLength: 64, nullable: true),
                    EntityId = table.Column<int>(type: "int", nullable: false),
                    EntityName = table.Column<string>(type: "nvarchar(128)", maxLength: 128, nullable: false),
                    NewValue = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true),
                    OldValue = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true),
                    PropertyName = table.Column<string>(type: "nvarchar(128)", maxLength: 128, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_RfqActivityLog", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_RfqItem_RfqId_ItemId_UomId_RequiredDate",
                schema: "Purchase",
                table: "RfqItem",
                columns: new[] { "RfqId", "ItemId", "UomId", "RequiredDate" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_RfqActivityLog_EntityName_EntityId_CreatedDate",
                schema: "Purchase",
                table: "RfqActivityLog",
                columns: new[] { "EntityName", "EntityId", "CreatedDate" });
        }
    }
}
