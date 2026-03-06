using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BSOFT.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class UnitTypeAdd : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Step 1: Add column as nullable so existing rows don't violate FK immediately
            migrationBuilder.AddColumn<int>(
                name: "UnitTypeId",
                schema: "AppData",
                table: "Unit",
                type: "int",
                nullable: true);

            // Step 2: Seed MiscTypeMaster for "Unit Type"
            migrationBuilder.Sql(@"
                IF NOT EXISTS (SELECT 1 FROM AppData.MiscTypeMaster WHERE MiscTypeCode = 'UNITTYPE')
                BEGIN
                    INSERT INTO AppData.MiscTypeMaster
                        (MiscTypeCode, Description, IsActive, IsDeleted, CreatedBy, CreatedDate, CreatedByName, CreatedIP)
                    VALUES
                        ('UNITTYPE', 'Unit Type', 1, 0, 1, GETDATE(), 'System', '127.0.0.1');
                END;
            ");

            // Step 3: Seed MiscMaster entries for Depot and Plant
            migrationBuilder.Sql(@"
                DECLARE @UnitTypeMiscTypeId INT = (SELECT TOP 1 Id FROM AppData.MiscTypeMaster WHERE MiscTypeCode = 'UNITTYPE');

                IF NOT EXISTS (SELECT 1 FROM AppData.MiscMaster WHERE Code = 'DEPOT' AND MiscTypeId = @UnitTypeMiscTypeId)
                BEGIN
                    INSERT INTO AppData.MiscMaster
                        (MiscTypeId, Code, Description, SortOrder, IsActive, IsDeleted, CreatedBy, CreatedDate, CreatedByName, CreatedIP)
                    VALUES
                        (@UnitTypeMiscTypeId, 'DEPOT', 'Depot', 1, 1, 0, 1, GETDATE(), 'System', '127.0.0.1');
                END;

                IF NOT EXISTS (SELECT 1 FROM AppData.MiscMaster WHERE Code = 'PLANT' AND MiscTypeId = @UnitTypeMiscTypeId)
                BEGIN
                    INSERT INTO AppData.MiscMaster
                        (MiscTypeId, Code, Description, SortOrder, IsActive, IsDeleted, CreatedBy, CreatedDate, CreatedByName, CreatedIP)
                    VALUES
                        (@UnitTypeMiscTypeId, 'PLANT', 'Plant', 2, 1, 0, 1, GETDATE(), 'System', '127.0.0.1');
                END;
            ");

            // Step 4: Default all existing Unit rows to the DEPOT MiscMaster Id
            migrationBuilder.Sql(@"
                DECLARE @DepotId INT = (
                    SELECT TOP 1 MM.Id
                    FROM AppData.MiscMaster MM
                    INNER JOIN AppData.MiscTypeMaster MT ON MT.Id = MM.MiscTypeId
                    WHERE MT.MiscTypeCode = 'UNITTYPE' AND MM.Code = 'DEPOT'
                );
                UPDATE AppData.Unit SET UnitTypeId = @DepotId WHERE UnitTypeId IS NULL;
            ");

            // Step 5: Now make column NOT NULL (all rows have a valid FK value)
            migrationBuilder.AlterColumn<int>(
                name: "UnitTypeId",
                schema: "AppData",
                table: "Unit",
                type: "int",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "int",
                oldNullable: true);

            // Step 6: Create index
            migrationBuilder.CreateIndex(
                name: "IX_Unit_UnitTypeId",
                schema: "AppData",
                table: "Unit",
                column: "UnitTypeId");

            // Step 7: Add FK constraint (all rows now have valid MiscMaster references)
            migrationBuilder.AddForeignKey(
                name: "FK_Unit_MiscMaster_UnitTypeId",
                schema: "AppData",
                table: "Unit",
                column: "UnitTypeId",
                principalSchema: "AppData",
                principalTable: "MiscMaster",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Unit_MiscMaster_UnitTypeId",
                schema: "AppData",
                table: "Unit");

            migrationBuilder.DropIndex(
                name: "IX_Unit_UnitTypeId",
                schema: "AppData",
                table: "Unit");

            migrationBuilder.DropColumn(
                name: "UnitTypeId",
                schema: "AppData",
                table: "Unit");
        }
    }
}
