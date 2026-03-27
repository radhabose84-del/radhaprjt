using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SalesManagement.Infrastructure.Data.Migrations
{
    /// <inheritdoc />
    public partial class ComplaintQCReview : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "ComplaintQCReview",
                schema: "Sales",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ComplaintHeaderId = table.Column<int>(type: "int", nullable: false),
                    PhysicalVerificationId = table.Column<int>(type: "int", nullable: false),
                    ComplaintStatusId = table.Column<int>(type: "int", nullable: true),
                    SeverityId = table.Column<int>(type: "int", nullable: true),
                    CompensationStructureId = table.Column<int>(type: "int", nullable: true),
                    LabVerificationRequired = table.Column<bool>(type: "bit", nullable: false),
                    LabResponsiblePersonId = table.Column<int>(type: "int", nullable: true),
                    ExpectedResolutionDate = table.Column<DateTime>(type: "date", nullable: true),
                    Comments = table.Column<string>(type: "nvarchar(1000)", nullable: true),
                    ReviewedBy = table.Column<int>(type: "int", nullable: true),
                    ReviewedDate = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true),
                    DecisionTimestamp = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false),
                    CreatedBy = table.Column<int>(type: "int", nullable: false),
                    CreatedDate = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true),
                    CreatedByName = table.Column<string>(type: "varchar(100)", nullable: true),
                    CreatedIP = table.Column<string>(type: "varchar(50)", nullable: true),
                    ModifiedBy = table.Column<int>(type: "int", nullable: true),
                    ModifiedDate = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true),
                    ModifiedByName = table.Column<string>(type: "varchar(100)", nullable: true),
                    ModifiedIP = table.Column<string>(type: "varchar(50)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ComplaintQCReview", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ComplaintQCReview_ComplaintHeader_ComplaintHeaderId",
                        column: x => x.ComplaintHeaderId,
                        principalSchema: "Sales",
                        principalTable: "ComplaintHeader",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_ComplaintQCReview_MiscMaster_CompensationStructureId",
                        column: x => x.CompensationStructureId,
                        principalSchema: "Sales",
                        principalTable: "MiscMaster",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_ComplaintQCReview_MiscMaster_ComplaintStatusId",
                        column: x => x.ComplaintStatusId,
                        principalSchema: "Sales",
                        principalTable: "MiscMaster",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_ComplaintQCReview_MiscMaster_PhysicalVerificationId",
                        column: x => x.PhysicalVerificationId,
                        principalSchema: "Sales",
                        principalTable: "MiscMaster",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_ComplaintQCReview_MiscMaster_SeverityId",
                        column: x => x.SeverityId,
                        principalSchema: "Sales",
                        principalTable: "MiscMaster",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "ComplaintQCReviewAssignment",
                schema: "Sales",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ComplaintQCReviewId = table.Column<int>(type: "int", nullable: false),
                    RoleId = table.Column<int>(type: "int", nullable: false),
                    ResponsiblePersonId = table.Column<int>(type: "int", nullable: false),
                    IsMandatory = table.Column<bool>(type: "bit", nullable: false),
                    AssignmentStatusId = table.Column<int>(type: "int", nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false),
                    CreatedBy = table.Column<int>(type: "int", nullable: false),
                    CreatedDate = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true),
                    CreatedByName = table.Column<string>(type: "varchar(100)", nullable: true),
                    CreatedIP = table.Column<string>(type: "varchar(50)", nullable: true),
                    ModifiedBy = table.Column<int>(type: "int", nullable: true),
                    ModifiedDate = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true),
                    ModifiedByName = table.Column<string>(type: "varchar(100)", nullable: true),
                    ModifiedIP = table.Column<string>(type: "varchar(50)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ComplaintQCReviewAssignment", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ComplaintQCReviewAssignment_ComplaintQCReview_ComplaintQCReviewId",
                        column: x => x.ComplaintQCReviewId,
                        principalSchema: "Sales",
                        principalTable: "ComplaintQCReview",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_ComplaintQCReviewAssignment_MiscMaster_AssignmentStatusId",
                        column: x => x.AssignmentStatusId,
                        principalSchema: "Sales",
                        principalTable: "MiscMaster",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_ComplaintQCReviewAssignment_MiscMaster_RoleId",
                        column: x => x.RoleId,
                        principalSchema: "Sales",
                        principalTable: "MiscMaster",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_ComplaintQCReview_CompensationStructureId",
                schema: "Sales",
                table: "ComplaintQCReview",
                column: "CompensationStructureId");

            migrationBuilder.CreateIndex(
                name: "IX_ComplaintQCReview_ComplaintHeaderId",
                schema: "Sales",
                table: "ComplaintQCReview",
                column: "ComplaintHeaderId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_ComplaintQCReview_ComplaintStatusId",
                schema: "Sales",
                table: "ComplaintQCReview",
                column: "ComplaintStatusId");

            migrationBuilder.CreateIndex(
                name: "IX_ComplaintQCReview_PhysicalVerificationId",
                schema: "Sales",
                table: "ComplaintQCReview",
                column: "PhysicalVerificationId");

            migrationBuilder.CreateIndex(
                name: "IX_ComplaintQCReview_SeverityId",
                schema: "Sales",
                table: "ComplaintQCReview",
                column: "SeverityId");

            migrationBuilder.CreateIndex(
                name: "IX_ComplaintQCReviewAssignment_AssignmentStatusId",
                schema: "Sales",
                table: "ComplaintQCReviewAssignment",
                column: "AssignmentStatusId");

            migrationBuilder.CreateIndex(
                name: "IX_ComplaintQCReviewAssignment_ComplaintQCReviewId",
                schema: "Sales",
                table: "ComplaintQCReviewAssignment",
                column: "ComplaintQCReviewId");

            migrationBuilder.CreateIndex(
                name: "IX_ComplaintQCReviewAssignment_ResponsiblePersonId",
                schema: "Sales",
                table: "ComplaintQCReviewAssignment",
                column: "ResponsiblePersonId");

            migrationBuilder.CreateIndex(
                name: "IX_ComplaintQCReviewAssignment_RoleId",
                schema: "Sales",
                table: "ComplaintQCReviewAssignment",
                column: "RoleId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ComplaintQCReviewAssignment",
                schema: "Sales");

            migrationBuilder.DropTable(
                name: "ComplaintQCReview",
                schema: "Sales");
        }
    }
}
