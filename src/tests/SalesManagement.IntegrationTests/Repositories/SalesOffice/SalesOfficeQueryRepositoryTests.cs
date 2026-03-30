using Contracts.Dtos.Lookups.Users;
using Contracts.Interfaces.Lookups.Users;
using Dapper;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using SalesManagement.Infrastructure.Repositories.SalesOffice;
using SalesManagement.Infrastructure.Repositories.SalesOrganisation;
using SalesManagement.IntegrationTests.Common;
using static SalesManagement.Domain.Common.BaseEntity;

namespace SalesManagement.IntegrationTests.Repositories.SalesOffice
{
    /// <summary>
    /// Integration tests for SalesOfficeQueryRepository.
    /// Verifies Dapper SQL queries (GetAll, GetById, AlreadyExists, NotFound, Autocomplete)
    /// against a real SQL Server database.
    /// ICityLookup is mocked to isolate from cross-module dependency.
    /// </summary>
    [Collection("DatabaseCollection")]
    public sealed class SalesOfficeQueryRepositoryTests
    {
        private readonly DbFixture _fixture;

        public SalesOfficeQueryRepositoryTests(DbFixture fixture)
        {
            _fixture = fixture;
        }

        // ── Helpers ───────────────────────────────────────────────────────────

        private SalesOfficeQueryRepository CreateQueryRepo(Mock<ICityLookup> cityLookup = null)
        {
            cityLookup ??= BuildDefaultCityLookup();
            var conn = new SqlConnection(_fixture.ConnectionString);
            return new SalesOfficeQueryRepository(conn, cityLookup.Object);
        }

        private Mock<ICityLookup> BuildDefaultCityLookup(int cityId = 1, string cityName = "Test City")
        {
            var mock = new Mock<ICityLookup>(MockBehavior.Loose);
            mock.Setup(c => c.GetAllCityAsync(It.IsAny<CancellationToken>()))
                .ReturnsAsync(new List<CityLookupDto>
                {
                    new CityLookupDto { CityId = cityId, CityName = cityName }
                });
            mock.Setup(c => c.GetByIdAsync(cityId, It.IsAny<CancellationToken>()))
                .ReturnsAsync(new CityLookupDto { CityId = cityId, CityName = cityName });
            return mock;
        }

        private async Task ClearTablesAsync()
        {
            await using var cnn = new SqlConnection(_fixture.ConnectionString);
            await cnn.OpenAsync();
            await cnn.ExecuteAsync("DELETE FROM Sales.SalesOrderHeader");
            await cnn.ExecuteAsync("DELETE FROM Sales.OfficerSalesGroup");
            await cnn.ExecuteAsync("DELETE FROM Sales.SalesGroup");
            await cnn.ExecuteAsync("DELETE FROM Sales.MarketingOfficer");
            await cnn.ExecuteAsync("DELETE FROM Sales.SalesOffice");
            await cnn.ExecuteAsync("DELETE FROM Sales.ItemPriceMaster");
            await cnn.ExecuteAsync("DELETE FROM Sales.SalesSegment");
            await cnn.ExecuteAsync("DELETE FROM Sales.SalesOrganisation");
        }

        private async Task<int> SeedSalesOrganisationAsync(string code = null)
        {
            code ??= "ORG" + Guid.NewGuid().ToString("N")[..6].ToUpper();
            await using var ctx = _fixture.CreateFreshDbContext();
            var repo = new SalesOrganisationCommandRepository(ctx);
            return await repo.CreateAsync(new Domain.Entities.SalesOrganisation
            {
                SalesOrganisationCode = code,
                SalesOrganisationName = "Test Org",
                CompanyId = 1,
                IsActive = Status.Active,
                IsDeleted = IsDelete.NotDeleted
            });
        }

        private async Task<int> SeedSalesOfficeAsync(
            int salesOrganisationId,
            string name = "Test Sales Office",
            int cityId = 1,
            bool isActive = true)
        {
            await using var ctx = _fixture.CreateFreshDbContext();
            var repo = new SalesOfficeCommandRepository(ctx);
            return await repo.CreateAsync(new Domain.Entities.SalesOffice
            {
                SalesOfficeName = name,
                SalesOrganisationId = salesOrganisationId,
                CityId = cityId,
                Pincode = "110001",
                Phone = "9876543210",
                Email = "office@test.com",
                ResponsibleManager = "Manager A",
                RegionTerritory = "North",
                Address = "123 Test Street",
                IsActive = isActive ? Status.Active : Status.Inactive,
                IsDeleted = IsDelete.NotDeleted
            });
        }

