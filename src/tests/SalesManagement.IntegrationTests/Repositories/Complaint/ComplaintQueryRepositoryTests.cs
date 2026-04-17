using Contracts.Dtos.Lookups.Inventory;
using Contracts.Dtos.Lookups.Party;
using Contracts.Dtos.Lookups.Production;
using Contracts.Dtos.Lookups.Users;
using Contracts.Interfaces;
using Contracts.Interfaces.Lookups.Inventory;
using Contracts.Interfaces.Lookups.Party;
using Contracts.Interfaces.Lookups.Production;
using Contracts.Interfaces.Lookups.Users;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using SalesManagement.Infrastructure.Data;
using SalesManagement.Infrastructure.Repositories.Complaint;
using SalesManagement.IntegrationTests.Common;
using static SalesManagement.Domain.Common.BaseEntity;

namespace SalesManagement.IntegrationTests.Repositories.Complaint
{
    [Collection("DatabaseCollection")]
    public sealed class ComplaintQueryRepositoryTests
    {
        private readonly DbFixture _fixture;

        public ComplaintQueryRepositoryTests(DbFixture fixture) => _fixture = fixture;

        // ---------------------------------------------------------------------------
        // Factory helpers
        // ---------------------------------------------------------------------------

        private ComplaintQueryRepository CreateRepo(
            Mock<IPartyLookup>? partyLookup = null,
            Mock<IItemLookup>? itemLookup = null,
            Mock<ILotMasterLookup>? lotLookup = null,
            Mock<IUnitLookup>? unitLookup = null,
            Mock<IUOMLookup>? uomLookup = null,
            Mock<IIPAddressService>? ipService = null,
            Mock<IDataAccessFilter>? dataAccessFilter = null)
        {
            partyLookup ??= BuildDefaultPartyLookup();
            itemLookup ??= BuildDefaultItemLookup();
            lotLookup ??= BuildDefaultLotLookup();
            unitLookup ??= BuildDefaultUnitLookup();
            uomLookup ??= BuildDefaultUomLookup();
            ipService ??= BuildDefaultIpService();
            dataAccessFilter ??= BuildUnrestrictedDataAccessFilter();

            return new ComplaintQueryRepository(
                new SqlConnection(_fixture.ConnectionString),
                partyLookup.Object,
                itemLookup.Object,
                lotLookup.Object,
                unitLookup.Object,
                uomLookup.Object,
                ipService.Object,
                dataAccessFilter.Object);
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
                        new ItemLookupDto { Id = id, ItemCode = "I" + id, ItemName = "Item " + id }).ToList());
            return mock;
        }

        private static Mock<ILotMasterLookup> BuildDefaultLotLookup()
        {
            var mock = new Mock<ILotMasterLookup>(MockBehavior.Loose);
            mock.Setup(l => l.GetByIdsAsync(It.IsAny<IEnumerable<int>>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync((IReadOnlyList<LotMasterLookupDto>)new List<LotMasterLookupDto>());
            return mock;
        }

        private static Mock<IUnitLookup> BuildDefaultUnitLookup()
        {
            var mock = new Mock<IUnitLookup>(MockBehavior.Loose);
            mock.Setup(u => u.GetByIdsAsync(It.IsAny<IEnumerable<int>>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync((IReadOnlyList<UnitLookupDto>)new List<UnitLookupDto>());
            return mock;
        }

        private static Mock<IUOMLookup> BuildDefaultUomLookup()
        {
            var mock = new Mock<IUOMLookup>(MockBehavior.Loose);
            return mock;
        }

        private static Mock<IIPAddressService> BuildDefaultIpService()
        {
            var mock = new Mock<IIPAddressService>(MockBehavior.Loose);
            mock.Setup(x => x.GetUserId()).Returns(1);
            mock.Setup(x => x.GetUserName()).Returns("test-user");
            mock.Setup(x => x.GetCompanyId()).Returns(1);
            mock.Setup(x => x.GetUnitId()).Returns(1);
            mock.Setup(x => x.GetGroupCode()).Returns("SUPER_ADMIN");
            return mock;
        }

        private static Mock<IDataAccessFilter> BuildUnrestrictedDataAccessFilter()
        {
            var mock = new Mock<IDataAccessFilter>(MockBehavior.Loose);
            mock.Setup(d => d.GetContextAsync(It.IsAny<CancellationToken>()))
                .ReturnsAsync(DataAccessContext.Unrestricted);
            return mock;
        }

        // ---------------------------------------------------------------------------
        // Seeding helpers
        // ---------------------------------------------------------------------------

        private async Task<int> EnsureMiscTypeAsync(ApplicationDbContext ctx, string code = "CQQ_MT")
        {
            var mt = await ctx.MiscTypeMaster.FirstOrDefaultAsync(x => x.MiscTypeCode == code);
            if (mt == null)
            {
                mt = new SalesManagement.Domain.Entities.MiscTypeMaster
                {
                    MiscTypeCode = code,
                    Description = "Complaint Test Type",
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

        /// <summary>
        /// Seeds a ComplaintHeader row and returns (statusId, complaintId).
        /// </summary>
        private async Task<(int statusId, int complaintId)> SeedComplaintAsync(
            string complaintNumber = "CQQ_C001",
            int customerId = 100,
            IsDelete deleted = IsDelete.NotDeleted,
            Status active = Status.Active,
            string? remarks = "test-remarks")
        {
            await using var ctx = _fixture.CreateFreshDbContext();
            var mtId = await EnsureMiscTypeAsync(ctx);
            var statusId = await EnsureMiscAsync(ctx, mtId, "CQQ_STATUS");

            var entity = new SalesManagement.Domain.Entities.ComplaintHeader
            {
                ComplaintNumber = complaintNumber,
                ComplaintDate = DateOnly.FromDateTime(DateTime.UtcNow.Date),
                CustomerId = customerId,
                StatusId = statusId,
                Remarks = remarks,
                IsActive = active,
                IsDeleted = deleted
            };
            await ctx.ComplaintHeader.AddAsync(entity);
            await ctx.SaveChangesAsync();
            return (statusId, entity.Id);
        }

        private async Task ClearAsync() =>
            await _fixture.ClearAllTablesAsync();

        // ---------------------------------------------------------------------------
        // GetAllAsync
        // ---------------------------------------------------------------------------

        [Fact]
        public async Task GetAllAsync_Should_Return_Seeded_Record()
        {
            await ClearAsync();
            await SeedComplaintAsync("CQQ_GA1");

            var (rows, total) = await CreateRepo().GetAllAsync(1, 10, null, null);

            rows.Should().HaveCount(1);
            total.Should().Be(1);
        }

        [Fact]
        public async Task GetAllAsync_Should_Exclude_SoftDeleted()
        {
            await ClearAsync();
            await SeedComplaintAsync("CQQ_DEL", deleted: IsDelete.Deleted);

            var (_, total) = await CreateRepo().GetAllAsync(1, 10, null, null);

            total.Should().Be(0);
        }

        [Fact]
        public async Task GetAllAsync_Should_Filter_By_SearchTerm()
        {
            await ClearAsync();
            await SeedComplaintAsync("CQQ_ALPHA");
            await SeedComplaintAsync("CQQ_BETA");

            var (rows, _) = await CreateRepo().GetAllAsync(1, 10, "CQQ_ALPHA", null);

            rows.Should().HaveCount(1);
            rows[0].ComplaintNumber.Should().Be("CQQ_ALPHA");
        }

        [Fact]
        public async Task GetAllAsync_Should_Populate_CustomerName_Via_Lookup()
        {
            await ClearAsync();
            await SeedComplaintAsync("CQQ_CUST", customerId: 42);

            var partyMock = new Mock<IPartyLookup>(MockBehavior.Loose);
            partyMock.Setup(p => p.GetByIdsAsync(It.IsAny<IEnumerable<int>>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync((IEnumerable<int> ids, CancellationToken _) =>
                    (IReadOnlyList<PartyLookupDto>)ids.Select(id =>
                        new PartyLookupDto { Id = id, PartyName = "Acme Corp" }).ToList());

            var (rows, _) = await CreateRepo(partyLookup: partyMock).GetAllAsync(1, 10, null, null);

            rows.Should().HaveCount(1);
            rows[0].CustomerName.Should().Be("Acme Corp");
        }

        [Fact]
        public async Task GetAllAsync_Should_Return_Pagination()
        {
            await ClearAsync();
            await SeedComplaintAsync("CQQ_PG1");
            await SeedComplaintAsync("CQQ_PG2");
            await SeedComplaintAsync("CQQ_PG3");

            var (rows, total) = await CreateRepo().GetAllAsync(1, 2, null, null);

            rows.Should().HaveCount(2);
            total.Should().Be(3);
        }

        // ---------------------------------------------------------------------------
        // GetByIdAsync
        // ---------------------------------------------------------------------------

        [Fact]
        public async Task GetByIdAsync_Should_Return_Correct_Dto()
        {
            await ClearAsync();
            var (_, id) = await SeedComplaintAsync("CQQ_GBI1", customerId: 55);

            var dto = await CreateRepo().GetByIdAsync(id);

            dto.Should().NotBeNull();
            dto!.ComplaintNumber.Should().Be("CQQ_GBI1");
            dto.CustomerId.Should().Be(55);
        }

        [Fact]
        public async Task GetByIdAsync_Should_Return_Null_When_SoftDeleted()
        {
            await ClearAsync();
            var (_, id) = await SeedComplaintAsync("CQQ_GBI_DEL", deleted: IsDelete.Deleted);

            var dto = await CreateRepo().GetByIdAsync(id);

            dto.Should().BeNull();
        }

        [Fact]
        public async Task GetByIdAsync_Should_Return_Null_For_NonExistent_Id()
        {
            var dto = await CreateRepo().GetByIdAsync(9999999);

            dto.Should().BeNull();
        }

        // ---------------------------------------------------------------------------
        // NotFoundAsync
        // ---------------------------------------------------------------------------

        [Fact]
        public async Task NotFoundAsync_Should_Return_True_For_Missing()
        {
            var result = await CreateRepo().NotFoundAsync(9999999);
            result.Should().BeTrue();
        }

        [Fact]
        public async Task NotFoundAsync_Should_Return_False_When_Exists()
        {
            await ClearAsync();
            var (_, id) = await SeedComplaintAsync("CQQ_NF1");

            var result = await CreateRepo().NotFoundAsync(id);

            result.Should().BeFalse();
        }

        [Fact]
        public async Task NotFoundAsync_Should_Return_True_When_SoftDeleted()
        {
            await ClearAsync();
            var (_, id) = await SeedComplaintAsync("CQQ_NF_DEL", deleted: IsDelete.Deleted);

            var result = await CreateRepo().NotFoundAsync(id);

            result.Should().BeTrue();
        }

        // ---------------------------------------------------------------------------
        // CustomerExistsAsync
        // ---------------------------------------------------------------------------

        [Fact]
        public async Task CustomerExistsAsync_Should_Return_True_When_Lookup_Returns_Party()
        {
            // CustomerExistsAsync delegates entirely to IPartyLookup.GetByIdAsync
            var result = await CreateRepo().CustomerExistsAsync(42);

            result.Should().BeTrue();
        }

        [Fact]
        public async Task CustomerExistsAsync_Should_Return_False_When_Lookup_Returns_Null()
        {
            var partyMock = new Mock<IPartyLookup>(MockBehavior.Loose);
            partyMock.Setup(p => p.GetByIdAsync(It.IsAny<int>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync((PartyLookupDto?)null);

            var result = await CreateRepo(partyLookup: partyMock).CustomerExistsAsync(999);

            result.Should().BeFalse();
        }

        // ---------------------------------------------------------------------------
        // AutocompleteAsync
        // ---------------------------------------------------------------------------

        [Fact]
        public async Task AutocompleteAsync_Should_Return_Active_Records()
        {
            await ClearAsync();
            await SeedComplaintAsync("CQQ_AC1", active: Status.Active);

            var results = await CreateRepo().AutocompleteAsync("CQQ_AC", CancellationToken.None);

            results.Should().HaveCount(1);
            results[0].ComplaintNumber.Should().Be("CQQ_AC1");
        }

        [Fact]
        public async Task AutocompleteAsync_Should_Exclude_Inactive()
        {
            await ClearAsync();
            await SeedComplaintAsync("CQQ_INAC", active: Status.Inactive);

            var results = await CreateRepo().AutocompleteAsync("CQQ_INAC", CancellationToken.None);

            results.Should().BeEmpty();
        }

        // ---------------------------------------------------------------------------
        // IsReadyForResolutionAsync
        // ---------------------------------------------------------------------------

        [Fact]
        public async Task IsReadyForResolutionAsync_Should_Return_False_When_No_QCReview()
        {
            await ClearAsync();
            var (_, id) = await SeedComplaintAsync("CQQ_RES1");

            var result = await CreateRepo().IsReadyForResolutionAsync(id);

            result.Should().BeFalse();
        }
    }
}
