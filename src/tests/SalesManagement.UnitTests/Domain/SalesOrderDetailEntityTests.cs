using SalesManagement.Domain.Entities;

namespace SalesManagement.UnitTests.Domain;

public class SalesOrderDetailEntityTests
{
    [Fact]
    public void Properties_ShouldBeAssignable()
    {
        var entity = new SalesOrderDetail
        {
            Id = 1,
            SalesOrderHeaderId = 10,
            ItemId = 5,
            HSNId = 3,
            QtyInBags = 100,
            BagWeight = 50.5m,
            TotalWeight = 5050m,
            ExMillRate = 120.50m,
            NetAmount = 60000m
        };

        entity.Id.Should().Be(1);
        entity.SalesOrderHeaderId.Should().Be(10);
        entity.ItemId.Should().Be(5);
        entity.QtyInBags.Should().Be(100);
        entity.NetAmount.Should().Be(60000m);
    }

    [Fact]
    public void NullableProperties_ShouldAcceptNull()
    {
        var entity = new SalesOrderDetail
        {
            VariantId = null,
            PackTypeId = null,
            LineItemStatusId = null,
            LineItemStatus = null
        };

        entity.VariantId.Should().BeNull();
        entity.PackTypeId.Should().BeNull();
        entity.LineItemStatusId.Should().BeNull();
        entity.LineItemStatus.Should().BeNull();
    }

    [Fact]
    public void NavigationProperties_ShouldBeAssignable()
    {
        var entity = new SalesOrderDetail
        {
            DispatchAdviceDetails = new List<DispatchAdviceDetail>
            {
                new() { Id = 1 }
            }
        };

        entity.DispatchAdviceDetails.Should().HaveCount(1);
    }
}