        // ── GetAllAsync ───────────────────────────────────────────────────────

        [Fact]
        public async Task GetAllAsync_Should_Return_PagedResults()
        {
            await ClearTablesAsync();
            var orgId = await SeedSalesOrganisationAsync();
            await SeedSalesOfficeAsync(orgId, name: "Office Alpha");
            await SeedSalesOfficeAsync(orgId, name: "Office Beta");
            await SeedSalesOfficeAsync(orgId, name: "Office Gamma");

            var repo = CreateQueryRepo();
            var (data, totalCount) = await repo.GetAllAsync(pageNumber: 1, pageSize: 2, searchTerm: null);

            totalCount.Should().Be(3);
            data.Should().HaveCount(2);
        }

        [Fact]
        public async Task GetAllAsync_Should_Filter_BySearchTerm()
        {
            await ClearTablesAsync();
            var orgId = await SeedSalesOrganisationAsync();
            await SeedSalesOfficeAsync(orgId, name: "Alpha Office");
            await SeedSalesOfficeAsync(orgId, name: "Beta Office");
            await SeedSalesOfficeAsync(orgId, name: "Match Office");

            var repo = CreateQueryRepo();
            var (data, totalCount) = await repo.GetAllAsync(pageNumber: 1, pageSize: 10, searchTerm: "Match");

            totalCount.Should().Be(1);
            data.Should().HaveCount(1);
            data[0].SalesOfficeName.Should().Be("Match Office");
        }

        [Fact]
        public async Task GetAllAsync_Should_Not_Return_SoftDeleted_Records()
        {
            await ClearTablesAsync();
            var orgId = await SeedSalesOrganisationAsync();
            var id = await SeedSalesOfficeAsync(orgId, name: "Deleted Office");

            await using var ctx = _fixture.CreateFreshDbContext();
            await new SalesOfficeCommandRepository(ctx).SoftDeleteAsync(id, CancellationToken.None);

            var repo = CreateQueryRepo();
            var (data, _) = await repo.GetAllAsync(1, 10, null);

            data.Should().NotContain(x => x.SalesOfficeName == "Deleted Office");
        }

        [Fact]
        public async Task GetAllAsync_Should_PopulateCityName_Via_Lookup()
        {
            await ClearTablesAsync();
            var orgId = await SeedSalesOrganisationAsync();
            await SeedSalesOfficeAsync(orgId, name: "Lookup Office", cityId: 1);

            var cityLookup = BuildDefaultCityLookup(cityId: 1, cityName: "Mumbai");
            var repo = CreateQueryRepo(cityLookup);

            var (data, _) = await repo.GetAllAsync(1, 10, null);

            data.Should().NotBeEmpty();
            data[0].CityName.Should().Be("Mumbai");
        }

        [Fact]
        public async Task GetAllAsync_Should_Return_Empty_WhenNoData()
        {
            await ClearTablesAsync();

            var repo = CreateQueryRepo();
            var (data, totalCount) = await repo.GetAllAsync(1, 10, null);

            data.Should().BeEmpty();
            totalCount.Should().Be(0);
        }

        [Fact]
        public async Task GetAllAsync_Should_Support_Pagination_Page2()
        {
            await ClearTablesAsync();
            var orgId = await SeedSalesOrganisationAsync();
            for (int i = 1; i <= 5; i++)
                await SeedSalesOfficeAsync(orgId, name: $"Office {i}");

            var repo = CreateQueryRepo();
            var (page1, total) = await repo.GetAllAsync(1, 3, null);
            var (page2, _) = await repo.GetAllAsync(2, 3, null);

            total.Should().Be(5);
            page1.Should().HaveCount(3);
            page2.Should().HaveCount(2);

            var page1Ids = page1.Select(x => x.Id).ToList();
            var page2Ids = page2.Select(x => x.Id).ToList();
            page1Ids.Should().NotIntersectWith(page2Ids);
        }

        // ── GetByIdAsync ──────────────────────────────────────────────────────

        [Fact]
        public async Task GetByIdAsync_Should_Return_Correct_DTO()
        {
            await ClearTablesAsync();
            var orgId = await SeedSalesOrganisationAsync();
            var id = await SeedSalesOfficeAsync(orgId, name: "ById Office", cityId: 1);

            var repo = CreateQueryRepo();
            var dto = await repo.GetByIdAsync(id);

            dto.Should().NotBeNull();
            dto!.Id.Should().Be(id);
            dto.SalesOfficeName.Should().Be("ById Office");
            dto.SalesOrganisationId.Should().Be(orgId);
        }

