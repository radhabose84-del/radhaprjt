using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MaintenanceManagement.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddProcessorHintToOutbox : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // OutboxMessages table already exists (created outside EF migrations).
            // Only add the new ProcessorHint column.
            migrationBuilder.AddColumn<string>(
                name: "ProcessorHint",
                schema: "maintenance",
                table: "OutboxMessages",
                type: "nvarchar(50)",
                maxLength: 50,
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ProcessorHint",
                schema: "maintenance",
                table: "OutboxMessages");
        }
    }
}
