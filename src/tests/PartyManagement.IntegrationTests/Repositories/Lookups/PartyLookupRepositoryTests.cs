using Contracts.Dtos.Lookups.Logistics;
using Contracts.Dtos.Lookups.Users;
using Contracts.Interfaces.Lookups.Logistics;
using Contracts.Interfaces.Lookups.Users;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using PartyManagement.Infrastructure.Repositories.Lookups;
using static PartyManagement.Domain.Common.BaseEntity;

namespace PartyManagement.IntegrationTests.Repositories.Lookups
{
    [Collection("DatabaseCollection")]
    public sealed class PartyLookupRepositoryTests
    {
        private readonly DbFixture _fixture;

        public PartyLookupRepositoryTests(DbFixture fixture)
        {
            _fixture = fixture;
        }

        private PartyLookupRepository CreateRepo(
            Mock<IFreightMasterLookup> freight = null,
            Mock<ICityLookup> city = null,
            Mock<IStateLookup> state = null,
            Mock<ICountryLookup> country = null)
        {
            freight ??= BuildFreightMock();
            city ??= BuildCityMock();
            state ??= BuildStateMock();
            country ??= BuildCountryMock();

            var conn = new SqlConnection(_fixture.ConnectionString);
            return new PartyLookupRepository(conn, freight.Object, city.Object, state.Object, country.Object);
        }

        private static Mock<IFreightMasterLookup> BuildFreightMock(params FreightMasterLookupDto[] rows)
        {
            var mock = new Mock<IFreightMasterLookup>(MockBehavior.Loose);
            mock.Setup(x => x.GetAllFreightMasterAsync()).ReturnsAsync(rows.ToList());
            return mock;
        }

