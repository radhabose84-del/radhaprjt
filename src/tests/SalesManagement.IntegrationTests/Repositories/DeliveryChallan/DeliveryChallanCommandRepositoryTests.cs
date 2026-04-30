using Contracts.Interfaces.Lookups.Finance;
using Microsoft.EntityFrameworkCore;
using SalesManagement.Infrastructure.Data;
using SalesManagement.Infrastructure.Repositories.DeliveryChallan;
using SalesManagement.IntegrationTests.Common;
using System.Data;
using static SalesManagement.Domain.Common.BaseEntity;

namespace SalesManagement.IntegrationTests.Repositories.DeliveryChallan
{
    /// <summary>
    /// Integration tests for DeliveryChallanCommandRepository.
    ///
    /// DeliveryChallanCommandRepository.CreateAsync is a complex transactional operation that:
    ///   1. Inserts header + details
    ///   2. Updates StockLedger rows (Packed → Reserved) via raw SQL per detail pack range
    ///   3. Increments Finance.DocumentSequence
    ///
    /// Full Create/SoftDelete testing requires a populated StockLedger with PROD-type records.
    /// These tests validate UpdateApprovalStatusAsync and not-found edge cases against
    /// seeded DC headers (bypassing the transactional CreateAsync path).
    /// </summary>
    [Collection("DatabaseCollection")]
    public sealed class DeliveryChallanCommandRepositoryTests
    {
        private readonly DbFixture _fixture;

        public DeliveryChallanCommandRepositoryTests(DbFixture fixture) => _fixture = fixture;

        private DeliveryChallanCommandRepository CreateRepo(
            ApplicationDbContext ctx,
            Mock<IDocumentSequenceLookup>? docSeq = null)
        {
            if (docSeq == null)
            {
                docSeq = new Mock<IDocumentSequenceLookup>(MockBehavior.Loose);
                docSeq.Setup(d => d.IncrementDocNoAsync(It.IsAny<int>(), It.IsAny<IDbConnection>(), It.IsAny<IDbTransaction>()))
                    .Returns(Task.CompletedTask);
            }
            return new DeliveryChallanCommandRepository(ctx, docSeq.Object);
        }

        // ── Prerequisites ─────────────────────────────────────────────────

        private async Task<int> EnsureMiscAsync(ApplicationDbContext ctx, int miscTypeId, string code)
        {
            var m = await ctx.MiscMaster.FirstOrDefaultAsync(x => x.MiscTypeId == miscTypeId && x.Code == code);
            if (m == null)
            {
                m = new SalesManagement.Domain.Entities.MiscMaster
                {
                    MiscTypeId = miscTypeId, Code = code, Description = code,
                    IsActive = Status.Active, IsDeleted = IsDelete.NotDeleted
                };
                await ctx.MiscMaster.AddAsync(m);
                await ctx.SaveChangesAsync();
            }
            return m.Id;
        }

