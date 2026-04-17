using Microsoft.EntityFrameworkCore;
using ProductionManagement.Infrastructure.Data;
using ProductionManagement.Infrastructure.Repositories.MiscTypeMaster;
using ProductionManagement.IntegrationTests.Common;
using static ProductionManagement.Domain.Common.BaseEntity;

namespace ProductionManagement.IntegrationTests.Repositories.MiscTypeMaster
{
    [Collection("DatabaseCollection")]
    public sealed class MiscTypeMasterCommandRepositoryTests
    {
        private readonly DbFixture _fixture;
        public MiscTypeMasterCommandRepositoryTests(DbFixture fixture) => _fixture = fixture;

        private MiscTypeMasterCommandRepository CreateRepo(ApplicationDbContext ctx) => new(ctx);

        private static Domain.Entities.MiscTypeMaster BuildEntity(string code = "MTM_CMD1", string desc = "Type Cmd") =>
            new()
            {
                MiscTypeCode = code,
                Description = desc,
                IsActive = Status.Active,
                IsDeleted = IsDelete.NotDeleted
            };

        private async Task ClearAsync(ApplicationDbContext ctx) =>
            await _fixture.ClearAllTablesAsync();

        [Fact]
        public async Task CreateAsync_Should_Return_NewId()
        {
            await using var ctx = _fixture.CreateFreshDbContext();
            await ClearAsync(ctx);

            var id = await CreateRepo(ctx).CreateAsync(BuildEntity("MTM_C1"));

            id.Should().BeGreaterThan(0);
        }

        [Fact]
        public async Task CreateAsync_Should_Persist_Fields()
        {
            await using var ctx = _fixture.CreateFreshDbContext();
            await ClearAsync(ctx);

            var id = await CreateRepo(ctx).CreateAsync(BuildEntity("MTM_C2", "TestDesc"));
            ctx.ChangeTracker.Clear();
            var saved = await ctx.MiscTypeMaster.FirstAsync(x => x.Id == id);

            saved.MiscTypeCode.Should().Be("MTM_C2");
            saved.Description.Should().Be("TestDesc");
        }

        [Fact]
        public async Task UpdateAsync_Should_Persist_Changes()
        {
            await using var ctx = _fixture.CreateFreshDbContext();
            await ClearAsync(ctx);
            var id = await CreateRepo(ctx).CreateAsync(BuildEntity("MTM_U1", "Old"));
            ctx.ChangeTracker.Clear();

            var result = await CreateRepo(ctx).UpdateAsync(new Domain.Entities.MiscTypeMaster
            {
                Id = id,
                Description = "New",
                IsActive = Status.Inactive
            });
            ctx.ChangeTracker.Clear();

            result.Should().Be(id);
            var reloaded = await ctx.MiscTypeMaster.FirstAsync(x => x.Id == id);
            reloaded.Description.Should().Be("New");
            reloaded.IsActive.Should().Be(Status.Inactive);
        }

        [Fact]
        public async Task UpdateAsync_Should_Return_Zero_When_NotFound()
        {
            await using var ctx = _fixture.CreateFreshDbContext();

            var result = await CreateRepo(ctx).UpdateAsync(new Domain.Entities.MiscTypeMaster
            {
                Id = 9999999, Description = "ghost", IsActive = Status.Active
            });

            result.Should().Be(0);
        }

        [Fact]
        public async Task SoftDeleteAsync_Should_Return_True_When_Successful()
        {
            await using var ctx = _fixture.CreateFreshDbContext();
            await ClearAsync(ctx);
            var id = await CreateRepo(ctx).CreateAsync(BuildEntity("MTM_D1"));
            ctx.ChangeTracker.Clear();

            var result = await CreateRepo(ctx).SoftDeleteAsync(id, CancellationToken.None);

            result.Should().BeTrue();
        }

        [Fact]
        public async Task SoftDeleteAsync_Should_Flag_IsDeleted()
        {
            await using var ctx = _fixture.CreateFreshDbContext();
            await ClearAsync(ctx);
            var id = await CreateRepo(ctx).CreateAsync(BuildEntity("MTM_D2"));
            ctx.ChangeTracker.Clear();

            await CreateRepo(ctx).SoftDeleteAsync(id, CancellationToken.None);
            ctx.ChangeTracker.Clear();

            var reloaded = await ctx.MiscTypeMaster.FirstAsync(x => x.Id == id);
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
