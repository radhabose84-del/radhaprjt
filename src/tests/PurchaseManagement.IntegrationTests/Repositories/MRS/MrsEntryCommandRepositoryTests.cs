using Contracts.Interfaces;
using Microsoft.EntityFrameworkCore;
using PurchaseManagement.Application.Common.Interfaces.IMiscMaster;
using PurchaseManagement.Domain.Entities.MRS;
using PurchaseManagement.Infrastructure.Data;
using PurchaseManagement.Infrastructure.Repositories.MRS;
using static PurchaseManagement.Domain.Common.BaseEntity;

namespace PurchaseManagement.IntegrationTests.Repositories.MRS
{
    [Collection("DatabaseCollection")]
    public sealed class MrsEntryCommandRepositoryTests
    {
        private readonly DbFixture _fixture;
        public MrsEntryCommandRepositoryTests(DbFixture fixture) => _fixture = fixture;

        private MrsEntryCommandRepository CreateRepo(
            ApplicationDbContext ctx,
            Mock<IIPAddressService> ipMock = null,
            Mock<IMiscMasterQueryRepository> miscMock = null)
        {
            ipMock ??= new Mock<IIPAddressService>(MockBehavior.Loose);
            ipMock.Setup(x => x.GetUnitId()).Returns(1);
            ipMock.Setup(x => x.GetUserId()).Returns(1);
            ipMock.Setup(x => x.GetUserName()).Returns("test-user");
            ipMock.Setup(x => x.GetUserIPAddress()).Returns("127.0.0.1");

            miscMock ??= new Mock<IMiscMasterQueryRepository>(MockBehavior.Loose);
            miscMock.Setup(m => m.GetMiscMasterByName(It.IsAny<string>(), It.IsAny<string>()))
                .ReturnsAsync(new PurchaseManagement.Domain.Entities.MiscMaster { Id = 0 });

            return new MrsEntryCommandRepository(ctx, ipMock.Object, miscMock.Object);
        }

        /// <summary>
        /// Seeds MiscTypeMaster + MiscMaster rows needed for MrsHeader FK constraints
        /// (RequestCategoryId and StatusId both point to Purchase.MiscMaster).
        /// Returns (requestCategoryId, statusId).
        /// </summary>
        private async Task<(int RequestCategoryId, int StatusId)> EnsureMiscFksAsync()
        {
            await using var ctx = _fixture.CreateFreshDbContext();

            // MiscTypeMaster: ApprovalStatus
            var mtApproval = await ctx.MiscTypeMaster
                .FirstOrDefaultAsync(x => x.MiscTypeCode == "ApprovalStatus");
            if (mtApproval == null)
            {
                mtApproval = new PurchaseManagement.Domain.Entities.MiscTypeMaster
                {
                    MiscTypeCode = "ApprovalStatus",
                    Description = "Approval Status",
                    IsActive = Status.Active,
                    IsDeleted = IsDelete.NotDeleted
                };
                await ctx.MiscTypeMaster.AddAsync(mtApproval);
                await ctx.SaveChangesAsync();
            }

            // Pending status (used by CreateAsync to auto-assign StatusId)
            var pending = await ctx.MiscMaster
                .FirstOrDefaultAsync(x => x.Code == "Pending" && x.MiscTypeId == mtApproval.Id);
            if (pending == null)
            {
                pending = new PurchaseManagement.Domain.Entities.MiscMaster
                {
                    MiscTypeId = mtApproval.Id,
                    Code = "Pending",
                    Description = "Pending",
                    IsActive = Status.Active,
                    IsDeleted = IsDelete.NotDeleted
                };
                await ctx.MiscMaster.AddAsync(pending);
                await ctx.SaveChangesAsync();
            }

            // MiscTypeMaster: MaterialRequest (for RequestCategoryId)
            var mtReqCat = await ctx.MiscTypeMaster
                .FirstOrDefaultAsync(x => x.MiscTypeCode == "MRS_REQCAT");
            if (mtReqCat == null)
            {
                mtReqCat = new PurchaseManagement.Domain.Entities.MiscTypeMaster
                {
                    MiscTypeCode = "MRS_REQCAT",
                    Description = "MRS Request Category",
                    IsActive = Status.Active,
                    IsDeleted = IsDelete.NotDeleted
                };
                await ctx.MiscTypeMaster.AddAsync(mtReqCat);
                await ctx.SaveChangesAsync();
            }

            var reqCat = await ctx.MiscMaster
                .FirstOrDefaultAsync(x => x.Code == "MRS_NORMAL" && x.MiscTypeId == mtReqCat.Id);
            if (reqCat == null)
            {
                reqCat = new PurchaseManagement.Domain.Entities.MiscMaster
                {
                    MiscTypeId = mtReqCat.Id,
                    Code = "MRS_NORMAL",
                    Description = "Normal Request",
                    IsActive = Status.Active,
                    IsDeleted = IsDelete.NotDeleted
                };
                await ctx.MiscMaster.AddAsync(reqCat);
                await ctx.SaveChangesAsync();
            }

            return (reqCat.Id, pending.Id);
        }

