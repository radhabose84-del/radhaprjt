using Contracts.Dtos.Lookups.Users;
using Contracts.Interfaces.Lookups.Users;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using ProductionManagement.Infrastructure.Repositories.YarnType;
using ProductionManagement.IntegrationTests.Common;
using static ProductionManagement.Domain.Common.BaseEntity;

namespace ProductionManagement.IntegrationTests.Repositories.YarnType
{
    [Collection("DatabaseCollection")]
    public sealed class YarnTypeQueryRepositoryTests
    {
        private readonly DbFixture _fixture;

        public YarnTypeQueryRepositoryTests(DbFixture fixture) => _fixture = fixture;

        private YarnTypeQueryRepository CreateQueryRepo(Mock<ICurrencyLookup>? currencyLookup = null)
        {
            currencyLookup ??= BuildDefaultCurrencyLookup();
            return new YarnTypeQueryRepository(new SqlConnection(_fixture.ConnectionString), currencyLookup.Object);
        }

        // Cross-module currency lookup is always mocked — never hit the UserManagement DB
        private static Mock<ICurrencyLookup> BuildDefaultCurrencyLookup(
            int currencyId = 1, string code = "INR", string name = "Indian Rupee")
        {
            var mock = new Mock<ICurrencyLookup>(MockBehavior.Loose);
            mock.Setup(c => c.GetByIdsAsync(It.IsAny<IEnumerable<int>>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new List<CurrencyLookupDto>
                {
                    new() { CurrencyId = currencyId, Code = code, Name = name }
                });
            return mock;
        }

        private static Mock<ICurrencyLookup> BuildEmptyCurrencyLookup()
        {
            var mock = new Mock<ICurrencyLookup>(MockBehavior.Loose);
            mock.Setup(c => c.GetByIdsAsync(It.IsAny<IEnumerable<int>>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new List<CurrencyLookupDto>());
            return mock;
        }

        private async Task<int> SeedAsync(
            string code = "YT001",
            string name = "Cotton",
            string desc = "Cotton yarn",
            decimal? additionalPrice = null,
            int? currencyId = null)
        {
            await using var ctx = _fixture.CreateFreshDbContext();
            var repo = new YarnTypeCommandRepository(ctx);
            return await repo.CreateAsync(new Domain.Entities.YarnType
            {
                YarnTypeCode = code,
                YarnTypeName = name,
                Description = desc,
                AdditionalPrice = additionalPrice,
                CurrencyId = currencyId,
                IsActive = Status.Active,
                IsDeleted = IsDelete.NotDeleted
            });
        }

        private async Task ClearTableAsync() =>
            await _fixture.ClearAllTablesAsync();

        // --- GET ALL ---

        [Fact]
        public async Task GetAllAsync_Should_Return_Seeded_Record()
        {
            await ClearTableAsync();
            await SeedAsync();

            var (items, total) = await CreateQueryRepo().GetAllAsync(1, 10, null);

            items.Should().HaveCount(1);
            total.Should().Be(1);
        }

        [Fact]
        public async Task GetAllAsync_Should_Exclude_SoftDeleted()
        {
            await ClearTableAsync();
            var id = await SeedAsync();

            await using var ctx = _fixture.CreateFreshDbContext();
            await new YarnTypeCommandRepository(ctx).SoftDeleteAsync(id, CancellationToken.None);

            var (items, total) = await CreateQueryRepo().GetAllAsync(1, 10, null);

            items.Should().BeEmpty();
            total.Should().Be(0);
        }

        [Fact]
        public async Task GetAllAsync_Should_Filter_By_SearchTerm()
        {
            await ClearTableAsync();
            await SeedAsync("YT001", "Cotton", "Cotton yarn");
            await SeedAsync("YT002", "Polyester", "Polyester yarn");

            var (items, _) = await CreateQueryRepo().GetAllAsync(1, 10, "Polyester");

            items.Should().HaveCount(1);
        }

        // --- GET BY ID ---

        [Fact]
        public async Task GetByIdAsync_Should_Return_Correct_Dto()
        {
            await ClearTableAsync();
            var id = await SeedAsync("YT001", "Cotton", "Cotton yarn");

            var dto = await CreateQueryRepo().GetByIdAsync(id);

            dto.Should().NotBeNull();
            dto!.YarnTypeCode.Should().Be("YT001");
            dto.YarnTypeName.Should().Be("Cotton");
        }

        [Fact]
        public async Task GetByIdAsync_Should_Return_Null_When_SoftDeleted()
        {
            await ClearTableAsync();
            var id = await SeedAsync();

            await using var ctx = _fixture.CreateFreshDbContext();
            await new YarnTypeCommandRepository(ctx).SoftDeleteAsync(id, CancellationToken.None);

            var dto = await CreateQueryRepo().GetByIdAsync(id);

            dto.Should().BeNull();
        }

        // --- AUTOCOMPLETE ---

        [Fact]
        public async Task AutocompleteAsync_Should_Return_Matching()
        {
            await ClearTableAsync();
            await SeedAsync("YT001", "Cotton");
            await SeedAsync("YT002", "Polyester");

            var results = await CreateQueryRepo().AutocompleteAsync("Cotton", CancellationToken.None);

            results.Should().HaveCount(1);
        }

        [Fact]
        public async Task AutocompleteAsync_Should_Exclude_Inactive()
        {
            await ClearTableAsync();
            var id = await SeedAsync("YT001", "Cotton");

            await using var ctx = _fixture.CreateFreshDbContext();
            var entity = await ctx.YarnType.FirstAsync(x => x.Id == id);
            entity.IsActive = Status.Inactive;
            await ctx.SaveChangesAsync();

            var results = await CreateQueryRepo().AutocompleteAsync("Cotton", CancellationToken.None);

            results.Should().BeEmpty();
        }

        // --- ALREADY EXISTS ---

        [Fact]
        public async Task AlreadyExistsAsync_Should_Return_True_When_Duplicate()
        {
            await ClearTableAsync();
            await SeedAsync("YT001");

            var exists = await CreateQueryRepo().AlreadyExistsAsync("YT001");

            exists.Should().BeTrue();
        }

        [Fact]
        public async Task AlreadyExistsAsync_Should_Return_False_For_SoftDeleted()
        {
            await ClearTableAsync();
            var id = await SeedAsync("YT001");

            await using var ctx = _fixture.CreateFreshDbContext();
            await new YarnTypeCommandRepository(ctx).SoftDeleteAsync(id, CancellationToken.None);

            var exists = await CreateQueryRepo().AlreadyExistsAsync("YT001");

            exists.Should().BeFalse();
        }

        // --- YARN TYPE NAME EXISTS ---

        [Fact]
        public async Task YarnTypeNameExistsAsync_Should_Return_True_When_Exists()
        {
            await ClearTableAsync();
            await SeedAsync("YT001", "Cotton");

            var exists = await CreateQueryRepo().YarnTypeNameExistsAsync("Cotton");

            exists.Should().BeTrue();
        }

        [Fact]
        public async Task YarnTypeNameExistsAsync_Should_Return_False_When_NotExists()
        {
            await ClearTableAsync();

            var exists = await CreateQueryRepo().YarnTypeNameExistsAsync("NonExistent");

            exists.Should().BeFalse();
        }

        // --- NOT FOUND ---

        [Fact]
        public async Task NotFoundAsync_Should_Return_True_When_Missing()
        {
            await ClearTableAsync();

            var notFound = await CreateQueryRepo().NotFoundAsync(99999);

            notFound.Should().BeTrue();
        }

        [Fact]
        public async Task NotFoundAsync_Should_Return_False_When_Exists()
        {
            await ClearTableAsync();
            var id = await SeedAsync();

            var notFound = await CreateQueryRepo().NotFoundAsync(id);

            notFound.Should().BeFalse();
        }

        // --- ADDITIONAL PRICE & CURRENCY ---

        [Fact]
        public async Task GetByIdAsync_Should_Return_AdditionalPrice()
        {
            await ClearTableAsync();
            var id = await SeedAsync("YT001", "Cotton", "Cotton yarn", additionalPrice: 12.3456m, currencyId: 1);

            var dto = await CreateQueryRepo().GetByIdAsync(id);

            dto.Should().NotBeNull();
            dto!.AdditionalPrice.Should().Be(12.3456m);
            dto.CurrencyId.Should().Be(1);
        }

        [Fact]
        public async Task GetByIdAsync_Should_Populate_CurrencyCode_And_Name_FromLookup()
        {
            await ClearTableAsync();
            var id = await SeedAsync("YT001", "Cotton", "Cotton yarn", additionalPrice: 50m, currencyId: 1);

            var dto = await CreateQueryRepo(BuildDefaultCurrencyLookup(1, "USD", "US Dollar")).GetByIdAsync(id);

            dto!.CurrencyCode.Should().Be("USD");
            dto.CurrencyName.Should().Be("US Dollar");
        }

        [Fact]
        public async Task GetByIdAsync_Should_LeaveCurrencyNamesNull_WhenNoCurrency()
        {
            await ClearTableAsync();
            var id = await SeedAsync("YT001", "Cotton", "Cotton yarn");

            var dto = await CreateQueryRepo().GetByIdAsync(id);

            dto!.CurrencyId.Should().BeNull();
            dto.CurrencyCode.Should().BeNull();
            dto.CurrencyName.Should().BeNull();
        }

        [Fact]
        public async Task GetAllAsync_Should_Populate_Currency_FromLookup()
        {
            await ClearTableAsync();
            await SeedAsync("YT001", "Cotton", "Cotton yarn", additionalPrice: 10m, currencyId: 1);

            var (items, _) = await CreateQueryRepo(BuildDefaultCurrencyLookup(1, "EUR", "Euro"))
                .GetAllAsync(1, 10, null);

            items.Should().HaveCount(1);
            items[0].AdditionalPrice.Should().Be(10m);
            items[0].CurrencyCode.Should().Be("EUR");
            items[0].CurrencyName.Should().Be("Euro");
        }

        [Fact]
        public async Task AutocompleteAsync_Should_Populate_Currency_FromLookup()
        {
            await ClearTableAsync();
            await SeedAsync("YT001", "Cotton", "Cotton yarn", additionalPrice: 7.5m, currencyId: 1);

            var results = await CreateQueryRepo(BuildDefaultCurrencyLookup(1, "INR", "Indian Rupee"))
                .AutocompleteAsync("Cotton", CancellationToken.None);

            results.Should().HaveCount(1);
            results[0].AdditionalPrice.Should().Be(7.5m);
            results[0].CurrencyCode.Should().Be("INR");
            results[0].CurrencyName.Should().Be("Indian Rupee");
        }

        // --- CURRENCY EXISTS ---

        [Fact]
        public async Task CurrencyExistsAsync_Should_Return_True_When_LookupHasMatch()
        {
            var exists = await CreateQueryRepo(BuildDefaultCurrencyLookup())
                .CurrencyExistsAsync(1, CancellationToken.None);

            exists.Should().BeTrue();
        }

        [Fact]
        public async Task CurrencyExistsAsync_Should_Return_False_When_LookupEmpty()
        {
            var exists = await CreateQueryRepo(BuildEmptyCurrencyLookup())
                .CurrencyExistsAsync(999, CancellationToken.None);

            exists.Should().BeFalse();
        }
    }
}
