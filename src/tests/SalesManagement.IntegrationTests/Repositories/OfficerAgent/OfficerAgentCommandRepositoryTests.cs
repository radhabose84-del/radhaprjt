using Dapper;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using SalesManagement.Infrastructure.Data;
using SalesManagement.Infrastructure.Repositories.OfficerAgent;
using SalesManagement.IntegrationTests.Common;
using static SalesManagement.Domain.Common.BaseEntity;

namespace SalesManagement.IntegrationTests.Repositories.OfficerAgent
{
    /// <summary>
    /// Integration tests for OfficerAgentCommandRepository.
    /// Note: OfficerAgent does NOT extend BaseEntity — it uses hard delete (DeleteAsync), not soft delete.
    /// The batch operations CreateBatchAsync and UpdateBatchAsync are tested here.
    /// OfficerAgent has a FK to Sales.MarketingOfficer, so MarketingOfficer must be seeded.
    /// </summary>
    [Collection("DatabaseCollection")]
    public sealed class OfficerAgentCommandRepositoryTests
    {
        private readonly DbFixture _fixture;

        public OfficerAgentCommandRepositoryTests(DbFixture fixture)
        {
            _fixture = fixture;
        }

        private OfficerAgentCommandRepository CreateRepository(ApplicationDbContext ctx)
            => new OfficerAgentCommandRepository(ctx);

        private async Task ClearTableAsync(ApplicationDbContext ctx)
        {
            await ctx.Database.ExecuteSqlRawAsync("DELETE FROM Sales.OfficerAgent");
        }

        private async Task<int> EnsureMarketingOfficerAsync()
        {
            await using var conn = new SqlConnection(_fixture.ConnectionString);
            await conn.OpenAsync();
            var existingId = await conn.ExecuteScalarAsync<int>(
                "SELECT TOP 1 Id FROM Sales.MarketingOfficer WHERE IsDeleted = 0");
            if (existingId > 0) return existingId;

            // Need SalesOffice for MarketingOfficer
            var salesOfficeId = await conn.ExecuteScalarAsync<int>(
                "SELECT TOP 1 Id FROM Sales.SalesOffice WHERE IsDeleted = 0");
            if (salesOfficeId == 0)
            {
                await using var ctx = _fixture.CreateFreshDbContext();
                var so = new Domain.Entities.SalesOffice
                {
                    SalesOfficeName = "Test Sales Office OA",
                    IsActive = Status.Active, IsDeleted = IsDelete.NotDeleted
                };
                ctx.SalesOffice.Add(so);
                await ctx.SaveChangesAsync();
                salesOfficeId = so.Id;
            }

            await using var ctx2 = _fixture.CreateFreshDbContext();
            var mo = new Domain.Entities.MarketingOfficer
            {
                EmployeeNo = "EMP_OA001",
                EmployeeName = "Test Officer OA",
                MobileNo = "9876543210",
                SalesOfficeId = salesOfficeId,
                IsActive = Status.Active, IsDeleted = IsDelete.NotDeleted
            };
            ctx2.MarketingOfficer.Add(mo);
            await ctx2.SaveChangesAsync();
            return mo.Id;
        }

        private Domain.Entities.OfficerAgent BuildEntity(int agentId, int marketingOfficerId)
            => new Domain.Entities.OfficerAgent
            {
                AgentId = agentId,
                MarketingOfficerId = marketingOfficerId,
                ValidityFrom = DateOnly.FromDateTime(DateTime.UtcNow),
                ValidityTo = DateOnly.FromDateTime(DateTime.UtcNow.AddYears(1)),
                IsActive = true
            };

        // ── CreateBatchAsync ─────────────────────────────────────────────────

        [Fact]
        public async Task CreateBatchAsync_Should_Return_Count_Of_Created_Records()
        {
            await using var ctx = _fixture.CreateFreshDbContext();
            await ClearTableAsync(ctx);
            var moId = await EnsureMarketingOfficerAsync();

            var entities = new List<Domain.Entities.OfficerAgent>
            {
                BuildEntity(agentId: 10, moId),
                BuildEntity(agentId: 11, moId)
            };

            var count = await CreateRepository(ctx).CreateBatchAsync(entities);

            count.Should().Be(2);
        }

        [Fact]
        public async Task CreateBatchAsync_Should_Persist_All_Entities()
        {
            await using var ctx = _fixture.CreateFreshDbContext();
            await ClearTableAsync(ctx);
            var moId = await EnsureMarketingOfficerAsync();

            var entities = new List<Domain.Entities.OfficerAgent>
            {
                BuildEntity(agentId: 20, moId),
                BuildEntity(agentId: 21, moId),
                BuildEntity(agentId: 22, moId)
            };

            await CreateRepository(ctx).CreateBatchAsync(entities);
            ctx.ChangeTracker.Clear();

            var allIds = entities.Select(e => e.Id).ToList();
            var savedCount = await ctx.OfficerAgent
                .CountAsync(x => allIds.Contains(x.Id));

            savedCount.Should().Be(3);
        }