        private static Mock<ICityLookup> BuildCityMock(params (int id, string name)[] rows)
        {
            var mock = new Mock<ICityLookup>(MockBehavior.Loose);
            var list = rows.Select(r => new CityLookupDto { CityId = r.id, CityName = r.name }).ToList();
            mock.Setup(x => x.GetByIdsAsync(It.IsAny<IEnumerable<int>>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(list);
            return mock;
        }

        private static Mock<IStateLookup> BuildStateMock(params (int id, string name)[] rows)
        {
            var mock = new Mock<IStateLookup>(MockBehavior.Loose);
            var list = rows.Select(r => new StateLookupDto { StateId = r.id, StateName = r.name }).ToList();
            mock.Setup(x => x.GetByIdsAsync(It.IsAny<IEnumerable<int>>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(list);
            return mock;
        }

        private static Mock<ICountryLookup> BuildCountryMock(params (int id, string name)[] rows)
        {
            var mock = new Mock<ICountryLookup>(MockBehavior.Loose);
            var list = rows.Select(r => new CountryLookupDto { CountryId = r.id, CountryName = r.name }).ToList();
            mock.Setup(x => x.GetByIdsAsync(It.IsAny<IEnumerable<int>>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(list);
            return mock;
        }

        private async Task<int> EnsureRegistrationTypeAsync()
        {
            await using var ctx = _fixture.CreateFreshDbContext();
            var existing = await ctx.MiscMaster.FirstOrDefaultAsync(m => m.Code == "Registered");
            if (existing != null) return existing.Id;

            var miscType = new PartyManagement.Domain.Entities.MiscTypeMaster
            {
                MiscTypeCode = "RegistrationType",
                Description = "Registration Type",
                IsActive = Status.Active,
                IsDeleted = IsDelete.NotDeleted
            };
            ctx.MiscTypeMaster.Add(miscType);
            await ctx.SaveChangesAsync();

            var misc = new PartyManagement.Domain.Entities.MiscMaster
            {
                MiscTypeId = miscType.Id,
                Code = "Registered",
                Description = "Registered",
                SortOrder = 1,
                IsActive = Status.Active,
                IsDeleted = IsDelete.NotDeleted
            };
            ctx.MiscMaster.Add(misc);
            await ctx.SaveChangesAsync();
            return misc.Id;
        }

        private async Task<int> SeedPartyAsync(
            string partyCode = "P001",
            string partyName = "Test Party",
            int? salesFreightId = null,
            int? purchaseFreightId = null)
        {
            var regId = await EnsureRegistrationTypeAsync();

            await using var ctx = _fixture.CreateFreshDbContext();
            var party = new PartyManagement.Domain.Entities.PartyMaster
            {
                PartyCode = partyCode,
                PartyName = partyName,
                SalesFreightId = salesFreightId,
                PurchaseFreightId = purchaseFreightId,
                RegistrationTypeId = regId,
                UnitId = 1,
                StatusId = regId,
                IsActive = Status.Active,
                IsDeleted = IsDelete.NotDeleted
            };
            ctx.PartyMaster.Add(party);
            await ctx.SaveChangesAsync();
            return party.Id;
        }

        private async Task SeedContactAsync(int partyId, string mobile, string email = "test@example.com")
        {
            await using var ctx = _fixture.CreateFreshDbContext();
            ctx.PartyContact.Add(new PartyManagement.Domain.Entities.PartyContact
            {
                PartyId = partyId,
                FirstName = "Test",
                LastName = "Contact",
                MobileNo = mobile,
                Phone = "044-0000000",
                EmailID = email
            });
            await ctx.SaveChangesAsync();
        }

        private async Task SeedAddressAsync(int partyId, int cityId, int stateId, int countryId,
            string addr1 = "Addr 1", string postal = "600001", string addressType = "Billing")
        {
            await using var ctx = _fixture.CreateFreshDbContext();
            ctx.PartyAddress.Add(new PartyManagement.Domain.Entities.PartyAddress
            {
                PartyId = partyId,
                AddressType = addressType,
                AddressLine1 = addr1,
                AddressLine2 = "Addr 2",
                CityId = cityId,
                StateId = stateId,
                CountryId = countryId,
                PostalCode = postal
            });
            await ctx.SaveChangesAsync();
        }

        // ── GetByIdAsync ────────────────────────────────────────────────────

        [Fact]
        public async Task GetByIdAsync_Returns_Null_When_Not_Found()
        {
            await _fixture.ClearAllTablesAsync();

            var result = await CreateRepo().GetByIdAsync(9999);

            result.Should().BeNull();
        }

        [Fact]
        public async Task GetByIdAsync_Returns_Core_Fields()
        {
            await _fixture.ClearAllTablesAsync();
            var id = await SeedPartyAsync(partyCode: "P100", partyName: "Acme");

            var result = await CreateRepo().GetByIdAsync(id);

            result.Should().NotBeNull();
            result!.Id.Should().Be(id);
            result.PartyCode.Should().Be("P100");
            result.PartyName.Should().Be("Acme");
        }

        [Fact]
        public async Task GetByIdAsync_Populates_Email_And_Mobile_From_Contact()
        {
            await _fixture.ClearAllTablesAsync();
            var id = await SeedPartyAsync();
            await SeedContactAsync(id, mobile: "9876543210", email: "hello@acme.com");

            var result = await CreateRepo().GetByIdAsync(id);

            result.Should().NotBeNull();
            result!.Mobile.Should().Be("9876543210");
            result.Email.Should().Be("hello@acme.com");
        }

        [Fact]
        public async Task GetByIdAsync_Skips_Contacts_With_Empty_Mobile()
        {
            await _fixture.ClearAllTablesAsync();
            var id = await SeedPartyAsync();
            await SeedContactAsync(id, mobile: "", email: "skip@me.com");
            await SeedContactAsync(id, mobile: "9111222333", email: "use@me.com");

            var result = await CreateRepo().GetByIdAsync(id);

            result!.Mobile.Should().Be("9111222333");
            result.Email.Should().Be("use@me.com");
        }

        [Fact]
        public async Task GetByIdAsync_Populates_Freight_Details()
        {
            await _fixture.ClearAllTablesAsync();
            var id = await SeedPartyAsync(salesFreightId: 11, purchaseFreightId: 22);

            var freight = BuildFreightMock(
                new FreightMasterLookupDto { Id = 11, FreightModeName = "Road", Rate = 10m },
                new FreightMasterLookupDto { Id = 22, FreightModeName = "Rail", Rate = 5m });

            var result = await CreateRepo(freight: freight).GetByIdAsync(id);

            result.Should().NotBeNull();
            result!.SalesFreight.Should().NotBeNull();
            result.SalesFreight!.Id.Should().Be(11);
            result.PurchaseFreight!.Id.Should().Be(22);
        }

        [Fact]
        public async Task GetByIdAsync_Populates_Addresses_With_City_State_Country_Names()
        {
            await _fixture.ClearAllTablesAsync();
            var id = await SeedPartyAsync();
            await SeedAddressAsync(id, cityId: 50, stateId: 60, countryId: 70);

            var city = BuildCityMock((50, "Chennai"));
            var state = BuildStateMock((60, "Tamil Nadu"));
            var country = BuildCountryMock((70, "India"));

            var result = await CreateRepo(city: city, state: state, country: country).GetByIdAsync(id);

            result.Should().NotBeNull();
            result!.Addresses.Should().ContainSingle();
            var addr = result.Addresses![0];
            addr.City.Should().Be("Chennai");
            addr.State.Should().Be("Tamil Nadu");
            addr.Country.Should().Be("India");
        }

        [Fact]
        public async Task GetByIdAsync_Returns_Empty_Addresses_When_None_Seeded()
        {
            await _fixture.ClearAllTablesAsync();
            var id = await SeedPartyAsync();

            var result = await CreateRepo().GetByIdAsync(id);

            result.Should().NotBeNull();
            result!.Addresses.Should().BeEmpty();
        }

        // ── GetByIdsAsync ───────────────────────────────────────────────────

        [Fact]
        public async Task GetByIdsAsync_Returns_Empty_For_Empty_Input()
        {
            var result = await CreateRepo().GetByIdsAsync(Array.Empty<int>());

            result.Should().BeEmpty();
        }

        [Fact]
        public async Task GetByIdsAsync_Returns_Empty_For_Null_Input()
        {
            var result = await CreateRepo().GetByIdsAsync(null!);

            result.Should().BeEmpty();
        }

        [Fact]
        public async Task GetByIdsAsync_Returns_Multiple_Parties()
        {
            await _fixture.ClearAllTablesAsync();
            var id1 = await SeedPartyAsync(partyCode: "P1", partyName: "Party One");
            var id2 = await SeedPartyAsync(partyCode: "P2", partyName: "Party Two");
            var id3 = await SeedPartyAsync(partyCode: "P3", partyName: "Party Three");

            var result = await CreateRepo().GetByIdsAsync(new[] { id1, id2 });

            result.Should().HaveCount(2);
            result.Select(r => r.PartyCode).Should().BeEquivalentTo(new[] { "P1", "P2" });
        }

        [Fact]
        public async Task GetByIdsAsync_Attaches_Addresses_To_Correct_Parties()
        {
            await _fixture.ClearAllTablesAsync();
            var id1 = await SeedPartyAsync(partyCode: "P1");
            var id2 = await SeedPartyAsync(partyCode: "P2");
            await SeedAddressAsync(id1, cityId: 10, stateId: 20, countryId: 30);
            await SeedAddressAsync(id2, cityId: 11, stateId: 21, countryId: 31);

            var city = BuildCityMock((10, "City A"), (11, "City B"));
            var state = BuildStateMock((20, "State A"), (21, "State B"));
            var country = BuildCountryMock((30, "Country A"), (31, "Country B"));

            var result = await CreateRepo(city: city, state: state, country: country)
                .GetByIdsAsync(new[] { id1, id2 });

            result.Should().HaveCount(2);
            result.First(r => r.Id == id1).Addresses!.Single().City.Should().Be("City A");
            result.First(r => r.Id == id2).Addresses!.Single().City.Should().Be("City B");
        }
    }
}
