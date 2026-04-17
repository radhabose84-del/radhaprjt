using Contracts.Dtos.Lookups.Inventory;
using Contracts.Dtos.Lookups.Production;
using Contracts.Dtos.Lookups.Users;
using Contracts.Dtos.Lookups.Warehouse;
using Contracts.Interfaces.Lookups.Inventory;
using Contracts.Interfaces.Lookups.Production;
using Contracts.Interfaces.Lookups.Users;
using Contracts.Interfaces.Lookups.Warehouse;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using SalesManagement.Infrastructure.Data;
using SalesManagement.Infrastructure.Repositories.StoReceipt;
using SalesManagement.IntegrationTests.Common;
using static SalesManagement.Domain.Common.BaseEntity;

namespace SalesManagement.IntegrationTests.Repositories.StoReceipt
{
    // Note: StoReceiptCommandRepository.CreateAsync interacts with the StockLedger table and
    // requires a full PROD→STO stock-ledger fixture to execute meaningfully. Only the Query
    // repo is exercised here; the Command repo's transactional behavior is covered by unit
    // tests and its integration behavior by end-to-end workflow tests (out of scope).
    [Collection("DatabaseCollection")]
    public sealed class StoReceiptQueryRepositoryTests
    {
        private readonly DbFixture _fixture;
        public StoReceiptQueryRepositoryTests(DbFixture fixture) => _fixture = fixture;

        private StoReceiptQueryRepository CreateRepo(
            Mock<IUnitLookup>? unitLookup = null,
            Mock<IWarehouseLookup>? warehouseLookup = null,
            Mock<IRackLookup>? rackLookup = null,
            Mock<IItemLookup>? itemLookup = null,
            Mock<ILotMasterLookup>? lotLookup = null,
            Mock<IUOMLookup>? uomLookup = null)
        {
            if (unitLookup == null)
            {
                unitLookup = new Mock<IUnitLookup>(MockBehavior.Loose);
                unitLookup.Setup(u => u.GetAllUnitAsync())
                    .ReturnsAsync(new List<UnitLookupDto>
                    {
                        new() { UnitId = 1, UnitName = "Plant 1" }
                    });
                unitLookup.Setup(u => u.GetByIdsAsync(It.IsAny<IEnumerable<int>>(), It.IsAny<CancellationToken>()))
                    .ReturnsAsync((IEnumerable<int> ids, CancellationToken _) =>
                        (IReadOnlyList<UnitLookupDto>)ids.Select(id =>
                            new UnitLookupDto { UnitId = id, UnitName = "Plant " + id }).ToList());
            }
            if (warehouseLookup == null)
            {
                warehouseLookup = new Mock<IWarehouseLookup>(MockBehavior.Loose);
                warehouseLookup.Setup(w => w.GetAllAsync(It.IsAny<CancellationToken>()))
                    .ReturnsAsync(new List<WarehouseLookupDto>
                    {
                        new() { Id = 1, WarehouseName = "WH 1" }
                    });
                warehouseLookup.Setup(w => w.GetByIdsAsync(It.IsAny<IEnumerable<int>>(), It.IsAny<CancellationToken>()))
                    .ReturnsAsync((IEnumerable<int> ids, CancellationToken _) =>
                        (IReadOnlyList<WarehouseLookupDto>)ids.Select(id =>
                            new WarehouseLookupDto { Id = id, WarehouseName = "WH " + id }).ToList());
            }
            if (rackLookup == null)
            {
                rackLookup = new Mock<IRackLookup>(MockBehavior.Loose);
                rackLookup.Setup(r => r.GetAllAsync(It.IsAny<CancellationToken>()))
                    .ReturnsAsync(new List<RackLookupDto>());
                rackLookup.Setup(r => r.GetByIdsAsync(It.IsAny<IEnumerable<int>>(), It.IsAny<CancellationToken>()))
                    .ReturnsAsync((IReadOnlyList<RackLookupDto>)new List<RackLookupDto>());
            }
            if (itemLookup == null)
            {
                itemLookup = new Mock<IItemLookup>(MockBehavior.Loose);
                itemLookup.Setup(i => i.GetByIdsAsync(It.IsAny<IEnumerable<int>>(), It.IsAny<CancellationToken>()))
                    .ReturnsAsync((IEnumerable<int> ids, CancellationToken _) =>
                        (IReadOnlyList<ItemLookupDto>)ids.Select(id =>
                            new ItemLookupDto { Id = id, ItemCode = "I" + id, ItemName = "Item " + id }).ToList());
            }
            if (lotLookup == null)
            {
                lotLookup = new Mock<ILotMasterLookup>(MockBehavior.Loose);
                lotLookup.Setup(l => l.GetByIdsAsync(It.IsAny<IEnumerable<int>>(), It.IsAny<CancellationToken>()))
                    .ReturnsAsync((IEnumerable<int> ids, CancellationToken _) =>
                        (IReadOnlyList<LotMasterLookupDto>)ids.Select(id =>
                            new LotMasterLookupDto { Id = id, LotCode = "LOT" + id }).ToList());
            }
            if (uomLookup == null)
            {
                uomLookup = new Mock<IUOMLookup>(MockBehavior.Loose);
                uomLookup.Setup(u => u.GetByIdsAsync(It.IsAny<IEnumerable<int>>(), It.IsAny<CancellationToken>()))
                    .ReturnsAsync((IEnumerable<int> ids, CancellationToken _) =>
                        (IReadOnlyList<UOMLookupDto>)ids.Select(id =>
                            new UOMLookupDto { Id = id, UOMName = "KG" }).ToList());
            }

            return new StoReceiptQueryRepository(
                new SqlConnection(_fixture.ConnectionString),
                unitLookup.Object, warehouseLookup.Object, rackLookup.Object,
                itemLookup.Object, lotLookup.Object, uomLookup.Object);
        }

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

