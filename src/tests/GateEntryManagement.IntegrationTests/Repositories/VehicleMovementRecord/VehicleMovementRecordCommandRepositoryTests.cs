using Contracts.Interfaces;
using Contracts.Interfaces.Lookups.Finance;
using GateEntryManagement.Infrastructure.Repositories.VehicleMovementRecord;
using GateEntryManagement.Infrastructure.Repositories.MiscTypeMaster;
using GateEntryManagement.Infrastructure.Repositories.MiscMaster;
using GateEntryManagement.Infrastructure.Data;

namespace GateEntryManagement.IntegrationTests.Repositories.VehicleMovementRecord
{
    [Collection("DatabaseCollection")]
    public sealed class VehicleMovementRecordCommandRepositoryTests
    {
        private readonly DbFixture _fixture;

        public VehicleMovementRecordCommandRepositoryTests(DbFixture fixture)
        {
            _fixture = fixture;
        }

        private VehicleMovementRecordCommandRepository CreateRepository(ApplicationDbContext ctx)
        {
            var mockDocSeqLookup = new Mock<IDocumentSequenceLookup>(MockBehavior.Loose);

            var mockIpService = new Mock<IIPAddressService>(MockBehavior.Loose);
            mockIpService.Setup(s => s.GetUserId()).Returns(1);
            mockIpService.Setup(s => s.GetUserName()).Returns("test-user");
            mockIpService.Setup(s => s.GetUserIPAddress()).Returns("127.0.0.1");

            return new VehicleMovementRecordCommandRepository(ctx, mockDocSeqLookup.Object, mockIpService.Object);
        }

        private async Task<int> SeedPrerequisitesAndGetMiscIdAsync(ApplicationDbContext ctx)
        {
            // Clear all tables (FK-safe)
            await _fixture.ClearAllTablesAsync();

            // Seed MiscTypeMaster
            var typeRepo = new MiscTypeMasterCommandRepository(ctx);
            var typeId = await typeRepo.CreateAsync(new GateEntryManagement.Domain.Entities.MiscTypeMaster
            {
                MiscTypeCode = "VMRSTATUS",
                Description = "VMR Status",
                IsActive = Status.Active,
                IsDeleted = IsDelete.NotDeleted
            });

            // Seed MiscMaster entry
            var miscRepo = new MiscMasterCommandRepository(ctx);
            var miscId = await miscRepo.CreateAsync(new GateEntryManagement.Domain.Entities.MiscMaster
            {
                MiscTypeId = typeId,
                Code = "IN",
                Description = "Inside Premises",
                SortOrder = 1,
                IsActive = Status.Active,
                IsDeleted = IsDelete.NotDeleted
            });

            return miscId;
        }

        private async Task<int> SeedVMRAsync(ApplicationDbContext ctx, int miscMasterId,
            string vehicleNumber = "KA01AB1234", string driverName = "John Doe")
        {
            var vmr = new GateEntryManagement.Domain.Entities.VehicleMovementRecord
            {
                VehicleMovementId = "VMR00001",
                VehicleNumber = vehicleNumber,
                DriverName = driverName,
                DriverMobileNo = "9876543210",
                PurposeOfVisitId = miscMasterId,
                StatusId = miscMasterId,
                GateInTime = DateTimeOffset.UtcNow,
                GateInBy = "test-user",
                UnitId = 1,
                IsActive = Status.Active,
                IsDeleted = IsDelete.NotDeleted
            };
            await ctx.VehicleMovementRecord.AddAsync(vmr);
            await ctx.SaveChangesAsync();
            return vmr.Id;
        }

        // --- UPDATE ---

        [Fact]
        public async Task UpdateAsync_Should_Persist_Changes()
        {
            await using var ctx = _fixture.CreateFreshDbContext();
            var miscId = await SeedPrerequisitesAndGetMiscIdAsync(ctx);
            var id = await SeedVMRAsync(ctx, miscId);
            ctx.ChangeTracker.Clear();

            var entity = await ctx.VehicleMovementRecord.FirstAsync(x => x.Id == id);
            entity.VehicleNumber = "MH01XY9999";
            entity.DriverName = "Updated Driver";
            entity.DriverMobileNo = "1111111111";
            entity.Remarks = "Updated remarks";

            await CreateRepository(ctx).UpdateAsync(entity);
            ctx.ChangeTracker.Clear();

            var updated = await ctx.VehicleMovementRecord.FirstAsync(x => x.Id == id);
            updated.VehicleNumber.Should().Be("MH01XY9999");
            updated.DriverName.Should().Be("Updated Driver");
            updated.DriverMobileNo.Should().Be("1111111111");
            updated.Remarks.Should().Be("Updated remarks");
        }

        [Fact]
        public async Task UpdateAsync_Should_Return_EntityId_When_Successful()
        {
            await using var ctx = _fixture.CreateFreshDbContext();
            var miscId = await SeedPrerequisitesAndGetMiscIdAsync(ctx);
            var id = await SeedVMRAsync(ctx, miscId);
            ctx.ChangeTracker.Clear();

            var entity = await ctx.VehicleMovementRecord.FirstAsync(x => x.Id == id);
            entity.DriverName = "Changed Name";

            var result = await CreateRepository(ctx).UpdateAsync(entity);

            result.Should().Be(id);
        }