        [Fact]
        public async Task CreateBatchAsync_Should_Populate_Audit_Fields()
        {
            await using var ctx = _fixture.CreateFreshDbContext();
            await ClearTableAsync(ctx);
            var moId = await EnsureMarketingOfficerAsync();

            var entities = new List<Domain.Entities.OfficerAgent>
            {
                BuildEntity(agentId: 30, moId)
            };

            await CreateRepository(ctx).CreateBatchAsync(entities);
            ctx.ChangeTracker.Clear();

            var saved = await ctx.OfficerAgent.FirstOrDefaultAsync(x => x.Id == entities[0].Id);

            saved.Should().NotBeNull();
            saved!.CreatedBy.Should().Be(1);
            saved.CreatedByName.Should().Be("test-user");
            saved.CreatedIP.Should().Be("127.0.0.1");
            saved.CreatedDate.Should().NotBeNull();
        }

        // ── UpdateBatchAsync ─────────────────────────────────────────────────

        [Fact]
        public async Task UpdateBatchAsync_Should_Persist_Updated_Fields()
        {
            await using var ctx = _fixture.CreateFreshDbContext();
            await ClearTableAsync(ctx);
            var moId = await EnsureMarketingOfficerAsync();

            var repo = CreateRepository(ctx);
            var entities = new List<Domain.Entities.OfficerAgent>
            {
                BuildEntity(agentId: 40, moId)
            };
            await repo.CreateBatchAsync(entities);
            ctx.ChangeTracker.Clear();

            var createdId = entities[0].Id;
            var newValidityTo = DateOnly.FromDateTime(DateTime.UtcNow.AddYears(2));
            var updates = new List<Domain.Entities.OfficerAgent>
            {
                new Domain.Entities.OfficerAgent
                {
                    Id = createdId,
                    AgentId = 41,
                    MarketingOfficerId = moId,
                    ValidityFrom = DateOnly.FromDateTime(DateTime.UtcNow),
                    ValidityTo = newValidityTo,
                    IsActive = false
                }
            };

            await repo.UpdateBatchAsync(updates);
            ctx.ChangeTracker.Clear();

            var saved = await ctx.OfficerAgent.FirstOrDefaultAsync(x => x.Id == createdId);
            saved!.AgentId.Should().Be(41);
            saved.IsActive.Should().BeFalse();
            saved.ValidityTo.Should().Be(newValidityTo);
        }

        // ── DeleteAsync (hard delete) ────────────────────────────────────────

        [Fact]
        public async Task DeleteAsync_Should_Return_True_WhenEntityExists()
        {
            await using var ctx = _fixture.CreateFreshDbContext();
            await ClearTableAsync(ctx);
            var moId = await EnsureMarketingOfficerAsync();

            var repo = CreateRepository(ctx);
            var entities = new List<Domain.Entities.OfficerAgent> { BuildEntity(agentId: 50, moId) };
            await repo.CreateBatchAsync(entities);
            ctx.ChangeTracker.Clear();

            var result = await repo.DeleteAsync(entities[0].Id, CancellationToken.None);

            result.Should().BeTrue();
        }

        [Fact]
        public async Task DeleteAsync_Should_Physically_Remove_Record()
        {
            await using var ctx = _fixture.CreateFreshDbContext();
            await ClearTableAsync(ctx);
            var moId = await EnsureMarketingOfficerAsync();

            var repo = CreateRepository(ctx);
            var entities = new List<Domain.Entities.OfficerAgent> { BuildEntity(agentId: 60, moId) };
            await repo.CreateBatchAsync(entities);
            ctx.ChangeTracker.Clear();

            var id = entities[0].Id;
            await repo.DeleteAsync(id, CancellationToken.None);
            ctx.ChangeTracker.Clear();

            var saved = await ctx.OfficerAgent.FirstOrDefaultAsync(x => x.Id == id);
            saved.Should().BeNull();
        }

        [Fact]
        public async Task DeleteAsync_Should_Return_False_WhenNotFound()
        {
            await using var ctx = _fixture.CreateFreshDbContext();
            await ClearTableAsync(ctx);

            var result = await CreateRepository(ctx).DeleteAsync(99999, CancellationToken.None);

            result.Should().BeFalse();
        }
    }
}
