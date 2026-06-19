using Microsoft.EntityFrameworkCore;
using FinanceManagement.Domain.Entities;
using FinanceManagement.Infrastructure.Data;
using FinanceManagement.IntegrationTests.Common;
using FinanceManagement.Infrastructure.Repositories.VoucherType;
using static FinanceManagement.Domain.Common.BaseEntity;

namespace FinanceManagement.IntegrationTests.Repositories.VoucherType
{
    [Collection("DatabaseCollection")]
    public sealed class VoucherTypeMasterCommandRepositoryTests
    {
        private readonly DbFixture _fixture;

        public VoucherTypeMasterCommandRepositoryTests(DbFixture fixture)
        {
            _fixture = fixture;
        }

        private static VoucherTypeMasterCommandRepository CreateRepository(ApplicationDbContext ctx) => new(ctx);

        private async Task ClearTableAsync() => await _fixture.ClearAllTablesAsync();

        private async Task<int> SeedAccountTypeAsync(string name, string startCode, int companyId = 1)
        {
            await using var ctx = _fixture.CreateFreshDbContext();
            ctx.AccountTypeMaster.Add(new AccountTypeMaster
            {
                CompanyId = companyId,
                AccountTypeName = name,
                StartCode = startCode,
                AccountCodeLength = 6,
                SortOrder = 1,
                IsActive = Status.Active,
                IsDeleted = IsDelete.NotDeleted
            });
            await ctx.SaveChangesAsync();
            return (await ctx.AccountTypeMaster.FirstAsync(x => x.AccountTypeName == name && x.CompanyId == companyId)).Id;
        }

        private static VoucherTypeMaster BuildEntity(
            string code = "JV", string name = "Journal Voucher", int padding = 4, bool isSystem = false, int companyId = 1) =>
            new()
            {
                CompanyId = companyId,
                VoucherTypeCode = code,
                VoucherTypeName = name,
                NumberPadding = padding,
                IsSystem = isSystem,
                IsActive = Status.Active,
                IsDeleted = IsDelete.NotDeleted
            };

        // --- CREATE (aggregate) ---

        [Fact]
        public async Task CreateAsync_Should_Return_NewId_GreaterThanZero()
        {
            await ClearTableAsync();
            var atId = await SeedAccountTypeAsync("Asset", "1");
            await using var ctx = _fixture.CreateFreshDbContext();

            var newId = await CreateRepository(ctx).CreateAsync(BuildEntity(), new List<int> { atId }, 3);

            newId.Should().BeGreaterThan(0);
        }

        [Fact]
        public async Task CreateAsync_Should_Persist_AllowedAccountTypes()
        {
            await ClearTableAsync();
            var asset = await SeedAccountTypeAsync("Asset", "1");
            var expense = await SeedAccountTypeAsync("Expense", "4");
            await using var ctx = _fixture.CreateFreshDbContext();

            var newId = await CreateRepository(ctx).CreateAsync(BuildEntity(), new List<int> { asset, expense }, 3);
            ctx.ChangeTracker.Clear();

            var rows = await ctx.VoucherTypeAccountType.Where(x => x.VoucherTypeId == newId).ToListAsync();
            rows.Should().HaveCount(2);
            rows.Select(r => r.AccountTypeId).Should().BeEquivalentTo(new[] { asset, expense });
        }

        [Fact]
        public async Task CreateAsync_Should_Seed_NumberSeries_For_FiscalYear()
        {
            await ClearTableAsync();
            var atId = await SeedAccountTypeAsync("Asset", "1");
            await using var ctx = _fixture.CreateFreshDbContext();

            var newId = await CreateRepository(ctx).CreateAsync(BuildEntity(), new List<int> { atId }, 3);
            ctx.ChangeTracker.Clear();

            var series = await ctx.VoucherTypeNumberSeries.FirstOrDefaultAsync(x => x.VoucherTypeId == newId && x.FinancialYearId == 3);
            series.Should().NotBeNull();
            series!.LastUsedNumber.Should().Be(0);
        }

        [Fact]
        public async Task CreateAsync_Should_Populate_Audit_Fields()
        {
            await ClearTableAsync();
            var atId = await SeedAccountTypeAsync("Asset", "1");
            await using var ctx = _fixture.CreateFreshDbContext();

            var newId = await CreateRepository(ctx).CreateAsync(BuildEntity(), new List<int> { atId }, 3);
            ctx.ChangeTracker.Clear();

            var saved = await ctx.VoucherTypeMaster.FirstAsync(x => x.Id == newId);
            saved.CreatedBy.Should().Be(1);
            saved.CreatedByName.Should().Be("test-user");
            saved.CreatedDate.Should().NotBeNull();
        }

        // --- UPDATE (reconcile) ---

