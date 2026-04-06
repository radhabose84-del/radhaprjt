using Microsoft.EntityFrameworkCore;
using SalesManagement.Infrastructure.Data;
using SalesManagement.Infrastructure.Repositories.MovementTypeConfig;
using SalesManagement.Infrastructure.Repositories.StoTypeMaster;
using SalesManagement.IntegrationTests.Common;
using static SalesManagement.Domain.Common.BaseEntity;

namespace SalesManagement.IntegrationTests.Repositories.StoTypeMaster
{
    [Collection("DatabaseCollection")]
    public sealed class StoTypeMasterCommandRepositoryTests
    {
        private readonly DbFixture _fixture;

        public StoTypeMasterCommandRepositoryTests(DbFixture fixture)
        {
            _fixture = fixture;
        }

        private StoTypeMasterCommandRepository CreateRepository(ApplicationDbContext ctx)
            => new StoTypeMasterCommandRepository(ctx);

        private MovementTypeConfigCommandRepository CreateMtcRepo(ApplicationDbContext ctx)
            => new MovementTypeConfigCommandRepository(ctx);

        private Domain.Entities.StoTypeMaster BuildEntity(
            string code = "STO001",
            string name = "Test STO Type",
            int pgiMovTypeId = 0,
            int grMovTypeId = 0)
            => new Domain.Entities.StoTypeMaster
            {
                StoTypeCode = code,
                StoTypeName = name,
                Description = "Test Description",
                PgiMovementTypeId = pgiMovTypeId,
                GrMovementTypeId = grMovTypeId,
                IsActive = Status.Active,
                IsDeleted = IsDelete.NotDeleted
            };

        private Domain.Entities.MovementTypeConfig BuildMtcEntity(string code = "MTC_STO_01")
            => new Domain.Entities.MovementTypeConfig
            {
                MovementCode = code,
                MovementDescription = "Test MTC for STO",
                MovementCategoryId = 1,
                FromStockTypeId = 2,
                ToStockTypeId = 3,
                QuantityUpdateFlag = true,
                IsActive = Status.Active,
                IsDeleted = IsDelete.NotDeleted
            };

        private async Task ClearTableAsync(ApplicationDbContext ctx)
        {
            await ctx.Database.ExecuteSqlRawAsync("DELETE FROM Sales.StoTypeMaster");
        }

        private async Task<(int pgiId, int grId)> SeedMovementTypesAsync()
        {
            await using var ctx = _fixture.CreateFreshDbContext();
            var repo = CreateMtcRepo(ctx);
            var pgiId = await repo.CreateAsync(BuildMtcEntity("MTCPGI_STO"));
            var grId = await repo.CreateAsync(BuildMtcEntity("MTCGR_STO"));
            return (pgiId, grId);
        }

        // ── CreateAsync ──────────────────────────────────────────────────────

        [Fact]
        public async Task CreateAsync_Should_Return_NewId_GreaterThanZero()
        {
            await using var ctx = _fixture.CreateFreshDbContext();
            await ClearTableAsync(ctx);
            var (pgiId, grId) = await SeedMovementTypesAsync();

            var newId = await CreateRepository(ctx).CreateAsync(BuildEntity(pgiMovTypeId: pgiId, grMovTypeId: grId));

            newId.Should().BeGreaterThan(0);
        }

        [Fact]
        public async Task CreateAsync_Should_Persist_Fields_Correctly()
        {
            await using var ctx = _fixture.CreateFreshDbContext();
            await ClearTableAsync(ctx);
            var (pgiId, grId) = await SeedMovementTypesAsync();

            var newId = await CreateRepository(ctx).CreateAsync(
                BuildEntity("STO001", "Test STO Name", pgiId, grId));
            ctx.ChangeTracker.Clear();

            var saved = await ctx.StoTypeMaster.FirstOrDefaultAsync(x => x.Id == newId);

            saved.Should().NotBeNull();
            saved!.StoTypeCode.Should().Be("STO001");
            saved.StoTypeName.Should().Be("Test STO Name");
            saved.PgiMovementTypeId.Should().Be(pgiId);
            saved.GrMovementTypeId.Should().Be(grId);
            saved.IsActive.Should().Be(Status.Active);
            saved.IsDeleted.Should().Be(IsDelete.NotDeleted);
        }

