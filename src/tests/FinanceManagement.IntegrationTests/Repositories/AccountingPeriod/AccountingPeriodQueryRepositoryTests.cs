using Contracts.Dtos.Lookups.Users;
using Contracts.Interfaces.Lookups.Users;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using FinanceManagement.Domain.Entities;
using FinanceManagement.IntegrationTests.Common;
using FinanceManagement.Infrastructure.Repositories.JournalMaster.AccountingPeriod;
using static FinanceManagement.Domain.Common.BaseEntity;

namespace FinanceManagement.IntegrationTests.Repositories.AccountingPeriod
{
    [Collection("DatabaseCollection")]
    public sealed class AccountingPeriodQueryRepositoryTests
    {
        private readonly DbFixture _fixture;

        public AccountingPeriodQueryRepositoryTests(DbFixture fixture)
        {
            _fixture = fixture;
        }

        private AccountingPeriodQueryRepository CreateQueryRepo(
            Mock<ICompanyLookup>? companyLookup = null,
            Mock<IFinancialYearLookup>? fyLookup = null)
        {
            companyLookup ??= BuildCompanyLookup();
            fyLookup ??= BuildFinancialYearLookup();
            var conn = new SqlConnection(_fixture.ConnectionString);
            return new AccountingPeriodQueryRepository(conn, companyLookup.Object, fyLookup.Object);
        }

        private static Mock<ICompanyLookup> BuildCompanyLookup(int companyId = 1, string companyName = "Test Company")
        {
            var mock = new Mock<ICompanyLookup>(MockBehavior.Loose);
            mock.Setup(c => c.GetAllCompanyAsync())
                .ReturnsAsync(new List<CompanyLookupDto> { new() { CompanyId = companyId, CompanyName = companyName } });
            return mock;
        }

        private static Mock<IFinancialYearLookup> BuildFinancialYearLookup()
        {
            var mock = new Mock<IFinancialYearLookup>(MockBehavior.Loose);
            mock.Setup(f => f.GetAllFinancialYearAsync()).ReturnsAsync(new List<FinancialYearLookupDto>
            {
                new() { FinancialYearId = 3, FinancialYearName = "2026-27", IsActive = true,
                        StartDate = new DateTime(2000, 1, 1), EndDate = new DateTime(2100, 1, 1) }
            });
            return mock;
        }

        private async Task ClearTableAsync() => await _fixture.ClearAllTablesAsync();

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

        private async Task<int> SeedPeriodAsync(
            int statusId, string name = "Jun 2026", int periodNo = 3, int fyId = 3, int companyId = 1, bool active = true)
        {
            await using var ctx = _fixture.CreateFreshDbContext();
            var repo = new AccountingPeriodCommandRepository(ctx);
            return await repo.CreateAsync(new FinanceManagement.Domain.Entities.AccountingPeriod
            {
                CompanyId = companyId,
                FinancialYearId = fyId,
                PeriodName = name,
                PeriodNo = periodNo,
                StartDate = new DateOnly(2026, 6, 1),
                EndDate = new DateOnly(2026, 6, 30),
                StatusId = statusId,
                IsActive = active ? Status.Active : Status.Inactive,
                IsDeleted = IsDelete.NotDeleted
            });
        }

        [Fact]
        public async Task GetAllAsync_Should_Return_Seeded_Record()
        {
            await ClearTableAsync();
            var statusId = await SeedStatusAsync();
            await SeedPeriodAsync(statusId);

            var (items, total) = await CreateQueryRepo().GetAllAsync(1, 10, null, 1, null);

            items.Should().HaveCount(1);
            total.Should().Be(1);
        }

        [Fact]
        public async Task GetAllAsync_Should_Populate_LookupNames()
        {
            await ClearTableAsync();
            var statusId = await SeedStatusAsync("OPEN", "Open");
            await SeedPeriodAsync(statusId);

            var (items, _) = await CreateQueryRepo().GetAllAsync(1, 10, null, 1, null);

            items[0].CompanyName.Should().Be("Test Company");
            items[0].FinancialYearName.Should().Be("2026-27");
            items[0].StatusName.Should().Be("Open");
        }

