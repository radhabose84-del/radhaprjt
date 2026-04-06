using Microsoft.EntityFrameworkCore;
using SalesManagement.Infrastructure.Data;
using SalesManagement.Infrastructure.Repositories.MovementTypeConfig;
using SalesManagement.IntegrationTests.Common;
using static SalesManagement.Domain.Common.BaseEntity;

namespace SalesManagement.IntegrationTests.Repositories.MovementTypeConfig
{
    [Collection("DatabaseCollection")]
    public sealed class MovementTypeConfigCommandRepositoryTests
    {
        private readonly DbFixture _fixture;

        public MovementTypeConfigCommandRepositoryTests(DbFixture fixture)
        {
            _fixture = fixture;
        }

        private MovementTypeConfigCommandRepository CreateRepository(ApplicationDbContext ctx)
            => new MovementTypeConfigCommandRepository(ctx);

        private Domain.Entities.MovementTypeConfig BuildEntity(
            string code = "MTC001",
            string description = "Test Movement Config",
            int movementCategoryId = 1,
            int fromStockTypeId = 2,
            int toStockTypeId = 3)
            => new Domain.Entities.MovementTypeConfig
            {
                MovementCode = code,
                MovementDescription = description,
                MovementCategoryId = movementCategoryId,
                FromStockTypeId = fromStockTypeId,
                ToStockTypeId = toStockTypeId,
                QuantityUpdateFlag = true,
                ValueUpdateFlag = false,
                AccountModifier = "ACC01",
                BatchRequiredFlag = false,
                NegativeStockAllowed = false,
                IsActive = Status.Active,
                IsDeleted = IsDelete.NotDeleted
            };

        private async Task ClearTableAsync(ApplicationDbContext ctx)
        {
            await ctx.Database.ExecuteSqlRawAsync("DELETE FROM Sales.StoTypeMaster");
            await ctx.Database.ExecuteSqlRawAsync("DELETE FROM Sales.MovementTypeConfig");
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

            var newId = await CreateRepository(ctx).CreateAsync(BuildEntity("MTC001", "Sales Config", 1, 2, 3));
            ctx.ChangeTracker.Clear();

            var saved = await ctx.MovementTypeConfig.FirstOrDefaultAsync(x => x.Id == newId);

            saved.Should().NotBeNull();
            saved!.MovementCode.Should().Be("MTC001");
            saved.MovementDescription.Should().Be("Sales Config");
            saved.MovementCategoryId.Should().Be(1);
            saved.FromStockTypeId.Should().Be(2);
            saved.ToStockTypeId.Should().Be(3);
            saved.QuantityUpdateFlag.Should().BeTrue();
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

            var saved = await ctx.MovementTypeConfig.FirstOrDefaultAsync(x => x.Id == newId);

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

            var repo = CreateRepository(ctx);
            var id = await repo.CreateAsync(BuildEntity("MTC002", "Original"));
            ctx.ChangeTracker.Clear();

            var updated = new Domain.Entities.MovementTypeConfig
            {
                Id = id,
                MovementDescription = "Updated Description",
                MovementCategoryId = 5,
                FromStockTypeId = 6,
                ToStockTypeId = 7,
                QuantityUpdateFlag = false,
                ValueUpdateFlag = true,
                AccountModifier = "ACC99",
                BatchRequiredFlag = true,
                NegativeStockAllowed = true,
                IsActive = Status.Inactive
            };

            var resultId = await repo.UpdateAsync(updated);
            ctx.ChangeTracker.Clear();

            resultId.Should().Be(id);
            var saved = await ctx.MovementTypeConfig.FirstOrDefaultAsync(x => x.Id == id);
            saved!.MovementDescription.Should().Be("Updated Description");
            saved.MovementCategoryId.Should().Be(5);
            saved.IsActive.Should().Be(Status.Inactive);
            saved.QuantityUpdateFlag.Should().BeFalse();
            saved.ValueUpdateFlag.Should().BeTrue();
        }

        [Fact]
        public async Task UpdateAsync_Should_Not_Change_MovementCode()
        {
            await using var ctx = _fixture.CreateFreshDbContext();
            await ClearTableAsync(ctx);

            var repo = CreateRepository(ctx);
            var id = await repo.CreateAsync(BuildEntity("IMMUTABLE01"));
            ctx.ChangeTracker.Clear();

            var updated = new Domain.Entities.MovementTypeConfig
            {
                Id = id,
                MovementDescription = "Changed Desc",
                MovementCategoryId = 1,
                FromStockTypeId = 2,
                ToStockTypeId = 3,
                IsActive = Status.Active
            };

            await repo.UpdateAsync(updated);
            ctx.ChangeTracker.Clear();

            var saved = await ctx.MovementTypeConfig.FirstOrDefaultAsync(x => x.Id == id);
            saved!.MovementCode.Should().Be("IMMUTABLE01");
        }

        [Fact]
        public async Task UpdateAsync_Should_Return_Zero_WhenNotFound()
        {
            await using var ctx = _fixture.CreateFreshDbContext();
            await ClearTableAsync(ctx);

            var updated = new Domain.Entities.MovementTypeConfig
            {
                Id = 99999,
                MovementDescription = "Ghost",
                MovementCategoryId = 1,
                FromStockTypeId = 2,
                ToStockTypeId = 3,
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

            var saved = await ctx.MovementTypeConfig
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

        [Fact]
        public async Task SoftDeleteAsync_Should_Return_False_WhenAlreadyDeleted()
        {
            await using var ctx = _fixture.CreateFreshDbContext();
            await ClearTableAsync(ctx);

            var repo = CreateRepository(ctx);
            var id = await repo.CreateAsync(BuildEntity());
            ctx.ChangeTracker.Clear();

            await repo.SoftDeleteAsync(id, CancellationToken.None);
            ctx.ChangeTracker.Clear();

            var result = await repo.SoftDeleteAsync(id, CancellationToken.None);

            result.Should().BeFalse();
        }
    }
}
