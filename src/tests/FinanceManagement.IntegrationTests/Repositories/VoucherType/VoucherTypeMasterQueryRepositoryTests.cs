using Contracts.Dtos.Lookups.Users;
using Contracts.Interfaces.Lookups.Users;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using FinanceManagement.Domain.Entities;
using FinanceManagement.IntegrationTests.Common;
using FinanceManagement.Infrastructure.Repositories.VoucherType;
using static FinanceManagement.Domain.Common.BaseEntity;

namespace FinanceManagement.IntegrationTests.Repositories.VoucherType
{
    [Collection("DatabaseCollection")]
    public sealed class VoucherTypeMasterQueryRepositoryTests
    {
        private readonly DbFixture _fixture;

        public VoucherTypeMasterQueryRepositoryTests(DbFixture fixture)
        {
            _fixture = fixture;
        }

        private VoucherTypeMasterQueryRepository CreateQueryRepo(
            Mock<ICompanyLookup>? companyLookup = null,
            Mock<IFinancialYearLookup>? fyLookup = null)
        {
            companyLookup ??= BuildCompanyLookup();
            fyLookup ??= BuildFinancialYearLookup();
            var conn = new SqlConnection(_fixture.ConnectionString);
            return new VoucherTypeMasterQueryRepository(conn, companyLookup.Object, fyLookup.Object);
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
            // Wide date range so the repo's "current FY by date" resolver (today ∈ [StartDate, EndDate])
            // always selects this year, regardless of when the test runs.
            var year = new FinancialYearLookupDto
            {
                FinancialYearId = 3,
                FinancialYearName = "2026-27",
                IsActive = true,
                StartDate = new DateTime(2000, 1, 1),
                EndDate = new DateTime(2100, 1, 1)
            };
            var mock = new Mock<IFinancialYearLookup>(MockBehavior.Loose);
            mock.Setup(f => f.GetAllFinancialYearAsync()).ReturnsAsync(new List<FinancialYearLookupDto> { year });
            mock.Setup(f => f.GetByIdAsync(3, It.IsAny<CancellationToken>())).ReturnsAsync(year);
            return mock;
        }

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

        private async Task<int> SeedVoucherTypeAsync(
            string code = "JV", string name = "Journal Voucher", int padding = 4, bool isSystem = false,
            int companyId = 1, IEnumerable<int>? accountTypeIds = null, int? fyId = 3)
        {
            await using var ctx = _fixture.CreateFreshDbContext();
            var repo = new VoucherTypeMasterCommandRepository(ctx);
            return await repo.CreateAsync(new VoucherTypeMaster
            {
                CompanyId = companyId,
                VoucherTypeCode = code,
                VoucherTypeName = name,
                NumberPadding = padding,
                IsSystem = isSystem,
                IsActive = Status.Active,
                IsDeleted = IsDelete.NotDeleted
            }, accountTypeIds ?? new List<int>(), fyId);
        }

        // --- GET ALL ---

        [Fact]
        public async Task GetAllAsync_Should_Return_Seeded_With_CompanyName_And_NextNumber()
        {
            await ClearTableAsync();
            var asset = await SeedAccountTypeAsync("Asset", "1");
            await SeedVoucherTypeAsync("JV", "Journal Voucher", accountTypeIds: new List<int> { asset });

            var (items, total) = await CreateQueryRepo(BuildCompanyLookup(1, "Acme Corp")).GetAllAsync(1, 10, null, 1);

            total.Should().Be(1);
            items.Should().HaveCount(1);
            items[0].CompanyName.Should().Be("Acme Corp");
            items[0].AllowedAccountTypes.Should().ContainSingle(a => a.AccountTypeName == "Asset");
            items[0].NextNumber.Should().Be("JV/2026-27/0001");
        }

