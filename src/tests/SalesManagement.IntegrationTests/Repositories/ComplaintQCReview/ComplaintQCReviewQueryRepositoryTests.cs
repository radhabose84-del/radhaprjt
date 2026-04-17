using Contracts.Dtos.Lookups.Inventory;
using Contracts.Dtos.Lookups.Party;
using Contracts.Dtos.Lookups.Users;
using Contracts.Interfaces.Lookups.Inventory;
using Contracts.Interfaces.Lookups.Party;
using Contracts.Interfaces.Lookups.Users;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using SalesManagement.Infrastructure.Data;
using SalesManagement.Infrastructure.Repositories.ComplaintQCReview;
using SalesManagement.IntegrationTests.Common;
using static SalesManagement.Domain.Common.BaseEntity;

namespace SalesManagement.IntegrationTests.Repositories.ComplaintQCReview
{
    /// <summary>
    /// Integration tests for ComplaintQCReviewQueryRepository (Dapper).
    /// Tests NotFoundAsync, ComplaintExistsAsync, ReviewAlreadyExistsAsync, and MiscMasterExistsAsync
    /// which only need same-module seeded data.
    /// GetAll/GetById are complex with many cross-module lookups so simplified tests are provided.
    /// </summary>
    [Collection("DatabaseCollection")]
    public sealed class ComplaintQCReviewQueryRepositoryTests
    {
        private readonly DbFixture _fixture;

        public ComplaintQCReviewQueryRepositoryTests(DbFixture fixture) => _fixture = fixture;

        // ---------------------------------------------------------------------------
        // Factory helpers
        // ---------------------------------------------------------------------------

        private ComplaintQCReviewQueryRepository CreateRepo(
            Mock<IUserLookup> userLookup = null,
            Mock<IPartyLookup> partyLookup = null,
            Mock<IItemLookup> itemLookup = null)
        {
            userLookup ??= BuildDefaultUserLookup();
            partyLookup ??= BuildDefaultPartyLookup();
            itemLookup ??= BuildDefaultItemLookup();

            return new ComplaintQCReviewQueryRepository(
                new SqlConnection(_fixture.ConnectionString),
                userLookup.Object,
                partyLookup.Object,
                itemLookup.Object);
        }

