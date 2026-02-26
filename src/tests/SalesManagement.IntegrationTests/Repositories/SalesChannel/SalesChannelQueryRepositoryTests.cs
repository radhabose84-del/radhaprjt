using Dapper;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using SalesManagement.Infrastructure.Data;
using SalesManagement.Infrastructure.Repositories.SalesChannel;
using SalesManagement.IntegrationTests.Common;
using static SalesManagement.Domain.Common.BaseEntity;

namespace SalesManagement.IntegrationTests.Repositories.SalesChannel
{
    /// <summary>
    /// Integration tests for SalesChannelQueryRepository.
    /// Verifies Dapper SQL queries (GetAll, GetById, AlreadyExists, NotFound, Autocomplete)
    /// against a real SQL Server database.
    /// SalesChannel has no cross-module FK, so no lookup service is required.
    /// </summary>
    [Collection("DatabaseCollection")]
    public sealed class SalesChannelQueryRepositoryTests
    {
        private readonly DbFixture _fixture;

        public SalesChannelQueryRepositoryTests(DbFixture fixture)
        {
            _fixture = fixture;
        }

        // ── Helpers ───────────────────────────────────────────────────────────

        private SqlConnection OpenConnection() => new SqlConnection(_fixture.ConnectionString);

        private SalesChannelQueryRepository CreateQueryRepo()
        {
            var conn = new SqlConnection(_fixture.ConnectionString);
            return new SalesChannelQueryRepository(conn);
        }

        private SalesChannelCommandRepository CreateCommandRepo(ApplicationDbContext ctx)
            => new SalesChannelCommandRepository(ctx);

        private Domain.Entities.SalesChannel BuildEntity(
            string code = "SC001",
            string name = "Test Sales Channel",
            bool isActive = true)
            => new Domain.Entities.SalesChannel
            {
                SalesChannelCode = code,
                SalesChannelName = name,
                IsActive = isActive ? Status.Active : Status.Inactive,
                IsDeleted = IsDelete.NotDeleted
            };

        private async Task ClearTableAsync()
        {
            await using var cnn = OpenConnection();
            await cnn.OpenAsync();
            await cnn.ExecuteAsync("DELETE FROM Sales.SalesItemPriceMaster");
            await cnn.ExecuteAsync("DELETE FROM Sales.SalesSegment");
            await cnn.ExecuteAsync("DELETE FROM Sales.SalesChannel");
        }

        private async Task<int> SeedEntityAsync(Domain.Entities.SalesChannel entity)
        {
            await using var ctx = _fixture.CreateFreshDbContext();
            var repo = CreateCommandRepo(ctx);
            return await repo.CreateAsync(entity);
        }

        // ── GetAllAsync ───────────────────────────────────────────────────────

        [Fact]
        public async Task GetAllAsync_Should_Return_PagedResults()
        {
            await ClearTableAsync();
            await SeedEntityAsync(BuildEntity(code: "SC001", name: "Channel Alpha"));
            await SeedEntityAsync(BuildEntity(code: "SC002", name: "Channel Beta"));
            await SeedEntityAsync(BuildEntity(code: "SC003", name: "Channel Gamma"));

            var repo = CreateQueryRepo();
            var (data, totalCount) = await repo.GetAllAsync(pageNumber: 1, pageSize: 2, searchTerm: null);

            totalCount.Should().Be(3);
            data.Should().HaveCount(2);
        }

        [Fact]
        public async Task GetAllAsync_Should_Filter_BySearchTerm()
        {
            await ClearTableAsync();
            await SeedEntityAsync(BuildEntity(code: "SC001", name: "Alpha Channel"));
            await SeedEntityAsync(BuildEntity(code: "SC002", name: "Beta Channel"));
            await SeedEntityAsync(BuildEntity(code: "MATCH01", name: "Match Channel"));

            var repo = CreateQueryRepo();
            var (data, totalCount) = await repo.GetAllAsync(pageNumber: 1, pageSize: 10, searchTerm: "MATCH");

            totalCount.Should().Be(1);
            data.Should().HaveCount(1);
            data[0].SalesChannelCode.Should().Be("MATCH01");
        }

        [Fact]
        public async Task GetAllAsync_Should_Not_Return_SoftDeleted_Records()
        {
            await ClearTableAsync();
            var id = await SeedEntityAsync(BuildEntity(code: "DEL001", name: "Deleted Channel"));

            await using var ctx = _fixture.CreateFreshDbContext();
            await CreateCommandRepo(ctx).SoftDeleteAsync(id, CancellationToken.None);

            var repo = CreateQueryRepo();
            var (data, totalCount) = await repo.GetAllAsync(1, 10, null);

            data.Should().NotContain(x => x.SalesChannelCode == "DEL001");
        }

        [Fact]
        public async Task GetAllAsync_Should_Return_Empty_WhenNoData()
        {
            await ClearTableAsync();

            var repo = CreateQueryRepo();
            var (data, totalCount) = await repo.GetAllAsync(1, 10, null);

            data.Should().BeEmpty();
            totalCount.Should().Be(0);
        }

