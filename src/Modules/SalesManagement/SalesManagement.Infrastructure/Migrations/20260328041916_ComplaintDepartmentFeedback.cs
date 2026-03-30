using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SalesManagement.Infrastructure.Data.Migrations
{
    /// <inheritdoc />
    public partial class ComplaintDepartmentFeedback : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "ComplaintDepartmentFeedback",
                schema: "Sales",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    AssignmentId = table.Column<int>(type: "int", nullable: false),
                    RootCauseText = table.Column<string>(type: "nvarchar(2000)", nullable: true),
                    RootCauseCategoryId = table.Column<int>(type: "int", nullable: true),
                    CorrectiveAction = table.Column<string>(type: "nvarchar(2000)", nullable: false),
                    PreventiveAction = table.Column<string>(type: "nvarchar(2000)", nullable: true),
                    Remarks = table.Column<string>(type: "nvarchar(1000)", nullable: true),
                    FeedbackStatusId = table.Column<int>(type: "int", nullable: false),
                    SubmittedBy = table.Column<int>(type: "int", nullable: true),
                    SubmittedDate = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true),
                    ReworkCount = table.Column<int>(type: "int", nullable: false, defaultValue: 0),
                    ReworkReason = table.Column<string>(type: "nvarchar(1000)", nullable: true),
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
                    table.PrimaryKey("PK_ComplaintDepartmentFeedback", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ComplaintDepartmentFeedback_ComplaintQCReviewAssignment_AssignmentId",
                        column: x => x.AssignmentId,
                        principalSchema: "Sales",
                        principalTable: "ComplaintQCReviewAssignment",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_ComplaintDepartmentFeedback_MiscMaster_FeedbackStatusId",
                        column: x => x.FeedbackStatusId,
                        principalSchema: "Sales",
                        principalTable: "MiscMaster",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_ComplaintDepartmentFeedback_MiscMaster_RootCauseCategoryId",
                        column: x => x.RootCauseCategoryId,
                        principalSchema: "Sales",
                        principalTable: "MiscMaster",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "ComplaintFeedbackAttachment",
                schema: "Sales",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    FeedbackId = table.Column<int>(type: "int", nullable: false),
                    FileName = table.Column<string>(type: "nvarchar(255)", nullable: false),
                    FilePath = table.Column<string>(type: "nvarchar(500)", nullable: false),
                    FileType = table.Column<string>(type: "nvarchar(50)", nullable: true),
                    FileSize = table.Column<long>(type: "bigint", nullable: true),
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
                    table.PrimaryKey("PK_ComplaintFeedbackAttachment", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ComplaintFeedbackAttachment_ComplaintDepartmentFeedback_FeedbackId",
                        column: x => x.FeedbackId,
                        principalSchema: "Sales",
                        principalTable: "ComplaintDepartmentFeedback",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_ComplaintDepartmentFeedback_AssignmentId",
                schema: "Sales",
                table: "ComplaintDepartmentFeedback",
                column: "AssignmentId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_ComplaintDepartmentFeedback_FeedbackStatusId",
                schema: "Sales",
                table: "ComplaintDepartmentFeedback",
                column: "FeedbackStatusId");

            migrationBuilder.CreateIndex(
                name: "IX_ComplaintDepartmentFeedback_RootCauseCategoryId",
                schema: "Sales",
                table: "ComplaintDepartmentFeedback",
                column: "RootCauseCategoryId");

            migrationBuilder.CreateIndex(
                name: "IX_ComplaintDepartmentFeedback_SubmittedBy",
                schema: "Sales",
                table: "ComplaintDepartmentFeedback",
                column: "SubmittedBy");

            migrationBuilder.CreateIndex(
                name: "IX_ComplaintFeedbackAttachment_FeedbackId",
                schema: "Sales",
                table: "ComplaintFeedbackAttachment",
                column: "FeedbackId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ComplaintFeedbackAttachment",
                schema: "Sales");

            migrationBuilder.DropTable(
                name: "ComplaintDepartmentFeedback",
                schema: "Sales");
        }
    }
}
