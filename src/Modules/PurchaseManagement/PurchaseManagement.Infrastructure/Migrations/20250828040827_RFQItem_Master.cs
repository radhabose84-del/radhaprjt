using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PurchaseManagement.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class RFQItem_Master : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
{
    // 1) Drop child FKs to allow rebuilding principal table
    migrationBuilder.Sql(@"
IF OBJECT_ID(N'[Purchase].[FK_RfqItem_RfqMaster_RfqId]', 'F') IS NOT NULL
  ALTER TABLE [Purchase].[RfqItem] DROP CONSTRAINT [FK_RfqItem_RfqMaster_RfqId];
IF OBJECT_ID(N'[Purchase].[FK_RfqSuppliers_RfqMaster_RfqId]', 'F') IS NOT NULL
  ALTER TABLE [Purchase].[RfqSuppliers] DROP CONSTRAINT [FK_RfqSuppliers_RfqMaster_RfqId];
");

    // 2) Create new table with desired schema (Id = IDENTITY PK)
    migrationBuilder.Sql(@"
CREATE TABLE [Purchase].[RfqMaster_New](
    [Id]               INT IDENTITY(1,1) NOT NULL,
    [UnitId]           INT NULL,
    [RfqCode]          VARCHAR(30) NOT NULL,
    [RfqStatusId]      INT NOT NULL,
    [InitiationTypeId] INT NOT NULL,
    [IndentId]         INT NULL,
    [SubmittedAt]      DATETIME2 NULL,
    [SentAt]           DATETIME2 NULL,
    -- BaseEntity / audit columns
    [IsActive]         BIT NOT NULL,
    [IsDeleted]        BIT NOT NULL,
    [CreatedBy]        INT NOT NULL,
    [CreatedDate]      DATETIMEOFFSET NULL,
    [CreatedByName]    VARCHAR(50) NULL,
    [CreatedIP]        VARCHAR(50) NULL,
    [ModifiedBy]       INT NULL,
    [ModifiedDate]     DATETIMEOFFSET NULL,
    [ModifiedByName]   VARCHAR(50) NULL,
    [ModifiedIP]       VARCHAR(50) NULL,
    CONSTRAINT [PK_RfqMaster_New] PRIMARY KEY ([Id])
);
");

    // 3) Copy data, preserving keys: set new Id = old UnitId
    migrationBuilder.Sql(@"
SET IDENTITY_INSERT [Purchase].[RfqMaster_New] ON;

INSERT INTO [Purchase].[RfqMaster_New]
(
    [Id],[UnitId],[RfqCode],[RfqStatusId],[InitiationTypeId],[IndentId],
    [SubmittedAt],[SentAt],
    [IsActive],[IsDeleted],[CreatedBy],[CreatedDate],[CreatedByName],[CreatedIP],
    [ModifiedBy],[ModifiedDate],[ModifiedByName],[ModifiedIP]
)
SELECT
    UnitId AS [Id], UnitId, RfqCode, RfqStatusId, InitiationTypeId, IndentId,
    SubmittedAt, SentAt,
    IsActive, IsDeleted, CreatedBy, CreatedDate, CreatedByName, CreatedIP,
    ModifiedBy, ModifiedDate, ModifiedByName, ModifiedIP
FROM [Purchase].[RfqMaster];

SET IDENTITY_INSERT [Purchase].[RfqMaster_New] OFF;
");

    // 4) Drop old table and rename the new one
    migrationBuilder.Sql(@"
DROP TABLE [Purchase].[RfqMaster];
EXEC sp_rename N'[Purchase].[RfqMaster_New]', N'RfqMaster';
");

    // 5) Recreate indexes (unique RFQ code)
    migrationBuilder.Sql(@"
CREATE UNIQUE INDEX [IX_RfqMaster_RfqCode] ON [Purchase].[RfqMaster]([RfqCode]);
");

    // 6) Recreate FKs from child tables to new PK (Id)
    migrationBuilder.Sql(@"
ALTER TABLE [Purchase].[RfqItem] WITH CHECK
  ADD CONSTRAINT [FK_RfqItem_RfqMaster_RfqId]
  FOREIGN KEY([RfqId]) REFERENCES [Purchase].[RfqMaster]([Id]) ON DELETE CASCADE;
ALTER TABLE [Purchase].[RfqItem] CHECK CONSTRAINT [FK_RfqItem_RfqMaster_RfqId];

ALTER TABLE [Purchase].[RfqSuppliers] WITH CHECK
  ADD CONSTRAINT [FK_RfqSuppliers_RfqMaster_RfqId]
  FOREIGN KEY([RfqId]) REFERENCES [Purchase].[RfqMaster]([Id]) ON DELETE CASCADE;
ALTER TABLE [Purchase].[RfqSuppliers] CHECK CONSTRAINT [FK_RfqSuppliers_RfqMaster_RfqId];
");
}


        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_RfqItem_RfqMaster_RfqId",
                schema: "Purchase",
                table: "RfqItem");

            migrationBuilder.DropForeignKey(
                name: "FK_RfqSuppliers_RfqMaster_RfqId",
                schema: "Purchase",
                table: "RfqSuppliers");

            migrationBuilder.DropPrimaryKey(
                name: "PK_RfqMaster",
                schema: "Purchase",
                table: "RfqMaster");

            migrationBuilder.AlterColumn<int>(
                name: "UnitId",
                schema: "Purchase",
                table: "RfqMaster",
                type: "int",
                nullable: false,
                defaultValue: 0,
                oldClrType: typeof(int),
                oldType: "int",
                oldNullable: true)
                .Annotation("SqlServer:Identity", "1, 1");

            migrationBuilder.AlterColumn<int>(
                name: "Id",
                schema: "Purchase",
                table: "RfqMaster",
                type: "int",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "int")
                .OldAnnotation("SqlServer:Identity", "1, 1");

            migrationBuilder.AddPrimaryKey(
                name: "PK_RfqMaster",
                schema: "Purchase",
                table: "RfqMaster",
                column: "UnitId");

            migrationBuilder.AddForeignKey(
                name: "FK_RfqItem_RfqMaster_RfqId",
                schema: "Purchase",
                table: "RfqItem",
                column: "RfqId",
                principalSchema: "Purchase",
                principalTable: "RfqMaster",
                principalColumn: "UnitId",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_RfqSuppliers_RfqMaster_RfqId",
                schema: "Purchase",
                table: "RfqSuppliers",
                column: "RfqId",
                principalSchema: "Purchase",
                principalTable: "RfqMaster",
                principalColumn: "UnitId",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
