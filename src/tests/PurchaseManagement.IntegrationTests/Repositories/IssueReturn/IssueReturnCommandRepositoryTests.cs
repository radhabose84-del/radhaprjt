using Contracts.Interfaces;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using PurchaseManagement.Application.Common.Interfaces.IMiscMaster;
using PurchaseManagement.Domain.Common;
using PurchaseManagement.Domain.Entities.IssueReturn;
using PurchaseManagement.Domain.Entities.MRS;
using PurchaseManagement.Domain.Entities.Issue;
using PurchaseManagement.Infrastructure.Data;
using PurchaseManagement.Infrastructure.Repositories.IssueReturn;
using static PurchaseManagement.Domain.Common.BaseEntity;

namespace PurchaseManagement.IntegrationTests.Repositories.IssueReturn
{
    [Collection("DatabaseCollection")]
    public sealed class IssueReturnCommandRepositoryTests
    {
        private readonly DbFixture _fixture;

        public IssueReturnCommandRepositoryTests(DbFixture fixture) => _fixture = fixture;

        // --- Helpers ---

        private IssueReturnEntryCommandRepository CreateRepo(
            ApplicationDbContext ctx,
            Mock<IMiscMasterQueryRepository> miscMock = null)
        {
            miscMock ??= BuildDefaultMiscMock(pendingStatusId: 100);
            return new IssueReturnEntryCommandRepository(ctx, _fixture.IpMock.Object, miscMock.Object);
        }

        private static Mock<IMiscMasterQueryRepository> BuildDefaultMiscMock(int pendingStatusId)
        {
            var mock = new Mock<IMiscMasterQueryRepository>(MockBehavior.Loose);
            mock.Setup(m => m.GetMiscMasterByName(
                    MiscEnumEntity.ApprovalStatus, MiscEnumEntity.Pending))
                .ReturnsAsync(new PurchaseManagement.Domain.Entities.MiscMaster
                {
                    Id = pendingStatusId,
                    Code = "PENDING",
                    Description = "Pending",
                    IsActive = Status.Active,
                    IsDeleted = IsDelete.NotDeleted
                });
            return mock;
        }

        /// <summary>
        /// Seeds MiscTypeMaster + MiscMaster rows required by FK constraints on IssueReturnHeader/Detail.
        /// Returns (miscTypeMasterId, statusMiscMasterId, reasonMiscMasterId, requestCategoryMiscMasterId).
        /// </summary>
        private async Task<(int MiscTypeId, int StatusId, int ReasonId, int RequestCategoryId)>
            SeedMiscMasterAsync(ApplicationDbContext ctx)
        {
            var miscType = new PurchaseManagement.Domain.Entities.MiscTypeMaster
            {
                MiscTypeCode = "IR_MISCTYPE",
                Description = "IR Test Type",
                IsActive = Status.Active,
                IsDeleted = IsDelete.NotDeleted
            };
            ctx.MiscTypeMaster.Add(miscType);
            await ctx.SaveChangesAsync();

            var statusMisc = new PurchaseManagement.Domain.Entities.MiscMaster
            {
                MiscTypeId = miscType.Id,
                Code = "IR_PENDING",
                Description = "Pending",
                SortOrder = 1,
                IsActive = Status.Active,
                IsDeleted = IsDelete.NotDeleted
            };
            var reasonMisc = new PurchaseManagement.Domain.Entities.MiscMaster
            {
                MiscTypeId = miscType.Id,
                Code = "IR_REASON",
                Description = "Damaged",
                SortOrder = 2,
                IsActive = Status.Active,
                IsDeleted = IsDelete.NotDeleted
            };
            var reqCatMisc = new PurchaseManagement.Domain.Entities.MiscMaster
            {
                MiscTypeId = miscType.Id,
                Code = "IR_REQCAT",
                Description = "Consumption",
                SortOrder = 3,
                IsActive = Status.Active,
                IsDeleted = IsDelete.NotDeleted
            };
            ctx.MiscMaster.AddRange(statusMisc, reasonMisc, reqCatMisc);
            await ctx.SaveChangesAsync();
            ctx.ChangeTracker.Clear();

            return (miscType.Id, statusMisc.Id, reasonMisc.Id, reqCatMisc.Id);
        }

