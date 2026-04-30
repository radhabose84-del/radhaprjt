using Contracts.Dtos.Lookups.Inventory;
using Contracts.Dtos.Lookups.Party;
using Contracts.Interfaces.Lookups.Inventory;
using Contracts.Interfaces.Lookups.Party;
using Dapper;
using Microsoft.Data.SqlClient;
using SalesManagement.Infrastructure.Repositories.GatePass;
using SalesManagement.IntegrationTests.Common;

namespace SalesManagement.IntegrationTests.Repositories.GatePass
{
    [Collection("DatabaseCollection")]
    public sealed class DeliveryChallanGatePassDocResolverTests
    {
        private readonly DbFixture _fixture;

        public DeliveryChallanGatePassDocResolverTests(DbFixture fixture)
        {
            _fixture = fixture;
        }

        private DeliveryChallanGatePassDocResolver CreateResolver(
            Mock<IItemLookup>? item = null,
            Mock<IUOMLookup>? uom = null,
            Mock<IPartyLookup>? party = null)
        {
            item ??= BuildItemLookupMock();
            uom ??= BuildUomLookupMock();
            party ??= BuildPartyLookupMock();

            var conn = new SqlConnection(_fixture.ConnectionString);
            return new DeliveryChallanGatePassDocResolver(conn, item.Object, uom.Object, party.Object);
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

        private static Mock<IPartyLookup> BuildPartyLookupMock(int partyId = 1, string partyName = "Acme Transport")
        {
            var mock = new Mock<IPartyLookup>(MockBehavior.Loose);
            mock.Setup(x => x.GetByIdsAsync(It.IsAny<IEnumerable<int>>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new List<PartyLookupDto>
                {
                    new PartyLookupDto { Id = partyId, PartyName = partyName }
                });
            return mock;
        }

        private async Task<int> SeedDcHeaderAsync(int transporterId = 1, bool isDeleted = false)
        {
            await using var conn = new SqlConnection(_fixture.ConnectionString);
            await conn.OpenAsync();
            return await conn.ExecuteScalarAsync<int>(@"
                ALTER TABLE Sales.DeliveryChallanHeader NOCHECK CONSTRAINT ALL;
                INSERT INTO Sales.DeliveryChallanHeader
                    (DeliveryNumber, DeliveryDate, StoHeaderId, DcTypeId, MovementTypeId,
                     FromPlantId, FromStorageLocationId, ToPlantId, ToStorageLocationId,
                     TransporterId, VehicleNumber,
                     DeliveryValue, ConsignmentValue, StatusId,
                     GEFlag, IsActive, IsDeleted,
                     CreatedBy, CreatedDate, CreatedByName, CreatedIP)
                OUTPUT INSERTED.Id
                VALUES
                    ('DC-001', SYSDATETIME(), 1, 0, 0,
                     1, 1, 2, 1,
                     @TransporterId, 'TN-01-1234',
                     0, 0, 1,
                     0, 1, @IsDeleted,
                     1, SYSDATETIMEOFFSET(), 'test', '127.0.0.1');
                ALTER TABLE Sales.DeliveryChallanHeader CHECK CONSTRAINT ALL;",
                new { TransporterId = transporterId, IsDeleted = isDeleted });
        }

        private async Task SeedDcDetailAsync(
            int headerId,
            int itemId = 1, int uomId = 1,
            decimal dispatchQty = 100m, decimal netWeight = 95m, decimal grossWeight = 100m)
        {
            await using var conn = new SqlConnection(_fixture.ConnectionString);
            await conn.OpenAsync();
            await conn.ExecuteAsync(@"
                ALTER TABLE Sales.DeliveryChallanDetail NOCHECK CONSTRAINT ALL;
                INSERT INTO Sales.DeliveryChallanDetail
                    (DeliveryChallanHeaderId, StoDetailId, ItemId, LotId,
                     StartPackNo, EndPackNo, DispatchQuantity, UOMId,
                     NetWeight, GrossWeight, ExMillRate, LineMovementValue)
                VALUES
                    (@HeaderId, 1, @ItemId, 1,
                     1, 10, @DispatchQty, @UomId,
                     @NetWeight, @GrossWeight, 0, 0);
                ALTER TABLE Sales.DeliveryChallanDetail CHECK CONSTRAINT ALL;",
                new { HeaderId = headerId, ItemId = itemId, UomId = uomId, DispatchQty = dispatchQty, NetWeight = netWeight, GrossWeight = grossWeight });
        }

        private async Task ClearAsync() =>
            await _fixture.ClearTablesAsync(
                "Sales.StoReceiptDetail", "Sales.StoReceiptHeader",
                "Sales.DeliveryChallanDetail", "Sales.DeliveryChallanHeader");

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
            var hdrId = await SeedDcHeaderAsync(transporterId: 7);
            await SeedDcDetailAsync(hdrId, itemId: 42, uomId: 3, dispatchQty: 100m, netWeight: 95m, grossWeight: 110m);

            var result = await CreateResolver(
                item: BuildItemLookupMock(42, "Premium Cotton"),
                uom: BuildUomLookupMock(3, "Kg"),
                party: BuildPartyLookupMock(7, "Acme Transport"))
                .GetSummariesAsync(new[] { hdrId });

            result.Should().ContainSingle();
            var summary = result[0];
            summary.DocId.Should().Be(hdrId);
            summary.TotalQty.Should().Be(100m);
            summary.NetKgs.Should().Be(95m);
            summary.GrossKgs.Should().Be(110m);
            summary.TransporterName.Should().Be("Acme Transport");
            summary.ItemDescription.Should().Be("Premium Cotton");
            summary.Uom.Should().Be("Kg");
        }

        [Fact]
        public async Task GetSummariesAsync_Picks_First_Detail_By_Id_When_Multiple()
        {
            await ClearAsync();
            var hdrId = await SeedDcHeaderAsync();
            await SeedDcDetailAsync(hdrId, itemId: 100, uomId: 1, dispatchQty: 50m, netWeight: 48m);
            await SeedDcDetailAsync(hdrId, itemId: 200, uomId: 2, dispatchQty: 70m, netWeight: 68m);

            var result = await CreateResolver(
                item: BuildItemLookupMock(100, "First Item"))
                .GetSummariesAsync(new[] { hdrId });

            result.Should().ContainSingle();
            result[0].TotalQty.Should().Be(50m);
        }

        [Fact]
        public async Task GetSummariesAsync_Excludes_SoftDeleted_Header()
        {
            await ClearAsync();
            var hdrId = await SeedDcHeaderAsync(isDeleted: true);
            await SeedDcDetailAsync(hdrId);

            var result = await CreateResolver().GetSummariesAsync(new[] { hdrId });

            result.Should().BeEmpty();
        }

        [Fact]
        public async Task GetSummariesAsync_Deduplicates_Input_Ids()
        {
            await ClearAsync();
            var hdrId = await SeedDcHeaderAsync();
            await SeedDcDetailAsync(hdrId);

            var result = await CreateResolver().GetSummariesAsync(new[] { hdrId, hdrId, hdrId });

            result.Should().ContainSingle();
        }

        [Fact]
        public async Task GetSummariesAsync_Ignores_NonPositive_Ids()
        {
            await ClearAsync();
            var hdrId = await SeedDcHeaderAsync();
            await SeedDcDetailAsync(hdrId);

            var result = await CreateResolver().GetSummariesAsync(new[] { hdrId, 0, -1 });

            result.Should().ContainSingle();
        }
    }
}
