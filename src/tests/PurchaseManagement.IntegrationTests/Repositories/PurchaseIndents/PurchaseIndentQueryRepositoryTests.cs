using Contracts.Common;
using Contracts.Dtos.Lookups.Users;
using Contracts.Interfaces;
using Contracts.Interfaces.Lookups.Users;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using PurchaseManagement.Domain.Entities;
using PurchaseManagement.Infrastructure.Data;
using PurchaseManagement.Infrastructure.Repositories.PurchaseIndents;
using PurchaseManagement.IntegrationTests.Common;
using static PurchaseManagement.Domain.Common.BaseEntity;

namespace PurchaseManagement.IntegrationTests.Repositories.PurchaseIndents
{
    [Collection("DatabaseCollection")]
    public sealed class PurchaseIndentQueryRepositoryTests
    {
        private readonly DbFixture _fixture;
        public PurchaseIndentQueryRepositoryTests(DbFixture fixture) => _fixture = fixture;

        private PurchaseIndentQueryRepository CreateRepo(Mock<IUnitLookup>? unitLookup = null)
        {
            // Only apply default setup when caller did NOT supply a custom mock.
            // Overriding a passed-in mock would clobber test-specific setups.
            if (unitLookup == null)
            {
                unitLookup = new Mock<IUnitLookup>(MockBehavior.Loose);
                unitLookup.Setup(l => l.GetByIdAsync(It.IsAny<int>(), It.IsAny<CancellationToken>()))
                    .ReturnsAsync(new UnitLookupDto { UnitId = 1, ShortName = "U1", UnitName = "Unit One" });
            }

            return new PurchaseIndentQueryRepository(
                new SqlConnection(_fixture.ConnectionString),
                _fixture.IpMock.Object,
                unitLookup.Object);
        }

        private async Task<int> EnsureMiscIdAsync(string code, string typeCode = "PIQ_TT")
        {
            await using var ctx = _fixture.CreateFreshDbContext();
            var t = await ctx.MiscTypeMaster.FirstOrDefaultAsync(x => x.MiscTypeCode == typeCode);
            if (t == null)
            {
                t = new PurchaseManagement.Domain.Entities.MiscTypeMaster
                {
                    MiscTypeCode = typeCode, Description = typeCode,
                    IsActive = Status.Active, IsDeleted = IsDelete.NotDeleted
                };
                await ctx.MiscTypeMaster.AddAsync(t);
                await ctx.SaveChangesAsync();
            }
            var m = await ctx.MiscMaster.FirstOrDefaultAsync(x => x.Code == code && x.MiscTypeId == t.Id);
            if (m == null)
            {
                m = new PurchaseManagement.Domain.Entities.MiscMaster
                {
                    MiscTypeId = t.Id, Code = code, Description = code,
                    IsActive = Status.Active, IsDeleted = IsDelete.NotDeleted
                };
                await ctx.MiscMaster.AddAsync(m);
                await ctx.SaveChangesAsync();
            }
            return m.Id;
        }

        private async Task<int> SeedHeaderAsync(
            string indentNumber,
            int? statusId = null,
            int? typeId = null,
            IsDelete deleted = IsDelete.NotDeleted,
            bool withDetail = true)
        {
            statusId ??= await EnsureMiscIdAsync("PIQ_OPEN", "PIQ_ST");
            typeId ??= await EnsureMiscIdAsync("PIQ_NORMAL", "PIQ_TT");

            await using var ctx = _fixture.CreateFreshDbContext();
            var h = new IndentHeader
            {
                IndentNumber = indentNumber,
                IndentDate = DateOnly.FromDateTime(DateTime.UtcNow),
                IndentTypeId = typeId.Value,
                UnitId = 1,
                DepartmentId = 1,
                Purpose = "Test",
                StatusId = statusId.Value,
                IsActive = Status.Active,
                IsDeleted = deleted,
                IndentDetails = withDetail
                    ? new List<IndentDetail>
                    {
                        new()
                        {
                            ItemId = 1, ItemCategoryId = 1, ItemUOMId = 1,
                            Rate = 10m, QuantityRequired = 5m,
                            RequiredDate = DateOnly.FromDateTime(DateTime.UtcNow.AddDays(7)),
                            TotalEstimatedCost = 50m, PRConsumptionDays = 7,
                            Remark = "test", IsRFQDone = false,
                            StatusId = statusId.Value,
                            IsActive = Status.Active, IsDeleted = IsDelete.NotDeleted
                        }
                    }
                    : new List<IndentDetail>()
            };
            await ctx.IndentHeader.AddAsync(h);
            await ctx.SaveChangesAsync();
            return h.Id;
        }

        private async Task ClearAsync() =>
            await _fixture.ClearAllTablesAsync();

        // --- GeneratePurchaseIndentNumberAsync ---

