using Dapper;
using MaintenanceManagement.Infrastructure.Repositories.Validations;
using Microsoft.Data.SqlClient;

namespace MaintenanceManagement.IntegrationTests.Repositories.Validations
{
    /// <summary>
    /// Integration tests for MaintenanceItemValidationRepository.
    /// Validates HasLinkedItemAsync and HasActiveItemAsync against
    /// PreventiveSchedulerItems.ItemId column.
    /// PreventiveSchedulerItems has a FK to PreventiveSchedulerHeader,
    /// so we use raw SQL with NOCHECK constraints for minimal seeding.
    /// </summary>
    [Collection("DatabaseCollection")]
    public sealed class MaintenanceItemValidationRepositoryTests
    {
        private readonly DbFixture _fixture;

        public MaintenanceItemValidationRepositoryTests(DbFixture fixture)
        {
            _fixture = fixture;
        }

        private MaintenanceItemValidationRepository CreateRepo()
        {
            var conn = new SqlConnection(_fixture.ConnectionString);
            return new MaintenanceItemValidationRepository(conn);
        }

        /// <summary>
        /// Seeds a minimal PreventiveSchedulerItems row via raw SQL with FK constraints temporarily disabled.
        /// </summary>
        private async Task SeedPreventiveSchedulerItemAsync(int itemId)
        {
            await using var conn = new SqlConnection(_fixture.ConnectionString);
            await conn.OpenAsync();

            await conn.ExecuteAsync(@"
                ALTER TABLE [Maintenance].[PreventiveSchedulerItems] NOCHECK CONSTRAINT ALL;");

            await conn.ExecuteAsync(@"
                INSERT INTO [Maintenance].[PreventiveSchedulerItems]
                    (PreventiveSchedulerHeaderId, ItemId, RequiredQty)
                VALUES
                    (1, @ItemId, 10);",
                new { ItemId = itemId });

            await conn.ExecuteAsync(@"
                ALTER TABLE [Maintenance].[PreventiveSchedulerItems] CHECK CONSTRAINT ALL;");
        }

        // --- HasLinkedItemAsync ---

        [Fact]
        public async Task HasLinkedItemAsync_Should_Return_True_When_SchedulerItem_Uses_ItemId()
        {
            await _fixture.ClearAllTablesAsync();
            await SeedPreventiveSchedulerItemAsync(itemId: 10);

            var result = await CreateRepo().HasLinkedItemAsync(10);

            result.Should().BeTrue();
        }

        [Fact]
        public async Task HasLinkedItemAsync_Should_Return_False_When_Unused()
        {
            await _fixture.ClearAllTablesAsync();

            var result = await CreateRepo().HasLinkedItemAsync(99999);

            result.Should().BeFalse();
        }

        // --- HasActiveItemAsync ---

        [Fact]
        public async Task HasActiveItemAsync_Should_Return_True_When_SchedulerItem_Exists()
        {
            await _fixture.ClearAllTablesAsync();
            await SeedPreventiveSchedulerItemAsync(itemId: 30);

            var result = await CreateRepo().HasActiveItemAsync(30);

            result.Should().BeTrue();
        }

        [Fact]
        public async Task HasActiveItemAsync_Should_Return_False_When_No_Items()
        {
            await _fixture.ClearAllTablesAsync();

            var result = await CreateRepo().HasActiveItemAsync(99999);

            result.Should().BeFalse();
        }
    }
}
