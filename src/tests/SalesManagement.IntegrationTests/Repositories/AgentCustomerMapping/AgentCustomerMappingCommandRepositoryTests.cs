using Microsoft.EntityFrameworkCore;
using SalesManagement.Infrastructure.Data;
using SalesManagement.Infrastructure.Repositories.AgentCustomerMapping;
using SalesManagement.IntegrationTests.Common;
using static SalesManagement.Domain.Common.BaseEntity;

namespace SalesManagement.IntegrationTests.Repositories.AgentCustomerMapping
{
    [Collection("DatabaseCollection")]
    public sealed class AgentCustomerMappingCommandRepositoryTests
    {
        private readonly DbFixture _fixture;
        public AgentCustomerMappingCommandRepositoryTests(DbFixture fixture) => _fixture = fixture;

        private AgentCustomerMappingCommandRepository CreateRepo(ApplicationDbContext ctx) => new(ctx);

        private async Task<int> EnsureSalesGroupAsync()
        {
            await using var ctx = _fixture.CreateFreshDbContext();

            var org = await ctx.SalesOrganisation.IgnoreQueryFilters()
                .FirstOrDefaultAsync(x => x.SalesOrganisationCode == "ACMSO");
            if (org == null)
            {
                org = new SalesManagement.Domain.Entities.SalesOrganisation
                {
                    SalesOrganisationCode = "ACMSO", SalesOrganisationName = "ACM Org",
                    CompanyId = 1, IsActive = Status.Active, IsDeleted = IsDelete.NotDeleted
                };
                await ctx.SalesOrganisation.AddAsync(org);
                await ctx.SaveChangesAsync();
            }

            var office = await ctx.SalesOffice.IgnoreQueryFilters()
                .FirstOrDefaultAsync(x => x.SalesOrganisationId == org.Id);
            if (office == null)
            {
                office = new SalesManagement.Domain.Entities.SalesOffice
                {
                    SalesOfficeName = "ACM Office",
                    SalesOrganisationId = org.Id,
                    IsActive = Status.Active,
                    IsDeleted = IsDelete.NotDeleted
                };
                await ctx.SalesOffice.AddAsync(office);
                await ctx.SaveChangesAsync();
            }

            var existing = await ctx.SalesGroup.IgnoreQueryFilters()
                .FirstOrDefaultAsync(x => x.SalesOfficeId == office.Id);
            if (existing != null) return existing.Id;

            var g = new SalesManagement.Domain.Entities.SalesGroup
            {
                SalesGroupName = "ACM Group",
                SalesOfficeId = office.Id,
                IsActive = Status.Active,
                IsDeleted = IsDelete.NotDeleted
            };
            await ctx.SalesGroup.AddAsync(g);
            await ctx.SaveChangesAsync();
            return g.Id;
        }

        private async Task<SalesManagement.Domain.Entities.AgentCustomerMapping> BuildEntityAsync(
            int customerId, int agentId, bool isDefault = false)
        {
            var grpId = await EnsureSalesGroupAsync();
            return new SalesManagement.Domain.Entities.AgentCustomerMapping
            {
                CustomerId = customerId,
                AgentId = agentId,
                SalesGroupId = grpId,
                EffectiveFrom = DateTime.UtcNow.Date,
                IsDefaultAgent = isDefault,
                Remarks = "test",
                IsActive = Status.Active,
                IsDeleted = IsDelete.NotDeleted
            };
        }

        private async Task ClearAsync(ApplicationDbContext ctx) =>
            await _fixture.ClearTablesAsync("Sales.AgentCustomerMapping");

        [Fact]
        public async Task CreateAsync_Should_Return_NewId()
        {
            await using var ctx = _fixture.CreateFreshDbContext();
            await ClearAsync(ctx);

            var id = await CreateRepo(ctx).CreateAsync(await BuildEntityAsync(1, 10));

            id.Should().BeGreaterThan(0);
        }

        [Fact]
        public async Task CreateAsync_With_IsDefault_Should_Clear_Other_Defaults_For_Same_Customer()
        {
            await using var ctx = _fixture.CreateFreshDbContext();
            await ClearAsync(ctx);
            var existingId = await CreateRepo(ctx).CreateAsync(await BuildEntityAsync(5, 100, isDefault: true));
            ctx.ChangeTracker.Clear();

            await CreateRepo(ctx).CreateAsync(await BuildEntityAsync(5, 200, isDefault: true));
            ctx.ChangeTracker.Clear();

            var oldDefault = await ctx.AgentCustomerMapping.FirstAsync(x => x.Id == existingId);
            oldDefault.IsDefaultAgent.Should().BeFalse();
        }

        [Fact]
        public async Task CreateAsync_With_IsDefault_False_Should_Not_Clear_Other_Defaults()
        {
            await using var ctx = _fixture.CreateFreshDbContext();
            await ClearAsync(ctx);
            var existingId = await CreateRepo(ctx).CreateAsync(await BuildEntityAsync(6, 300, isDefault: true));
            ctx.ChangeTracker.Clear();

            await CreateRepo(ctx).CreateAsync(await BuildEntityAsync(6, 400, isDefault: false));
            ctx.ChangeTracker.Clear();

            var stillDefault = await ctx.AgentCustomerMapping.FirstAsync(x => x.Id == existingId);
            stillDefault.IsDefaultAgent.Should().BeTrue();
        }

