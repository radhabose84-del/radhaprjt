using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using PartyManagement.Infrastructure.Repositories.Lookups;
using static PartyManagement.Domain.Common.BaseEntity;

namespace PartyManagement.IntegrationTests.Repositories.Lookups
{
    [Collection("DatabaseCollection")]
    public sealed class CustomerLookupRepositoryTests
    {
        private readonly DbFixture _fixture;

        public CustomerLookupRepositoryTests(DbFixture fixture)
        {
            _fixture = fixture;
        }

        private CustomerLookupRepository CreateRepo()
        {
            var conn = new SqlConnection(_fixture.ConnectionString);
            return new CustomerLookupRepository(conn);
        }

        private async Task<int> EnsureTypeMiscAsync(string typeDescription)
        {
            await using var ctx = _fixture.CreateFreshDbContext();

            var existing = await ctx.MiscMaster.FirstOrDefaultAsync(m => m.Description == typeDescription);
            if (existing != null)
                return existing.Id;

            var miscType = await ctx.MiscTypeMaster.FirstOrDefaultAsync(t => t.MiscTypeCode == "PartyType");
            if (miscType == null)
            {
                miscType = new PartyManagement.Domain.Entities.MiscTypeMaster
                {
                    MiscTypeCode = "PartyType",
                    Description = "Party Type",
                    IsActive = Status.Active,
                    IsDeleted = IsDelete.NotDeleted
                };
                ctx.MiscTypeMaster.Add(miscType);
                await ctx.SaveChangesAsync();
            }

            var misc = new PartyManagement.Domain.Entities.MiscMaster
            {
                MiscTypeId = miscType.Id,
                Code = typeDescription,
                Description = typeDescription,
                SortOrder = 1,
                IsActive = Status.Active,
                IsDeleted = IsDelete.NotDeleted
            };
            ctx.MiscMaster.Add(misc);
            await ctx.SaveChangesAsync();

            return misc.Id;
        }

        private async Task<int> EnsurePartyGroupAsync()
        {
            await using var ctx = _fixture.CreateFreshDbContext();
            var existing = await ctx.PartyGroup.FirstOrDefaultAsync();
            if (existing != null) return existing.Id;

            var miscId = await EnsureTypeMiscAsync("GroupType");
            var group = new PartyManagement.Domain.Entities.PartyGroup
            {
                PartyGroupName = "TestGroup",
                GroupTypeId = miscId,
                GlCategoryId = miscId,
                IsActive = Status.Active,
                IsDeleted = IsDelete.NotDeleted
            };
            ctx.PartyGroup.Add(group);
            await ctx.SaveChangesAsync();
            return group.Id;
        }

        private async Task<int> SeedPartyWithTypeAsync(
            string partyCode, string partyName, string typeDescription,
            bool isActive = true, bool isDeleted = false)
        {
            var miscId = await EnsureTypeMiscAsync(typeDescription);
            var groupId = await EnsurePartyGroupAsync();

            await using var ctx = _fixture.CreateFreshDbContext();

            var party = new PartyManagement.Domain.Entities.PartyMaster
            {
                PartyCode = partyCode,
                PartyName = partyName,
                RegistrationTypeId = miscId,
                UnitId = 1,
                StatusId = miscId,
                IsActive = isActive ? Status.Active : Status.Inactive,
                IsDeleted = isDeleted ? IsDelete.Deleted : IsDelete.NotDeleted
            };
            ctx.PartyMaster.Add(party);
            await ctx.SaveChangesAsync();

            ctx.PartyType.Add(new PartyManagement.Domain.Entities.PartyType
            {
                PartyId = party.Id,
                PartyTypeId = miscId,
                PartyGroupId = groupId
            });
            await ctx.SaveChangesAsync();

            return party.Id;
        }

        [Fact]
        public async Task GetAllCustomerAsync_Returns_Only_Parties_Of_Type_CUSTOMER()
        {
            await _fixture.ClearAllTablesAsync();
            await SeedPartyWithTypeAsync("CU001", "Customer Alpha", "CUSTOMER");
            await SeedPartyWithTypeAsync("AG001", "Agent One", "AGENT");
            await SeedPartyWithTypeAsync("CU002", "Customer Beta", "CUSTOMER");

            var result = await CreateRepo().GetAllCustomerAsync();

            result.Select(r => r.CustomerCode).Should().BeEquivalentTo(new[] { "CU001", "CU002" });
        }

        [Fact]
        public async Task GetAllCustomerAsync_Orders_By_PartyName_Asc()
        {
            await _fixture.ClearAllTablesAsync();
            await SeedPartyWithTypeAsync("C1", "Zulu Corp", "CUSTOMER");
            await SeedPartyWithTypeAsync("C2", "Alpha Inc", "CUSTOMER");
            await SeedPartyWithTypeAsync("C3", "Mike Ltd", "CUSTOMER");

            var result = await CreateRepo().GetAllCustomerAsync();

            result.Select(r => r.CustomerName).Should().ContainInOrder("Alpha Inc", "Mike Ltd", "Zulu Corp");
        }

        [Fact]
        public async Task GetAllCustomerAsync_Excludes_Inactive()
        {
            await _fixture.ClearAllTablesAsync();
            await SeedPartyWithTypeAsync("C1", "Active", "CUSTOMER");
            await SeedPartyWithTypeAsync("C2", "Inactive", "CUSTOMER", isActive: false);

            var result = await CreateRepo().GetAllCustomerAsync();

            result.Should().ContainSingle().Which.CustomerCode.Should().Be("C1");
        }

        [Fact]
        public async Task GetAllCustomerAsync_Excludes_SoftDeleted()
        {
            await _fixture.ClearAllTablesAsync();
            await SeedPartyWithTypeAsync("C1", "Keep", "CUSTOMER");
            await SeedPartyWithTypeAsync("C2", "Deleted", "CUSTOMER", isDeleted: true);

            var result = await CreateRepo().GetAllCustomerAsync();

            result.Should().ContainSingle().Which.CustomerCode.Should().Be("C1");
        }

        [Fact]
        public async Task GetAllCustomerAsync_Returns_Empty_When_NoData()
        {
            await _fixture.ClearAllTablesAsync();

            var result = await CreateRepo().GetAllCustomerAsync();

            result.Should().BeEmpty();
        }
    }
}
