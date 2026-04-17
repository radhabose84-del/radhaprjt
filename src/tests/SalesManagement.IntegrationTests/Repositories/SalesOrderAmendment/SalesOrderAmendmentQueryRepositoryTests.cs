using Contracts.Dtos.Lookups.Inventory;
using Contracts.Interfaces.Lookups.Inventory;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using SalesManagement.Infrastructure.Data;
using SalesManagement.Infrastructure.Repositories.SalesOrderAmendment;
using SalesManagement.IntegrationTests.Common;
using static SalesManagement.Domain.Common.BaseEntity;

namespace SalesManagement.IntegrationTests.Repositories.SalesOrderAmendment
{
    [Collection("DatabaseCollection")]
    public sealed class SalesOrderAmendmentQueryRepositoryTests
    {
        private readonly DbFixture _fixture;
        public SalesOrderAmendmentQueryRepositoryTests(DbFixture fixture) => _fixture = fixture;

        private SalesOrderAmendmentQueryRepository CreateRepo(Mock<IItemLookup>? item = null)
        {
            if (item == null)
            {
                item = new Mock<IItemLookup>(MockBehavior.Loose);
                item.Setup(i => i.GetByIdsAsync(It.IsAny<IEnumerable<int>>(), It.IsAny<CancellationToken>()))
                    .ReturnsAsync((IEnumerable<int> ids, CancellationToken _) =>
                        (IReadOnlyList<ItemLookupDto>)ids.Select(id =>
                            new ItemLookupDto { Id = id, ItemCode = "I" + id, ItemName = "Item " + id }).ToList());
            }
            return new SalesOrderAmendmentQueryRepository(new SqlConnection(_fixture.ConnectionString), item.Object);
        }

