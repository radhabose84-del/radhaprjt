using Contracts.Dtos.Lookups.Inventory;
using Contracts.Dtos.Lookups.Party;
using Contracts.Dtos.Lookups.Production;
using Contracts.Dtos.Lookups.Users;
using Contracts.Dtos.Lookups.Warehouse;
using Contracts.Interfaces;
using Contracts.Interfaces.Lookups.Finance;
using Contracts.Interfaces.Lookups.Inventory;
using Contracts.Interfaces.Lookups.Party;
using Contracts.Interfaces.Lookups.Production;
using Contracts.Interfaces.Lookups.Users;
using Contracts.Interfaces.Lookups.Warehouse;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using SalesManagement.Infrastructure.Data;
using SalesManagement.Infrastructure.Repositories.DeliveryChallan;
using SalesManagement.IntegrationTests.Common;
using static SalesManagement.Domain.Common.BaseEntity;

namespace SalesManagement.IntegrationTests.Repositories.DeliveryChallan
{
    [Collection("DatabaseCollection")]
    public sealed class DeliveryChallanQueryRepositoryTests
    {
        private readonly DbFixture _fixture;
        public DeliveryChallanQueryRepositoryTests(DbFixture fixture) => _fixture = fixture;

        // ----- Lookup mocks (all Loose) -----

