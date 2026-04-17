using Contracts.Dtos.Lookups.Inventory;
using Contracts.Dtos.Lookups.Party;
using Contracts.Dtos.Lookups.Production;
using Contracts.Dtos.Lookups.Warehouse;
using Contracts.Interfaces.Lookups.Inventory;
using Contracts.Interfaces.Lookups.Party;
using Contracts.Interfaces.Lookups.Production;
using Contracts.Interfaces.Lookups.Warehouse;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using SalesManagement.Infrastructure.Data;
using SalesManagement.Infrastructure.Repositories.SalesReturn;
using SalesManagement.IntegrationTests.Common;
using static SalesManagement.Domain.Common.BaseEntity;

namespace SalesManagement.IntegrationTests.Repositories.SalesReturn
{
    // Note: SalesReturnDetail has FKs to InvoiceHeader/InvoiceDetail. Tests here cover
    // header-level methods that don't require the full Invoice chain; pack-range/detail
    // methods require upstream Invoice fixtures and are deferred.
    [Collection("DatabaseCollection")]
    public sealed class SalesReturnQueryRepositoryTests
    {
        private readonly DbFixture _fixture;
        public SalesReturnQueryRepositoryTests(DbFixture fixture) => _fixture = fixture;

        private SalesReturnQueryRepository CreateRepo(
            Mock<IPartyLookup>? partyLookup = null,
            Mock<IItemLookup>? itemLookup = null,
            Mock<ILotMasterLookup>? lotLookup = null,
            Mock<IWarehouseLookup>? warehouseLookup = null,
            Mock<IBinLookup>? binLookup = null)
        {
            if (partyLookup == null)
            {
                partyLookup = new Mock<IPartyLookup>(MockBehavior.Loose);
                partyLookup.Setup(p => p.GetByIdsAsync(It.IsAny<IEnumerable<int>>(), It.IsAny<CancellationToken>()))
                    .ReturnsAsync((IEnumerable<int> ids, CancellationToken _) =>
                        (IReadOnlyList<PartyLookupDto>)ids.Select(id =>
                            new PartyLookupDto { Id = id, PartyName = "Party " + id }).ToList());
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
                    .ReturnsAsync((IReadOnlyList<LotMasterLookupDto>)new List<LotMasterLookupDto>());
            }
            if (warehouseLookup == null)
            {
                warehouseLookup = new Mock<IWarehouseLookup>(MockBehavior.Loose);
                warehouseLookup.Setup(w => w.GetByIdsAsync(It.IsAny<IEnumerable<int>>(), It.IsAny<CancellationToken>()))
                    .ReturnsAsync((IEnumerable<int> ids, CancellationToken _) =>
                        (IReadOnlyList<WarehouseLookupDto>)ids.Select(id =>
                            new WarehouseLookupDto { Id = id, WarehouseName = "WH " + id }).ToList());
            }
            if (binLookup == null)
            {
                binLookup = new Mock<IBinLookup>(MockBehavior.Loose);
                binLookup.Setup(b => b.GetByIdsAsync(It.IsAny<IEnumerable<int>>(), It.IsAny<CancellationToken>()))
                    .ReturnsAsync((IReadOnlyList<BinLookupDto>)new List<BinLookupDto>());
            }

            return new SalesReturnQueryRepository(
                new SqlConnection(_fixture.ConnectionString),
                partyLookup.Object, itemLookup.Object, lotLookup.Object,
                warehouseLookup.Object, binLookup.Object);
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

        private async Task<(int complaintStatusId, int returnStatusId, int complaintId)> EnsureComplaintAsync(string complaintNumber = "SRQ_CMP1")
        {
            await using var ctx = _fixture.CreateFreshDbContext();

            var mt = await ctx.MiscTypeMaster.FirstOrDefaultAsync(x => x.MiscTypeCode == "SRQ_MT");
            if (mt == null)
            {
                mt = new SalesManagement.Domain.Entities.MiscTypeMaster
                {
                    MiscTypeCode = "SRQ_MT", Description = "Type",
                    IsActive = Status.Active, IsDeleted = IsDelete.NotDeleted
                };
                await ctx.MiscTypeMaster.AddAsync(mt);
                await ctx.SaveChangesAsync();
            }
            var complaintStatusId = await EnsureMiscAsync(ctx, mt.Id, "SRQ_CS");
            var returnStatusId = await EnsureMiscAsync(ctx, mt.Id, "SRQ_RS");

            var complaint = await ctx.ComplaintHeader.FirstOrDefaultAsync(x => x.ComplaintNumber == complaintNumber);
            if (complaint == null)
            {
                complaint = new SalesManagement.Domain.Entities.ComplaintHeader
                {
                    ComplaintNumber = complaintNumber,
                    ComplaintDate = DateOnly.FromDateTime(DateTime.UtcNow.Date),
                    CustomerId = 100,
                    StatusId = complaintStatusId,
                    IsActive = Status.Active, IsDeleted = IsDelete.NotDeleted
                };
                await ctx.ComplaintHeader.AddAsync(complaint);
                await ctx.SaveChangesAsync();
            }
            return (complaintStatusId, returnStatusId, complaint.Id);
        }

        private async Task<int> SeedReturnAsync(string returnNumber = "SR_Q1", IsDelete deleted = IsDelete.NotDeleted, Status active = Status.Active)
        {
            var (_, returnStatusId, complaintId) = await EnsureComplaintAsync();
            await using var ctx = _fixture.CreateFreshDbContext();
            var sr = new SalesManagement.Domain.Entities.SalesReturnHeader
            {
                ReturnNumber = returnNumber,
                ReturnDate = DateOnly.FromDateTime(DateTime.UtcNow.Date),
                ComplaintHeaderId = complaintId,
                CustomerId = 100,
                WarehouseId = 1,
                BinId = 1,
                StatusId = returnStatusId,
                Remarks = "test",
                IsActive = active, IsDeleted = deleted
            };
            await ctx.SalesReturnHeader.AddAsync(sr);
            await ctx.SaveChangesAsync();
            return sr.Id;
        }

        private async Task ClearAsync() =>
            await _fixture.ClearAllTablesAsync();

        [Fact]
        public async Task GetAllAsync_Should_Return_Seeded()
        {
            await ClearAsync();
            await SeedReturnAsync("SR_A1");

            var (rows, total) = await CreateRepo().GetAllAsync(1, 10, null);

            rows.Should().HaveCount(1);
            total.Should().Be(1);
        }

        [Fact]
        public async Task GetAllAsync_Should_Exclude_SoftDeleted()
        {
            await ClearAsync();
            await SeedReturnAsync("SR_DEL", deleted: IsDelete.Deleted);

            var (_, total) = await CreateRepo().GetAllAsync(1, 10, null);

            total.Should().Be(0);
        }

        [Fact]
        public async Task GetAllAsync_Should_Filter_By_SearchTerm()
        {
            await ClearAsync();
            await SeedReturnAsync("UNIQUE_SR_A");
            await SeedReturnAsync("OTHER_SR_B");

            var (rows, _) = await CreateRepo().GetAllAsync(1, 10, "UNIQUE_SR");

            rows.Should().HaveCount(1);
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
            var id = await SeedReturnAsync("SR_NF");

            var result = await CreateRepo().NotFoundAsync(id);

            result.Should().BeFalse();
        }

        [Fact]
        public async Task ComplaintExistsAsync_Should_Return_True()
        {
            var (_, _, complaintId) = await EnsureComplaintAsync();

            var result = await CreateRepo().ComplaintExistsAsync(complaintId);

            result.Should().BeTrue();
        }

        [Fact]
        public async Task ComplaintExistsAsync_Should_Return_False_When_NotFound()
        {
            var result = await CreateRepo().ComplaintExistsAsync(9999999);
            result.Should().BeFalse();
        }

        [Fact]
        public async Task PackRangeOverlapsAsync_Should_Return_False_When_No_Details_Exist()
        {
            var result = await CreateRepo().PackRangeOverlapsAsync(1, 1, 10);
            result.Should().BeFalse();
        }

        [Fact]
        public async Task GetReturnProgressAsync_Should_Return_Zero_When_No_Data()
        {
            var (_, _, complaintId) = await EnsureComplaintAsync("SR_PROG_CMP");

            var (totalDispatched, totalReturned) = await CreateRepo().GetReturnProgressAsync(complaintId);

            totalDispatched.Should().Be(0);
            totalReturned.Should().Be(0);
        }

        [Fact]
        public async Task IsComplaintReturnEligibleAsync_Should_Return_False_When_No_Resolution()
        {
            var (_, _, complaintId) = await EnsureComplaintAsync("SR_ELIG_CMP");

            var result = await CreateRepo().IsComplaintReturnEligibleAsync(complaintId);

            result.Should().BeFalse();
        }
    }
}
