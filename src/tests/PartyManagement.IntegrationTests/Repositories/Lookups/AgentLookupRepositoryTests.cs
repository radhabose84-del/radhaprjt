using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using PartyManagement.Infrastructure.Repositories.Lookups;
using static PartyManagement.Domain.Common.BaseEntity;

namespace PartyManagement.IntegrationTests.Repositories.Lookups
{
    [Collection("DatabaseCollection")]
    public sealed class AgentLookupRepositoryTests
    {
        private readonly DbFixture _fixture;

        public AgentLookupRepositoryTests(DbFixture fixture)
        {
            _fixture = fixture;
        }

        private AgentLookupRepository CreateRepo()
        {
            var conn = new SqlConnection(_fixture.ConnectionString);
            return new AgentLookupRepository(conn);
        }

        /// <summary>
        /// Seeds (or reuses) a MiscMaster row whose Description matches <paramref name="typeDescription"/>.
        /// The AgentLookupRepository filters parties on mm.Description = 'AGENT', so this is the filter key.
        /// </summary>
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
        public async Task GetAllAgentAsync_Returns_Only_Parties_Of_Type_AGENT()
        {
            await _fixture.ClearAllTablesAsync();
            await SeedPartyWithTypeAsync("AG001", "Agent Alpha", "AGENT");
            await SeedPartyWithTypeAsync("CU001", "Customer One", "CUSTOMER");
            await SeedPartyWithTypeAsync("AG002", "Agent Beta", "AGENT");

            var result = await CreateRepo().GetAllAgentAsync();

            result.Select(r => r.AgentCode).Should().BeEquivalentTo(new[] { "AG001", "AG002" });
        }

        [Fact]
        public async Task GetAllAgentAsync_Orders_By_PartyName_Asc()
        {
            await _fixture.ClearAllTablesAsync();
            await SeedPartyWithTypeAsync("AG1", "Zulu Agent", "AGENT");
            await SeedPartyWithTypeAsync("AG2", "Alpha Agent", "AGENT");
            await SeedPartyWithTypeAsync("AG3", "Mike Agent", "AGENT");

            var result = await CreateRepo().GetAllAgentAsync();

            result.Select(r => r.AgentName).Should().ContainInOrder("Alpha Agent", "Mike Agent", "Zulu Agent");
        }

        [Fact]
        public async Task GetAllAgentAsync_Excludes_Inactive_Parties()
        {
            await _fixture.ClearAllTablesAsync();
            await SeedPartyWithTypeAsync("AG1", "Active Agent", "AGENT");
            await SeedPartyWithTypeAsync("AG2", "Inactive Agent", "AGENT", isActive: false);

            var result = await CreateRepo().GetAllAgentAsync();

            result.Should().ContainSingle().Which.AgentCode.Should().Be("AG1");
        }

        [Fact]
        public async Task GetAllAgentAsync_Excludes_SoftDeleted_Parties()
        {
            await _fixture.ClearAllTablesAsync();
            await SeedPartyWithTypeAsync("AG1", "Kept", "AGENT");
            await SeedPartyWithTypeAsync("AG2", "Deleted", "AGENT", isDeleted: true);

            var result = await CreateRepo().GetAllAgentAsync();

            result.Should().ContainSingle().Which.AgentCode.Should().Be("AG1");
        }

        [Fact]
        public async Task GetAllAgentAsync_Returns_Empty_When_NoData()
        {
            await _fixture.ClearAllTablesAsync();

            var result = await CreateRepo().GetAllAgentAsync();

            result.Should().BeEmpty();
        }
    }
}