        /// <summary>
        /// Seeds an MrsHeader (required FK for IssueHeader).
        /// </summary>
        private async Task<int> SeedMrsHeaderAsync(ApplicationDbContext ctx, int statusId, int requestCategoryId)
        {
            var mrs = new MrsHeader
            {
                UnitId = 1,
                RequestCategoryId = requestCategoryId,
                MrsNo = "IR_MRS001",
                MrsDate = DateTimeOffset.UtcNow,
                DepartmentId = 1,
                SubDepartmentId = 1,
                StatusId = statusId,
                CreatedBy = 1,
                CreatedDate = DateTimeOffset.UtcNow,
                CreatedByName = "test-user",
                CreatedIP = "127.0.0.1"
            };
            ctx.MrsHeader.Add(mrs);
            await ctx.SaveChangesAsync();
            ctx.ChangeTracker.Clear();
            return mrs.Id;
        }

        /// <summary>
        /// Seeds an IssueHeader (optional FK for IssueReturnHeader).
        /// </summary>
        private async Task<int> SeedIssueHeaderAsync(ApplicationDbContext ctx, int mrsHeaderId)
        {
            var issue = new IssueHeader
            {
                UnitId = 1,
                IssueNo = "IR_ISS001",
                IssueDate = DateTimeOffset.UtcNow,
                MrsHeaderId = mrsHeaderId,
                IssuedBy = 1,
                IssuedDate = DateTimeOffset.UtcNow,
                IssuedByName = "test-user",
                IssuedIp = "127.0.0.1"
            };
            ctx.IssueHeader.Add(issue);
            await ctx.SaveChangesAsync();
            ctx.ChangeTracker.Clear();
            return issue.Id;
        }

        private IssueReturnHeader BuildHeader(
            int statusId,
            int requestCategoryId,
            int reasonId,
            int? issueHeaderId = null,
            string issueReturnNo = "IR_RET001",
            int detailCount = 1)
        {
            var header = new IssueReturnHeader
            {
                UnitId = 1,
                IssueReturnNo = issueReturnNo,
                IssueReturnDate = DateTimeOffset.UtcNow,
                IssueHeaderId = issueHeaderId,
                DepartmentId = 1,
                RequestCategoryId = requestCategoryId,
                StatusId = statusId,
                Remarks = "test",
                CreatedBy = 1,
                CreatedDate = DateTimeOffset.UtcNow,
                CreatedByName = "test-user",
                CreatedIP = "127.0.0.1",
                IssueReturnDetailsHeaderName = Enumerable.Range(1, detailCount)
                    .Select(i => new IssueReturnDetail
                    {
                        ItemId = i * 10,
                        UomId = 1,
                        WarehouseStockId = 1,
                        ReturnQuantity = 5m,
                        ReturnValue = 50m,
                        ReasonId = reasonId,
                        StatusId = statusId,
                        Remarks = $"detail-{i}",
                        CreatedBy = 1,
                        CreatedDate = DateTimeOffset.UtcNow,
                        CreatedByName = "test-user",
                        CreatedIP = "127.0.0.1"
                    }).ToList()
            };
            return header;
        }

        // --- TESTS ---

        [Fact]
        public async Task CreateAsync_Should_Persist_Header_And_Details()
        {
            await _fixture.ClearAllTablesAsync();
            await using var ctx = _fixture.CreateFreshDbContext();
            var (_, statusId, reasonId, reqCatId) = await SeedMiscMasterAsync(ctx);

            var miscMock = BuildDefaultMiscMock(statusId);
            var repo = CreateRepo(ctx, miscMock);

            var header = BuildHeader(statusId, reqCatId, reasonId, detailCount: 2);
            var result = await repo.CreateAsync(header);

            result.Id.Should().BeGreaterThan(0);
            ctx.ChangeTracker.Clear();

            var details = await ctx.IssueReturnDetail
                .Where(d => d.IssueReturnHeaderId == result.Id)
                .ToListAsync();
            details.Should().HaveCount(2);
        }