        /// <summary>
        /// Seeds the full MovementTypeConfig → StoTypeMaster → StoHeader chain and approval
        /// status rows. Returns (stoHeaderId, approvedStatusId, pendingStatusId, dcTypeId, movementTypeId).
        /// dcTypeId + movementTypeId are required because DeliveryChallanHeader now has NOT NULL
        /// FK columns to MiscMaster (DCType) and MovementTypeConfig.
        /// </summary>
        private async Task<(int stoHeaderId, int approvedStatusId, int pendingStatusId, int dcTypeId, int movementTypeId)> EnsurePrerequisitesAsync()
        {
            await using var ctx = _fixture.CreateFreshDbContext();

            // Category misc for MovementTypeConfig
            var catType = await ctx.MiscTypeMaster.FirstOrDefaultAsync(x => x.MiscTypeCode == "DCC_CAT");
            if (catType == null)
            {
                catType = new SalesManagement.Domain.Entities.MiscTypeMaster
                {
                    MiscTypeCode = "DCC_CAT", Description = "DC Cat",
                    IsActive = Status.Active, IsDeleted = IsDelete.NotDeleted
                };
                await ctx.MiscTypeMaster.AddAsync(catType);
                await ctx.SaveChangesAsync();
            }
            var movCatId = await EnsureMiscAsync(ctx, catType.Id, "DCC_MC");
            var fromStId = await EnsureMiscAsync(ctx, catType.Id, "DCC_FS");
            var toStId = await EnsureMiscAsync(ctx, catType.Id, "DCC_TS");

            // StoApprovalStatus type
            var approvalType = await ctx.MiscTypeMaster.FirstOrDefaultAsync(x => x.MiscTypeCode == "ApprovalStatus");
            if (approvalType == null)
            {
                approvalType = new SalesManagement.Domain.Entities.MiscTypeMaster
                {
                    MiscTypeCode = "ApprovalStatus", Description = "STO Approval",
                    IsActive = Status.Active, IsDeleted = IsDelete.NotDeleted
                };
                await ctx.MiscTypeMaster.AddAsync(approvalType);
                await ctx.SaveChangesAsync();
            }
            var approvedId = await EnsureMiscAsync(ctx, approvalType.Id, "Approved");
            var pendingId = await EnsureMiscAsync(ctx, approvalType.Id, "Pending");

            // MovementTypeConfig
            var movement = await ctx.MovementTypeConfig.FirstOrDefaultAsync(x => x.MovementCode == "DCCM");
            if (movement == null)
            {
                movement = new SalesManagement.Domain.Entities.MovementTypeConfig
                {
                    MovementCode = "DCCM", MovementDescription = "DC Movement",
                    MovementCategoryId = movCatId,
                    FromStockTypeId = fromStId, ToStockTypeId = toStId,
                    QuantityUpdateFlag = true, ValueUpdateFlag = false,
                    BatchRequiredFlag = false, NegativeStockAllowed = false,
                    IsActive = Status.Active, IsDeleted = IsDelete.NotDeleted
                };
                await ctx.MovementTypeConfig.AddAsync(movement);
                await ctx.SaveChangesAsync();
            }

            // StoTypeMaster
            var stoType = await ctx.StoTypeMaster.FirstOrDefaultAsync(x => x.StoTypeCode == "DCCT");
            if (stoType == null)
            {
                stoType = new SalesManagement.Domain.Entities.StoTypeMaster
                {
                    StoTypeCode = "DCCT", StoTypeName = "DC STO Type",
                    PgiMovementTypeId = movement.Id, GrMovementTypeId = movement.Id,
                    IsActive = Status.Active, IsDeleted = IsDelete.NotDeleted
                };
                await ctx.StoTypeMaster.AddAsync(stoType);
                await ctx.SaveChangesAsync();
            }

            // StoHeader
            var sto = await ctx.StoHeader.FirstOrDefaultAsync(x => x.StoNumber == "DCC_STO1");
            if (sto == null)
            {
                sto = new SalesManagement.Domain.Entities.StoHeader
                {
                    StoNumber = "DCC_STO1",
                    DocumentDate = DateOnly.FromDateTime(DateTime.UtcNow.Date),
                    ExpectedDeliveryDate = DateOnly.FromDateTime(DateTime.UtcNow.Date.AddDays(5)),
                    StoTypeId = stoType.Id, MovementTypeId = movement.Id,
                    SupplyingPlantId = 1, SupplyingStorageLocationId = 1,
                    ReceivingPlantId = 2, ReceivingStorageLocationId = 2,
                    IsActive = Status.Active, IsDeleted = IsDelete.NotDeleted
                };
                await ctx.StoHeader.AddAsync(sto);
                await ctx.SaveChangesAsync();
            }

            // DCType MiscMaster — needed for the new DeliveryChallanHeader.DcTypeId NOT NULL FK
            var dcTypeMisc = await ctx.MiscTypeMaster.FirstOrDefaultAsync(x => x.MiscTypeCode == "DCType");
            if (dcTypeMisc == null)
            {
                dcTypeMisc = new SalesManagement.Domain.Entities.MiscTypeMaster
                {
                    MiscTypeCode = "DCType", Description = "DC Type",
                    IsActive = Status.Active, IsDeleted = IsDelete.NotDeleted
                };
                await ctx.MiscTypeMaster.AddAsync(dcTypeMisc);
                await ctx.SaveChangesAsync();
            }
            var dcTypeId = await EnsureMiscAsync(ctx, dcTypeMisc.Id, "Non-Returnable");

            return (sto.Id, approvedId, pendingId, dcTypeId, movement.Id);
        }

