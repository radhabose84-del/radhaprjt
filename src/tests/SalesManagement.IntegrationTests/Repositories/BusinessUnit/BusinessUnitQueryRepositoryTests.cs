using Dapper;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using SalesManagement.Infrastructure.Data;
using SalesManagement.Infrastructure.Repositories.BusinessUnit;
using SalesManagement.IntegrationTests.Common;
using static SalesManagement.Domain.Common.BaseEntity;

namespace SalesManagement.IntegrationTests.Repositories.BusinessUnit
{
    /// <summary>
    /// Integration tests for BusinessUnitQueryRepository.
    /// Verifies Dapper SQL queries (GetAll, GetById, AlreadyExists, NotFound, Autocomplete)
    /// against a real SQL Server database.
    /// BusinessUnit has no cross-module FK, so no lookup service is required.
    /// </summary>
    [Collection("DatabaseCollection")]
    public sealed class BusinessUnitQueryRepositoryTests
    {
        private readonly DbFixture _fixture;

        public BusinessUnitQueryRepositoryTests(DbFixture fixture)
        {
            _fixture = fixture;
        }

        // ── Helpers ───────────────────────────────────────────────────────────

        private SqlConnection OpenConnection() => new SqlConnection(_fixture.ConnectionString);

        private BusinessUnitQueryRepository CreateQueryRepo()
        {
            var conn = new SqlConnection(_fixture.ConnectionString);
            return new BusinessUnitQueryRepository(conn);
        }

        private BusinessUnitCommandRepository CreateCommandRepo(ApplicationDbContext ctx)
            => new BusinessUnitCommandRepository(ctx);

        private Domain.Entities.BusinessUnit BuildEntity(
            string code = "BU001",
            string name = "Test Business Unit",
            string description = "Test Description",
            bool isActive = true)
            => new Domain.Entities.BusinessUnit
            {
                BusinessUnitCode = code,
                BusinessUnitName = name,
                Description = description,
                IsActive = isActive ? Status.Active : Status.Inactive,
                IsDeleted = IsDelete.NotDeleted
            };

        private async Task ClearTableAsync() =>
            await _fixture.ClearTablesAsync("Sales.BusinessUnit");

        private async Task<int> SeedEntityAsync(Domain.Entities.BusinessUnit entity)
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
            await SeedEntityAsync(BuildEntity(code: "BU001", name: "Unit Alpha"));
            await SeedEntityAsync(BuildEntity(code: "BU002", name: "Unit Beta"));
            await SeedEntityAsync(BuildEntity(code: "BU003", name: "Unit Gamma"));

            var repo = CreateQueryRepo();
            var (data, totalCount) = await repo.GetAllAsync(pageNumber: 1, pageSize: 2, searchTerm: null);

            totalCount.Should().Be(3);
            data.Should().HaveCount(2);
        }

        [Fact]
        public async Task GetAllAsync_Should_Filter_BySearchTerm_OnCode()
        {
            await ClearTableAsync();
            await SeedEntityAsync(BuildEntity(code: "BU001", name: "Alpha Unit"));
            await SeedEntityAsync(BuildEntity(code: "BU002", name: "Beta Unit"));
            await SeedEntityAsync(BuildEntity(code: "MATCH01", name: "Match Unit"));

            var repo = CreateQueryRepo();
            var (data, totalCount) = await repo.GetAllAsync(pageNumber: 1, pageSize: 10, searchTerm: "MATCH");

            totalCount.Should().Be(1);
            data.Should().HaveCount(1);
            data[0].BusinessUnitCode.Should().Be("MATCH01");
        }

        [Fact]
        public async Task GetAllAsync_Should_Filter_BySearchTerm_OnName()
        {
            await ClearTableAsync();
            await SeedEntityAsync(BuildEntity(code: "BU010", name: "Northern Unit"));
            await SeedEntityAsync(BuildEntity(code: "BU011", name: "Southern Unit"));

            var repo = CreateQueryRepo();
            var (data, totalCount) = await repo.GetAllAsync(1, 10, "Northern");

            totalCount.Should().Be(1);
            data[0].BusinessUnitCode.Should().Be("BU010");
        }

        [Fact]
        public async Task GetAllAsync_Should_Not_Return_SoftDeleted_Records()
        {
            await ClearTableAsync();
            var id = await SeedEntityAsync(BuildEntity(code: "DEL001", name: "Deleted Unit"));

            await using var ctx = _fixture.CreateFreshDbContext();
            await CreateCommandRepo(ctx).SoftDeleteAsync(id, CancellationToken.None);

            var repo = CreateQueryRepo();
            var (data, _) = await repo.GetAllAsync(1, 10, null);

            data.Should().NotContain(x => x.BusinessUnitCode == "DEL001");
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
                await SeedEntityAsync(BuildEntity(code: $"BU{i:D3}", name: $"Unit {i}"));

            var repo = CreateQueryRepo();
            var (page1, total) = await repo.GetAllAsync(1, 3, null);
            var (page2, _) = await repo.GetAllAsync(2, 3, null);

            total.Should().Be(5);
            page1.Should().HaveCount(3);
            page2.Should().HaveCount(2);

            var page1Codes = page1.Select(x => x.BusinessUnitCode).ToList();
            var page2Codes = page2.Select(x => x.BusinessUnitCode).ToList();
            page1Codes.Should().NotIntersectWith(page2Codes);
        }

