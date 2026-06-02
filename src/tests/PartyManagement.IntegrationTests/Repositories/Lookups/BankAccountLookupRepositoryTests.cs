using Contracts.Interfaces.Lookups.Users;
using Microsoft.Data.SqlClient;
using PartyManagement.Infrastructure.Repositories.Lookups;
using PartyManagement.IntegrationTests.Common;
using Entities = PartyManagement.Domain.Entities;
using static PartyManagement.Domain.Common.BaseEntity;

namespace PartyManagement.IntegrationTests.Repositories.Lookups
{
    [Collection("DatabaseCollection")]
    public sealed class BankAccountLookupRepositoryTests
    {
        private readonly DbFixture _fixture;

        public BankAccountLookupRepositoryTests(DbFixture fixture)
        {
            _fixture = fixture;
        }

        private BankAccountLookupRepository CreateRepo(Mock<ICityLookup>? cityLookup = null, Mock<IStateLookup>? stateLookup = null)
        {
            var conn = new SqlConnection(_fixture.ConnectionString);
            cityLookup ??= new Mock<ICityLookup>(MockBehavior.Loose);
            stateLookup ??= new Mock<IStateLookup>(MockBehavior.Loose);
            return new BankAccountLookupRepository(conn, cityLookup.Object, stateLookup.Object);
        }

        // Creates a MiscType + a MiscMaster row with the given code, returns the MiscMaster Id.
        private async Task<int> SeedMiscAsync(string code)
        {
            await using var ctx = _fixture.CreateFreshDbContext();
            var miscType = new Entities.MiscTypeMaster
            {
                MiscTypeCode = "MT_" + code,
                Description = code + " Type",
                IsActive = Status.Active,
                IsDeleted = IsDelete.NotDeleted
            };
            ctx.MiscTypeMaster.Add(miscType);
            await ctx.SaveChangesAsync();

            var misc = new Entities.MiscMaster
            {
                MiscTypeId = miscType.Id,
                Code = code,
                Description = code,
                SortOrder = 1,
                IsActive = Status.Active,
                IsDeleted = IsDelete.NotDeleted
            };
            ctx.MiscMaster.Add(misc);
            await ctx.SaveChangesAsync();
            return misc.Id;
        }

        private async Task<int> SeedBankMasterAsync(string bankName)
        {
            await using var ctx = _fixture.CreateFreshDbContext();
            var bank = new Entities.BankMaster
            {
                BankCode = "B" + Guid.NewGuid().ToString("N").Substring(0, 6).ToUpper(),
                BankName = bankName,
                IsActive = Status.Active,
                IsDeleted = IsDelete.NotDeleted
            };
            ctx.Set<Entities.BankMaster>().Add(bank);
            await ctx.SaveChangesAsync();
            return bank.Id;
        }

        private async Task<int> SeedBankAccountAsync(
            int bankId, int miscId, int? ownerTypeId, string accountNumber,
            string holder = "Test Holder", Status isActive = Status.Active,
            string? addressLine1 = null, int? cityId = null, int? stateId = null, string? pincode = null)
        {
            await using var ctx = _fixture.CreateFreshDbContext();
            var account = new Entities.BankAccount
            {
                BankId = bankId,
                AccountHolderName = holder,
                AccountNumber = accountNumber,
                BranchId = miscId,
                AccountTypeId = miscId,
                OwnerTypeId = ownerTypeId,
                IFSCCode = "HDFC0000001",
                AddressLine1 = addressLine1,
                CityId = cityId,
                StateId = stateId,
                Pincode = pincode,
                IsActive = isActive,
                IsDeleted = IsDelete.NotDeleted
            };
            ctx.Set<Entities.BankAccount>().Add(account);
            await ctx.SaveChangesAsync();
            return account.Id;
        }

        [Fact]
        public async Task ExistsForOwnerTypeAsync_ReturnsTrue_ForMatchingActiveUnitAccount()
        {
            await _fixture.ClearAllTablesAsync();
            var unitTypeId = await SeedMiscAsync("Unit");
            var bankId = await SeedBankMasterAsync("HDFC");
            var id = await SeedBankAccountAsync(bankId, unitTypeId, unitTypeId, "1001");

            var exists = await CreateRepo().ExistsForOwnerTypeAsync(id, "Unit");

            exists.Should().BeTrue();
        }

        [Fact]
        public async Task ExistsForOwnerTypeAsync_ReturnsFalse_ForDifferentOwnerType()
        {
            await _fixture.ClearAllTablesAsync();
            var partyTypeId = await SeedMiscAsync("Party");
            var bankId = await SeedBankMasterAsync("SBI");
            var id = await SeedBankAccountAsync(bankId, partyTypeId, partyTypeId, "1002");

            var exists = await CreateRepo().ExistsForOwnerTypeAsync(id, "Unit");

            exists.Should().BeFalse();
        }

