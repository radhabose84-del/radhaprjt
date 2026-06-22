using System.Data;
using Contracts.Interfaces.Lookups.Finance;
using Microsoft.EntityFrameworkCore;
using SalesManagement.Infrastructure.Data;
using SalesManagement.Infrastructure.Repositories.TripSheet;
using SalesManagement.IntegrationTests.Common;
using static SalesManagement.Domain.Common.BaseEntity;

namespace SalesManagement.IntegrationTests.Repositories.TripSheet
{
    [Collection("DatabaseCollection")]
    public sealed class TripSheetCommandRepositoryTests
    {
        private readonly DbFixture _fixture;
        public TripSheetCommandRepositoryTests(DbFixture fixture) => _fixture = fixture;

        // IncrementDocNoAsync is a no-op in tests (its DocumentSequence side effect is out of scope).
        private TripSheetCommandRepository CreateRepo(ApplicationDbContext ctx)
        {
            var docSeq = new Mock<IDocumentSequenceLookup>(MockBehavior.Loose);
            docSeq.Setup(d => d.IncrementDocNoAsync(It.IsAny<int>(), It.IsAny<IDbConnection>(), It.IsAny<IDbTransaction>()))
                  .Returns(Task.CompletedTask);
            return new TripSheetCommandRepository(ctx, docSeq.Object);
        }

        private static SalesManagement.Domain.Entities.TripSheetHeader BuildEntity(string tripSheetNo) =>
            new()
            {
                TripSheetNo = tripSheetNo,
                TripDate = new DateOnly(2026, 1, 15),
                VehicleNo = "KA01AB1234",
                UnitId = 1,
                Remarks = "desc",
                IsActive = Status.Active,
                IsDeleted = IsDelete.NotDeleted
            };

        private async Task ClearAsync() => await _fixture.ClearAllTablesAsync();

        [Fact]
        public async Task CreateAsync_Should_Return_NewId()
        {
            await ClearAsync();
            await using var ctx = _fixture.CreateFreshDbContext();

            var id = await CreateRepo(ctx).CreateAsync(BuildEntity("TS_C1"), 1);

            id.Should().BeGreaterThan(0);
        }

        [Fact]
        public async Task CreateAsync_Should_Persist_Fields()
        {
            await ClearAsync();
            await using var ctx = _fixture.CreateFreshDbContext();

            var id = await CreateRepo(ctx).CreateAsync(BuildEntity("TS_C2"), 1);
            ctx.ChangeTracker.Clear();

            var saved = await ctx.TripSheetHeader.FirstAsync(x => x.Id == id);
            saved.TripSheetNo.Should().Be("TS_C2");
            saved.VehicleNo.Should().Be("KA01AB1234");
            saved.UnitId.Should().Be(1);
        }

        [Fact]
        public async Task UpdateAsync_Should_Persist_Changes()
        {
            await ClearAsync();
            await using var ctx = _fixture.CreateFreshDbContext();
            var id = await CreateRepo(ctx).CreateAsync(BuildEntity("TS_U1"), 1);
            ctx.ChangeTracker.Clear();

            var entity = BuildEntity("TS_U1");
            entity.Id = id;
            entity.VehicleNo = "KA09ZZ9999";
            entity.Remarks = "updated";
            entity.IsActive = Status.Inactive;

            var result = await CreateRepo(ctx).UpdateAsync(entity, new List<SalesManagement.Domain.Entities.TripSheetDetail>());
            ctx.ChangeTracker.Clear();

            result.Should().Be(id);
            var reloaded = await ctx.TripSheetHeader.FirstAsync(x => x.Id == id);
            reloaded.VehicleNo.Should().Be("KA09ZZ9999");
            reloaded.Remarks.Should().Be("updated");
            reloaded.IsActive.Should().Be(Status.Inactive);
        }

        [Fact]
        public async Task UpdateAsync_Should_Return_Zero_When_NotFound()
        {
            await using var ctx = _fixture.CreateFreshDbContext();
            var entity = BuildEntity("TS_GHOST");
            entity.Id = 9999999;

            var result = await CreateRepo(ctx).UpdateAsync(entity, new List<SalesManagement.Domain.Entities.TripSheetDetail>());

            result.Should().Be(0);
        }

        [Fact]
        public async Task SoftDeleteAsync_Should_Return_True_And_Flag_Record()
        {
            await ClearAsync();
            await using var ctx = _fixture.CreateFreshDbContext();
            var id = await CreateRepo(ctx).CreateAsync(BuildEntity("TS_D1"), 1);
            ctx.ChangeTracker.Clear();

            var result = await CreateRepo(ctx).SoftDeleteAsync(id, CancellationToken.None);
            ctx.ChangeTracker.Clear();

            result.Should().BeTrue();
            var reloaded = await ctx.TripSheetHeader.FirstAsync(x => x.Id == id);
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