        private async Task<(int movementTypeId, int stoTypeId, int approvedStatus, int dcStatus, int srStatus)> EnsurePrerequisiteChainAsync()
        {
            await using var ctx = _fixture.CreateFreshDbContext();

            // MiscType for movement categories and stock types
            var catType = await ctx.MiscTypeMaster.FirstOrDefaultAsync(x => x.MiscTypeCode == "SRQ_CAT");
            if (catType == null)
            {
                catType = new SalesManagement.Domain.Entities.MiscTypeMaster
                {
                    MiscTypeCode = "SRQ_CAT", Description = "Category",
                    IsActive = Status.Active, IsDeleted = IsDelete.NotDeleted
                };
                await ctx.MiscTypeMaster.AddAsync(catType);
                await ctx.SaveChangesAsync();
            }
            var movementCatId = await EnsureMiscAsync(ctx, catType.Id, "SRQ_MC");
            var fromStockTypeId = await EnsureMiscAsync(ctx, catType.Id, "SRQ_FST");
            var toStockTypeId = await EnsureMiscAsync(ctx, catType.Id, "SRQ_TST");

            // ApprovalStatus type (used by IsDcApproved)
            var approvalType = await ctx.MiscTypeMaster.FirstOrDefaultAsync(x => x.MiscTypeCode == "ApprovalStatus");
            if (approvalType == null)
            {
                approvalType = new SalesManagement.Domain.Entities.MiscTypeMaster
                {
                    MiscTypeCode = "ApprovalStatus", Description = "Approval Status",
                    IsActive = Status.Active, IsDeleted = IsDelete.NotDeleted
                };
                await ctx.MiscTypeMaster.AddAsync(approvalType);
                await ctx.SaveChangesAsync();
            }
            var approvedStatusId = await EnsureMiscAsync(ctx, approvalType.Id, "Approved");
            var pendingStatusId = await EnsureMiscAsync(ctx, approvalType.Id, "Pending");

            // Movement type config
            var movement = await ctx.MovementTypeConfig.FirstOrDefaultAsync(x => x.MovementCode == "SRQM");
            if (movement == null)
            {
                movement = new SalesManagement.Domain.Entities.MovementTypeConfig
                {
                    MovementCode = "SRQM", MovementDescription = "STO",
                    MovementCategoryId = movementCatId,
                    FromStockTypeId = fromStockTypeId,
                    ToStockTypeId = toStockTypeId,
                    QuantityUpdateFlag = true,
                    ValueUpdateFlag = false,
                    BatchRequiredFlag = false,
                    NegativeStockAllowed = false,
                    IsActive = Status.Active, IsDeleted = IsDelete.NotDeleted
                };
                await ctx.MovementTypeConfig.AddAsync(movement);
                await ctx.SaveChangesAsync();
            }

            var stoType = await ctx.StoTypeMaster.FirstOrDefaultAsync(x => x.StoTypeCode == "SRQT");
            if (stoType == null)
            {
                stoType = new SalesManagement.Domain.Entities.StoTypeMaster
                {
                    StoTypeCode = "SRQT", StoTypeName = "STO Type",
                    PgiMovementTypeId = movement.Id,
                    GrMovementTypeId = movement.Id,
                    IsActive = Status.Active, IsDeleted = IsDelete.NotDeleted
                };
                await ctx.StoTypeMaster.AddAsync(stoType);
                await ctx.SaveChangesAsync();
            }

            return (movement.Id, stoType.Id, approvedStatusId, approvedStatusId, approvedStatusId);
        }

