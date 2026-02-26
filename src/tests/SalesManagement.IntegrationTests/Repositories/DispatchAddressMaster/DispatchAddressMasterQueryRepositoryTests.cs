using Contracts.Dtos.Lookups.Users;
using Contracts.Interfaces.Lookups.Users;
using Dapper;
using Microsoft.Data.SqlClient;
using SalesManagement.Infrastructure.Repositories.DispatchAddressMaster;
using SalesManagement.IntegrationTests.Common;
using static SalesManagement.Domain.Common.BaseEntity;

namespace SalesManagement.IntegrationTests.Repositories.DispatchAddressMaster
{
    /// <summary>
    /// Integration tests for DispatchAddressMasterQueryRepository.
    /// Verifies Dapper SQL queries against a real SQL Server database.
    /// City, State, and Country are cross-module FKs — mocked via ICityLookup / IStateLookup / ICountryLookup.
    /// </summary>
    [Collection("DatabaseCollection")]
    public sealed class DispatchAddressMasterQueryRepositoryTests
    {
        private readonly DbFixture _fixture;

        public DispatchAddressMasterQueryRepositoryTests(DbFixture fixture)
        {
            _fixture = fixture;
        }

        // ── Helpers ───────────────────────────────────────────────────────────

        private SqlConnection OpenConnection() => new(_fixture.ConnectionString);

        private Mock<ICityLookup> BuildCityLookup(int cityId = 1, string cityName = "Delhi")
        {
            var mock = new Mock<ICityLookup>(MockBehavior.Loose);
            mock.Setup(c => c.GetAllCityAsync(It.IsAny<CancellationToken>()))
                .ReturnsAsync(new List<CityLookupDto>
                {
                    new() { CityId = cityId, CityName = cityName }
                });
            mock.Setup(c => c.GetByIdAsync(cityId, It.IsAny<CancellationToken>()))
                .ReturnsAsync(new CityLookupDto { CityId = cityId, CityName = cityName });
            mock.Setup(c => c.GetByIdAsync(It.Is<int>(x => x != cityId), It.IsAny<CancellationToken>()))
                .ReturnsAsync((CityLookupDto?)null);
            return mock;
        }

        private Mock<IStateLookup> BuildStateLookup(int stateId = 1, string stateName = "Delhi")
        {
            var mock = new Mock<IStateLookup>(MockBehavior.Loose);
            mock.Setup(s => s.GetAllStatesAsync(It.IsAny<CancellationToken>()))
                .ReturnsAsync(new List<StateLookupDto>
                {
                    new() { StateId = stateId, StateName = stateName }
                });
            mock.Setup(s => s.GetByIdAsync(stateId, It.IsAny<CancellationToken>()))
                .ReturnsAsync(new StateLookupDto { StateId = stateId, StateName = stateName });
            mock.Setup(s => s.GetByIdAsync(It.Is<int>(x => x != stateId), It.IsAny<CancellationToken>()))
                .ReturnsAsync((StateLookupDto?)null);
            return mock;
        }

        private Mock<ICountryLookup> BuildCountryLookup(int countryId = 1, string countryName = "India")
        {
            var mock = new Mock<ICountryLookup>(MockBehavior.Loose);
            mock.Setup(c => c.GetAllCountriesAsync(It.IsAny<CancellationToken>()))
                .ReturnsAsync(new List<CountryLookupDto>
                {
                    new() { CountryId = countryId, CountryName = countryName }
                });
            mock.Setup(c => c.GetByIdAsync(countryId, It.IsAny<CancellationToken>()))
                .ReturnsAsync(new CountryLookupDto { CountryId = countryId, CountryName = countryName });
            mock.Setup(c => c.GetByIdAsync(It.Is<int>(x => x != countryId), It.IsAny<CancellationToken>()))
                .ReturnsAsync((CountryLookupDto?)null);
            return mock;
        }

