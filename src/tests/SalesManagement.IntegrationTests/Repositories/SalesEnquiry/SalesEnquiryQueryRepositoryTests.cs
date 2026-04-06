using Contracts.Interfaces.Lookups.Inventory;
using Contracts.Interfaces.Lookups.Party;
using Contracts.Interfaces.Lookups.Purchase;
using Dapper;
using Microsoft.Data.SqlClient;
using SalesManagement.Domain.Entities;
using SalesManagement.Infrastructure.Data;
using SalesManagement.Infrastructure.Repositories.SalesEnquiry;
using SalesManagement.IntegrationTests.Common;
using static SalesManagement.Domain.Common.BaseEntity;

namespace SalesManagement.IntegrationTests.Repositories.SalesEnquiry
{
    [Collection("DatabaseCollection")]
    public sealed class SalesEnquiryQueryRepositoryTests
    {
        private readonly DbFixture _fixture;

        public SalesEnquiryQueryRepositoryTests(DbFixture fixture)
        {
            _fixture = fixture;
        }

        private SalesEnquiryQueryRepository CreateQueryRepo(
            Mock<IPartyLookup>? partyLookup = null,
            Mock<IPaymentTermLookup>? paymentTermLookup = null,
            Mock<IItemLookup>? itemLookup = null)
        {
            var mockParty = partyLookup ?? BuildEmptyPartyLookup();
            var mockPt = paymentTermLookup ?? BuildEmptyPaymentTermLookup();
            var mockItem = itemLookup ?? BuildEmptyItemLookup();

            return new SalesEnquiryQueryRepository(
                new SqlConnection(_fixture.ConnectionString),
                mockParty.Object,
                mockPt.Object,
                mockItem.Object);
        }