        private static Mock<IUserLookup> BuildDefaultUserLookup()
        {
            var mock = new Mock<IUserLookup>(MockBehavior.Loose);
            mock.Setup(u => u.GetByIdsAsync(It.IsAny<IEnumerable<int>>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync((IEnumerable<int> ids, CancellationToken _) =>
                    (IReadOnlyList<UserLookupDto>)ids.Select(id =>
                        new UserLookupDto { UserId = id, FirstName = "User", LastName = id.ToString() }).ToList());
            mock.Setup(u => u.GetByIdAsync(It.IsAny<int>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync((int id, CancellationToken _) =>
                    new UserLookupDto { UserId = id, FirstName = "User", LastName = id.ToString() });
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

        // ---------------------------------------------------------------------------
        // Seeding helpers
        // ---------------------------------------------------------------------------

        private async Task<int> EnsureMiscTypeAsync(ApplicationDbContext ctx, string code = "QCRQ_MT")
        {
            var mt = await ctx.MiscTypeMaster.FirstOrDefaultAsync(x => x.MiscTypeCode == code);
            if (mt == null)
            {
                mt = new SalesManagement.Domain.Entities.MiscTypeMaster
                {
                    MiscTypeCode = code,
                    Description = "QCReview Query Test",
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

        private async Task<(int complaintId, int pvId, int miscId)> SeedPrerequisitesAsync()
        {
            await using var ctx = _fixture.CreateFreshDbContext();
            var mtId = await EnsureMiscTypeAsync(ctx);
            var pvId = await EnsureMiscAsync(ctx, mtId, "QCRQ_PV");
            var statusId = await EnsureMiscAsync(ctx, mtId, "QCRQ_STATUS");

            var complaint = new SalesManagement.Domain.Entities.ComplaintHeader
            {
                ComplaintNumber = "QCRQ_C" + Guid.NewGuid().ToString("N")[..6],
                ComplaintDate = DateOnly.FromDateTime(DateTime.UtcNow.Date),
                CustomerId = 100,
                StatusId = statusId,
                IsActive = Status.Active,
                IsDeleted = IsDelete.NotDeleted
            };
            await ctx.ComplaintHeader.AddAsync(complaint);
            await ctx.SaveChangesAsync();

            return (complaint.Id, pvId, statusId);
        }

        private async Task<int> SeedQCReviewAsync(int complaintId, int pvId)
        {
            await using var ctx = _fixture.CreateFreshDbContext();
            var entity = new SalesManagement.Domain.Entities.ComplaintQCReview
            {
                ComplaintHeaderId = complaintId,
                PhysicalVerificationId = pvId,
                LabVerificationRequired = false,
                IsActive = Status.Active,
                IsDeleted = IsDelete.NotDeleted
            };
            await ctx.ComplaintQCReview.AddAsync(entity);
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
            var (complaintId, pvId, _) = await SeedPrerequisitesAsync();
            var reviewId = await SeedQCReviewAsync(complaintId, pvId);

            var result = await CreateRepo().NotFoundAsync(reviewId);

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
        // ReviewAlreadyExistsAsync
        // ---------------------------------------------------------------------------

        [Fact]
        public async Task ReviewAlreadyExistsAsync_Should_Return_True_When_Exists()
        {
            await ClearAsync();
            var (complaintId, pvId, _) = await SeedPrerequisitesAsync();
            await SeedQCReviewAsync(complaintId, pvId);

            var result = await CreateRepo().ReviewAlreadyExistsAsync(complaintId);

            result.Should().BeTrue();
        }

        [Fact]
        public async Task ReviewAlreadyExistsAsync_Should_Return_False_When_ExcludingSelf()
        {
            await ClearAsync();
            var (complaintId, pvId, _) = await SeedPrerequisitesAsync();
            var reviewId = await SeedQCReviewAsync(complaintId, pvId);

            var result = await CreateRepo().ReviewAlreadyExistsAsync(complaintId, excludeId: reviewId);

            result.Should().BeFalse();
        }

        [Fact]
        public async Task ReviewAlreadyExistsAsync_Should_Return_False_When_NoReview()
        {
            await ClearAsync();
            var (complaintId, _, _) = await SeedPrerequisitesAsync();

            var result = await CreateRepo().ReviewAlreadyExistsAsync(complaintId);

            result.Should().BeFalse();
        }

        // ---------------------------------------------------------------------------
        // MiscMasterExistsAsync
        // ---------------------------------------------------------------------------

        [Fact]
        public async Task MiscMasterExistsAsync_Should_Return_True_When_ActiveMiscExists()
        {
            await ClearAsync();
            var (_, pvId, _) = await SeedPrerequisitesAsync();

            var result = await CreateRepo().MiscMasterExistsAsync(pvId);

            result.Should().BeTrue();
        }

        [Fact]
        public async Task MiscMasterExistsAsync_Should_Return_False_When_NotExists()
        {
            var result = await CreateRepo().MiscMasterExistsAsync(99999);
            result.Should().BeFalse();
        }

        // ---------------------------------------------------------------------------
        // UserExistsAsync (delegates to IUserLookup)
        // ---------------------------------------------------------------------------

        [Fact]
        public async Task UserExistsAsync_Should_Return_True_When_Lookup_Finds_User()
        {
            var result = await CreateRepo().UserExistsAsync(1);
            result.Should().BeTrue();
        }

        [Fact]
        public async Task UserExistsAsync_Should_Return_False_When_Lookup_Returns_Null()
        {
            var userMock = new Mock<IUserLookup>(MockBehavior.Loose);
            userMock.Setup(u => u.GetByIdAsync(It.IsAny<int>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync((UserLookupDto)null);

            var result = await CreateRepo(userLookup: userMock).UserExistsAsync(99999);

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
        public async Task GetByIdAsync_Should_Return_Review_When_Exists()
        {
            await ClearAsync();
            var (complaintId, pvId, _) = await SeedPrerequisitesAsync();
            var reviewId = await SeedQCReviewAsync(complaintId, pvId);

            var result = await CreateRepo().GetByIdAsync(reviewId);

            result.Should().NotBeNull();
            result!.Id.Should().Be(reviewId);
            result.ComplaintHeaderId.Should().Be(complaintId);
        }

        // ---------------------------------------------------------------------------
        // GetByComplaintIdAsync
        // ---------------------------------------------------------------------------

        [Fact]
        public async Task GetByComplaintIdAsync_Should_Return_Null_When_NoReview()
        {
            await ClearAsync();
            var (complaintId, _, _) = await SeedPrerequisitesAsync();

            var result = await CreateRepo().GetByComplaintIdAsync(complaintId);

            result.Should().BeNull();
        }

        [Fact]
        public async Task GetByComplaintIdAsync_Should_Return_Review()
        {
            await ClearAsync();
            var (complaintId, pvId, _) = await SeedPrerequisitesAsync();
            var reviewId = await SeedQCReviewAsync(complaintId, pvId);

            var result = await CreateRepo().GetByComplaintIdAsync(complaintId);

            result.Should().NotBeNull();
            result!.Id.Should().Be(reviewId);
        }
    }
}
