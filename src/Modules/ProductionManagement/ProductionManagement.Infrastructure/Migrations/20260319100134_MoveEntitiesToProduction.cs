using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ProductionManagement.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class MoveEntitiesToProduction : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Tables already exist in database (moved via ALTER SCHEMA from Sales → Production)
            // This migration is intentionally empty — only updates the EF snapshot
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            // No-op — tables were moved manually, not created by EF
        }
    }
}