        [Fact]
        public async Task UpdateAsync_Should_Return_Zero_When_NotFound()
        {
            await using var ctx = _fixture.CreateFreshDbContext();
            var miscId = await SeedPrerequisitesAndGetMiscIdAsync(ctx);

            var nonExistentEntity = new GateEntryManagement.Domain.Entities.VehicleMovementRecord
            {
                Id = 99999,
                VehicleNumber = "XX00ZZ0000",
                DriverName = "Nobody",
                PurposeOfVisitId = miscId,
                StatusId = miscId,
                GateInTime = DateTimeOffset.UtcNow,
                UnitId = 1,
                IsActive = Status.Active,
                IsDeleted = IsDelete.NotDeleted
            };

            var result = await CreateRepository(ctx).UpdateAsync(nonExistentEntity);

            result.Should().Be(0);
        }

        [Fact]
        public async Task UpdateAsync_Should_Not_Change_VehicleMovementId()
        {
            await using var ctx = _fixture.CreateFreshDbContext();
            var miscId = await SeedPrerequisitesAndGetMiscIdAsync(ctx);
            var id = await SeedVMRAsync(ctx, miscId);
            ctx.ChangeTracker.Clear();

            var entity = await ctx.VehicleMovementRecord.FirstAsync(x => x.Id == id);
            entity.DriverName = "Different Driver";
            await CreateRepository(ctx).UpdateAsync(entity);
            ctx.ChangeTracker.Clear();

            var updated = await ctx.VehicleMovementRecord.FirstAsync(x => x.Id == id);
            updated.VehicleMovementId.Should().Be("VMR00001");
        }

        [Fact]
        public async Task UpdateAsync_Should_Update_IsActive_Status()
        {
            await using var ctx = _fixture.CreateFreshDbContext();
            var miscId = await SeedPrerequisitesAndGetMiscIdAsync(ctx);
            var id = await SeedVMRAsync(ctx, miscId);
            ctx.ChangeTracker.Clear();

            var entity = await ctx.VehicleMovementRecord.FirstAsync(x => x.Id == id);
            entity.IsActive = Status.Inactive;
            await CreateRepository(ctx).UpdateAsync(entity);
            ctx.ChangeTracker.Clear();

            var updated = await ctx.VehicleMovementRecord.FirstAsync(x => x.Id == id);
            updated.IsActive.Should().Be(Status.Inactive);
        }

        // --- SOFT DELETE ---

        [Fact]
        public async Task SoftDeleteAsync_Should_Return_True_When_Successful()
        {
            await using var ctx = _fixture.CreateFreshDbContext();
            var miscId = await SeedPrerequisitesAndGetMiscIdAsync(ctx);
            var id = await SeedVMRAsync(ctx, miscId);
            ctx.ChangeTracker.Clear();

            var result = await CreateRepository(ctx).SoftDeleteAsync(id, CancellationToken.None);

            result.Should().BeTrue();
        }

        [Fact]
        public async Task SoftDeleteAsync_Should_Set_IsDeleted_Flag()
        {
            await using var ctx = _fixture.CreateFreshDbContext();
            var miscId = await SeedPrerequisitesAndGetMiscIdAsync(ctx);
            var id = await SeedVMRAsync(ctx, miscId);
            ctx.ChangeTracker.Clear();

            await CreateRepository(ctx).SoftDeleteAsync(id, CancellationToken.None);
            ctx.ChangeTracker.Clear();

            var deleted = await ctx.VehicleMovementRecord
                .IgnoreQueryFilters()
                .FirstOrDefaultAsync(x => x.Id == id);

            deleted.Should().NotBeNull();
            deleted.IsDeleted.Should().Be(IsDelete.Deleted);
        }

        [Fact]
        public async Task SoftDeleteAsync_Should_Return_False_When_NotFound()
        {
            await using var ctx = _fixture.CreateFreshDbContext();
            var miscId = await SeedPrerequisitesAndGetMiscIdAsync(ctx);

            var result = await CreateRepository(ctx).SoftDeleteAsync(99999, CancellationToken.None);

            result.Should().BeFalse();
        }

        [Fact]
        public async Task SoftDeleteAsync_Should_Return_False_When_Already_Deleted()
        {
            await using var ctx = _fixture.CreateFreshDbContext();
            var miscId = await SeedPrerequisitesAndGetMiscIdAsync(ctx);
            var id = await SeedVMRAsync(ctx, miscId);
            ctx.ChangeTracker.Clear();

            await CreateRepository(ctx).SoftDeleteAsync(id, CancellationToken.None);
            ctx.ChangeTracker.Clear();

            var result = await CreateRepository(ctx).SoftDeleteAsync(id, CancellationToken.None);

            result.Should().BeFalse();
        }

        [Fact]
        public async Task SoftDeleteAsync_Should_Populate_ModifiedAuditFields()
        {
            await using var ctx = _fixture.CreateFreshDbContext();
            var miscId = await SeedPrerequisitesAndGetMiscIdAsync(ctx);
            var id = await SeedVMRAsync(ctx, miscId);
            ctx.ChangeTracker.Clear();

            await CreateRepository(ctx).SoftDeleteAsync(id, CancellationToken.None);
            ctx.ChangeTracker.Clear();

            var deleted = await ctx.VehicleMovementRecord
                .IgnoreQueryFilters()
                .FirstOrDefaultAsync(x => x.Id == id);

            deleted.Should().NotBeNull();
            deleted.ModifiedBy.Should().NotBeNull();
            deleted.ModifiedDate.Should().NotBeNull();
        }
    }
}