        private async Task<(int pendingStatusId, int approvedStatusId)> EnsureApprovalStatusMiscAsync()
        {
            await using var ctx = _fixture.CreateFreshDbContext();
            var mt = await ctx.MiscTypeMaster.FirstOrDefaultAsync(x => x.MiscTypeCode == "ApprovalStatus");
            if (mt == null)
            {
                mt = new SalesManagement.Domain.Entities.MiscTypeMaster
                {
                    MiscTypeCode = "ApprovalStatus", Description = "Approval Status",
                    IsActive = Status.Active, IsDeleted = IsDelete.NotDeleted
                };
                await ctx.MiscTypeMaster.AddAsync(mt);
                await ctx.SaveChangesAsync();
            }
            var pending = await EnsureMiscAsync(ctx, mt.Id, "Pending");
            var approved = await EnsureMiscAsync(ctx, mt.Id, "Approved");
            return (pending, approved);
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

        private async Task<(int soId, int soDetailId, int freightId, int enquiryId)> EnsureSalesOrderAsync()
        {
            await using var ctx = _fixture.CreateFreshDbContext();

            var aux = await ctx.MiscTypeMaster.FirstOrDefaultAsync(x => x.MiscTypeCode == "SOAQ_AUX");
            if (aux == null)
            {
                aux = new SalesManagement.Domain.Entities.MiscTypeMaster
                {
                    MiscTypeCode = "SOAQ_AUX", Description = "Aux",
                    IsActive = Status.Active, IsDeleted = IsDelete.NotDeleted
                };
                await ctx.MiscTypeMaster.AddAsync(aux);
                await ctx.SaveChangesAsync();
            }
            var freightId = await EnsureMiscAsync(ctx, aux.Id, "SOAQ_FT");
            var enquiryId = await EnsureMiscAsync(ctx, aux.Id, "SOAQ_ENQ");

            var org = await ctx.SalesOrganisation.FirstOrDefaultAsync(x => x.SalesOrganisationCode == "SOAQ_ORG");
            if (org == null)
            {
                org = new SalesManagement.Domain.Entities.SalesOrganisation
                {
                    SalesOrganisationCode = "SOAQ_ORG", SalesOrganisationName = "Org",
                    CompanyId = 1, IsActive = Status.Active, IsDeleted = IsDelete.NotDeleted
                };
                await ctx.SalesOrganisation.AddAsync(org);
                await ctx.SaveChangesAsync();
            }
            var office = await ctx.SalesOffice.FirstOrDefaultAsync(x => x.SalesOfficeName == "SOAQ_OFC");
            if (office == null)
            {
                office = new SalesManagement.Domain.Entities.SalesOffice
                {
                    SalesOfficeName = "SOAQ_OFC", SalesOrganisationId = org.Id,
                    IsActive = Status.Active, IsDeleted = IsDelete.NotDeleted
                };
                await ctx.SalesOffice.AddAsync(office);
                await ctx.SaveChangesAsync();
            }
            var sg = await ctx.SalesGroup.FirstOrDefaultAsync(x => x.SalesGroupName == "SOAQ_SG");
            if (sg == null)
            {
                sg = new SalesManagement.Domain.Entities.SalesGroup
                {
                    SalesGroupName = "SOAQ_SG", SalesOfficeId = office.Id,
                    IsActive = Status.Active, IsDeleted = IsDelete.NotDeleted
                };
                await ctx.SalesGroup.AddAsync(sg);
                await ctx.SaveChangesAsync();
            }

            var so = await ctx.SalesOrderHeader
                .Include(h => h.SalesOrderDetails)
                .FirstOrDefaultAsync(x => x.SalesOrderNo == "SOAQ_SO1");
            if (so == null)
            {
                so = new SalesManagement.Domain.Entities.SalesOrderHeader
                {
                    SalesOrderNo = "SOAQ_SO1",
                    OrderDate = DateOnly.FromDateTime(DateTime.UtcNow.Date),
                    SalesGroupId = sg.Id,
                    EnquiryType = enquiryId,
                    UnitId = 1,
                    PartyId = 100,
                    FreightTypeId = freightId,
                    FinalAmount = 1000m,
                    IsActive = Status.Active, IsDeleted = IsDelete.NotDeleted,
                    SalesOrderDetails = new List<SalesManagement.Domain.Entities.SalesOrderDetail>
                    {
                        new()
                        {
                            ItemId = 10, HSNId = 1,
                            QtyInBags = 20, BagWeight = 50m, SaleUOMId = 1, TotalWeight = 1000m,
                            ExMillRate = 10m, DiscountPerUnit = 0m, Freight = 0m,
                            TaxableAmount = 100m, TaxPercentage = 5m, TaxAmount = 5m,
                            TCSPercentage = 0m, TCSAmount = 0m,
                            NetAmount = 105m, NetRatePerKg = 10.5m,
                            ExpectedDeliveryDate = DateOnly.FromDateTime(DateTime.UtcNow.Date.AddDays(30)),
                            PendingQty = 20
                        }
                    }
                };
                await ctx.SalesOrderHeader.AddAsync(so);
                await ctx.SaveChangesAsync();
            }
            return (so.Id, so.SalesOrderDetails!.First().Id, freightId, enquiryId);
        }

        private async Task<int> SeedAmendmentAsync(string amendmentNo = "AM_Q1", string changeType = "Modified", int? statusId = null, IsDelete deleted = IsDelete.NotDeleted)
        {
            var (soId, soDetailId, _, _) = await EnsureSalesOrderAsync();
            var (pending, _) = await EnsureApprovalStatusMiscAsync();
            await using var ctx = _fixture.CreateFreshDbContext();
            var h = new SalesManagement.Domain.Entities.SalesOrderAmendmentHeader
            {
                SalesOrderHeaderId = soId,
                UnitId = 1,
                AmendmentNo = amendmentNo,
                RevisionNumber = 1,
                AmendmentDate = DateOnly.FromDateTime(DateTime.UtcNow.Date),
                Reason = "qty change",
                StatusId = statusId ?? pending,
                TotalBags = 25, TotalWeightKgs = 1250m, TotalDiscountPerKg = 0m,
                ItemValue = 125m, TotalFreight = 0m, TaxableAmount = 125m,
                GSTPercentage = 5m, TotalGST = 6.25m, TotalWithGST = 131.25m,
                TCSPercentage = 0m, TotalTCS = 0m, FinalAmount = 131.25m,
                IsActive = Status.Active, IsDeleted = deleted,
                SalesOrderAmendmentDetails = new List<SalesManagement.Domain.Entities.SalesOrderAmendmentDetail>
                {
                    new()
                    {
                        ChangeType = changeType,
                        SalesOrderDetailId = soDetailId,
                        OldItemId = 10, OldQtyInBags = 20, OldExMillRate = 10m,
                        OldExpectedDeliveryDate = DateOnly.FromDateTime(DateTime.UtcNow.Date.AddDays(30)),
                        NewQtyInBags = changeType == "Modified" ? 25 : null,
                        NewExMillRate = changeType == "Modified" ? 11m : null,
                        NewExpectedDeliveryDate = changeType == "Modified" ? DateOnly.FromDateTime(DateTime.UtcNow.Date.AddDays(35)) : null,
                        TaxableAmount = 125m, TaxAmount = 6.25m, TCSAmount = 0m,
                        NetAmount = 131.25m, NetRatePerKg = 10.5m,
                        PendingQty = 25
                    }
                }
            };
            await ctx.SalesOrderAmendmentHeader.AddAsync(h);
            await ctx.SaveChangesAsync();
            return h.Id;
        }

        private async Task ClearAsync() =>
            await _fixture.ClearAllTablesAsync();

        [Fact]
        public async Task GetAllAsync_Should_Return_Seeded()
        {
            await ClearAsync();
            await SeedAmendmentAsync("AM_A1");

            var (rows, total) = await CreateRepo().GetAllAsync(1, 10, null);

            rows.Should().HaveCount(1);
            total.Should().Be(1);
        }

        [Fact]
        public async Task GetAllAsync_Should_Filter_By_SearchTerm()
        {
            await ClearAsync();
            await SeedAmendmentAsync("AM_UNIQZZ");
            await SeedAmendmentAsync("AM_OTHER");

            var (rows, _) = await CreateRepo().GetAllAsync(1, 10, "AM_UNIQZZ");

            rows.Should().HaveCount(1);
            rows[0].AmendmentNo.Should().Be("AM_UNIQZZ");
        }

        [Fact]
        public async Task GetAllAsync_Should_Exclude_SoftDeleted()
        {
            await ClearAsync();
            await SeedAmendmentAsync("AM_DEL", deleted: IsDelete.Deleted);

            var (_, total) = await CreateRepo().GetAllAsync(1, 10, null);

            total.Should().Be(0);
        }

        [Fact]
        public async Task GetByIdAsync_Should_Return_With_Details()
        {
            await ClearAsync();
            var id = await SeedAmendmentAsync("AM_B1");

            var result = await CreateRepo().GetByIdAsync(id);

            result.Should().NotBeNull();
            result!.SalesOrderAmendmentDetails.Should().HaveCount(1);
        }

        [Fact]
        public async Task GetByIdAsync_Should_Populate_OldItemName_From_Lookup()
        {
            await ClearAsync();
            var id = await SeedAmendmentAsync("AM_B2");

            var result = await CreateRepo().GetByIdAsync(id);

            result!.SalesOrderAmendmentDetails[0].OldItemName.Should().Be("Item 10");
        }

        [Fact]
        public async Task GetByIdAsync_Should_Populate_Remarks_For_Modified()
        {
            await ClearAsync();
            var id = await SeedAmendmentAsync("AM_MOD", changeType: "Modified");

            var result = await CreateRepo().GetByIdAsync(id);

            result!.SalesOrderAmendmentDetails[0].Remarks.Should().Contain("Qty Amendment");
        }

        [Fact]
        public async Task GetByIdAsync_Should_Populate_Remarks_For_Removed()
        {
            await ClearAsync();
            var id = await SeedAmendmentAsync("AM_REM", changeType: "Removed");

            var result = await CreateRepo().GetByIdAsync(id);

            result!.SalesOrderAmendmentDetails[0].Remarks.Should().Be("Item Removed");
        }

        [Fact]
        public async Task GetByIdAsync_Should_Return_Null_When_NotFound()
        {
            var result = await CreateRepo().GetByIdAsync(9999999);
            result.Should().BeNull();
        }

        [Fact]
        public async Task GetBySalesOrderHeaderIdAsync_Should_Return_All_For_SO()
        {
            await ClearAsync();
            await SeedAmendmentAsync("AM_SO_A");
            await SeedAmendmentAsync("AM_SO_B");
            var (soId, _, _, _) = await EnsureSalesOrderAsync();

            var result = await CreateRepo().GetBySalesOrderHeaderIdAsync(soId);

            result.Should().HaveCount(2);
        }

        [Fact]
        public async Task GetPendingAsync_Should_Return_Only_Pending()
        {
            await ClearAsync();
            var (pending, approved) = await EnsureApprovalStatusMiscAsync();
            await SeedAmendmentAsync("AM_P1", statusId: pending);
            await SeedAmendmentAsync("AM_P2", statusId: approved);

            var (rows, total) = await CreateRepo().GetPendingAsync(1, 10, null);

            rows.Should().HaveCount(1);
            rows[0].AmendmentNo.Should().Be("AM_P1");
            total.Should().Be(1);
        }

        [Fact]
        public async Task SalesOrderExistsAndApprovedAsync_Should_Return_False_When_Pending()
        {
            var (soId, _, _, _) = await EnsureSalesOrderAsync();

            var result = await CreateRepo().SalesOrderExistsAndApprovedAsync(soId);

            result.Should().BeFalse();
        }

        [Fact]
        public async Task HasDispatchAdviceAsync_Should_Return_False_When_None()
        {
            var (soId, _, _, _) = await EnsureSalesOrderAsync();

            var result = await CreateRepo().HasDispatchAdviceAsync(soId);

            result.Should().BeFalse();
        }

        [Fact]
        public async Task HasPendingAmendmentAsync_Should_Return_True_When_Pending()
        {
            await ClearAsync();
            await SeedAmendmentAsync("AM_PEND");
            var (soId, _, _, _) = await EnsureSalesOrderAsync();

            var result = await CreateRepo().HasPendingAmendmentAsync(soId);

            result.Should().BeTrue();
        }

        [Fact]
        public async Task HasPendingAmendmentAsync_Should_Return_False_When_Approved()
        {
            await ClearAsync();
            var (_, approved) = await EnsureApprovalStatusMiscAsync();
            await SeedAmendmentAsync("AM_AP", statusId: approved);
            var (soId, _, _, _) = await EnsureSalesOrderAsync();

            var result = await CreateRepo().HasPendingAmendmentAsync(soId);

            result.Should().BeFalse();
        }

        [Fact]
        public async Task SalesOrderDetailExistsAsync_Should_Return_True()
        {
            var (soId, soDetailId, _, _) = await EnsureSalesOrderAsync();

            var result = await CreateRepo().SalesOrderDetailExistsAsync(soDetailId, soId);

            result.Should().BeTrue();
        }

        [Fact]
        public async Task SalesOrderDetailExistsAsync_Should_Return_False_For_Wrong_Pair()
        {
            var (soId, _, _, _) = await EnsureSalesOrderAsync();

            var result = await CreateRepo().SalesOrderDetailExistsAsync(9999999, soId);

            result.Should().BeFalse();
        }
    }
}
