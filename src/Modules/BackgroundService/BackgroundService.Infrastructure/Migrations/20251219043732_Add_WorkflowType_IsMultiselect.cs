using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BackgroundService.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class Add_WorkflowType_IsMultiselect : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "IsMultiselect",
                schema: "AppData",
                table: "WorkflowType",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "IsEdit",
                schema: "AppData",
                table: "ApprovalStepDetail",
                type: "bit",
                nullable: false,
                defaultValue: false);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "IsMultiselect",
                schema: "AppData",
                table: "WorkflowType");

            migrationBuilder.DropColumn(
                name: "IsEdit",
                schema: "AppData",
                table: "ApprovalStepDetail");
        }
    }
}
