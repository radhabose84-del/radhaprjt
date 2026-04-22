using Dapper;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using ProductionManagement.IntegrationTests.Common;
using static ProductionManagement.Domain.Common.BaseEntity;

namespace ProductionManagement.IntegrationTests.Repositories.Validations
{
    /// <summary>
    /// Integration tests for ProductionUomValidationRepository (internal sealed).
    /// Since the class is internal with no InternalsVisibleTo, we test the same SQL
    /// logic via raw Dapper against the real test database, verifying the EXISTS queries
    /// that check CountMaster for UOMId references.
    /// </summary>
    [Collection("DatabaseCollection")]
    public sealed class ProductionUomValidationRepositoryTests
    {
        private readonly DbFixture _fixture;
        public ProductionUomValidationRepositoryTests(DbFixture fixture) => _fixture = fixture;

        /// <summary>
        /// Mirrors the SQL from ProductionUomValidationRepository.HasLinkedUomAsync.
        /// </summary>
        private async Task<bool> HasLinkedUomAsync(int uomId)
        {
            await using var conn = new SqlConnection(_fixture.ConnectionString);
            await conn.OpenAsync();

            const string sql = @"
                SELECT CASE WHEN EXISTS (
                    SELECT 1 FROM [Production].[CountMaster]
                    WHERE UOMId = @Id AND IsDeleted = 0
                ) THEN 1 ELSE 0 END";

            return await conn.ExecuteScalarAsync<bool>(sql, new { Id = uomId });
        }

        /// <summary>
        /// Mirrors the SQL from ProductionUomValidationRepository.HasActiveUomAsync.
        /// </summary>
        private async Task<bool> HasActiveUomAsync(int uomId)
        {
            await using var conn = new SqlConnection(_fixture.ConnectionString);
            await conn.OpenAsync();

            const string sql = @"
                SELECT CASE WHEN EXISTS (
                    SELECT 1 FROM [Production].[CountMaster]
                    WHERE UOMId = @Id AND IsDeleted = 0 AND IsActive = 1
                ) THEN 1 ELSE 0 END";

            return await conn.ExecuteScalarAsync<bool>(sql, new { Id = uomId });
        }

        private async Task<int> EnsureMiscIdAsync()
        {
            await using var ctx = _fixture.CreateFreshDbContext();
            var miscType = await ctx.MiscTypeMaster.FirstOrDefaultAsync(x => x.MiscTypeCode == "PUV_MT");
            if (miscType == null)
            {
                miscType = new Domain.Entities.MiscTypeMaster
                {
                    MiscTypeCode = "PUV_MT", Description = "PUV Type",
                    IsActive = Status.Active, IsDeleted = IsDelete.NotDeleted
                };
                await ctx.MiscTypeMaster.AddAsync(miscType);
                await ctx.SaveChangesAsync();
            }

            var misc = await ctx.MiscMaster.FirstOrDefaultAsync(x => x.Code == "PUV_MM");
            if (misc == null)
            {
                misc = new Domain.Entities.MiscMaster
                {
                    MiscTypeId = miscType.Id, Code = "PUV_MM", Description = "PUV Misc",
                    SortOrder = 1, IsActive = Status.Active, IsDeleted = IsDelete.NotDeleted
                };
                await ctx.MiscMaster.AddAsync(misc);
                await ctx.SaveChangesAsync();
            }

            return misc.Id;
        }

        private async Task<int> SeedCountMasterAsync(int uomId, Status active = Status.Active, IsDelete deleted = IsDelete.NotDeleted)
        {
            var miscId = await EnsureMiscIdAsync();
            await using var ctx = _fixture.CreateFreshDbContext();

            var entity = new Domain.Entities.CountMaster
            {
                CountCode = "PUV" + uomId,
                CountValue = 20m,
                ShortName = "U",
                CountTypeId = miscId,
                CountCategoryId = miscId,
                CountDescription = "UomTest",
                UOMId = uomId,
                IsActive = active,
                IsDeleted = deleted
            };
            await ctx.CountMaster.AddAsync(entity);
            await ctx.SaveChangesAsync();
            return entity.Id;
        }

        // --- HasLinkedUomAsync ---

        [Fact]
        public async Task HasLinkedUomAsync_Should_Return_False_When_No_CountMaster_References()
        {
            await _fixture.ClearAllTablesAsync();

            var result = await HasLinkedUomAsync(999999);

            result.Should().BeFalse();
        }

        [Fact]
        public async Task HasLinkedUomAsync_Should_Return_True_When_CountMaster_References_UomId()
        {
            await _fixture.ClearAllTablesAsync();
            await SeedCountMasterAsync(uomId: 77);

            var result = await HasLinkedUomAsync(77);

            result.Should().BeTrue();
        }

        [Fact]
        public async Task HasLinkedUomAsync_Should_Return_False_When_CountMaster_SoftDeleted()
        {
            await _fixture.ClearAllTablesAsync();
            await SeedCountMasterAsync(uomId: 78, deleted: IsDelete.Deleted);

            var result = await HasLinkedUomAsync(78);

            result.Should().BeFalse();
        }

        [Fact]
        public async Task HasLinkedUomAsync_Should_Return_True_When_Inactive_CountMaster_Exists()
        {
            await _fixture.ClearAllTablesAsync();
            await SeedCountMasterAsync(uomId: 79, active: Status.Inactive);

            // HasLinkedUomAsync only checks IsDeleted = 0, NOT IsActive
            var result = await HasLinkedUomAsync(79);

            result.Should().BeTrue();
        }

        // --- HasActiveUomAsync ---

        [Fact]
        public async Task HasActiveUomAsync_Should_Return_False_When_No_CountMaster_References()
        {
            await _fixture.ClearAllTablesAsync();

            var result = await HasActiveUomAsync(999999);

            result.Should().BeFalse();
        }

        [Fact]
        public async Task HasActiveUomAsync_Should_Return_True_When_Active_CountMaster_Exists()
        {
            await _fixture.ClearAllTablesAsync();
            await SeedCountMasterAsync(uomId: 80, active: Status.Active);

            var result = await HasActiveUomAsync(80);

            result.Should().BeTrue();
        }

        [Fact]
        public async Task HasActiveUomAsync_Should_Return_False_When_Inactive_CountMaster()
        {
            await _fixture.ClearAllTablesAsync();
            await SeedCountMasterAsync(uomId: 81, active: Status.Inactive);

            var result = await HasActiveUomAsync(81);

            result.Should().BeFalse();
        }

        [Fact]
        public async Task HasActiveUomAsync_Should_Return_False_When_SoftDeleted_CountMaster()
        {
            await _fixture.ClearAllTablesAsync();
            await SeedCountMasterAsync(uomId: 82, deleted: IsDelete.Deleted);

            var result = await HasActiveUomAsync(82);

            result.Should().BeFalse();
        }
    }
}
