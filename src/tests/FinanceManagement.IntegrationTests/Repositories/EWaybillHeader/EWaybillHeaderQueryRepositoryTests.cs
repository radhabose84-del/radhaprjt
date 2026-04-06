using Dapper;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Moq;
using Contracts.Dtos.Lookups.Party;
using Contracts.Dtos.Lookups.Users;
using Contracts.Interfaces;
using Contracts.Interfaces.Lookups.Party;
using Contracts.Interfaces.Lookups.Users;
using FinanceManagement.Infrastructure.Data;
using FinanceManagement.IntegrationTests.Common;
using FinanceManagement.Infrastructure.Repositories.EWaybillHeader;
using static FinanceManagement.Domain.Common.BaseEntity;

namespace FinanceManagement.IntegrationTests.Repositories.EWaybillHeader
{
    [Collection("DatabaseCollection")]
    public sealed class EWaybillHeaderQueryRepositoryTests
    {
        private readonly DbFixture _fixture;

        public EWaybillHeaderQueryRepositoryTests(DbFixture fixture)
        {
            _fixture = fixture;
        }

        private EWaybillHeaderQueryRepository CreateQueryRepo(
            Mock<IUnitLookup>? unitLookup = null,
            Mock<IPartyLookup>? partyLookup = null,
            Mock<IIPAddressService>? ipService = null)
        {
            unitLookup ??= BuildDefaultUnitLookup();
            partyLookup ??= BuildDefaultPartyLookup();
            ipService ??= BuildDefaultIpService();
            var conn = new SqlConnection(_fixture.ConnectionString);
            return new EWaybillHeaderQueryRepository(conn, unitLookup.Object, partyLookup.Object, ipService.Object);
        }