        [Fact]
        public async Task CreateAsync_Should_Set_StatusId_From_MiscMaster_Pending()
        {
            await _fixture.ClearAllTablesAsync();
            await using var ctx = _fixture.CreateFreshDbContext();
            var (_, statusId, reasonId, reqCatId) = await SeedMiscMasterAsync(ctx);

            var miscMock = BuildDefaultMiscMock(statusId);
            var repo = CreateRepo(ctx, miscMock);

            var header = BuildHeader(statusId, reqCatId, reasonId);
            var result = await repo.CreateAsync(header);

            result.StatusId.Should().Be(statusId);
        }

        [Fact]
        public async Task CreateAsync_Should_Populate_Detail_Audit_Fields()
        {
            await _fixture.ClearAllTablesAsync();
            await using var ctx = _fixture.CreateFreshDbContext();
            var (_, statusId, reasonId, reqCatId) = await SeedMiscMasterAsync(ctx);

            var miscMock = BuildDefaultMiscMock(statusId);
            var repo = CreateRepo(ctx, miscMock);

            var header = BuildHeader(statusId, reqCatId, reasonId);
            await repo.CreateAsync(header);
            ctx.ChangeTracker.Clear();

            var detail = await ctx.IssueReturnDetail.FirstAsync();
            detail.CreatedBy.Should().Be(1);
            detail.CreatedByName.Should().Be("test-user");
            detail.CreatedIP.Should().NotBeNullOrEmpty();
        }

        [Fact]
        public async Task UpdateAsync_Should_Replace_Details()
        {
            await _fixture.ClearAllTablesAsync();
            await using var ctx = _fixture.CreateFreshDbContext();
            var (_, statusId, reasonId, reqCatId) = await SeedMiscMasterAsync(ctx);

            var miscMock = BuildDefaultMiscMock(statusId);
            var repo = CreateRepo(ctx, miscMock);

            // Create with 1 detail
            var header = BuildHeader(statusId, reqCatId, reasonId, detailCount: 1);
            await repo.CreateAsync(header);
            ctx.ChangeTracker.Clear();

            // Update with 3 new details (old one removed, 3 new ones added)
            var updatedHeader = new IssueReturnHeader
            {
                Id = header.Id,
                UnitId = 1,
                IssueReturnNo = header.IssueReturnNo,
                IssueReturnDate = DateTimeOffset.UtcNow,
                DepartmentId = 2,
                RequestCategoryId = reqCatId,
                StatusId = statusId,
                Remarks = "updated",
                CreatedBy = 1,
                CreatedByName = "test-user",
                CreatedIP = "127.0.0.1",
                IssueReturnDetailsHeaderName = Enumerable.Range(1, 3)
                    .Select(i => new IssueReturnDetail
                    {
                        ItemId = i * 100,
                        UomId = 1,
                        WarehouseStockId = 1,
                        ReturnQuantity = 10m,
                        ReturnValue = 100m,
                        ReasonId = reasonId,
                        StatusId = statusId,
                        CreatedBy = 1,
                        CreatedByName = "test-user",
                        CreatedIP = "127.0.0.1"
                    }).ToList()
            };

            var result = await repo.UpdateAsync(updatedHeader);
            result.Should().BeTrue();
            ctx.ChangeTracker.Clear();

            var details = await ctx.IssueReturnDetail
                .Where(d => d.IssueReturnHeaderId == header.Id)
                .ToListAsync();
            details.Should().HaveCount(3);
        }

