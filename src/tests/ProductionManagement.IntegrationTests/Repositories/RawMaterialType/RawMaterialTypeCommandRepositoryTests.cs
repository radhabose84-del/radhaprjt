using Microsoft.EntityFrameworkCore;
using ProductionManagement.Infrastructure.Data;
using ProductionManagement.Infrastructure.Repositories.RawMaterialType;
using ProductionManagement.IntegrationTests.Common;
using static ProductionManagement.Domain.Common.BaseEntity;

namespace ProductionManagement.IntegrationTests.Repositories.RawMaterialType
{
    [Collection("DatabaseCollection")]
    public sealed class RawMaterialTypeCommandRepositoryTests
    {
        private readonly DbFixture _fixture;

        public RawMaterialTypeCommandRepositoryTests(DbFixture fixture) => _fixture = fixture;

        private RawMaterialTypeCommandRepository CreateRepo(ApplicationDbContext ctx) => new(ctx);

        private static Domain.Entities.RawMaterialType BuildEntity(
            string code = "RMT001", string name = "Cotton", string? desc = "Cotton raw material") =>
            new()
            {
                RawMaterialTypeCode = code,
                RawMaterialTypeName = name,
                Description = desc,
                EffectiveFrom = DateTimeOffset.UtcNow,
                IsActive = Status.Active,
                IsDeleted = IsDelete.NotDeleted
            };

        private async Task ClearTableAsync(ApplicationDbContext ctx) =>
            await _fixture.ClearAllTablesAsync();

        // --- CREATE ---

        [Fact]
        public async Task CreateAsync_Should_Return_NewId_GreaterThanZero()
        {
            await using var ctx = _fixture.CreateFreshDbContext();
            await ClearTableAsync(ctx);

            var id = await CreateRepo(ctx).CreateAsync(BuildEntity("RMC1"));

            id.Should().BeGreaterThan(0);
        }

        [Fact]
        public async Task CreateAsync_Should_Persist_Fields()
        {
            await using var ctx = _fixture.CreateFreshDbContext();
            await ClearTableAsync(ctx);

            var id = await CreateRepo(ctx).CreateAsync(BuildEntity("RMC2", "Wool", "Wool desc"));
            ctx.ChangeTracker.Clear();
            var saved = await ctx.RawMaterialType.FirstAsync(x => x.Id == id);

            saved.RawMaterialTypeCode.Should().Be("RMC2");
            saved.RawMaterialTypeName.Should().Be("Wool");
            saved.Description.Should().Be("Wool desc");
        }

        [Fact]
        public async Task CreateAsync_Should_AutoPopulate_Audit_Fields()
        {
            await using var ctx = _fixture.CreateFreshDbContext();
            await ClearTableAsync(ctx);

            var id = await CreateRepo(ctx).CreateAsync(BuildEntity("RMC3"));
            ctx.ChangeTracker.Clear();
            var saved = await ctx.RawMaterialType.FirstAsync(x => x.Id == id);

            saved.CreatedBy.Should().Be(1);
            saved.CreatedByName.Should().Be("test-user");
            saved.CreatedIP.Should().Be("127.0.0.1");
        }

        // --- UPDATE ---

        [Fact]
        public async Task UpdateAsync_Should_Persist_Changes()
        {
            await using var ctx = _fixture.CreateFreshDbContext();
            await ClearTableAsync(ctx);
            var id = await CreateRepo(ctx).CreateAsync(BuildEntity("RMU1", "Old"));
            ctx.ChangeTracker.Clear();

            var entity = new Domain.Entities.RawMaterialType
            {
                Id = id,
                RawMaterialTypeName = "New",
                Description = "Updated",
                EffectiveFrom = DateTimeOffset.UtcNow,
                IsActive = Status.Active
            };
            var result = await CreateRepo(ctx).UpdateAsync(entity);
            ctx.ChangeTracker.Clear();

            result.Should().Be(id);
            var reloaded = await ctx.RawMaterialType.FirstAsync(x => x.Id == id);
            reloaded.RawMaterialTypeName.Should().Be("New");
            reloaded.Description.Should().Be("Updated");
        }