        private DispatchAddressMasterQueryRepository CreateQueryRepo(
            Mock<ICityLookup>? cityMock = null,
            Mock<IStateLookup>? stateMock = null,
            Mock<ICountryLookup>? countryMock = null)
        {
            var conn = new SqlConnection(_fixture.ConnectionString);
            return new DispatchAddressMasterQueryRepository(
                conn,
                (cityMock ?? BuildCityLookup()).Object,
                (stateMock ?? BuildStateLookup()).Object,
                (countryMock ?? BuildCountryLookup()).Object);
        }

        private DispatchAddressMasterCommandRepository CreateCommandRepo(Infrastructure.Data.ApplicationDbContext ctx)
            => new(ctx);

        private async Task ClearTableAsync()
        {
            await using var cnn = OpenConnection();
            await cnn.OpenAsync();
            await cnn.ExecuteAsync("DELETE FROM Sales.DispatchAddressMaster");
        }

        private async Task<int> SeedEntityAsync(
            string name = "Test Dispatch Address",
            string addressLine1 = "123 Main Street",
            int cityId = 1,
            int stateId = 1,
            int countryId = 1,
            string pinCode = "110001",
            bool isActive = true)
        {
            await using var ctx = _fixture.CreateFreshDbContext();
            return await CreateCommandRepo(ctx).CreateAsync(new Domain.Entities.DispatchAddressMaster
            {
                DispatchAddressName = name,
                AddressLine1 = addressLine1,
                CityId = cityId,
                StateId = stateId,
                CountryId = countryId,
                PinCode = pinCode,
                IsActive = isActive ? Status.Active : Status.Inactive,
                IsDeleted = IsDelete.NotDeleted
            });
        }

        // ── GetAllAsync ───────────────────────────────────────────────────────

        [Fact]
        public async Task GetAllAsync_Should_Return_Seeded_Records()
        {
            await ClearTableAsync();
            await SeedEntityAsync("Address One", "1 Street A");
            await SeedEntityAsync("Address Two", "2 Street B");

            var (data, totalCount) = await CreateQueryRepo().GetAllAsync(1, 10, null);

            totalCount.Should().Be(2);
            data.Should().HaveCount(2);
        }

        [Fact]
        public async Task GetAllAsync_Should_Populate_CityName_StateName_CountryName()
        {
            await ClearTableAsync();
            await SeedEntityAsync("Lookup Address", "1 Lookup Street", cityId: 1, stateId: 1, countryId: 1);

            var cityMock = BuildCityLookup(1, "New Delhi");
            var stateMock = BuildStateLookup(1, "Delhi State");
            var countryMock = BuildCountryLookup(1, "India");

            var (data, _) = await CreateQueryRepo(cityMock, stateMock, countryMock).GetAllAsync(1, 10, null);

            data.Should().NotBeEmpty();
            data[0].CityName.Should().Be("New Delhi");
            data[0].StateName.Should().Be("Delhi State");
            data[0].CountryName.Should().Be("India");
        }

        [Fact]
        public async Task GetAllAsync_Should_Not_Return_SoftDeleted_Records()
        {
            await ClearTableAsync();
            var id = await SeedEntityAsync("Deleted Address", "1 Delete Street");

            await using var ctx = _fixture.CreateFreshDbContext();
            await CreateCommandRepo(ctx).SoftDeleteAsync(id, CancellationToken.None);

            var (data, totalCount) = await CreateQueryRepo().GetAllAsync(1, 10, null);

            data.Should().NotContain(x => x.DispatchAddressName == "Deleted Address");
            totalCount.Should().Be(0);
        }

        [Fact]
        public async Task GetAllAsync_Should_Filter_By_SearchTerm_OnName()
        {
            await ClearTableAsync();
            await SeedEntityAsync("Alpha Warehouse", "1 Alpha Road");
            await SeedEntityAsync("Beta Warehouse", "2 Beta Road");

            var (data, totalCount) = await CreateQueryRepo().GetAllAsync(1, 10, "Alpha");

            totalCount.Should().Be(1);
            data[0].DispatchAddressName.Should().Be("Alpha Warehouse");
        }

