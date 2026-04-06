using Dapper;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Moq;
using Contracts.Dtos.Lookups.Party;
using Contracts.Interfaces;
using Contracts.Interfaces.Lookups.Party;
using FinanceManagement.Infrastructure.Data;
using FinanceManagement.IntegrationTests.Common;
using FinanceManagement.Infrastructure.Repositories.EInvoiceHeader;
using static FinanceManagement.Domain.Common.BaseEntity;

namespace FinanceManagement.IntegrationTests.Repositories.EInvoiceHeader
{
    [Collection("DatabaseCollection")]
    public sealed class EInvoiceHeaderQueryRepositoryTests
    {
        private readonly DbFixture _fixture;

        public EInvoiceHeaderQueryRepositoryTests(DbFixture fixture)
        {
            _fixture = fixture;
        }

        private EInvoiceHeaderQueryRepository CreateQueryRepo(
            Mock<IPartyLookup>? partyLookup = null,
            Mock<IIPAddressService>? ipService = null)
        {
            partyLookup ??= BuildDefaultPartyLookup();
            ipService ??= BuildDefaultIpService();
            var conn = new SqlConnection(_fixture.ConnectionString);
            return new EInvoiceHeaderQueryRepository(conn, partyLookup.Object, ipService.Object);
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
            string invoiceNo = "INV001",
            string docType = "INV",
            string? irnNumber = null,
            int partyId = 1)
        {
            await using var ctx = _fixture.CreateFreshDbContext();
            var repo = new EInvoiceHeaderCommandRepository(ctx);
            return await repo.CreateAsync(new Domain.Entities.EInvoiceHeader
            {
                UnitId = 1,
                DocType = docType,
                SupplyType = "B2B",
                InvoiceNo = invoiceNo,
                InvoiceDate = DateOnly.FromDateTime(DateTime.Today),
                PlaceOfSupply = "29",
                PartyId = partyId,
                IrnNumber = irnNumber,
                CGST = 100m,
                SGST = 100m,
                InvoiceAmount = 1200m,
                IrnStatus = "Pending",
                IsActive = Status.Active,
                IsDeleted = IsDelete.NotDeleted
            });
        }

        private async Task<int> SeedEntityWithDetailsAsync(
            string invoiceNo = "INV_DET001",
            int partyId = 1)
        {
            await using var ctx = _fixture.CreateFreshDbContext();
            var entity = new Domain.Entities.EInvoiceHeader
            {
                UnitId = 1,
                DocType = "INV",
                SupplyType = "B2B",
                InvoiceNo = invoiceNo,
                InvoiceDate = DateOnly.FromDateTime(DateTime.Today),
                PlaceOfSupply = "29",
                PartyId = partyId,
                CGST = 50m,
                SGST = 50m,
                InvoiceAmount = 600m,
                IrnStatus = "Pending",
                IsActive = Status.Active,
                IsDeleted = IsDelete.NotDeleted,
                EInvoiceDetails = new List<Domain.Entities.EInvoiceDetail>
                {
                    new()
                    {
                        ItemSno = 1,
                        ItemId = 1,
                        ItemName = "Item A",
                        HsnNo = "1001",
                        Qty = 5m,
                        Rate = 100m,
                        UnitPrice = 100m,
                        TaxableAmount = 500m,
                        GrossAmount = 500m,
                        TotalAmount = 550m
                    }
                }
            };
            var repo = new EInvoiceHeaderCommandRepository(ctx);
            return await repo.CreateAsync(entity);
        }

        private async Task ClearTablesAsync()
        {
            await using var conn = new SqlConnection(_fixture.ConnectionString);
            await conn.OpenAsync();
            await conn.ExecuteAsync("DELETE FROM [Finance].[EInvoiceDetail]");
            await conn.ExecuteAsync("DELETE FROM [Finance].[EWaybillDetail]");
            await conn.ExecuteAsync("DELETE FROM [Finance].[EWaybillHeader]");
            await conn.ExecuteAsync("DELETE FROM [Finance].[EInvoiceHeader]");
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
            await new EInvoiceHeaderCommandRepository(ctx).SoftDeleteAsync(id, CancellationToken.None);

            var (items, total) = await CreateQueryRepo().GetAllAsync(1, 10, null);

            items.Should().BeEmpty();
            total.Should().Be(0);
        }

        [Fact]
        public async Task GetAllAsync_Should_Filter_By_SearchTerm()
        {
            await ClearTablesAsync();
            await SeedEntityAsync(invoiceNo: "ALPHA001");
            await SeedEntityAsync(invoiceNo: "BETA002");

            var (items, total) = await CreateQueryRepo().GetAllAsync(1, 10, "ALPHA");

            items.Should().HaveCount(1);
            items[0].InvoiceNo.Should().Be("ALPHA001");
        }

