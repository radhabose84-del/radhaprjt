using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SalesManagement.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class RemoveMovedEntities : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Tables were moved to Production module via ALTER SCHEMA (manual migration)
            // FK constraints were already dropped and recreated pointing to Production.MiscMaster
            // This migration is intentionally empty — only updates the EF snapshot
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            // No-op — tables were moved manually, not dropped by EF
        }
    }
}
