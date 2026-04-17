using SalesManagement.Domain.Entities;

namespace SalesManagement.UnitTests.Domain;

public class DeliveryChallanDetailEntityTests
{
    [Fact]
    public void Properties_ShouldBeAssignable()
    {
        var entity = new DeliveryChallanDetail
        {
            Id = 1,
            DeliveryChallanHeaderId = 10,
            StoDetailId = 20,
            ItemId = 5,
            LotId = 3,
            StartPackNo = 1,
            EndPackNo = 50,
            DispatchQuantity = 2500m,
            UOMId = 2,
            NetWeight = 2450m,
            ExMillRate = 120.50m,
            LineMovementValue = 295225m
        };

        entity.Id.Should().Be(1);
        entity.DeliveryChallanHeaderId.Should().Be(10);
        entity.ItemId.Should().Be(5);
        entity.DispatchQuantity.Should().Be(2500m);
        entity.LineMovementValue.Should().Be(295225m);
    }

    [Fact]
    public void NullableProperties_ShouldAcceptNull()
    {
        var entity = new DeliveryChallanDetail
        {
            BagCount = null,
            BaleCount = null,
            GrossWeight = null,
            StoDetail = null
        };

        entity.BagCount.Should().BeNull();
        entity.BaleCount.Should().BeNull();
        entity.GrossWeight.Should().BeNull();
        entity.StoDetail.Should().BeNull();
    }

    [Fact]
    public void NavigationProperties_ShouldBeAssignable()
    {
        var entity = new DeliveryChallanDetail
        {
            StoReceiptDetails = new List<StoReceiptDetail>
            {
                new() { Id = 1 }
            }
        };

        entity.StoReceiptDetails.Should().HaveCount(1);
    }
}
