using Dapper;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using SalesManagement.Infrastructure.Data;
using SalesManagement.Infrastructure.Repositories.MiscTypeMaster;
using SalesManagement.IntegrationTests.Common;
using static SalesManagement.Domain.Common.BaseEntity;

namespace SalesManagement.IntegrationTests.Repositories.MiscTypeMaster
{
    /// <summary>
    /// Integration tests for MiscTypeMasterQueryRepository.
    /// Verifies Dapper SQL queries (GetAll, GetById, AlreadyExists, NotFound, Autocomplete)
    /// against a real SQL Server database.
    /// MiscTypeMaster has no cross-module FK dependencies — no lookup mocks needed.
    /// </summary>
    [Collection("DatabaseCollection")]
    public sealed class MiscTypeMasterQueryRepositoryTests
    {
        private readonly DbFixture _fixture;

        public MiscTypeMasterQueryRepositoryTests(DbFixture fixture)
        {
            _fixture = fixture;
        }

        // ── Helpers ───────────────────────────────────────────────────────────

        private SqlConnection OpenConnection() => new SqlConnection(_fixture.ConnectionString);

        private MiscTypeMasterQueryRepository CreateQueryRepo()
        {
            var conn = new SqlConnection(_fixture.ConnectionString);
            return new MiscTypeMasterQueryRepository(conn);
        }

        private MiscTypeMasterCommandRepository CreateCommandRepo(ApplicationDbContext ctx)
            => new MiscTypeMasterCommandRepository(ctx);

        private Domain.Entities.MiscTypeMaster BuildEntity(
            string code = "MISC001",
            string description = "Test Misc Type",
            bool isActive = true) =>
            new Domain.Entities.MiscTypeMaster
            {
                MiscTypeCode = code,
                Description = description,
                IsActive = isActive ? Status.Active : Status.Inactive,
                IsDeleted = IsDelete.NotDeleted
            };

