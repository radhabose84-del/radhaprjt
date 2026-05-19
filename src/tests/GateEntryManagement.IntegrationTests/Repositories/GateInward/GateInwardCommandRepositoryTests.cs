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

        private async Task<int> SeedGateInwardWithAttachmentAsync(ApplicationDbContext ctx, int vmrId)
        {
            var entity = new GateEntryManagement.Domain.Entities.GateInwardHdr
            {
                GateEntryNo = "GE-ATT-1",
                VehicleMovementRecordId = vmrId,
                GrossWeight = 1000,
                TareWeight = 200,
                NetWeight = 800,
                QAInspectionRequired = false,
                UnitId = 1,
                AttachmentFileName = "abc.pdf",
                AttachmentOriginalFileName = "lr-copy.pdf",
                AttachmentFilePath = "GateEntry/abc.pdf",
                AttachmentFileType = "application/pdf",
                AttachmentFileSize = 2048,
                IsActive = Status.Active,
                IsDeleted = IsDelete.NotDeleted
            };
            await ctx.GateInwardHdr.AddAsync(entity);
            await ctx.SaveChangesAsync();
            return entity.Id;
        }

        // --- CLEAR ATTACHMENT ---

        [Fact]
        public async Task ClearAttachmentAsync_Should_Return_OldPath_And_NullColumns()
        {
            await using var ctx = _fixture.CreateFreshDbContext();
            var (_, vmrId) = await SeedPrerequisitesAsync(ctx);
            var id = await SeedGateInwardWithAttachmentAsync(ctx, vmrId);
            ctx.ChangeTracker.Clear();

            var oldPath = await CreateRepository(ctx).ClearAttachmentAsync(id, CancellationToken.None);
            ctx.ChangeTracker.Clear();

            oldPath.Should().Be("GateEntry/abc.pdf");
            var reloaded = await ctx.GateInwardHdr.FirstAsync(x => x.Id == id);
            reloaded.AttachmentFileName.Should().BeNull();
            reloaded.AttachmentFilePath.Should().BeNull();
            reloaded.AttachmentFileSize.Should().BeNull();
        }

        [Fact]
        public async Task ClearAttachmentAsync_Should_Return_Null_When_NoAttachment()
        {
            await using var ctx = _fixture.CreateFreshDbContext();
            var (_, vmrId) = await SeedPrerequisitesAsync(ctx);
            var id = await SeedGateInwardAsync(ctx, vmrId);
            ctx.ChangeTracker.Clear();

            var oldPath = await CreateRepository(ctx).ClearAttachmentAsync(id, CancellationToken.None);

            oldPath.Should().BeNull();
        }

        [Fact]
        public async Task ClearAttachmentAsync_Should_Return_Null_When_NotFound()
        {
            await using var ctx = _fixture.CreateFreshDbContext();
            await SeedPrerequisitesAsync(ctx);

            var oldPath = await CreateRepository(ctx).ClearAttachmentAsync(9999, CancellationToken.None);

            oldPath.Should().BeNull();
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
