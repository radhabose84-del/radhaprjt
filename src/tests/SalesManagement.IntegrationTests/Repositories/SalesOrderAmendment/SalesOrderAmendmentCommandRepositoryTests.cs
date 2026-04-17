using Microsoft.EntityFrameworkCore;
using SalesManagement.Application.Common.Interfaces.IMiscMaster;
using SalesManagement.Domain.Common;
using SalesManagement.Infrastructure.Data;
using SalesManagement.Infrastructure.Repositories.SalesOrderAmendment;
using SalesManagement.IntegrationTests.Common;
using static SalesManagement.Domain.Common.BaseEntity;

namespace SalesManagement.IntegrationTests.Repositories.SalesOrderAmendment
{
    [Collection("DatabaseCollection")]
    public sealed class SalesOrderAmendmentCommandRepositoryTests
    {
        private readonly DbFixture _fixture;
        public SalesOrderAmendmentCommandRepositoryTests(DbFixture fixture) => _fixture = fixture;

        private SalesOrderAmendmentCommandRepository CreateRepo(ApplicationDbContext ctx, Mock<IMiscMasterQueryRepository>? miscRepo = null)
        {
            if (miscRepo == null)
            {
                miscRepo = new Mock<IMiscMasterQueryRepository>(MockBehavior.Loose);
                miscRepo.Setup(m => m.GetMiscMasterByName(MiscEnumEntity.SalesOrderApprovalStatus, MiscEnumEntity.SalesOrderStatusPending))
                    .ReturnsAsync(() => LookupMiscAsync("ApprovalStatus", "Pending").GetAwaiter().GetResult());
                miscRepo.Setup(m => m.GetMiscMasterByName(MiscEnumEntity.SalesOrderApprovalStatus, MiscEnumEntity.SalesOrderStatusApproved))
                    .ReturnsAsync(() => LookupMiscAsync("ApprovalStatus", "Approved").GetAwaiter().GetResult());
                miscRepo.Setup(m => m.GetMiscMasterByName(MiscEnumEntity.SalesOrderApprovalStatus, MiscEnumEntity.SalesOrderStatusRejected))
                    .ReturnsAsync(() => LookupMiscAsync("ApprovalStatus", "Rejected").GetAwaiter().GetResult());
                miscRepo.Setup(m => m.GetMiscMasterByName(MiscEnumEntity.LineItemApprovalStatus, MiscEnumEntity.LineStatusDeleted))
                    .ReturnsAsync(() => LookupMiscAsync("LineItemStatus", "Deleted").GetAwaiter().GetResult());
            }
            return new SalesOrderAmendmentCommandRepository(ctx, miscRepo.Object);
        }

        private async Task<SalesManagement.Domain.Entities.MiscMaster?> LookupMiscAsync(string typeCode, string code)
        {
            await using var ctx = _fixture.CreateFreshDbContext();
            return await ctx.MiscMaster
                .Include(m => m.MiscTypeMaster)
                .FirstOrDefaultAsync(m => m.Code == code && m.MiscTypeMaster != null && m.MiscTypeMaster.MiscTypeCode == typeCode);
        }

        private async Task<(int miscTypeId, int pending, int approved, int rejected, int freight, int enquiry, int lineOpen, int lineDeleted)> EnsureStatusMiscAsync()
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
            var rejected = await EnsureMiscAsync(ctx, mt.Id, "Rejected");

            var aux = await ctx.MiscTypeMaster.FirstOrDefaultAsync(x => x.MiscTypeCode == "SOA_AUX");
            if (aux == null)
            {
                aux = new SalesManagement.Domain.Entities.MiscTypeMaster
                {
                    MiscTypeCode = "SOA_AUX", Description = "Aux",
                    IsActive = Status.Active, IsDeleted = IsDelete.NotDeleted
                };
                await ctx.MiscTypeMaster.AddAsync(aux);
                await ctx.SaveChangesAsync();
            }
            var freight = await EnsureMiscAsync(ctx, aux.Id, "SOA_FT");
            var enquiry = await EnsureMiscAsync(ctx, aux.Id, "SOA_ENQ");

