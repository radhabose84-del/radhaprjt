using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using PartyManagement.Infrastructure.Repositories.Lookups;
using static PartyManagement.Domain.Common.BaseEntity;

namespace PartyManagement.IntegrationTests.Repositories.Lookups
{
    [Collection("DatabaseCollection")]
    public sealed class PartyDetailLookupRepositoryTests
    {
        private readonly DbFixture _fixture;

        public PartyDetailLookupRepositoryTests(DbFixture fixture)
        {
            _fixture = fixture;
        }

        private PartyDetailLookupRepository CreateRepo()
        {
            var conn = new SqlConnection(_fixture.ConnectionString);
            return new PartyDetailLookupRepository(conn);
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
            string gstNumber = "22AAAAA0000A1Z5",
            string pan = "AAAPA1234A",
            int? gstStateCode = 22,
            int? creditDays = 30,
            bool reverseCharge = false)
        {
            var regId = await EnsureRegistrationTypeAsync();

            await using var ctx = _fixture.CreateFreshDbContext();
            var party = new PartyManagement.Domain.Entities.PartyMaster
            {
                PartyCode = partyCode,
                PartyName = partyName,
                GSTNumber = gstNumber,
                PAN = pan,
                GSTStateCode = gstStateCode,
                CreditDays = creditDays,
                IsGstReverseCharge = reverseCharge,
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

        private async Task SeedAddressAsync(int partyId, string addr1 = "Line 1", string addr2 = "Line 2",
            int cityId = 10, int stateId = 20, string postal = "600001")
        {
            await using var ctx = _fixture.CreateFreshDbContext();
            ctx.PartyAddress.Add(new PartyManagement.Domain.Entities.PartyAddress
            {
                PartyId = partyId,
                AddressType = "Billing",
                AddressLine1 = addr1,
                AddressLine2 = addr2,
                CityId = cityId,
                StateId = stateId,
                CountryId = 1,
                PostalCode = postal
            });
            await ctx.SaveChangesAsync();
        }

        private async Task SeedContactAsync(int partyId, string mobile = "9876543210", string phone = "044-12345678")
        {
            await using var ctx = _fixture.CreateFreshDbContext();
            ctx.PartyContact.Add(new PartyManagement.Domain.Entities.PartyContact
            {
                PartyId = partyId,
                FirstName = "Test",
                LastName = "Contact",
                MobileNo = mobile,
                Phone = phone,
                EmailID = "test@example.com"
            });
            await ctx.SaveChangesAsync();
        }

        [Fact]
        public async Task GetByIdAsync_Returns_Null_When_PartyNotFound()
        {
            await _fixture.ClearAllTablesAsync();

            var result = await CreateRepo().GetByIdAsync(9999);

            result.Should().BeNull();
        }

        [Fact]
        public async Task GetByIdAsync_Returns_Core_Party_Fields()
        {
            await _fixture.ClearAllTablesAsync();
            var id = await SeedPartyAsync(
                partyCode: "P100", partyName: "Acme Corp",
                gstNumber: "22AAAAA0000A1Z5", pan: "AAAAA9999A",
                gstStateCode: 22, creditDays: 45, reverseCharge: true);

            var result = await CreateRepo().GetByIdAsync(id);

            result.Should().NotBeNull();
            result!.Id.Should().Be(id);
            result.PartyCode.Should().Be("P100");
            result.PartyName.Should().Be("Acme Corp");
            result.GSTNumber.Should().Be("22AAAAA0000A1Z5");
            result.PAN.Should().Be("AAAAA9999A");
            result.GSTStateCode.Should().Be(22);
            result.CreditDays.Should().Be(45);
            result.IsGstReverseCharge.Should().BeTrue();
        }

        [Fact]
        public async Task GetByIdAsync_Populates_Address_Fields()
        {
            await _fixture.ClearAllTablesAsync();
            var id = await SeedPartyAsync();
            await SeedAddressAsync(id, addr1: "123 Main St", addr2: "Suite 4B", cityId: 77, stateId: 88, postal: "560001");

            var result = await CreateRepo().GetByIdAsync(id);

            result.Should().NotBeNull();
            result!.AddressLine1.Should().Be("123 Main St");
            result.AddressLine2.Should().Be("Suite 4B");
            result.CityId.Should().Be(77);
            result.StateId.Should().Be(88);
            result.PostalCode.Should().Be("560001");
        }

        [Fact]
        public async Task GetByIdAsync_Populates_Contact_Fields()
        {
            await _fixture.ClearAllTablesAsync();
            var id = await SeedPartyAsync();
            await SeedContactAsync(id, mobile: "9000000001", phone: "080-11112222");

            var result = await CreateRepo().GetByIdAsync(id);

            result.Should().NotBeNull();
            result!.MobileNo.Should().Be("9000000001");
            result.Phone.Should().Be("080-11112222");
        }

        [Fact]
        public async Task GetByIdAsync_Returns_Party_When_No_Address_Or_Contact()
        {
            await _fixture.ClearAllTablesAsync();
            var id = await SeedPartyAsync(partyCode: "P-NO-ADDR");

            var result = await CreateRepo().GetByIdAsync(id);

            result.Should().NotBeNull();
            result!.Id.Should().Be(id);
            result.PartyCode.Should().Be("P-NO-ADDR");
            result.AddressLine1.Should().BeNull();
            result.MobileNo.Should().BeNull();
        }
    }
}
