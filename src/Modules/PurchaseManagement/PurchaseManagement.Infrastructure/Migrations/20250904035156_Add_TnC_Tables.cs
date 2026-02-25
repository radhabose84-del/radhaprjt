using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PurchaseManagement.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class Add_TnC_Tables : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
        

        
            migrationBuilder.CreateTable(
                name: "TnCTemplateMaster",
                schema: "Purchase",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    TemplateCode = table.Column<string>(type: "nvarchar(50)", nullable: false),
                    TemplateName = table.Column<string>(type: "nvarchar(200)", nullable: false),
                    TemplateTypeId = table.Column<int>(type: "int", nullable: false),
                    TermsHtml = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    ApprovalFlag = table.Column<bool>(type: "bit", nullable: true),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false),
                    CreatedBy = table.Column<int>(type: "int", nullable: false),
                    CreatedDate = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true),
                    CreatedByName = table.Column<string>(type: "varchar(50)", nullable: false),
                    CreatedIP = table.Column<string>(type: "varchar(20)", nullable: false),
                    ModifiedBy = table.Column<int>(type: "int", nullable: true),
                    ModifiedDate = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true),
                    ModifiedByName = table.Column<string>(type: "varchar(50)", nullable: true),
                    ModifiedIP = table.Column<string>(type: "varchar(20)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TnCTemplateMaster", x => x.Id);
                    table.ForeignKey(
                        name: "FK_TnCTemplateMaster_MiscMaster_TemplateTypeId",
                        column: x => x.TemplateTypeId,
                        principalSchema: "Purchase",
                        principalTable: "MiscMaster",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

          
            migrationBuilder.CreateTable(
                name: "TnCTemplateApplicability",
                schema: "Purchase",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    TnCTemplateMasterId = table.Column<int>(type: "int", nullable: false),
                    ApplicabilityId = table.Column<int>(type: "int", nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false),
                    CreatedBy = table.Column<int>(type: "int", nullable: false),
                    CreatedDate = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true),
                    CreatedByName = table.Column<string>(type: "varchar(50)", nullable: false),
                    CreatedIP = table.Column<string>(type: "varchar(20)", nullable: false),
                    ModifiedBy = table.Column<int>(type: "int", nullable: true),
                    ModifiedDate = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true),
                    ModifiedByName = table.Column<string>(type: "varchar(50)", nullable: true),
                    ModifiedIP = table.Column<string>(type: "varchar(20)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TnCTemplateApplicability", x => x.Id);
                    table.ForeignKey(
                        name: "FK_TnCTemplateApplicability_MiscMaster_ApplicabilityId",
                        column: x => x.ApplicabilityId,
                        principalSchema: "Purchase",
                        principalTable: "MiscMaster",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_TnCTemplateApplicability_TnCTemplateMaster_TnCTemplateMasterId",
                        column: x => x.TnCTemplateMasterId,
                        principalSchema: "Purchase",
                        principalTable: "TnCTemplateMaster",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

           


         


            migrationBuilder.CreateIndex(
                name: "IX_TnCTemplateApplicability_ApplicabilityId",
                schema: "Purchase",
                table: "TnCTemplateApplicability",
                column: "ApplicabilityId");

            migrationBuilder.CreateIndex(
                name: "UX_TnC_Template_App",
                schema: "Purchase",
                table: "TnCTemplateApplicability",
                columns: new[] { "TnCTemplateMasterId", "ApplicabilityId" },
                unique: true,
                filter: "[IsDeleted] = 0");

            migrationBuilder.CreateIndex(
                name: "UX_TnC_Type_Name",
                schema: "Purchase",
                table: "TnCTemplateMaster",
                columns: new[] { "TemplateTypeId", "TemplateName" },
                unique: true,
                filter: "[IsDeleted] = 0");

        

        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_IndentDetail_MiscMaster_StatusId",
                schema: "Purchase",
                table: "IndentDetail");

            migrationBuilder.DropForeignKey(
                name: "FK_IndentHeader_MiscMaster_StatusId",
                schema: "Purchase",
                table: "IndentHeader");

            migrationBuilder.DropTable(
                name: "PaymentTermInstallment",
                schema: "Purchase");

            migrationBuilder.DropTable(
                name: "TnCTemplateApplicability",
                schema: "Purchase");

            migrationBuilder.DropTable(
                name: "PaymentTermMaster",
                schema: "Purchase");

            migrationBuilder.DropTable(
                name: "TnCTemplateMaster",
                schema: "Purchase");

            migrationBuilder.DropIndex(
                name: "IX_IndentHeader_StatusId",
                schema: "Purchase",
                table: "IndentHeader");

            migrationBuilder.DropIndex(
                name: "IX_IndentDetail_StatusId",
                schema: "Purchase",
                table: "IndentDetail");

            migrationBuilder.DropColumn(
                name: "StatusId",
                schema: "Purchase",
                table: "IndentHeader");

            migrationBuilder.DropColumn(
                name: "StatusId",
                schema: "Purchase",
                table: "IndentDetail");
        }
    }
}
