using Contracts.Interfaces;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using PurchaseManagement.Domain.Entities;
using PurchaseManagement.Domain.Entities.Issue;
using PurchaseManagement.Domain.Entities.MRS;
using PurchaseManagement.Infrastructure.Data;
using PurchaseManagement.Infrastructure.Repositories.Issue;
using Dapper;
using static PurchaseManagement.Domain.Common.BaseEntity;
using MiscMasterEntity = PurchaseManagement.Domain.Entities.MiscMaster;
using MiscTypeMasterEntity = PurchaseManagement.Domain.Entities.MiscTypeMaster;

namespace PurchaseManagement.IntegrationTests.Repositories.Issue
{
    [Collection("DatabaseCollection")]
    public sealed class IssueEntryQueryRepositoryTests
    {
        private readonly DbFixture _fixture;

        public IssueEntryQueryRepositoryTests(DbFixture fixture) => _fixture = fixture;

        private IssueEntryQueryRepository CreateQueryRepo()
        {
            var ipMock = new Mock<IIPAddressService>(MockBehavior.Loose);
            ipMock.Setup(x => x.GetUnitId()).Returns(1);
            ipMock.Setup(x => x.GetUserId()).Returns(1);
            ipMock.Setup(x => x.GetUserName()).Returns("test-user");
            ipMock.Setup(x => x.GetSystemIPAddress()).Returns("127.0.0.1");
            var conn = new SqlConnection(_fixture.ConnectionString);
            return new IssueEntryQueryRepository(conn, ipMock.Object);
        }

        private IssueEntryCommandRepository CreateCommandRepo(ApplicationDbContext ctx)
        {
            var ipMock = new Mock<IIPAddressService>(MockBehavior.Loose);
            ipMock.Setup(x => x.GetUnitId()).Returns(1);
            ipMock.Setup(x => x.GetUserId()).Returns(1);
            ipMock.Setup(x => x.GetUserName()).Returns("test-user");
            ipMock.Setup(x => x.GetSystemIPAddress()).Returns("127.0.0.1");
            return new IssueEntryCommandRepository(ctx, ipMock.Object);
        }

        /// <summary>
        /// Seeds MiscTypeMaster + MiscMaster records for Status and RequestCategory.
        /// Returns (approvedStatusId, requestCategoryId).
        /// </summary>
        private async Task<(int approvedStatusId, int requestCategoryId)> SeedMiscDataAsync(
            ApplicationDbContext ctx, string statusDesc = "Approved")
        {
            var miscType = new MiscTypeMasterEntity
            {
                MiscTypeCode = "ISS_QMT",
                Description = "ISS_QTestType",
                IsActive = Status.Active,
                IsDeleted = IsDelete.NotDeleted
            };
            await ctx.Set<MiscTypeMasterEntity>().AddAsync(miscType);
            await ctx.SaveChangesAsync();

            var statusMisc = new MiscMasterEntity
            {
                MiscTypeId = miscType.Id,
                Code = "ISS_QS",
                Description = statusDesc,
                SortOrder = 1,
                IsActive = Status.Active,
                IsDeleted = IsDelete.NotDeleted
            };
            var reqCatMisc = new MiscMasterEntity
            {
                MiscTypeId = miscType.Id,
                Code = "ISS_QRC",
                Description = "Consumption",
                SortOrder = 2,
                IsActive = Status.Active,
                IsDeleted = IsDelete.NotDeleted
            };
            await ctx.Set<MiscMasterEntity>().AddRangeAsync(statusMisc, reqCatMisc);
            await ctx.SaveChangesAsync();

            return (statusMisc.Id, reqCatMisc.Id);
        }

