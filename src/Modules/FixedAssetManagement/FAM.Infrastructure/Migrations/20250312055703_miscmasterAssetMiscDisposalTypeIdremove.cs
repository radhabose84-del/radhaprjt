using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace FAM.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class miscmasterAssetMiscDisposalTypeIdremove : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
             // Drop Foreign Key Constraint if it exists
            migrationBuilder.Sql(@"
                IF EXISTS (SELECT 1 FROM sys.foreign_keys WHERE name = 'FK_MiscMaster_AssetDisposal_AssetMiscDisposalTypeId')
                ALTER TABLE [FixedAsset].[MiscMaster] DROP CONSTRAINT FK_MiscMaster_AssetDisposal_AssetMiscDisposalTypeId;
            ");

            // Drop Index if it exists
            migrationBuilder.Sql(@"
                IF EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'IX_MiscMaster_AssetMiscDisposalTypeId')
                DROP INDEX IX_MiscMaster_AssetMiscDisposalTypeId ON [FixedAsset].[MiscMaster];
            ");

            // Drop Column if it exists
            migrationBuilder.Sql(@"
                IF EXISTS (SELECT 1 FROM INFORMATION_SCHEMA.COLUMNS 
                           WHERE TABLE_SCHEMA = 'FixedAsset' 
                           AND TABLE_NAME = 'MiscMaster' 
                           AND COLUMN_NAME = 'AssetMiscDisposalTypeId')
                ALTER TABLE [FixedAsset].[MiscMaster] DROP COLUMN AssetMiscDisposalTypeId;
            ");

        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
              // Re-add column in case of rollback
            migrationBuilder.AddColumn<int>(
                name: "AssetMiscDisposalTypeId",
                schema: "FixedAsset",
                table: "MiscMaster",
                type: "int",
                nullable: true);

            // Re-add Index
            migrationBuilder.Sql(@"
                CREATE INDEX IX_MiscMaster_AssetMiscDisposalTypeId 
                ON [FixedAsset].[MiscMaster] (AssetMiscDisposalTypeId);
            ");

            // Re-add Foreign Key
            migrationBuilder.Sql(@"
                ALTER TABLE [FixedAsset].[MiscMaster] ADD CONSTRAINT FK_MiscMaster_AssetDisposal_AssetMiscDisposalTypeId 
                FOREIGN KEY (AssetMiscDisposalTypeId) REFERENCES [FixedAsset].[AssetDisposal] (Id) ON DELETE RESTRICT;
            ");

        }
    }
}
