using Microsoft.EntityFrameworkCore;
using FAM.Domain.Common;
using FAM.Domain.Entities.AssetMaster;
using FAM.Infrastructure.Repositories.AssetMaster.AssetTransfer;
using FAM.Infrastructure.Repositories.AssetTransferIssueApproval;
using FAM.Infrastructure.Repositories.MiscMaster;
using FAM.Infrastructure.Repositories.MiscTypeMaster;

namespace FixedAssetManagement.IntegrationTests.Repositories.AssetTransferIssueApproval
{
    [Collection("DatabaseCollection")]
    public sealed class AssetTransferIssueCommandRepositoryTests
    {
        private readonly DbFixture _fixture;

        public AssetTransferIssueCommandRepositoryTests(DbFixture fixture)
        {
            _fixture = fixture;
        }

        private AssetTransferIssueCommandRepository CreateRepository(FAM.Infrastructure.Data.ApplicationDbContext ctx) =>
            new(ctx);

        private async Task<int> SeedMiscMasterAsync(string typeCode)
        {
            await using var ctx = _fixture.CreateFreshDbContext();
            var typeId = (await new MiscTypeMasterCommandRepository(ctx).CreateAsync(new FAM.Domain.Entities.MiscTypeMaster
            {
                MiscTypeCode = typeCode,
                Description = "T",
                IsActive = BaseEntity.Status.Active,
                IsDeleted = BaseEntity.IsDelete.NotDeleted
            })).Id;

            return (await new MiscMasterCommandRepository(ctx).CreateAsync(new FAM.Domain.Entities.MiscMaster
            {
                MiscTypeId = typeId,
                Code = "MM_" + typeCode,
                Description = "M",
                IsActive = BaseEntity.Status.Active,
                IsDeleted = BaseEntity.IsDelete.NotDeleted
            })).Id;
        }

        private async Task<int> SeedHdrAsync(string status = "Pending")
        {
            var ttId = await SeedMiscMasterAsync("ATA_TT_" + status.Substring(0, 3));
            await using var ctx = _fixture.CreateFreshDbContext();
            return await new AssetTransferCommandRepository(ctx).CreateAssetTransferAsync(new AssetTransferIssueHdr
            {
                DocDate = DateTimeOffset.UtcNow,
                TransferType = ttId,
                FromUnitId = 1,
                ToUnitId = 2,
                FromDepartmentId = 1,
                ToDepartmentId = 2,
                FromCustodianId = 1,
                ToCustodianId = 2,
                FromCustodianName = "From",
                ToCustodianName = "To",
                Status = status,
                AckStatus = 0,
                CreatedBy = 1,
                CreatedDate = DateTimeOffset.UtcNow,
                CreatedByName = "test-user",
                CreatedIP = "127.0.0.1"
            });
        }

        private async Task ClearTablesAsync(FAM.Infrastructure.Data.ApplicationDbContext ctx) =>
            await _fixture.ClearAllTablesAsync();

        [Fact]
        public async Task ExecuteBulkUpdateAsync_Should_Update_Pending_Records()
        {
            await using var ctx = _fixture.CreateFreshDbContext();
            await ClearTablesAsync(ctx);
            var id1 = await SeedHdrAsync();
            var id2 = await SeedHdrAsync();

            var count = await CreateRepository(ctx).ExecuteBulkUpdateAsync(
                new List<int> { id1, id2 },
                "Approved",
                userId: 1,
                currentTime: DateTimeOffset.UtcNow,
                username: "approver",
                currentIp: "127.0.0.1");

            count.Should().Be(2);

            ctx.ChangeTracker.Clear();
            var updated = await ctx.AssetTransferIssueHdr.FirstAsync(x => x.Id == id1);
            updated.Status.Should().Be("Approved");
            updated.AuthorizedBy.Should().Be(1);
            updated.AuthorizedByName.Should().Be("approver");
        }

        [Fact]
        public async Task ExecuteBulkUpdateAsync_Should_Skip_NonPending_Records()
        {
            await using var ctx = _fixture.CreateFreshDbContext();
            await ClearTablesAsync(ctx);
            var pendingId = await SeedHdrAsync("Pending");
            var approvedId = await SeedHdrAsync("Approved");

            var count = await CreateRepository(ctx).ExecuteBulkUpdateAsync(
                new List<int> { pendingId, approvedId },
                "Rejected",
                userId: 1,
                currentTime: DateTimeOffset.UtcNow,
                username: "user",
                currentIp: "127.0.0.1");

            count.Should().Be(1);
        }

        [Fact]
        public async Task ExecuteBulkUpdateAsync_Should_Return_Zero_When_NoMatch()
        {
            await using var ctx = _fixture.CreateFreshDbContext();
            await ClearTablesAsync(ctx);

            var count = await CreateRepository(ctx).ExecuteBulkUpdateAsync(
                new List<int> { 9999 },
                "Approved",
                userId: 1,
                currentTime: DateTimeOffset.UtcNow,
                username: "user",
                currentIp: "127.0.0.1");

            count.Should().Be(0);
        }

        [Fact]
        public async Task GetByIdsAsync_Should_Return_Pending_Records()
        {
            await using var ctx = _fixture.CreateFreshDbContext();
            await ClearTablesAsync(ctx);
            var id1 = await SeedHdrAsync("Pending");
            var id2 = await SeedHdrAsync("Pending");

            var result = await CreateRepository(ctx).GetByIdsAsync(new List<int> { id1, id2 });

            result.Should().HaveCount(2);
        }

        [Fact]
        public async Task GetByIdsAsync_Should_Exclude_NonPending()
        {
            await using var ctx = _fixture.CreateFreshDbContext();
            await ClearTablesAsync(ctx);
            var pendingId = await SeedHdrAsync("Pending");
            var approvedId = await SeedHdrAsync("Approved");

            var result = await CreateRepository(ctx).GetByIdsAsync(new List<int> { pendingId, approvedId });

            result.Should().HaveCount(1);
            result[0].Id.Should().Be(pendingId);
        }
    }
}