        private async Task<int> SeedMrsWithDetailsAsync(
            ApplicationDbContext ctx, int statusId, int requestCategoryId,
            string mrsNo = "ISS_QMRS001", int detailCount = 1)
        {
            var mrsHeader = new MrsHeader
            {
                UnitId = 1,
                RequestCategoryId = requestCategoryId,
                MrsNo = mrsNo,
                MrsDate = DateTimeOffset.UtcNow,
                DepartmentId = 1,
                SubDepartmentId = 1,
                StatusId = statusId,
                CreatedBy = 1,
                CreatedDate = DateTimeOffset.UtcNow,
                CreatedByName = "test-user",
                CreatedIP = "127.0.0.1",
                Remarks = "test mrs",
                MrsDetailHeaderName = Enumerable.Range(1, detailCount).Select(i => new MrsDetail
                {
                    ItemId = i * 10,
                    UomId = 1,
                    RequestQuantity = 100m,
                    WarehouseStockId = 1
                }).ToList()
            };
            await ctx.Set<MrsHeader>().AddAsync(mrsHeader);
            await ctx.SaveChangesAsync();
            return mrsHeader.Id;
        }

        private async Task<int> SeedIssueAsync(
            ApplicationDbContext ctx, int mrsHeaderId, string issueNo = "ISS_Q001", int detailCount = 1)
        {
            var header = new IssueHeader
            {
                UnitId = 1,
                IssueNo = issueNo,
                IssueDate = DateTimeOffset.UtcNow,
                MrsHeaderId = mrsHeaderId,
                IssuedBy = 1,
                IssuedDate = DateTimeOffset.UtcNow,
                IssuedByName = "test-user",
                IssuedIp = "127.0.0.1",
                Remarks = "test issue",
                IssueHeaderName = Enumerable.Range(1, detailCount).Select(i => new IssueDetail
                {
                    Sno = i,
                    ItemId = i * 10,
                    UomId = 1,
                    RequestQuantity = 100m,
                    WarehouseStockId = 1,
                    StorageTypeId = 1,
                    TargetId = 1,
                    IssueQuantity = 50m,
                    IssueValue = 500m
                }).ToList()
            };
            await CreateCommandRepo(ctx).CreateIssueAsync(header);
            return header.Id;
        }

        // --- GetDescriptionByIdAsync ---

        [Fact]
        public async Task GetDescriptionByIdAsync_Should_Return_Description_When_Found()
        {
            await _fixture.ClearAllTablesAsync();
            await using var ctx = _fixture.CreateFreshDbContext();

            var (statusId, _) = await SeedMiscDataAsync(ctx);

            var result = await CreateQueryRepo().GetDescriptionByIdAsync(statusId);

            result.Should().Be("Approved");
        }

        [Fact]
        public async Task GetDescriptionByIdAsync_Should_Return_Null_When_NotFound()
        {
            await _fixture.ClearAllTablesAsync();

            var result = await CreateQueryRepo().GetDescriptionByIdAsync(999999);

            result.Should().BeNull();
        }

        // --- GetApprovedMrsDetails ---

        [Fact]
        public async Task GetApprovedMrsDetails_Should_Return_Approved_Mrs_Records()
        {
            await _fixture.ClearAllTablesAsync();
            await using var ctx = _fixture.CreateFreshDbContext();

            var (statusId, reqCatId) = await SeedMiscDataAsync(ctx, "Approved");
            await SeedMrsWithDetailsAsync(ctx, statusId, reqCatId, "ISS_AMRS01");

            var result = await CreateQueryRepo().GetApprovedMrsDetails(null);

            result.Should().NotBeEmpty();
            result[0].MrsNo.Should().Be("ISS_AMRS01");
        }

        [Fact]
        public async Task GetApprovedMrsDetails_Should_Filter_By_SearchPattern()
        {
            await _fixture.ClearAllTablesAsync();
            await using var ctx = _fixture.CreateFreshDbContext();

            var (statusId, reqCatId) = await SeedMiscDataAsync(ctx, "Approved");
            await SeedMrsWithDetailsAsync(ctx, statusId, reqCatId, "ISS_ALPHA01");
            await SeedMrsWithDetailsAsync(ctx, statusId, reqCatId, "ISS_BETA02");

            var result = await CreateQueryRepo().GetApprovedMrsDetails("ALPHA");

            result.Should().HaveCount(1);
            result[0].MrsNo.Should().Be("ISS_ALPHA01");
        }

