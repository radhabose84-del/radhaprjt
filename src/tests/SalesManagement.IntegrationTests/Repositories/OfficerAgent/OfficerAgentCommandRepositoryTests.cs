using Microsoft.EntityFrameworkCore;
using SalesManagement.Infrastructure.Data;
using SalesManagement.Infrastructure.Repositories.OfficerAgent;
using SalesManagement.IntegrationTests.Common;
using static SalesManagement.Domain.Common.BaseEntity;

namespace SalesManagement.IntegrationTests.Repositories.OfficerAgent
{
    [Collection("DatabaseCollection")]
    public sealed class OfficerAgentCommandRepositoryTests
    {
        private readonly DbFixture _fixture;
        public OfficerAgentCommandRepositoryTests(DbFixture fixture) => _fixture = fixture;

        private OfficerAgentCommandRepository CreateRepo(ApplicationDbContext ctx) => new(ctx);

        private async Task<int> EnsureMarketingOfficerAsync(string empNo = "OAC_MO")
        {
            await using var ctx = _fixture.CreateFreshDbContext();
            var existing = await ctx.MarketingOfficer.FirstOrDefaultAsync(x => x.EmployeeNo == empNo);
            if (existing != null) return existing.Id;

            var org = await ctx.SalesOrganisation.FirstOrDefaultAsync(x => x.SalesOrganisationCode == "OAC_ORG");
            if (org == null)
            {
                org = new SalesManagement.Domain.Entities.SalesOrganisation
                {
                    SalesOrganisationCode = "OAC_ORG", SalesOrganisationName = "Org",
                    CompanyId = 1, IsActive = Status.Active, IsDeleted = IsDelete.NotDeleted
                };
                await ctx.SalesOrganisation.AddAsync(org);
                await ctx.SaveChangesAsync();
            }
            var office = await ctx.SalesOffice.FirstOrDefaultAsync(x => x.SalesOfficeName == "OAC_OFC");
            if (office == null)
            {
                office = new SalesManagement.Domain.Entities.SalesOffice
                {
                    SalesOfficeName = "OAC_OFC", SalesOrganisationId = org.Id,
                    IsActive = Status.Active, IsDeleted = IsDelete.NotDeleted
                };
                await ctx.SalesOffice.AddAsync(office);
                await ctx.SaveChangesAsync();
            }
            var mo = new SalesManagement.Domain.Entities.MarketingOfficer
            {
                EmployeeNo = empNo,
                EmployeeName = "Officer " + empNo,
                MobileNo = "9876543210",
                Email = "mo@y.com",
                Unit = "U", Department = "Sales", Designation = "Mgr",
                SalesOfficeId = office.Id,
                IsActive = Status.Active, IsDeleted = IsDelete.NotDeleted
            };
            await ctx.MarketingOfficer.AddAsync(mo);
            await ctx.SaveChangesAsync();
            return mo.Id;
        }

        private SalesManagement.Domain.Entities.OfficerAgent BuildEntity(int officerId, int agentId = 111) =>
            new()
            {
                AgentId = agentId,
                MarketingOfficerId = officerId,
                ValidityFrom = DateOnly.FromDateTime(DateTime.UtcNow.Date),
                ValidityTo = DateOnly.FromDateTime(DateTime.UtcNow.Date.AddYears(1)),
                IsActive = true
            };

        private async Task ClearAsync(ApplicationDbContext ctx) =>
            await _fixture.ClearAllTablesAsync();

        [Fact]
        public async Task CreateBatchAsync_Should_Insert_All_And_Return_Count()
        {
            await using var ctx = _fixture.CreateFreshDbContext();
            await ClearAsync(ctx);
            var moId = await EnsureMarketingOfficerAsync();

            var entities = new List<SalesManagement.Domain.Entities.OfficerAgent>
            {
                BuildEntity(moId, 111), BuildEntity(moId, 222), BuildEntity(moId, 333)
            };

            var count = await CreateRepo(ctx).CreateBatchAsync(entities);

            count.Should().Be(3);
            (await ctx.OfficerAgent.CountAsync(x => x.MarketingOfficerId == moId)).Should().Be(3);
        }

        [Fact]
        public async Task CreateBatchAsync_Should_Persist_All_Fields()
        {
            await using var ctx = _fixture.CreateFreshDbContext();
            await ClearAsync(ctx);
            var moId = await EnsureMarketingOfficerAsync();

            var entities = new List<SalesManagement.Domain.Entities.OfficerAgent> { BuildEntity(moId, 555) };

            await CreateRepo(ctx).CreateBatchAsync(entities);
            ctx.ChangeTracker.Clear();

            var saved = await ctx.OfficerAgent.FirstAsync(x => x.AgentId == 555);
            saved.MarketingOfficerId.Should().Be(moId);
            saved.IsActive.Should().BeTrue();
        }

        [Fact]
        public async Task UpdateBatchAsync_Should_Update_Existing_Records()
        {
            await using var ctx = _fixture.CreateFreshDbContext();
            await ClearAsync(ctx);
            var moId = await EnsureMarketingOfficerAsync();

            var e1 = BuildEntity(moId, 100);
            var e2 = BuildEntity(moId, 200);
            await CreateRepo(ctx).CreateBatchAsync(new List<SalesManagement.Domain.Entities.OfficerAgent> { e1, e2 });
            ctx.ChangeTracker.Clear();

            e1.AgentId = 101;
            e1.IsActive = false;
            e2.AgentId = 202;

            var count = await CreateRepo(ctx).UpdateBatchAsync(new List<SalesManagement.Domain.Entities.OfficerAgent> { e1, e2 });
            ctx.ChangeTracker.Clear();

            count.Should().Be(2);
            var row1 = await ctx.OfficerAgent.FirstAsync(x => x.Id == e1.Id);
            row1.AgentId.Should().Be(101);
            row1.IsActive.Should().BeFalse();
            (await ctx.OfficerAgent.FirstAsync(x => x.Id == e2.Id)).AgentId.Should().Be(202);
        }

        [Fact]
        public async Task UpdateBatchAsync_Should_Ignore_Unknown_Ids()
        {
            await using var ctx = _fixture.CreateFreshDbContext();
            await ClearAsync(ctx);

            var ghost = BuildEntity(9999, 1);
            ghost.Id = 9999999;

            var count = await CreateRepo(ctx).UpdateBatchAsync(new List<SalesManagement.Domain.Entities.OfficerAgent> { ghost });

            count.Should().Be(0);
        }

        [Fact]
        public async Task DeleteAsync_Should_Remove_Physical_Record()
        {
            await using var ctx = _fixture.CreateFreshDbContext();
            await ClearAsync(ctx);
            var moId = await EnsureMarketingOfficerAsync();
            var entity = BuildEntity(moId, 400);
            await CreateRepo(ctx).CreateBatchAsync(new List<SalesManagement.Domain.Entities.OfficerAgent> { entity });
            ctx.ChangeTracker.Clear();

            var result = await CreateRepo(ctx).DeleteAsync(entity.Id, CancellationToken.None);
            ctx.ChangeTracker.Clear();

            result.Should().BeTrue();
            (await ctx.OfficerAgent.FirstOrDefaultAsync(x => x.Id == entity.Id)).Should().BeNull();
        }

        [Fact]
        public async Task DeleteAsync_Should_Return_False_When_NotFound()
        {
            await using var ctx = _fixture.CreateFreshDbContext();
            var result = await CreateRepo(ctx).DeleteAsync(9999999, CancellationToken.None);
            result.Should().BeFalse();
        }
    }
}