        private Mock<IPartyLookup> BuildEmptyPartyLookup()
        {
            var mock = new Mock<IPartyLookup>(MockBehavior.Loose);
            mock.Setup(x => x.GetByIdsAsync(It.IsAny<IEnumerable<int>>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new List<Contracts.Dtos.Lookups.Party.PartyLookupDto>());
            return mock;
        }

        private Mock<IPartyLookup> BuildPartyLookup(int partyId = 1, string partyName = "Test Party")
        {
            var mock = new Mock<IPartyLookup>(MockBehavior.Loose);
            var dto = new Contracts.Dtos.Lookups.Party.PartyLookupDto
            {
                Id = partyId, PartyCode = "P001", PartyName = partyName
            };
            mock.Setup(x => x.GetByIdsAsync(It.IsAny<IEnumerable<int>>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new List<Contracts.Dtos.Lookups.Party.PartyLookupDto> { dto });
            return mock;
        }

        private Mock<IPaymentTermLookup> BuildEmptyPaymentTermLookup()
        {
            var mock = new Mock<IPaymentTermLookup>(MockBehavior.Loose);
            mock.Setup(x => x.GetAllPaymentTermAsync())
                .ReturnsAsync(new List<Contracts.Dtos.Lookups.Purchase.PaymentTermLookupDto>());
            return mock;
        }

        private Mock<IItemLookup> BuildEmptyItemLookup()
        {
            var mock = new Mock<IItemLookup>(MockBehavior.Loose);
            mock.Setup(x => x.GetByIdsAsync(It.IsAny<IEnumerable<int>>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new List<Contracts.Dtos.Lookups.Inventory.ItemLookupDto>());
            return mock;
        }

        private SalesEnquiryCommandRepository CreateCommandRepo(ApplicationDbContext ctx)
            => new SalesEnquiryCommandRepository(ctx);

        private SalesEnquiryHeader BuildHeader(
            int partyId = 1,
            string? contactPerson = "Test Contact",
            string? remarks = "Test Enquiry",
            bool isActive = true)
            => new SalesEnquiryHeader
            {
                PartyId = partyId,
                EnquiryDate = DateTimeOffset.UtcNow,
                ContactPerson = contactPerson,
                Remarks = remarks,
                IsActive = isActive ? Status.Active : Status.Inactive,
                IsDeleted = IsDelete.NotDeleted
            };

        private async Task ClearTableAsync()
        {
            await using var conn = new SqlConnection(_fixture.ConnectionString);
            await conn.OpenAsync();
            await conn.ExecuteAsync("DELETE FROM Sales.SalesEnquiryDetail");
            await conn.ExecuteAsync("DELETE FROM Sales.SalesEnquiryHeader");
        }

        private async Task<int> SeedAsync(SalesEnquiryHeader header)
        {
            await using var ctx = _fixture.CreateFreshDbContext();
            return await CreateCommandRepo(ctx).CreateAsync(header);
        }

        // ── GetAllAsync ──────────────────────────────────────────────────────

        [Fact]
        public async Task GetAllAsync_Should_Return_SeededRecord()
        {
            await ClearTableAsync();
            await SeedAsync(BuildHeader(contactPerson: "Test Contact"));

            var (items, total) = await CreateQueryRepo().GetAllAsync(1, 10, null);

            total.Should().Be(1);
            items.Should().HaveCount(1);
        }

        [Fact]
        public async Task GetAllAsync_Should_Exclude_SoftDeleted()
        {
            await ClearTableAsync();
            var id = await SeedAsync(BuildHeader());

            await using var ctx = _fixture.CreateFreshDbContext();
            await CreateCommandRepo(ctx).SoftDeleteAsync(id, CancellationToken.None);

            var (items, total) = await CreateQueryRepo().GetAllAsync(1, 10, null);

            items.Should().BeEmpty();
            total.Should().Be(0);
        }

        [Fact]
        public async Task GetAllAsync_Should_Filter_BySearchTerm_OnRemarks()
        {
            await ClearTableAsync();
            await SeedAsync(BuildHeader(remarks: "Urgent Enquiry Alpha"));
            await SeedAsync(BuildHeader(partyId: 2, remarks: "Normal Enquiry Beta"));

            var (items, total) = await CreateQueryRepo().GetAllAsync(1, 10, "Urgent");

            total.Should().Be(1);
            items[0].Remarks.Should().Be("Urgent Enquiry Alpha");
        }

        [Fact]
        public async Task GetAllAsync_Should_Filter_BySearchTerm_OnContactPerson()
        {
            await ClearTableAsync();
            await SeedAsync(BuildHeader(contactPerson: "John Smith", remarks: "Enquiry 1"));
            await SeedAsync(BuildHeader(partyId: 2, contactPerson: "Jane Doe", remarks: "Enquiry 2"));

            var (items, total) = await CreateQueryRepo().GetAllAsync(1, 10, "John");

            total.Should().Be(1);
            items[0].ContactPerson.Should().Be("John Smith");
        }

        [Fact]
        public async Task GetAllAsync_Should_Return_Empty_WhenNoData()
        {
            await ClearTableAsync();

            var (items, total) = await CreateQueryRepo().GetAllAsync(1, 10, null);

            items.Should().BeEmpty();
            total.Should().Be(0);
        }

        [Fact]
        public async Task GetAllAsync_Should_Populate_PartyName_FromLookup()
        {
            await ClearTableAsync();
            await SeedAsync(BuildHeader(partyId: 1));

            var mockParty = BuildPartyLookup(1, "Party Inc");
            var (items, _) = await CreateQueryRepo(mockParty).GetAllAsync(1, 10, null);

            items[0].PartyName.Should().Be("Party Inc");
        }

        // ── GetByIdAsync ─────────────────────────────────────────────────────

        [Fact]
        public async Task GetByIdAsync_Should_Return_Correct_Dto()
        {
            await ClearTableAsync();
            var id = await SeedAsync(BuildHeader(partyId: 3, contactPerson: "ById Contact", remarks: "ById Remarks"));

            var dto = await CreateQueryRepo().GetByIdAsync(id);

            dto.Should().NotBeNull();
            dto!.Id.Should().Be(id);
            dto.PartyId.Should().Be(3);
            dto.ContactPerson.Should().Be("ById Contact");
            dto.Remarks.Should().Be("ById Remarks");
        }

        [Fact]
        public async Task GetByIdAsync_Should_Return_Null_WhenNotFound()
        {
            await ClearTableAsync();

            var dto = await CreateQueryRepo().GetByIdAsync(99999);

            dto.Should().BeNull();
        }

        [Fact]
        public async Task GetByIdAsync_Should_Return_Null_AfterSoftDelete()
        {
            await ClearTableAsync();
            var id = await SeedAsync(BuildHeader());

            await using var ctx = _fixture.CreateFreshDbContext();
            await CreateCommandRepo(ctx).SoftDeleteAsync(id, CancellationToken.None);

            var dto = await CreateQueryRepo().GetByIdAsync(id);

            dto.Should().BeNull();
        }

        [Fact]
        public async Task GetByIdAsync_Should_Return_EmptyDetails_WhenNoDetailLines()
        {
            await ClearTableAsync();
            var id = await SeedAsync(BuildHeader());

            var dto = await CreateQueryRepo().GetByIdAsync(id);

            dto.Should().NotBeNull();
            dto!.SalesEnquiryDetails.Should().NotBeNull();
            dto.SalesEnquiryDetails.Should().BeEmpty();
        }

        // ── NotFoundAsync ────────────────────────────────────────────────────

        [Fact]
        public async Task NotFoundAsync_Should_Return_False_WhenEntityExists()
        {
            await ClearTableAsync();
            var id = await SeedAsync(BuildHeader());

            var result = await CreateQueryRepo().NotFoundAsync(id);

            result.Should().BeFalse();
        }

        [Fact]
        public async Task NotFoundAsync_Should_Return_True_WhenEntityDoesNotExist()
        {
            await ClearTableAsync();

            var result = await CreateQueryRepo().NotFoundAsync(99999);

            result.Should().BeTrue();
        }

        [Fact]
        public async Task NotFoundAsync_Should_Return_True_AfterSoftDelete()
        {
            await ClearTableAsync();
            var id = await SeedAsync(BuildHeader());

            await using var ctx = _fixture.CreateFreshDbContext();
            await CreateCommandRepo(ctx).SoftDeleteAsync(id, CancellationToken.None);

            var result = await CreateQueryRepo().NotFoundAsync(id);

            result.Should().BeTrue();
        }
    }
}
