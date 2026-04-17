using Contracts.Dtos.Lookups.Inventory;
using Contracts.Dtos.Lookups.Logistics;
using Contracts.Dtos.Lookups.Party;
using Contracts.Dtos.Lookups.Production;
using Contracts.Dtos.Lookups.Users;
using Contracts.Dtos.Lookups.Warehouse;
using Contracts.Interfaces;
using Contracts.Interfaces.Lookups.Inventory;
using Contracts.Interfaces.Lookups.Logistics;
using Contracts.Interfaces.Lookups.Party;
using Contracts.Interfaces.Lookups.Production;
using Contracts.Interfaces.Lookups.Warehouse;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using SalesManagement.Application.Common.Interfaces;
using SalesManagement.Infrastructure.Data;
using SalesManagement.Infrastructure.Repositories.DispatchAdvice;
using SalesManagement.IntegrationTests.Common;
using static SalesManagement.Domain.Common.BaseEntity;

namespace SalesManagement.IntegrationTests.Repositories.DispatchAdvice
{
    [Collection("DatabaseCollection")]
    public sealed class DispatchAdviceQueryRepositoryTests
    {
        private readonly DbFixture _fixture;
        public DispatchAdviceQueryRepositoryTests(DbFixture fixture) => _fixture = fixture;