        [Fact]
        public async Task GetAllAsync_Should_Support_Pagination_Page2()
        {
            await ClearTableAsync();
            for (int i = 1; i <= 5; i++)
                await SeedEntityAsync(BuildEntity(code: $"SC{i:D3}", name: $"Channel {i}"));

            var repo = CreateQueryRepo();
            var (page1, total) = await repo.GetAllAsync(1, 3, null);
            var (page2, _) = await repo.GetAllAsync(2, 3, null);

            total.Should().Be(5);
            page1.Should().HaveCount(3);
            page2.Should().HaveCount(2);

            var page1Codes = page1.Select(x => x.SalesChannelCode).ToList();
            var page2Codes = page2.Select(x => x.SalesChannelCode).ToList();
            page1Codes.Should().NotIntersectWith(page2Codes);
        }

        [Fact]
        public async Task GetAllAsync_Should_Filter_ByName_SearchTerm()
        {
            await ClearTableAsync();
            await SeedEntityAsync(BuildEntity(code: "SC010", name: "Northern Channel"));
            await SeedEntityAsync(BuildEntity(code: "SC011", name: "Southern Channel"));

            var repo = CreateQueryRepo();
            var (data, totalCount) = await repo.GetAllAsync(1, 10, "Northern");

            totalCount.Should().Be(1);
            data[0].SalesChannelCode.Should().Be("SC010");
        }

        // ── GetByIdAsync ──────────────────────────────────────────────────────

        [Fact]
        public async Task GetByIdAsync_Should_Return_Correct_DTO()
        {
            await ClearTableAsync();
            var id = await SeedEntityAsync(BuildEntity(code: "BYID01", name: "ById Channel"));

            var repo = CreateQueryRepo();
            var dto = await repo.GetByIdAsync(id);

            dto.Should().NotBeNull();
            dto!.Id.Should().Be(id);
            dto.SalesChannelCode.Should().Be("BYID01");
            dto.SalesChannelName.Should().Be("ById Channel");
        }

        [Fact]
        public async Task GetByIdAsync_Should_Return_Null_WhenNotFound()
        {
            await ClearTableAsync();

            var repo = CreateQueryRepo();
            var dto = await repo.GetByIdAsync(99999);

            dto.Should().BeNull();
        }

        [Fact]
        public async Task GetByIdAsync_Should_Return_Null_WhenSoftDeleted()
        {
            await ClearTableAsync();
            var id = await SeedEntityAsync(BuildEntity(code: "SDEL01"));

            await using var ctx = _fixture.CreateFreshDbContext();
            await CreateCommandRepo(ctx).SoftDeleteAsync(id, CancellationToken.None);

            var repo = CreateQueryRepo();
            var dto = await repo.GetByIdAsync(id);

            dto.Should().BeNull();
        }

        [Fact]
        public async Task GetByIdAsync_Should_Return_Audit_Fields()
        {
            await ClearTableAsync();
            var id = await SeedEntityAsync(BuildEntity(code: "AUDIT01", name: "Audit Channel"));

            var repo = CreateQueryRepo();
            var dto = await repo.GetByIdAsync(id);

            dto!.CreatedBy.Should().Be(1);
            dto.CreatedByName.Should().Be("test-user");
            dto.CreatedIP.Should().Be("127.0.0.1");
            dto.CreatedDate.Should().NotBeNull();
        }

        // ── AlreadyExistsAsync ────────────────────────────────────────────────

        [Fact]
        public async Task AlreadyExistsAsync_Should_Return_True_WhenCodeExists()
        {
            await ClearTableAsync();
            await SeedEntityAsync(BuildEntity(code: "EXISTS01"));

            var repo = CreateQueryRepo();
            var result = await repo.AlreadyExistsAsync("EXISTS01");

            result.Should().BeTrue();
        }

        [Fact]
        public async Task AlreadyExistsAsync_Should_Return_False_WhenCodeDoesNotExist()
        {
            await ClearTableAsync();

            var repo = CreateQueryRepo();
            var result = await repo.AlreadyExistsAsync("NOEXIST99");

            result.Should().BeFalse();
        }

        [Fact]
        public async Task AlreadyExistsAsync_Should_Return_False_WhenExcludedId_Matches()
        {
            await ClearTableAsync();
            var id = await SeedEntityAsync(BuildEntity(code: "EXCL01"));

            // Excluding the same ID means it's an update for that record — not a duplicate
            var repo = CreateQueryRepo();
            var result = await repo.AlreadyExistsAsync("EXCL01", id: id);

            result.Should().BeFalse();
        }

        [Fact]
        public async Task AlreadyExistsAsync_Should_Return_True_WhenDifferentRecord_HasSameCode()
        {
            await ClearTableAsync();
            await SeedEntityAsync(BuildEntity(code: "DUP01", name: "Record A"));
            var idB = await SeedEntityAsync(BuildEntity(code: "DUP02", name: "Record B"));

            // Record B tries to use DUP01 — that belongs to Record A
            var repo = CreateQueryRepo();
            var result = await repo.AlreadyExistsAsync("DUP01", id: idB);

            result.Should().BeTrue();
        }