        [Fact]
        public async Task GetAllAsync_Should_Exclude_SoftDeleted()
        {
            await ClearTableAsync();
            var statusId = await SeedStatusAsync();
            var id = await SeedPeriodAsync(statusId);
            await using (var ctx = _fixture.CreateFreshDbContext())
                await new AccountingPeriodCommandRepository(ctx).SoftDeleteAsync(id, CancellationToken.None);

            var (items, total) = await CreateQueryRepo().GetAllAsync(1, 10, null, 1, null);

            items.Should().BeEmpty();
            total.Should().Be(0);
        }

        [Fact]
        public async Task GetAllAsync_Should_Filter_By_SearchTerm()
        {
            await ClearTableAsync();
            var statusId = await SeedStatusAsync();
            await SeedPeriodAsync(statusId, "Jun 2026", 3);
            await SeedPeriodAsync(statusId, "Jul 2026", 4);

            var (items, _) = await CreateQueryRepo().GetAllAsync(1, 10, "Jul", 1, null);

            items.Should().HaveCount(1);
            items[0].PeriodName.Should().Be("Jul 2026");
        }

        [Fact]
        public async Task GetByIdAsync_Should_Return_Correct_Dto()
        {
            await ClearTableAsync();
            var statusId = await SeedStatusAsync();
            var id = await SeedPeriodAsync(statusId, "Jun 2026", 3);

            var dto = await CreateQueryRepo().GetByIdAsync(id);

            dto.Should().NotBeNull();
            dto!.PeriodName.Should().Be("Jun 2026");
            dto.PeriodNo.Should().Be(3);
        }

        [Fact]
        public async Task GetByIdAsync_Should_Return_Null_When_SoftDeleted()
        {
            await ClearTableAsync();
            var statusId = await SeedStatusAsync();
            var id = await SeedPeriodAsync(statusId);
            await using (var ctx = _fixture.CreateFreshDbContext())
                await new AccountingPeriodCommandRepository(ctx).SoftDeleteAsync(id, CancellationToken.None);

            var dto = await CreateQueryRepo().GetByIdAsync(id);

            dto.Should().BeNull();
        }

        [Fact]
        public async Task AlreadyExistsAsync_Should_Return_True_For_Duplicate()
        {
            await ClearTableAsync();
            var statusId = await SeedStatusAsync();
            await SeedPeriodAsync(statusId, "Jun 2026", 3, fyId: 3);

            var exists = await CreateQueryRepo().AlreadyExistsAsync(1, 3, 3);

            exists.Should().BeTrue();
        }

        [Fact]
        public async Task AlreadyExistsAsync_Should_Return_False_For_DifferentPeriodNo()
        {
            await ClearTableAsync();
            var statusId = await SeedStatusAsync();
            await SeedPeriodAsync(statusId, "Jun 2026", 3, fyId: 3);

            var exists = await CreateQueryRepo().AlreadyExistsAsync(1, 3, 4);

            exists.Should().BeFalse();
        }

        [Fact]
        public async Task NotFoundAsync_Should_Return_True_For_Missing()
        {
            await ClearTableAsync();

            var notFound = await CreateQueryRepo().NotFoundAsync(9999);

            notFound.Should().BeTrue();
        }

        [Fact]
        public async Task StatusExistsAsync_Should_Return_True_For_PeriodStatus()
        {
            await ClearTableAsync();
            var statusId = await SeedStatusAsync();

            var exists = await CreateQueryRepo().StatusExistsAsync(statusId);

            exists.Should().BeTrue();
        }

        [Fact]
        public async Task StatusExistsAsync_Should_Return_False_For_Unknown()
        {
            await ClearTableAsync();
            await SeedStatusAsync();

            var exists = await CreateQueryRepo().StatusExistsAsync(9999);

            exists.Should().BeFalse();
        }

        [Fact]
        public async Task AutocompleteAsync_Should_Exclude_Inactive()
        {
            await ClearTableAsync();
            var statusId = await SeedStatusAsync();
            await SeedPeriodAsync(statusId, "Jun 2026", 3, active: false);

            var results = await CreateQueryRepo().AutocompleteAsync("Jun", 1, null, CancellationToken.None);

            results.Should().BeEmpty();
        }
    }
}