        [Fact]
        public async Task UpdateAsync_Should_Persist_Changes()
        {
            await using var ctx = _fixture.CreateFreshDbContext();
            await ClearAsync(ctx);
            var id = await CreateRepo(ctx).CreateAsync(await BuildEntityAsync(7, 700));
            ctx.ChangeTracker.Clear();

            var entity = await BuildEntityAsync(7, 700);
            entity.Id = id;
            entity.AgentId = 800;
            entity.Remarks = "updated";
            entity.IsActive = Status.Inactive;

            var result = await CreateRepo(ctx).UpdateAsync(entity);
            ctx.ChangeTracker.Clear();

            result.Should().Be(id);
            var reloaded = await ctx.AgentCustomerMapping.FirstAsync(x => x.Id == id);
            reloaded.AgentId.Should().Be(800);
            reloaded.Remarks.Should().Be("updated");
            reloaded.IsActive.Should().Be(Status.Inactive);
        }

        [Fact]
        public async Task UpdateAsync_Should_Persist_Changed_CustomerId()
        {
            await using var ctx = _fixture.CreateFreshDbContext();
            await ClearAsync(ctx);
            var id = await CreateRepo(ctx).CreateAsync(await BuildEntityAsync(customerId: 1096, agentId: 1077));
            ctx.ChangeTracker.Clear();

            var entity = await BuildEntityAsync(customerId: 1103, agentId: 1077);
            entity.Id = id;

            await CreateRepo(ctx).UpdateAsync(entity);
            ctx.ChangeTracker.Clear();

            var reloaded = await ctx.AgentCustomerMapping.FirstAsync(x => x.Id == id);
            reloaded.CustomerId.Should().Be(1103);
        }

        [Fact]
        public async Task UpdateAsync_WithIsDefault_Should_Clear_Defaults_For_New_Customer_Not_Old()
        {
            await using var ctx = _fixture.CreateFreshDbContext();
            await ClearAsync(ctx);

            // Existing default on OLD customer (1096) — should remain unchanged after re-parent
            var oldCustomerDefaultId = await CreateRepo(ctx).CreateAsync(
                await BuildEntityAsync(customerId: 1096, agentId: 2001, isDefault: true));

            // Existing default on NEW customer (1103) — should be cleared when our row is re-parented to 1103
            var newCustomerDefaultId = await CreateRepo(ctx).CreateAsync(
                await BuildEntityAsync(customerId: 1103, agentId: 2002, isDefault: true));

            // Row under test — starts on 1096, non-default
            var rowUnderTestId = await CreateRepo(ctx).CreateAsync(
                await BuildEntityAsync(customerId: 1096, agentId: 2003, isDefault: false));
            ctx.ChangeTracker.Clear();

            // Re-parent to 1103 AND mark as default
            var update = await BuildEntityAsync(customerId: 1103, agentId: 2003, isDefault: true);
            update.Id = rowUnderTestId;
            await CreateRepo(ctx).UpdateAsync(update);
            ctx.ChangeTracker.Clear();

            var oldCustomerDefault = await ctx.AgentCustomerMapping.FirstAsync(x => x.Id == oldCustomerDefaultId);
            var newCustomerDefault = await ctx.AgentCustomerMapping.FirstAsync(x => x.Id == newCustomerDefaultId);

            oldCustomerDefault.IsDefaultAgent.Should().BeTrue("re-parent must not touch the OLD customer's default");
            newCustomerDefault.IsDefaultAgent.Should().BeFalse("new customer's previous default must be cleared");
        }

        [Fact]
        public async Task UpdateAsync_Should_Return_Zero_When_NotFound()
        {
            await using var ctx = _fixture.CreateFreshDbContext();

            var ghost = await BuildEntityAsync(99, 999);
            ghost.Id = 9999999;

            var result = await CreateRepo(ctx).UpdateAsync(ghost);

            result.Should().Be(0);
        }

        [Fact]
        public async Task SoftDeleteAsync_Should_Return_True_When_Successful()
        {
            await using var ctx = _fixture.CreateFreshDbContext();
            await ClearAsync(ctx);
            var id = await CreateRepo(ctx).CreateAsync(await BuildEntityAsync(8, 80));
            ctx.ChangeTracker.Clear();

            var result = await CreateRepo(ctx).SoftDeleteAsync(id, CancellationToken.None);

            result.Should().BeTrue();
        }

        [Fact]
        public async Task SoftDeleteAsync_Should_Flag_Record()
        {
            await using var ctx = _fixture.CreateFreshDbContext();
            await ClearAsync(ctx);
            var id = await CreateRepo(ctx).CreateAsync(await BuildEntityAsync(9, 90));
            ctx.ChangeTracker.Clear();

            await CreateRepo(ctx).SoftDeleteAsync(id, CancellationToken.None);
            ctx.ChangeTracker.Clear();

            var reloaded = await ctx.AgentCustomerMapping.FirstAsync(x => x.Id == id);
            reloaded.IsDeleted.Should().Be(IsDelete.Deleted);
        }

        [Fact]
        public async Task SoftDeleteAsync_Should_Return_False_When_NotFound()
        {
            await using var ctx = _fixture.CreateFreshDbContext();

            var result = await CreateRepo(ctx).SoftDeleteAsync(9999999, CancellationToken.None);

            result.Should().BeFalse();
        }
    }
}
