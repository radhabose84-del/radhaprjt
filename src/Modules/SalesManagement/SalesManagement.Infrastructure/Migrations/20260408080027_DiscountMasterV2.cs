using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SalesManagement.Infrastructure.Data.Migrations
{
    /// <inheritdoc />
    public partial class DiscountMasterV2 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_DiscountMaster_MiscMaster_ApplicableLevelId",
                schema: "Sales",
                table: "DiscountMaster");

            migrationBuilder.DropForeignKey(
                name: "FK_DiscountMaster_MiscMaster_DiscountTypeId",
                schema: "Sales",
                table: "DiscountMaster");

            migrationBuilder.DropIndex(
                name: "IX_DiscountMaster_DiscountTypeId",
                schema: "Sales",
                table: "DiscountMaster");

            migrationBuilder.RenameColumn(
                name: "DiscountValue",
                schema: "Sales",
                table: "DiscountMaster",
                newName: "MaxDiscountValue");

            migrationBuilder.RenameColumn(
                name: "DiscountTypeId",
                schema: "Sales",
                table: "DiscountMaster",
                newName: "Priority");

            migrationBuilder.RenameColumn(
                name: "ApplicableLevelId",
                schema: "Sales",
                table: "DiscountMaster",
                newName: "ExecutionTypeId");

            migrationBuilder.RenameIndex(
                name: "IX_DiscountMaster_ApplicableLevelId",
                schema: "Sales",
                table: "DiscountMaster",
                newName: "IX_DiscountMaster_ExecutionTypeId");

            migrationBuilder.AlterColumn<int>(
                name: "SlabTypeId",
                schema: "Sales",
                table: "DiscountMaster",
                type: "int",
                nullable: false,
                defaultValue: 0,
                oldClrType: typeof(int),
                oldType: "int",
                oldNullable: true);

            migrationBuilder.AddColumn<int>(
                name: "CurrencyId",
                schema: "Sales",
                table: "DiscountMaster",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "CustomerGroupId",
                schema: "Sales",
                table: "DiscountMaster",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "DiscountBasisId",
                schema: "Sales",
                table: "DiscountMaster",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "ExclusionGroupId",
                schema: "Sales",
                table: "DiscountMaster",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "IsStackable",
                schema: "Sales",
                table: "DiscountMaster",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.CreateIndex(
                name: "IX_DiscountMaster_CurrencyId",
                schema: "Sales",
                table: "DiscountMaster",
                column: "CurrencyId");

            migrationBuilder.CreateIndex(
                name: "IX_DiscountMaster_CustomerGroupId",
                schema: "Sales",
                table: "DiscountMaster",
                column: "CustomerGroupId");

            migrationBuilder.CreateIndex(
                name: "IX_DiscountMaster_DiscountBasisId",
                schema: "Sales",
                table: "DiscountMaster",
                column: "DiscountBasisId");

            migrationBuilder.CreateIndex(
                name: "IX_DiscountMaster_ExclusionGroupId",
                schema: "Sales",
                table: "DiscountMaster",
                column: "ExclusionGroupId");

            migrationBuilder.AddForeignKey(
                name: "FK_DiscountMaster_MiscMaster_CustomerGroupId",
                schema: "Sales",
                table: "DiscountMaster",
                column: "CustomerGroupId",
                principalSchema: "Sales",
                principalTable: "MiscMaster",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_DiscountMaster_MiscMaster_DiscountBasisId",
                schema: "Sales",
                table: "DiscountMaster",
                column: "DiscountBasisId",
                principalSchema: "Sales",
                principalTable: "MiscMaster",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_DiscountMaster_MiscMaster_ExclusionGroupId",
                schema: "Sales",
                table: "DiscountMaster",
                column: "ExclusionGroupId",
                principalSchema: "Sales",
                principalTable: "MiscMaster",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_DiscountMaster_MiscMaster_ExecutionTypeId",
                schema: "Sales",
                table: "DiscountMaster",
                column: "ExecutionTypeId",
                principalSchema: "Sales",
                principalTable: "MiscMaster",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_DiscountMaster_MiscMaster_CustomerGroupId",
                schema: "Sales",
                table: "DiscountMaster");

            migrationBuilder.DropForeignKey(
                name: "FK_DiscountMaster_MiscMaster_DiscountBasisId",
                schema: "Sales",
                table: "DiscountMaster");

            migrationBuilder.DropForeignKey(
                name: "FK_DiscountMaster_MiscMaster_ExclusionGroupId",
                schema: "Sales",
                table: "DiscountMaster");

            migrationBuilder.DropForeignKey(
                name: "FK_DiscountMaster_MiscMaster_ExecutionTypeId",
                schema: "Sales",
                table: "DiscountMaster");

            migrationBuilder.DropIndex(
                name: "IX_DiscountMaster_CurrencyId",
                schema: "Sales",
                table: "DiscountMaster");

            migrationBuilder.DropIndex(
                name: "IX_DiscountMaster_CustomerGroupId",
                schema: "Sales",
                table: "DiscountMaster");

            migrationBuilder.DropIndex(
                name: "IX_DiscountMaster_DiscountBasisId",
                schema: "Sales",
                table: "DiscountMaster");

            migrationBuilder.DropIndex(
                name: "IX_DiscountMaster_ExclusionGroupId",
                schema: "Sales",
                table: "DiscountMaster");

            migrationBuilder.DropColumn(
                name: "CurrencyId",
                schema: "Sales",
                table: "DiscountMaster");

            migrationBuilder.DropColumn(
                name: "CustomerGroupId",
                schema: "Sales",
                table: "DiscountMaster");

            migrationBuilder.DropColumn(
                name: "DiscountBasisId",
                schema: "Sales",
                table: "DiscountMaster");

            migrationBuilder.DropColumn(
                name: "ExclusionGroupId",
                schema: "Sales",
                table: "DiscountMaster");

            migrationBuilder.DropColumn(
                name: "IsStackable",
                schema: "Sales",
                table: "DiscountMaster");

            migrationBuilder.RenameColumn(
                name: "Priority",
                schema: "Sales",
                table: "DiscountMaster",
                newName: "DiscountTypeId");

            migrationBuilder.RenameColumn(
                name: "MaxDiscountValue",
                schema: "Sales",
                table: "DiscountMaster",
                newName: "DiscountValue");

            migrationBuilder.RenameColumn(
                name: "ExecutionTypeId",
                schema: "Sales",
                table: "DiscountMaster",
                newName: "ApplicableLevelId");

            migrationBuilder.RenameIndex(
                name: "IX_DiscountMaster_ExecutionTypeId",
                schema: "Sales",
                table: "DiscountMaster",
                newName: "IX_DiscountMaster_ApplicableLevelId");

            migrationBuilder.AlterColumn<int>(
                name: "SlabTypeId",
                schema: "Sales",
                table: "DiscountMaster",
                type: "int",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "int");

            migrationBuilder.CreateIndex(
                name: "IX_DiscountMaster_DiscountTypeId",
                schema: "Sales",
                table: "DiscountMaster",
                column: "DiscountTypeId");

            migrationBuilder.AddForeignKey(
                name: "FK_DiscountMaster_MiscMaster_ApplicableLevelId",
                schema: "Sales",
                table: "DiscountMaster",
                column: "ApplicableLevelId",
                principalSchema: "Sales",
                principalTable: "MiscMaster",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_DiscountMaster_MiscMaster_DiscountTypeId",
                schema: "Sales",
                table: "DiscountMaster",
                column: "DiscountTypeId",
                principalSchema: "Sales",
                principalTable: "MiscMaster",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }
    }
}
