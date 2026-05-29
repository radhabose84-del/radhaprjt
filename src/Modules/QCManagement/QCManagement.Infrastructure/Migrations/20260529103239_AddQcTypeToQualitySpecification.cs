using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace QCManagement.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddQcTypeToQualitySpecification : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "QcTypeId",
                schema: "QC",
                table: "QualitySpecification",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateIndex(
                name: "IX_QualitySpecification_QcTypeId",
                schema: "QC",
                table: "QualitySpecification",
                column: "QcTypeId");

            migrationBuilder.AddForeignKey(
                name: "FK_QualitySpecification_MiscMaster_QcTypeId",
                schema: "QC",
                table: "QualitySpecification",
                column: "QcTypeId",
                principalSchema: "QC",
                principalTable: "MiscMaster",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_QualitySpecification_MiscMaster_QcTypeId",
                schema: "QC",
                table: "QualitySpecification");

            migrationBuilder.DropIndex(
                name: "IX_QualitySpecification_QcTypeId",
                schema: "QC",
                table: "QualitySpecification");

            migrationBuilder.DropColumn(
                name: "QcTypeId",
                schema: "QC",
                table: "QualitySpecification");
        }
    }
}
