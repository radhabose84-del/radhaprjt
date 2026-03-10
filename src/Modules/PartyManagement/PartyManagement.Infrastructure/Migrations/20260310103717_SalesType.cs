using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PartyManagement.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class SalesType : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "SalesType",
                schema: "Party",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    PartyId = table.Column<int>(type: "int", nullable: false),
                    SalesSegmentId = table.Column<int>(type: "int", nullable: true),
                    OrderTypeId = table.Column<int>(type: "int", nullable: true),
                    IncotermId = table.Column<int>(type: "int", nullable: true),
                    PaymentTermsId = table.Column<int>(type: "int", nullable: true),
                    ShippingConditionId = table.Column<int>(type: "int", nullable: true),
                    AccountAssignmentId = table.Column<int>(type: "int", nullable: true),
                    Active = table.Column<byte>(type: "tinyint", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SalesType", x => x.Id);
                    table.ForeignKey(
                        name: "FK_SalesType_MiscMaster_AccountAssignmentId",
                        column: x => x.AccountAssignmentId,
                        principalSchema: "Party",
                        principalTable: "MiscMaster",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_SalesType_MiscMaster_ShippingConditionId",
                        column: x => x.ShippingConditionId,
                        principalSchema: "Party",
                        principalTable: "MiscMaster",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_SalesType_PartyMaster_PartyId",
                        column: x => x.PartyId,
                        principalSchema: "Party",
                        principalTable: "PartyMaster",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_SalesType_AccountAssignmentId",
                schema: "Party",
                table: "SalesType",
                column: "AccountAssignmentId");

            migrationBuilder.CreateIndex(
                name: "IX_SalesType_PartyId",
                schema: "Party",
                table: "SalesType",
                column: "PartyId");

            migrationBuilder.CreateIndex(
                name: "IX_SalesType_ShippingConditionId",
                schema: "Party",
                table: "SalesType",
                column: "ShippingConditionId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "SalesType",
                schema: "Party");
        }
    }
}
