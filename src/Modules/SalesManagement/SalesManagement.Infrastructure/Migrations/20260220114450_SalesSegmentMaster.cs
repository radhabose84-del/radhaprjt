using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SalesManagement.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class SalesSegmentMaster : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "SalesSegment",
                schema: "Sales",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    SalesOrganisationId = table.Column<int>(type: "int", nullable: false),
                    SalesChannelId = table.Column<int>(type: "int", nullable: false),
                    BusinessUnitId = table.Column<int>(type: "int", nullable: false),
                    CurrencyId = table.Column<int>(type: "int", nullable: true),
                    ValidFrom = table.Column<DateTime>(type: "datetime2", nullable: true),
                    SegmentName = table.Column<string>(type: "varchar(200)", nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false),
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
                    table.PrimaryKey("PK_SalesSegment", x => x.Id);
                    table.ForeignKey(
                        name: "FK_SalesSegment_BusinessUnit_BusinessUnitId",
                        column: x => x.BusinessUnitId,
                        principalSchema: "Sales",
                        principalTable: "BusinessUnit",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_SalesSegment_SalesChannel_SalesChannelId",
                        column: x => x.SalesChannelId,
                        principalSchema: "Sales",
                        principalTable: "SalesChannel",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_SalesSegment_SalesOrganisation_SalesOrganisationId",
                        column: x => x.SalesOrganisationId,
                        principalSchema: "Sales",
                        principalTable: "SalesOrganisation",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_SalesSegment_BusinessUnitId",
                schema: "Sales",
                table: "SalesSegment",
                column: "BusinessUnitId");

            migrationBuilder.CreateIndex(
                name: "IX_SalesSegment_CurrencyId",
                schema: "Sales",
                table: "SalesSegment",
                column: "CurrencyId");

            migrationBuilder.CreateIndex(
                name: "IX_SalesSegment_SalesChannelId",
                schema: "Sales",
                table: "SalesSegment",
                column: "SalesChannelId");

            migrationBuilder.CreateIndex(
                name: "IX_SalesSegment_SalesOrganisationId",
                schema: "Sales",
                table: "SalesSegment",
                column: "SalesOrganisationId");

            migrationBuilder.CreateIndex(
                name: "IX_SalesSegment_SalesOrganisationId_SalesChannelId_BusinessUnitId",
                schema: "Sales",
                table: "SalesSegment",
                columns: new[] { "SalesOrganisationId", "SalesChannelId", "BusinessUnitId" },
                unique: true,
                filter: "[IsDeleted] = 0");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "SalesSegment",
                schema: "Sales");
        }
    }
}