        [Fact]
        public async Task CreateAsync_Should_Populate_Audit_Fields()
        {
            await using var ctx = _fixture.CreateFreshDbContext();
            await ClearTableAsync(ctx);
            var (pgiId, grId) = await SeedMovementTypesAsync();

            var newId = await CreateRepository(ctx).CreateAsync(BuildEntity(pgiMovTypeId: pgiId, grMovTypeId: grId));
            ctx.ChangeTracker.Clear();

            var saved = await ctx.StoTypeMaster.FirstOrDefaultAsync(x => x.Id == newId);

            saved!.CreatedBy.Should().Be(1);
            saved.CreatedByName.Should().Be("test-user");
            saved.CreatedIP.Should().Be("127.0.0.1");
            saved.CreatedDate.Should().NotBeNull();
        }

        // ── UpdateAsync ──────────────────────────────────────────────────────

        [Fact]
        public async Task UpdateAsync_Should_Persist_Mutable_Fields()
        {
            await using var ctx = _fixture.CreateFreshDbContext();
            await ClearTableAsync(ctx);
            var (pgiId, grId) = await SeedMovementTypesAsync();

            var repo = CreateRepository(ctx);
            var id = await repo.CreateAsync(BuildEntity("STO002", "Original Name", pgiId, grId));
            ctx.ChangeTracker.Clear();

            var updated = new Domain.Entities.StoTypeMaster
            {
                Id = id,
                StoTypeName = "Updated Name",
                Description = "Updated Desc",
                PgiMovementTypeId = grId,
                GrMovementTypeId = pgiId,
                IsActive = Status.Inactive
            };

            var resultId = await repo.UpdateAsync(updated);
            ctx.ChangeTracker.Clear();

            resultId.Should().Be(id);
            var saved = await ctx.StoTypeMaster.FirstOrDefaultAsync(x => x.Id == id);
            saved!.StoTypeName.Should().Be("Updated Name");
            saved.Description.Should().Be("Updated Desc");
            saved.IsActive.Should().Be(Status.Inactive);
        }

        [Fact]
        public async Task UpdateAsync_Should_Not_Change_StoTypeCode()
        {
            await using var ctx = _fixture.CreateFreshDbContext();
            await ClearTableAsync(ctx);
            var (pgiId, grId) = await SeedMovementTypesAsync();

            var repo = CreateRepository(ctx);
            var id = await repo.CreateAsync(BuildEntity("IMMUTABLE01", "Original Name", pgiId, grId));
            ctx.ChangeTracker.Clear();

            var updated = new Domain.Entities.StoTypeMaster
            {
                Id = id,
                StoTypeName = "Updated Name",
                Description = "Updated Desc",
                PgiMovementTypeId = pgiId,
                GrMovementTypeId = grId,
                IsActive = Status.Active
            };

            await repo.UpdateAsync(updated);
            ctx.ChangeTracker.Clear();

            var saved = await ctx.StoTypeMaster.FirstOrDefaultAsync(x => x.Id == id);
            saved!.StoTypeCode.Should().Be("IMMUTABLE01");
        }

        [Fact]
        public async Task UpdateAsync_Should_Return_Zero_WhenNotFound()
        {
            await using var ctx = _fixture.CreateFreshDbContext();
            await ClearTableAsync(ctx);

            var updated = new Domain.Entities.StoTypeMaster
            {
                Id = 99999,
                StoTypeName = "Ghost",
                PgiMovementTypeId = 1,
                GrMovementTypeId = 2,
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
            var (pgiId, grId) = await SeedMovementTypesAsync();

            var repo = CreateRepository(ctx);
            var id = await repo.CreateAsync(BuildEntity(pgiMovTypeId: pgiId, grMovTypeId: grId));
            ctx.ChangeTracker.Clear();

            var result = await repo.SoftDeleteAsync(id, CancellationToken.None);

            result.Should().BeTrue();
        }

        [Fact]
        public async Task SoftDeleteAsync_Should_Set_IsDeleted_Flag()
        {
            await using var ctx = _fixture.CreateFreshDbContext();
            await ClearTableAsync(ctx);
            var (pgiId, grId) = await SeedMovementTypesAsync();

            var repo = CreateRepository(ctx);
            var id = await repo.CreateAsync(BuildEntity(pgiMovTypeId: pgiId, grMovTypeId: grId));
            ctx.ChangeTracker.Clear();

            await repo.SoftDeleteAsync(id, CancellationToken.None);
            ctx.ChangeTracker.Clear();

            var saved = await ctx.StoTypeMaster
                .IgnoreQueryFilters()
                .FirstOrDefaultAsync(x => x.Id == id);

            saved.Should().NotBeNull();
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
