using Contracts.Dtos.Lookups.Finance;
using Contracts.Dtos.Lookups.Inventory;
using Contracts.Dtos.Lookups.Party;
using Contracts.Dtos.Lookups.Production;
using Contracts.Dtos.Lookups.Purchase;
using Contracts.Dtos.Lookups.Users;
using Contracts.Interfaces;
using Contracts.Interfaces.Lookups.Finance;
using Contracts.Interfaces.Lookups.Inventory;
using Contracts.Interfaces.Lookups.Party;
using Contracts.Interfaces.Lookups.Production;
using Contracts.Interfaces.Lookups.Purchase;
using Contracts.Interfaces.Lookups.Users;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using SalesManagement.Application.Common.Interfaces;
using SalesManagement.Infrastructure.Data;
using SalesManagement.Infrastructure.Repositories.SalesOrder;
using SalesManagement.IntegrationTests.Common;
using static SalesManagement.Domain.Common.BaseEntity;

namespace SalesManagement.IntegrationTests.Repositories.SalesOrder
{
    [Collection("DatabaseCollection")]
    public sealed class SalesOrderQueryRepositoryTests
    {
        private readonly DbFixture _fixture;

        public SalesOrderQueryRepositoryTests(DbFixture fixture) => _fixture = fixture;

        // ── Mock factory ──────────────────────────────────────────────