        private DispatchAdviceQueryRepository CreateRepo()
        {
            var party = new Mock<IPartyLookup>(MockBehavior.Loose);
            party.Setup(p => p.GetByIdsAsync(It.IsAny<IEnumerable<int>>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync((IEnumerable<int> ids, CancellationToken _) =>
                    (IReadOnlyList<PartyLookupDto>)ids.Select(id =>
                        new PartyLookupDto { Id = id, PartyName = "Party " + id }).ToList());

            var item = new Mock<IItemLookup>(MockBehavior.Loose);
            item.Setup(i => i.GetByIdsAsync(It.IsAny<IEnumerable<int>>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync((IEnumerable<int> ids, CancellationToken _) =>
                    (IReadOnlyList<ItemLookupDto>)ids.Select(id =>
                        new ItemLookupDto { Id = id, ItemCode = "I" + id, ItemName = "Item " + id }).ToList());

            var hsn = new Mock<IHSNLookup>(MockBehavior.Loose);
            hsn.Setup(h => h.GetByIdsAsync(It.IsAny<IEnumerable<int>>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync((IReadOnlyList<HSNLookupDto>)new List<HSNLookupDto>());

            var ip = new Mock<IIPAddressService>(MockBehavior.Loose);
            ip.Setup(x => x.GetUserId()).Returns(1);
            ip.Setup(x => x.GetUnitId()).Returns(1);

            var packType = new Mock<IPackTypeLookup>(MockBehavior.Loose);
            packType.Setup(p => p.GetByIdsAsync(It.IsAny<IEnumerable<int>>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync((IReadOnlyList<PackTypeLookupDto>)new List<PackTypeLookupDto>());

            var lot = new Mock<ILotMasterLookup>(MockBehavior.Loose);
            lot.Setup(l => l.GetByIdsAsync(It.IsAny<IEnumerable<int>>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync((IReadOnlyList<LotMasterLookupDto>)new List<LotMasterLookupDto>());

            var freight = new Mock<IFreightMasterLookup>(MockBehavior.Loose);
            freight.Setup(f => f.GetAllFreightMasterAsync())
                .ReturnsAsync(new List<FreightMasterLookupDto>());

            var warehouse = new Mock<IWarehouseLookup>(MockBehavior.Loose);
            warehouse.Setup(w => w.GetByIdsAsync(It.IsAny<IEnumerable<int>>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync((IReadOnlyList<WarehouseLookupDto>)new List<WarehouseLookupDto>());

            var bin = new Mock<IBinLookup>(MockBehavior.Loose);
            bin.Setup(b => b.GetByIdsAsync(It.IsAny<IEnumerable<int>>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync((IReadOnlyList<BinLookupDto>)new List<BinLookupDto>());

            var uom = new Mock<IUOMLookup>(MockBehavior.Loose);
            uom.Setup(u => u.GetByIdsAsync(It.IsAny<IEnumerable<int>>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync((IReadOnlyList<UOMLookupDto>)new List<UOMLookupDto>());

            return new DispatchAdviceQueryRepository(
                new SqlConnection(_fixture.ConnectionString),
                party.Object, item.Object, hsn.Object, ip.Object,
                packType.Object, lot.Object, freight.Object,
                warehouse.Object, bin.Object, uom.Object);
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

        private async Task<(int soId, int statusId, int dispatchTypeId)> EnsureSalesOrderAndMiscAsync()
        {
            await using var ctx = _fixture.CreateFreshDbContext();

            var mt = await ctx.MiscTypeMaster.FirstOrDefaultAsync(x => x.MiscTypeCode == "DAQ_MT");
            if (mt == null)
            {
                mt = new SalesManagement.Domain.Entities.MiscTypeMaster
                {
                    MiscTypeCode = "DAQ_MT", Description = "DA Misc",
                    IsActive = Status.Active, IsDeleted = IsDelete.NotDeleted
                };
                await ctx.MiscTypeMaster.AddAsync(mt);
                await ctx.SaveChangesAsync();
            }
            var statusId = await EnsureMiscAsync(ctx, mt.Id, "DAQ_ST");
            var dispTypeId = await EnsureMiscAsync(ctx, mt.Id, "DAQ_DT");
            var freightId = await EnsureMiscAsync(ctx, mt.Id, "DAQ_FT");
            var enquiryId = await EnsureMiscAsync(ctx, mt.Id, "DAQ_ENQ");

            var org = await ctx.SalesOrganisation.FirstOrDefaultAsync(x => x.SalesOrganisationCode == "DAQ_ORG");
            if (org == null)
            {
                org = new SalesManagement.Domain.Entities.SalesOrganisation
                {
                    SalesOrganisationCode = "DAQ_ORG", SalesOrganisationName = "Org",
                    CompanyId = 1, IsActive = Status.Active, IsDeleted = IsDelete.NotDeleted
                };
                await ctx.SalesOrganisation.AddAsync(org);
                await ctx.SaveChangesAsync();
            }
            var office = await ctx.SalesOffice.FirstOrDefaultAsync(x => x.SalesOfficeName == "DAQ_OFC");
            if (office == null)
            {
                office = new SalesManagement.Domain.Entities.SalesOffice
                {
                    SalesOfficeName = "DAQ_OFC", SalesOrganisationId = org.Id,
                    IsActive = Status.Active, IsDeleted = IsDelete.NotDeleted
                };
                await ctx.SalesOffice.AddAsync(office);
                await ctx.SaveChangesAsync();
            }
            var sg = await ctx.SalesGroup.FirstOrDefaultAsync(x => x.SalesGroupName == "DAQ_SG");
            if (sg == null)
            {
                sg = new SalesManagement.Domain.Entities.SalesGroup
                {
                    SalesGroupName = "DAQ_SG", SalesOfficeId = office.Id,
                    IsActive = Status.Active, IsDeleted = IsDelete.NotDeleted
                };
                await ctx.SalesGroup.AddAsync(sg);
                await ctx.SaveChangesAsync();
            }

            var so = await ctx.SalesOrderHeader.FirstOrDefaultAsync(x => x.SalesOrderNo == "DAQ_SO1");
            if (so == null)
            {
                so = new SalesManagement.Domain.Entities.SalesOrderHeader
                {
                    SalesOrderNo = "DAQ_SO1",
                    OrderDate = DateOnly.FromDateTime(DateTime.UtcNow.Date),
                    SalesGroupId = sg.Id,
                    EnquiryType = enquiryId,
                    UnitId = 1, PartyId = 100,
                    FreightTypeId = freightId,
                    FinalAmount = 5000m,
                    IsActive = Status.Active, IsDeleted = IsDelete.NotDeleted
                };
                await ctx.SalesOrderHeader.AddAsync(so);
                await ctx.SaveChangesAsync();
            }
            return (so.Id, statusId, dispTypeId);
        }

        private async Task<int> SeedAsync(string dispatchNo = "DA_Q1", IsDelete deleted = IsDelete.NotDeleted, Status active = Status.Active)
        {
            var (soId, statusId, dispTypeId) = await EnsureSalesOrderAndMiscAsync();
            await using var ctx = _fixture.CreateFreshDbContext();
            var da = new SalesManagement.Domain.Entities.DispatchAdviceHeader
            {
                DispatchNo = dispatchNo,
                DispatchDate = DateOnly.FromDateTime(DateTime.UtcNow.Date),
                StatusId = statusId,
                SalesOrderId = soId,
                PartyId = 100,
                TotOrderQty = 100m,
                TotDispatchedQty = 50m,
                TotPendingQty = 50m,
                DispatchTypeId = dispTypeId,
                FreightId = 1,
                UnitId = 1,
                InvFlg = false,
                IsActive = active, IsDeleted = deleted
            };
            await ctx.DispatchAdviceHeader.AddAsync(da);
            await ctx.SaveChangesAsync();
            return da.Id;
        }

        private async Task ClearAsync() =>
            await _fixture.ClearAllTablesAsync();

        [Fact]
        public async Task GetAllAsync_Should_Return_Seeded()
        {
            await ClearAsync();
            await SeedAsync("DA_A1");

            var (rows, total) = await CreateRepo().GetAllAsync(1, 10, null);

            rows.Should().HaveCount(1);
            total.Should().Be(1);
        }

        [Fact]
        public async Task GetAllAsync_Should_Exclude_SoftDeleted()
        {
            await ClearAsync();
            await SeedAsync("DA_DEL", deleted: IsDelete.Deleted);

            var (_, total) = await CreateRepo().GetAllAsync(1, 10, null);

            total.Should().Be(0);
        }

        [Fact]
        public async Task GetAllAsync_Should_Filter_By_SearchTerm()
        {
            await ClearAsync();
            await SeedAsync("UNIQ_DA_ZZ");
            await SeedAsync("OTHER_DA");

            var (rows, _) = await CreateRepo().GetAllAsync(1, 10, "UNIQ_DA_ZZ");

            rows.Should().HaveCount(1);
        }

        [Fact]
        public async Task GetByIdAsync_Should_Return_Matching()
        {
            await ClearAsync();
            var id = await SeedAsync("DA_B1");

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
            await SeedAsync("DA_AC_MATCH");

            var result = await CreateRepo().AutocompleteAsync("DA_AC_MATCH", CancellationToken.None);

            result.Should().HaveCount(1);
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
            var id = await SeedAsync("DA_NF");

            var result = await CreateRepo().NotFoundAsync(id);

            result.Should().BeFalse();
        }

        [Fact]
        public async Task SalesOrderExistsAsync_Should_Return_True()
        {
            var (soId, _, _) = await EnsureSalesOrderAndMiscAsync();

            var result = await CreateRepo().SalesOrderExistsAsync(soId);

            result.Should().BeTrue();
        }

        [Fact]
        public async Task SalesOrderExistsAsync_Should_Return_False_When_Missing()
        {
            var result = await CreateRepo().SalesOrderExistsAsync(9999999);
            result.Should().BeFalse();
        }

        [Fact]
        public async Task HasPendingAmendmentAsync_Should_Return_False_When_None()
        {
            var (soId, _, _) = await EnsureSalesOrderAndMiscAsync();

            var result = await CreateRepo().HasPendingAmendmentAsync(soId);

            result.Should().BeFalse();
        }

        [Fact]
        public async Task HasInvoiceAsync_Should_Return_False_When_None()
        {
            await ClearAsync();
            var id = await SeedAsync("DA_INV");

            var result = await CreateRepo().HasInvoiceAsync(id);

            result.Should().BeFalse();
        }

        [Fact]
        public async Task GetSalesOrderUnitIdAsync_Should_Return_OrderUnitId()
        {
            var (soId, _, _) = await EnsureSalesOrderAndMiscAsync();

            var result = await CreateRepo().GetSalesOrderUnitIdAsync(soId);

            // OrderUnitId is nullable and not seeded in test fixture — returns 0 via ExecuteScalar<int>
            result.Should().Be(0);
        }
    }
}
