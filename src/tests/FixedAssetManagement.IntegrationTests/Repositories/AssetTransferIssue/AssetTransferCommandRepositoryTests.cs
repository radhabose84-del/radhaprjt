using Microsoft.EntityFrameworkCore;
using FAM.Domain.Common;
using FAM.Domain.Entities.AssetMaster;
using FAM.Infrastructure.Repositories.AssetMaster.AssetTransfer;
using FAM.Infrastructure.Repositories.MiscMaster;
using FAM.Infrastructure.Repositories.MiscTypeMaster;

namespace FixedAssetManagement.IntegrationTests.Repositories.AssetTransferIssue
{
    [Collection("DatabaseCollection")]
    public sealed class AssetTransferCommandRepositoryTests
    {
        private readonly DbFixture _fixture;

        public AssetTransferCommandRepositoryTests(DbFixture fixture)
        {
            _fixture = fixture;
        }

        private AssetTransferCommandRepository CreateRepository(FAM.Infrastructure.Data.ApplicationDbContext ctx) =>
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

        private static AssetTransferIssueHdr BuildHdr(int transferTypeId, string status = "Pending") =>
            new AssetTransferIssueHdr
            {
                DocDate = DateTimeOffset.UtcNow,
                TransferType = transferTypeId,
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
            };

        private async Task ClearTablesAsync(FAM.Infrastructure.Data.ApplicationDbContext ctx) =>
            await _fixture.ClearAllTablesAsync();

        [Fact]
        public async Task CreateAssetTransferAsync_Should_Return_Id_GreaterThanZero()
        {
            await using var ctx = _fixture.CreateFreshDbContext();
            await ClearTablesAsync(ctx);
            var ttId = await SeedMiscMasterAsync("ATI_TT_C1");

            var newId = await CreateRepository(ctx).CreateAssetTransferAsync(BuildHdr(ttId));

            newId.Should().BeGreaterThan(0);
        }

        [Fact]
        public async Task CreateAssetTransferAsync_Should_Persist_Fields()
        {
            await using var ctx = _fixture.CreateFreshDbContext();
            await ClearTablesAsync(ctx);
            var ttId = await SeedMiscMasterAsync("ATI_TT_C2");

            var newId = await CreateRepository(ctx).CreateAssetTransferAsync(BuildHdr(ttId, "Approved"));
            ctx.ChangeTracker.Clear();

            var saved = await ctx.AssetTransferIssueHdr.FirstAsync(x => x.Id == newId);
            saved.Status.Should().Be("Approved");
            saved.FromUnitId.Should().Be(1);
            saved.ToUnitId.Should().Be(2);
        }

        [Fact]
        public async Task UpdateAssetTransferAsync_Should_Persist_Changes()
        {
            await using var ctx = _fixture.CreateFreshDbContext();
            await ClearTablesAsync(ctx);
            var ttId = await SeedMiscMasterAsync("ATI_TT_U1");
            var newId = await CreateRepository(ctx).CreateAssetTransferAsync(BuildHdr(ttId, "Pending"));
            ctx.ChangeTracker.Clear();

            var result = await CreateRepository(ctx).UpdateAssetTransferAsync(new AssetTransferIssueHdr
            {
                Id = newId,
                DocDate = DateTimeOffset.UtcNow,
                TransferType = ttId,
                FromUnitId = 1,
                ToUnitId = 3,
                FromDepartmentId = 1,
                ToDepartmentId = 4,
                FromCustodianId = 1,
                ToCustodianId = 5,
                FromCustodianName = "FromUpd",
                ToCustodianName = "ToUpd",
                Status = "Updated",
                ModifiedBy = 1,
                ModifiedDate = DateTimeOffset.UtcNow
            });
            ctx.ChangeTracker.Clear();

            // When no detail rows change, second SaveChangesAsync returns 0 → result is false.
            // Verify persisted state directly instead.
            var updated = await ctx.AssetTransferIssueHdr.FirstAsync(x => x.Id == newId);
            updated.Status.Should().Be("Updated");
            updated.ToUnitId.Should().Be(3);
        }
    }
}
