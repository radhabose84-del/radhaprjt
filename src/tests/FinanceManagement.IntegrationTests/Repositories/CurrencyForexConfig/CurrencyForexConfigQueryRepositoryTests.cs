using Contracts.Dtos.Lookups.Users;
using Contracts.Interfaces.Lookups.Users;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using FinanceManagement.IntegrationTests.Common;
using FinanceManagement.Infrastructure.Repositories.CurrencyForexConfig;
using static FinanceManagement.Domain.Common.BaseEntity;

namespace FinanceManagement.IntegrationTests.Repositories.CurrencyForexConfig
{
    [Collection("DatabaseCollection")]
    public sealed class CurrencyForexConfigQueryRepositoryTests
    {
        private readonly DbFixture _fixture;

        public CurrencyForexConfigQueryRepositoryTests(DbFixture fixture)
        {
            _fixture = fixture;
        }

        private CurrencyForexConfigQueryRepository CreateQueryRepo(Mock<ICompanyLookup>? companyLookup = null)
        {
            companyLookup ??= BuildDefaultCompanyLookup();
            var conn = new SqlConnection(_fixture.ConnectionString);
            return new CurrencyForexConfigQueryRepository(conn, companyLookup.Object);
        }

        private static Mock<ICompanyLookup> BuildDefaultCompanyLookup(int companyId = 1, string companyName = "Test Company")
        {
            var mock = new Mock<ICompanyLookup>(MockBehavior.Loose);
            mock.Setup(c => c.GetAllCompanyAsync())
                .ReturnsAsync(new List<CompanyLookupDto> { new() { CompanyId = companyId, CompanyName = companyName } });
            return mock;
        }

        private async Task<int> SeedAsync(string code = "FOREX", string name = "Forex", int companyId = 1)
        {
            await using var ctx = _fixture.CreateFreshDbContext();
            var repo = new CurrencyForexConfigCommandRepository(ctx);
            return await repo.CreateAsync(new Domain.Entities.CurrencyForexConfig
            {
                CompanyId = companyId,
                CurrencyTypeCode = code,
                CurrencyTypeName = name,
                IsActive = Status.Active,
                IsDeleted = IsDelete.NotDeleted
            });
        }

        private async Task ClearTableAsync() => await _fixture.ClearAllTablesAsync();

        // --- GET ALL ---

        [Fact]
        public async Task GetAllAsync_Should_Return_Seeded_Record()
        {
            await ClearTableAsync();
            await SeedAsync();

            var (items, total) = await CreateQueryRepo().GetAllAsync(1, 10, null, 1);

            items.Should().HaveCount(1);
            total.Should().Be(1);
        }

        [Fact]
        public async Task GetAllAsync_Should_Populate_CompanyName()
        {
            await ClearTableAsync();
            await SeedAsync();

            var (items, _) = await CreateQueryRepo(BuildDefaultCompanyLookup(1, "Acme Corp")).GetAllAsync(1, 10, null, 1);

            items[0].CompanyName.Should().Be("Acme Corp");
        }

        [Fact]
        public async Task GetAllAsync_Should_Exclude_SoftDeleted()
        {
            await ClearTableAsync();
            var id = await SeedAsync();
            await using (var ctx = _fixture.CreateFreshDbContext())
                await new CurrencyForexConfigCommandRepository(ctx).SoftDeleteAsync(id, CancellationToken.None);

            var (items, total) = await CreateQueryRepo().GetAllAsync(1, 10, null, 1);

            items.Should().BeEmpty();
            total.Should().Be(0);
        }

        [Fact]
        public async Task GetAllAsync_Should_Filter_By_SearchTerm()
        {
            await ClearTableAsync();
            await SeedAsync("INRONLY", "INR-only");
            await SeedAsync("FOREX", "Forex");

            var (items, _) = await CreateQueryRepo().GetAllAsync(1, 10, "Forex", 1);

            items.Should().HaveCount(1);
            items[0].CurrencyTypeCode.Should().Be("FOREX");
        }

        // --- GET BY ID ---

