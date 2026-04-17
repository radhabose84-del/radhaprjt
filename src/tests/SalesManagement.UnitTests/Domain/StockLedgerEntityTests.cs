using SalesManagement.Domain.Entities;

namespace SalesManagement.UnitTests.Domain;

public class StockLedgerEntityTests
{
    [Fact]
    public void ShouldNotInheritFromBaseEntity()
    {
        typeof(SalesManagement.Domain.Common.BaseEntity)
            .IsAssignableFrom(typeof(StockLedger)).Should().BeFalse();
    }

    [Fact]
    public void Properties_ShouldBeAssignable()
    {
        var entity = new StockLedger
        {
            Id = 1,
            UnitId = 10,
            DocType = "SO",
            DocNo = 100,
            DetailDocNo = 200,
            DocDate = new DateOnly(2026, 4, 16),
            ItemId = 5,
            LotId = 3,
            PackNo = 1,
            PackTypeId = 2,
            WarehouseId = 7,
            BinId = 4,
            TotalQty = 50,
            TotalValue = 12500.00m,
            StatusId = 1,
            TypeId = 2
        };

        entity.Id.Should().Be(1);
        entity.UnitId.Should().Be(10);
        entity.DocType.Should().Be("SO");
        entity.DocNo.Should().Be(100);
        entity.DocDate.Should().Be(new DateOnly(2026, 4, 16));
        entity.ItemId.Should().Be(5);
        entity.TotalQty.Should().Be(50);
        entity.TotalValue.Should().Be(12500.00m);
    }

    [Fact]
    public void NullableProperties_ShouldAcceptNull()
    {
        var entity = new StockLedger
        {
            DocType = null,
            TypeId = null
        };

        entity.DocType.Should().BeNull();
        entity.TypeId.Should().BeNull();
    }
}