        private async Task<(int dcHeaderId, int dcDetailId, int stoHeaderId, int stoDetailId)> EnsureDcChainAsync(string dcNumber = "SRQ_DC1", int statusId = 0)
        {
            var (movementId, stoTypeId, approvedStatusId, _, _) = await EnsurePrerequisiteChainAsync();
            if (statusId == 0) statusId = approvedStatusId;

            await using var ctx = _fixture.CreateFreshDbContext();

            var sto = await ctx.StoHeader.Include(h => h.StoDetails).FirstOrDefaultAsync(x => x.StoNumber == "SRQ_STO1");
            if (sto == null)
            {
                sto = new SalesManagement.Domain.Entities.StoHeader
                {
                    StoNumber = "SRQ_STO1",
                    DocumentDate = DateOnly.FromDateTime(DateTime.UtcNow.Date),
                    ExpectedDeliveryDate = DateOnly.FromDateTime(DateTime.UtcNow.Date.AddDays(5)),
                    StoTypeId = stoTypeId,
                    MovementTypeId = movementId,
                    SupplyingPlantId = 1, SupplyingStorageLocationId = 1,
                    ReceivingPlantId = 1, ReceivingStorageLocationId = 1,
                    IsActive = Status.Active, IsDeleted = IsDelete.NotDeleted,
                    StoDetails = new List<SalesManagement.Domain.Entities.StoDetail>
                    {
                        new() { ItemId = 10, Quantity = 100m, UOMId = 1, TransferPrice = 50m }
                    }
                };
                await ctx.StoHeader.AddAsync(sto);
                await ctx.SaveChangesAsync();
            }
            var stoDetailId = sto.StoDetails!.First().Id;

            var dc = await ctx.DeliveryChallanHeader.Include(h => h.DeliveryChallanDetails).FirstOrDefaultAsync(x => x.DeliveryNumber == dcNumber);
            if (dc == null)
            {
                dc = new SalesManagement.Domain.Entities.DeliveryChallanHeader
                {
                    DeliveryNumber = dcNumber,
                    DeliveryDate = DateOnly.FromDateTime(DateTime.UtcNow.Date),
                    StoHeaderId = sto.Id,
                    FromPlantId = 1, FromStorageLocationId = 1,
                    ToPlantId = 1, ToStorageLocationId = 1,
                    TransporterId = 1,
                    VehicleNumber = "TN01AB1234",
                    DeliveryValue = 5000m,
                    ConsignmentValue = 5000m,
                    StatusId = statusId,
                    IsActive = Status.Active, IsDeleted = IsDelete.NotDeleted,
                    DeliveryChallanDetails = new List<SalesManagement.Domain.Entities.DeliveryChallanDetail>
                    {
                        new()
                        {
                            StoDetailId = stoDetailId,
                            ItemId = 10, LotId = 100,
                            StartPackNo = 1, EndPackNo = 20,
                            DispatchQuantity = 20m, UOMId = 1,
                            NetWeight = 1000m, ExMillRate = 50m,
                            LineMovementValue = 1000m
                        }
                    }
                };
                await ctx.DeliveryChallanHeader.AddAsync(dc);
                await ctx.SaveChangesAsync();
            }
            return (dc.Id, dc.DeliveryChallanDetails!.First().Id, sto.Id, stoDetailId);
        }

