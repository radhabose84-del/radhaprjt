using Contracts.Dtos.Lookups.Inventory;
using Contracts.Dtos.Lookups.Party;
using Contracts.Dtos.Lookups.Users;
using Contracts.Dtos.Lookups.Warehouse;
using Contracts.Interfaces.Lookups.Inventory;
using Contracts.Interfaces.Lookups.Party;
using Contracts.Interfaces.Lookups.Users;
using Contracts.Interfaces.Lookups.Warehouse;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using SalesManagement.Infrastructure.Data;
using SalesManagement.Infrastructure.Repositories.ComplaintResolution;
using SalesManagement.IntegrationTests.Common;
using static SalesManagement.Domain.Common.BaseEntity;

namespace SalesManagement.IntegrationTests.Repositories.ComplaintResolution
{
    /// <summary>
    /// Integration tests for ComplaintResolutionQueryRepository (Dapper).
    /// Tests NotFoundAsync, ComplaintExistsAsync, ResolutionExistsForComplaintAsync, MiscMasterExistsAsync
    /// plus simplified GetById tests.
    /// </summary>
    [Collection("DatabaseCollection")]
    public sealed class ComplaintResolutionQueryRepositoryTests
    {
        private readonly DbFixture _fixture;

        public ComplaintResolutionQueryRepositoryTests(DbFixture fixture) => _fixture = fixture;

        // ---------------------------------------------------------------------------
        // Factory helpers
        // ---------------------------------------------------------------------------

        private ComplaintResolutionQueryRepository CreateRepo()
        {
            return new ComplaintResolutionQueryRepository(
                new SqlConnection(_fixture.ConnectionString),
                BuildDefaultUserLookup().Object,
                BuildDefaultPartyLookup().Object,
                BuildDefaultItemLookup().Object,
                BuildDefaultWarehouseLookup().Object,
                BuildDefaultBinLookup().Object);
        }