            var lineStatus = await ctx.MiscTypeMaster.FirstOrDefaultAsync(x => x.MiscTypeCode == "LineItemStatus");
            if (lineStatus == null)
            {
                lineStatus = new SalesManagement.Domain.Entities.MiscTypeMaster
                {
                    MiscTypeCode = "LineItemStatus", Description = "Line Item Status",
                    IsActive = Status.Active, IsDeleted = IsDelete.NotDeleted
                };
                await ctx.MiscTypeMaster.AddAsync(lineStatus);
                await ctx.SaveChangesAsync();
            }
            var lineOpen = await EnsureMiscAsync(ctx, lineStatus.Id, "Open");
            var lineDeleted = await EnsureMiscAsync(ctx, lineStatus.Id, "Deleted");

            return (mt.Id, pending, approved, rejected, freight, enquiry, lineOpen, lineDeleted);
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

        private async Task<(int soId, int soDetailId)> EnsureSalesOrderAsync()
        {
            var (_, _, _, _, freightId, enquiryId, _, _) = await EnsureStatusMiscAsync();

            await using var ctx = _fixture.CreateFreshDbContext();

            var org = await ctx.SalesOrganisation.FirstOrDefaultAsync(x => x.SalesOrganisationCode == "SOA_ORG");
            if (org == null)
            {
                org = new SalesManagement.Domain.Entities.SalesOrganisation
                {
                    SalesOrganisationCode = "SOA_ORG", SalesOrganisationName = "Org",
                    CompanyId = 1, IsActive = Status.Active, IsDeleted = IsDelete.NotDeleted
                };
                await ctx.SalesOrganisation.AddAsync(org);
                await ctx.SaveChangesAsync();
            }
            var office = await ctx.SalesOffice.FirstOrDefaultAsync(x => x.SalesOfficeName == "SOA_OFC");
            if (office == null)
            {
                office = new SalesManagement.Domain.Entities.SalesOffice
                {
                    SalesOfficeName = "SOA_OFC", SalesOrganisationId = org.Id,
                    IsActive = Status.Active, IsDeleted = IsDelete.NotDeleted
                };
                await ctx.SalesOffice.AddAsync(office);
                await ctx.SaveChangesAsync();
            }
            var sg = await ctx.SalesGroup.FirstOrDefaultAsync(x => x.SalesGroupName == "SOA_SG");
            if (sg == null)
            {
                sg = new SalesManagement.Domain.Entities.SalesGroup
                {
                    SalesGroupName = "SOA_SG", SalesOfficeId = office.Id,
                    IsActive = Status.Active, IsDeleted = IsDelete.NotDeleted
                };
                await ctx.SalesGroup.AddAsync(sg);
                await ctx.SaveChangesAsync();
            }

            var so = await ctx.SalesOrderHeader
                .Include(h => h.SalesOrderDetails)
                .FirstOrDefaultAsync(x => x.SalesOrderNo == "SOA_SO1");
            if (so == null)
            {
                so = new SalesManagement.Domain.Entities.SalesOrderHeader
                {
                    SalesOrderNo = "SOA_SO1",
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
            return (so.Id, so.SalesOrderDetails!.First().Id);
        }

        private async Task<(SalesManagement.Domain.Entities.SalesOrderAmendmentHeader header, List<SalesManagement.Domain.Entities.SalesOrderAmendmentDetail> details)> BuildAmendmentAsync(string amendmentNo = "AM001", string changeType = "Modified")
        {
            var (soId, soDetailId) = await EnsureSalesOrderAsync();
            var header = new SalesManagement.Domain.Entities.SalesOrderAmendmentHeader
            {
                SalesOrderHeaderId = soId,
                UnitId = 1,
                AmendmentNo = amendmentNo,
                RevisionNumber = 1,
                AmendmentDate = DateOnly.FromDateTime(DateTime.UtcNow.Date),
                Reason = "Customer requested qty change",
                TotalBags = 25, TotalWeightKgs = 1250m, TotalDiscountPerKg = 0m,
                ItemValue = 125m, TotalFreight = 0m, TaxableAmount = 125m,
                GSTPercentage = 5m, TotalGST = 6.25m, TotalWithGST = 131.25m,
                TCSPercentage = 0m, TotalTCS = 0m, FinalAmount = 131.25m,
                IsActive = Status.Active, IsDeleted = IsDelete.NotDeleted
            };
            var details = new List<SalesManagement.Domain.Entities.SalesOrderAmendmentDetail>
            {
                new()
                {
                    ChangeType = changeType,
                    SalesOrderDetailId = soDetailId,
                    OldItemId = 10, OldQtyInBags = 20,
                    OldExMillRate = 10m,
                    OldExpectedDeliveryDate = DateOnly.FromDateTime(DateTime.UtcNow.Date.AddDays(30)),
                    NewQtyInBags = changeType == "Modified" ? 25 : null,
                    NewExMillRate = changeType == "Modified" ? 11m : null,
                    NewExpectedDeliveryDate = changeType == "Modified" ? DateOnly.FromDateTime(DateTime.UtcNow.Date.AddDays(35)) : null,
                    TaxableAmount = 125m, TaxAmount = 6.25m, TCSAmount = 0m,
                    NetAmount = 131.25m, NetRatePerKg = 10.5m,
                    PendingQty = 25
                }
            };
            return (header, details);
        }

        private async Task ClearAsync(ApplicationDbContext ctx) =>
            await _fixture.ClearAllTablesAsync();

        [Fact]
        public async Task CreateAsync_Should_Return_NewId()
        {
            await using var ctx = _fixture.CreateFreshDbContext();
            await ClearAsync(ctx);

            var (h, d) = await BuildAmendmentAsync("AM_C1");
            var id = await CreateRepo(ctx).CreateAsync(h, d);

            id.Should().BeGreaterThan(0);
        }

        [Fact]
        public async Task CreateAsync_Should_Set_StatusId_To_Pending()
        {
            await using var ctx = _fixture.CreateFreshDbContext();
            await ClearAsync(ctx);

            var (h, d) = await BuildAmendmentAsync("AM_C2");
            var id = await CreateRepo(ctx).CreateAsync(h, d);
            ctx.ChangeTracker.Clear();

            var saved = await ctx.SalesOrderAmendmentHeader.FirstAsync(x => x.Id == id);
            saved.StatusId.Should().NotBeNull();
            var status = await ctx.MiscMaster.FirstAsync(m => m.Id == saved.StatusId);
            status.Code.Should().Be("Pending");
        }

        [Fact]
        public async Task CreateAsync_Should_Persist_Details()
        {
            await using var ctx = _fixture.CreateFreshDbContext();
            await ClearAsync(ctx);

            var (h, d) = await BuildAmendmentAsync("AM_C3");
            var id = await CreateRepo(ctx).CreateAsync(h, d);
            ctx.ChangeTracker.Clear();

            var details = await ctx.SalesOrderAmendmentDetail.Where(x => x.SalesOrderAmendmentHeaderId == id).ToListAsync();
            details.Should().HaveCount(1);
            details[0].ChangeType.Should().Be("Modified");
        }

        [Fact]
        public async Task ApplyAmendmentAsync_Approved_Should_Update_Status()
        {
            await using var ctx = _fixture.CreateFreshDbContext();
            await ClearAsync(ctx);
            var (h, d) = await BuildAmendmentAsync("AM_AP1");
            var id = await CreateRepo(ctx).CreateAsync(h, d);
            ctx.ChangeTracker.Clear();

            var result = await CreateRepo(ctx).ApplyAmendmentAsync(
                id, MiscEnumEntity.SalesOrderStatusApproved, 1, "test-user", "127.0.0.1", CancellationToken.None);
            ctx.ChangeTracker.Clear();

            result.Should().BeTrue();
            var amended = await ctx.SalesOrderAmendmentHeader.FirstAsync(x => x.Id == id);
            var status = await ctx.MiscMaster.FirstAsync(m => m.Id == amended.StatusId);
            status.Code.Should().Be("Approved");
            amended.ApprovedBy.Should().Be(1);
        }

        [Fact]
        public async Task ApplyAmendmentAsync_Approved_Should_Propagate_Changes_To_SalesOrderDetail()
        {
            await using var ctx = _fixture.CreateFreshDbContext();
            await ClearAsync(ctx);
            var (h, d) = await BuildAmendmentAsync("AM_AP2");
            var id = await CreateRepo(ctx).CreateAsync(h, d);
            ctx.ChangeTracker.Clear();

            await CreateRepo(ctx).ApplyAmendmentAsync(
                id, MiscEnumEntity.SalesOrderStatusApproved, 1, "u", "ip", CancellationToken.None);
            ctx.ChangeTracker.Clear();

            var soDetail = await ctx.SalesOrderDetail.FirstAsync(x => x.Id == d[0].SalesOrderDetailId);
            soDetail.QtyInBags.Should().Be(25); // amended NewQtyInBags
            soDetail.ExMillRate.Should().Be(11m);
        }

        [Fact]
        public async Task ApplyAmendmentAsync_Rejected_Should_Update_Status_Only()
        {
            await using var ctx = _fixture.CreateFreshDbContext();
            await ClearAsync(ctx);
            var (h, d) = await BuildAmendmentAsync("AM_RJ1");
            var id = await CreateRepo(ctx).CreateAsync(h, d);
            ctx.ChangeTracker.Clear();

            var originalDetail = await ctx.SalesOrderDetail.FirstAsync(x => x.Id == d[0].SalesOrderDetailId);
            var originalQty = originalDetail.QtyInBags;

            await CreateRepo(ctx).ApplyAmendmentAsync(
                id, MiscEnumEntity.SalesOrderStatusRejected, 1, "u", "ip", CancellationToken.None);
            ctx.ChangeTracker.Clear();

            var amended = await ctx.SalesOrderAmendmentHeader.FirstAsync(x => x.Id == id);
            var status = await ctx.MiscMaster.FirstAsync(m => m.Id == amended.StatusId);
            status.Code.Should().Be("Rejected");

            var soDetail = await ctx.SalesOrderDetail.FirstAsync(x => x.Id == d[0].SalesOrderDetailId);
            soDetail.QtyInBags.Should().Be(originalQty); // unchanged
        }

        [Fact]
        public async Task ApplyAmendmentAsync_Should_Return_False_When_NotFound()
        {
            await using var ctx = _fixture.CreateFreshDbContext();

            var result = await CreateRepo(ctx).ApplyAmendmentAsync(
                9999999, MiscEnumEntity.SalesOrderStatusApproved, 1, "u", "ip", CancellationToken.None);

            result.Should().BeFalse();
        }

        [Fact]
        public async Task GetByIdEntityAsync_Should_Return_With_Details()
        {
            await using var ctx = _fixture.CreateFreshDbContext();
            await ClearAsync(ctx);
            var (h, d) = await BuildAmendmentAsync("AM_G1");
            var id = await CreateRepo(ctx).CreateAsync(h, d);
            ctx.ChangeTracker.Clear();

            var result = await CreateRepo(ctx).GetByIdEntityAsync(id);

            result.Should().NotBeNull();
            result!.SalesOrderAmendmentDetails.Should().HaveCount(1);
        }

        [Fact]
        public async Task GetSalesOrderEntityAsync_Should_Return_With_Details()
        {
            await using var ctx = _fixture.CreateFreshDbContext();
            var (soId, _) = await EnsureSalesOrderAsync();

            var result = await CreateRepo(ctx).GetSalesOrderEntityAsync(soId);

            result.Should().NotBeNull();
            result!.SalesOrderDetails.Should().NotBeNullOrEmpty();
        }

        [Fact]
        public async Task GetByIdAmendmentWorkFlowAsync_Should_Return_Projection()
        {
            await using var ctx = _fixture.CreateFreshDbContext();
            await ClearAsync(ctx);
            var (h, d) = await BuildAmendmentAsync("AM_WF1");
            var id = await CreateRepo(ctx).CreateAsync(h, d);
            ctx.ChangeTracker.Clear();

            var result = await CreateRepo(ctx).GetByIdAmendmentWorkFlowAsync(id);

            result.Should().NotBeNull();
            result.AmendmentNo.Should().Be("AM_WF1");
            result.SalesOrderNo.Should().Be("SOA_SO1");
        }
    }
}
