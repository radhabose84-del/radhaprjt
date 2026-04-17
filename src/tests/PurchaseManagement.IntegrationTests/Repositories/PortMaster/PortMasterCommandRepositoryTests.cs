using Microsoft.EntityFrameworkCore;
using PurchaseManagement.Domain.Entities;
using PurchaseManagement.Infrastructure.Repositories.Port;
using PurchaseManagement.IntegrationTests.Common;
using static PurchaseManagement.Domain.Common.BaseEntity;

namespace PurchaseManagement.IntegrationTests.Repositories.PortMaster
{
    [Collection("DatabaseCollection")]
    public sealed class PortMasterCommandRepositoryTests
    {
        private readonly DbFixture _fixture;

        public PortMasterCommandRepositoryTests(DbFixture fixture)
        {
            _fixture = fixture;
        }

        private PortMasterCommandRepository CreateRepository(PurchaseManagement.Infrastructure.Data.ApplicationDbContext ctx) =>
            new(ctx, _fixture.IpMock.Object);

        private static PurchaseManagement.Domain.Entities.PortMaster BuildEntity(
            string portCode = "PORT001",
            string portName = "Test Port",
            int countryId = 1) =>
            new()
            {
                PortCode = portCode,
                PortName = portName,
                CountryId = countryId,
                TypeId = null,       // nullable FK — no seeding needed
                PortTypeId = null,   // nullable FK — no seeding needed
                IsActive = Status.Active,
                IsDeleted = IsDelete.NotDeleted
            };

        private async Task ClearTableAsync(PurchaseManagement.Infrastructure.Data.ApplicationDbContext ctx) =>
            await _fixture.ClearAllTablesAsync();

        // --- CREATE ---

        [Fact]
        public async Task CreateAsync_Should_Return_Entity_With_Id_GreaterThanZero()
        {
            await using var ctx = _fixture.CreateFreshDbContext();
            await ClearTableAsync(ctx);

            var entity = BuildEntity();
            var result = await CreateRepository(ctx).CreateAsync(entity, CancellationToken.None);

            result.Id.Should().BeGreaterThan(0);
        }

        [Fact]
        public async Task CreateAsync_Should_Persist_Fields_Correctly()
        {
            await using var ctx = _fixture.CreateFreshDbContext();
            await ClearTableAsync(ctx);

            var entity = BuildEntity("PORT002", "Mumbai Port", 5);
            var result = await CreateRepository(ctx).CreateAsync(entity, CancellationToken.None);
            ctx.ChangeTracker.Clear();

            var saved = await ctx.Set<PurchaseManagement.Domain.Entities.PortMaster>()
                .FirstOrDefaultAsync(x => x.Id == result.Id);

            saved!.PortCode.Should().Be("PORT002");
            saved.PortName.Should().Be("Mumbai Port");
            saved.CountryId.Should().Be(5);
            saved.IsActive.Should().Be(Status.Active);
            saved.IsDeleted.Should().Be(IsDelete.NotDeleted);
        }

        [Fact]
        public async Task CreateAsync_Should_Trim_PortCode_And_PortName()
        {
            await using var ctx = _fixture.CreateFreshDbContext();
            await ClearTableAsync(ctx);

            var entity = BuildEntity("  PORT003  ", "  Chennai Port  ");
            var result = await CreateRepository(ctx).CreateAsync(entity, CancellationToken.None);
            ctx.ChangeTracker.Clear();

            var saved = await ctx.Set<PurchaseManagement.Domain.Entities.PortMaster>()
                .FirstOrDefaultAsync(x => x.Id == result.Id);

            saved!.PortCode.Should().Be("PORT003");
            saved.PortName.Should().Be("Chennai Port");
        }

        // --- UPDATE ---

        [Fact]
        public async Task UpdateAsync_Should_Persist_Changes()
        {
            await using var ctx = _fixture.CreateFreshDbContext();
            await ClearTableAsync(ctx);

            var entity = BuildEntity("PORT004", "Old Name");
            var created = await CreateRepository(ctx).CreateAsync(entity, CancellationToken.None);
            ctx.ChangeTracker.Clear();

            var toUpdate = new PurchaseManagement.Domain.Entities.PortMaster
            {
                Id = created.Id,
                PortCode = "PORT004",
                PortName = "New Name",
                CountryId = 1,
                IsActive = Status.Active,
                IsDeleted = IsDelete.NotDeleted
            };

            await CreateRepository(ctx).UpdateAsync(toUpdate, CancellationToken.None);
            ctx.ChangeTracker.Clear();

            var saved = await ctx.Set<PurchaseManagement.Domain.Entities.PortMaster>()
                .FirstOrDefaultAsync(x => x.Id == created.Id);

            saved!.PortName.Should().Be("New Name");
        }

        [Fact]
        public async Task UpdateAsync_Should_Throw_When_NotFound()
        {
            await using var ctx = _fixture.CreateFreshDbContext();
            await ClearTableAsync(ctx);

            var nonExistent = new PurchaseManagement.Domain.Entities.PortMaster
            {
                Id = 9999,
                PortCode = "NOPORT",
                PortName = "Non-Existent"
            };

            Func<Task> act = async () => await CreateRepository(ctx).UpdateAsync(nonExistent, CancellationToken.None);

            await act.Should().ThrowAsync<KeyNotFoundException>();
        }

        // --- SOFT DELETE ---

        [Fact]
        public async Task SoftDeleteAsync_Should_Return_True_When_Successful()
        {
            await using var ctx = _fixture.CreateFreshDbContext();
            await ClearTableAsync(ctx);
            var created = await CreateRepository(ctx).CreateAsync(BuildEntity("PORT005"), CancellationToken.None);
            ctx.ChangeTracker.Clear();

            var result = await CreateRepository(ctx).SoftDeleteAsync(created.Id, CancellationToken.None);

            result.Should().BeTrue();
        }

        [Fact]
        public async Task SoftDeleteAsync_Should_Set_IsDeleted_Flag()
        {
            await using var ctx = _fixture.CreateFreshDbContext();
            await ClearTableAsync(ctx);
            var created = await CreateRepository(ctx).CreateAsync(BuildEntity("PORT006"), CancellationToken.None);
            ctx.ChangeTracker.Clear();

            await CreateRepository(ctx).SoftDeleteAsync(created.Id, CancellationToken.None);
            ctx.ChangeTracker.Clear();

            var deleted = await ctx.Set<PurchaseManagement.Domain.Entities.PortMaster>()
                .IgnoreQueryFilters()
                .FirstOrDefaultAsync(x => x.Id == created.Id);

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
    }
}