        private async Task ClearTableAsync()
        {
            await using var cnn = OpenConnection();
            await cnn.OpenAsync();
            await cnn.ExecuteAsync(@"
                DELETE FROM Sales.AgentCommissionSlab;
                DELETE FROM Sales.AgentCommissionPaymentTerm;
                DELETE FROM Sales.AgentCommissionSalesGroup;
                DELETE FROM Sales.AgentCommissionConfig;
                DELETE FROM Sales.CommissionSplitDetail;
                DELETE FROM Sales.CommissionSplit;
                DELETE FROM Sales.ItemPriceMaster;
                DELETE FROM Sales.MiscMaster;
                DELETE FROM Sales.MiscTypeMaster;");
        }

        private async Task<int> SeedEntityAsync(Domain.Entities.MiscTypeMaster entity)
        {
            await using var ctx = _fixture.CreateFreshDbContext();
            return await CreateCommandRepo(ctx).CreateAsync(entity);
        }

        // ── GetAllAsync ───────────────────────────────────────────────────────

        [Fact]
        public async Task GetAllAsync_Should_Return_Seeded_Records()
        {
            await ClearTableAsync();
            await SeedEntityAsync(BuildEntity("MISC001", "Type One"));
            await SeedEntityAsync(BuildEntity("MISC002", "Type Two"));

            var (data, totalCount) = await CreateQueryRepo().GetAllAsync(1, 10, null);

            totalCount.Should().Be(2);
            data.Should().HaveCount(2);
        }

        [Fact]
        public async Task GetAllAsync_Should_Support_Pagination()
        {
            await ClearTableAsync();
            for (int i = 1; i <= 5; i++)
                await SeedEntityAsync(BuildEntity($"MISC{i:D3}", $"Type {i}"));

            var (page1, total) = await CreateQueryRepo().GetAllAsync(1, 3, null);
            var (page2, _) = await CreateQueryRepo().GetAllAsync(2, 3, null);

            total.Should().Be(5);
            page1.Should().HaveCount(3);
            page2.Should().HaveCount(2);

            var page1Codes = page1.Select(x => x.MiscTypeCode).ToList();
            var page2Codes = page2.Select(x => x.MiscTypeCode).ToList();
            page1Codes.Should().NotIntersectWith(page2Codes);
        }

        [Fact]
        public async Task GetAllAsync_Should_Filter_By_SearchTerm()
        {
            await ClearTableAsync();
            await SeedEntityAsync(BuildEntity("ALPHA01", "Alpha Type"));
            await SeedEntityAsync(BuildEntity("BETA01", "Beta Type"));

            var (data, totalCount) = await CreateQueryRepo().GetAllAsync(1, 10, "ALPHA");

            totalCount.Should().Be(1);
            data.Should().HaveCount(1);
            data[0].MiscTypeCode.Should().Be("ALPHA01");
        }

        [Fact]
        public async Task GetAllAsync_Should_Not_Return_SoftDeleted_Records()
        {
            await ClearTableAsync();
            var id = await SeedEntityAsync(BuildEntity("DELTYPE01", "Deleted Type"));

            await using var ctx = _fixture.CreateFreshDbContext();
            await CreateCommandRepo(ctx).SoftDeleteAsync(id, CancellationToken.None);

            var (data, totalCount) = await CreateQueryRepo().GetAllAsync(1, 10, null);

            data.Should().NotContain(x => x.MiscTypeCode == "DELTYPE01");
            totalCount.Should().Be(0);
        }

        [Fact]
        public async Task GetAllAsync_Should_Return_Empty_WhenNoData()
        {
            await ClearTableAsync();

            var (data, totalCount) = await CreateQueryRepo().GetAllAsync(1, 10, null);

            data.Should().BeEmpty();
            totalCount.Should().Be(0);
        }

        // ── GetByIdAsync ──────────────────────────────────────────────────────

        [Fact]
        public async Task GetByIdAsync_Should_Return_Correct_DTO()
        {
            await ClearTableAsync();
            var id = await SeedEntityAsync(BuildEntity("BYID01", "ById Misc Type"));

            var dto = await CreateQueryRepo().GetByIdAsync(id);

            dto.Should().NotBeNull();
            dto!.Id.Should().Be(id);
            dto.MiscTypeCode.Should().Be("BYID01");
            dto.Description.Should().Be("ById Misc Type");
        }

        [Fact]
        public async Task GetByIdAsync_Should_Return_Null_WhenNotFound()
        {
            await ClearTableAsync();

            var dto = await CreateQueryRepo().GetByIdAsync(99999);

            dto.Should().BeNull();
        }

        [Fact]
        public async Task GetByIdAsync_Should_Return_Null_WhenSoftDeleted()
        {
            await ClearTableAsync();
            var id = await SeedEntityAsync(BuildEntity("SDEL01", "Soft Deleted Type"));

            await using var ctx = _fixture.CreateFreshDbContext();
            await CreateCommandRepo(ctx).SoftDeleteAsync(id, CancellationToken.None);

            var dto = await CreateQueryRepo().GetByIdAsync(id);

            dto.Should().BeNull();
        }

        // ── AlreadyExistsAsync ────────────────────────────────────────────────

        [Fact]
        public async Task AlreadyExistsAsync_Should_Return_True_WhenCodeExists()
        {
            await ClearTableAsync();
            await SeedEntityAsync(BuildEntity("EXISTS01"));

            var result = await CreateQueryRepo().AlreadyExistsAsync("EXISTS01");

            result.Should().BeTrue();
        }

        [Fact]
        public async Task AlreadyExistsAsync_Should_Return_False_WhenCodeDoesNotExist()
        {
            await ClearTableAsync();

            var result = await CreateQueryRepo().AlreadyExistsAsync("NOEXIST99");

            result.Should().BeFalse();
        }

        [Fact]
        public async Task AlreadyExistsAsync_Should_Return_False_WhenExcludedId_Matches()
        {
            await ClearTableAsync();
            var id = await SeedEntityAsync(BuildEntity("EXCL01"));

            // Excluding own ID means it's an update — not a duplicate
            var result = await CreateQueryRepo().AlreadyExistsAsync("EXCL01", id: id);

            result.Should().BeFalse();
        }

        [Fact]
        public async Task AlreadyExistsAsync_Should_Return_True_WhenDifferentRecord_HasSameCode()
        {
            await ClearTableAsync();
            await SeedEntityAsync(BuildEntity("DUP001"));
            var idB = await SeedEntityAsync(BuildEntity("DUP002"));

            // Record B checks if DUP001 is available — it belongs to Record A
            var result = await CreateQueryRepo().AlreadyExistsAsync("DUP001", id: idB);

            result.Should().BeTrue();
        }

        [Fact]
        public async Task AlreadyExistsAsync_Should_Return_False_ForSoftDeleted_Records()
        {
            await ClearTableAsync();
            var id = await SeedEntityAsync(BuildEntity("DELDUP01"));

            await using var ctx = _fixture.CreateFreshDbContext();
            await CreateCommandRepo(ctx).SoftDeleteAsync(id, CancellationToken.None);

            var result = await CreateQueryRepo().AlreadyExistsAsync("DELDUP01");

            result.Should().BeFalse();
        }

        // ── NotFoundAsync ─────────────────────────────────────────────────────

        [Fact]
        public async Task NotFoundAsync_Should_Return_False_WhenEntityExists()
        {
            await ClearTableAsync();
            var id = await SeedEntityAsync(BuildEntity());

            var result = await CreateQueryRepo().NotFoundAsync(id);

            result.Should().BeFalse(); // false = IS found
        }

        [Fact]
        public async Task NotFoundAsync_Should_Return_True_WhenEntityDoesNotExist()
        {
            await ClearTableAsync();

            var result = await CreateQueryRepo().NotFoundAsync(99999);

            result.Should().BeTrue(); // true = NOT found
        }

        [Fact]
        public async Task NotFoundAsync_Should_Return_True_AfterSoftDelete()
        {
            await ClearTableAsync();
            var id = await SeedEntityAsync(BuildEntity("NFDEL01"));

            await using var ctx = _fixture.CreateFreshDbContext();
            await CreateCommandRepo(ctx).SoftDeleteAsync(id, CancellationToken.None);

            var result = await CreateQueryRepo().NotFoundAsync(id);

            result.Should().BeTrue();
        }

        // ── AutocompleteAsync ─────────────────────────────────────────────────

        [Fact]
        public async Task AutocompleteAsync_Should_Return_Matching_Results()
        {
            await ClearTableAsync();
            await SeedEntityAsync(BuildEntity("ACM001", "Acme Type One"));
            await SeedEntityAsync(BuildEntity("ACM002", "Acme Type Two"));
            await SeedEntityAsync(BuildEntity("XYZ001", "XYZ Type"));

            var results = await CreateQueryRepo().AutocompleteAsync("ACM", CancellationToken.None);

            results.Should().HaveCount(2);
            results.Select(r => r.MiscTypeCode).Should().Contain(new[] { "ACM001", "ACM002" });
        }

        [Fact]
        public async Task AutocompleteAsync_Should_Return_Empty_WhenNoMatch()
        {
            await ClearTableAsync();
            await SeedEntityAsync(BuildEntity("MISC001", "Test Type"));

            var results = await CreateQueryRepo().AutocompleteAsync("ZZZNOMATCH", CancellationToken.None);

            results.Should().BeEmpty();
        }

        [Fact]
        public async Task AutocompleteAsync_Should_Not_Return_Inactive_Records()
        {
            await ClearTableAsync();
            await SeedEntityAsync(BuildEntity("ACTV01", "Active Type", isActive: true));
            await SeedEntityAsync(BuildEntity("INAC01", "Inactive Type", isActive: false));

            var results = await CreateQueryRepo().AutocompleteAsync("Type", CancellationToken.None);

            results.Should().NotContain(r => r.MiscTypeCode == "INAC01");
            results.Should().Contain(r => r.MiscTypeCode == "ACTV01");
        }

        [Fact]
        public async Task AutocompleteAsync_Should_Not_Return_SoftDeleted_Records()
        {
            await ClearTableAsync();
            var id = await SeedEntityAsync(BuildEntity("DLAUTO01", "Deleted Type"));

            await using var ctx = _fixture.CreateFreshDbContext();
            await CreateCommandRepo(ctx).SoftDeleteAsync(id, CancellationToken.None);

            var results = await CreateQueryRepo().AutocompleteAsync("Deleted", CancellationToken.None);

            results.Should().NotContain(r => r.MiscTypeCode == "DLAUTO01");
        }

        [Fact]
        public async Task AutocompleteAsync_Should_Match_By_Description()
        {
            await ClearTableAsync();
            await SeedEntityAsync(BuildEntity("MISC100", "Payment Terms"));

            var results = await CreateQueryRepo().AutocompleteAsync("Payment", CancellationToken.None);

            results.Should().HaveCount(1);
            results[0].Description.Should().Be("Payment Terms");
        }
    }
}
