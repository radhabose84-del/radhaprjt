using SalesManagement.Domain.Entities;

namespace SalesManagement.UnitTests.Domain;

public class StoDetailEntityTests
{
    [Fact]
    public void Properties_ShouldBeAssignable()
    {
        var entity = new StoDetail
        {
            Id = 1,
            StoHeaderId = 10,
            ItemId = 5,
            Quantity = 500m,
            UOMId = 2,
            TransferPrice = 95.75m
        };

        entity.Id.Should().Be(1);
        entity.StoHeaderId.Should().Be(10);
        entity.ItemId.Should().Be(5);
        entity.Quantity.Should().Be(500m);
        entity.TransferPrice.Should().Be(95.75m);
    }

    [Fact]
    public void NullableProperties_ShouldAcceptNull()
    {
        var entity = new StoDetail
        {
            LineStatusId = null,
            LineStatus = null
        };

        entity.LineStatusId.Should().BeNull();
        entity.LineStatus.Should().BeNull();
    }

    [Fact]
    public void NavigationProperties_ShouldBeAssignable()
    {
        var entity = new StoDetail
        {
            DeliveryChallanDetails = new List<DeliveryChallanDetail>
            {
                new() { Id = 1 }
            }
        };

        entity.DeliveryChallanDetails.Should().HaveCount(1);
    }
}
