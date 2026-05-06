using Contracts.Interfaces;
using Contracts.Interfaces.Lookups.Users;
using Dapper;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using FAM.Domain.Common;
using FAM.Domain.Entities.AssetMaster;
using FAM.Infrastructure.Repositories.AssetMaster.AssetTransfer;
using FAM.Infrastructure.Repositories.AssetMaster.AssetTransferIssue;
using FAM.Infrastructure.Repositories.MiscMaster;
using FAM.Infrastructure.Repositories.MiscTypeMaster;

namespace FixedAssetManagement.IntegrationTests.Repositories.AssetTransferIssue
{
    [Collection("DatabaseCollection")]
    public sealed class AssetTransferQueryRepositoryTests
    {
        private readonly DbFixture _fixture;

        public AssetTransferQueryRepositoryTests(DbFixture fixture)
        {
            _fixture = fixture;
        }

        private AssetTransferQueryRepository CreateQueryRepo()
        {
            var conn = new SqlConnection(_fixture.ConnectionString);
            var ipMock = new Mock<IIPAddressService>(MockBehavior.Loose);
            ipMock.Setup(x => x.GetUnitId()).Returns(1);
            ipMock.Setup(x => x.GetCompanyId()).Returns(1);
            var unitLookup = new Mock<IUnitLookup>(MockBehavior.Loose);
            var deptLookup = new Mock<IDepartmentLookup>(MockBehavior.Loose);
            return new AssetTransferQueryRepository(conn, ipMock.Object, unitLookup.Object, deptLookup.Object);
        }

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

        private async Task ClearTablesAsync() =>
            await _fixture.ClearAllTablesAsync();

