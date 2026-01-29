using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BSOFT.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class customfield : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "CustomField",
                schema: "AppData",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    LabelName = table.Column<string>(type: "varchar(50)", nullable: false),
                    DataTypeId = table.Column<int>(type: "int", nullable: false),
                    Length = table.Column<int>(type: "int", nullable: true),
                    LabelTypeId = table.Column<int>(type: "int", nullable: false),
                    IsRequired = table.Column<bool>(type: "bit", nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    CreatedBy = table.Column<int>(type: "int", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreatedByName = table.Column<string>(type: "varchar(50)", nullable: false),
                    CreatedIP = table.Column<string>(type: "varchar(255)", nullable: false),
                    ModifiedBy = table.Column<int>(type: "int", nullable: true),
                    ModifiedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    ModifiedByName = table.Column<string>(type: "varchar(50)", nullable: true),
                    ModifiedIP = table.Column<string>(type: "varchar(255)", nullable: true),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CustomField", x => x.Id);
                    table.ForeignKey(
                        name: "FK_CustomField_MiscMaster_DataTypeId",
                        column: x => x.DataTypeId,
                        principalSchema: "AppData",
                        principalTable: "MiscMaster",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_CustomField_MiscMaster_LabelTypeId",
                        column: x => x.LabelTypeId,
                        principalSchema: "AppData",
                        principalTable: "MiscMaster",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "CustomFieldMenu",
                schema: "AppData",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    CustomFieldId = table.Column<int>(type: "int", nullable: false),
                    MenuId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CustomFieldMenu", x => x.Id);
                    table.ForeignKey(
                        name: "FK_CustomFieldMenu_CustomField_CustomFieldId",
                        column: x => x.CustomFieldId,
                        principalSchema: "AppData",
                        principalTable: "CustomField",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_CustomFieldMenu_Menus_MenuId",
                        column: x => x.MenuId,
                        principalSchema: "AppData",
                        principalTable: "Menus",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "CustomFieldOptionalValue",
                schema: "AppData",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    CustomFieldId = table.Column<int>(type: "int", nullable: false),
                    OptionFieldValue = table.Column<string>(type: "varchar(200)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CustomFieldOptionalValue", x => x.Id);
                    table.ForeignKey(
                        name: "FK_CustomFieldOptionalValue_CustomField_CustomFieldId",
                        column: x => x.CustomFieldId,
                        principalSchema: "AppData",
                        principalTable: "CustomField",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "CustomFieldUnit",
                schema: "AppData",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    CustomFieldId = table.Column<int>(type: "int", nullable: false),
                    UnitId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CustomFieldUnit", x => x.Id);
                    table.ForeignKey(
                        name: "FK_CustomFieldUnit_CustomField_CustomFieldId",
                        column: x => x.CustomFieldId,
                        principalSchema: "AppData",
                        principalTable: "CustomField",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_CustomFieldUnit_Unit_UnitId",
                        column: x => x.UnitId,
                        principalSchema: "AppData",
                        principalTable: "Unit",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_CustomField_DataTypeId",
                schema: "AppData",
                table: "CustomField",
                column: "DataTypeId");

            migrationBuilder.CreateIndex(
                name: "IX_CustomField_LabelTypeId",
                schema: "AppData",
                table: "CustomField",
                column: "LabelTypeId");

            migrationBuilder.CreateIndex(
                name: "IX_CustomFieldMenu_CustomFieldId",
                schema: "AppData",
                table: "CustomFieldMenu",
                column: "CustomFieldId");

            migrationBuilder.CreateIndex(
                name: "IX_CustomFieldMenu_MenuId",
                schema: "AppData",
                table: "CustomFieldMenu",
                column: "MenuId");

            migrationBuilder.CreateIndex(
                name: "IX_CustomFieldOptionalValue_CustomFieldId",
                schema: "AppData",
                table: "CustomFieldOptionalValue",
                column: "CustomFieldId");

            migrationBuilder.CreateIndex(
                name: "IX_CustomFieldUnit_CustomFieldId",
                schema: "AppData",
                table: "CustomFieldUnit",
                column: "CustomFieldId");

            migrationBuilder.CreateIndex(
                name: "IX_CustomFieldUnit_UnitId",
                schema: "AppData",
                table: "CustomFieldUnit",
                column: "UnitId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "CustomFieldMenu",
                schema: "AppData");

            migrationBuilder.DropTable(
                name: "CustomFieldOptionalValue",
                schema: "AppData");

            migrationBuilder.DropTable(
                name: "CustomFieldUnit",
                schema: "AppData");

            migrationBuilder.DropTable(
                name: "CustomField",
                schema: "AppData");
        }
    }
}