        private SalesOrderQueryRepository CreateRepo()
        {
            var unit = new Mock<IUnitLookup>(MockBehavior.Loose);
            unit.Setup(u => u.GetByIdsAsync(It.IsAny<IEnumerable<int>>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync((IEnumerable<int> ids, CancellationToken _) =>
                    (IReadOnlyList<UnitLookupDto>)ids.Select(id =>
                        new UnitLookupDto { UnitId = id, UnitName = "Unit " + id }).ToList());
            unit.Setup(u => u.GetByIdAsync(It.IsAny<int>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync((int id, CancellationToken _) =>
                    new UnitLookupDto { UnitId = id, UnitName = "Unit " + id });
            unit.Setup(u => u.GetAllUnitAsync())
                .ReturnsAsync(new List<UnitLookupDto>
                {
                    new UnitLookupDto { UnitId = 1, UnitName = "Unit 1" }
                });

            var party = new Mock<IPartyLookup>(MockBehavior.Loose);
            party.Setup(p => p.GetByIdsAsync(It.IsAny<IEnumerable<int>>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync((IEnumerable<int> ids, CancellationToken _) =>
                    (IReadOnlyList<PartyLookupDto>)ids.Select(id =>
                        new PartyLookupDto { Id = id, PartyName = "Party " + id }).ToList());

            var paymentTerm = new Mock<IPaymentTermLookup>(MockBehavior.Loose);
            paymentTerm.Setup(p => p.GetAllPaymentTermAsync())
                .ReturnsAsync(new List<PaymentTermLookupDto>());

            var item = new Mock<IItemLookup>(MockBehavior.Loose);
            item.Setup(i => i.GetByIdsAsync(It.IsAny<IEnumerable<int>>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync((IReadOnlyList<ItemLookupDto>)new List<ItemLookupDto>());

            var hsn = new Mock<IHSNLookup>(MockBehavior.Loose);
            hsn.Setup(h => h.GetByIdsAsync(It.IsAny<IEnumerable<int>>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync((IReadOnlyList<HSNLookupDto>)new List<HSNLookupDto>());

            var uom = new Mock<IUOMLookup>(MockBehavior.Loose);
            uom.Setup(u => u.GetByIdsAsync(It.IsAny<IEnumerable<int>>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync((IReadOnlyList<UOMLookupDto>)new List<UOMLookupDto>());

            var ip = new Mock<IIPAddressService>(MockBehavior.Loose);
            ip.Setup(x => x.GetUserId()).Returns(1);
            ip.Setup(x => x.GetUnitId()).Returns(1);
            ip.Setup(x => x.GetCompanyId()).Returns(1);

            var company = new Mock<ICompanyLookup>(MockBehavior.Loose);
            company.Setup(c => c.GetAllCompanyAsync())
                .ReturnsAsync(new List<CompanyLookupDto>
                {
                    new CompanyLookupDto { CompanyId = 1, CompanyName = "Test Company" }
                });

            var packType = new Mock<IPackTypeLookup>(MockBehavior.Loose);
            packType.Setup(p => p.GetByIdsAsync(It.IsAny<IEnumerable<int>>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync((IReadOnlyList<PackTypeLookupDto>)new List<PackTypeLookupDto>());

            var txnType = new Mock<ITransactionTypeLookup>(MockBehavior.Loose);
            txnType.Setup(t => t.GetByIdsAsync(It.IsAny<IEnumerable<int>>()))
                .ReturnsAsync((IReadOnlyList<TransactionTypeLookupDto>)new List<TransactionTypeLookupDto>());

            var accessFilter = new Mock<IMarketingOfficerAccessFilter>(MockBehavior.Loose);
            accessFilter.Setup(a => a.IsMarketingOfficer()).Returns(false);

            var division = new Mock<IDivisionLookup>(MockBehavior.Loose);

            return new SalesOrderQueryRepository(
                new SqlConnection(_fixture.ConnectionString),
                unit.Object, party.Object, paymentTerm.Object,
                item.Object, hsn.Object, uom.Object,
                ip.Object, company.Object, packType.Object,
                txnType.Object, accessFilter.Object, division.Object);
        }

        // ── Seed helpers ──────────────────────────────────────────────

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
        /// Seeds the full SalesGroup → SalesOffice → SalesOrganisation chain plus required MiscMaster
        /// rows and returns IDs needed to create a SalesOrderHeader.
        /// </summary>
        private async Task<(int salesGroupId, int enquiryTypeId, int freightTypeId, int statusId)>
            EnsureDependenciesAsync()
        {
            await using var ctx = _fixture.CreateFreshDbContext();

            // MiscTypeMaster
            var mt = await ctx.MiscTypeMaster.FirstOrDefaultAsync(x => x.MiscTypeCode == "SOQM");
            if (mt == null)
            {
                mt = new SalesManagement.Domain.Entities.MiscTypeMaster
                {
                    MiscTypeCode = "SOQM", Description = "SO Query Misc",
                    IsActive = Status.Active, IsDeleted = IsDelete.NotDeleted
                };
                await ctx.MiscTypeMaster.AddAsync(mt);
                await ctx.SaveChangesAsync();
            }

            var enquiryTypeId = await EnsureMiscAsync(ctx, mt.Id, "SOQE");
            var freightTypeId = await EnsureMiscAsync(ctx, mt.Id, "SOQF");
            var statusId = await EnsureMiscAsync(ctx, mt.Id, "SOQS");

            // SalesOrganisation → SalesOffice → SalesGroup
            var org = await ctx.SalesOrganisation.FirstOrDefaultAsync(x => x.SalesOrganisationCode == "SOQORG");
            if (org == null)
            {
                org = new SalesManagement.Domain.Entities.SalesOrganisation
                {
                    SalesOrganisationCode = "SOQORG", SalesOrganisationName = "SOQ Org",
                    CompanyId = 1, IsActive = Status.Active, IsDeleted = IsDelete.NotDeleted
                };
                await ctx.SalesOrganisation.AddAsync(org);
                await ctx.SaveChangesAsync();
            }

            var office = await ctx.SalesOffice.FirstOrDefaultAsync(x => x.SalesOfficeName == "SOQ_OFC");
            if (office == null)
            {
                office = new SalesManagement.Domain.Entities.SalesOffice
                {
                    SalesOfficeName = "SOQ_OFC", SalesOrganisationId = org.Id,
                    IsActive = Status.Active, IsDeleted = IsDelete.NotDeleted
                };
                await ctx.SalesOffice.AddAsync(office);
                await ctx.SaveChangesAsync();
            }

            var sg = await ctx.SalesGroup.FirstOrDefaultAsync(x => x.SalesGroupName == "SOQ_SG");
            if (sg == null)
            {
                sg = new SalesManagement.Domain.Entities.SalesGroup
                {
                    SalesGroupName = "SOQ_SG", SalesOfficeId = office.Id,
                    IsActive = Status.Active, IsDeleted = IsDelete.NotDeleted
                };
                await ctx.SalesGroup.AddAsync(sg);
                await ctx.SaveChangesAsync();
            }

            return (sg.Id, enquiryTypeId, freightTypeId, statusId);
        }

        private async Task<int> SeedAsync(
            string salesOrderNo = "SOQ_01",
            IsDelete deleted = IsDelete.NotDeleted,
            Status active = Status.Active,
            int? orderUnitId = 1)
        {
            var (sgId, enqId, freightId, statusId) = await EnsureDependenciesAsync();
            await using var ctx = _fixture.CreateFreshDbContext();

            var so = new SalesManagement.Domain.Entities.SalesOrderHeader
            {
                SalesOrderNo = salesOrderNo,
                OrderDate = DateOnly.FromDateTime(DateTime.UtcNow.Date),
                SalesGroupId = sgId,
                EnquiryType = enqId,
                UnitId = 1,
                PartyId = 100,
                FreightTypeId = freightId,
                StatusId = statusId,
                OrderUnitId = orderUnitId,
                FinalAmount = 5000m,
                IsActive = active,
                IsDeleted = deleted
            };
            await ctx.SalesOrderHeader.AddAsync(so);
            await ctx.SaveChangesAsync();
            return so.Id;
        }

        private async Task ClearAsync() =>
            await _fixture.ClearAllTablesAsync();

        // ── GetAllAsync ───────────────────────────────────────────────

        [Fact]
        public async Task GetAllAsync_Should_Return_Seeded_Record()
        {
            await ClearAsync();
            await SeedAsync("SOQ_GA1");

            var (rows, total) = await CreateRepo().GetAllAsync(1, 10, null);

            rows.Should().HaveCount(1);
            total.Should().Be(1);
        }

        [Fact]
        public async Task GetAllAsync_Should_Exclude_SoftDeleted()
        {
            await ClearAsync();
            await SeedAsync("SOQ_DEL", deleted: IsDelete.Deleted);

            var (_, total) = await CreateRepo().GetAllAsync(1, 10, null);

            total.Should().Be(0);
        }

        [Fact]
        public async Task GetAllAsync_Should_Filter_By_SearchTerm()
        {
            await ClearAsync();
            await SeedAsync("SOQ_ALPHA");
            await SeedAsync("SOQ_BETA");

            var (rows, _) = await CreateRepo().GetAllAsync(1, 10, "SOQ_ALPHA");

            rows.Should().HaveCount(1);
            rows[0].SalesOrderNo.Should().Be("SOQ_ALPHA");
        }

        [Fact]
        public async Task GetAllAsync_Should_Respect_Pagination()
        {
            await ClearAsync();
            await SeedAsync("SOQ_PG1");
            await SeedAsync("SOQ_PG2");
            await SeedAsync("SOQ_PG3");

            var (rows, total) = await CreateRepo().GetAllAsync(1, 2, null);

            rows.Should().HaveCount(2);
            total.Should().Be(3);
        }

        // ── GetByIdAsync ──────────────────────────────────────────────

        [Fact]
        public async Task GetByIdAsync_Should_Return_Matching_Record()
        {
            await ClearAsync();
            var id = await SeedAsync("SOQ_GBI");

            var result = await CreateRepo().GetByIdAsync(id);

            result.Should().NotBeNull();
            result!.Id.Should().Be(id);
            result.SalesOrderNo.Should().Be("SOQ_GBI");
        }

        [Fact]
        public async Task GetByIdAsync_Should_Return_Null_When_SoftDeleted()
        {
            await ClearAsync();
            var id = await SeedAsync("SOQ_GBIDEL", deleted: IsDelete.Deleted);

            var result = await CreateRepo().GetByIdAsync(id);

            result.Should().BeNull();
        }

        [Fact]
        public async Task GetByIdAsync_Should_Return_Null_When_NotFound()
        {
            var result = await CreateRepo().GetByIdAsync(9999999);

            result.Should().BeNull();
        }

        // ── NotFoundAsync ─────────────────────────────────────────────

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
            var id = await SeedAsync("SOQ_NF1");

            var result = await CreateRepo().NotFoundAsync(id);

            result.Should().BeFalse();
        }

        // ── SalesGroupExistsAsync ─────────────────────────────────────

        [Fact]
        public async Task SalesGroupExistsAsync_Should_Return_True_When_Active()
        {
            var (sgId, _, _, _) = await EnsureDependenciesAsync();

            var result = await CreateRepo().SalesGroupExistsAsync(sgId);

            result.Should().BeTrue();
        }

        [Fact]
        public async Task SalesGroupExistsAsync_Should_Return_False_When_Missing()
        {
            var result = await CreateRepo().SalesGroupExistsAsync(9999999);

            result.Should().BeFalse();
        }

        // ── MiscMasterExistsAsync ─────────────────────────────────────

        [Fact]
        public async Task MiscMasterExistsAsync_Should_Return_True_For_Seeded()
        {
            var (_, enqId, _, _) = await EnsureDependenciesAsync();

            var result = await CreateRepo().MiscMasterExistsAsync(enqId);

            result.Should().BeTrue();
        }

        [Fact]
        public async Task MiscMasterExistsAsync_Should_Return_False_When_Missing()
        {
            var result = await CreateRepo().MiscMasterExistsAsync(9999999);

            result.Should().BeFalse();
        }

        // ── HasDispatchAdviceAsync ────────────────────────────────────

        [Fact]
        public async Task HasDispatchAdviceAsync_Should_Return_False_When_None()
        {
            await ClearAsync();
            var id = await SeedAsync("SOQ_NODA");

            var result = await CreateRepo().HasDispatchAdviceAsync(id);

            result.Should().BeFalse();
        }
    }
}
