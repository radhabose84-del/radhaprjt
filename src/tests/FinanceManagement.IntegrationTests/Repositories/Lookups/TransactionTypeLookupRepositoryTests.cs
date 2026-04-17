using FinanceManagement.Domain.Common;
using FinanceManagement.Domain.Entities;
using FinanceManagement.Infrastructure.Repositories.Lookups.Finance;
using FinanceManagement.IntegrationTests.Common;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace FinanceManagement.IntegrationTests.Repositories.Lookups
{
    [Collection("DatabaseCollection")]
    public sealed class TransactionTypeLookupRepositoryTests
    {
        private readonly DbFixture _fixture;

        public TransactionTypeLookupRepositoryTests(DbFixture fixture)
        {
            _fixture = fixture;
        }

        private TransactionTypeLookupRepository CreateRepo()
        {
            var conn = new SqlConnection(_fixture.ConnectionString);
            return new TransactionTypeLookupRepository(conn);
        }

        private async Task<int> SeedAsync(
            string typeName,
            string shortName = "",
            int unitId = 1,
            int moduleId = 1,
            int menuId = 1,
            BaseEntity.Status active = BaseEntity.Status.Active,
            BaseEntity.IsDelete deleted = BaseEntity.IsDelete.NotDeleted)
        {
            // Derive unique short name from typeName when not supplied (avoids unique-index collision)
            var rawShort = typeName.Replace(" ", "");
            var derivedShort = string.IsNullOrEmpty(shortName)
                ? rawShort.Substring(0, Math.Min(rawShort.Length, 8))
                : shortName;
            await using var ctx = _fixture.CreateFreshDbContext();
            var entity = new FinanceManagement.Domain.Entities.TransactionTypeMaster
            {
                UnitId = unitId, ModuleId = moduleId, MenuId = menuId,
                TypeName = typeName, ShortName = derivedShort, Description = typeName,
                IsActive = active, IsDeleted = deleted
            };
            await ctx.TransactionTypeMaster.AddAsync(entity);
            await ctx.SaveChangesAsync();
            return entity.Id;
        }

        private async Task ClearAsync()
        {
            await using var ctx = _fixture.CreateFreshDbContext();
            var ds = await ctx.DocumentSequence.ToListAsync();
            ctx.DocumentSequence.RemoveRange(ds);
            await ctx.SaveChangesAsync();

            var rows = await ctx.TransactionTypeMaster.ToListAsync();
            ctx.TransactionTypeMaster.RemoveRange(rows);
            await ctx.SaveChangesAsync();
        }

        // --- GetAllTransactionTypeAsync ---

        [Fact]
        public async Task GetAllTransactionTypeAsync_Should_Return_Seeded_Records()
        {
            await ClearAsync();
            await SeedAsync("Sales Invoice", "SI");

            var result = await CreateRepo().GetAllTransactionTypeAsync();

            result.Should().Contain(t => t.TypeName == "Sales Invoice");
        }

        [Fact]
        public async Task GetAllTransactionTypeAsync_Should_Exclude_Inactive()
        {
            await ClearAsync();
            await SeedAsync("Active", "A");
            await SeedAsync("Inactive", "I", active: BaseEntity.Status.Inactive);

            var result = await CreateRepo().GetAllTransactionTypeAsync();

            result.Should().Contain(t => t.TypeName == "Active");
            result.Should().NotContain(t => t.TypeName == "Inactive");
        }

        [Fact]
        public async Task GetAllTransactionTypeAsync_Should_Exclude_SoftDeleted()
        {
            await ClearAsync();
            await SeedAsync("Keep", "K");
            await SeedAsync("Drop", "D", deleted: BaseEntity.IsDelete.Deleted);

            var result = await CreateRepo().GetAllTransactionTypeAsync();

            result.Should().Contain(t => t.TypeName == "Keep");
            result.Should().NotContain(t => t.TypeName == "Drop");
        }

        [Fact]
        public async Task GetAllTransactionTypeAsync_Should_Order_By_TypeName()
        {
            await ClearAsync();
            await SeedAsync("Zeta", "Z");
            await SeedAsync("Alpha", "A");
            await SeedAsync("Beta", "B");

            var result = await CreateRepo().GetAllTransactionTypeAsync();
            var seeded = result.Where(t => t.TypeName is "Alpha" or "Beta" or "Zeta").ToList();

            var indexAlpha = seeded.FindIndex(t => t.TypeName == "Alpha");
            var indexBeta = seeded.FindIndex(t => t.TypeName == "Beta");
            var indexZeta = seeded.FindIndex(t => t.TypeName == "Zeta");
            indexAlpha.Should().BeLessThan(indexBeta);
            indexBeta.Should().BeLessThan(indexZeta);
        }

        // --- GetByIdsAsync ---

        [Fact]
        public async Task GetByIdsAsync_Should_Return_Matching_Records()
        {
            await ClearAsync();
            var id1 = await SeedAsync("T1");
            var id2 = await SeedAsync("T2");
            await SeedAsync("T3");

            var result = await CreateRepo().GetByIdsAsync(new[] { id1, id2 });

            result.Should().HaveCount(2);
        }

        [Fact]
        public async Task GetByIdsAsync_Should_Return_Empty_When_Ids_Empty()
        {
            var result = await CreateRepo().GetByIdsAsync(Array.Empty<int>());

            result.Should().BeEmpty();
        }

        [Fact]
        public async Task GetByIdsAsync_Should_Exclude_SoftDeleted()
        {
            await ClearAsync();
            var id1 = await SeedAsync("K1");
            var id2 = await SeedAsync("K2", deleted: BaseEntity.IsDelete.Deleted);

            var result = await CreateRepo().GetByIdsAsync(new[] { id1, id2 });

            result.Should().HaveCount(1);
            result[0].Id.Should().Be(id1);
        }

        [Fact]
        public async Task GetByIdsAsync_Should_Include_Inactive_When_NotDeleted()
        {
            await ClearAsync();
            var id = await SeedAsync("InactiveKept", active: BaseEntity.Status.Inactive);

            var result = await CreateRepo().GetByIdsAsync(new[] { id });

            result.Should().HaveCount(1);
        }
    }
}
