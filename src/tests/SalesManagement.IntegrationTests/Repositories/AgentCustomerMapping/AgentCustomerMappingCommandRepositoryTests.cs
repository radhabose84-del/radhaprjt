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

        public AgentCustomerMappingCommandRepositoryTests(DbFixture fixture)
        {
            _fixture = fixture;
        }

        private AgentCustomerMappingCommandRepository CreateRepository(ApplicationDbContext ctx)
            => new AgentCustomerMappingCommandRepository(ctx);

        private Domain.Entities.AgentCustomerMapping BuildEntity(
            int customerId = 1,
            int agentId = 2,
            int salesSegmentId = 1,
            bool isDefaultAgent = false,
            string? remarks = "Test Remarks")
            => new Domain.Entities.AgentCustomerMapping
            {
                CustomerId = customerId,
                AgentId = agentId,
                SubAgentId = null,
                SalesSegmentId = salesSegmentId,
                EffectiveFrom = DateTime.UtcNow.Date,
                EffectiveTo = null,
                IsDefaultAgent = isDefaultAgent,
                Remarks = remarks,
                IsActive = Status.Active,
                IsDeleted = IsDelete.NotDeleted
            };

        private async Task ClearTableAsync(ApplicationDbContext ctx)
        {
            await ctx.Database.ExecuteSqlRawAsync("DELETE FROM Sales.AgentCustomerMapping");
        }

        // ── CreateAsync ──────────────────────────────────────────────────────

        [Fact]
        public async Task CreateAsync_Should_Return_NewId_GreaterThanZero()
        {
            await using var ctx = _fixture.CreateFreshDbContext();
            await ClearTableAsync(ctx);

            var newId = await CreateRepository(ctx).CreateAsync(BuildEntity());

            newId.Should().BeGreaterThan(0);
        }

        [Fact]
        public async Task CreateAsync_Should_Persist_Fields_Correctly()
        {
            await using var ctx = _fixture.CreateFreshDbContext();
            await ClearTableAsync(ctx);

            var entity = BuildEntity(customerId: 10, agentId: 20, salesSegmentId: 1, isDefaultAgent: false, remarks: "My Remarks");
            var newId = await CreateRepository(ctx).CreateAsync(entity);
            ctx.ChangeTracker.Clear();

            var saved = await ctx.AgentCustomerMapping.FirstOrDefaultAsync(x => x.Id == newId);

            saved.Should().NotBeNull();
            saved!.CustomerId.Should().Be(10);
            saved.AgentId.Should().Be(20);
            saved.Remarks.Should().Be("My Remarks");
            saved.IsDefaultAgent.Should().BeFalse();
            saved.IsActive.Should().Be(Status.Active);
            saved.IsDeleted.Should().Be(IsDelete.NotDeleted);
        }

        [Fact]
        public async Task CreateAsync_Should_Populate_Audit_Fields()
        {
            await using var ctx = _fixture.CreateFreshDbContext();
            await ClearTableAsync(ctx);

            var newId = await CreateRepository(ctx).CreateAsync(BuildEntity());
            ctx.ChangeTracker.Clear();

            var saved = await ctx.AgentCustomerMapping.FirstOrDefaultAsync(x => x.Id == newId);

            saved!.CreatedBy.Should().Be(1);
            saved.CreatedByName.Should().Be("test-user");
            saved.CreatedIP.Should().Be("127.0.0.1");
            saved.CreatedDate.Should().NotBeNull();
        }

        [Fact]
        public async Task CreateAsync_Should_Clear_ExistingDefault_WhenNewDefaultCreated()
        {
            await using var ctx = _fixture.CreateFreshDbContext();
            await ClearTableAsync(ctx);

            var repo = CreateRepository(ctx);
            // First mapping is default
            var firstId = await repo.CreateAsync(BuildEntity(customerId: 5, agentId: 10, isDefaultAgent: true));
            ctx.ChangeTracker.Clear();

            // Second mapping for same customer is also default — should clear first
            await repo.CreateAsync(BuildEntity(customerId: 5, agentId: 11, isDefaultAgent: true));
            ctx.ChangeTracker.Clear();

            var first = await ctx.AgentCustomerMapping.FirstOrDefaultAsync(x => x.Id == firstId);
            first!.IsDefaultAgent.Should().BeFalse();
        }

        // ── UpdateAsync ──────────────────────────────────────────────────────

        [Fact]
        public async Task UpdateAsync_Should_Persist_Mutable_Fields()
        {
            await using var ctx = _fixture.CreateFreshDbContext();
            await ClearTableAsync(ctx);

            var repo = CreateRepository(ctx);
            var id = await repo.CreateAsync(BuildEntity(customerId: 1, agentId: 2, remarks: "Original"));
            ctx.ChangeTracker.Clear();

            var updated = new Domain.Entities.AgentCustomerMapping
            {
                Id = id,
                AgentId = 5,
                SubAgentId = 6,
                EffectiveFrom = DateTime.UtcNow.Date,
                EffectiveTo = DateTime.UtcNow.Date.AddMonths(6),
                IsDefaultAgent = false,
                Remarks = "Updated Remarks",
                IsActive = Status.Inactive
            };

            var resultId = await repo.UpdateAsync(updated);
            ctx.ChangeTracker.Clear();

            resultId.Should().Be(id);
            var saved = await ctx.AgentCustomerMapping.FirstOrDefaultAsync(x => x.Id == id);
            saved!.AgentId.Should().Be(5);
            saved.SubAgentId.Should().Be(6);
            saved.Remarks.Should().Be("Updated Remarks");
            saved.IsActive.Should().Be(Status.Inactive);
        }

        [Fact]
        public async Task UpdateAsync_Should_Return_Zero_WhenNotFound()
        {
            await using var ctx = _fixture.CreateFreshDbContext();
            await ClearTableAsync(ctx);

            var updated = new Domain.Entities.AgentCustomerMapping
            {
                Id = 99999,
                AgentId = 1,
                EffectiveFrom = DateTime.UtcNow.Date,
                IsActive = Status.Active
            };

            var resultId = await CreateRepository(ctx).UpdateAsync(updated);

            resultId.Should().Be(0);
        }

        // ── SoftDeleteAsync ──────────────────────────────────────────────────

        [Fact]
        public async Task SoftDeleteAsync_Should_Return_True_WhenEntityExists()
        {
            await using var ctx = _fixture.CreateFreshDbContext();
            await ClearTableAsync(ctx);

            var repo = CreateRepository(ctx);
            var id = await repo.CreateAsync(BuildEntity());
            ctx.ChangeTracker.Clear();

            var result = await repo.SoftDeleteAsync(id, CancellationToken.None);

            result.Should().BeTrue();
        }

        [Fact]
        public async Task SoftDeleteAsync_Should_Set_IsDeleted_Flag()
        {
            await using var ctx = _fixture.CreateFreshDbContext();
            await ClearTableAsync(ctx);

            var repo = CreateRepository(ctx);
            var id = await repo.CreateAsync(BuildEntity());
            ctx.ChangeTracker.Clear();

            await repo.SoftDeleteAsync(id, CancellationToken.None);
            ctx.ChangeTracker.Clear();

            var saved = await ctx.AgentCustomerMapping
                .IgnoreQueryFilters()
                .FirstOrDefaultAsync(x => x.Id == id);

            saved!.IsDeleted.Should().Be(IsDelete.Deleted);
        }

        [Fact]
        public async Task SoftDeleteAsync_Should_Return_False_WhenNotFound()
        {
            await using var ctx = _fixture.CreateFreshDbContext();
            await ClearTableAsync(ctx);

            var result = await CreateRepository(ctx).SoftDeleteAsync(99999, CancellationToken.None);

            result.Should().BeFalse();
        }
    }
}
