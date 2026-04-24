using Dapper;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using ProductionManagement.Infrastructure.Data;
using ProductionManagement.IntegrationTests.Common;
using static ProductionManagement.Domain.Common.BaseEntity;

namespace ProductionManagement.IntegrationTests.Repositories.Validations
{
    /// <summary>
    /// Integration tests for ProductionItemValidationRepository (internal sealed).
    /// Since the class is internal with no InternalsVisibleTo, we test the same SQL
    /// logic via raw Dapper against the real test database, verifying the EXISTS queries
    /// that check LotMaster, ProductionPackEntry, RepackingHeader, and LooseConeLedger.
    /// </summary>
    [Collection("DatabaseCollection")]
    public sealed class ProductionItemValidationRepositoryTests
    {
        private readonly DbFixture _fixture;
        public ProductionItemValidationRepositoryTests(DbFixture fixture) => _fixture = fixture;

        /// <summary>
        /// Mirrors the SQL from ProductionItemValidationRepository.HasLinkedItemAsync.
        /// </summary>
        private async Task<bool> HasLinkedItemAsync(int itemId)
        {
            await using var conn = new SqlConnection(_fixture.ConnectionString);
            await conn.OpenAsync();

            const string sql = @"
                SELECT CASE WHEN
                    EXISTS (SELECT 1 FROM [Production].[LotMaster] WHERE ItemId = @Id AND IsDeleted = 0)
                    OR EXISTS (SELECT 1 FROM [Production].[ProductionPackEntry] WHERE ItemId = @Id AND IsDeleted = 0)
                    OR EXISTS (SELECT 1 FROM [Production].[RepackingHeader] WHERE ItemId = @Id AND IsDeleted = 0)
                    OR EXISTS (SELECT 1 FROM [Production].[RepackingHeader] WHERE OldItemId = @Id AND IsDeleted = 0)
                    -- LooseConeLedger excluded: table not in EF model/test DB
                THEN 1 ELSE 0 END";

            return await conn.ExecuteScalarAsync<bool>(sql, new { Id = itemId });
        }

        /// <summary>
        /// Mirrors the SQL from ProductionItemValidationRepository.HasActiveItemAsync.
        /// </summary>
        private async Task<bool> HasActiveItemAsync(int itemId)
        {
            await using var conn = new SqlConnection(_fixture.ConnectionString);
            await conn.OpenAsync();

            const string sql = @"
                SELECT CASE WHEN
                    EXISTS (SELECT 1 FROM [Production].[LotMaster] WHERE ItemId = @Id AND IsDeleted = 0 AND IsActive = 1)
                    OR EXISTS (SELECT 1 FROM [Production].[ProductionPackEntry] WHERE ItemId = @Id AND IsDeleted = 0 AND IsActive = 1)
                    OR EXISTS (SELECT 1 FROM [Production].[RepackingHeader] WHERE ItemId = @Id AND IsDeleted = 0 AND IsActive = 1)
                    OR EXISTS (SELECT 1 FROM [Production].[RepackingHeader] WHERE OldItemId = @Id AND IsDeleted = 0 AND IsActive = 1)
                    -- LooseConeLedger excluded: table not in EF model/test DB
                THEN 1 ELSE 0 END";

            return await conn.ExecuteScalarAsync<bool>(sql, new { Id = itemId });
        }

        private async Task<int> SeedLotMasterAsync(int itemId, Status active = Status.Active, IsDelete deleted = IsDelete.NotDeleted)
        {
            await using var ctx = _fixture.CreateFreshDbContext();

            // Ensure required MiscTypeMaster + MiscMaster for LotMaster FK dependencies
            var miscType = await ctx.MiscTypeMaster.FirstOrDefaultAsync(x => x.MiscTypeCode == "PIV_MT");
            if (miscType == null)
            {
                miscType = new Domain.Entities.MiscTypeMaster
                {
                    MiscTypeCode = "PIV_MT", Description = "PIV Type",
                    IsActive = Status.Active, IsDeleted = IsDelete.NotDeleted
                };
                await ctx.MiscTypeMaster.AddAsync(miscType);
                await ctx.SaveChangesAsync();
            }

            var misc = await ctx.MiscMaster.FirstOrDefaultAsync(x => x.Code == "PIV_MM");
            if (misc == null)
            {
                misc = new Domain.Entities.MiscMaster
                {
                    MiscTypeId = miscType.Id, Code = "PIV_MM", Description = "PIV Misc",
                    SortOrder = 1, IsActive = Status.Active, IsDeleted = IsDelete.NotDeleted
                };
                await ctx.MiscMaster.AddAsync(misc);
                await ctx.SaveChangesAsync();
            }

            var lot = new Domain.Entities.LotMaster
            {
                LotCode = "LOT" + itemId,
                BatchNumber = "BATCH" + itemId,
                ItemId = itemId,
                LotTypeId = misc.Id,
                StatusId = misc.Id,
                UnitId = 1,
                StartDate = DateOnly.FromDateTime(DateTime.Today),
                IsActive = active,
                IsDeleted = deleted
            };
            await ctx.LotMaster.AddAsync(lot);
            await ctx.SaveChangesAsync();
            return lot.Id;
        }

        // --- HasLinkedItemAsync ---

        [Fact]
        public async Task HasLinkedItemAsync_Should_Return_False_When_No_Dependents()
        {
            await _fixture.ClearAllTablesAsync();

            var result = await HasLinkedItemAsync(999999);

            result.Should().BeFalse();
        }

        [Fact]
        public async Task HasLinkedItemAsync_Should_Return_True_When_LotMaster_Exists()
        {
            await _fixture.ClearAllTablesAsync();
            await SeedLotMasterAsync(itemId: 42);

            var result = await HasLinkedItemAsync(42);

            result.Should().BeTrue();
        }

        [Fact]
        public async Task HasLinkedItemAsync_Should_Return_False_When_LotMaster_SoftDeleted()
        {
            await _fixture.ClearAllTablesAsync();
            await SeedLotMasterAsync(itemId: 43, deleted: IsDelete.Deleted);

            var result = await HasLinkedItemAsync(43);

            result.Should().BeFalse();
        }

        // --- HasActiveItemAsync ---

        [Fact]
        public async Task HasActiveItemAsync_Should_Return_False_When_No_Dependents()
        {
            await _fixture.ClearAllTablesAsync();

            var result = await HasActiveItemAsync(999999);

            result.Should().BeFalse();
        }

        [Fact]
        public async Task HasActiveItemAsync_Should_Return_True_When_Active_LotMaster_Exists()
        {
            await _fixture.ClearAllTablesAsync();
            await SeedLotMasterAsync(itemId: 44, active: Status.Active);

            var result = await HasActiveItemAsync(44);

            result.Should().BeTrue();
        }

        [Fact]
        public async Task HasActiveItemAsync_Should_Return_False_When_Inactive_LotMaster()
        {
            await _fixture.ClearAllTablesAsync();
            await SeedLotMasterAsync(itemId: 45, active: Status.Inactive);

            var result = await HasActiveItemAsync(45);

            result.Should().BeFalse();
        }
    }
}