        private async Task<int> SeedStoReceiptAsync(string number = "SR_Q1", IsDelete deleted = IsDelete.NotDeleted, Status active = Status.Active)
        {
            var (dcHeaderId, dcDetailId, _, _) = await EnsureDcChainAsync();
            var (_, _, approvedStatus, _, _) = await EnsurePrerequisiteChainAsync();

            await using var ctx = _fixture.CreateFreshDbContext();
            var sr = new SalesManagement.Domain.Entities.StoReceiptHeader
            {
                StoReceiptNumber = number,
                StoReceiptDate = DateOnly.FromDateTime(DateTime.UtcNow.Date),
                DeliveryChallanHeaderId = dcHeaderId,
                ReceivingPlantId = 1,
                ReceivingStorageLocationId = 1,
                VehicleNumber = "TN01AB1234",
                Remarks = "test receipt",
                StatusId = approvedStatus,
                IsActive = active, IsDeleted = deleted,
                StoReceiptDetails = new List<SalesManagement.Domain.Entities.StoReceiptDetail>
                {
                    new()
                    {
                        DeliveryChallanDetailId = dcDetailId,
                        ItemId = 10, LotId = 100,
                        StartPackNo = 1, EndPackNo = 20,
                        DispatchQuantity = 20m, ReceivedQuantity = 20m,
                        DamageQuantity = 0m, AcceptedQuantity = 20m,
                        UOMId = 1, NetWeight = 1000m,
                        LineStatusId = approvedStatus
                    }
                }
            };
            await ctx.StoReceiptHeader.AddAsync(sr);
            await ctx.SaveChangesAsync();
            return sr.Id;
        }

        private async Task ClearAsync() =>
            await _fixture.ClearAllTablesAsync();

        [Fact]
        public async Task GetAllAsync_Should_Return_Seeded()
        {
            await ClearAsync();
            await SeedStoReceiptAsync("SR_A1");

            var (rows, total) = await CreateRepo().GetAllAsync(1, 10, null);

            rows.Should().HaveCount(1);
            total.Should().Be(1);
        }

        [Fact]
        public async Task GetAllAsync_Should_Filter_By_SearchTerm()
        {
            await ClearAsync();
            await SeedStoReceiptAsync("UNIQUE_SR_ZZ");
            await SeedStoReceiptAsync("OTHER_SR_YY");

            var (rows, _) = await CreateRepo().GetAllAsync(1, 10, "UNIQUE_SR_ZZ");

            rows.Should().HaveCount(1);
            rows[0].StoReceiptNumber.Should().Be("UNIQUE_SR_ZZ");
        }

        [Fact]
        public async Task GetAllAsync_Should_Exclude_SoftDeleted()
        {
            await ClearAsync();
            await SeedStoReceiptAsync("SR_DEL", deleted: IsDelete.Deleted);

            var (_, total) = await CreateRepo().GetAllAsync(1, 10, null);

            total.Should().Be(0);
        }

        [Fact]
        public async Task GetByIdAsync_Should_Return_With_Details()
        {
            await ClearAsync();
            var id = await SeedStoReceiptAsync("SR_B1");

            var result = await CreateRepo().GetByIdAsync(id);

            result.Should().NotBeNull();
            result!.Id.Should().Be(id);
        }

