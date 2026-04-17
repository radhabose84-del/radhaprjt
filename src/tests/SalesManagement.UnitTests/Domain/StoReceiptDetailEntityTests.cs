using SalesManagement.Domain.Entities;

namespace SalesManagement.UnitTests.Domain;

public class StoReceiptDetailEntityTests
{
    [Fact]
    public void Properties_ShouldBeAssignable()
    {
        var entity = new StoReceiptDetail
        {
            Id = 1,
            StoReceiptHeaderId = 10,
            DeliveryChallanDetailId = 20,
            ItemId = 5,
            LotId = 3,
            StartPackNo = 1,
            EndPackNo = 50,
            DispatchQuantity = 2500m,
            ReceivedQuantity = 2450m,
            DamageQuantity = 50m,
            AcceptedQuantity = 2400m,
            UOMId = 2,
            NetWeight = 2400m,
            LineStatusId = 1
        };

        entity.Id.Should().Be(1);
        entity.StoReceiptHeaderId.Should().Be(10);
        entity.ItemId.Should().Be(5);
        entity.ReceivedQuantity.Should().Be(2450m);
        entity.AcceptedQuantity.Should().Be(2400m);
    }

    [Fact]
    public void NullableProperties_ShouldAcceptNull()
    {
        var entity = new StoReceiptDetail
        {
            BagCount = null,
            GrossWeight = null,
            DeliveryChallanDetail = null,
            LineStatus = null
        };

        entity.BagCount.Should().BeNull();
        entity.GrossWeight.Should().BeNull();
        entity.DeliveryChallanDetail.Should().BeNull();
        entity.LineStatus.Should().BeNull();
    }
}