        private async Task<int> SeedDeliveryChallanAsync(int stoHeaderId, int statusId, int dcTypeId, int movementTypeId, string dcNumber = "DCC_01")
        {
            await using var ctx = _fixture.CreateFreshDbContext();
            var dc = new SalesManagement.Domain.Entities.DeliveryChallanHeader
            {
                DeliveryNumber = dcNumber,
                DeliveryDate = DateOnly.FromDateTime(DateTime.UtcNow.Date),
                StoHeaderId = stoHeaderId,
                FromPlantId = 1, FromStorageLocationId = 1,
                ToPlantId = 2, ToStorageLocationId = 2,
                TransporterId = 1, VehicleNumber = "TN01AB1234",
                DeliveryValue = 5000m, ConsignmentValue = 5000m,
                StatusId = statusId,
                DcTypeId = dcTypeId,
                MovementTypeId = movementTypeId,
                IsActive = Status.Active, IsDeleted = IsDelete.NotDeleted
            };
            await ctx.DeliveryChallanHeader.AddAsync(dc);
            await ctx.SaveChangesAsync();
            return dc.Id;
        }

        private async Task ClearAsync() =>
            await _fixture.ClearTablesAsync(
                "Sales.StoReceiptDetail",
                "Sales.StoReceiptHeader",
                "Sales.DeliveryChallanDetail",
                "Sales.DeliveryChallanHeader");

        // ── UpdateApprovalStatusAsync ─────────────────────────────────────

        [Fact]
        public async Task UpdateApprovalStatusAsync_Should_Update_StatusId()
        {
            await ClearAsync();
            var (stoId, approvedId, pendingId, dcTypeId, movementTypeId) = await EnsurePrerequisitesAsync();
            var dcId = await SeedDeliveryChallanAsync(stoId, pendingId, dcTypeId, movementTypeId, "DCC_UAS1");

            await using var ctx = _fixture.CreateFreshDbContext();
            await CreateRepo(ctx).UpdateApprovalStatusAsync(dcId, "Approved", 7, "tester", "127.0.0.1", CancellationToken.None);
            ctx.ChangeTracker.Clear();

            var saved = await ctx.DeliveryChallanHeader.FirstOrDefaultAsync(x => x.Id == dcId);
            saved.Should().NotBeNull();
            saved!.StatusId.Should().Be(approvedId);
        }

        [Fact]
        public async Task UpdateApprovalStatusAsync_Should_NoOp_When_NotFound()
        {
            await using var ctx = _fixture.CreateFreshDbContext();

            // Should not throw
            await CreateRepo(ctx).UpdateApprovalStatusAsync(9999999, "Approved", 7, "tester", "127.0.0.1", CancellationToken.None);
        }

        // ── SoftDeleteAsync ───────────────────────────────────────────────

        [Fact]
        public async Task SoftDeleteAsync_Should_Return_False_When_NotFound()
        {
            await using var ctx = _fixture.CreateFreshDbContext();

            var result = await CreateRepo(ctx).SoftDeleteAsync(
                9999999, reservedStatusId: 0, packedStatusId: 0, CancellationToken.None);

            result.Should().BeFalse();
        }

        [Fact]
        public async Task SoftDeleteAsync_Should_SetIsDeleted_On_HeaderOnly_Record()
        {
            await ClearAsync();
            var (stoId, _, pendingId, dcTypeId, movementTypeId) = await EnsurePrerequisitesAsync();
            var dcId = await SeedDeliveryChallanAsync(stoId, pendingId, dcTypeId, movementTypeId, "DCC_SD1");

            await using var ctx = _fixture.CreateFreshDbContext();
            // No detail lines — no StockLedger reversal occurs
            var result = await CreateRepo(ctx).SoftDeleteAsync(
                dcId, reservedStatusId: 0, packedStatusId: 0, CancellationToken.None);
            ctx.ChangeTracker.Clear();

            result.Should().BeTrue();
            var saved = await ctx.DeliveryChallanHeader
                .IgnoreQueryFilters()
                .FirstOrDefaultAsync(x => x.Id == dcId);
            saved!.IsDeleted.Should().Be(IsDelete.Deleted);
        }
    }
}