        private MrsHeader BuildHeader(
            string mrsNo,
            int requestCategoryId,
            int statusId,
            int detailCount = 1)
        {
            return new MrsHeader
            {
                MrsNo = mrsNo,
                MrsDate = DateTimeOffset.UtcNow,
                UnitId = 1,
                RequestCategoryId = requestCategoryId,
                StatusId = statusId,
                DepartmentId = 10,
                SubDepartmentId = 20,
                Remarks = "MRS test",
                CreatedBy = 1,
                CreatedDate = DateTimeOffset.UtcNow,
                CreatedByName = "test-user",
                CreatedIP = "127.0.0.1",
                MrsDetailHeaderName = Enumerable.Range(1, detailCount).Select(i => new MrsDetail
                {
                    ItemId = i * 100,
                    UomId = 1,
                    RequestQuantity = 10m + i,
                    WarehouseStockId = 1
                }).ToList()
            };
        }

        // ---- CREATE ----

        [Fact]
        public async Task CreateAsync_Should_Persist_Header_And_Return_Entity()
        {
            await _fixture.ClearAllTablesAsync();
            var (reqCatId, _) = await EnsureMiscFksAsync();

            await using var ctx = _fixture.CreateFreshDbContext();
            // StatusId is irrelevant here; CreateAsync looks up Pending internally
            var header = BuildHeader("MRS_C1", reqCatId, statusId: 0, detailCount: 2);
            var result = await CreateRepo(ctx).CreateAsync(header);

            result.Id.Should().BeGreaterThan(0);
        }

        [Fact]
        public async Task CreateAsync_Should_Persist_Details()
        {
            await _fixture.ClearAllTablesAsync();
            var (reqCatId, _) = await EnsureMiscFksAsync();

            await using var ctx = _fixture.CreateFreshDbContext();
            var header = BuildHeader("MRS_C2", reqCatId, statusId: 0, detailCount: 3);
            var result = await CreateRepo(ctx).CreateAsync(header);
            ctx.ChangeTracker.Clear();

            var details = await ctx.MrsDetail
                .Where(d => d.MrsHeaderId == result.Id)
                .ToListAsync();

            details.Should().HaveCount(3);
        }

        [Fact]
        public async Task CreateAsync_Should_Set_Pending_StatusId()
        {
            await _fixture.ClearAllTablesAsync();
            var (reqCatId, pendingId) = await EnsureMiscFksAsync();

            await using var ctx = _fixture.CreateFreshDbContext();
            var header = BuildHeader("MRS_C3", reqCatId, statusId: 0);
            var result = await CreateRepo(ctx).CreateAsync(header);

            result.StatusId.Should().Be(pendingId);
        }

        // ---- UPDATE ----

