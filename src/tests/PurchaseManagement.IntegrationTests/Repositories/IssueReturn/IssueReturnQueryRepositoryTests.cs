using Contracts.Interfaces;
using Dapper;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using PurchaseManagement.Application.Common.Interfaces.IMiscMaster;
using PurchaseManagement.Domain.Common;
using PurchaseManagement.Domain.Entities.IssueReturn;
using PurchaseManagement.Infrastructure.Data;
using PurchaseManagement.Infrastructure.Repositories.IssueReturn;
using static PurchaseManagement.Domain.Common.BaseEntity;

namespace PurchaseManagement.IntegrationTests.Repositories.IssueReturn
{
    [Collection("DatabaseCollection")]
    public sealed class IssueReturnQueryRepositoryTests
    {
        private readonly DbFixture _fixture;

        public IssueReturnQueryRepositoryTests(DbFixture fixture) => _fixture = fixture;

        // --- Helpers ---

        private IssueReturnEntryQueryRepository CreateQueryRepo()
        {
            var conn = new SqlConnection(_fixture.ConnectionString);
            return new IssueReturnEntryQueryRepository(conn, _fixture.IpMock.Object);
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
        /// Seeds MiscTypeMaster + MiscMaster rows.
        /// Returns (statusId, reasonId, requestCategoryId).
        /// </summary>
        private async Task<(int StatusId, int ReasonId, int RequestCategoryId)>
            SeedMiscMasterAsync(ApplicationDbContext ctx)
        {
            var miscType = new PurchaseManagement.Domain.Entities.MiscTypeMaster
            {
                MiscTypeCode = "IR_Q_TYPE",
                Description = "IR Query Type",
                IsActive = Status.Active,
                IsDeleted = IsDelete.NotDeleted
            };
            ctx.MiscTypeMaster.Add(miscType);
            await ctx.SaveChangesAsync();

            var statusMisc = new PurchaseManagement.Domain.Entities.MiscMaster
            {
                MiscTypeId = miscType.Id,
                Code = "IR_Q_PEND",
                Description = "Pending",
                SortOrder = 1,
                IsActive = Status.Active,
                IsDeleted = IsDelete.NotDeleted
            };
            var reasonMisc = new PurchaseManagement.Domain.Entities.MiscMaster
            {
                MiscTypeId = miscType.Id,
                Code = "IR_Q_RSN",
                Description = "Damaged",
                SortOrder = 2,
                IsActive = Status.Active,
                IsDeleted = IsDelete.NotDeleted
            };
            var reqCatMisc = new PurchaseManagement.Domain.Entities.MiscMaster
            {
                MiscTypeId = miscType.Id,
                Code = "IR_Q_RCAT",
                Description = "Consumption",
                SortOrder = 3,
                IsActive = Status.Active,
                IsDeleted = IsDelete.NotDeleted
            };
            ctx.MiscMaster.AddRange(statusMisc, reasonMisc, reqCatMisc);
            await ctx.SaveChangesAsync();
            ctx.ChangeTracker.Clear();

            return (statusMisc.Id, reasonMisc.Id, reqCatMisc.Id);
        }

        /// <summary>
        /// Seeds an IssueReturnHeader + details via EF Core (bypasses command repo's IMiscMaster dependency).
        /// </summary>
        private async Task<int> SeedIssueReturnAsync(
            ApplicationDbContext ctx,
            int statusId,
            int reasonId,
            int requestCategoryId,
            string issueReturnNo = "IR_Q_RET001",
            int detailCount = 1)
        {
            var header = new IssueReturnHeader
            {
                UnitId = 1,
                IssueReturnNo = issueReturnNo,
                IssueReturnDate = DateTimeOffset.UtcNow,
                DepartmentId = 1,
                RequestCategoryId = requestCategoryId,
                StatusId = statusId,
                Remarks = "query test",
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
                        Remarks = $"q-detail-{i}",
                        CreatedBy = 1,
                        CreatedDate = DateTimeOffset.UtcNow,
                        CreatedByName = "test-user",
                        CreatedIP = "127.0.0.1"
                    }).ToList()
            };

            ctx.IssueReturnHeader.Add(header);
            await ctx.SaveChangesAsync();
            ctx.ChangeTracker.Clear();
            return header.Id;
        }

        // --- TESTS ---

        [Fact]
        public async Task GetByIdWithDetails_Should_Return_Header_With_Details()
        {
            await _fixture.ClearAllTablesAsync();
            await using var ctx = _fixture.CreateFreshDbContext();
            var (statusId, reasonId, reqCatId) = await SeedMiscMasterAsync(ctx);

            var headerId = await SeedIssueReturnAsync(ctx, statusId, reasonId, reqCatId,
                detailCount: 2);

            var queryRepo = CreateQueryRepo();
            var dto = await queryRepo.GetByIdWithDetails(headerId);

            dto.Should().NotBeNull();
            dto.Id.Should().Be(headerId);
            dto.RequestCategoryName.Should().Be("Consumption");
            dto.StatusName.Should().Be("Pending");
            dto.getIssueReturnDetails.Should().HaveCount(2);
        }

        [Fact]
        public async Task GetByIdWithDetails_Should_Populate_Detail_StatusName()
        {
            await _fixture.ClearAllTablesAsync();
            await using var ctx = _fixture.CreateFreshDbContext();
            var (statusId, reasonId, reqCatId) = await SeedMiscMasterAsync(ctx);

            var headerId = await SeedIssueReturnAsync(ctx, statusId, reasonId, reqCatId);

            var queryRepo = CreateQueryRepo();
            var dto = await queryRepo.GetByIdWithDetails(headerId);

            dto.getIssueReturnDetails.Should().NotBeNull();
            dto.getIssueReturnDetails!.First().StatusName.Should().Be("Pending");
        }

        [Fact]
        public async Task GetByIdWithDetails_Should_Return_Correct_HeaderFields()
        {
            await _fixture.ClearAllTablesAsync();
            await using var ctx = _fixture.CreateFreshDbContext();
            var (statusId, reasonId, reqCatId) = await SeedMiscMasterAsync(ctx);

            var headerId = await SeedIssueReturnAsync(ctx, statusId, reasonId, reqCatId,
                issueReturnNo: "IR_Q_FIELDS");

            var queryRepo = CreateQueryRepo();
            var dto = await queryRepo.GetByIdWithDetails(headerId);

            dto.IssueReturnNo.Should().Be("IR_Q_FIELDS");
            dto.DepartmentId.Should().Be(1);
            dto.UnitId.Should().Be(1);
            dto.CreatedByName.Should().Be("test-user");
        }

        [Fact]
        public async Task GetByIdWithDetails_Should_Return_Detail_Quantities()
        {
            await _fixture.ClearAllTablesAsync();
            await using var ctx = _fixture.CreateFreshDbContext();
            var (statusId, reasonId, reqCatId) = await SeedMiscMasterAsync(ctx);

            var headerId = await SeedIssueReturnAsync(ctx, statusId, reasonId, reqCatId);

            var queryRepo = CreateQueryRepo();
            var dto = await queryRepo.GetByIdWithDetails(headerId);

            var detail = dto.getIssueReturnDetails!.First();
            detail.ReturnQuantity.Should().Be(5m);
            detail.ReturnValue.Should().Be(50m);
            detail.ItemId.Should().Be(10);
        }
    }
}
