using Contracts.Interfaces;
using Dapper;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using FinanceManagement.Infrastructure.Data;
using FinanceManagement.IntegrationTests.Common;
using FinanceManagement.Infrastructure.Repositories.TransactionTypeMaster;
using static FinanceManagement.Domain.Common.BaseEntity;

namespace FinanceManagement.IntegrationTests.Repositories.TransactionTypeMaster
{
    [Collection("DatabaseCollection")]
    public sealed class TransactionTypeMasterCommandRepositoryTests
    {
        private readonly DbFixture _fixture;

        public TransactionTypeMasterCommandRepositoryTests(DbFixture fixture)
        {
            _fixture = fixture;
        }

        private TransactionTypeMasterCommandRepository CreateRepository(ApplicationDbContext ctx, int? tokenUnitId = 1)
        {
            var ip = new Mock<IIPAddressService>(MockBehavior.Loose);
            ip.Setup(x => x.GetUnitId()).Returns(tokenUnitId);
            return new TransactionTypeMasterCommandRepository(ctx, ip.Object);
        }

        private static Domain.Entities.TransactionTypeMaster BuildEntity(
            string typeName = "Invoice",
            string shortName = "INV",
            string description = "Invoice Type",
            int unitId = 1,
            int moduleId = 1,
            int menuId = 1) =>
            new()
            {
                UnitId = unitId,
                ModuleId = moduleId,
                MenuId = menuId,
                TypeName = typeName,
                ShortName = shortName,
                Description = description,
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

            var newId = await CreateRepository(ctx).CreateAsync(BuildEntity());

            newId.Should().BeGreaterThan(0);
        }

        [Fact]
        public async Task CreateAsync_Should_Persist_Fields_Correctly()
        {
            await using var ctx = _fixture.CreateFreshDbContext();
            await ClearTableAsync(ctx);

            var newId = await CreateRepository(ctx).CreateAsync(BuildEntity("Invoice", "INV", "Invoice Type", 1, 2, 3));
            ctx.ChangeTracker.Clear();

            var saved = await ctx.TransactionTypeMaster.FirstOrDefaultAsync(x => x.Id == newId);

            saved.Should().NotBeNull();
            saved!.TypeName.Should().Be("Invoice");
            saved.ShortName.Should().Be("INV");
            saved.Description.Should().Be("Invoice Type");
            saved.UnitId.Should().Be(1);
            saved.ModuleId.Should().Be(2);
            saved.MenuId.Should().Be(3);
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

            var saved = await ctx.TransactionTypeMaster.FirstOrDefaultAsync(x => x.Id == newId);

            saved.Should().NotBeNull();
            saved!.CreatedBy.Should().Be(1);
            saved.CreatedByName.Should().Be("test-user");
            saved.CreatedIP.Should().Be("127.0.0.1");
            saved.CreatedDate.Should().NotBeNull();
        }

        // --- UPDATE ---

        [Fact]
        public async Task UpdateAsync_Should_Persist_Changes()
        {
            await using var ctx = _fixture.CreateFreshDbContext();
            await ClearTableAsync(ctx);
            var id = await CreateRepository(ctx).CreateAsync(BuildEntity());
            ctx.ChangeTracker.Clear();

            var entity = await ctx.TransactionTypeMaster.FirstAsync(x => x.Id == id);
            entity.TypeName = "Updated Type";
            entity.ShortName = "UPD";
            entity.Description = "Updated Description";
            ctx.ChangeTracker.Clear();

            var result = await CreateRepository(ctx).UpdateAsync(entity);
            ctx.ChangeTracker.Clear();

            result.Should().Be(id);
            var updated = await ctx.TransactionTypeMaster.FirstAsync(x => x.Id == id);
            updated.TypeName.Should().Be("Updated Type");
            updated.ShortName.Should().Be("UPD");
            updated.Description.Should().Be("Updated Description");
        }

