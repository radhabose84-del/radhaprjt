using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using PartyManagement.Infrastructure.Repositories.Lookups;
using static PartyManagement.Domain.Common.BaseEntity;

namespace PartyManagement.IntegrationTests.Repositories.Lookups
{
    [Collection("DatabaseCollection")]
    public sealed class PartyBankLookupRepositoryTests
    {
        private readonly DbFixture _fixture;

        public PartyBankLookupRepositoryTests(DbFixture fixture)
        {
            _fixture = fixture;
        }

        private PartyBankLookupRepository CreateRepo()
        {
            var conn = new SqlConnection(_fixture.ConnectionString);
            return new PartyBankLookupRepository(conn);
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

        private async Task<int> SeedPartyAsync(string gstNumber)
        {
            var regId = await EnsureRegistrationTypeAsync();

            await using var ctx = _fixture.CreateFreshDbContext();
            var party = new PartyManagement.Domain.Entities.PartyMaster
            {
                PartyCode = "P" + Guid.NewGuid().ToString("N").Substring(0, 6).ToUpper(),
                PartyName = "Test Party",
                GSTNumber = gstNumber,
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

        private async Task SeedBankAsync(
            int partyId, string bankName, string acctNo, string branch, string ifsc,
            bool isDefault = false, bool isPrimary = false)
        {
            await using var ctx = _fixture.CreateFreshDbContext();
            ctx.PartyBank.Add(new PartyManagement.Domain.Entities.PartyBank
            {
                PartyId = partyId,
                BankName = bankName,
                BankAccountNumber = acctNo,
                BankBranch = branch,
                IFSCCode = ifsc,
                IsDefaultAccount = isDefault,
                IsPrimaryAccount = isPrimary
            });
            await ctx.SaveChangesAsync();
        }

        [Fact]
        public async Task GetDefaultBankByGstAsync_Returns_Default_Account()
        {
            await _fixture.ClearAllTablesAsync();
            var partyId = await SeedPartyAsync("22AAAAA0000A1Z5");
            await SeedBankAsync(partyId, "HDFC", "1111", "Chennai", "HDFC0000001", isDefault: true);
            await SeedBankAsync(partyId, "SBI", "2222", "Mumbai", "SBIN0000002");

            var result = await CreateRepo().GetDefaultBankByGstAsync("22AAAAA0000A1Z5");

            result.Should().NotBeNull();
            result!.BankName.Should().Be("HDFC");
            result.IFSCCode.Should().Be("HDFC0000001");
        }

        [Fact]
        public async Task GetDefaultBankByGstAsync_Returns_Primary_Account_When_No_Default()
        {
            await _fixture.ClearAllTablesAsync();
            var partyId = await SeedPartyAsync("22BBBBB0000B1Z5");
            await SeedBankAsync(partyId, "ICICI", "3333", "Pune", "ICIC0000003", isPrimary: true);
            await SeedBankAsync(partyId, "AXIS", "4444", "Delhi", "AXIS0000004");

            var result = await CreateRepo().GetDefaultBankByGstAsync("22BBBBB0000B1Z5");

            result.Should().NotBeNull();
            result!.BankName.Should().Be("ICICI");
        }

        [Fact]
        public async Task GetDefaultBankByGstAsync_Returns_Null_When_No_Party_With_Gst()
        {
            await _fixture.ClearAllTablesAsync();

            var result = await CreateRepo().GetDefaultBankByGstAsync("99ZZZZZ9999Z9Z9");

            result.Should().BeNull();
        }

        [Fact]
        public async Task GetDefaultBankByGstAsync_Returns_Null_When_No_Default_Or_Primary()
        {
            await _fixture.ClearAllTablesAsync();
            var partyId = await SeedPartyAsync("22CCCCC0000C1Z5");
            await SeedBankAsync(partyId, "HDFC", "5555", "Chennai", "HDFC0000005", isDefault: false, isPrimary: false);

            var result = await CreateRepo().GetDefaultBankByGstAsync("22CCCCC0000C1Z5");

            result.Should().BeNull();
        }

        [Fact]
        public async Task GetDefaultBankByGstAsync_Maps_All_Columns()
        {
            await _fixture.ClearAllTablesAsync();
            var partyId = await SeedPartyAsync("22DDDDD0000D1Z5");
            await SeedBankAsync(partyId, "Kotak", "9999", "Bangalore", "KKBK0000009", isDefault: true);

            var result = await CreateRepo().GetDefaultBankByGstAsync("22DDDDD0000D1Z5");

            result.Should().NotBeNull();
            result!.BankName.Should().Be("Kotak");
            result.BankAccountNumber.Should().Be("9999");
            result.BankBranch.Should().Be("Bangalore");
            result.IFSCCode.Should().Be("KKBK0000009");
        }
    }
}