        [Fact]
        public async Task UpdateAsync_Should_Throw_When_Header_NotFound()
        {
            await _fixture.ClearAllTablesAsync();
            await using var ctx = _fixture.CreateFreshDbContext();
            var (_, statusId, reasonId, reqCatId) = await SeedMiscMasterAsync(ctx);

            var miscMock = BuildDefaultMiscMock(statusId);
            var repo = CreateRepo(ctx, miscMock);

            var header = new IssueReturnHeader
            {
                Id = 9999,
                UnitId = 1,
                IssueReturnNo = "IR_NOTEXIST",
                IssueReturnDate = DateTimeOffset.UtcNow,
                DepartmentId = 1,
                RequestCategoryId = reqCatId,
                StatusId = statusId,
                CreatedBy = 1,
                CreatedByName = "test-user",
                CreatedIP = "127.0.0.1"
            };

            Func<Task> act = () => repo.UpdateAsync(header);
            await act.Should().ThrowAsync<KeyNotFoundException>();
        }

        [Fact]
        public async Task FinalizeStatus_Should_Update_Header_And_Details_Status()
        {
            await _fixture.ClearAllTablesAsync();
            await using var ctx = _fixture.CreateFreshDbContext();
            var (_, statusId, reasonId, reqCatId) = await SeedMiscMasterAsync(ctx);

            // Seed a second status (Approved) for finalization
            var approvedMisc = new PurchaseManagement.Domain.Entities.MiscMaster
            {
                MiscTypeId = (await ctx.MiscTypeMaster.FirstAsync()).Id,
                Code = "IR_APPROVED",
                Description = "Approved",
                SortOrder = 4,
                IsActive = Status.Active,
                IsDeleted = IsDelete.NotDeleted
            };
            ctx.MiscMaster.Add(approvedMisc);
            await ctx.SaveChangesAsync();
            ctx.ChangeTracker.Clear();

            var miscMock = BuildDefaultMiscMock(statusId);
            var repo = CreateRepo(ctx, miscMock);

            // Create header with Pending status
            var header = BuildHeader(statusId, reqCatId, reasonId, detailCount: 2);
            await repo.CreateAsync(header);
            ctx.ChangeTracker.Clear();

            // Finalize to Approved (no line-level list -> all lines get updated)
            var finalizeHeader = new IssueReturnHeader
            {
                Id = header.Id,
                StatusId = approvedMisc.Id
            };
            var result = await repo.FinalizeStatus(finalizeHeader);
            result.Should().BeTrue();
            ctx.ChangeTracker.Clear();

            var saved = await ctx.IssueReturnHeader
                .Include(h => h.IssueReturnDetailsHeaderName)
                .FirstAsync(h => h.Id == header.Id);

            saved.StatusId.Should().Be(approvedMisc.Id);
            saved.IssueReturnDetailsHeaderName!.Should().OnlyContain(d => d.StatusId == approvedMisc.Id);
        }

        [Fact]
        public async Task FinalizeStatus_Should_Return_False_When_NotFound()
        {
            await _fixture.ClearAllTablesAsync();
            await using var ctx = _fixture.CreateFreshDbContext();

            var repo = CreateRepo(ctx);

            var header = new IssueReturnHeader { Id = 9999, StatusId = 1 };
            var result = await repo.FinalizeStatus(header);
            result.Should().BeFalse();
        }

        [Fact]
        public async Task GenerateNextCodeAsync_Should_Return_Prefixed_Code()
        {
            await _fixture.ClearAllTablesAsync();
            await using var ctx = _fixture.CreateFreshDbContext();
            var (_, statusId, reasonId, reqCatId) = await SeedMiscMasterAsync(ctx);

            var miscMock = BuildDefaultMiscMock(statusId);
            var repo = CreateRepo(ctx, miscMock);

            var code = await repo.GenerateNextCodeAsync();

            // UnitId mock returns 1 → prefix is "RET-1-"
            code.Should().StartWith("RET-1-");
            code.Should().EndWith("01");
        }
    }
}
