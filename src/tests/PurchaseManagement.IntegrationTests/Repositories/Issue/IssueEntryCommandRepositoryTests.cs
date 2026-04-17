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
    public sealed class IssueEntryCommandRepositoryTests
    {
        private readonly DbFixture _fixture;

        public IssueEntryCommandRepositoryTests(DbFixture fixture) => _fixture = fixture;

        private IssueEntryCommandRepository CreateRepo(ApplicationDbContext ctx)
        {
            var ipMock = new Mock<IIPAddressService>(MockBehavior.Loose);
            ipMock.Setup(x => x.GetUnitId()).Returns(1);
            ipMock.Setup(x => x.GetUserId()).Returns(1);
            ipMock.Setup(x => x.GetUserName()).Returns("test-user");
            ipMock.Setup(x => x.GetSystemIPAddress()).Returns("127.0.0.1");
            return new IssueEntryCommandRepository(ctx, ipMock.Object);
        }

        /// <summary>
        /// Seeds MiscTypeMaster + two MiscMaster records (for StatusId and RequestCategoryId).
        /// Returns (statusMiscId, requestCategoryMiscId).
        /// </summary>
        private async Task<(int statusId, int requestCategoryId)> SeedMiscDataAsync(ApplicationDbContext ctx)
        {
            var miscType = new MiscTypeMasterEntity
            {
                MiscTypeCode = "ISS_MT01",
                Description = "ISS_TestType",
                IsActive = Status.Active,
                IsDeleted = IsDelete.NotDeleted
            };
            await ctx.Set<MiscTypeMasterEntity>().AddAsync(miscType);
            await ctx.SaveChangesAsync();

            var statusMisc = new MiscMasterEntity
            {
                MiscTypeId = miscType.Id,
                Code = "ISS_STAT",
                Description = "Approved",
                SortOrder = 1,
                IsActive = Status.Active,
                IsDeleted = IsDelete.NotDeleted
            };
            var reqCatMisc = new MiscMasterEntity
            {
                MiscTypeId = miscType.Id,
                Code = "ISS_RCAT",
                Description = "Consumption",
                SortOrder = 2,
                IsActive = Status.Active,
                IsDeleted = IsDelete.NotDeleted
            };
            await ctx.Set<MiscMasterEntity>().AddRangeAsync(statusMisc, reqCatMisc);
            await ctx.SaveChangesAsync();

            return (statusMisc.Id, reqCatMisc.Id);
        }

        /// <summary>
        /// Seeds an MrsHeader record (required FK parent for IssueHeader).
        /// Returns the MrsHeader Id.
        /// </summary>
        private async Task<int> SeedMrsHeaderAsync(ApplicationDbContext ctx, int statusId, int requestCategoryId)
        {
            var mrsHeader = new MrsHeader
            {
                UnitId = 1,
                RequestCategoryId = requestCategoryId,
                MrsNo = "ISS_MRS001",
                MrsDate = DateTimeOffset.UtcNow,
                DepartmentId = 1,
                SubDepartmentId = 1,
                StatusId = statusId,
                CreatedBy = 1,
                CreatedDate = DateTimeOffset.UtcNow,
                CreatedByName = "test-user",
                CreatedIP = "127.0.0.1",
                Remarks = "test mrs"
            };
            await ctx.Set<MrsHeader>().AddAsync(mrsHeader);
            await ctx.SaveChangesAsync();
            return mrsHeader.Id;
        }

        private IssueHeader BuildIssueHeader(int mrsHeaderId, string issueNo = "ISS_001", int detailCount = 1)
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
            return header;
        }

        // --- CREATE ---

        [Fact]
        public async Task CreateIssueAsync_Should_Return_NewId_GreaterThanZero()
        {
            await _fixture.ClearAllTablesAsync();
            await using var ctx = _fixture.CreateFreshDbContext();

            var (statusId, reqCatId) = await SeedMiscDataAsync(ctx);
            var mrsId = await SeedMrsHeaderAsync(ctx, statusId, reqCatId);

            var entity = BuildIssueHeader(mrsId, "ISS_C1");
            var newId = await CreateRepo(ctx).CreateIssueAsync(entity);

            newId.Should().BeGreaterThan(0);
        }

        [Fact]
        public async Task CreateIssueAsync_Should_Persist_Header_And_Details()
        {
            await _fixture.ClearAllTablesAsync();
            await using var ctx = _fixture.CreateFreshDbContext();

            var (statusId, reqCatId) = await SeedMiscDataAsync(ctx);
            var mrsId = await SeedMrsHeaderAsync(ctx, statusId, reqCatId);

            var entity = BuildIssueHeader(mrsId, "ISS_C2", detailCount: 3);
            await CreateRepo(ctx).CreateIssueAsync(entity);
            ctx.ChangeTracker.Clear();

            var savedHeader = await ctx.Set<IssueHeader>().FirstOrDefaultAsync(x => x.Id == entity.Id);
            savedHeader.Should().NotBeNull();
            savedHeader!.IssueNo.Should().Be("ISS_C2");
            savedHeader.MrsHeaderId.Should().Be(mrsId);
            savedHeader.UnitId.Should().Be(1);

            var savedDetails = await ctx.Set<IssueDetail>()
                .Where(d => d.IssueHeaderId == entity.Id).ToListAsync();
            savedDetails.Should().HaveCount(3);
        }

        [Fact]
        public async Task CreateIssueAsync_Should_Persist_Detail_Fields_Correctly()
        {
            await _fixture.ClearAllTablesAsync();
            await using var ctx = _fixture.CreateFreshDbContext();

            var (statusId, reqCatId) = await SeedMiscDataAsync(ctx);
            var mrsId = await SeedMrsHeaderAsync(ctx, statusId, reqCatId);

            var entity = BuildIssueHeader(mrsId, "ISS_C3", detailCount: 1);
            await CreateRepo(ctx).CreateIssueAsync(entity);
            ctx.ChangeTracker.Clear();

            var detail = await ctx.Set<IssueDetail>()
                .FirstOrDefaultAsync(d => d.IssueHeaderId == entity.Id);

            detail.Should().NotBeNull();
            detail!.ItemId.Should().Be(10);
            detail.UomId.Should().Be(1);
            detail.IssueQuantity.Should().Be(50m);
            detail.IssueValue.Should().Be(500m);
            detail.RequestQuantity.Should().Be(100m);
        }

        [Fact]
        public async Task CreateIssueAsync_Should_Invoke_PublishEvents_Callback()
        {
            await _fixture.ClearAllTablesAsync();
            await using var ctx = _fixture.CreateFreshDbContext();

            var (statusId, reqCatId) = await SeedMiscDataAsync(ctx);
            var mrsId = await SeedMrsHeaderAsync(ctx, statusId, reqCatId);

            var entity = BuildIssueHeader(mrsId, "ISS_C4");
            var callbackInvoked = false;

            await CreateRepo(ctx).CreateIssueAsync(entity, async () =>
            {
                callbackInvoked = true;
                await Task.CompletedTask;
            });

            callbackInvoked.Should().BeTrue();
        }

        // --- GENERATE NEXT CODE ---

        [Fact]
        public async Task GenerateNextCodeAsync_Should_Return_First_Code_When_No_Existing()
        {
            await _fixture.ClearAllTablesAsync();
            await using var ctx = _fixture.CreateFreshDbContext();

            var result = await CreateRepo(ctx).GenerateNextCodeAsync();

            // UnitId=1 so prefix is "ISS-1-", first code is "ISS-1-01"
            result.Should().Be("ISS-1-01");
        }

        [Fact]
        public async Task GenerateNextCodeAsync_Should_Increment_After_Existing_Records()
        {
            await _fixture.ClearAllTablesAsync();
            await using var ctx = _fixture.CreateFreshDbContext();

            var (statusId, reqCatId) = await SeedMiscDataAsync(ctx);
            var mrsId = await SeedMrsHeaderAsync(ctx, statusId, reqCatId);

            // Seed two existing issue headers with the expected prefix pattern
            var h1 = BuildIssueHeader(mrsId, "ISS-1-01");
            var h2 = BuildIssueHeader(mrsId, "ISS-1-02");
            await CreateRepo(ctx).CreateIssueAsync(h1);
            await CreateRepo(ctx).CreateIssueAsync(h2);

            var result = await CreateRepo(ctx).GenerateNextCodeAsync();

            result.Should().Be("ISS-1-03");
        }
    }
}
