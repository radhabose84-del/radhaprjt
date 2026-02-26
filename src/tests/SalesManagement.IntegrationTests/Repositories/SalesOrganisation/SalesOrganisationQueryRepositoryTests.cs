using Contracts.Dtos.Lookups.Users;
using Contracts.Interfaces.Lookups.Users;
using Dapper;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using SalesManagement.Infrastructure.Data;
using SalesManagement.Infrastructure.Repositories.SalesOrganisation;
using SalesManagement.IntegrationTests.Common;
using static SalesManagement.Domain.Common.BaseEntity;

namespace SalesManagement.IntegrationTests.Repositories.SalesOrganisation
{
    /// <summary>
    /// Integration tests for SalesOrganisationQueryRepository.
    /// Verifies Dapper SQL queries (GetAll, GetById, AlreadyExists, NotFound, Autocomplete)
    /// against a real SQL Server database.
    /// ICompanyLookup is mocked to isolate from cross-module dependency.
    /// </summary>
    [Collection("DatabaseCollection")]
    public sealed class SalesOrganisationQueryRepositoryTests
    {
        private readonly DbFixture _fixture;

        public SalesOrganisationQueryRepositoryTests(DbFixture fixture)
        {
            _fixture = fixture;
        }

        // ── Helpers ───────────────────────────────────────────────────────────

        private SqlConnection OpenConnection() => new SqlConnection(_fixture.ConnectionString);

        private SalesOrganisationQueryRepository CreateQueryRepo(Mock<ICompanyLookup> companyLookup = null)
        {
            companyLookup ??= BuildDefaultCompanyLookup();
            var conn = new SqlConnection(_fixture.ConnectionString);
            return new SalesOrganisationQueryRepository(conn, companyLookup.Object);
        }

        private SalesOrganisationCommandRepository CreateCommandRepo(ApplicationDbContext ctx)
            => new SalesOrganisationCommandRepository(ctx);

        private Mock<ICompanyLookup> BuildDefaultCompanyLookup(int companyId = 1, string companyName = "Test Company")
        {
            var mock = new Mock<ICompanyLookup>(MockBehavior.Loose);
            mock.Setup(c => c.GetAllCompanyAsync())
                .ReturnsAsync(new List<CompanyLookupDto>
                {
                    new CompanyLookupDto { CompanyId = companyId, CompanyName = companyName }
                });
            return mock;
        }

        private Domain.Entities.SalesOrganisation BuildEntity(
            string code = "SO001",
            string name = "Test Sales Org",
            int companyId = 1,
            bool isActive = true,
            string description = null)
            => new Domain.Entities.SalesOrganisation
            {
                SalesOrganisationCode = code,
                SalesOrganisationName = name,
                CompanyId = companyId,
                Description = description,
                IsActive = isActive ? Status.Active : Status.Inactive,
                IsDeleted = IsDelete.NotDeleted
            };

        private async Task ClearTableAsync()
        {
            await using var cnn = OpenConnection();
            await cnn.OpenAsync();
            await cnn.ExecuteAsync("DELETE FROM Sales.SalesItemPriceMaster");
            await cnn.ExecuteAsync("DELETE FROM Sales.SalesSegment");
            await cnn.ExecuteAsync("DELETE FROM Sales.SalesOrganisation");
        }

        private async Task<int> SeedEntityAsync(Domain.Entities.SalesOrganisation entity)
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
            await SeedEntityAsync(BuildEntity(code: "SO001", name: "Sales Org Alpha"));
            await SeedEntityAsync(BuildEntity(code: "SO002", name: "Sales Org Beta"));
            await SeedEntityAsync(BuildEntity(code: "SO003", name: "Sales Org Gamma"));

            var repo = CreateQueryRepo();
            var (data, totalCount) = await repo.GetAllAsync(pageNumber: 1, pageSize: 2, searchTerm: null);

            totalCount.Should().Be(3);
            data.Should().HaveCount(2);
        }

        [Fact]
        public async Task GetAllAsync_Should_Filter_BySearchTerm()
        {
            await ClearTableAsync();
            await SeedEntityAsync(BuildEntity(code: "SO001", name: "Alpha Sales"));
            await SeedEntityAsync(BuildEntity(code: "SO002", name: "Beta Sales"));
            await SeedEntityAsync(BuildEntity(code: "MATCH01", name: "Match Org"));

            var repo = CreateQueryRepo();
            var (data, totalCount) = await repo.GetAllAsync(pageNumber: 1, pageSize: 10, searchTerm: "MATCH");

            totalCount.Should().Be(1);
            data.Should().HaveCount(1);
            data[0].SalesOrganisationCode.Should().Be("MATCH01");
        }

        [Fact]
        public async Task GetAllAsync_Should_Not_Return_SoftDeleted_Records()
        {
            await ClearTableAsync();
            var id = await SeedEntityAsync(BuildEntity(code: "DEL001", name: "Deleted Org"));

            // Soft delete via command repo
            await using var ctx = _fixture.CreateFreshDbContext();
            await CreateCommandRepo(ctx).SoftDeleteAsync(id, CancellationToken.None);

            var repo = CreateQueryRepo();
            var (data, totalCount) = await repo.GetAllAsync(1, 10, null);

            data.Should().NotContain(x => x.SalesOrganisationCode == "DEL001");
        }