        private static Mock<IUnitLookup> BuildDefaultUnitLookup(
            int unitId = 1,
            string unitName = "Test Unit")
        {
            var mock = new Mock<IUnitLookup>(MockBehavior.Loose);
            mock.Setup(u => u.GetByIdsAsync(It.IsAny<IEnumerable<int>>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new List<UnitLookupDto>
                {
                    new UnitLookupDto { UnitId = unitId, UnitName = unitName }
                });
            mock.Setup(u => u.GetByIdAsync(It.IsAny<int>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new UnitLookupDto { UnitId = unitId, UnitName = unitName });
            return mock;
        }

        private static Mock<IPartyLookup> BuildDefaultPartyLookup(
            int partyId = 1,
            string partyName = "Test Party")
        {
            var mock = new Mock<IPartyLookup>(MockBehavior.Loose);
            mock.Setup(p => p.GetByIdsAsync(It.IsAny<IEnumerable<int>>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new List<PartyLookupDto>
                {
                    new PartyLookupDto { Id = partyId, PartyName = partyName }
                });
            mock.Setup(p => p.GetByIdAsync(It.IsAny<int>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new PartyLookupDto { Id = partyId, PartyName = partyName });
            return mock;
        }

        private static Mock<IIPAddressService> BuildDefaultIpService()
        {
            var mock = new Mock<IIPAddressService>(MockBehavior.Loose);
            mock.Setup(x => x.GetUnitId()).Returns(1);
            return mock;
        }

        private async Task<int> SeedEntityAsync(
            string ewbNumber = "EWB001",
            string invoiceNo = "INV001",
            decimal invoiceValue = 1000m,
            int unitId = 1,
            string ewbStatus = "Pending",
            int? partyId = null)
        {
            await using var ctx = _fixture.CreateFreshDbContext();
            var repo = new EWaybillHeaderCommandRepository(ctx);
            return await repo.CreateAsync(new Domain.Entities.EWaybillHeader
            {
                EInvoiceHeaderId = null,
                UnitId = unitId,
                EWBNumber = ewbNumber,
                InvoiceNo = invoiceNo,
                InvoiceDate = DateOnly.FromDateTime(DateTime.Today),
                InvoiceValue = invoiceValue,
                SupplyType = "O",
                EwbStatus = ewbStatus,
                PartyId = partyId,
                IsActive = Status.Active,
                IsDeleted = IsDelete.NotDeleted
            });
        }

        private async Task ClearTablesAsync()
        {
            await using var conn = new SqlConnection(_fixture.ConnectionString);
            await conn.OpenAsync();
            await conn.ExecuteAsync("DELETE FROM [Finance].[EWaybillDetail]");
            await conn.ExecuteAsync("DELETE FROM [Finance].[EWaybillHeader]");
        }

        // --- GET ALL ---

        [Fact]
        public async Task GetAllAsync_Should_Return_Seeded_Record()
        {
            await ClearTablesAsync();
            await SeedEntityAsync();

            var (items, total) = await CreateQueryRepo().GetAllAsync(1, 10, null);

            items.Should().HaveCount(1);
            total.Should().Be(1);
        }

        [Fact]
        public async Task GetAllAsync_Should_Exclude_SoftDeleted()
        {
            await ClearTablesAsync();
            var id = await SeedEntityAsync();
            await using var ctx = _fixture.CreateFreshDbContext();
            await new EWaybillHeaderCommandRepository(ctx).SoftDeleteAsync(id, CancellationToken.None);

            var (items, total) = await CreateQueryRepo().GetAllAsync(1, 10, null);

            items.Should().BeEmpty();
            total.Should().Be(0);
        }

        [Fact]
        public async Task GetAllAsync_Should_Filter_By_SearchTerm()
        {
            await ClearTablesAsync();
            await SeedEntityAsync(ewbNumber: "EWB_ALPHA01", invoiceNo: "INV_ALPHA01");
            await SeedEntityAsync(ewbNumber: "EWB_BETA02", invoiceNo: "INV_BETA02");

            var (items, total) = await CreateQueryRepo().GetAllAsync(1, 10, "ALPHA");

            items.Should().HaveCount(1);
            items[0].EWBNumber.Should().Be("EWB_ALPHA01");
        }

        // --- GET BY ID ---

        [Fact]
        public async Task GetByIdAsync_Should_Return_Correct_Dto()
        {
            await ClearTablesAsync();
            var id = await SeedEntityAsync(ewbNumber: "EWB_BYID", invoiceNo: "INV_BYID", invoiceValue: 5000m);

            var dto = await CreateQueryRepo().GetByIdAsync(id);

            dto.Should().NotBeNull();
            dto!.EWBNumber.Should().Be("EWB_BYID");
            dto.InvoiceNo.Should().Be("INV_BYID");
            dto.InvoiceValue.Should().Be(5000m);
        }

        [Fact]
        public async Task GetByIdAsync_Should_Return_Null_When_SoftDeleted()
        {
            await ClearTablesAsync();
            var id = await SeedEntityAsync();
            await using var ctx = _fixture.CreateFreshDbContext();
            await new EWaybillHeaderCommandRepository(ctx).SoftDeleteAsync(id, CancellationToken.None);

            var dto = await CreateQueryRepo().GetByIdAsync(id);

            dto.Should().BeNull();
        }

        // --- AUTOCOMPLETE ---

        [Fact]
        public async Task AutocompleteAsync_Should_Return_Matching_Records()
        {
            await ClearTablesAsync();
            await SeedEntityAsync(ewbNumber: "EWB_AUTO01");
            await SeedEntityAsync(ewbNumber: "EWB_AUTO02");
            await SeedEntityAsync(ewbNumber: "OTHER_EWB01");

            var results = await CreateQueryRepo().AutocompleteAsync("EWB_AUTO", CancellationToken.None);

            results.Should().HaveCount(2);
        }

        [Fact]
        public async Task AutocompleteAsync_Should_Exclude_Inactive()
        {
            await ClearTablesAsync();
            var id = await SeedEntityAsync(ewbNumber: "EWB_INACTIVE");
            await using var ctx = _fixture.CreateFreshDbContext();
            var entity = await ctx.EWaybillHeader.FirstAsync(x => x.Id == id);
            entity.IsActive = Status.Inactive;
            await ctx.SaveChangesAsync();

            var results = await CreateQueryRepo().AutocompleteAsync("EWB_INACTIVE", CancellationToken.None);

            results.Should().BeEmpty();
        }

        // --- EWB NUMBER EXISTS ---

        [Fact]
        public async Task EWBNumberExistsAsync_Should_Return_True_When_Exists()
        {
            await ClearTablesAsync();
            await SeedEntityAsync(ewbNumber: "EWB_EXISTS01");

            var exists = await CreateQueryRepo().EWBNumberExistsAsync("EWB_EXISTS01");

            exists.Should().BeTrue();
        }

        [Fact]
        public async Task EWBNumberExistsAsync_Should_Return_False_When_Not_Exists()
        {
            await ClearTablesAsync();

            var exists = await CreateQueryRepo().EWBNumberExistsAsync("EWB_NONEXISTENT");

            exists.Should().BeFalse();
        }

        // --- NOT FOUND ---

        [Fact]
        public async Task NotFoundAsync_Should_Return_True_When_Not_Exists()
        {
            await ClearTablesAsync();

            var notFound = await CreateQueryRepo().NotFoundAsync(9999);

            notFound.Should().BeTrue();
        }

        [Fact]
        public async Task NotFoundAsync_Should_Return_False_When_Exists()
        {
            await ClearTablesAsync();
            var id = await SeedEntityAsync();

            var notFound = await CreateQueryRepo().NotFoundAsync(id);

            notFound.Should().BeFalse();
        }
    }
}