        private static Mock<IUserLookup> BuildDefaultUserLookup()
        {
            var mock = new Mock<IUserLookup>(MockBehavior.Loose);
            mock.Setup(u => u.GetByIdsAsync(It.IsAny<IEnumerable<int>>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync((IEnumerable<int> ids, CancellationToken _) =>
                    (IReadOnlyList<UserLookupDto>)ids.Select(id =>
                        new UserLookupDto { UserId = id, FirstName = "User", LastName = id.ToString() }).ToList());
            return mock;
        }

        private static Mock<IPartyLookup> BuildDefaultPartyLookup()
        {
            var mock = new Mock<IPartyLookup>(MockBehavior.Loose);
            mock.Setup(p => p.GetByIdsAsync(It.IsAny<IEnumerable<int>>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync((IEnumerable<int> ids, CancellationToken _) =>
                    (IReadOnlyList<PartyLookupDto>)ids.Select(id =>
                        new PartyLookupDto { Id = id, PartyName = "Party " + id }).ToList());
            mock.Setup(p => p.GetByIdAsync(It.IsAny<int>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync((int id, CancellationToken _) =>
                    new PartyLookupDto { Id = id, PartyName = "Party " + id });
            return mock;
        }

        private static Mock<IItemLookup> BuildDefaultItemLookup()
        {
            var mock = new Mock<IItemLookup>(MockBehavior.Loose);
            mock.Setup(i => i.GetByIdsAsync(It.IsAny<IEnumerable<int>>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync((IEnumerable<int> ids, CancellationToken _) =>
                    (IReadOnlyList<ItemLookupDto>)ids.Select(id =>
                        new ItemLookupDto { Id = id, ItemName = "Item " + id }).ToList());
            return mock;
        }

        private static Mock<IWarehouseLookup> BuildDefaultWarehouseLookup()
        {
            var mock = new Mock<IWarehouseLookup>(MockBehavior.Loose);
            mock.Setup(w => w.GetByIdsAsync(It.IsAny<IEnumerable<int>>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync((IReadOnlyList<WarehouseLookupDto>)new List<WarehouseLookupDto>());
            mock.Setup(w => w.GetAllAsync(It.IsAny<CancellationToken>()))
                .ReturnsAsync(new List<WarehouseLookupDto>());
            return mock;
        }

        private static Mock<IBinLookup> BuildDefaultBinLookup()
        {
            var mock = new Mock<IBinLookup>(MockBehavior.Loose);
            mock.Setup(b => b.GetByIdsAsync(It.IsAny<IEnumerable<int>>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync((IReadOnlyList<BinLookupDto>)new List<BinLookupDto>());
            mock.Setup(b => b.GetAllAsync(It.IsAny<CancellationToken>()))
                .ReturnsAsync(new List<BinLookupDto>());
            return mock;
        }

        // ---------------------------------------------------------------------------
        // Seeding helpers
        // ---------------------------------------------------------------------------

        private async Task<int> EnsureMiscTypeAsync(ApplicationDbContext ctx, string code = "CRQ_MT")
        {
            var mt = await ctx.MiscTypeMaster.FirstOrDefaultAsync(x => x.MiscTypeCode == code);
            if (mt == null)
            {
                mt = new SalesManagement.Domain.Entities.MiscTypeMaster
                {
                    MiscTypeCode = code,
                    Description = "Resolution Query Test",
                    IsActive = Status.Active,
                    IsDeleted = IsDelete.NotDeleted
                };
                await ctx.MiscTypeMaster.AddAsync(mt);
                await ctx.SaveChangesAsync();
            }
            return mt.Id;
        }

        private async Task<int> EnsureMiscAsync(ApplicationDbContext ctx, int miscTypeId, string code)
        {
            var m = await ctx.MiscMaster.FirstOrDefaultAsync(x => x.MiscTypeId == miscTypeId && x.Code == code);
            if (m == null)
            {
                m = new SalesManagement.Domain.Entities.MiscMaster
                {
                    MiscTypeId = miscTypeId,
                    Code = code,
                    Description = code,
                    IsActive = Status.Active,
                    IsDeleted = IsDelete.NotDeleted
                };
                await ctx.MiscMaster.AddAsync(m);
                await ctx.SaveChangesAsync();
            }
            return m.Id;
        }

        private async Task<(int complaintId, int resTypeId, int miscId)> SeedPrerequisitesAsync()
        {
            await using var ctx = _fixture.CreateFreshDbContext();
            var mtId = await EnsureMiscTypeAsync(ctx);
            var statusId = await EnsureMiscAsync(ctx, mtId, "CRQ_STATUS");
            var resTypeId = await EnsureMiscAsync(ctx, mtId, "CRQ_RESTYPE");

            var complaint = new SalesManagement.Domain.Entities.ComplaintHeader
            {
                ComplaintNumber = "CRQ_C" + Guid.NewGuid().ToString("N")[..6],
                ComplaintDate = DateOnly.FromDateTime(DateTime.UtcNow.Date),
                CustomerId = 100,
                StatusId = statusId,
                IsActive = Status.Active,
                IsDeleted = IsDelete.NotDeleted
            };
            await ctx.ComplaintHeader.AddAsync(complaint);
            await ctx.SaveChangesAsync();

            return (complaint.Id, resTypeId, statusId);
        }

        private async Task<int> SeedResolutionAsync(int complaintId, int resTypeId)
        {
            await using var ctx = _fixture.CreateFreshDbContext();
            var entity = new SalesManagement.Domain.Entities.ComplaintResolution
            {
                ComplaintHeaderId = complaintId,
                ResolutionTypeId = resTypeId,
                ResolutionSummary = "Test resolution",
                IsActive = Status.Active,
                IsDeleted = IsDelete.NotDeleted
            };
            await ctx.ComplaintResolution.AddAsync(entity);
            await ctx.SaveChangesAsync();
            return entity.Id;
        }

        private async Task ClearAsync() => await _fixture.ClearTablesAsync(
            "Sales.ComplaintDepartmentFeedback",
            "Sales.ComplaintFeedbackAttachment",
            "Sales.ComplaintQCReviewAssignment",
            "Sales.ComplaintResolution",
            "Sales.ComplaintQCReview",
            "Sales.ComplaintDetailNature",
            "Sales.ComplaintDetail",
            "Sales.ComplaintAttachment",
            "Sales.ComplaintHeader");

        // ---------------------------------------------------------------------------
        // NotFoundAsync
        // ---------------------------------------------------------------------------

        [Fact]
        public async Task NotFoundAsync_Should_Return_True_When_NotExists()
        {
            var result = await CreateRepo().NotFoundAsync(99999);
            result.Should().BeTrue();
        }

        [Fact]
        public async Task NotFoundAsync_Should_Return_False_When_Exists()
        {
            await ClearAsync();
            var (complaintId, resTypeId, _) = await SeedPrerequisitesAsync();
            var resId = await SeedResolutionAsync(complaintId, resTypeId);

            var result = await CreateRepo().NotFoundAsync(resId);

            result.Should().BeFalse();
        }

        // ---------------------------------------------------------------------------
        // ComplaintExistsAsync
        // ---------------------------------------------------------------------------

        [Fact]
        public async Task ComplaintExistsAsync_Should_Return_True_When_Exists()
        {
            await ClearAsync();
            var (complaintId, _, _) = await SeedPrerequisitesAsync();

            var result = await CreateRepo().ComplaintExistsAsync(complaintId);

            result.Should().BeTrue();
        }

        [Fact]
        public async Task ComplaintExistsAsync_Should_Return_False_When_NotExists()
        {
            var result = await CreateRepo().ComplaintExistsAsync(99999);
            result.Should().BeFalse();
        }

        // ---------------------------------------------------------------------------
        // ResolutionExistsForComplaintAsync
        // ---------------------------------------------------------------------------

        [Fact]
        public async Task ResolutionExistsForComplaintAsync_Should_Return_True_When_Exists()
        {
            await ClearAsync();
            var (complaintId, resTypeId, _) = await SeedPrerequisitesAsync();
            await SeedResolutionAsync(complaintId, resTypeId);

            var result = await CreateRepo().ResolutionExistsForComplaintAsync(complaintId);

            result.Should().BeTrue();
        }

        [Fact]
        public async Task ResolutionExistsForComplaintAsync_Should_Return_False_When_ExcludingSelf()
        {
            await ClearAsync();
            var (complaintId, resTypeId, _) = await SeedPrerequisitesAsync();
            var resId = await SeedResolutionAsync(complaintId, resTypeId);

            var result = await CreateRepo().ResolutionExistsForComplaintAsync(complaintId, excludeId: resId);

            result.Should().BeFalse();
        }

        [Fact]
        public async Task ResolutionExistsForComplaintAsync_Should_Return_False_When_No_Resolution()
        {
            await ClearAsync();
            var (complaintId, _, _) = await SeedPrerequisitesAsync();

            var result = await CreateRepo().ResolutionExistsForComplaintAsync(complaintId);

            result.Should().BeFalse();
        }

        // ---------------------------------------------------------------------------
        // MiscMasterExistsAsync
        // ---------------------------------------------------------------------------

        [Fact]
        public async Task MiscMasterExistsAsync_Should_Return_True_When_Active()
        {
            await ClearAsync();
            var (_, resTypeId, _) = await SeedPrerequisitesAsync();

            var result = await CreateRepo().MiscMasterExistsAsync(resTypeId);

            result.Should().BeTrue();
        }

        [Fact]
        public async Task MiscMasterExistsAsync_Should_Return_False_When_NotExists()
        {
            var result = await CreateRepo().MiscMasterExistsAsync(99999);
            result.Should().BeFalse();
        }

        // ---------------------------------------------------------------------------
        // GetByIdAsync (simplified — validates basic flow)
        // ---------------------------------------------------------------------------

        [Fact]
        public async Task GetByIdAsync_Should_Return_Null_When_NotFound()
        {
            var result = await CreateRepo().GetByIdAsync(99999);
            result.Should().BeNull();
        }

        [Fact]
        public async Task GetByIdAsync_Should_Return_Resolution_When_Exists()
        {
            await ClearAsync();
            var (complaintId, resTypeId, _) = await SeedPrerequisitesAsync();
            var resId = await SeedResolutionAsync(complaintId, resTypeId);

            var result = await CreateRepo().GetByIdAsync(resId);

            result.Should().NotBeNull();
            result!.Id.Should().Be(resId);
            result.ComplaintHeaderId.Should().Be(complaintId);
            result.ResolutionSummary.Should().Be("Test resolution");
        }

        // ---------------------------------------------------------------------------
        // GetByComplaintHeaderIdAsync
        // ---------------------------------------------------------------------------

        [Fact]
        public async Task GetByComplaintHeaderIdAsync_Should_Return_Null_When_NoResolution()
        {
            await ClearAsync();
            var (complaintId, _, _) = await SeedPrerequisitesAsync();

            var result = await CreateRepo().GetByComplaintHeaderIdAsync(complaintId);

            result.Should().BeNull();
        }

        [Fact]
        public async Task GetByComplaintHeaderIdAsync_Should_Return_Resolution()
        {
            await ClearAsync();
            var (complaintId, resTypeId, _) = await SeedPrerequisitesAsync();
            var resId = await SeedResolutionAsync(complaintId, resTypeId);

            var result = await CreateRepo().GetByComplaintHeaderIdAsync(complaintId);

            result.Should().NotBeNull();
            result!.Id.Should().Be(resId);
        }
    }
}
