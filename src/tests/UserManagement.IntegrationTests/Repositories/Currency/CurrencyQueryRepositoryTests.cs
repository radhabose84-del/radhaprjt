using Contracts.Interfaces;
using Contracts.Interfaces.Validations.SalesManagement;
using Contracts.Interfaces.Validations.PurchaseManagement;
using Contracts.Interfaces.Validations.BudgetManagement;
using Contracts.Interfaces.Validations.ProjectManagement;
using UserManagement.Application.Common.Interfaces;
using UserManagement.Domain.Enums.Common;
using FluentAssertions;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Moq;
using UserManagement.Infrastructure.Data;
using UserManagement.Infrastructure.Repositories.Currency;
using UserManagement.IntegrationTests.Common;
using Xunit;

namespace UserManagement.IntegrationTests.Repositories.Currency
{
    [Collection("DatabaseCollection")]
    public sealed class CurrencyQueryRepositoryTests
    {
        private readonly DbFixture _fixture;

        public CurrencyQueryRepositoryTests(DbFixture fixture)
        {
            _fixture = fixture;
        }

        private ApplicationDbContext CreateDbContext()
        {
            var ipMock = new Mock<IIPAddressService>(MockBehavior.Loose);
            ipMock.Setup(x => x.GetCompanyId()).Returns(1);
            ipMock.Setup(x => x.GetUnitId()).Returns(1);
            ipMock.Setup(x => x.GetEntityId()).Returns(1);
            ipMock.Setup(x => x.GetGroupCode()).Returns("SUPER_ADMIN");
            ipMock.Setup(x => x.GetSystemIPAddress()).Returns("127.0.0.1");
            ipMock.Setup(x => x.GetUserId()).Returns(1);
            ipMock.Setup(x => x.GetUserName()).Returns("test-user");

            var tzMock = new Mock<ITimeZoneService>(MockBehavior.Loose);
            tzMock.Setup(x => x.GetSystemTimeZone()).Returns("UTC");

            var options = new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseSqlServer(_fixture.ConnectionString)
                .Options;

            return new ApplicationDbContext(options, ipMock.Object, tzMock.Object);
        }

        private CurrencyQueryRepository CreateQueryRepo()
        {
            var conn = new SqlConnection(_fixture.ConnectionString);
            var salesVal = new Mock<ISalesCurrencyValidation>(MockBehavior.Loose);
            var purchaseVal = new Mock<IPurchaseCurrencyValidation>(MockBehavior.Loose);
            var budgetVal = new Mock<IBudgetCurrencyValidation>(MockBehavior.Loose);
            var projectVal = new Mock<IProjectCurrencyValidation>(MockBehavior.Loose);
            return new CurrencyQueryRepository(conn, salesVal.Object, purchaseVal.Object, budgetVal.Object, projectVal.Object);
        }

        private async Task<int> SeedEntityAsync(string code = "USD", string name = "US Dollar")
        {
            await using var ctx = CreateDbContext();
            var repo = new CurrencyCommandRepository(ctx);
            return await repo.CreateAsync(new Domain.Entities.Currency
            {
                Code = code,
                Name = name,
                IsActive = Enums.Status.Active,
                IsDeleted = Enums.IsDelete.NotDeleted
            });
        }

        private async Task ClearTableAsync() =>
            await _fixture.ClearAllTablesAsync();

        // --- GET ALL ---

        [Fact]
        public async Task GetAllCurrencyAsync_Should_Return_Seeded_Record()
        {
            await ClearTableAsync();
            await SeedEntityAsync();

            var (items, total) = await CreateQueryRepo().GetAllCurrencyAsync(1, 10, null);

            items.Should().HaveCount(1);
            total.Should().Be(1);
        }

        [Fact]
        public async Task GetAllCurrencyAsync_Should_Exclude_SoftDeleted()
        {
            await ClearTableAsync();
            var id = await SeedEntityAsync();

            await using var ctx = CreateDbContext();
            var deleteModel = new Domain.Entities.Currency { IsDeleted = Enums.IsDelete.Deleted };
            await new CurrencyCommandRepository(ctx).DeletecurrencyAsync(id, deleteModel);

            var (items, total) = await CreateQueryRepo().GetAllCurrencyAsync(1, 10, null);

            items.Should().BeEmpty();
            total.Should().Be(0);
        }

        [Fact]
        public async Task GetAllCurrencyAsync_Should_Filter_By_SearchTerm()
        {
            await ClearTableAsync();
            await SeedEntityAsync("USD", "US Dollar");
            await SeedEntityAsync("EUR", "Euro");

            var (items, total) = await CreateQueryRepo().GetAllCurrencyAsync(1, 10, "Euro");

            items.Should().HaveCount(1);
            items[0].Name.Should().Be("Euro");
        }

        [Fact]
        public async Task GetAllCurrencyAsync_Should_Return_Correct_Pagination()
        {
            await ClearTableAsync();
            await SeedEntityAsync("USD", "US Dollar");
            await SeedEntityAsync("EUR", "Euro");
            await SeedEntityAsync("GBP", "British Pound");

            var (items, total) = await CreateQueryRepo().GetAllCurrencyAsync(1, 2, null);

            items.Should().HaveCount(2);
            total.Should().Be(3);
        }

        // --- GET BY ID ---

        [Fact]
        public async Task GetByIdAsync_Should_Return_Correct_Entity()
        {
            await ClearTableAsync();
            var id = await SeedEntityAsync("USD", "US Dollar");

            var result = await CreateQueryRepo().GetByIdAsync(id);

            result.Should().NotBeNull();
            result!.Code.Should().Be("USD");
            result.Name.Should().Be("US Dollar");
        }

        [Fact]
        public async Task GetByIdAsync_Should_Return_Null_When_SoftDeleted()
        {
            await ClearTableAsync();
            var id = await SeedEntityAsync();

            await using var ctx = CreateDbContext();
            var deleteModel = new Domain.Entities.Currency { IsDeleted = Enums.IsDelete.Deleted };
            await new CurrencyCommandRepository(ctx).DeletecurrencyAsync(id, deleteModel);

            var result = await CreateQueryRepo().GetByIdAsync(id);

            result.Should().BeNull();
        }

        [Fact]
        public async Task GetByIdAsync_Should_Return_Null_When_NotFound()
        {
            await ClearTableAsync();

            var result = await CreateQueryRepo().GetByIdAsync(9999);

            result.Should().BeNull();
        }

        // --- GET BY CURRENCY NAME (Autocomplete) ---

        [Fact]
        public async Task GetByCurrencyNameAsync_Should_Return_Matching_Records()
        {
            await ClearTableAsync();
            await SeedEntityAsync("USD", "US Dollar");
            await SeedEntityAsync("EUR", "Euro");

            var results = await CreateQueryRepo().GetByCurrencyNameAsync("Dollar");

            results.Should().HaveCount(1);
        }

        // --- GET BY IDS ---

        [Fact]
        public async Task GetByIdsAsync_Should_Return_Matching_Records()
        {
            await ClearTableAsync();
            var id1 = await SeedEntityAsync("USD", "US Dollar");
            var id2 = await SeedEntityAsync("EUR", "Euro");
            await SeedEntityAsync("GBP", "British Pound");

            var results = await CreateQueryRepo().GetByIdsAsync(new[] { id1, id2 });

            results.Should().HaveCount(2);
        }

        [Fact]
        public async Task GetByIdsAsync_Should_Return_Empty_For_Empty_Input()
        {
            await ClearTableAsync();

            var results = await CreateQueryRepo().GetByIdsAsync(Array.Empty<int>());

            results.Should().BeEmpty();
        }
    }
}