        // ── GetByIdAsync ──────────────────────────────────────────────────────

        [Fact]
        public async Task GetByIdAsync_Should_Return_Correct_DTO()
        {
            await ClearTableAsync();
            var id = await SeedEntityAsync(BuildEntity(code: "BYID01", name: "ById Unit", description: "ById Desc"));

            var repo = CreateQueryRepo();
            var dto = await repo.GetByIdAsync(id);

            dto.Should().NotBeNull();
            dto!.Id.Should().Be(id);
            dto.BusinessUnitCode.Should().Be("BYID01");
            dto.BusinessUnitName.Should().Be("ById Unit");
            dto.Description.Should().Be("ById Desc");
        }

        [Fact]
        public async Task GetByIdAsync_Should_Return_Empty_Description_WhenEmpty()
        {
            await ClearTableAsync();
            var id = await SeedEntityAsync(BuildEntity(code: "NODESC01", name: "Empty Desc Unit", description: ""));

            var repo = CreateQueryRepo();
            var dto = await repo.GetByIdAsync(id);

            dto.Should().NotBeNull();
            dto!.Description.Should().BeEmpty();
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
            var id = await SeedEntityAsync(BuildEntity(code: "AUDIT01", name: "Audit Unit"));

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
            await SeedEntityAsync(BuildEntity(code: "ACM001", name: "Acme Unit"));
            await SeedEntityAsync(BuildEntity(code: "ACM002", name: "Acme North"));
            await SeedEntityAsync(BuildEntity(code: "XYZ001", name: "XYZ Unit"));

            var repo = CreateQueryRepo();
            var results = await repo.AutocompleteAsync("ACM", CancellationToken.None);

            results.Should().HaveCount(2);
            results.Select(r => r.BusinessUnitCode).Should().Contain(new[] { "ACM001", "ACM002" });
        }

        [Fact]
        public async Task AutocompleteAsync_Should_Return_Empty_WhenNoMatch()
        {
            await ClearTableAsync();
            await SeedEntityAsync(BuildEntity(code: "BU001", name: "Unit One"));

            var repo = CreateQueryRepo();
            var results = await repo.AutocompleteAsync("ZZZNOMATCH", CancellationToken.None);

            results.Should().BeEmpty();
        }

        [Fact]
        public async Task AutocompleteAsync_Should_Not_Return_Inactive_Records()
        {
            await ClearTableAsync();
            await SeedEntityAsync(BuildEntity(code: "ACTV01", name: "Active Unit", isActive: true));
            await SeedEntityAsync(BuildEntity(code: "INAC01", name: "Inactive Unit", isActive: false));

            var repo = CreateQueryRepo();
            var results = await repo.AutocompleteAsync("Unit", CancellationToken.None);

            results.Should().NotContain(r => r.BusinessUnitCode == "INAC01");
            results.Should().Contain(r => r.BusinessUnitCode == "ACTV01");
        }

        [Fact]
        public async Task AutocompleteAsync_Should_Not_Return_SoftDeleted_Records()
        {
            await ClearTableAsync();
            var id = await SeedEntityAsync(BuildEntity(code: "DLAUTO01", name: "Deleted Auto Unit"));

            await using var ctx = _fixture.CreateFreshDbContext();
            await CreateCommandRepo(ctx).SoftDeleteAsync(id, CancellationToken.None);

            var repo = CreateQueryRepo();
            var results = await repo.AutocompleteAsync("Deleted", CancellationToken.None);

            results.Should().NotContain(r => r.BusinessUnitCode == "DLAUTO01");
        }

        [Fact]
        public async Task AutocompleteAsync_Should_Match_ByName_As_Well()
        {
            await ClearTableAsync();
            await SeedEntityAsync(BuildEntity(code: "BU100", name: "Eastern Business Unit"));

            var repo = CreateQueryRepo();
            var results = await repo.AutocompleteAsync("Eastern", CancellationToken.None);

            results.Should().HaveCount(1);
            results[0].BusinessUnitName.Should().Be("Eastern Business Unit");
        }

        [Fact]
        public async Task AutocompleteAsync_Should_Return_All_Active_WhenTermIsNull()
        {
            await ClearTableAsync();
            await SeedEntityAsync(BuildEntity(code: "BU201", name: "Unit One", isActive: true));
            await SeedEntityAsync(BuildEntity(code: "BU202", name: "Unit Two", isActive: true));
            await SeedEntityAsync(BuildEntity(code: "BU203", name: "Unit Three", isActive: false));

            var repo = CreateQueryRepo();
            var results = await repo.AutocompleteAsync(null, CancellationToken.None);

            results.Should().HaveCount(2);
            results.Should().NotContain(r => r.BusinessUnitCode == "BU203");
        }
    }
}