        [Fact]
        public async Task GetAllAsync_Should_Filter_By_SearchTerm_OnAddressLine1()
        {
            await ClearTableAsync();
            await SeedEntityAsync("General Address", "Unique Lane 999");
            await SeedEntityAsync("Other Address", "Common Lane 1");

            var (data, totalCount) = await CreateQueryRepo().GetAllAsync(1, 10, "Unique Lane");

            totalCount.Should().Be(1);
            data[0].AddressLine1.Should().Be("Unique Lane 999");
        }

        [Fact]
        public async Task GetAllAsync_Should_Support_Pagination()
        {
            await ClearTableAsync();
            for (int i = 1; i <= 5; i++)
                await SeedEntityAsync($"Address {i:D2}", $"{i} Paginated Street", pinCode: $"11000{i}");

            var (page1, total) = await CreateQueryRepo().GetAllAsync(1, 3, null);
            var (page2, _) = await CreateQueryRepo().GetAllAsync(2, 3, null);

            total.Should().Be(5);
            page1.Should().HaveCount(3);
            page2.Should().HaveCount(2);

            var page1Ids = page1.Select(x => x.Id).ToList();
            var page2Ids = page2.Select(x => x.Id).ToList();
            page1Ids.Should().NotIntersectWith(page2Ids);
        }

        // ── GetByIdAsync ──────────────────────────────────────────────────────

        [Fact]
        public async Task GetByIdAsync_Should_Return_Correct_Dto()
        {
            await ClearTableAsync();
            var id = await SeedEntityAsync("Specific Address", "99 Test Lane", cityId: 1, stateId: 1, countryId: 1, pinCode: "560001");

            var dto = await CreateQueryRepo().GetByIdAsync(id);

            dto.Should().NotBeNull();
            dto!.Id.Should().Be(id);
            dto.DispatchAddressName.Should().Be("Specific Address");
            dto.AddressLine1.Should().Be("99 Test Lane");
            dto.PinCode.Should().Be("560001");
            dto.CityId.Should().Be(1);
            dto.StateId.Should().Be(1);
            dto.CountryId.Should().Be(1);
        }

        [Fact]
        public async Task GetByIdAsync_Should_Populate_LookupNames()
        {
            await ClearTableAsync();
            var id = await SeedEntityAsync(cityId: 1, stateId: 1, countryId: 1);

            var cityMock = BuildCityLookup(1, "Mumbai");
            var stateMock = BuildStateLookup(1, "Maharashtra");
            var countryMock = BuildCountryLookup(1, "India");

            var dto = await CreateQueryRepo(cityMock, stateMock, countryMock).GetByIdAsync(id);

            dto!.CityName.Should().Be("Mumbai");
            dto.StateName.Should().Be("Maharashtra");
            dto.CountryName.Should().Be("India");
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
            var id = await SeedEntityAsync("To Be Deleted", "Delete Street");

            await using var ctx = _fixture.CreateFreshDbContext();
            await CreateCommandRepo(ctx).SoftDeleteAsync(id, CancellationToken.None);

            var dto = await CreateQueryRepo().GetByIdAsync(id);

            dto.Should().BeNull();
        }

        // ── CompositeKeyExistsAsync ───────────────────────────────────────────

        [Fact]
        public async Task CompositeKeyExistsAsync_Should_Return_True_WhenDuplicateExists()
        {
            await ClearTableAsync();
            await SeedEntityAsync("Duplicate Name", "1 Street", cityId: 5, pinCode: "500001");

            var exists = await CreateQueryRepo().CompositeKeyExistsAsync("Duplicate Name", 5, "500001");

            exists.Should().BeTrue();
        }

