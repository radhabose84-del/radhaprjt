using Dapper;
using Microsoft.Data.SqlClient;
using SalesManagement.Infrastructure.Data;
using SalesManagement.Infrastructure.Repositories.MovementTypeConfig;
using SalesManagement.IntegrationTests.Common;
using static SalesManagement.Domain.Common.BaseEntity;

namespace SalesManagement.IntegrationTests.Repositories.MovementTypeConfig
{
    [Collection("DatabaseCollection")]
    public sealed class MovementTypeConfigQueryRepositoryTests
    {
        private readonly DbFixture _fixture;

        public MovementTypeConfigQueryRepositoryTests(DbFixture fixture)
        {
            _fixture = fixture;
        }

        private MovementTypeConfigQueryRepository CreateQueryRepo()
            => new MovementTypeConfigQueryRepository(new SqlConnection(_fixture.ConnectionString));

        private MovementTypeConfigCommandRepository CreateCommandRepo(ApplicationDbContext ctx)
            => new MovementTypeConfigCommandRepository(ctx);

        private Domain.Entities.MovementTypeConfig BuildEntity(
            string code = "MTC001",
            string description = "Test Movement Config",
            bool isActive = true)
            => new Domain.Entities.MovementTypeConfig
            {
                MovementCode = code,
                MovementDescription = description,
                MovementCategoryId = 1,
                FromStockTypeId = 2,
                ToStockTypeId = 3,
                QuantityUpdateFlag = true,
                ValueUpdateFlag = false,
                IsActive = isActive ? Status.Active : Status.Inactive,
                IsDeleted = IsDelete.NotDeleted
            };

        private async Task ClearTableAsync()
        {
            await using var conn = new SqlConnection(_fixture.ConnectionString);
            await conn.OpenAsync();
            await conn.ExecuteAsync("DELETE FROM Sales.StoTypeMaster");
            await conn.ExecuteAsync("DELETE FROM Sales.MovementTypeConfig");
        }

        private async Task<int> SeedAsync(Domain.Entities.MovementTypeConfig entity)
        {
            await using var ctx = _fixture.CreateFreshDbContext();
            return await CreateCommandRepo(ctx).CreateAsync(entity);
        }

        // ── GetAllAsync ──────────────────────────────────────────────────────

        [Fact]
        public async Task GetAllAsync_Should_Return_SeededRecord()
        {
            await ClearTableAsync();
            await SeedAsync(BuildEntity("MTC001", "Config One"));

            var (items, total) = await CreateQueryRepo().GetAllAsync(1, 10, null);

            total.Should().Be(1);
            items.Should().HaveCount(1);
            items[0].MovementCode.Should().Be("MTC001");
        }

        [Fact]
        public async Task GetAllAsync_Should_Exclude_SoftDeleted()
        {
            await ClearTableAsync();
            var id = await SeedAsync(BuildEntity("DEL001"));

            await using var ctx = _fixture.CreateFreshDbContext();
            await CreateCommandRepo(ctx).SoftDeleteAsync(id, CancellationToken.None);

            var (items, total) = await CreateQueryRepo().GetAllAsync(1, 10, null);

            items.Should().NotContain(x => x.MovementCode == "DEL001");
            total.Should().Be(0);
        }

        [Fact]
        public async Task GetAllAsync_Should_Filter_BySearchTerm_OnCode()
        {
            await ClearTableAsync();
            await SeedAsync(BuildEntity("ALPHA001", "Alpha Config"));
            await SeedAsync(BuildEntity("BETA001", "Beta Config"));

            var (items, total) = await CreateQueryRepo().GetAllAsync(1, 10, "ALPHA");

            total.Should().Be(1);
            items[0].MovementCode.Should().Be("ALPHA001");
        }

        [Fact]
        public async Task GetAllAsync_Should_Filter_BySearchTerm_OnDescription()
        {
            await ClearTableAsync();
            await SeedAsync(BuildEntity("MTC010", "Outbound Transfer"));
            await SeedAsync(BuildEntity("MTC011", "Inbound Receipt"));

            var (items, total) = await CreateQueryRepo().GetAllAsync(1, 10, "Outbound");

            total.Should().Be(1);
            items[0].MovementDescription.Should().Be("Outbound Transfer");
        }

        [Fact]
        public async Task GetAllAsync_Should_Support_Pagination()
        {
            await ClearTableAsync();
            for (int i = 1; i <= 5; i++)
                await SeedAsync(BuildEntity($"PAG{i:D3}", $"Page Config {i}"));

            var (page1, total) = await CreateQueryRepo().GetAllAsync(1, 3, null);
            var (page2, _) = await CreateQueryRepo().GetAllAsync(2, 3, null);

            total.Should().Be(5);
            page1.Should().HaveCount(3);
            page2.Should().HaveCount(2);
        }

        [Fact]
        public async Task GetAllAsync_Should_Return_Empty_WhenNoData()
        {
            await ClearTableAsync();

            var (items, total) = await CreateQueryRepo().GetAllAsync(1, 10, null);

            items.Should().BeEmpty();
            total.Should().Be(0);
        }

        // ── GetByIdAsync ─────────────────────────────────────────────────────

        [Fact]
        public async Task GetByIdAsync_Should_Return_Correct_Dto()
        {
            await ClearTableAsync();
            var id = await SeedAsync(BuildEntity("BYID001", "ById Config"));

            var dto = await CreateQueryRepo().GetByIdAsync(id);

            dto.Should().NotBeNull();
            dto!.Id.Should().Be(id);
            dto.MovementCode.Should().Be("BYID001");
            dto.MovementDescription.Should().Be("ById Config");
        }