        [Fact]
        public async Task UpdateAsync_Should_Persist_Changed_Fields()
        {
            await _fixture.ClearAllTablesAsync();
            var (reqCatId, _) = await EnsureMiscFksAsync();

            await using var ctx = _fixture.CreateFreshDbContext();
            var header = BuildHeader("MRS_U1", reqCatId, statusId: 0);
            await CreateRepo(ctx).CreateAsync(header);
            ctx.ChangeTracker.Clear();

            // Build update payload with new Remarks and replaced details
            var updateHeader = new MrsHeader
            {
                Id = header.Id,
                RequestCategoryId = reqCatId,
                DepartmentId = 99,
                SubDepartmentId = 88,
                Remarks = "Updated MRS",
                ModifiedBy = 2,
                ModifiedDate = DateTimeOffset.UtcNow,
                ModifiedIP = "10.0.0.1",
                ModifiedByName = "editor",
                MrsDetailHeaderName = new List<MrsDetail>
                {
                    new MrsDetail { MrsHeaderId = header.Id, ItemId = 500, UomId = 1, RequestQuantity = 50m, WarehouseStockId = 1 }
                }
            };

            var ok = await CreateRepo(ctx).UpdateAsync(updateHeader);
            ok.Should().BeTrue();
            ctx.ChangeTracker.Clear();

            var saved = await ctx.MrsHeader.FirstAsync(x => x.Id == header.Id);
            saved.Remarks.Should().Be("Updated MRS");
            saved.DepartmentId.Should().Be(99);
        }

        [Fact]
        public async Task UpdateAsync_Should_Replace_Details()
        {
            await _fixture.ClearAllTablesAsync();
            var (reqCatId, _) = await EnsureMiscFksAsync();

            await using var ctx = _fixture.CreateFreshDbContext();
            var header = BuildHeader("MRS_U2", reqCatId, statusId: 0, detailCount: 2);
            await CreateRepo(ctx).CreateAsync(header);
            ctx.ChangeTracker.Clear();

            var updateHeader = new MrsHeader
            {
                Id = header.Id,
                RequestCategoryId = reqCatId,
                DepartmentId = 10,
                SubDepartmentId = 20,
                Remarks = "replaced details",
                MrsDetailHeaderName = new List<MrsDetail>
                {
                    new MrsDetail { MrsHeaderId = header.Id, ItemId = 700, UomId = 1, RequestQuantity = 77m, WarehouseStockId = 1 }
                }
            };

            await CreateRepo(ctx).UpdateAsync(updateHeader);
            ctx.ChangeTracker.Clear();

            var details = await ctx.MrsDetail.Where(d => d.MrsHeaderId == header.Id).ToListAsync();
            details.Should().HaveCount(1);
            details[0].ItemId.Should().Be(700);
        }

        // ---- GENERATE NEXT CODE ----

        [Fact]
        public async Task GenerateNextCodeAsync_Should_Return_Incremented_Code()
        {
            await _fixture.ClearAllTablesAsync();
            var (reqCatId, _) = await EnsureMiscFksAsync();

            await using var ctx = _fixture.CreateFreshDbContext();
            var repo = CreateRepo(ctx);

            // Create one header so the sequence has a starting point
            var header = BuildHeader("MRS-1-01", reqCatId, statusId: 0);
            await repo.CreateAsync(header);

            var next = await repo.GenerateNextCodeAsync();
            next.Should().Be("MRS-1-02");
        }

        // ---- APPROVE ----

        [Fact]
        public async Task UpdateMrsApproveAsync_Should_Return_False_When_NotFound()
        {
            await _fixture.ClearAllTablesAsync();

            var miscMock = new Mock<IMiscMasterQueryRepository>(MockBehavior.Loose);
            miscMock.Setup(m => m.GetMiscMasterByName(It.IsAny<string>(), It.IsAny<string>()))
                .ReturnsAsync(new PurchaseManagement.Domain.Entities.MiscMaster { Id = 0 });

            await using var ctx = _fixture.CreateFreshDbContext();
            var result = await CreateRepo(ctx, miscMock: miscMock)
                .UpdateMrsApproveAsync(9999, 1, CancellationToken.None);

            result.Should().BeFalse();
        }
    }
}