        [Fact]
        public async Task GetByIdAsync_Should_Return_Null_When_NotFound()
        {
            var result = await CreateRepo().GetByIdAsync(9999999);
            result.Should().BeNull();
        }

        [Fact]
        public async Task AutocompleteAsync_Should_Return_Active_Matching()
        {
            await ClearAsync();
            await SeedStoReceiptAsync("SR_AC_MATCH");

            var result = await CreateRepo().AutocompleteAsync("SR_AC_MATCH", CancellationToken.None);

            result.Should().HaveCount(1);
        }

        [Fact]
        public async Task AutocompleteAsync_Should_Exclude_Inactive()
        {
            await ClearAsync();
            await SeedStoReceiptAsync("SR_INACT", active: Status.Inactive);

            var result = await CreateRepo().AutocompleteAsync("SR_INACT", CancellationToken.None);

            result.Should().BeEmpty();
        }

        [Fact]
        public async Task NotFoundAsync_Should_Return_True_For_Missing()
        {
            var result = await CreateRepo().NotFoundAsync(9999999);
            result.Should().BeTrue();
        }

        [Fact]
        public async Task NotFoundAsync_Should_Return_False_When_Exists()
        {
            await ClearAsync();
            var id = await SeedStoReceiptAsync("SR_NF");

            var result = await CreateRepo().NotFoundAsync(id);

            result.Should().BeFalse();
        }

        [Fact]
        public async Task DeliveryChallanHeaderExistsAsync_Should_Return_True_For_Active()
        {
            var (dcHeaderId, _, _, _) = await EnsureDcChainAsync();

            var result = await CreateRepo().DeliveryChallanHeaderExistsAsync(dcHeaderId);

            result.Should().BeTrue();
        }

        [Fact]
        public async Task DeliveryChallanHeaderExistsAsync_Should_Return_False_When_Missing()
        {
            var result = await CreateRepo().DeliveryChallanHeaderExistsAsync(9999999);
            result.Should().BeFalse();
        }

        [Fact]
        public async Task IsDcApprovedAsync_Should_Return_True_When_Approved()
        {
            var (dcHeaderId, _, _, _) = await EnsureDcChainAsync();

            var result = await CreateRepo().IsDcApprovedAsync(dcHeaderId);

            result.Should().BeTrue();
        }

        [Fact]
        public async Task IsStoReceiptExistsForDcAsync_Should_Return_True_When_Exists()
        {
            await ClearAsync();
            await SeedStoReceiptAsync("SR_EXIST");
            var (dcHeaderId, _, _, _) = await EnsureDcChainAsync();

            var result = await CreateRepo().IsStoReceiptExistsForDcAsync(dcHeaderId);

            result.Should().BeTrue();
        }

        [Fact]
        public async Task IsStoReceiptExistsForDcAsync_Should_Return_False_When_None()
        {
            await ClearAsync();
            var (dcHeaderId, _, _, _) = await EnsureDcChainAsync();

            var result = await CreateRepo().IsStoReceiptExistsForDcAsync(dcHeaderId);

            result.Should().BeFalse();
        }

        [Fact]
        public async Task GetDcOpenQtyAsync_Should_Return_Open_Quantity()
        {
            await ClearAsync();
            var (_, dcDetailId, _, _) = await EnsureDcChainAsync();

            var result = await CreateRepo().GetDcOpenQtyAsync(dcDetailId);

            result.Should().NotBeNull();
            result!.DispatchQuantity.Should().Be(20m);
            result.OpenQuantity.Should().Be(20m); // nothing received yet
        }

        [Fact]
        public async Task GetDcOpenQtyAsync_Should_Return_Null_When_NotFound()
        {
            var result = await CreateRepo().GetDcOpenQtyAsync(9999999);
            result.Should().BeNull();
        }
    }
}