        private DeliveryChallanQueryRepository CreateRepo()
        {
            var unit = new Mock<IUnitLookup>(MockBehavior.Loose);
            unit.Setup(u => u.GetByIdsAsync(It.IsAny<IEnumerable<int>>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync((IEnumerable<int> ids, CancellationToken _) =>
                    (IReadOnlyList<UnitLookupDto>)ids.Select(id =>
                        new UnitLookupDto { UnitId = id, UnitName = "Plant " + id }).ToList());
            unit.Setup(u => u.GetByIdAsync(It.IsAny<int>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync((int id, CancellationToken _) =>
                    new UnitLookupDto { UnitId = id, UnitName = "Plant " + id });

            var warehouse = new Mock<IWarehouseLookup>(MockBehavior.Loose);
            warehouse.Setup(w => w.GetByIdsAsync(It.IsAny<IEnumerable<int>>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync((IEnumerable<int> ids, CancellationToken _) =>
                    (IReadOnlyList<WarehouseLookupDto>)ids.Select(id =>
                        new WarehouseLookupDto { Id = id, WarehouseName = "WH " + id }).ToList());

            var party = new Mock<IPartyLookup>(MockBehavior.Loose);
            party.Setup(p => p.GetByIdsAsync(It.IsAny<IEnumerable<int>>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync((IEnumerable<int> ids, CancellationToken _) =>
                    (IReadOnlyList<PartyLookupDto>)ids.Select(id =>
                        new PartyLookupDto { Id = id, PartyName = "Transporter " + id }).ToList());
            party.Setup(p => p.GetByIdAsync(It.IsAny<int>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync((int id, CancellationToken _) =>
                    new PartyLookupDto { Id = id, PartyName = "Transporter " + id });

            var item = new Mock<IItemLookup>(MockBehavior.Loose);
            item.Setup(i => i.GetByIdsAsync(It.IsAny<IEnumerable<int>>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync((IReadOnlyList<ItemLookupDto>)new List<ItemLookupDto>());

            var lot = new Mock<ILotMasterLookup>(MockBehavior.Loose);
            lot.Setup(l => l.GetByIdsAsync(It.IsAny<IEnumerable<int>>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync((IReadOnlyList<LotMasterLookupDto>)new List<LotMasterLookupDto>());

            var uom = new Mock<IUOMLookup>(MockBehavior.Loose);
            uom.Setup(u => u.GetByIdsAsync(It.IsAny<IEnumerable<int>>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync((IReadOnlyList<UOMLookupDto>)new List<UOMLookupDto>());

            var user = new Mock<IUserLookup>(MockBehavior.Loose);
            user.Setup(u => u.GetAllUserAsync())
                .ReturnsAsync(new List<UserLookupDto>());

            var eWaybill = new Mock<IEWaybillLookup>(MockBehavior.Loose);

            var ip = new Mock<IIPAddressService>(MockBehavior.Loose);
            ip.Setup(x => x.GetUserId()).Returns(1);
            ip.Setup(x => x.GetUnitId()).Returns(1);

            return new DeliveryChallanQueryRepository(
                new SqlConnection(_fixture.ConnectionString),
                unit.Object, warehouse.Object, party.Object,
                item.Object, lot.Object, uom.Object, user.Object,
                eWaybill.Object, ip.Object);
        }

        // ----- Seeding helpers -----

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
        /// Seeds the full FK chain: MiscTypeMaster -> MiscMaster (x3 for MovementTypeConfig)
        /// -> MovementTypeConfig -> StoTypeMaster -> StoHeader.
        /// Also seeds a StatusId MiscMaster for DeliveryChallanHeader.
        /// Returns (stoHeaderId, statusId).
        /// </summary>
        private async Task<(int stoHeaderId, int statusId)> EnsureStoChainAsync()
        {
            await using var ctx = _fixture.CreateFreshDbContext();

            // MiscTypeMaster
            var mt = await ctx.MiscTypeMaster.FirstOrDefaultAsync(x => x.MiscTypeCode == "DCQ_MT");
            if (mt == null)
            {
                mt = new SalesManagement.Domain.Entities.MiscTypeMaster
                {
                    MiscTypeCode = "DCQ_MT", Description = "DC Query Misc",
                    IsActive = Status.Active, IsDeleted = IsDelete.NotDeleted
                };
                await ctx.MiscTypeMaster.AddAsync(mt);
                await ctx.SaveChangesAsync();
            }

            // MiscMaster entries for MovementTypeConfig (3 FKs) + DC StatusId
            var movCatId = await EnsureMiscAsync(ctx, mt.Id, "DCQ_MCAT");
            var fromStId = await EnsureMiscAsync(ctx, mt.Id, "DCQ_FST");
            var toStId = await EnsureMiscAsync(ctx, mt.Id, "DCQ_TST");
            var statusId = await EnsureMiscAsync(ctx, mt.Id, "DCQ_STATUS");

            // MovementTypeConfig
            var mvc = await ctx.MovementTypeConfig.FirstOrDefaultAsync(x => x.MovementCode == "DCQM");
            if (mvc == null)
            {
                mvc = new SalesManagement.Domain.Entities.MovementTypeConfig
                {
                    MovementCode = "DCQM", MovementDescription = "DC Query MVC",
                    MovementCategoryId = movCatId,
                    FromStockTypeId = fromStId,
                    ToStockTypeId = toStId,
                    IsActive = Status.Active, IsDeleted = IsDelete.NotDeleted
                };
                await ctx.MovementTypeConfig.AddAsync(mvc);
                await ctx.SaveChangesAsync();
            }

            // StoTypeMaster
            var stoType = await ctx.StoTypeMaster.FirstOrDefaultAsync(x => x.StoTypeCode == "DCQT");
            if (stoType == null)
            {
                stoType = new SalesManagement.Domain.Entities.StoTypeMaster
                {
                    StoTypeCode = "DCQT", StoTypeName = "DC Query StoType", Description = "Test",
                    PgiMovementTypeId = mvc.Id,
                    GrMovementTypeId = mvc.Id,
                    IsActive = Status.Active, IsDeleted = IsDelete.NotDeleted
                };
                await ctx.StoTypeMaster.AddAsync(stoType);
                await ctx.SaveChangesAsync();
            }

            // StoHeader
            var sto = await ctx.StoHeader.FirstOrDefaultAsync(x => x.StoNumber == "DCQ_STO1");
            if (sto == null)
            {
                sto = new SalesManagement.Domain.Entities.StoHeader
                {
                    StoNumber = "DCQ_STO1",
                    DocumentDate = DateOnly.FromDateTime(DateTime.UtcNow.Date),
                    ExpectedDeliveryDate = DateOnly.FromDateTime(DateTime.UtcNow.Date.AddDays(5)),
                    StoTypeId = stoType.Id,
                    MovementTypeId = mvc.Id,
                    SupplyingPlantId = 1,
                    SupplyingStorageLocationId = 1,
                    ReceivingPlantId = 2,
                    ReceivingStorageLocationId = 2,
                    IsActive = Status.Active, IsDeleted = IsDelete.NotDeleted
                };
                await ctx.StoHeader.AddAsync(sto);
                await ctx.SaveChangesAsync();
            }

            return (sto.Id, statusId);
        }

        private async Task<int> SeedAsync(
            string deliveryNo = "DCQ_DC1",
            IsDelete deleted = IsDelete.NotDeleted,
            Status active = Status.Active)
        {
            var (stoHeaderId, statusId) = await EnsureStoChainAsync();

            await using var ctx = _fixture.CreateFreshDbContext();
            var dc = new SalesManagement.Domain.Entities.DeliveryChallanHeader
            {
                DeliveryNumber = deliveryNo,
                DeliveryDate = DateOnly.FromDateTime(DateTime.UtcNow.Date),
                StoHeaderId = stoHeaderId,
                FromPlantId = 1,
                FromStorageLocationId = 1,
                ToPlantId = 2,
                ToStorageLocationId = 2,
                TransporterId = 100,
                VehicleNumber = "TN01AB1234",
                TransportDistance = 150m,
                DeliveryValue = 5000m,
                ConsignmentValue = 4800m,
                StatusId = statusId,
                Remarks = "Integration test DC",
                GEFlag = false,
                IsActive = active, IsDeleted = deleted
            };
            await ctx.DeliveryChallanHeader.AddAsync(dc);
            await ctx.SaveChangesAsync();
            return dc.Id;
        }

        private async Task ClearAsync() =>
            await _fixture.ClearAllTablesAsync();

        // ----- Tests: GetAllAsync -----

        [Fact]
        public async Task GetAllAsync_Should_Return_Seeded_Record()
        {
            await ClearAsync();
            await SeedAsync("DCQ_GA1");

            var (rows, total) = await CreateRepo().GetAllAsync(1, 10, null);

            rows.Should().HaveCount(1);
            total.Should().Be(1);
        }

        [Fact]
        public async Task GetAllAsync_Should_Exclude_SoftDeleted()
        {
            await ClearAsync();
            await SeedAsync("DCQ_DEL", deleted: IsDelete.Deleted);

            var (_, total) = await CreateRepo().GetAllAsync(1, 10, null);

            total.Should().Be(0);
        }

        [Fact]
        public async Task GetAllAsync_Should_Filter_By_SearchTerm()
        {
            await ClearAsync();
            await SeedAsync("DCQ_UNIQ_ALPHA");
            await SeedAsync("DCQ_OTHER_BETA");

            var (rows, _) = await CreateRepo().GetAllAsync(1, 10, "UNIQ_ALPHA");

            rows.Should().HaveCount(1);
            rows[0].DeliveryNumber.Should().Be("DCQ_UNIQ_ALPHA");
        }

        [Fact]
        public async Task GetAllAsync_Should_Populate_CrossModule_Names()
        {
            await ClearAsync();
            await SeedAsync("DCQ_LOOKUP");

            var (rows, _) = await CreateRepo().GetAllAsync(1, 10, null);

            rows.Should().HaveCount(1);
            rows[0].FromPlantName.Should().NotBeNullOrEmpty();
            rows[0].ToPlantName.Should().NotBeNullOrEmpty();
            rows[0].TransporterName.Should().NotBeNullOrEmpty();
        }

        // ----- Tests: GetByIdAsync -----

        [Fact]
        public async Task GetByIdAsync_Should_Return_Matching()
        {
            await ClearAsync();
            var id = await SeedAsync("DCQ_BID1");

            var result = await CreateRepo().GetByIdAsync(id);

            result.Should().NotBeNull();
            result!.Id.Should().Be(id);
            result.DeliveryNumber.Should().Be("DCQ_BID1");
        }

        [Fact]
        public async Task GetByIdAsync_Should_Return_Null_When_NotFound()
        {
            var result = await CreateRepo().GetByIdAsync(9999999);
            result.Should().BeNull();
        }

        [Fact]
        public async Task GetByIdAsync_Should_Return_Null_When_SoftDeleted()
        {
            await ClearAsync();
            var id = await SeedAsync("DCQ_BID_DEL", deleted: IsDelete.Deleted);

            var result = await CreateRepo().GetByIdAsync(id);

            result.Should().BeNull();
        }

        // ----- Tests: NotFoundAsync -----

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
            var id = await SeedAsync("DCQ_NF1");

            var result = await CreateRepo().NotFoundAsync(id);

            result.Should().BeFalse();
        }

        // ----- Tests: StoHeaderExistsAsync -----

        [Fact]
        public async Task StoHeaderExistsAsync_Should_Return_True_When_Active()
        {
            var (stoHeaderId, _) = await EnsureStoChainAsync();

            var result = await CreateRepo().StoHeaderExistsAsync(stoHeaderId);

            result.Should().BeTrue();
        }

        [Fact]
        public async Task StoHeaderExistsAsync_Should_Return_False_When_Missing()
        {
            var result = await CreateRepo().StoHeaderExistsAsync(9999999);
            result.Should().BeFalse();
        }

        // ----- Tests: HasStoReceiptAsync -----

        [Fact]
        public async Task HasStoReceiptAsync_Should_Return_False_When_None()
        {
            await ClearAsync();
            var id = await SeedAsync("DCQ_RCPT");

            var result = await CreateRepo().HasStoReceiptAsync(id);

            result.Should().BeFalse();
        }

        // ----- Tests: AutocompleteAsync -----

        [Fact]
        public async Task AutocompleteAsync_Should_Return_Active_Matching()
        {
            await ClearAsync();
            await SeedAsync("DCQ_AUTO_MATCH");

            var result = await CreateRepo().AutocompleteAsync("DCQ_AUTO_MATCH", CancellationToken.None);

            result.Should().HaveCount(1);
            result[0].DeliveryNumber.Should().Be("DCQ_AUTO_MATCH");
        }

        [Fact]
        public async Task AutocompleteAsync_Should_Exclude_Inactive()
        {
            await ClearAsync();
            await SeedAsync("DCQ_AUTO_INACT", active: Status.Inactive);

            var result = await CreateRepo().AutocompleteAsync("DCQ_AUTO_INACT", CancellationToken.None);

            result.Should().BeEmpty();
        }
    }
}
