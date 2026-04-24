using Contracts.Dtos.Lookups.Finance;
using Contracts.Dtos.Lookups.Inventory;
using Contracts.Dtos.Lookups.Party;
using Contracts.Dtos.Lookups.Production;
using Contracts.Dtos.Lookups.Users;
using Contracts.Interfaces;
using Contracts.Interfaces.Lookups.Finance;
using Contracts.Interfaces.Lookups.Inventory;
using Contracts.Interfaces.Lookups.Party;
using Contracts.Interfaces.Lookups.Production;
using Contracts.Interfaces.Lookups.Users;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using SalesManagement.Infrastructure.Data;
using SalesManagement.Infrastructure.Repositories.Invoice;
using SalesManagement.IntegrationTests.Common;
using static SalesManagement.Domain.Common.BaseEntity;

namespace SalesManagement.IntegrationTests.Repositories.Invoice
{
    [Collection("DatabaseCollection")]
    public sealed class InvoiceQueryRepositoryTests
    {
        private readonly DbFixture _fixture;
        public InvoiceQueryRepositoryTests(DbFixture fixture) => _fixture = fixture;

        // ──────────────────────────────────────────────
        //  Factory — mock ALL 17 lookups with Loose
        // ──────────────────────────────────────────────

        private InvoiceQueryRepository CreateRepo()
        {
            var party = new Mock<IPartyLookup>(MockBehavior.Loose);
            party.Setup(p => p.GetByIdsAsync(It.IsAny<IEnumerable<int>>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync((IEnumerable<int> ids, CancellationToken _) =>
                    (IReadOnlyList<PartyLookupDto>)ids.Select(id =>
                        new PartyLookupDto { Id = id, PartyName = "Party " + id }).ToList());
            party.Setup(p => p.GetByIdAsync(It.IsAny<int>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync((int id, CancellationToken _) =>
                    new PartyLookupDto { Id = id, PartyName = "Party " + id });

            var unit = new Mock<IUnitLookup>(MockBehavior.Loose);
            unit.Setup(u => u.GetAllUnitAsync())
                .ReturnsAsync(new List<UnitLookupDto>
                {
                    new UnitLookupDto { UnitId = 1, UnitName = "Unit 1" }
                });

            var item = new Mock<IItemLookup>(MockBehavior.Loose);
            item.Setup(i => i.GetByIdsAsync(It.IsAny<IEnumerable<int>>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync((IReadOnlyList<ItemLookupDto>)new List<ItemLookupDto>());

            var uom = new Mock<IUOMLookup>(MockBehavior.Loose);
            uom.Setup(u => u.GetByIdsAsync(It.IsAny<IEnumerable<int>>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync((IReadOnlyList<UOMLookupDto>)new List<UOMLookupDto>());

            var finYear = new Mock<IFinancialYearLookup>(MockBehavior.Loose);
            finYear.Setup(f => f.GetByIdsAsync(It.IsAny<IEnumerable<int>>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync((IReadOnlyList<FinancialYearLookupDto>)new List<FinancialYearLookupDto>());
            finYear.Setup(f => f.GetByIdAsync(It.IsAny<int>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync((FinancialYearLookupDto?)null);

            var packType = new Mock<IPackTypeLookup>(MockBehavior.Loose);
            packType.Setup(p => p.GetByIdsAsync(It.IsAny<IEnumerable<int>>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync((IReadOnlyList<PackTypeLookupDto>)new List<PackTypeLookupDto>());

            var lot = new Mock<ILotMasterLookup>(MockBehavior.Loose);
            lot.Setup(l => l.GetByIdsAsync(It.IsAny<IEnumerable<int>>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync((IReadOnlyList<LotMasterLookupDto>)new List<LotMasterLookupDto>());

            var ip = new Mock<IIPAddressService>(MockBehavior.Loose);
            ip.Setup(x => x.GetUserId()).Returns(1);
            ip.Setup(x => x.GetUnitId()).Returns((int?)null); // no unit filter

            var state = new Mock<IStateLookup>(MockBehavior.Loose);
            state.Setup(s => s.GetByIdsAsync(It.IsAny<IEnumerable<int>>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync((IReadOnlyList<StateLookupDto>)new List<StateLookupDto>());

            var city = new Mock<ICityLookup>(MockBehavior.Loose);
            city.Setup(c => c.GetByIdsAsync(It.IsAny<IEnumerable<int>>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync((IReadOnlyList<CityLookupDto>)new List<CityLookupDto>());

            var companyDetail = new Mock<ICompanyDetailLookup>(MockBehavior.Loose);
            var unitDetail = new Mock<IUnitDetailLookup>(MockBehavior.Loose);
            var partyDetail = new Mock<IPartyDetailLookup>(MockBehavior.Loose);
            var partyBank = new Mock<IPartyBankLookup>(MockBehavior.Loose);
            var eInvoice = new Mock<IEInvoiceLookup>(MockBehavior.Loose);
            var eWaybill = new Mock<IEWaybillLookup>(MockBehavior.Loose);

            return new InvoiceQueryRepository(
                new SqlConnection(_fixture.ConnectionString),
                party.Object,
                unit.Object,
                item.Object,
                uom.Object,
                finYear.Object,
                packType.Object,
                lot.Object,
                ip.Object,
                state.Object,
                city.Object,
                companyDetail.Object,
                unitDetail.Object,
                partyDetail.Object,
                partyBank.Object,
                eInvoice.Object,
                eWaybill.Object);
        }

        // ──────────────────────────────────────────────
        //  Seed helpers — full FK chain
        // ──────────────────────────────────────────────

        private async Task<int> EnsureMiscAsync(ApplicationDbContext ctx, int miscTypeId, string code)
        {
            var m = await ctx.MiscMaster.FirstOrDefaultAsync(x => x.MiscTypeId == miscTypeId && x.Code == code);
            if (m == null)
            {
                m = new SalesManagement.Domain.Entities.MiscMaster
                {
                    MiscTypeId = miscTypeId,
                    Code = code,
                    Description = code,
                    IsActive = Status.Active,
                    IsDeleted = IsDelete.NotDeleted
                };
                await ctx.MiscMaster.AddAsync(m);
                await ctx.SaveChangesAsync();
            }
            return m.Id;
        }

        /// <summary>
        /// Creates the full seed chain:
        /// MiscTypeMaster → MiscMaster (x4) → SalesOrganisation → SalesOffice → SalesGroup
        /// → SalesOrderHeader → DispatchAdviceHeader
        /// Returns (dispatchAdviceId, statusId) — StatusId for InvoiceHeader.StatusId
        /// </summary>
        private async Task<(int daId, int statusId)> EnsureDispatchAdviceChainAsync()
        {
            await using var ctx = _fixture.CreateFreshDbContext();

            // MiscTypeMaster
            var mt = await ctx.MiscTypeMaster.FirstOrDefaultAsync(x => x.MiscTypeCode == "IVQM");
            if (mt == null)
            {
                mt = new SalesManagement.Domain.Entities.MiscTypeMaster
                {
                    MiscTypeCode = "IVQM",
                    Description = "IVQ Misc",
                    IsActive = Status.Active,
                    IsDeleted = IsDelete.NotDeleted
                };
                await ctx.MiscTypeMaster.AddAsync(mt);
                await ctx.SaveChangesAsync();
            }

            var statusId = await EnsureMiscAsync(ctx, mt.Id, "IVQS");
            var dispTypeId = await EnsureMiscAsync(ctx, mt.Id, "IVQD");
            var freightId = await EnsureMiscAsync(ctx, mt.Id, "IVQF");
            var enquiryId = await EnsureMiscAsync(ctx, mt.Id, "IVQE");

            // SalesOrganisation → SalesOffice → SalesGroup
            var org = await ctx.SalesOrganisation.FirstOrDefaultAsync(x => x.SalesOrganisationCode == "IVQORG");
            if (org == null)
            {
                org = new SalesManagement.Domain.Entities.SalesOrganisation
                {
                    SalesOrganisationCode = "IVQORG",
                    SalesOrganisationName = "IVQ Org",
                    CompanyId = 1,
                    IsActive = Status.Active,
                    IsDeleted = IsDelete.NotDeleted
                };
                await ctx.SalesOrganisation.AddAsync(org);
                await ctx.SaveChangesAsync();
            }

            var office = await ctx.SalesOffice.FirstOrDefaultAsync(x => x.SalesOfficeName == "IVQ_OFC");
            if (office == null)
            {
                office = new SalesManagement.Domain.Entities.SalesOffice
                {
                    SalesOfficeName = "IVQ_OFC",
                    SalesOrganisationId = org.Id,
                    IsActive = Status.Active,
                    IsDeleted = IsDelete.NotDeleted
                };
                await ctx.SalesOffice.AddAsync(office);
                await ctx.SaveChangesAsync();
            }

            var sg = await ctx.SalesGroup.FirstOrDefaultAsync(x => x.SalesGroupName == "IVQ_SG");
            if (sg == null)
            {
                sg = new SalesManagement.Domain.Entities.SalesGroup
                {
                    SalesGroupName = "IVQ_SG",
                    SalesOfficeId = office.Id,
                    IsActive = Status.Active,
                    IsDeleted = IsDelete.NotDeleted
                };
                await ctx.SalesGroup.AddAsync(sg);
                await ctx.SaveChangesAsync();
            }

            // SalesOrderHeader
            var so = await ctx.SalesOrderHeader.FirstOrDefaultAsync(x => x.SalesOrderNo == "IVQ_SO1");
            if (so == null)
            {
                so = new SalesManagement.Domain.Entities.SalesOrderHeader
                {
                    SalesOrderNo = "IVQ_SO1",
                    OrderDate = DateOnly.FromDateTime(DateTime.UtcNow.Date),
                    SalesGroupId = sg.Id,
                    EnquiryType = enquiryId,
                    UnitId = 1,
                    PartyId = 100,
                    FreightTypeId = freightId,
                    FinalAmount = 5000m,
                    IsActive = Status.Active,
                    IsDeleted = IsDelete.NotDeleted
                };
                await ctx.SalesOrderHeader.AddAsync(so);
                await ctx.SaveChangesAsync();
            }

            // DispatchAdviceHeader
            var da = await ctx.DispatchAdviceHeader.FirstOrDefaultAsync(x => x.DispatchNo == "IVQ_DA1");
            if (da == null)
            {
                da = new SalesManagement.Domain.Entities.DispatchAdviceHeader
                {
                    DispatchNo = "IVQ_DA1",
                    DispatchDate = DateOnly.FromDateTime(DateTime.UtcNow.Date),
                    StatusId = statusId,
                    SalesOrderId = so.Id,
                    PartyId = 100,
                    TotOrderQty = 100m,
                    TotDispatchedQty = 50m,
                    TotPendingQty = 50m,
                    DispatchTypeId = dispTypeId,
                    FreightId = 1,
                    UnitId = 1,
                    InvFlg = false,
                    IsActive = Status.Active,
                    IsDeleted = IsDelete.NotDeleted
                };
                await ctx.DispatchAdviceHeader.AddAsync(da);
                await ctx.SaveChangesAsync();
            }

            return (da.Id, statusId);
        }

        private async Task<int> SeedInvoiceAsync(
            string invoiceNo = "INV001",
            IsDelete deleted = IsDelete.NotDeleted,
            Status active = Status.Active,
            int? statusId = null)
        {
            var (daId, defaultStatusId) = await EnsureDispatchAdviceChainAsync();
            await using var ctx = _fixture.CreateFreshDbContext();

            var inv = new SalesManagement.Domain.Entities.InvoiceHeader
            {
                InvoiceNo = invoiceNo,
                InvoiceDate = DateOnly.FromDateTime(DateTime.UtcNow.Date),
                DispatchAdviceId = daId,
                PartyId = 100,
                UnitId = 1,
                FinancialYearId = 1,
                StatusId = statusId ?? defaultStatusId,
                TotalBags = 10,
                TotalWeight = 500m,
                TaxableValue = 10000m,
                InvoiceAmount = 11800m,
                IsActive = active,
                IsDeleted = deleted
            };
            await ctx.InvoiceHeader.AddAsync(inv);
            await ctx.SaveChangesAsync();
            return inv.Id;
        }

        private async Task ClearAsync() =>
            await _fixture.ClearAllTablesAsync();

        // ──────────────────────────────────────────────
        //  GetAllAsync
        // ──────────────────────────────────────────────

        [Fact]
        public async Task GetAllAsync_Should_Return_Seeded_Record()
        {
            await ClearAsync();
            await SeedInvoiceAsync("IVQ_A1");

            var (rows, total) = await CreateRepo().GetAllAsync(1, 10, null);

            rows.Should().HaveCount(1);
            total.Should().Be(1);
        }

        [Fact]
        public async Task GetAllAsync_Should_Exclude_SoftDeleted()
        {
            await ClearAsync();
            await SeedInvoiceAsync("IVQ_DEL", deleted: IsDelete.Deleted);

            var (_, total) = await CreateRepo().GetAllAsync(1, 10, null);

            total.Should().Be(0);
        }

        [Fact]
        public async Task GetAllAsync_Should_Filter_By_SearchTerm()
        {
            await ClearAsync();
            await SeedInvoiceAsync("IVQ_UNIQ");
            await SeedInvoiceAsync("IVQ_OTHR");

            var (rows, _) = await CreateRepo().GetAllAsync(1, 10, "IVQ_UNIQ");

            rows.Should().HaveCount(1);
            rows[0].InvoiceNo.Should().Be("IVQ_UNIQ");
        }

        [Fact]
        public async Task GetAllAsync_Should_Return_Pagination_Metadata()
        {
            await ClearAsync();
            await SeedInvoiceAsync("IVQ_PG1");
            await SeedInvoiceAsync("IVQ_PG2");
            await SeedInvoiceAsync("IVQ_PG3");

            var (rows, total) = await CreateRepo().GetAllAsync(1, 2, null);

            total.Should().Be(3);
            rows.Should().HaveCount(2);
        }

        // ──────────────────────────────────────────────
        //  GetByIdAsync
        // ──────────────────────────────────────────────

        [Fact]
        public async Task GetByIdAsync_Should_Return_Matching_Record()
        {
            await ClearAsync();
            var id = await SeedInvoiceAsync("IVQ_BID");

            var result = await CreateRepo().GetByIdAsync(id);

            result.Should().NotBeNull();
            result!.Id.Should().Be(id);
            result.InvoiceNo.Should().Be("IVQ_BID");
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
            var id = await SeedInvoiceAsync("IVQ_BDEL", deleted: IsDelete.Deleted);

            var result = await CreateRepo().GetByIdAsync(id);

            result.Should().BeNull();
        }

        // ──────────────────────────────────────────────
        //  NotFoundAsync
        // ──────────────────────────────────────────────

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
            var id = await SeedInvoiceAsync("IVQ_NF");

            var result = await CreateRepo().NotFoundAsync(id);

            result.Should().BeFalse();
        }

        // ──────────────────────────────────────────────
        //  AutocompleteAsync
        // ──────────────────────────────────────────────

        [Fact]
        public async Task AutocompleteAsync_Should_Return_Active_Matching()
        {
            await ClearAsync();
            await SeedInvoiceAsync("IVQ_AUTO");

            var result = await CreateRepo().AutocompleteAsync("IVQ_AUTO", CancellationToken.None);

            result.Should().HaveCount(1);
            result[0].InvoiceNo.Should().Be("IVQ_AUTO");
        }

        [Fact]
        public async Task AutocompleteAsync_Should_Exclude_Inactive()
        {
            await ClearAsync();
            await SeedInvoiceAsync("IVQ_INAC", active: Status.Inactive);

            var result = await CreateRepo().AutocompleteAsync("IVQ_INAC", CancellationToken.None);

            result.Should().BeEmpty();
        }

        // ──────────────────────────────────────────────
        //  DispatchAdviceExistsAsync
        // ──────────────────────────────────────────────

        [Fact]
        public async Task DispatchAdviceExistsAsync_Should_Return_True_When_Exists()
        {
            var (daId, _) = await EnsureDispatchAdviceChainAsync();

            var result = await CreateRepo().DispatchAdviceExistsAsync(daId);

            result.Should().BeTrue();
        }

        [Fact]
        public async Task DispatchAdviceExistsAsync_Should_Return_False_When_Missing()
        {
            var result = await CreateRepo().DispatchAdviceExistsAsync(9999999);

            result.Should().BeFalse();
        }

        // ──────────────────────────────────────────────
        //  IsAlreadyInvoicedAsync
        // ──────────────────────────────────────────────

        [Fact]
        public async Task IsAlreadyInvoicedAsync_Should_Return_True_When_Invoice_Exists()
        {
            await ClearAsync();
            await SeedInvoiceAsync("IVQ_INVD");

            // The seeded invoice references the DA created by EnsureDispatchAdviceChainAsync
            var (daId, _) = await EnsureDispatchAdviceChainAsync();

            var result = await CreateRepo().IsAlreadyInvoicedAsync(daId);

            result.Should().BeTrue();
        }

        [Fact]
        public async Task IsAlreadyInvoicedAsync_Should_Return_False_When_No_Invoice()
        {
            var result = await CreateRepo().IsAlreadyInvoicedAsync(9999999);

            result.Should().BeFalse();
        }

        // ──────────────────────────────────────────────
        //  GetDispatchAdviceDateAsync
        // ──────────────────────────────────────────────

        [Fact]
        public async Task GetDispatchAdviceDateAsync_Should_Return_DispatchDate()
        {
            var (daId, _) = await EnsureDispatchAdviceChainAsync();

            var result = await CreateRepo().GetDispatchAdviceDateAsync(daId);

            result.Should().Be(DateOnly.FromDateTime(DateTime.UtcNow.Date));
        }
    }
}