        [Fact]
        public async Task UpdateAsync_Should_Return_Zero_When_NotFound()
        {
            await using var ctx = _fixture.CreateFreshDbContext();
            await ClearTableAsync(ctx);

            var entity = BuildEntity();
            entity.Id = 9999;

            var result = await CreateRepository(ctx).UpdateAsync(entity);

            result.Should().Be(0);
        }

        [Fact]
        public async Task UpdateAsync_Should_Return_Zero_When_DifferentUnit()
        {
            await using var ctx = _fixture.CreateFreshDbContext();
            await ClearTableAsync(ctx);
            var id = await CreateRepository(ctx).CreateAsync(BuildEntity(unitId: 2));
            ctx.ChangeTracker.Clear();

            var entity = BuildEntity(unitId: 2);
            entity.Id = id;
            entity.TypeName = "Hacked";

            var result = await CreateRepository(ctx, tokenUnitId: 1).UpdateAsync(entity);

            result.Should().Be(0);
        }

        [Fact]
        public async Task UpdateAsync_Should_Preserve_UnitId()
        {
            await using var ctx = _fixture.CreateFreshDbContext();
            await ClearTableAsync(ctx);
            var id = await CreateRepository(ctx).CreateAsync(BuildEntity(unitId: 1));
            ctx.ChangeTracker.Clear();

            // Attacker attempts to change UnitId via payload
            var entity = BuildEntity(unitId: 99);
            entity.Id = id;
            entity.TypeName = "Modified";

            await CreateRepository(ctx).UpdateAsync(entity);
            ctx.ChangeTracker.Clear();

            var reloaded = await ctx.TransactionTypeMaster.FirstAsync(x => x.Id == id);
            reloaded.UnitId.Should().Be(1);
            reloaded.TypeName.Should().Be("Modified");
        }

        // --- SOFT DELETE ---

        [Fact]
        public async Task SoftDeleteAsync_Should_Return_True_When_Successful()
        {
            await using var ctx = _fixture.CreateFreshDbContext();
            await ClearTableAsync(ctx);
            var id = await CreateRepository(ctx).CreateAsync(BuildEntity());
            ctx.ChangeTracker.Clear();

            var result = await CreateRepository(ctx).SoftDeleteAsync(id, CancellationToken.None);

            result.Should().BeTrue();
        }

        [Fact]
        public async Task SoftDeleteAsync_Should_Set_IsDeleted_Flag()
        {
            await using var ctx = _fixture.CreateFreshDbContext();
            await ClearTableAsync(ctx);
            var id = await CreateRepository(ctx).CreateAsync(BuildEntity());
            ctx.ChangeTracker.Clear();

            await CreateRepository(ctx).SoftDeleteAsync(id, CancellationToken.None);
            ctx.ChangeTracker.Clear();

            var deleted = await ctx.TransactionTypeMaster
                .IgnoreQueryFilters()
                .FirstOrDefaultAsync(x => x.Id == id);

            deleted.Should().NotBeNull();
            deleted!.IsDeleted.Should().Be(IsDelete.Deleted);
        }

        [Fact]
        public async Task SoftDeleteAsync_Should_Return_False_When_NotFound()
        {
            await using var ctx = _fixture.CreateFreshDbContext();
            await ClearTableAsync(ctx);

            var result = await CreateRepository(ctx).SoftDeleteAsync(9999, CancellationToken.None);

            result.Should().BeFalse();
        }

        [Fact]
        public async Task SoftDeleteAsync_Should_Return_False_When_DifferentUnit()
        {
            await using var ctx = _fixture.CreateFreshDbContext();
            await ClearTableAsync(ctx);
            var id = await CreateRepository(ctx).CreateAsync(BuildEntity(unitId: 2));
            ctx.ChangeTracker.Clear();

            var result = await CreateRepository(ctx, tokenUnitId: 1)
                .SoftDeleteAsync(id, CancellationToken.None);

            result.Should().BeFalse();
        }
    }
}