        private async Task<int> SeedHdrAsync(string status = "Pending", byte ackStatus = 0)
        {
            // Unique short MiscType code so concurrent / repeated seeds within a test don't collide
            var ttId = await SeedMiscMasterAsync($"ATQ_{Guid.NewGuid().ToString("N").Substring(0, 8)}");
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
                AckStatus = ackStatus,
                CreatedBy = 1,
                CreatedDate = DateTimeOffset.UtcNow,
                CreatedByName = "test-user",
                CreatedIP = "127.0.0.1"
            });
        }

        [Fact]
        public async Task GetTransferTypeAsync_Should_Return_Empty_When_No_MiscType_Match()
        {
            await ClearTablesAsync();

            var result = await CreateQueryRepo().GetTransferTypeAsync();

            result.Should().BeEmpty();
        }

        [Fact]
        public async Task IsAssetPendingOrApprovedAsync_Should_Return_False_When_NoData()
        {
            await ClearTablesAsync();

            (await CreateQueryRepo().IsAssetPendingOrApprovedAsync(9999)).Should().BeFalse();
        }

        // SCRUM-1463 — duplicate-asset guard SQL behaviour

        [Fact]
        public async Task IsAssetPendingOrApprovedAsync_Should_Return_True_When_Pending_Transfer_Exists()
        {
            await ClearTablesAsync();
            var hdrId = await SeedHdrAsync(status: "Pending");
            await SeedTransferDtlRawAsync(hdrId: hdrId, assetId: 7777);

            var result = await CreateQueryRepo().IsAssetPendingOrApprovedAsync(7777);

            result.Should().BeTrue();
        }

        [Fact]
        public async Task IsAssetPendingOrApprovedAsync_Should_Return_True_When_Approved_But_Not_Acknowledged()
        {
            await ClearTablesAsync();
            var hdrId = await SeedHdrAsync(status: "Approved", ackStatus: 0);
            await SeedTransferDtlRawAsync(hdrId: hdrId, assetId: 7778);

            var result = await CreateQueryRepo().IsAssetPendingOrApprovedAsync(7778);

            result.Should().BeTrue();
        }

        [Fact]
        public async Task IsAssetPendingOrApprovedAsync_Should_Return_False_When_Approved_And_Acknowledged()
        {
            await ClearTablesAsync();
            var hdrId = await SeedHdrAsync(status: "Approved", ackStatus: 1);
            await SeedTransferDtlRawAsync(hdrId: hdrId, assetId: 7779);

            var result = await CreateQueryRepo().IsAssetPendingOrApprovedAsync(7779);

            result.Should().BeFalse();
        }

        [Fact]
        public async Task IsAssetPendingOrApprovedAsync_Should_Return_False_When_Rejected()
        {
            await ClearTablesAsync();
            var hdrId = await SeedHdrAsync(status: "Rejected");
            await SeedTransferDtlRawAsync(hdrId: hdrId, assetId: 7780);

            var result = await CreateQueryRepo().IsAssetPendingOrApprovedAsync(7780);

            result.Should().BeFalse();
        }

        [Fact]
        public async Task IsAssetPendingOrApprovedAsync_Should_Honour_ExcludeTransferId()
        {
            await ClearTablesAsync();
            var hdrId = await SeedHdrAsync(status: "Pending");
            await SeedTransferDtlRawAsync(hdrId: hdrId, assetId: 7781);

            // Excluding the only blocking row → no other active rows for this asset
            var result = await CreateQueryRepo().IsAssetPendingOrApprovedAsync(7781, excludeTransferId: hdrId);

            result.Should().BeFalse();
        }

        [Fact]
        public async Task IsAssetPendingOrApprovedAsync_Should_Return_False_For_Different_Asset()
        {
            await ClearTablesAsync();
            var hdrId = await SeedHdrAsync(status: "Pending");
            await SeedTransferDtlRawAsync(hdrId: hdrId, assetId: 7782);

            var result = await CreateQueryRepo().IsAssetPendingOrApprovedAsync(9999);

            result.Should().BeFalse();
        }

        // Inserts an AssetTransferIssueDtl row directly via raw SQL with FK constraints disabled,
        // since the AssetMaster → AssetCategory → … chain isn't relevant to the IsAssetPendingOrApprovedAsync
        // query (it joins only Hdr ↔ Dtl by AssetTransferId / AssetId).
        private async Task SeedTransferDtlRawAsync(int hdrId, int assetId, decimal assetValue = 1000m)
        {
            await using var conn = new SqlConnection(_fixture.ConnectionString);
            await conn.OpenAsync();

            await conn.ExecuteAsync(
                "ALTER TABLE FixedAsset.AssetTransferIssueDtl NOCHECK CONSTRAINT ALL;");

            const string sql = @"
                INSERT INTO FixedAsset.AssetTransferIssueDtl (AssetTransferId, AssetId, AssetValue)
                VALUES (@AssetTransferId, @AssetId, @AssetValue);";

            await conn.ExecuteAsync(sql, new { AssetTransferId = hdrId, AssetId = assetId, AssetValue = assetValue });

            // Re-enable constraints WITHOUT revalidating existing rows. Using `WITH CHECK`
            // forces SQL Server to validate every row against every FK, which fails because
            // the seed row uses a fake AssetId that has no AssetMaster parent (by design —
            // the query under test never joins to AssetMaster). The constraint becomes
            // "not trusted" but is still enforced for future inserts/updates.
            await conn.ExecuteAsync(
                "ALTER TABLE FixedAsset.AssetTransferIssueDtl CHECK CONSTRAINT ALL;");
        }

        [Fact]
        public async Task GetAssetTransferByIDAsync_Should_Return_Empty_When_NoMatch()
        {
            await ClearTablesAsync();

            var result = await CreateQueryRepo().GetAssetTransferByIDAsync(9999);

            result.Should().BeEmpty();
        }

        [Fact]
        public async Task DepartmentSoftDeleteValidation_Should_Return_False_When_NotLinked()
        {
            await ClearTablesAsync();

            (await CreateQueryRepo().DepartmentSoftDeleteValidation(9999)).Should().BeFalse();
        }

        [Fact]
        public async Task GetCategoriesByDepartmentAsync_Should_Return_Empty_When_NoData()
        {
            await ClearTablesAsync();

            var result = await CreateQueryRepo().GetCategoriesByDepartmentAsync(9999);

            result.Should().BeEmpty();
        }

        [Fact]
        public async Task GetAssetsByCategoryAsync_Should_Return_Empty_When_NoData()
        {
            await ClearTablesAsync();

            var result = await CreateQueryRepo().GetAssetsByCategoryAsync(9999, 9999);

            result.Should().BeEmpty();
        }

        [Fact]
        public async Task GetAssetTransferByIdAsync_Should_Return_Null_When_NotFound()
        {
            await ClearTablesAsync();

            var result = await CreateQueryRepo().GetAssetTransferByIdAsync(9999);

            result.Should().BeNull();
        }
    }
}