        [Fact]
        public async Task GetByIdAsync_Should_Return_Correct_Dto()
        {
            await ClearTableAsync();
            var id = await SeedAsync("FOREX", "Forex");

            var dto = await CreateQueryRepo().GetByIdAsync(id);

            dto.Should().NotBeNull();
            dto!.CurrencyTypeCode.Should().Be("FOREX");
            dto.CurrencyTypeName.Should().Be("Forex");
        }

        [Fact]
        public async Task GetByIdAsync_Should_Return_Null_When_SoftDeleted()
        {
            await ClearTableAsync();
            var id = await SeedAsync();
            await using (var ctx = _fixture.CreateFreshDbContext())
                await new CurrencyForexConfigCommandRepository(ctx).SoftDeleteAsync(id, CancellationToken.None);

            var dto = await CreateQueryRepo().GetByIdAsync(id);

            dto.Should().BeNull();
        }

        // --- ALREADY EXISTS ---

        [Fact]
        public async Task AlreadyExistsByCodeAsync_Should_Return_True_When_Duplicate()
        {
            await ClearTableAsync();
            await SeedAsync("FOREX", "Forex");

            var exists = await CreateQueryRepo().AlreadyExistsByCodeAsync("FOREX", 1);

            exists.Should().BeTrue();
        }

        [Fact]
        public async Task AlreadyExistsByCodeAsync_Should_Return_False_For_SoftDeleted()
        {
            await ClearTableAsync();
            var id = await SeedAsync("FOREX", "Forex");
            await using (var ctx = _fixture.CreateFreshDbContext())
                await new CurrencyForexConfigCommandRepository(ctx).SoftDeleteAsync(id, CancellationToken.None);

            var exists = await CreateQueryRepo().AlreadyExistsByCodeAsync("FOREX", 1);

            exists.Should().BeFalse();
        }

        [Fact]
        public async Task AlreadyExistsByNameAsync_Should_Return_True_When_Duplicate()
        {
            await ClearTableAsync();
            await SeedAsync("FOREX", "Forex");

            var exists = await CreateQueryRepo().AlreadyExistsByNameAsync("Forex", 1);

            exists.Should().BeTrue();
        }

        // --- AUTOCOMPLETE ---

        [Fact]
        public async Task AutocompleteAsync_Should_Return_Active_Records()
        {
            await ClearTableAsync();
            await SeedAsync("FOREX", "Forex");

            var results = await CreateQueryRepo().AutocompleteAsync("For", 1, CancellationToken.None);

            results.Should().ContainSingle();
            results[0].CurrencyTypeCode.Should().Be("FOREX");
        }

        [Fact]
        public async Task AutocompleteAsync_Should_Exclude_Inactive()
        {
            await ClearTableAsync();
            var id = await SeedAsync("FOREX", "Forex");
            await using (var ctx = _fixture.CreateFreshDbContext())
            {
                var entity = await ctx.CurrencyForexConfig.FirstAsync(x => x.Id == id);
                entity.IsActive = Status.Inactive;
                await ctx.SaveChangesAsync();
            }

            var results = await CreateQueryRepo().AutocompleteAsync("For", 1, CancellationToken.None);

            results.Should().BeEmpty();
        }

        // --- NOT FOUND / RULE 25 ---

        [Fact]
        public async Task NotFoundAsync_Should_Return_True_When_Missing()
        {
            await ClearTableAsync();

            var notFound = await CreateQueryRepo().NotFoundAsync(9999);

            notFound.Should().BeTrue();
        }

        [Fact]
        public async Task SoftDeleteValidationAsync_Should_Return_False_When_NoGlAccountReferences()
        {
            await ClearTableAsync();
            var id = await SeedAsync();

            var linked = await CreateQueryRepo().SoftDeleteValidationAsync(id);

            linked.Should().BeFalse();
        }

        [Fact]
        public async Task IsCurrencyForexConfigLinkedAsync_Should_Return_False_When_NoGlAccountReferences()
        {
            await ClearTableAsync();
            var id = await SeedAsync();

            var linked = await CreateQueryRepo().IsCurrencyForexConfigLinkedAsync(id);

            linked.Should().BeFalse();
        }
    }
}
