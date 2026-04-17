using SalesManagement.Domain.Entities;

namespace SalesManagement.UnitTests.Domain;

public class DispatchAdviceDetailEntityTests
{
    [Fact]
    public void Properties_ShouldBeAssignable()
    {
        var entity = new DispatchAdviceDetail
        {
            Id = 1,
            DispatchAdviceHeaderId = 10,
            SalesOrderDetailId = 20,
            ItemId = 5,
            LotId = 3,
            StartPackNo = 1,
            EndPackNo = 50,
            DispatchQty = 2500m,
            PackTypeId = 2
        };

        entity.Id.Should().Be(1);
        entity.DispatchAdviceHeaderId.Should().Be(10);
        entity.SalesOrderDetailId.Should().Be(20);
        entity.ItemId.Should().Be(5);
        entity.DispatchQty.Should().Be(2500m);
    }

    [Fact]
    public void NullableNavigationProperties_ShouldAcceptNull()
    {
        var entity = new DispatchAdviceDetail
        {
            DispatchAdviceHeader = null,
            SalesOrderDetail = null
        };

        entity.DispatchAdviceHeader.Should().BeNull();
        entity.SalesOrderDetail.Should().BeNull();
    }
}