        [Fact]
        public async Task GetAllAsync_Should_Exclude_SoftDeleted()
        {
            await ClearTableAsync();
            var asset = await SeedAccountTypeAsync("Asset", "1");
            var id = await SeedVoucherTypeAsync("JV", accountTypeIds: new List<int> { asset });
            await using (var ctx = _fixture.CreateFreshDbContext())
                await new VoucherTypeMasterCommandRepository(ctx).SoftDeleteAsync(id, CancellationToken.None);

            var (items, total) = await CreateQueryRepo().GetAllAsync(1, 10, null, 1);

            items.Should().BeEmpty();
            total.Should().Be(0);
        }

        [Fact]
        public async Task GetAllAsync_Should_Filter_By_SearchTerm()
        {
            await ClearTableAsync();
            await SeedVoucherTypeAsync("JV", "Journal Voucher");
            await SeedVoucherTypeAsync("PV", "Payment Voucher");

            var (items, _) = await CreateQueryRepo().GetAllAsync(1, 10, "Payment", 1);

            items.Should().HaveCount(1);
            items[0].VoucherTypeCode.Should().Be("PV");
        }

        // --- GET BY ID ---

        [Fact]
        public async Task GetByIdAsync_Should_Return_Dto_With_AllowedAccountTypes()
        {
            await ClearTableAsync();
            var asset = await SeedAccountTypeAsync("Asset", "1");
            var income = await SeedAccountTypeAsync("Income", "3");
            var id = await SeedVoucherTypeAsync("CN", "Credit Note", accountTypeIds: new List<int> { asset, income });

            var dto = await CreateQueryRepo().GetByIdAsync(id);

            dto.Should().NotBeNull();
            dto!.VoucherTypeCode.Should().Be("CN");
            dto.AllowedAccountTypes.Should().HaveCount(2);
        }

        [Fact]
        public async Task GetByIdAsync_Should_Return_Null_When_SoftDeleted()
        {
            await ClearTableAsync();
            var id = await SeedVoucherTypeAsync();
            await using (var ctx = _fixture.CreateFreshDbContext())
                await new VoucherTypeMasterCommandRepository(ctx).SoftDeleteAsync(id, CancellationToken.None);

            var dto = await CreateQueryRepo().GetByIdAsync(id);

            dto.Should().BeNull();
        }

        // --- AUTOCOMPLETE ---

        [Fact]
        public async Task AutocompleteAsync_Should_Exclude_Inactive()
        {
            await ClearTableAsync();
            var id = await SeedVoucherTypeAsync("JV", "Journal Voucher");
            await using (var ctx = _fixture.CreateFreshDbContext())
            {
                var entity = await ctx.VoucherTypeMaster.FirstAsync(x => x.Id == id);
                entity.IsActive = Status.Inactive;
                await ctx.SaveChangesAsync();
            }

            var results = await CreateQueryRepo().AutocompleteAsync("JV", 1, CancellationToken.None);

            results.Should().BeEmpty();
        }

        // --- EXISTS / NOT FOUND / FLAGS ---

        [Fact]
        public async Task AlreadyExistsByCodeAsync_Should_Return_True_When_Duplicate()
        {
            await ClearTableAsync();
            await SeedVoucherTypeAsync("JV");

            (await CreateQueryRepo().AlreadyExistsByCodeAsync("JV", 1)).Should().BeTrue();
        }

        [Fact]
        public async Task AlreadyExistsByCodeAsync_Should_Return_False_For_SoftDeleted()
        {
            await ClearTableAsync();
            var id = await SeedVoucherTypeAsync("JV");
            await using (var ctx = _fixture.CreateFreshDbContext())
                await new VoucherTypeMasterCommandRepository(ctx).SoftDeleteAsync(id, CancellationToken.None);

            (await CreateQueryRepo().AlreadyExistsByCodeAsync("JV", 1)).Should().BeFalse();
        }

        [Fact]
        public async Task NotFoundAsync_Should_Return_True_When_Missing()
        {
            await ClearTableAsync();
            (await CreateQueryRepo().NotFoundAsync(9999)).Should().BeTrue();
        }