        [Fact]
        public async Task UpdateAsync_Should_Persist_NameAndPadding_NotCode()
        {
            await ClearTableAsync();
            var atId = await SeedAccountTypeAsync("Asset", "1");
            int id;
            await using (var ctx = _fixture.CreateFreshDbContext())
                id = await CreateRepository(ctx).CreateAsync(BuildEntity("JV", "Journal Voucher"), new List<int> { atId }, 3);

            await using (var ctx = _fixture.CreateFreshDbContext())
            {
                var entity = BuildEntity("HACKED", "Journal Voucher Edited", padding: 5);
                entity.Id = id;
                entity.IsActive = Status.Inactive;
                await CreateRepository(ctx).UpdateAsync(entity, new List<int> { atId });
            }

            await using var verify = _fixture.CreateFreshDbContext();
            var updated = await verify.VoucherTypeMaster.FirstAsync(x => x.Id == id);
            updated.VoucherTypeCode.Should().Be("JV");                 // immutable
            updated.VoucherTypeName.Should().Be("Journal Voucher Edited");
            updated.NumberPadding.Should().Be(5);
            updated.IsActive.Should().Be(Status.Inactive);
        }

        [Fact]
        public async Task UpdateAsync_Should_Reconcile_AccountTypes()
        {
            await ClearTableAsync();
            var asset = await SeedAccountTypeAsync("Asset", "1");
            var expense = await SeedAccountTypeAsync("Expense", "4");
            var income = await SeedAccountTypeAsync("Income", "3");
            int id;
            await using (var ctx = _fixture.CreateFreshDbContext())
                id = await CreateRepository(ctx).CreateAsync(BuildEntity(), new List<int> { asset, expense }, 3);

            // Drop expense, add income
            await using (var ctx = _fixture.CreateFreshDbContext())
            {
                var entity = BuildEntity();
                entity.Id = id;
                await CreateRepository(ctx).UpdateAsync(entity, new List<int> { asset, income });
            }

            await using var verify = _fixture.CreateFreshDbContext();
            var active = await verify.VoucherTypeAccountType
                .Where(x => x.VoucherTypeId == id && x.IsDeleted == IsDelete.NotDeleted)
                .Select(x => x.AccountTypeId)
                .ToListAsync();
            active.Should().BeEquivalentTo(new[] { asset, income });
        }

        // --- SOFT DELETE ---

        [Fact]
        public async Task SoftDeleteAsync_Should_Set_IsDeleted_Flag()
        {
            await ClearTableAsync();
            var atId = await SeedAccountTypeAsync("Asset", "1");
            int id;
            await using (var ctx = _fixture.CreateFreshDbContext())
                id = await CreateRepository(ctx).CreateAsync(BuildEntity(), new List<int> { atId }, 3);

            await using (var ctx = _fixture.CreateFreshDbContext())
                (await CreateRepository(ctx).SoftDeleteAsync(id, CancellationToken.None)).Should().BeTrue();

            await using var verify = _fixture.CreateFreshDbContext();
            var deleted = await verify.VoucherTypeMaster.IgnoreQueryFilters().FirstAsync(x => x.Id == id);
            deleted.IsDeleted.Should().Be(IsDelete.Deleted);
        }

        [Fact]
        public async Task SoftDeleteAsync_Should_Return_False_When_NotFound()
        {
            await ClearTableAsync();
            await using var ctx = _fixture.CreateFreshDbContext();

            var result = await CreateRepository(ctx).SoftDeleteAsync(9999, CancellationToken.None);

            result.Should().BeFalse();
        }

        // --- RESET SERIES ---

        [Fact]
        public async Task ResetSeriesAsync_Should_Reset_Existing_Counter_To_Zero()
        {
            await ClearTableAsync();
            var atId = await SeedAccountTypeAsync("Asset", "1");
            int id;
            await using (var ctx = _fixture.CreateFreshDbContext())
                id = await CreateRepository(ctx).CreateAsync(BuildEntity(), new List<int> { atId }, 3);

            // Bump the counter then reset
            await using (var ctx = _fixture.CreateFreshDbContext())
            {
                var series = await ctx.VoucherTypeNumberSeries.FirstAsync(x => x.VoucherTypeId == id && x.FinancialYearId == 3);
                series.LastUsedNumber = 427;
                await ctx.SaveChangesAsync();
            }

            await using (var ctx = _fixture.CreateFreshDbContext())
                await CreateRepository(ctx).ResetSeriesAsync(id, 3);

            await using var verify = _fixture.CreateFreshDbContext();
            var reset = await verify.VoucherTypeNumberSeries.FirstAsync(x => x.VoucherTypeId == id && x.FinancialYearId == 3);
            reset.LastUsedNumber.Should().Be(0);
        }

        [Fact]
        public async Task ResetSeriesAsync_Should_Create_Row_When_Missing()
        {
            await ClearTableAsync();
            var atId = await SeedAccountTypeAsync("Asset", "1");
            int id;
            await using (var ctx = _fixture.CreateFreshDbContext())
                id = await CreateRepository(ctx).CreateAsync(BuildEntity(), new List<int> { atId }, 3);

            // Reset a fiscal year (4) that has no row yet
            await using (var ctx = _fixture.CreateFreshDbContext())
                await CreateRepository(ctx).ResetSeriesAsync(id, 4);

            await using var verify = _fixture.CreateFreshDbContext();
            var created = await verify.VoucherTypeNumberSeries.FirstOrDefaultAsync(x => x.VoucherTypeId == id && x.FinancialYearId == 4);
            created.Should().NotBeNull();
            created!.LastUsedNumber.Should().Be(0);
        }
    }
}