        [Fact]
        public async Task CompositeKeyExistsAsync_Should_Return_False_WhenNoMatch()
        {
            await ClearTableAsync();

            var exists = await CreateQueryRepo().CompositeKeyExistsAsync("NonExistent", 99, "000000");

            exists.Should().BeFalse();
        }

        [Fact]
        public async Task CompositeKeyExistsAsync_Should_Return_False_ForSoftDeleted()
        {
            await ClearTableAsync();
            var id = await SeedEntityAsync("Soft Deleted", "1 Street", cityId: 7, pinCode: "700001");

            await using var ctx = _fixture.CreateFreshDbContext();
            await CreateCommandRepo(ctx).SoftDeleteAsync(id, CancellationToken.None);

            var exists = await CreateQueryRepo().CompositeKeyExistsAsync("Soft Deleted", 7, "700001");

            exists.Should().BeFalse();
        }

        [Fact]
        public async Task CompositeKeyExistsAsync_Should_Return_False_WhenExcludingOwnId()
        {
            await ClearTableAsync();
            var id = await SeedEntityAsync("Own Address", "1 Own Street", cityId: 3, pinCode: "300001");

            // Update scenario: exclude own ID — should NOT be flagged as duplicate
            var exists = await CreateQueryRepo().CompositeKeyExistsAsync("Own Address", 3, "300001", excludeId: id);

            exists.Should().BeFalse();
        }

        [Fact]
        public async Task CompositeKeyExistsAsync_Should_Return_True_WhenDifferentIdHasSameKey()
        {
            await ClearTableAsync();
            await SeedEntityAsync("Same Name", "1 Same Street", cityId: 4, pinCode: "400001");
            var id2 = await SeedEntityAsync("Different Name", "2 Other Street", cityId: 4, pinCode: "400002");

            // id2 tries to update but a DIFFERENT record has the same key
            var exists = await CreateQueryRepo().CompositeKeyExistsAsync("Same Name", 4, "400001", excludeId: id2);

            exists.Should().BeTrue();
        }

        // ── NotFoundAsync ─────────────────────────────────────────────────────

        [Fact]
        public async Task NotFoundAsync_Should_Return_False_WhenEntityExists()
        {
            await ClearTableAsync();
            var id = await SeedEntityAsync();

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
            var id = await SeedEntityAsync();

            await using var ctx = _fixture.CreateFreshDbContext();
            await CreateCommandRepo(ctx).SoftDeleteAsync(id, CancellationToken.None);

            var result = await CreateQueryRepo().NotFoundAsync(id);

            result.Should().BeTrue();
        }

        // ── CityExistsAsync ───────────────────────────────────────────────────

        [Fact]
        public async Task CityExistsAsync_Should_Return_True_WhenCityInLookup()
        {
            var cityMock = BuildCityLookup(10, "Hyderabad");

            var result = await CreateQueryRepo(cityMock: cityMock).CityExistsAsync(10);

            result.Should().BeTrue();
        }

        [Fact]
        public async Task CityExistsAsync_Should_Return_False_WhenCityNotInLookup()
        {
            var cityMock = BuildCityLookup(10, "Hyderabad");

            var result = await CreateQueryRepo(cityMock: cityMock).CityExistsAsync(999);

            result.Should().BeFalse();
        }

        // ── StateExistsAsync ──────────────────────────────────────────────────

        [Fact]
        public async Task StateExistsAsync_Should_Return_True_WhenStateInLookup()
        {
            var stateMock = BuildStateLookup(20, "Maharashtra");

            var result = await CreateQueryRepo(stateMock: stateMock).StateExistsAsync(20);

            result.Should().BeTrue();
        }

        [Fact]
        public async Task StateExistsAsync_Should_Return_False_WhenStateNotInLookup()
        {
            var stateMock = BuildStateLookup(20, "Maharashtra");

            var result = await CreateQueryRepo(stateMock: stateMock).StateExistsAsync(999);

            result.Should().BeFalse();
        }

