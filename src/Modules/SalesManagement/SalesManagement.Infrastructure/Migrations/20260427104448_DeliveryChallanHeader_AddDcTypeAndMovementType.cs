using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SalesManagement.Infrastructure.Data.Migrations
{
    /// <inheritdoc />
    public partial class DeliveryChallanHeader_AddDcTypeAndMovementType : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "DcTypeId",
                schema: "Sales",
                table: "DeliveryChallanHeader",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "MovementTypeId",
                schema: "Sales",
                table: "DeliveryChallanHeader",
                type: "int",
                nullable: false,
                defaultValue: 0);

            // Backfill existing rows with default Non-Returnable DcType and the first
            // active MovementTypeConfig. The new columns default to 0, which would
            // violate the FK constraints below — so seed real values first.
            // If either default is missing in the target tables, the migration fails fast.
            migrationBuilder.Sql(@"
                DECLARE @DefaultDcTypeId INT = (
                    SELECT mm.Id FROM Sales.MiscMaster mm
                    INNER JOIN Sales.MiscTypeMaster mt ON mm.MiscTypeId = mt.Id
                    WHERE mt.MiscTypeCode = 'DCType'
                      AND mm.Code = 'Non-Returnable'
                      AND mm.IsDeleted = 0);

                DECLARE @DefaultMovementTypeId INT = (
                    SELECT TOP 1 Id FROM Sales.MovementTypeConfig
                    WHERE IsActive = 1 AND IsDeleted = 0
                    ORDER BY Id);

                IF @DefaultDcTypeId IS NULL
                    THROW 50001, 'Backfill failed: MiscMaster row for DCType=Non-Returnable not found. Run the seed SQL before applying this migration.', 1;

                IF @DefaultMovementTypeId IS NULL
                    THROW 50002, 'Backfill failed: no active row in Sales.MovementTypeConfig.', 1;

                UPDATE Sales.DeliveryChallanHeader
                SET DcTypeId       = @DefaultDcTypeId,
                    MovementTypeId = @DefaultMovementTypeId
                WHERE DcTypeId = 0 OR MovementTypeId = 0;
            ");

            migrationBuilder.CreateIndex(
                name: "IX_DeliveryChallanHeader_DcTypeId",
                schema: "Sales",
                table: "DeliveryChallanHeader",
                column: "DcTypeId");

            migrationBuilder.CreateIndex(
                name: "IX_DeliveryChallanHeader_MovementTypeId",
                schema: "Sales",
                table: "DeliveryChallanHeader",
                column: "MovementTypeId");

            migrationBuilder.AddForeignKey(
                name: "FK_DeliveryChallanHeader_MiscMaster_DcTypeId",
                schema: "Sales",
                table: "DeliveryChallanHeader",
                column: "DcTypeId",
                principalSchema: "Sales",
                principalTable: "MiscMaster",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_DeliveryChallanHeader_MovementTypeConfig_MovementTypeId",
                schema: "Sales",
                table: "DeliveryChallanHeader",
                column: "MovementTypeId",
                principalSchema: "Sales",
                principalTable: "MovementTypeConfig",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_DeliveryChallanHeader_MiscMaster_DcTypeId",
                schema: "Sales",
                table: "DeliveryChallanHeader");

            migrationBuilder.DropForeignKey(
                name: "FK_DeliveryChallanHeader_MovementTypeConfig_MovementTypeId",
                schema: "Sales",
                table: "DeliveryChallanHeader");

            migrationBuilder.DropIndex(
                name: "IX_DeliveryChallanHeader_DcTypeId",
                schema: "Sales",
                table: "DeliveryChallanHeader");

            migrationBuilder.DropIndex(
                name: "IX_DeliveryChallanHeader_MovementTypeId",
                schema: "Sales",
                table: "DeliveryChallanHeader");

            migrationBuilder.DropColumn(
                name: "DcTypeId",
                schema: "Sales",
                table: "DeliveryChallanHeader");

            migrationBuilder.DropColumn(
                name: "MovementTypeId",
                schema: "Sales",
                table: "DeliveryChallanHeader");
        }
    }
}