        // --- GetPendingIssueHeaderAsync ---

        [Fact]
        public async Task GetPendingIssueHeaderAsync_Should_Return_Approved_Mrs_With_Pending_Issues()
        {
            await _fixture.ClearAllTablesAsync();
            await using var ctx = _fixture.CreateFreshDbContext();

            var (statusId, reqCatId) = await SeedMiscDataAsync(ctx, "Approved");
            await SeedMrsWithDetailsAsync(ctx, statusId, reqCatId, "ISS_PIH01");

            var (data, total) = await CreateQueryRepo().GetPendingIssueHeaderAsync(
                null, null, 1, 10, null);

            total.Should().BeGreaterThanOrEqualTo(1);
            data.Should().NotBeEmpty();
            data[0].MrsNo.Should().Be("ISS_PIH01");
        }

        [Fact]
        public async Task GetPendingIssueHeaderAsync_Should_Return_Empty_When_No_Approved_Mrs()
        {
            await _fixture.ClearAllTablesAsync();

            var (data, total) = await CreateQueryRepo().GetPendingIssueHeaderAsync(
                null, null, 1, 10, null);

            data.Should().BeEmpty();
            total.Should().Be(0);
        }

        // --- GetPendingIssuesAsync ---

        [Fact]
        public async Task GetPendingIssuesAsync_Should_Return_Pending_Details_For_Mrs()
        {
            await _fixture.ClearAllTablesAsync();
            await using var ctx = _fixture.CreateFreshDbContext();

            var (statusId, reqCatId) = await SeedMiscDataAsync(ctx, "Approved");
            var mrsId = await SeedMrsWithDetailsAsync(ctx, statusId, reqCatId, "ISS_PI01", detailCount: 2);

            var result = await CreateQueryRepo().GetPendingIssuesAsync(mrsId);

            result.Should().NotBeEmpty();
            result[0].MrsId.Should().Be(mrsId);
            result[0].PendingIssueDetails.Should().NotBeEmpty();
        }

        [Fact]
        public async Task GetPendingIssuesAsync_Should_Reflect_Issued_Quantity()
        {
            await _fixture.ClearAllTablesAsync();
            await using var ctx = _fixture.CreateFreshDbContext();

            var (statusId, reqCatId) = await SeedMiscDataAsync(ctx, "Approved");
            var mrsId = await SeedMrsWithDetailsAsync(ctx, statusId, reqCatId, "ISS_PI02", detailCount: 1);

            // Get the MrsDetail Sno for linking
            var mrsDetail = await ctx.Set<MrsDetail>().FirstAsync(d => d.MrsHeaderId == mrsId);

            // Issue partial quantity against this MRS
            var issueHeader = new IssueHeader
            {
                UnitId = 1,
                IssueNo = "ISS_PIQ01",
                IssueDate = DateTimeOffset.UtcNow,
                MrsHeaderId = mrsId,
                IssuedBy = 1,
                IssuedDate = DateTimeOffset.UtcNow,
                IssuedByName = "test-user",
                IssuedIp = "127.0.0.1",
                IssueHeaderName = new List<IssueDetail>
                {
                    new IssueDetail
                    {
                        Sno = mrsDetail.Id,
                        ItemId = mrsDetail.ItemId,
                        UomId = 1,
                        RequestQuantity = 100m,
                        WarehouseStockId = 1,
                        StorageTypeId = 1,
                        TargetId = 1,
                        IssueQuantity = 30m,
                        IssueValue = 300m
                    }
                }
            };
            await CreateCommandRepo(ctx).CreateIssueAsync(issueHeader);
            ctx.ChangeTracker.Clear();

            var result = await CreateQueryRepo().GetPendingIssuesAsync(mrsId);

            result.Should().NotBeEmpty();
            var detail = result[0].PendingIssueDetails[0];
            detail.IssuedQuantity.Should().Be(30m);
            detail.PendingQuantity.Should().Be(70m); // 100 - 30
        }
    }
}