        [Fact]
        public async Task AlreadyExistsAsync_Should_Return_False_ForDeleted_Records()
        {
            await ClearTableAsync();
            var id = await SeedEntityAsync(BuildEntity(code: "DELDUP01"));

            await using var ctx = _fixture.CreateFreshDbContext();
            await CreateCommandRepo(ctx).SoftDeleteAsync(id, CancellationToken.None);

            // After soft delete, the code should be available again
            var repo = CreateQueryRepo();
            var result = await repo.AlreadyExistsAsync("DELDUP01");

            result.Should().BeFalse();
        }

        // ── NotFoundAsync ─────────────────────────────────────────────────────

        [Fact]
        public async Task NotFoundAsync_Should_Return_False_WhenEntityExists()
        {
            await ClearTableAsync();
            var id = await SeedEntityAsync(BuildEntity());

            var repo = CreateQueryRepo();
            var result = await repo.NotFoundAsync(id);

            result.Should().BeFalse(); // false = entity IS found
        }

        [Fact]
        public async Task NotFoundAsync_Should_Return_True_WhenEntityDoesNotExist()
        {
            await ClearTableAsync();

            var repo = CreateQueryRepo();
            var result = await repo.NotFoundAsync(99999);

            result.Should().BeTrue(); // true = entity is NOT found
        }

        [Fact]
        public async Task NotFoundAsync_Should_Return_True_AfterSoftDelete()
        {
            await ClearTableAsync();
            var id = await SeedEntityAsync(BuildEntity(code: "NFDEL01"));

            await using var ctx = _fixture.CreateFreshDbContext();
            await CreateCommandRepo(ctx).SoftDeleteAsync(id, CancellationToken.None);

            var repo = CreateQueryRepo();
            var result = await repo.NotFoundAsync(id);

            result.Should().BeTrue(); // soft-deleted record is treated as not found
        }

        // ── AutocompleteAsync ─────────────────────────────────────────────────

        [Fact]
        public async Task AutocompleteAsync_Should_Return_Matching_Results()
        {
            await ClearTableAsync();
            await SeedEntityAsync(BuildEntity(code: "ACM001", name: "Acme Channel"));
            await SeedEntityAsync(BuildEntity(code: "ACM002", name: "Acme North"));
            await SeedEntityAsync(BuildEntity(code: "XYZ001", name: "XYZ Channel"));

            var repo = CreateQueryRepo();
            var results = await repo.AutocompleteAsync("ACM", CancellationToken.None);

            results.Should().HaveCount(2);
            results.Select(r => r.SalesChannelCode).Should().Contain(new[] { "ACM001", "ACM002" });
        }

        [Fact]
        public async Task AutocompleteAsync_Should_Return_Empty_WhenNoMatch()
        {
            await ClearTableAsync();
            await SeedEntityAsync(BuildEntity(code: "SC001", name: "Channel One"));

            var repo = CreateQueryRepo();
            var results = await repo.AutocompleteAsync("ZZZNOMATCH", CancellationToken.None);

            results.Should().BeEmpty();
        }

        [Fact]
        public async Task AutocompleteAsync_Should_Not_Return_Inactive_Records()
        {
            await ClearTableAsync();
            await SeedEntityAsync(BuildEntity(code: "ACTV01", name: "Active Channel", isActive: true));
            await SeedEntityAsync(BuildEntity(code: "INAC01", name: "Inactive Channel", isActive: false));

            var repo = CreateQueryRepo();
            var results = await repo.AutocompleteAsync("Channel", CancellationToken.None);

            results.Should().NotContain(r => r.SalesChannelCode == "INAC01");
            results.Should().Contain(r => r.SalesChannelCode == "ACTV01");
        }

        [Fact]
        public async Task AutocompleteAsync_Should_Not_Return_SoftDeleted_Records()
        {
            await ClearTableAsync();
            var id = await SeedEntityAsync(BuildEntity(code: "DLAUTO01", name: "Deleted Auto Channel"));

            await using var ctx = _fixture.CreateFreshDbContext();
            await CreateCommandRepo(ctx).SoftDeleteAsync(id, CancellationToken.None);

            var repo = CreateQueryRepo();
            var results = await repo.AutocompleteAsync("Deleted", CancellationToken.None);

            results.Should().NotContain(r => r.SalesChannelCode == "DLAUTO01");
        }

        [Fact]
        public async Task AutocompleteAsync_Should_Match_ByName_As_Well()
        {
            await ClearTableAsync();
            await SeedEntityAsync(BuildEntity(code: "SC100", name: "Western Sales Channel"));

            var repo = CreateQueryRepo();
            var results = await repo.AutocompleteAsync("Western", CancellationToken.None);

            results.Should().HaveCount(1);
            results[0].SalesChannelName.Should().Be("Western Sales Channel");
        }
    }
}