        [Fact]
        public async Task GetAllAsync_Should_PopulateCompanyName_Via_Lookup()
        {
            await ClearTableAsync();
            await SeedEntityAsync(BuildEntity(code: "SO010", companyId: 1));

            var companyLookup = BuildDefaultCompanyLookup(companyId: 1, companyName: "Acme Corp");
            var repo = CreateQueryRepo(companyLookup);

            var (data, _) = await repo.GetAllAsync(1, 10, null);

            data.Should().NotBeEmpty();
            data[0].CompanyName.Should().Be("Acme Corp");
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
                await SeedEntityAsync(BuildEntity(code: $"SO{i:D3}", name: $"Org {i}"));

            var repo = CreateQueryRepo();
            var (page1, total) = await repo.GetAllAsync(1, 3, null);
            var (page2, _) = await repo.GetAllAsync(2, 3, null);

            total.Should().Be(5);
            page1.Should().HaveCount(3);
            page2.Should().HaveCount(2);

            // No overlap between pages
            var page1Codes = page1.Select(x => x.SalesOrganisationCode).ToList();
            var page2Codes = page2.Select(x => x.SalesOrganisationCode).ToList();
            page1Codes.Should().NotIntersectWith(page2Codes);
        }

        // ── GetByIdAsync ──────────────────────────────────────────────────────

        [Fact]
        public async Task GetByIdAsync_Should_Return_Correct_DTO()
        {
            await ClearTableAsync();
            var id = await SeedEntityAsync(BuildEntity(code: "BYID01", name: "ById Org", companyId: 1, description: "ById Desc"));

            var repo = CreateQueryRepo();
            var dto = await repo.GetByIdAsync(id);

            dto.Should().NotBeNull();
            dto!.Id.Should().Be(id);
            dto.SalesOrganisationCode.Should().Be("BYID01");
            dto.SalesOrganisationName.Should().Be("ById Org");
            dto.Description.Should().Be("ById Desc");
        }

        [Fact]
        public async Task GetByIdAsync_Should_PopulateCompanyName_Via_Lookup()
        {
            await ClearTableAsync();
            var id = await SeedEntityAsync(BuildEntity(code: "BYLK01", companyId: 1));

            var companyLookup = BuildDefaultCompanyLookup(companyId: 1, companyName: "Lookup Company");
            var repo = CreateQueryRepo(companyLookup);

            var dto = await repo.GetByIdAsync(id);

            dto!.CompanyName.Should().Be("Lookup Company");
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
            await SeedEntityAsync(BuildEntity(code: "ACM001", name: "Acme Sales"));
            await SeedEntityAsync(BuildEntity(code: "ACM002", name: "Acme North"));
            await SeedEntityAsync(BuildEntity(code: "XYZ001", name: "XYZ Corp"));

            var repo = CreateQueryRepo();
            var results = await repo.AutocompleteAsync("ACM", CancellationToken.None);

            results.Should().HaveCount(2);
            results.Select(r => r.SalesOrganisationCode).Should().Contain(new[] { "ACM001", "ACM002" });
        }

        [Fact]
        public async Task AutocompleteAsync_Should_Return_Empty_WhenNoMatch()
        {
            await ClearTableAsync();
            await SeedEntityAsync(BuildEntity(code: "SO001", name: "Org One"));

            var repo = CreateQueryRepo();
            var results = await repo.AutocompleteAsync("ZZZNOMATCH", CancellationToken.None);

            results.Should().BeEmpty();
        }

        [Fact]
        public async Task AutocompleteAsync_Should_Not_Return_Inactive_Records()
        {
            await ClearTableAsync();
            await SeedEntityAsync(BuildEntity(code: "ACTV01", name: "Active Org", isActive: true));
            await SeedEntityAsync(BuildEntity(code: "INAC01", name: "Inactive Org", isActive: false));

            var repo = CreateQueryRepo();
            var results = await repo.AutocompleteAsync("Org", CancellationToken.None);

            results.Should().NotContain(r => r.SalesOrganisationCode == "INAC01");
            results.Should().Contain(r => r.SalesOrganisationCode == "ACTV01");
        }

        [Fact]
        public async Task AutocompleteAsync_Should_Not_Return_SoftDeleted_Records()
        {
            await ClearTableAsync();
            var id = await SeedEntityAsync(BuildEntity(code: "DLAUTO01", name: "Deleted Auto Org"));

            await using var ctx = _fixture.CreateFreshDbContext();
            await CreateCommandRepo(ctx).SoftDeleteAsync(id, CancellationToken.None);

            var repo = CreateQueryRepo();
            var results = await repo.AutocompleteAsync("Deleted", CancellationToken.None);

            results.Should().NotContain(r => r.SalesOrganisationCode == "DLAUTO01");
        }

        [Fact]
        public async Task AutocompleteAsync_Should_Match_ByName_As_Well()
        {
            await ClearTableAsync();
            await SeedEntityAsync(BuildEntity(code: "SO100", name: "Northern Sales Region"));

            var repo = CreateQueryRepo();
            var results = await repo.AutocompleteAsync("Northern", CancellationToken.None);

            results.Should().HaveCount(1);
            results[0].SalesOrganisationName.Should().Be("Northern Sales Region");
        }
    }
}
