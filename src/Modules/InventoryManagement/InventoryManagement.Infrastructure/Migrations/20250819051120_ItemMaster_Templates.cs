using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace InventoryManagement.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class ItemMaster_Templates : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "InspectionTemplate",
                schema: "Inventory",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    TemplateName = table.Column<string>(type: "varchar(200)", nullable: false),
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
                    table.PrimaryKey("PK_InspectionTemplate", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "InspectionParameter",
                schema: "Inventory",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    TemplateId = table.Column<int>(type: "int", nullable: false),
                    Parameter = table.Column<string>(type: "varchar(150)", nullable: false),
                    AcceptanceCriteriaValue = table.Column<string>(type: "varchar(100)", nullable: true),
                    Numeric = table.Column<bool>(type: "bit", nullable: false),
                    MinimumValue = table.Column<decimal>(type: "decimal(18,3)", nullable: true),
                    MaximumValue = table.Column<decimal>(type: "decimal(18,3)", nullable: true),
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
                    table.PrimaryKey("PK_InspectionParameter", x => x.Id);
                    table.ForeignKey(
                        name: "FK_InspectionParameter_InspectionTemplate_TemplateId",
                        column: x => x.TemplateId,
                        principalSchema: "Inventory",
                        principalTable: "InspectionTemplate",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_ItemQuality_InspectionTemplateId",
                schema: "Inventory",
                table: "ItemQuality",
                column: "InspectionTemplateId");

            migrationBuilder.CreateIndex(
                name: "IX_InspectionParameter_TemplateId",
                schema: "Inventory",
                table: "InspectionParameter",
                column: "TemplateId");

            migrationBuilder.CreateIndex(
                name: "IX_InspectionTemplate_TemplateName",
                schema: "Inventory",
                table: "InspectionTemplate",
                column: "TemplateName");

            migrationBuilder.AddForeignKey(
                name: "FK_ItemQuality_InspectionTemplate_InspectionTemplateId",
                schema: "Inventory",
                table: "ItemQuality",
                column: "InspectionTemplateId",
                principalSchema: "Inventory",
                principalTable: "InspectionTemplate",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_ItemQuality_InspectionTemplate_InspectionTemplateId",
                schema: "Inventory",
                table: "ItemQuality");

            migrationBuilder.DropTable(
                name: "InspectionParameter",
                schema: "Inventory");

            migrationBuilder.DropTable(
                name: "InspectionTemplate",
                schema: "Inventory");

            migrationBuilder.DropIndex(
                name: "IX_ItemQuality_InspectionTemplateId",
                schema: "Inventory",
                table: "ItemQuality");
        }
    }
}