        [Fact]
        public async Task GetByIdAsync_Should_PopulateCityName_Via_Lookup()
        {
            await ClearTablesAsync();
            var orgId = await SeedSalesOrganisationAsync();
            var id = await SeedSalesOfficeAsync(orgId, name: "Lookup Office", cityId: 1);

            var cityLookup = BuildDefaultCityLookup(cityId: 1, cityName: "Delhi");
            var repo = CreateQueryRepo(cityLookup);

            var dto = await repo.GetByIdAsync(id);

            dto!.CityName.Should().Be("Delhi");
        }

        [Fact]
        public async Task GetByIdAsync_Should_Return_Null_WhenNotFound()
        {
            await ClearTablesAsync();

            var repo = CreateQueryRepo();
            var dto = await repo.GetByIdAsync(99999);

            dto.Should().BeNull();
        }

        [Fact]
        public async Task GetByIdAsync_Should_Return_Null_WhenSoftDeleted()
        {
            await ClearTablesAsync();
            var orgId = await SeedSalesOrganisationAsync();
            var id = await SeedSalesOfficeAsync(orgId);

            await using var ctx = _fixture.CreateFreshDbContext();
            await new SalesOfficeCommandRepository(ctx).SoftDeleteAsync(id, CancellationToken.None);

            var repo = CreateQueryRepo();
            var dto = await repo.GetByIdAsync(id);

            dto.Should().BeNull();
        }

        [Fact]
        public async Task GetByIdAsync_Should_Return_Audit_Fields()
        {
            await ClearTablesAsync();
            var orgId = await SeedSalesOrganisationAsync();
            var id = await SeedSalesOfficeAsync(orgId);

            var repo = CreateQueryRepo();
            var dto = await repo.GetByIdAsync(id);

            dto!.CreatedBy.Should().Be(1);
            dto.CreatedByName.Should().Be("test-user");
            dto.CreatedIP.Should().Be("127.0.0.1");
            dto.CreatedDate.Should().NotBeNull();
        }

        // ── AlreadyExistsAsync ────────────────────────────────────────────────

        [Fact]
        public async Task AlreadyExistsAsync_Should_Return_True_WhenCompositeKeyExists()
        {
            await ClearTablesAsync();
            var orgId = await SeedSalesOrganisationAsync();
            await SeedSalesOfficeAsync(orgId, name: "Existing Office");

            var repo = CreateQueryRepo();
            var result = await repo.AlreadyExistsAsync("Existing Office", orgId);

            result.Should().BeTrue();
        }

        [Fact]
        public async Task AlreadyExistsAsync_Should_Return_False_WhenDoesNotExist()
        {
            await ClearTablesAsync();

            var repo = CreateQueryRepo();
            var result = await repo.AlreadyExistsAsync("NonExistent Office", 99999);

            result.Should().BeFalse();
        }

        [Fact]
        public async Task AlreadyExistsAsync_Should_Return_False_WhenExcludedId_Matches()
        {
            await ClearTablesAsync();
            var orgId = await SeedSalesOrganisationAsync();
            var id = await SeedSalesOfficeAsync(orgId, name: "Self Office");

            var repo = CreateQueryRepo();
            var result = await repo.AlreadyExistsAsync("Self Office", orgId, id: id);

            result.Should().BeFalse();
        }

        [Fact]
        public async Task AlreadyExistsAsync_Should_Return_True_WhenDifferentRecord_HasSameKey()
        {
            await ClearTablesAsync();
            var orgId = await SeedSalesOrganisationAsync();
            await SeedSalesOfficeAsync(orgId, name: "Duplicate Office");
            var idB = await SeedSalesOfficeAsync(orgId, name: "Other Office");

            var repo = CreateQueryRepo();
            var result = await repo.AlreadyExistsAsync("Duplicate Office", orgId, id: idB);

            result.Should().BeTrue();
        }

        [Fact]
        public async Task AlreadyExistsAsync_Should_Return_False_ForDeleted_Records()
        {
            await ClearTablesAsync();
            var orgId = await SeedSalesOrganisationAsync();
            var id = await SeedSalesOfficeAsync(orgId, name: "Deleted Office");

            await using var ctx = _fixture.CreateFreshDbContext();
            await new SalesOfficeCommandRepository(ctx).SoftDeleteAsync(id, CancellationToken.None);

            var repo = CreateQueryRepo();
            var result = await repo.AlreadyExistsAsync("Deleted Office", orgId);

            result.Should().BeFalse();
        }

