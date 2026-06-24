using Microsoft.EntityFrameworkCore;
using FinanceManagement.Domain.Entities;
using FinanceManagement.Infrastructure.Data;
using FinanceManagement.IntegrationTests.Common;
using FinanceManagement.Infrastructure.Repositories.JournalMaster.AccountingPeriod;
using static FinanceManagement.Domain.Common.BaseEntity;

namespace FinanceManagement.IntegrationTests.Repositories.AccountingPeriod
{
    [Collection("DatabaseCollection")]
    public sealed class AccountingPeriodCommandRepositoryTests
    {
        private readonly DbFixture _fixture;

        public AccountingPeriodCommandRepositoryTests(DbFixture fixture)
        {
            _fixture = fixture;
        }

        private static AccountingPeriodCommandRepository CreateRepository(ApplicationDbContext ctx) => new(ctx);

        private async Task ClearTableAsync() => await _fixture.ClearAllTablesAsync();

        // Seeds a PERIOD_STATUS MiscMaster row and returns its Id (FK target for AccountingPeriod.StatusId).
        private async Task<int> SeedStatusAsync(string code = "OPEN", string description = "Open")
        {
            await using var ctx = _fixture.CreateFreshDbContext();
            var type = new MiscTypeMaster
            {
                MiscTypeCode = "PERIOD_STATUS",
                Description = "Period Status",
                IsActive = Status.Active,
                IsDeleted = IsDelete.NotDeleted
            };
            ctx.MiscTypeMaster.Add(type);
            await ctx.SaveChangesAsync();

            var misc = new MiscMaster
            {
                MiscTypeId = type.Id,
                Code = code,
                Description = description,
                SortOrder = 1,
                IsActive = Status.Active,
                IsDeleted = IsDelete.NotDeleted
            };
            ctx.MiscMaster.Add(misc);
            await ctx.SaveChangesAsync();
            return misc.Id;
        }

        private static FinanceManagement.Domain.Entities.AccountingPeriod BuildEntity(
            int statusId, string name = "Jun 2026", int periodNo = 3, int financialYearId = 3, int companyId = 1) =>
            new()
            {
                CompanyId = companyId,
                FinancialYearId = financialYearId,
                PeriodName = name,
                PeriodNo = periodNo,
                StartDate = new DateOnly(2026, 6, 1),
                EndDate = new DateOnly(2026, 6, 30),
                StatusId = statusId,
                IsActive = Status.Active,
                IsDeleted = IsDelete.NotDeleted
            };

        [Fact]
        public async Task CreateAsync_Should_Return_NewId_GreaterThanZero()
        {
            await ClearTableAsync();
            var statusId = await SeedStatusAsync();
            await using var ctx = _fixture.CreateFreshDbContext();

            var newId = await CreateRepository(ctx).CreateAsync(BuildEntity(statusId));

            newId.Should().BeGreaterThan(0);
        }

        [Fact]
        public async Task CreateAsync_Should_Persist_Fields()
        {
            await ClearTableAsync();
            var statusId = await SeedStatusAsync();
            await using var ctx = _fixture.CreateFreshDbContext();

            var newId = await CreateRepository(ctx).CreateAsync(BuildEntity(statusId, "Jun 2026", 3, 3));
            ctx.ChangeTracker.Clear();

            var saved = await ctx.AccountingPeriod.FirstAsync(x => x.Id == newId);
            saved.PeriodName.Should().Be("Jun 2026");
            saved.PeriodNo.Should().Be(3);
            saved.FinancialYearId.Should().Be(3);
            saved.StatusId.Should().Be(statusId);
            saved.IsDeleted.Should().Be(IsDelete.NotDeleted);
        }

        [Fact]
        public async Task CreateAsync_Should_Populate_Audit_Fields()
        {
            await ClearTableAsync();
            var statusId = await SeedStatusAsync();
            await using var ctx = _fixture.CreateFreshDbContext();

            var newId = await CreateRepository(ctx).CreateAsync(BuildEntity(statusId));
            ctx.ChangeTracker.Clear();

            var saved = await ctx.AccountingPeriod.FirstAsync(x => x.Id == newId);
            saved.CreatedBy.Should().Be(1);
            saved.CreatedByName.Should().Be("test-user");
            saved.CreatedDate.Should().NotBeNull();
        }

        [Fact]
        public async Task UpdateAsync_Should_Persist_Mutable_Not_PeriodNo_Or_FinancialYear()
        {
            await ClearTableAsync();
            var statusId = await SeedStatusAsync();
            int id;
            await using (var ctx = _fixture.CreateFreshDbContext())
                id = await CreateRepository(ctx).CreateAsync(BuildEntity(statusId, "Jun 2026", 3, 3));

            await using (var ctx = _fixture.CreateFreshDbContext())
            {
                var entity = BuildEntity(statusId, "Jun 2026 Edited", periodNo: 9, financialYearId: 99);
                entity.Id = id;
                entity.IsActive = Status.Inactive;
                await CreateRepository(ctx).UpdateAsync(entity);
            }

            await using var verify = _fixture.CreateFreshDbContext();
            var updated = await verify.AccountingPeriod.FirstAsync(x => x.Id == id);
            updated.PeriodName.Should().Be("Jun 2026 Edited");
            updated.IsActive.Should().Be(Status.Inactive);
            updated.PeriodNo.Should().Be(3);            // immutable
            updated.FinancialYearId.Should().Be(3);     // immutable
        }

        [Fact]
        public async Task SoftDeleteAsync_Should_Set_IsDeleted_Flag()
        {
            await ClearTableAsync();
            var statusId = await SeedStatusAsync();
            int id;
            await using (var ctx = _fixture.CreateFreshDbContext())
                id = await CreateRepository(ctx).CreateAsync(BuildEntity(statusId));

            await using (var ctx = _fixture.CreateFreshDbContext())
                (await CreateRepository(ctx).SoftDeleteAsync(id, CancellationToken.None)).Should().BeTrue();

            await using var verify = _fixture.CreateFreshDbContext();
            var deleted = await verify.AccountingPeriod.IgnoreQueryFilters().FirstAsync(x => x.Id == id);
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
    }
}