        [Fact]
        public async Task UpdateAsync_Should_Not_Modify_Code()
        {
            await using var ctx = _fixture.CreateFreshDbContext();
            await ClearTableAsync(ctx);
            var id = await CreateRepo(ctx).CreateAsync(BuildEntity("RMI1", "Orig"));
            ctx.ChangeTracker.Clear();

            await CreateRepo(ctx).UpdateAsync(new Domain.Entities.RawMaterialType
            {
                Id = id,
                RawMaterialTypeCode = "CHANGED",
                RawMaterialTypeName = "AnyName",
                EffectiveFrom = DateTimeOffset.UtcNow,
                IsActive = Status.Active
            });
            ctx.ChangeTracker.Clear();

            var reloaded = await ctx.RawMaterialType.FirstAsync(x => x.Id == id);
            reloaded.RawMaterialTypeCode.Should().Be("RMI1");
        }

        [Fact]
        public async Task UpdateAsync_Should_Return_Zero_When_NotFound()
        {
            await using var ctx = _fixture.CreateFreshDbContext();

            var result = await CreateRepo(ctx).UpdateAsync(new Domain.Entities.RawMaterialType
            {
                Id = 9999999,
                RawMaterialTypeName = "ghost",
                EffectiveFrom = DateTimeOffset.UtcNow,
                IsActive = Status.Active
            });

            result.Should().Be(0);
        }

        [Fact]
        public async Task UpdateAsync_Should_Return_Zero_When_SoftDeleted()
        {
            await using var ctx = _fixture.CreateFreshDbContext();
            await ClearTableAsync(ctx);
            var id = await CreateRepo(ctx).CreateAsync(BuildEntity("RMSD"));
            await CreateRepo(ctx).SoftDeleteAsync(id, CancellationToken.None);
            ctx.ChangeTracker.Clear();

            var result = await CreateRepo(ctx).UpdateAsync(new Domain.Entities.RawMaterialType
            {
                Id = id,
                RawMaterialTypeName = "After delete",
                EffectiveFrom = DateTimeOffset.UtcNow,
                IsActive = Status.Active
            });

            result.Should().Be(0);
        }

        // --- SOFT DELETE ---

        [Fact]
        public async Task SoftDeleteAsync_Should_Return_True_When_Successful()
        {
            await using var ctx = _fixture.CreateFreshDbContext();
            await ClearTableAsync(ctx);
            var id = await CreateRepo(ctx).CreateAsync(BuildEntity("RMD1"));
            ctx.ChangeTracker.Clear();

            var result = await CreateRepo(ctx).SoftDeleteAsync(id, CancellationToken.None);

            result.Should().BeTrue();
        }

        [Fact]
        public async Task SoftDeleteAsync_Should_Flag_Record()
        {
            await using var ctx = _fixture.CreateFreshDbContext();
            await ClearTableAsync(ctx);
            var id = await CreateRepo(ctx).CreateAsync(BuildEntity("RMD2"));
            ctx.ChangeTracker.Clear();

            await CreateRepo(ctx).SoftDeleteAsync(id, CancellationToken.None);
            ctx.ChangeTracker.Clear();

            var reloaded = await ctx.RawMaterialType.FirstAsync(x => x.Id == id);
            reloaded.IsDeleted.Should().Be(IsDelete.Deleted);
        }

        [Fact]
        public async Task SoftDeleteAsync_Should_Return_False_When_NotFound()
        {
            await using var ctx = _fixture.CreateFreshDbContext();

            var result = await CreateRepo(ctx).SoftDeleteAsync(9999999, CancellationToken.None);

            result.Should().BeFalse();
        }

        [Fact]
        public async Task SoftDeleteAsync_Should_Return_False_When_AlreadyDeleted()
        {
            await using var ctx = _fixture.CreateFreshDbContext();
            await ClearTableAsync(ctx);
            var id = await CreateRepo(ctx).CreateAsync(BuildEntity("RMDD"));
            await CreateRepo(ctx).SoftDeleteAsync(id, CancellationToken.None);
            ctx.ChangeTracker.Clear();

            var result = await CreateRepo(ctx).SoftDeleteAsync(id, CancellationToken.None);

            result.Should().BeFalse();
        }
    }
}