        [Fact]
        public async Task IsSystemAsync_Should_Return_True_For_SystemType()
        {
            await ClearTableAsync();
            var id = await SeedVoucherTypeAsync("JV", isSystem: true);

            (await CreateQueryRepo().IsSystemAsync(id)).Should().BeTrue();
        }

        [Fact]
        public async Task AccountTypeExistsAsync_Should_Reflect_Existence()
        {
            await ClearTableAsync();
            var asset = await SeedAccountTypeAsync("Asset", "1");

            (await CreateQueryRepo().AccountTypeExistsAsync(asset, 1)).Should().BeTrue();
            (await CreateQueryRepo().AccountTypeExistsAsync(9999, 1)).Should().BeFalse();
        }

        // --- DELETE GUARD (Rule 25) ---

        [Fact]
        public async Task SoftDeleteValidationAsync_Should_Return_True_When_SeriesConsumed()
        {
            await ClearTableAsync();
            var id = await SeedVoucherTypeAsync("JV", fyId: 3);
            await using (var ctx = _fixture.CreateFreshDbContext())
            {
                var series = await ctx.VoucherTypeNumberSeries.FirstAsync(x => x.VoucherTypeId == id);
                series.LastUsedNumber = 5;
                await ctx.SaveChangesAsync();
            }

            (await CreateQueryRepo().SoftDeleteValidationAsync(id)).Should().BeTrue();
        }

        [Fact]
        public async Task SoftDeleteValidationAsync_Should_Return_False_When_SeriesUnused()
        {
            await ClearTableAsync();
            var id = await SeedVoucherTypeAsync("JV", fyId: 3);

            (await CreateQueryRepo().SoftDeleteValidationAsync(id)).Should().BeFalse();
        }

        // --- SUMMARY ---

        [Fact]
        public async Task GetSummaryAsync_Should_Count_Total_System_Custom()
        {
            await ClearTableAsync();
            await SeedVoucherTypeAsync("JV", isSystem: true);
            await SeedVoucherTypeAsync("PV", isSystem: true);
            await SeedVoucherTypeAsync("DN", isSystem: false);

            var summary = await CreateQueryRepo().GetSummaryAsync(1);

            summary.TotalCount.Should().Be(3);
            summary.SystemCount.Should().Be(2);
            summary.CustomCount.Should().Be(1);
            summary.ActiveCount.Should().Be(3);
        }

        // --- NUMBER SERIES ---

        [Fact]
        public async Task GetNumberSeriesAsync_Should_Return_NextNumber_Formatted()
        {
            await ClearTableAsync();
            var id = await SeedVoucherTypeAsync("JV", padding: 4, fyId: 3);
            await using (var ctx = _fixture.CreateFreshDbContext())
            {
                var series = await ctx.VoucherTypeNumberSeries.FirstAsync(x => x.VoucherTypeId == id);
                series.LastUsedNumber = 427;
                await ctx.SaveChangesAsync();
            }

            var rows = await CreateQueryRepo().GetNumberSeriesAsync(3, 1);

            rows.Should().ContainSingle();
            rows[0].LastUsedNumber.Should().Be(427);
            rows[0].NextNumberValue.Should().Be(428);
            rows[0].NextNumber.Should().Be("JV/2026-27/0428");
        }

        [Fact]
        public async Task GetNumberSeriesAsync_Should_Default_To_One_When_No_Counter()
        {
            await ClearTableAsync();
            await SeedVoucherTypeAsync("FAJ", padding: 4, fyId: null);  // no series row seeded

            var rows = await CreateQueryRepo().GetNumberSeriesAsync(3, 1);

            rows.Should().ContainSingle();
            rows[0].LastUsedNumber.Should().Be(0);
            rows[0].NextNumber.Should().Be("FAJ/2026-27/0001");
        }
    }
}
