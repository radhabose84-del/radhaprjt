using Contracts.Interfaces.Lookups.Finance;
using GateEntryManagement.Infrastructure.Repositories.GateInward;
using GateEntryManagement.Infrastructure.Data;

namespace GateEntryManagement.IntegrationTests.Repositories.GateInward
{
    [Collection("DatabaseCollection")]
    public sealed class GateInwardCommandRepositoryTests
    {
        private readonly DbFixture _fixture;

        public GateInwardCommandRepositoryTests(DbFixture fixture) => _fixture = fixture;

        private GateInwardCommandRepository CreateRepository(ApplicationDbContext ctx)
        {
            var mockDocSeq = new Mock<IDocumentSequenceLookup>(MockBehavior.Loose);
            return new GateInwardCommandRepository(ctx, mockDocSeq.Object);
        }

        private async Task<(int miscMasterId, int vmrId)> SeedPrerequisitesAsync(ApplicationDbContext ctx)
        {
            await _fixture.ClearAllTablesAsync();

            var miscType = new GateEntryManagement.Domain.Entities.MiscTypeMaster
            {
                MiscTypeCode = "VMRSTATUS",
                Description = "VMR Status",
                IsActive = Status.Active,
                IsDeleted = IsDelete.NotDeleted
            };
            await ctx.MiscTypeMaster.AddAsync(miscType);
            await ctx.SaveChangesAsync();

            var miscMaster = new GateEntryManagement.Domain.Entities.MiscMaster
            {
                MiscTypeId = miscType.Id,
                Code = "INSIDE",
                Description = "Inside Premises",
                SortOrder = 1,
                IsActive = Status.Active,
                IsDeleted = IsDelete.NotDeleted
            };
            await ctx.MiscMaster.AddAsync(miscMaster);
            await ctx.SaveChangesAsync();

            var vmr = new GateEntryManagement.Domain.Entities.VehicleMovementRecord
            {
                VehicleMovementId = "VMR00001",
                VehicleNumber = "KA01AB1234",
                DriverName = "John Doe",
                DriverMobileNo = "9876543210",
                PurposeOfVisitId = miscMaster.Id,
                StatusId = miscMaster.Id,
                GateInTime = DateTimeOffset.UtcNow,
                GateInBy = "test-user",
                UnitId = 1,
                IsActive = Status.Active,
                IsDeleted = IsDelete.NotDeleted
            };
            await ctx.VehicleMovementRecord.AddAsync(vmr);
            await ctx.SaveChangesAsync();

            return (miscMaster.Id, vmr.Id);
        }

        private async Task<int> SeedGateInwardAsync(ApplicationDbContext ctx, int vmrId)
        {
            var entity = new GateEntryManagement.Domain.Entities.GateInwardHdr
            {
                GateEntryNo = "GE00001",
                VehicleMovementRecordId = vmrId,
                GrossWeight = 1000,
                TareWeight = 200,
                NetWeight = 800,
                QAInspectionRequired = false,
                UnitId = 1,
                IsActive = Status.Active,
                IsDeleted = IsDelete.NotDeleted
            };
            await ctx.GateInwardHdr.AddAsync(entity);
            await ctx.SaveChangesAsync();
            return entity.Id;
        }

        // --- SOFT DELETE ---

        [Fact]
        public async Task SoftDeleteAsync_Should_Return_True_When_Successful()
        {
            await using var ctx = _fixture.CreateFreshDbContext();
            var (_, vmrId) = await SeedPrerequisitesAsync(ctx);
            var id = await SeedGateInwardAsync(ctx, vmrId);
            ctx.ChangeTracker.Clear();

            var result = await CreateRepository(ctx).SoftDeleteAsync(id, CancellationToken.None);

            result.Should().BeTrue();
        }

        [Fact]
        public async Task SoftDeleteAsync_Should_Set_IsDeleted_Flag()
        {
            await using var ctx = _fixture.CreateFreshDbContext();
            var (_, vmrId) = await SeedPrerequisitesAsync(ctx);
            var id = await SeedGateInwardAsync(ctx, vmrId);
            ctx.ChangeTracker.Clear();

            await CreateRepository(ctx).SoftDeleteAsync(id, CancellationToken.None);
            ctx.ChangeTracker.Clear();

            var deleted = await ctx.GateInwardHdr
                .IgnoreQueryFilters()
                .FirstOrDefaultAsync(x => x.Id == id);

            deleted.Should().NotBeNull();
            deleted!.IsDeleted.Should().Be(IsDelete.Deleted);
        }

        [Fact]
        public async Task SoftDeleteAsync_Should_Return_False_When_NotFound()
        {
            await using var ctx = _fixture.CreateFreshDbContext();
            var (_, _) = await SeedPrerequisitesAsync(ctx);

            var result = await CreateRepository(ctx).SoftDeleteAsync(9999, CancellationToken.None);

            result.Should().BeFalse();
        }
    }
}