        [Fact]
        public async Task GeneratePurchaseIndentNumberAsync_Should_Return_PI_With_Sequence()
        {
            await ClearAsync();

            var result = await CreateRepo().GeneratePurchaseIndentNumberAsync(1);

            result.Should().StartWith("PI/U1/");
            result.Should().EndWith("0001");
        }

        [Fact]
        public async Task GeneratePurchaseIndentNumberAsync_Should_Throw_When_Unit_NotFound()
        {
            // CreateRepo always sets up GetByIdAsync to return a stub unit. Override here to return null
            // and verify the repo throws ExceptionRules.
            var unitLookup = new Mock<IUnitLookup>(MockBehavior.Loose);
            unitLookup.Setup(l => l.GetByIdAsync(99999, It.IsAny<CancellationToken>()))
                .ReturnsAsync((UnitLookupDto?)null);

            Func<Task> act = async () => await CreateRepo(unitLookup: unitLookup).GeneratePurchaseIndentNumberAsync(99999);

            await act.Should().ThrowAsync<ExceptionRules>().WithMessage("*Invalid Unit*");
        }

        // --- GetByIdAsync ---

        [Fact]
        public async Task GetByIdAsync_Should_Return_Matching_With_Details()
        {
            await ClearAsync();
            var id = await SeedHeaderAsync("PI_GBI1");

            var result = await CreateRepo().GetByIdAsync(id);

            result.Should().NotBeNull();
            result!.IndentNumber.Should().Be("PI_GBI1");
            result.IndentDetails.Should().ContainSingle();
        }

        [Fact]
        public async Task GetByIdAsync_Should_Return_Null_When_NotFound()
        {
            var result = await CreateRepo().GetByIdAsync(9999999);
            result.Should().BeNull();
        }

        [Fact]
        public async Task GetByIdAsync_Should_Return_Null_When_SoftDeleted()
        {
            await ClearAsync();
            var id = await SeedHeaderAsync("PI_GSD", deleted: IsDelete.Deleted);

            var result = await CreateRepo().GetByIdAsync(id);

            result.Should().BeNull();
        }

        // --- NotFoundAsync ---

        [Fact]
        public async Task NotFoundAsync_Should_Return_True_For_Missing()
        {
            var result = await CreateRepo().NotFoundAsync(9999999);
            result.Should().BeTrue();
        }

        [Fact]
        public async Task NotFoundAsync_Should_Return_False_For_Existing()
        {
            await ClearAsync();
            var id = await SeedHeaderAsync("PI_NF1");

            var result = await CreateRepo().NotFoundAsync(id);

            result.Should().BeFalse();
        }

        // --- GetPendingPurchaseIndentAsync ---

        [Fact]
        public async Task GetPendingPurchaseIndentAsync_Should_Return_Pending_Status_Rows()
        {
            await ClearAsync();
            var pendingId = await EnsureMiscIdAsync("Pending", "PIQ_ST");
            await SeedHeaderAsync("PI_PEND1", statusId: pendingId);

            var (rows, total) = await CreateRepo().GetPendingPurchaseIndentAsync(1, 10, null);

            rows.Should().NotBeEmpty();
            total.Should().BeGreaterThan(0);
        }

        [Fact]
        public async Task GetPendingPurchaseIndentAsync_Should_Filter_By_SearchTerm()
        {
            await ClearAsync();
            var pendingId = await EnsureMiscIdAsync("Pending", "PIQ_ST");
            await SeedHeaderAsync("PI_PEND_UNIQ", statusId: pendingId);
            await SeedHeaderAsync("PI_OTHER", statusId: pendingId);

            var (rows, _) = await CreateRepo().GetPendingPurchaseIndentAsync(1, 10, "PI_PEND_UNIQ");

            rows.Should().HaveCount(1);
            rows[0].IndentNumber.Should().Be("PI_PEND_UNIQ");
        }

        [Fact]
        public async Task GetPendingPurchaseIndentAsync_Should_Exclude_Non_Pending()
        {
            await ClearAsync();
            var openId = await EnsureMiscIdAsync("Open", "PIQ_ST"); // non-Pending
            await SeedHeaderAsync("PI_NOTPEND1", statusId: openId);

            var (rows, total) = await CreateRepo().GetPendingPurchaseIndentAsync(1, 10, null);

            rows.Should().BeEmpty();
            total.Should().Be(0);
        }

        // --- SoftDeleteValidationAsync ---

        [Fact]
        public async Task SoftDeleteValidationAsync_Should_Return_True_When_Has_Active_Details()
        {
            await ClearAsync();
            var id = await SeedHeaderAsync("PI_SDV1", withDetail: true);

            var result = await CreateRepo().SoftDeleteValidationAsync(id);

            result.Should().BeTrue();
        }

        [Fact]
        public async Task SoftDeleteValidationAsync_Should_Return_False_When_No_Details()
        {
            await ClearAsync();
            var id = await SeedHeaderAsync("PI_SDV2", withDetail: false);

            var result = await CreateRepo().SoftDeleteValidationAsync(id);

            result.Should().BeFalse();
        }
    }
}
