using ProductionManagement.Domain.Common;
using ProductionManagement.Domain.Entities;

namespace ProductionManagement.UnitTests.Domain
{
    public class ProductionStockLedgerEntityTests
    {
        [Fact]
        public void ProductionStockLedger_ShouldNotInheritFromBaseEntity()
        {
            typeof(BaseEntity).IsAssignableFrom(typeof(ProductionStockLedger)).Should().BeFalse();
        }

        [Fact]
        public void ProductionStockLedger_Properties_ShouldBeAssignable()
        {
            var docDate = new DateOnly(2025, 7, 15);
            var entity = new ProductionStockLedger
            {
                Id = 1,
                UnitId = 2,
                ItemId = 10,
                LotId = 5,
                DocDate = docDate,
                OpeningLooseKgs = 100.5m,
                ProdKgs = 200m,
                TotalProdKgs = 300.5m,
                PackTypeId = 3,
                NetWeightPerPack = 25m,
                TotalBags = 12,
                NetWeight = 300m,
                BagsRepacked = 2,
                RepackKgs = 50m,
                ClosingLooseKgs = 0.5m,
                ClosingPackKgs = 300m,
                ClosingBags = 12,
                StockClosing = true
            };
            entity.Id.Should().Be(1);
            entity.UnitId.Should().Be(2);
            entity.ItemId.Should().Be(10);
            entity.LotId.Should().Be(5);
            entity.DocDate.Should().Be(docDate);
            entity.OpeningLooseKgs.Should().Be(100.5m);
            entity.ProdKgs.Should().Be(200m);
            entity.TotalProdKgs.Should().Be(300.5m);
            entity.PackTypeId.Should().Be(3);
            entity.NetWeightPerPack.Should().Be(25m);
            entity.TotalBags.Should().Be(12);
            entity.NetWeight.Should().Be(300m);
            entity.BagsRepacked.Should().Be(2);
            entity.RepackKgs.Should().Be(50m);
            entity.ClosingLooseKgs.Should().Be(0.5m);
            entity.ClosingPackKgs.Should().Be(300m);
            entity.ClosingBags.Should().Be(12);
            entity.StockClosing.Should().BeTrue();
        }

        [Fact]
        public void ProductionStockLedger_DefaultValues_ShouldBeDefaults()
        {
            var entity = new ProductionStockLedger();
            entity.Id.Should().Be(0);
            entity.UnitId.Should().Be(0);
            entity.ItemId.Should().Be(0);
            entity.LotId.Should().Be(0);
            entity.OpeningLooseKgs.Should().Be(0m);
            entity.ProdKgs.Should().Be(0m);
            entity.TotalProdKgs.Should().Be(0m);
            entity.TotalBags.Should().Be(0);
            entity.NetWeight.Should().Be(0m);
            entity.BagsRepacked.Should().Be(0);
            entity.RepackKgs.Should().Be(0m);
            entity.ClosingLooseKgs.Should().Be(0m);
            entity.ClosingPackKgs.Should().Be(0m);
            entity.ClosingBags.Should().Be(0);
            entity.StockClosing.Should().BeFalse();
        }
    }
}