        // ── CountryExistsAsync ────────────────────────────────────────────────

        [Fact]
        public async Task CountryExistsAsync_Should_Return_True_WhenCountryInLookup()
        {
            var countryMock = BuildCountryLookup(30, "USA");

            var result = await CreateQueryRepo(countryMock: countryMock).CountryExistsAsync(30);

            result.Should().BeTrue();
        }

        [Fact]
        public async Task CountryExistsAsync_Should_Return_False_WhenCountryNotInLookup()
        {
            var countryMock = BuildCountryLookup(30, "USA");

            var result = await CreateQueryRepo(countryMock: countryMock).CountryExistsAsync(999);

            result.Should().BeFalse();
        }

        // ── SoftDeleteValidationAsync ─────────────────────────────────────────

        [Fact]
        public async Task SoftDeleteValidationAsync_Should_Always_Return_False()
        {
            await ClearTableAsync();
            var id = await SeedEntityAsync();

            var result = await CreateQueryRepo().SoftDeleteValidationAsync(id);

            result.Should().BeFalse();
        }

        // ── AutocompleteAsync ─────────────────────────────────────────────────

        [Fact]
        public async Task AutocompleteAsync_Should_Return_Matching_Results()
        {
            await ClearTableAsync();
            await SeedEntityAsync("Acme Warehouse", "1 Acme Street");
            await SeedEntityAsync("Acme Office", "2 Acme Lane");
            await SeedEntityAsync("Zeta Storage", "3 Zeta Road");

            var results = await CreateQueryRepo().AutocompleteAsync("Acme", CancellationToken.None);

            results.Should().HaveCount(2);
            results.Select(r => r.DispatchAddressName).Should().Contain(new[] { "Acme Warehouse", "Acme Office" });
        }

        [Fact]
        public async Task AutocompleteAsync_Should_Return_Empty_WhenNoMatch()
        {
            await ClearTableAsync();
            await SeedEntityAsync("Test Address", "1 Test Street");

            var results = await CreateQueryRepo().AutocompleteAsync("ZZZNOMATCH", CancellationToken.None);

            results.Should().BeEmpty();
        }

        [Fact]
        public async Task AutocompleteAsync_Should_Not_Return_Inactive_Records()
        {
            await ClearTableAsync();
            await SeedEntityAsync("Active Dispatch", "1 Active Street", isActive: true);
            await SeedEntityAsync("Inactive Dispatch", "2 Inactive Street", isActive: false);

            var results = await CreateQueryRepo().AutocompleteAsync("Dispatch", CancellationToken.None);

            results.Should().NotContain(r => r.DispatchAddressName == "Inactive Dispatch");
            results.Should().Contain(r => r.DispatchAddressName == "Active Dispatch");
        }

        [Fact]
        public async Task AutocompleteAsync_Should_Not_Return_SoftDeleted_Records()
        {
            await ClearTableAsync();
            var id = await SeedEntityAsync("Deleted Dispatch", "1 Deleted Street");

            await using var ctx = _fixture.CreateFreshDbContext();
            await CreateCommandRepo(ctx).SoftDeleteAsync(id, CancellationToken.None);

            var results = await CreateQueryRepo().AutocompleteAsync("Deleted", CancellationToken.None);

            results.Should().NotContain(r => r.DispatchAddressName == "Deleted Dispatch");
        }

        [Fact]
        public async Task AutocompleteAsync_Should_Populate_CityName()
        {
            await ClearTableAsync();
            await SeedEntityAsync("CityName Test", "1 Lookup Lane", cityId: 1);

            var cityMock = BuildCityLookup(1, "Chennai");
            var results = await CreateQueryRepo(cityMock: cityMock).AutocompleteAsync("CityName", CancellationToken.None);

            results.Should().HaveCount(1);
            results[0].CityName.Should().Be("Chennai");
        }
    }
}
