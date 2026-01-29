using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BackgroundService.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class approvalstepDepartment : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "ApprovalStepDepartmentMapping",
                schema: "AppData",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ApprovalStepDetailId = table.Column<int>(type: "int", nullable: false),
                    DepartmentId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ApprovalStepDepartmentMapping", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ApprovalStepDepartmentMapping_ApprovalStepDetail_ApprovalStepDetailId",
                        column: x => x.ApprovalStepDetailId,
                        principalSchema: "AppData",
                        principalTable: "ApprovalStepDetail",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateIndex(
                name: "IX_ApprovalStepDepartmentMapping_ApprovalStepDetailId",
                schema: "AppData",
                table: "ApprovalStepDepartmentMapping",
                column: "ApprovalStepDetailId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ApprovalStepDepartmentMapping",
                schema: "AppData");
        }
    }
}