        // ── NotFoundAsync ─────────────────────────────────────────────────────

        [Fact]
        public async Task NotFoundAsync_Should_Return_False_WhenEntityExists()
        {
            await ClearTablesAsync();
            var orgId = await SeedSalesOrganisationAsync();
            var id = await SeedSalesOfficeAsync(orgId);

            var repo = CreateQueryRepo();
            var result = await repo.NotFoundAsync(id);

            result.Should().BeFalse();
        }

        [Fact]
        public async Task NotFoundAsync_Should_Return_True_WhenEntityDoesNotExist()
        {
            await ClearTablesAsync();

            var repo = CreateQueryRepo();
            var result = await repo.NotFoundAsync(99999);

            result.Should().BeTrue();
        }

        [Fact]
        public async Task NotFoundAsync_Should_Return_True_AfterSoftDelete()
        {
            await ClearTablesAsync();
            var orgId = await SeedSalesOrganisationAsync();
            var id = await SeedSalesOfficeAsync(orgId);

            await using var ctx = _fixture.CreateFreshDbContext();
            await new SalesOfficeCommandRepository(ctx).SoftDeleteAsync(id, CancellationToken.None);

            var repo = CreateQueryRepo();
            var result = await repo.NotFoundAsync(id);

            result.Should().BeTrue();
        }

        // ── AutocompleteAsync ─────────────────────────────────────────────────

        [Fact]
        public async Task AutocompleteAsync_Should_Return_Matching_Results()
        {
            await ClearTablesAsync();
            var orgId = await SeedSalesOrganisationAsync();
            await SeedSalesOfficeAsync(orgId, name: "Acme Office");
            await SeedSalesOfficeAsync(orgId, name: "Acme North");
            await SeedSalesOfficeAsync(orgId, name: "XYZ Office");

            var repo = CreateQueryRepo();
            var results = await repo.AutocompleteAsync("Acme", CancellationToken.None);

            results.Should().HaveCount(2);
        }

        [Fact]
        public async Task AutocompleteAsync_Should_Return_Empty_WhenNoMatch()
        {
            await ClearTablesAsync();
            var orgId = await SeedSalesOrganisationAsync();
            await SeedSalesOfficeAsync(orgId, name: "Office One");

            var repo = CreateQueryRepo();
            var results = await repo.AutocompleteAsync("ZZZNOMATCH", CancellationToken.None);

            results.Should().BeEmpty();
        }

        [Fact]
        public async Task AutocompleteAsync_Should_Not_Return_Inactive_Records()
        {
            await ClearTablesAsync();
            var orgId = await SeedSalesOrganisationAsync();
            await SeedSalesOfficeAsync(orgId, name: "Active Office", isActive: true);
            await SeedSalesOfficeAsync(orgId, name: "Inactive Office", isActive: false);

            var repo = CreateQueryRepo();
            var results = await repo.AutocompleteAsync("Office", CancellationToken.None);

            results.Should().NotContain(r => r.SalesOfficeName == "Inactive Office");
            results.Should().Contain(r => r.SalesOfficeName == "Active Office");
        }

        [Fact]
        public async Task AutocompleteAsync_Should_Not_Return_SoftDeleted_Records()
        {
            await ClearTablesAsync();
            var orgId = await SeedSalesOrganisationAsync();
            var id = await SeedSalesOfficeAsync(orgId, name: "Deleted Auto Office");

            await using var ctx = _fixture.CreateFreshDbContext();
            await new SalesOfficeCommandRepository(ctx).SoftDeleteAsync(id, CancellationToken.None);

            var repo = CreateQueryRepo();
            var results = await repo.AutocompleteAsync("Deleted", CancellationToken.None);

            results.Should().NotContain(r => r.SalesOfficeName == "Deleted Auto Office");
        }

        // ── SalesOrganisationExistsAsync ────────────────────────────────────

        [Fact]
        public async Task SalesOrganisationExistsAsync_Should_Return_True_WhenExists()
        {
            await ClearTablesAsync();
            var orgId = await SeedSalesOrganisationAsync();

            var repo = CreateQueryRepo();
            var result = await repo.SalesOrganisationExistsAsync(orgId);

            result.Should().BeTrue();
        }

        [Fact]
        public async Task SalesOrganisationExistsAsync_Should_Return_False_WhenNotExists()
        {
            await ClearTablesAsync();

            var repo = CreateQueryRepo();
            var result = await repo.SalesOrganisationExistsAsync(99999);

            result.Should().BeFalse();
        }
    }
}
