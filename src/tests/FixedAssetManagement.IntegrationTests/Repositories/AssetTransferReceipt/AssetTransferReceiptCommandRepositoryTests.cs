using Microsoft.EntityFrameworkCore;
using FAM.Domain.Common;
using FAM.Domain.Entities.AssetMaster;
using FAM.Infrastructure.Repositories.AssetMaster.AssetTransfer;
using FAM.Infrastructure.Repositories.AssetTransferReceipt;
using FAM.Infrastructure.Repositories.MiscMaster;
using FAM.Infrastructure.Repositories.MiscTypeMaster;

namespace FixedAssetManagement.IntegrationTests.Repositories.AssetTransferReceipt
{
    [Collection("DatabaseCollection")]
    public sealed class AssetTransferReceiptCommandRepositoryTests
    {
        private readonly DbFixture _fixture;

        public AssetTransferReceiptCommandRepositoryTests(DbFixture fixture)
        {
            _fixture = fixture;
        }

        private AssetTransferReceiptCommandRepository CreateRepository(FAM.Infrastructure.Data.ApplicationDbContext ctx) =>
            new(ctx);

        private async Task<int> SeedMiscMasterAsync(string typeCode)
        {
            await using var ctx = _fixture.CreateFreshDbContext();
            var typeId = (await new MiscTypeMasterCommandRepository(ctx).CreateAsync(new FAM.Domain.Entities.MiscTypeMaster
            {
                MiscTypeCode = typeCode, Description = "T",
                IsActive = BaseEntity.Status.Active, IsDeleted = BaseEntity.IsDelete.NotDeleted
            })).Id;
            return (await new MiscMasterCommandRepository(ctx).CreateAsync(new FAM.Domain.Entities.MiscMaster
            {
                MiscTypeId = typeId, Code = "MM_" + typeCode, Description = "M",
                IsActive = BaseEntity.Status.Active, IsDeleted = BaseEntity.IsDelete.NotDeleted
            })).Id;
        }

        private async Task<int> SeedTransferHdrAsync(string status = "Approved")
        {
            var ttId = await SeedMiscMasterAsync("ATR_TT_" + status[..3]);
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
        public async Task CreateAsync_Should_Insert_New_Receipt_When_NotExists()
        {
            await using var ctx = _fixture.CreateFreshDbContext();
            await ClearTablesAsync(ctx);
            var transferId = await SeedTransferHdrAsync();

            var hdr = new AssetTransferReceiptHdr
            {
                AssetTransferId = transferId,
                DocDate = DateTimeOffset.UtcNow,
                Sdcno = "SDC001",
                GatePassNo = "GP001",
                Remarks = "First receipt",
                AuthorizedBy = 1,
                AuthorizedByName = "test-user",
                AuthorizedDate = DateTimeOffset.UtcNow,
                AuthorizedIP = "127.0.0.1",
                AssetTransferReceiptDtl = new List<AssetTransferReceiptDtl>()
            };

            var resultId = await CreateRepository(ctx).CreateAsync(hdr, new List<FAM.Domain.Entities.AssetMaster.AssetLocation>());

            resultId.Should().BeGreaterThan(0);

            ctx.ChangeTracker.Clear();
            var saved = await ctx.AssetTransferReceiptHdr.FirstAsync(x => x.AssetTransferId == transferId);
            saved.Sdcno.Should().Be("SDC001");
        }

        [Fact]
        public async Task CreateAsync_Should_Persist_Sdcno_And_Remarks()
        {
            await using var ctx = _fixture.CreateFreshDbContext();
            await ClearTablesAsync(ctx);
            var transferId = await SeedTransferHdrAsync();

            var hdr = new AssetTransferReceiptHdr
            {
                AssetTransferId = transferId,
                DocDate = DateTimeOffset.UtcNow,
                Sdcno = "SDC_PERSIST",
                Remarks = "Test remarks",
                AuthorizedBy = 1,
                AuthorizedByName = "test-user",
                AuthorizedDate = DateTimeOffset.UtcNow,
                AuthorizedIP = "127.0.0.1",
                AssetTransferReceiptDtl = new List<AssetTransferReceiptDtl>()
            };

            await CreateRepository(ctx).CreateAsync(hdr, new List<FAM.Domain.Entities.AssetMaster.AssetLocation>());
            ctx.ChangeTracker.Clear();

            var saved = await ctx.AssetTransferReceiptHdr.FirstAsync(x => x.AssetTransferId == transferId);
            saved.Sdcno.Should().Be("SDC_PERSIST");
            saved.Remarks.Should().Be("Test remarks");
        }

        [Fact]
        public async Task CreateAsync_With_Existing_Receipt_Should_Return_Existing_Id()
        {
            await using var ctx = _fixture.CreateFreshDbContext();
            await ClearTablesAsync(ctx);
            var transferId = await SeedTransferHdrAsync();

            // First create
            var firstHdr = new AssetTransferReceiptHdr
            {
                AssetTransferId = transferId,
                DocDate = DateTimeOffset.UtcNow,
                Sdcno = "SDC1",
                AuthorizedBy = 1,
                AuthorizedByName = "test-user",
                AuthorizedDate = DateTimeOffset.UtcNow,
                AuthorizedIP = "127.0.0.1",
                AssetTransferReceiptDtl = new List<AssetTransferReceiptDtl>()
            };
            var firstId = await CreateRepository(ctx).CreateAsync(firstHdr, new List<FAM.Domain.Entities.AssetMaster.AssetLocation>());

            // Second create with same AssetTransferId — should return existing id
            ctx.ChangeTracker.Clear();
            var secondHdr = new AssetTransferReceiptHdr
            {
                AssetTransferId = transferId,
                DocDate = DateTimeOffset.UtcNow,
                AuthorizedBy = 1,
                AuthorizedByName = "test-user",
                AuthorizedDate = DateTimeOffset.UtcNow,
                AuthorizedIP = "127.0.0.1",
                AssetTransferReceiptDtl = new List<AssetTransferReceiptDtl>()
            };
            var secondId = await CreateRepository(ctx).CreateAsync(secondHdr, new List<FAM.Domain.Entities.AssetMaster.AssetLocation>());

            secondId.Should().Be(firstId);
        }
    }
}
