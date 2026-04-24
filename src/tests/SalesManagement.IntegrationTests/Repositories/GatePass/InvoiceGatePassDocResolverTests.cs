using Contracts.Dtos.Lookups.Inventory;
using Contracts.Interfaces.Lookups.Inventory;
using Dapper;
using Microsoft.Data.SqlClient;
using SalesManagement.Infrastructure.Repositories.GatePass;
using SalesManagement.IntegrationTests.Common;

namespace SalesManagement.IntegrationTests.Repositories.GatePass
{
    [Collection("DatabaseCollection")]
    public sealed class InvoiceGatePassDocResolverTests
    {
        private readonly DbFixture _fixture;

        public InvoiceGatePassDocResolverTests(DbFixture fixture)
        {
            _fixture = fixture;
        }

        private InvoiceGatePassDocResolver CreateResolver(
            Mock<IItemLookup>? item = null,
            Mock<IUOMLookup>? uom = null)
        {
            item ??= BuildItemLookupMock();
            uom ??= BuildUomLookupMock();

            var conn = new SqlConnection(_fixture.ConnectionString);
            return new InvoiceGatePassDocResolver(conn, item.Object, uom.Object);
        }

        private static Mock<IItemLookup> BuildItemLookupMock(int itemId = 1, string itemName = "Cotton Yarn")
        {
            var mock = new Mock<IItemLookup>(MockBehavior.Loose);
            mock.Setup(x => x.GetByIdsAsync(It.IsAny<IEnumerable<int>>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new List<ItemLookupDto>
                {
                    new ItemLookupDto { Id = itemId, ItemName = itemName }
                });
            return mock;
        }

        private static Mock<IUOMLookup> BuildUomLookupMock(int uomId = 1, string uomName = "Kg")
        {
            var mock = new Mock<IUOMLookup>(MockBehavior.Loose);
            mock.Setup(x => x.GetByIdsAsync(It.IsAny<IEnumerable<int>>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new List<UOMLookupDto>
                {
                    new UOMLookupDto { Id = uomId, UOMName = uomName }
                });
            return mock;
        }

        private async Task<int> SeedInvoiceHeaderAsync(
            string transporterName = "Acme Transport",
            int totalBags = 50, decimal totalWeight = 4500m,
            bool isDeleted = false)
        {
            await using var conn = new SqlConnection(_fixture.ConnectionString);
            await conn.OpenAsync();
            return await conn.ExecuteScalarAsync<int>(@"
                ALTER TABLE Sales.InvoiceHeader NOCHECK CONSTRAINT ALL;
                INSERT INTO Sales.InvoiceHeader
                    (InvoiceNo, InvoiceDate, DispatchAdviceId, PartyId, UnitId, FinancialYearId,
                     TotalBags, TotalWeight, TaxableValue,
                     TotalDiscount, TotalFreight, TotalCommission,
                     Insurance, HandlingCharge, TotalCharity, OtherCharges,
                     CGST, SGST, IGST, TaxAmount,
                     TCSPercentage, TCS, RoundOff, InvoiceAmountBeforeTCS, InvoiceAmount,
                     TransporterName,
                     GEFlag, IsActive, IsDeleted,
                     CreatedBy, CreatedDate, CreatedByName, CreatedIP)
                OUTPUT INSERTED.Id
                VALUES
                    ('INV-001', SYSDATETIME(), 1, 1, 1, 1,
                     @TotalBags, @TotalWeight, 0,
                     0, 0, 0,
                     0, 0, 0, 0,
                     0, 0, 0, 0,
                     0, 0, 0, 0, 0,
                     @TransporterName,
                     0, 1, @IsDeleted,
                     1, SYSDATETIMEOFFSET(), 'test', '127.0.0.1');
                ALTER TABLE Sales.InvoiceHeader CHECK CONSTRAINT ALL;",
                new
                {
                    TransporterName = transporterName,
                    TotalBags = totalBags,
                    TotalWeight = totalWeight,
                    IsDeleted = isDeleted
                });
        }

        private async Task SeedInvoiceDetailAsync(int headerId, int itemSno = 1, int itemId = 1, int uomId = 1)
        {
            await using var conn = new SqlConnection(_fixture.ConnectionString);
            await conn.OpenAsync();
            await conn.ExecuteAsync(@"
                ALTER TABLE Sales.InvoiceDetail NOCHECK CONSTRAINT ALL;
                INSERT INTO Sales.InvoiceDetail
                    (InvoiceHeaderId, ItemSno, ItemId, HsnCode, GstPercentage,
                     NoOfBags, BagWeight, NetWeight, RatePerKg,
                     DiscountValue, FreightValue, CommissionValue,
                     TaxableAmount,
                     CgstPercentage, SgstPercentage, IgstPercentage,
                     CGST, SGST, IGST, TaxAmount,
                     UOMId, Charity, HandlingCharges, TotalAmount)
                VALUES
                    (@HeaderId, @ItemSno, @ItemId, '52010000', 5,
                     10, 10, 100, 150,
                     0, 0, 0,
                     15000,
                     2.5, 2.5, 0,
                     375, 375, 0, 750,
                     @UomId, 0, 0, 15750);
                ALTER TABLE Sales.InvoiceDetail CHECK CONSTRAINT ALL;",
                new { HeaderId = headerId, ItemSno = itemSno, ItemId = itemId, UomId = uomId });
        }

        private async Task ClearAsync() =>
            await _fixture.ClearTablesAsync("Sales.InvoiceDetail", "Sales.InvoiceHeader");

        [Fact]
        public async Task GetSummariesAsync_Returns_Empty_For_Empty_Ids()
        {
            var result = await CreateResolver().GetSummariesAsync(Array.Empty<int>());

            result.Should().BeEmpty();
        }

        [Fact]
        public async Task GetSummariesAsync_Returns_Empty_For_Null_Ids()
        {
            var result = await CreateResolver().GetSummariesAsync(null!);

            result.Should().BeEmpty();
        }

        [Fact]
        public async Task GetSummariesAsync_Returns_Summary_With_First_Detail()
        {
            await ClearAsync();
            var hdrId = await SeedInvoiceHeaderAsync(
                transporterName: "Acme Transport",
                totalBags: 50,
                totalWeight: 4500m);
            await SeedInvoiceDetailAsync(hdrId, itemSno: 1, itemId: 42, uomId: 3);

            var result = await CreateResolver(
                item: BuildItemLookupMock(42, "Premium Cotton"),
                uom: BuildUomLookupMock(3, "Kg"))
                .GetSummariesAsync(new[] { hdrId });

            result.Should().ContainSingle();
            var summary = result[0];
            summary.DocId.Should().Be(hdrId);
            summary.TotalQty.Should().Be(50m);
            summary.NetKgs.Should().Be(4500m);
            summary.TransporterName.Should().Be("Acme Transport");
            summary.ItemDescription.Should().Be("Premium Cotton");
            summary.Uom.Should().Be("Kg");
        }

        [Fact]
        public async Task GetSummariesAsync_Picks_Detail_Ordered_By_ItemSno()
        {
            await ClearAsync();
            var hdrId = await SeedInvoiceHeaderAsync();
            // Insert out-of-order — resolver should pick ItemSno=1 (first)
            await SeedInvoiceDetailAsync(hdrId, itemSno: 3, itemId: 300, uomId: 1);
            await SeedInvoiceDetailAsync(hdrId, itemSno: 1, itemId: 100, uomId: 1);
            await SeedInvoiceDetailAsync(hdrId, itemSno: 2, itemId: 200, uomId: 1);

            var result = await CreateResolver(
                item: BuildItemLookupMock(100, "First Item"))
                .GetSummariesAsync(new[] { hdrId });

            result.Should().ContainSingle();
            result[0].ItemDescription.Should().Be("First Item");
        }

        [Fact]
        public async Task GetSummariesAsync_Excludes_SoftDeleted_Header()
        {
            await ClearAsync();
            var hdrId = await SeedInvoiceHeaderAsync(isDeleted: true);
            await SeedInvoiceDetailAsync(hdrId);

            var result = await CreateResolver().GetSummariesAsync(new[] { hdrId });

            result.Should().BeEmpty();
        }

        [Fact]
        public async Task GetSummariesAsync_Deduplicates_Input_Ids()
        {
            await ClearAsync();
            var hdrId = await SeedInvoiceHeaderAsync();
            await SeedInvoiceDetailAsync(hdrId);

            var result = await CreateResolver().GetSummariesAsync(new[] { hdrId, hdrId });

            result.Should().ContainSingle();
        }
    }
}