        [Fact]
        public async Task GetByIdAsync_Should_Return_Null_WhenNotFound()
        {
            await ClearTableAsync();

            var dto = await CreateQueryRepo().GetByIdAsync(99999);

            dto.Should().BeNull();
        }

        [Fact]
        public async Task GetByIdAsync_Should_Return_Null_AfterSoftDelete()
        {
            await ClearTableAsync();
            var id = await SeedAsync(BuildEntity("SDEL001"));

            await using var ctx = _fixture.CreateFreshDbContext();
            await CreateCommandRepo(ctx).SoftDeleteAsync(id, CancellationToken.None);

            var dto = await CreateQueryRepo().GetByIdAsync(id);

            dto.Should().BeNull();
        }

        [Fact]
        public async Task GetByIdAsync_Should_Return_Audit_Fields()
        {
            await ClearTableAsync();
            var id = await SeedAsync(BuildEntity("AUD001"));

            var dto = await CreateQueryRepo().GetByIdAsync(id);

            dto!.CreatedBy.Should().Be(1);
            dto.CreatedByName.Should().Be("test-user");
            dto.CreatedIP.Should().Be("127.0.0.1");
            dto.CreatedDate.Should().NotBeNull();
        }

        // ── AlreadyExistsAsync ───────────────────────────────────────────────

        [Fact]
        public async Task AlreadyExistsAsync_Should_Return_True_WhenCodeExists()
        {
            await ClearTableAsync();
            await SeedAsync(BuildEntity("EXISTS001"));

            var result = await CreateQueryRepo().AlreadyExistsAsync("EXISTS001");

            result.Should().BeTrue();
        }

        [Fact]
        public async Task AlreadyExistsAsync_Should_Return_False_WhenCodeDoesNotExist()
        {
            await ClearTableAsync();

            var result = await CreateQueryRepo().AlreadyExistsAsync("NOEXIST999");

            result.Should().BeFalse();
        }

        [Fact]
        public async Task AlreadyExistsAsync_Should_Return_False_WhenExcludedId_Matches()
        {
            await ClearTableAsync();
            var id = await SeedAsync(BuildEntity("EXCL001"));

            var result = await CreateQueryRepo().AlreadyExistsAsync("EXCL001", id);

            result.Should().BeFalse();
        }

        [Fact]
        public async Task AlreadyExistsAsync_Should_Return_False_ForSoftDeleted()
        {
            await ClearTableAsync();
            var id = await SeedAsync(BuildEntity("DELDUP001"));

            await using var ctx = _fixture.CreateFreshDbContext();
            await CreateCommandRepo(ctx).SoftDeleteAsync(id, CancellationToken.None);

            var result = await CreateQueryRepo().AlreadyExistsAsync("DELDUP001");

            result.Should().BeFalse();
        }

        // ── NotFoundAsync ────────────────────────────────────────────────────

        [Fact]
        public async Task NotFoundAsync_Should_Return_False_WhenEntityExists()
        {
            await ClearTableAsync();
            var id = await SeedAsync(BuildEntity());

            var result = await CreateQueryRepo().NotFoundAsync(id);

            result.Should().BeFalse();
        }

        [Fact]
        public async Task NotFoundAsync_Should_Return_True_WhenEntityDoesNotExist()
        {
            await ClearTableAsync();

            var result = await CreateQueryRepo().NotFoundAsync(99999);

            result.Should().BeTrue();
        }

        [Fact]
        public async Task NotFoundAsync_Should_Return_True_AfterSoftDelete()
        {
            await ClearTableAsync();
            var id = await SeedAsync(BuildEntity());

            await using var ctx = _fixture.CreateFreshDbContext();
            await CreateCommandRepo(ctx).SoftDeleteAsync(id, CancellationToken.None);

            var result = await CreateQueryRepo().NotFoundAsync(id);

            result.Should().BeTrue();
        }

        // ── AutocompleteAsync ────────────────────────────────────────────────

        [Fact]
        public async Task AutocompleteAsync_Should_Return_MatchingResults()
        {
            await ClearTableAsync();
            await SeedAsync(BuildEntity("ACM001", "Autocomplete Match One"));
            await SeedAsync(BuildEntity("XYZ001", "Other Config"));

            var results = await CreateQueryRepo().AutocompleteAsync("Autocomplete", CancellationToken.None);

            results.Should().HaveCount(1);
            results[0].MovementCode.Should().Be("ACM001");
        }

        [Fact]
        public async Task AutocompleteAsync_Should_Exclude_Inactive()
        {
            await ClearTableAsync();
            await SeedAsync(BuildEntity("ACTV001", "Active Config", isActive: true));
            await SeedAsync(BuildEntity("INAC001", "Inactive Config", isActive: false));

            var results = await CreateQueryRepo().AutocompleteAsync("Config", CancellationToken.None);

            results.Should().NotContain(r => r.MovementCode == "INAC001");
            results.Should().Contain(r => r.MovementCode == "ACTV001");
        }

        [Fact]
        public async Task AutocompleteAsync_Should_Exclude_SoftDeleted()
        {
            await ClearTableAsync();
            var id = await SeedAsync(BuildEntity("DLAUTO001", "Deleted Config"));

            await using var ctx = _fixture.CreateFreshDbContext();
            await CreateCommandRepo(ctx).SoftDeleteAsync(id, CancellationToken.None);

            var results = await CreateQueryRepo().AutocompleteAsync("Deleted", CancellationToken.None);

            results.Should().NotContain(r => r.MovementCode == "DLAUTO001");
        }
    }
}