        [Fact]
        public async Task GetAllAsync_Should_Populate_PartyName()
        {
            await ClearTablesAsync();
            await SeedEntityAsync(partyId: 1);

            var partyMock = BuildDefaultPartyLookup(1, "Acme Corp");
            var (items, _) = await CreateQueryRepo(partyLookup: partyMock).GetAllAsync(1, 10, null);

            items[0].PartyName.Should().Be("Acme Corp");
        }

        // --- GET BY ID ---

        [Fact]
        public async Task GetByIdAsync_Should_Return_Correct_Dto_With_Details()
        {
            await ClearTablesAsync();
            var id = await SeedEntityWithDetailsAsync();

            var dto = await CreateQueryRepo().GetByIdAsync(id);

            dto.Should().NotBeNull();
            dto!.InvoiceNo.Should().Be("INV_DET001");
            dto.Details.Should().HaveCount(1);
            dto.Details[0].ItemName.Should().Be("Item A");
        }

        [Fact]
        public async Task GetByIdAsync_Should_Return_Null_When_SoftDeleted()
        {
            await ClearTablesAsync();
            var id = await SeedEntityAsync();
            await using var ctx = _fixture.CreateFreshDbContext();
            await new EInvoiceHeaderCommandRepository(ctx).SoftDeleteAsync(id, CancellationToken.None);

            var dto = await CreateQueryRepo().GetByIdAsync(id);

            dto.Should().BeNull();
        }

        // --- AUTOCOMPLETE ---

        [Fact]
        public async Task AutocompleteAsync_Should_Return_Matching_Records()
        {
            await ClearTablesAsync();
            await SeedEntityAsync(invoiceNo: "INV_AUTO001");
            await SeedEntityAsync(invoiceNo: "INV_AUTO002");
            await SeedEntityAsync(invoiceNo: "OTHER001");

            var results = await CreateQueryRepo().AutocompleteAsync("INV_AUTO", CancellationToken.None);

            results.Should().HaveCount(2);
        }

        [Fact]
        public async Task AutocompleteAsync_Should_Exclude_Inactive()
        {
            await ClearTablesAsync();
            var id = await SeedEntityAsync(invoiceNo: "INV_INACTIVE");
            await using var ctx = _fixture.CreateFreshDbContext();
            var entity = await ctx.EInvoiceHeader.FirstAsync(x => x.Id == id);
            entity.IsActive = Status.Inactive;
            await ctx.SaveChangesAsync();

            var results = await CreateQueryRepo().AutocompleteAsync("INV_INACTIVE", CancellationToken.None);

            results.Should().BeEmpty();
        }

        // --- IRN NUMBER EXISTS ---

        [Fact]
        public async Task IrnNumberExistsAsync_Should_Return_True_When_Exists()
        {
            await ClearTablesAsync();
            await SeedEntityAsync(invoiceNo: "INV_IRN1", irnNumber: "IRN_EXISTS_001");

            var exists = await CreateQueryRepo().IrnNumberExistsAsync("IRN_EXISTS_001");

            exists.Should().BeTrue();
        }

        [Fact]
        public async Task IrnNumberExistsAsync_Should_Return_False_When_Not_Exists()
        {
            await ClearTablesAsync();

            var exists = await CreateQueryRepo().IrnNumberExistsAsync("IRN_NONEXISTENT");

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

        // --- EXISTS BY INVOICE NO (requires IrnNumber not null) ---

        [Fact]
        public async Task ExistsByInvoiceNoAsync_Should_Return_True_When_Irn_Present()
        {
            await ClearTablesAsync();
            await SeedEntityAsync(invoiceNo: "INV_EXIST01", irnNumber: "IRN001");

            var exists = await CreateQueryRepo().ExistsByInvoiceNoAsync("INV_EXIST01", CancellationToken.None);

            exists.Should().BeTrue();
        }

        [Fact]
        public async Task ExistsByInvoiceNoAsync_Should_Return_False_When_Irn_Null()
        {
            await ClearTablesAsync();
            await SeedEntityAsync(invoiceNo: "INV_NOIRN", irnNumber: null);

            var exists = await CreateQueryRepo().ExistsByInvoiceNoAsync("INV_NOIRN", CancellationToken.None);

            exists.Should().BeFalse();
        }

        // --- GET ID BY INVOICE NO ---

        [Fact]
        public async Task GetIdByInvoiceNoAsync_Should_Return_Id_When_Found()
        {
            await ClearTablesAsync();
            var id = await SeedEntityAsync(invoiceNo: "INV_GETID01", irnNumber: "IRN_GETID01");

            var result = await CreateQueryRepo().GetIdByInvoiceNoAsync("INV_GETID01", CancellationToken.None);

            result.Should().Be(id);
        }

        [Fact]
        public async Task GetIdByInvoiceNoAsync_Should_Return_Null_When_Not_Found()
        {
            await ClearTablesAsync();

            var result = await CreateQueryRepo().GetIdByInvoiceNoAsync("INV_NONEXIST", CancellationToken.None);

            result.Should().BeNull();
        }
    }
}