        [Fact]
        public async Task ExistsForOwnerTypeAsync_ReturnsTrue_ForMatchingPartyAccount()
        {
            await _fixture.ClearAllTablesAsync();
            var partyTypeId = await SeedMiscAsync("Party");
            var bankId = await SeedBankMasterAsync("Yes Bank");
            var id = await SeedBankAccountAsync(bankId, partyTypeId, partyTypeId, "1500");

            var exists = await CreateRepo().ExistsForOwnerTypeAsync(id, "Party");

            exists.Should().BeTrue();
        }

        [Fact]
        public async Task ExistsForOwnerTypeAsync_ReturnsFalse_WhenInactive()
        {
            await _fixture.ClearAllTablesAsync();
            var unitTypeId = await SeedMiscAsync("Unit");
            var bankId = await SeedBankMasterAsync("ICICI");
            var id = await SeedBankAccountAsync(bankId, unitTypeId, unitTypeId, "1003", isActive: Status.Inactive);

            var exists = await CreateRepo().ExistsForOwnerTypeAsync(id, "Unit");

            exists.Should().BeFalse();
        }

        [Fact]
        public async Task GetByIdAsync_Returns_Account_With_BankName()
        {
            await _fixture.ClearAllTablesAsync();
            var unitTypeId = await SeedMiscAsync("Unit");
            var bankId = await SeedBankMasterAsync("Kotak");
            var id = await SeedBankAccountAsync(bankId, unitTypeId, unitTypeId, "1004", holder: "Acme Unit");

            var dto = await CreateRepo().GetByIdAsync(id);

            dto.Should().NotBeNull();
            dto!.AccountNumber.Should().Be("1004");
            dto.AccountHolderName.Should().Be("Acme Unit");
            dto.BankName.Should().Be("Kotak");
            dto.IFSCCode.Should().Be("HDFC0000001");
            // Branch + AccountType both point at the seeded "Unit" misc row in this test
            dto.AccountTypeName.Should().Be("Unit");
            dto.BranchName.Should().Be("Unit");
        }

        [Fact]
        public async Task GetByIdAsync_Returns_Address_With_Resolved_City_And_State()
        {
            await _fixture.ClearAllTablesAsync();
            var unitTypeId = await SeedMiscAsync("Unit");
            var bankId = await SeedBankMasterAsync("ICICI");
            var id = await SeedBankAccountAsync(bankId, unitTypeId, unitTypeId, "7001",
                addressLine1: "12 MG Road", cityId: 5, stateId: 9, pincode: "641001");

            var cityLookup = new Mock<ICityLookup>(MockBehavior.Loose);
            cityLookup.Setup(c => c.GetByIdAsync(5, It.IsAny<CancellationToken>()))
                      .ReturnsAsync(new Contracts.Dtos.Lookups.Users.CityLookupDto { CityId = 5, CityName = "Coimbatore" });
            var stateLookup = new Mock<IStateLookup>(MockBehavior.Loose);
            stateLookup.Setup(s => s.GetByIdAsync(9, It.IsAny<CancellationToken>()))
                       .ReturnsAsync(new Contracts.Dtos.Lookups.Users.StateLookupDto { StateId = 9, StateName = "Tamil Nadu" });

            var dto = await CreateRepo(cityLookup, stateLookup).GetByIdAsync(id);

            dto.Should().NotBeNull();
            dto!.AddressLine1.Should().Be("12 MG Road");
            dto.Pincode.Should().Be("641001");
            dto.CityId.Should().Be(5);
            dto.CityName.Should().Be("Coimbatore");
            dto.StateId.Should().Be(9);
            dto.StateName.Should().Be("Tamil Nadu");
        }

        [Fact]
        public async Task GetByOwnerTypeAsync_Returns_Only_Matching_Owner_Type()
        {
            await _fixture.ClearAllTablesAsync();
            var unitTypeId = await SeedMiscAsync("Unit");
            var partyTypeId = await SeedMiscAsync("Party");
            var bankId = await SeedBankMasterAsync("Axis");
            await SeedBankAccountAsync(bankId, unitTypeId, unitTypeId, "2001");
            await SeedBankAccountAsync(bankId, partyTypeId, partyTypeId, "2002");

            var results = await CreateRepo().GetByOwnerTypeAsync("Unit", null);

            results.Should().HaveCount(1);
            results[0].AccountNumber.Should().Be("2001");
        }
    }
}
