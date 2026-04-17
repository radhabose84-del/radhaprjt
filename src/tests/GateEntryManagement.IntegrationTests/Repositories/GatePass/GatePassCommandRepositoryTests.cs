using Contracts.Interfaces;
using Contracts.Interfaces.Lookups.Finance;
using GateEntryManagement.Infrastructure.Repositories.GatePass;
using GateEntryManagement.Infrastructure.Data;

namespace GateEntryManagement.IntegrationTests.Repositories.GatePass
{
    [Collection("DatabaseCollection")]
    public sealed class GatePassCommandRepositoryTests
    {
        private readonly DbFixture _fixture;

        public GatePassCommandRepositoryTests(DbFixture fixture) => _fixture = fixture;

        private GatePassCommandRepository CreateRepository(ApplicationDbContext ctx)
        {
            var mockDocSeq = new Mock<IDocumentSequenceLookup>(MockBehavior.Loose);
            var mockDocProcessors = new List<IGatePassDocumentProcessor>();
            var mockIpService = new Mock<IIPAddressService>(MockBehavior.Loose);
            mockIpService.Setup(s => s.GetUnitId()).Returns(1);
            mockIpService.Setup(s => s.GetUserName()).Returns("test-user");
            return new GatePassCommandRepository(ctx, mockDocSeq.Object, mockDocProcessors, mockIpService.Object);
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

        private async Task<int> SeedGatePassAsync(ApplicationDbContext ctx, int vmrId,
            string gatePassNo = "GP00001")
        {
            var entity = new GateEntryManagement.Domain.Entities.GatePassHdr
            {
                GatePassNo = gatePassNo,
                GatePassDate = DateOnly.FromDateTime(DateTime.Today),
                VehicleMovementRecordId = vmrId,
                VehicleNumber = "KA01AB1234",
                DriverName = "John Doe",
                DriverMobile = "9876543210",
                TransporterName = "Test Transporter",
                UnitId = 1,
                TotalItems = 5,
                TotalDocumentQty = 100,
                TotalDispatchQty = 100,
                TotalValue = 50000,
                IsActive = Status.Active,
                IsDeleted = IsDelete.NotDeleted
            };
            await ctx.GatePassHdr.AddAsync(entity);
            await ctx.SaveChangesAsync();
            return entity.Id;
        }

        // --- SOFT DELETE ---

        [Fact]
        public async Task SoftDeleteAsync_Should_Return_True_When_Successful()
        {
            await using var ctx = _fixture.CreateFreshDbContext();
            var (miscMasterId, vmrId) = await SeedPrerequisitesAsync(ctx);
            var id = await SeedGatePassAsync(ctx, vmrId);
            ctx.ChangeTracker.Clear();

            var result = await CreateRepository(ctx).SoftDeleteAsync(id, miscMasterId, CancellationToken.None);

            result.Should().BeTrue();
        }

        [Fact]
        public async Task SoftDeleteAsync_Should_Set_IsDeleted_Flag()
        {
            await using var ctx = _fixture.CreateFreshDbContext();
            var (miscMasterId, vmrId) = await SeedPrerequisitesAsync(ctx);
            var id = await SeedGatePassAsync(ctx, vmrId);
            ctx.ChangeTracker.Clear();

            await CreateRepository(ctx).SoftDeleteAsync(id, miscMasterId, CancellationToken.None);
            ctx.ChangeTracker.Clear();

            var deleted = await ctx.GatePassHdr
                .IgnoreQueryFilters()
                .FirstOrDefaultAsync(x => x.Id == id);

            deleted.Should().NotBeNull();
            deleted!.IsDeleted.Should().Be(IsDelete.Deleted);
        }

        [Fact]
        public async Task SoftDeleteAsync_Should_Revert_VMR_Status()
        {
            await using var ctx = _fixture.CreateFreshDbContext();
            var (miscMasterId, vmrId) = await SeedPrerequisitesAsync(ctx);
            var id = await SeedGatePassAsync(ctx, vmrId);
            ctx.ChangeTracker.Clear();

            await CreateRepository(ctx).SoftDeleteAsync(id, miscMasterId, CancellationToken.None);
            ctx.ChangeTracker.Clear();

            var vmr = await ctx.VehicleMovementRecord.FirstOrDefaultAsync(x => x.Id == vmrId);

            vmr.Should().NotBeNull();
            vmr!.StatusId.Should().Be(miscMasterId);
            vmr.GateOutTime.Should().BeNull();
            vmr.GateOutBy.Should().BeNull();
        }

        [Fact]
        public async Task SoftDeleteAsync_Should_Return_False_When_NotFound()
        {
            await using var ctx = _fixture.CreateFreshDbContext();
            var (miscMasterId, _) = await SeedPrerequisitesAsync(ctx);

            var result = await CreateRepository(ctx).SoftDeleteAsync(9999, miscMasterId, CancellationToken.None);

            result.Should().BeFalse();
        }
    }
}
