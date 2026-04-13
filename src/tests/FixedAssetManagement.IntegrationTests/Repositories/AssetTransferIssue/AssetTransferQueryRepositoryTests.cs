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

        private async Task<int> SeedHdrAsync(string status = "Pending")
        {
            var ttId = await SeedMiscMasterAsync("ATQ_TT_" + status.Substring(0, 3));
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
